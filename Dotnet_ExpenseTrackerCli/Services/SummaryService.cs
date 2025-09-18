using System.Globalization;

namespace Dotnet_ExpenseTrackerCli.Services;

public class SummaryService
{
    private readonly IExpenseRepository _expenseRepository;

    public SummaryService(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }
    
    public void TotalExpenses(int? month, int? year)
    {
        var items = _expenseRepository.Load();
    
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
}