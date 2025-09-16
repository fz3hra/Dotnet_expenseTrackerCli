namespace Dotnet_ExpenseTrackerCli;

public class Expense
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    
    public Expense(){}

    public Expense(int id, string expenseName, DateTime date, decimal amount)
    {
        this.Id = id;
        this.Name = expenseName;
        this.Date = date;
        this.Amount = amount;
    }
}