using System.Collections.Concurrent;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

public class OrderStatisticsUnsafe
{
    public int    TotalProcessed { get; private set; }
    public decimal TotalRevenue  { get; private set; }
    public Dictionary<OrderStatus, int> OrdersPerStatus { get; } = new();
    public List<string> ProcessingErrors                { get; } = new();

    public void Record(Order order)
    {
        TotalProcessed++;                           
        TotalRevenue += order.TotalAmount;          

        if (!OrdersPerStatus.ContainsKey(order.Status))
            OrdersPerStatus[order.Status] = 0;
        OrdersPerStatus[order.Status]++;           

        if (order.Items.Count == 0)
            ProcessingErrors.Add($"#{order.Id}: brak pozycji"); 
    }

    public void PrintSummary(string label)
    {
        System.Console.WriteLine($"\n  [{label}]");
        System.Console.WriteLine($"    TotalProcessed : {TotalProcessed}");
        System.Console.WriteLine($"    TotalRevenue   : {TotalRevenue:C}");
        System.Console.WriteLine($"    OrdersPerStatus: " +
            string.Join(", ", OrdersPerStatus.Select(kv => $"{kv.Key}={kv.Value}")));
        System.Console.WriteLine($"    Errors         : {ProcessingErrors.Count}");
    }
}

public class OrderStatistics
{
    private int _totalProcessed;
    public  int  TotalProcessed => _totalProcessed;

    private decimal _totalRevenue;
    private readonly object _lock = new();
    public decimal TotalRevenue { get { lock (_lock) return _totalRevenue; } }

    public ConcurrentDictionary<OrderStatus, int> OrdersPerStatus { get; } = new();

    public List<string> ProcessingErrors { get; } = new();

    public void Record(Order order)
    {
        Interlocked.Increment(ref _totalProcessed);

        lock (_lock)
        {
            _totalRevenue += order.TotalAmount;

            if (order.Items.Count == 0)
                ProcessingErrors.Add($"#{order.Id}: brak pozycji");
        }

        OrdersPerStatus.AddOrUpdate(
            key:            order.Status,
            addValue:       1,
            updateValueFactory: (_, old) => old + 1);
    }

    public void PrintSummary(string label)
    {
        System.Console.WriteLine($"\n  [{label}]");
        System.Console.WriteLine($"    TotalProcessed : {TotalProcessed}");
        System.Console.WriteLine($"    TotalRevenue   : {TotalRevenue:C}");
        System.Console.WriteLine($"    OrdersPerStatus: " +
            string.Join(", ", OrdersPerStatus.Select(kv => $"{kv.Key}={kv.Value}")));
        System.Console.WriteLine($"    Errors         : {ProcessingErrors.Count}");
    }
}
