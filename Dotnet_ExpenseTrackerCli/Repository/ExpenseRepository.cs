using System.Text.Json;

namespace Dotnet_ExpenseTrackerCli;

public class ExpenseRepository: IExpenseRepository
{
    private readonly FileInfo _file;

    public ExpenseRepository(FileInfo file)
    {
        _file = file;
        _file.Directory?.Create();
        if (!_file.Exists) File.WriteAllText(_file.FullName, "[]");
    }
    public List<Expense> Load()
    {
        if(!_file.Exists) return new List<Expense>();
        string json = File.ReadAllText(_file.FullName);
        return string.IsNullOrWhiteSpace(json) ? new List<Expense>() : 
            (JsonSerializer.Deserialize<List<Expense>>(json) ?? new List<Expense>());
    }
    

    public void Save(List<Expense> items)
    {
        Console.WriteLine("Serialising expenses list...");
        var json = JsonSerializer.Serialize(items);
        File.WriteAllText(_file.FullName, json);
    }
}