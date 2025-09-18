namespace Dotnet_ExpenseTrackerCli.Services;


public class ExpenseService
{
    // using constructor injection:
    private readonly IExpenseRepository _expenseRepository;

    public ExpenseService(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }
    
    public void ReadExpenses()
    {
        var items = _expenseRepository.Load();
        foreach (var item in items)
        {
            Console.WriteLine($"Id: {item.Id}, Name: {item.Name}, Amount: {item.Amount}");
        }
    }
    
    public void CreateExpense(string expenseName, decimal expenseAmount)
    {
        var items = _expenseRepository.Load();
        var id = items.Count ==0 ? 1 : items.Max(item => item.Id) + 1;
        items.Add(new Expense(id, expenseName, DateTime.Now.ToString("yyyy-MM-dd")	, expenseAmount));
        _expenseRepository.Save(items);
    }
    
    public  void DeleteExpense(int id)
    {
        Console.WriteLine("Deleting task");
        var items = _expenseRepository.Load();
        items.Where(item => item.Id == id).ToList().ForEach(item => items.Remove(item));
        
        _expenseRepository.Save(items);
        Console.WriteLine($"Deleted task {id}");
    }
    
    public void UpdateExpense(int id, string expenseName, decimal expenseAmount)
    {
        Console.WriteLine("Updating task");
        var items = _expenseRepository.Load();
        var item = items.SingleOrDefault(item => item.Id == id);
        if (item != null)
        {
            item.Name = expenseName; item.Amount = expenseAmount;
        }
        _expenseRepository.Save(items);
        Console.WriteLine($"Updated task {id}");
    }

    public void ClearExpenses()
    {
        Console.WriteLine("Clearing expenses...");
        var items = _expenseRepository.Load();
        items.Clear();
        _expenseRepository.Save(items);
    }

}