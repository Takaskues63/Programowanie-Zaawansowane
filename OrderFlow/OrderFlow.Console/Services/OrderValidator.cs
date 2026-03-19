using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

public delegate bool ValidationRule(Order order, out string errorMessage);

public class OrderValidator
{
    private readonly List<ValidationRule> _customRules = new();
    private readonly List<Func<Order, bool>> _funcRules = new();

    public OrderValidator()
    {
        _customRules.Add(HasItems);
        _customRules.Add(AmountWithinLimit);
        _customRules.Add(PositiveQuantities);

        _funcRules.Add(o => o.OrderDate <= DateTime.Now);
        _funcRules.Add(o => o.Status != OrderStatus.Cancelled);
    }

    private bool HasItems(Order order, out string errorMessage)
    {
        if (order.Items.Any())
        {
            errorMessage = string.Empty;
            return true;
        }
        errorMessage = "Zamówienie nie zawiera żadnych pozycji.";
        return false;
    }

    private bool AmountWithinLimit(Order order, out string errorMessage)
    {
        if (order.TotalAmount <= 20000m)
        {
            errorMessage = string.Empty;
            return true;
        }
        errorMessage = "Kwota zamówienia przekracza limit 20 000 zł.";
        return false;
    }

    private bool PositiveQuantities(Order order, out string errorMessage)
    {
        if (order.Items.All(i => i.Quantity > 0))
        {
            errorMessage = string.Empty;
            return true;
        }
        errorMessage = "Wszystkie pozycje muszą mieć ilość większą niż 0.";
        return false;
    }

    public bool ValidateAll(Order order, out List<string> errors)
    {
        errors = new List<string>();

        foreach (var rule in _customRules)
        {
            if (!rule(order, out string error))
            {
                errors.Add(error);
            }
        }

        if (!_funcRules[0](order)) errors.Add("Data zamówienia jest z przyszłości.");
        if (!_funcRules[1](order)) errors.Add("Zamówienie jest anulowane.");

        return !errors.Any();
    }
}