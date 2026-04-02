using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

public delegate bool ValidationRule(Order order, out string errorMessage);

public class OrderValidator
{
    private readonly List<ValidationRule> _customRules = new();

    private readonly List<(Func<Order, bool> Rule, string ErrorMessage)> _funcRules = new();

    public OrderValidator()
    {
        _customRules.Add(HasItems);
        _customRules.Add(AmountWithinLimit);
        _customRules.Add(PositiveQuantities);

        _funcRules.Add((o => o.OrderDate <= DateTime.Now,  "Data zamówienia jest z przyszłości."));
        _funcRules.Add((o => o.Status != OrderStatus.Cancelled, "Zamówienie jest anulowane."));
    }

    private bool HasItems(Order order, out string errorMessage)
    {
        if (order.Items.Any()) { errorMessage = string.Empty; return true; }
        errorMessage = "Zamówienie nie zawiera żadnych pozycji.";
        return false;
    }

    private bool AmountWithinLimit(Order order, out string errorMessage)
    {
        if (order.TotalAmount <= 20_000m) { errorMessage = string.Empty; return true; }
        errorMessage = "Kwota zamówienia przekracza limit 20 000 zł.";
        return false;
    }

    private bool PositiveQuantities(Order order, out string errorMessage)
    {
        if (order.Items.All(i => i.Quantity > 0)) { errorMessage = string.Empty; return true; }
        errorMessage = "Wszystkie pozycje muszą mieć ilość większą niż 0.";
        return false;
    }

    public bool ValidateAll(Order order, out List<string> errors)
    {
        errors = new List<string>();

        foreach (var rule in _customRules)
            if (!rule(order, out string err))
                errors.Add(err);

        foreach (var (rule, message) in _funcRules)
            if (!rule(order))
                errors.Add(message);

        return errors.Count == 0;
    }
}
