namespace Dotnet_ExpenseTrackerCli;
/*
 *In case things changes -- to avoid tight coupling
 * and design issue
 * for example, if we werent using interfaces
 * we would need to call a method directly, thats when
 * tight coupling occurs.
 */
public interface IExpenseRepository
{
    List<Expense> Load();
    void Save(List<Expense> items);
}