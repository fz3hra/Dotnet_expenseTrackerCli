// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.Text.Json;
using Dotnet_ExpenseTrackerCli;

static class Program
{
    static int Main(string[] args)
    {
        Option<FileInfo> fileOption = new("--file")
        {
            Description = "An option whose argument is parsed as a FileInfo",
            Required = false,
            DefaultValueFactory = result =>
            {
                if (result.Tokens.Count == 0)
                {
                    return new FileInfo("expenses.json");

                }
                string filePath = result.Tokens.Single().Value;
                if (!File.Exists(filePath))
                {
                    result.AddError("File does not exist");
                    return null;
                }
                else
                {
                    return new FileInfo(filePath);
                }
            }
        };
        
        Option<int> idOption = new("--id")
        {
            Description = "id for expenses",
            Required = true,
        };

        Option<string> nameOption = new("--name")
        {
            Description = "description for expenses",
            Required = true,
            AllowMultipleArgumentsPerToken = true
        };
        
        Option<decimal> amountOption = new("--amount")
        {
            Description = "amount for expenses",
            Required = true,
        };
        
        // arguments that will be passed
        // var idArgument = new Argument<int>("Id");
        // var nameArgument = new Argument<string>("Name");
        // var amountArgument = new Argument<int>("amount");
        
        // commands required
        RootCommand rootCommand = new("dotnet-expense-tracker");
        fileOption.Recursive = true;
        rootCommand.Options.Add(fileOption);

        
        Command readCommand = new("read", "read expenses list");
        rootCommand.Subcommands.Add(readCommand);
        
        Command createCommand = new("add", "create expenses list");
        rootCommand.Aliases.Add("create");
        createCommand.Options.Add(nameOption);
        createCommand.Options.Add(amountOption);
        rootCommand.Subcommands.Add(createCommand);
        
        
        Command updateCommand = new("update", "update expenses list");
        rootCommand.Aliases.Add("edit");
        updateCommand.Options.Add(nameOption);
        updateCommand.Options.Add(amountOption);
        rootCommand.Subcommands.Add(updateCommand);
        
        Command deleteCommand = new("delete", "update expenses list");
        rootCommand.Aliases.Add("remove");
        deleteCommand.Options.Add(idOption);
        rootCommand.Subcommands.Add(deleteCommand);
        
        Command clearCommand = new("clear", "clear expenses list");
        rootCommand.Subcommands.Add(clearCommand);
        
        readCommand.SetAction(result => ReadExpenses(
                result.GetValue(fileOption)
            )
        );
        
        createCommand.SetAction(result => CreateExpense(
                result.GetValue(fileOption), 
                result.GetValue(nameOption),
                
                result.GetValue(amountOption)
            )
        );
        
        updateCommand.SetAction(result => UpdateExpense(
                result.GetValue(fileOption), 
                result.GetValue(idOption),
                result.GetValue(nameOption),
                
                result.GetValue(amountOption)
            )
        );
        
        deleteCommand.SetAction(parseResult => DeleteExpense(
            parseResult.GetValue(fileOption),
            parseResult.GetValue(idOption)
        ));
        
        clearCommand.SetAction(result => ClearExpenses(
                result.GetValue(fileOption)
            )
        );
            
        return rootCommand.Parse(args).Invoke();
    }
    
    // Method for manipulating jSON
    static List<Expense> LoadExpensesJson(FileInfo file)
    {
        if(!file.Exists) return new List<Expense>();
        string json = File.ReadAllText(file.FullName);
        return string.IsNullOrWhiteSpace(json) ? new List<Expense>() : 
            (JsonSerializer.Deserialize<List<Expense>>(json) ?? new List<Expense>());
    }
    
    static void SaveExpensesJson(FileInfo file, List<Expense> items)
    {
        Console.WriteLine("Serialising expenses list...");
        var json = JsonSerializer.Serialize(items);
        File.WriteAllText(file.FullName, json);
    }

    private static void ReadExpenses(FileInfo file)
    {
        foreach (var line in File.ReadLines(file.FullName))
        {
            Console.WriteLine(line);
        }
    }

    private static void CreateExpense(FileInfo files, string expenseName, decimal expenseAmount)
    {
        var items = LoadExpensesJson(files);
        var id = items.Count ==0 ? 1 : items.Max(item => item.Id +1);
        items.Add(new Expense(id, expenseName, DateTime.Now, expenseAmount));
        SaveExpensesJson(files, items);
    }

    private static void DeleteExpense(FileInfo file, int id)
    {
        Console.WriteLine("Deleting task");
        var items = LoadExpensesJson(file);
        items.Where(item => item.Id == id).ToList().ForEach(item => items.Remove(item));
        
        SaveExpensesJson(file, items);
        Console.WriteLine($"Deleted task {id}");
    }

    private static void UpdateExpense(FileInfo file, int id, string expenseName, decimal expenseAmount)
    {
        Console.WriteLine("Updating task");
        var items = LoadExpensesJson(file);
        var item = items.SingleOrDefault(item => item.Id == id);
        if (item != null)
        {
            item.Name = expenseName; item.Amount = expenseAmount;
        }
        SaveExpensesJson(file, items);
        Console.WriteLine($"Updated task {id}");
    }

    private static void ClearExpenses(FileInfo file)
    {
        Console.WriteLine("Clearing expenses...");
        var items = LoadExpensesJson(file);
        items.Clear();
        SaveExpensesJson(file, items);
    }
}
