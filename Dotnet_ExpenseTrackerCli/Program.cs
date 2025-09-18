// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using Dotnet_ExpenseTrackerCli;
using Dotnet_ExpenseTrackerCli.Services;

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
        
        
        readCommand.SetAction(result =>
            {
                var file = result.GetValue(fileOption);
                var repo = new ExpenseRepository(file);
                var service = new ExpenseService(repo);
                service.ReadExpenses();
            }
        );
    
        createCommand.SetAction(result =>
            {
                var file = result.GetValue(fileOption);
                var name = result.GetValue(nameOption);
                var amount = result.GetValue(amountOption);

                var repo = new ExpenseRepository(file);
                var service = new ExpenseService(repo);
                service.CreateExpense(name, amount);
            }
        );
       
        updateCommand.SetAction(result =>
            {
                var file = result.GetValue(fileOption);
                var id = result.GetValue(idOption);
                var name = result.GetValue(nameOption);
                var amount = result.GetValue(amountOption);

                var repo = new ExpenseRepository(file);
                var service = new ExpenseService(repo);
                service.UpdateExpense(id, name, amount);
            }
        );
        
    
        deleteCommand.SetAction(result =>
            {
                var file = result.GetValue(fileOption);
                var id = result.GetValue(idOption);
                
                var repo = new ExpenseRepository(file);
                var service = new ExpenseService(repo);
                service.DeleteExpense(id);
            }
        );
        
     
        clearCommand.SetAction(result =>
            {
                var file = result.GetValue(fileOption);
                
                var repo = new ExpenseRepository(file);
                var service = new ExpenseService(repo);
                service.ClearExpenses();
            }
        );

        summaryCommand.SetAction(result =>
            {
                var file = result.GetValue(fileOption);
                var month = result.GetValue(filterMonthOption);
                var year = result.GetValue(filterYearOption);
                
                var repo = new ExpenseRepository(file);
                var service = new SummaryService(repo);
                service.TotalExpenses(month, year);
            }
        );
        
        exportCsvCommand.SetAction(result =>
            {
                var file = result.GetValue(fileOption);
                var outCsv = result.GetValue(outCsvOption);
                
                var repo = new ExpenseRepository(file);
                var service = new CsvExporterService(repo);
                service.ExportJsonToCsv(outCsv);
                
            }
        );
            
        return rootCommand.Parse(args).Invoke();
    }
}
