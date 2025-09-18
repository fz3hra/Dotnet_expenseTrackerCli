// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.Globalization;
using System.Text.Json;
using ChoETL;
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
        
        Option<FileInfo> outCsvOption = new("--out")
        {
            Description = "CSV output file",
            Required = false,
            DefaultValueFactory = _ => new FileInfo("expenses.csv")
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
            Required = false,
        };
        
        Option<int?> filterMonthOption = new("--month")
        {
            Description = "filter expenses for month",
            Required = false,
        };
        
        Option<int?> filterYearOption = new("--year")
        {
            Description = "filter expenses for year; it defaults to current year unless otherwise",
            Required = false,
        };
        
        
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
        updateCommand.Options.Add(idOption);
        updateCommand.Options.Add(nameOption);
        updateCommand.Options.Add(amountOption);
        rootCommand.Subcommands.Add(updateCommand);
        
        Command deleteCommand = new("delete", "delete expense in a list");
        rootCommand.Aliases.Add("remove");
        deleteCommand.Options.Add(idOption);
        rootCommand.Subcommands.Add(deleteCommand);
        
        Command clearCommand = new("clear", "clear expenses list");
        rootCommand.Subcommands.Add(clearCommand);
        
        Command summaryCommand = new("summary", "summary expenses list");
        summaryCommand.Options.Add(filterMonthOption);
        summaryCommand.Options.Add(filterYearOption);
        rootCommand.Subcommands.Add(summaryCommand);
        
        Command exportCsvCommand = new("export-csv", "export expenses list");
        exportCsvCommand.Options.Add(outCsvOption);
        rootCommand.Subcommands.Add(exportCsvCommand);
        
        //
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
        
        summaryCommand.SetAction(result => TotalExpenses(
                result.GetValue(fileOption),
                result.GetValue(filterMonthOption),
                
                result.GetValue(filterYearOption)
            )
        );
        
        exportCsvCommand.SetAction(result => ExportJsonToCsv(
            result.GetValue(fileOption),   
            result.GetValue(outCsvOption) 
        ));
            
        return rootCommand.Parse(args).Invoke();
    }
    
    // Method for manipulating jSON
    static List<Expense> LoadExpenses(FileInfo file)
    {
        if(!file.Exists) return new List<Expense>();
        string json = File.ReadAllText(file.FullName);
        return string.IsNullOrWhiteSpace(json) ? new List<Expense>() : 
            (JsonSerializer.Deserialize<List<Expense>>(json) ?? new List<Expense>());
    }
    
    static void SaveExpenses(FileInfo file, List<Expense> items)
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
        var items = LoadExpenses(files);
        var id = items.Count ==0 ? 1 : items.Max(item => item.Id) + 1;
        items.Add(new Expense(id, expenseName, DateTime.Now.ToString("yyyy-MM-dd")	, expenseAmount));
        SaveExpenses(files, items);
    }

    private static void DeleteExpense(FileInfo file, int id)
    {
        Console.WriteLine("Deleting task");
        var items = LoadExpenses(file);
        items.Where(item => item.Id == id).ToList().ForEach(item => items.Remove(item));
        
        SaveExpenses(file, items);
        Console.WriteLine($"Deleted task {id}");
    }

    private static void UpdateExpense(FileInfo file, int id, string expenseName, decimal expenseAmount)
    {
        Console.WriteLine("Updating task");
        var items = LoadExpenses(file);
        var item = items.SingleOrDefault(item => item.Id == id);
        if (item != null)
        {
            item.Name = expenseName; item.Amount = expenseAmount;
        }
        SaveExpenses(file, items);
        Console.WriteLine($"Updated task {id}");
    }

    private static void ClearExpenses(FileInfo file)
    {
        Console.WriteLine("Clearing expenses...");
        var items = LoadExpenses(file);
        items.Clear();
        SaveExpenses(file, items);
    }

    private static void TotalExpenses(FileInfo file, int? month, int? year)
    {
        var items = LoadExpenses(file);

        if (month.HasValue)
        {
            year ??= DateTime.Now.Year;
            var itemsFiltered = FilterByMonthYear(items, month, year);
            decimal sum = SumExpenses(itemsFiltered);
            Console.WriteLine($"Total expenses for month {month} of year {year}: {sum}");
        }
        else
        {
            decimal sum = SumExpenses(items);
            Console.WriteLine($"Total expenses for current year: {sum}");
        }
    }

    private static decimal SumExpenses(IEnumerable<Expense> items)
    {
        return items.Sum(item => item.Amount);
    }

    private static IEnumerable<Expense> FilterByMonthYear(List<Expense> items,  int? month, int? year)
    {
        var matched = items.Where(e =>
        {
            if (!DateTime.TryParseExact(e.Date, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                return false; 
            }
            return dt.Year == year && dt.Month == month;
        });
        return matched;
    }

    private static void ExportJsonToCsv(FileInfo file, FileInfo outCsv)
    {
        var items = LoadExpenses(file);
        
        if(outCsv.Directory is not null) Directory.CreateDirectory(outCsv.DirectoryName);
        
        var writer = new StreamWriter(outCsv.FullName);
        
        using (var w = new ChoCSVWriter<Expense>(writer)
                       .WithFirstLineHeader())
            {
                w.Write(items);
                Console.WriteLine($"Exported {items.Count} expenses to {outCsv.FullName}");

            }
    }
}
