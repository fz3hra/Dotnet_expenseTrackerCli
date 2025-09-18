using ChoETL;

namespace Dotnet_ExpenseTrackerCli.Services;

public class CsvExporterService
{
    private readonly IExpenseRepository _expenseRepository;

    public CsvExporterService(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }
    
    public void ExportJsonToCsv(FileInfo outCsv)
    {
        var items = _expenseRepository.Load();
        
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