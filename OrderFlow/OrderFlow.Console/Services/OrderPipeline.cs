using OrderFlow.Console.Events;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

public class OrderPipeline
{
    private readonly OrderValidator _validator = new();

    public event EventHandler<OrderStatusChangedEventArgs>? StatusChanged;
    public event EventHandler<OrderValidationEventArgs>?    ValidationCompleted;

    private void ChangeStatus(Order order, OrderStatus newStatus)
    {
        var old = order.Status;
        order.Status = newStatus;
        StatusChanged?.Invoke(this, new OrderStatusChangedEventArgs(order, old, newStatus));
    }

    public void ProcessOrder(Order order)
    {
        System.Console.WriteLine($"\n>>> Zamówienie #{order.Id} ({order.Customer.Name}) — start pipeline");

        bool isValid = _validator.ValidateAll(order, out var errors);
        ValidationCompleted?.Invoke(
            this, new OrderValidationEventArgs(order, isValid, errors));

        if (!isValid)
        {
            ChangeStatus(order, OrderStatus.Cancelled);
            return;
        }

        ChangeStatus(order, OrderStatus.Validated);
        Thread.Sleep(80);
        ChangeStatus(order, OrderStatus.Processing);
        Thread.Sleep(80);
        ChangeStatus(order, OrderStatus.Completed);
    }
}
