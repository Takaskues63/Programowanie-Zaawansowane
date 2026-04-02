using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

public class OrderProcessor
{
    private IEnumerable<Order> _orders;

    public OrderProcessor(IEnumerable<Order> orders)
    {
        _orders = orders;
    }

    public IEnumerable<Order> FilterOrders(Predicate<Order> filter)
    {
        return _orders.Where(o => filter(o)).ToList();
    }

    public void ProcessOrders(Action<Order> action, IEnumerable<Order> ordersToProcess)
    {
        foreach (var order in ordersToProcess)
        {
            action(order);
        }
    }

    public IEnumerable<T> ProjectOrders<T>(Func<Order, T> projector, IEnumerable<Order> ordersToProject)
    {
        return ordersToProject.Select(projector).ToList();
    }

    public decimal AggregateOrders(Func<IEnumerable<Order>, decimal> aggregator)
    {
        return aggregator(_orders);
    }

    public void ProcessPipeline(Predicate<Order> filter, Func<Order, decimal> sortKey, int topN, Action<Order> finalAction)
    {
        var result = _orders
            .Where(o => filter(o))
            .OrderByDescending(sortKey)
            .Take(topN);

        foreach (var order in result)
        {
            finalAction(order);
        }
    }
}