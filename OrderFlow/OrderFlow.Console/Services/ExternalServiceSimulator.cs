using System.Diagnostics;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

public class ExternalServiceSimulator
{

    public async Task<bool> CheckInventoryAsync(Product product)
    {
        int delay = Random.Shared.Next(500, 1501);
        await Task.Delay(delay);
        bool inStock = Random.Shared.NextDouble() > 0.1; 
        System.Console.WriteLine(
            $"    [Magazyn]  {product.Name,-20} → {(inStock ? "Dostępny ✓" : "Brak ✗"),12}  ({delay} ms)");
        return inStock;
    }

    public async Task<bool> ValidatePaymentAsync(Order order)
    {
        int delay = Random.Shared.Next(1000, 2001);
        await Task.Delay(delay);
        bool ok = Random.Shared.NextDouble() > 0.05; 
        System.Console.WriteLine(
            $"    [Płatność] Zamówienie #{order.Id,-3}         → {(ok ? "Zatwierdzona ✓" : "Odrzucona ✗"),14}  ({delay} ms)");
        return ok;
    }

    public async Task<decimal> CalculateShippingAsync(Order order)
    {
        int delay = Random.Shared.Next(300, 801);
        await Task.Delay(delay);
        decimal shipping = Math.Round((decimal)(Random.Shared.NextDouble() * 30 + 10), 2);
        System.Console.WriteLine(
            $"    [Wysyłka]  Zamówienie #{order.Id,-3}         → {shipping,8:C}              ({delay} ms)");
        return shipping;
    }

    public async Task<(bool allInStock, bool paymentOk, decimal shipping)>
        ProcessOrderAsync(Order order)
    {
        System.Console.WriteLine($"\n  [Async] Zamówienie #{order.Id} — równoległe wywołania:");

        var sw = Stopwatch.StartNew();

        var inventoryTasks = order.Items
            .Select(i => CheckInventoryAsync(i.Product))
            .ToList();

        var paymentTask  = ValidatePaymentAsync(order);
        var shippingTask = CalculateShippingAsync(order);

        var allTasks = inventoryTasks
            .Cast<Task>()
            .Append(paymentTask)
            .Append(shippingTask);

        await Task.WhenAll(allTasks);

        sw.Stop();

        bool allInStock = inventoryTasks.All(t => t.Result);
        bool paymentOk  = paymentTask.Result;
        decimal shipping = shippingTask.Result;

        System.Console.WriteLine(
            $"  [Async] #{order.Id} ukończone w {sw.ElapsedMilliseconds} ms" +
            $"  | Magazyn: {allInStock}  | Płatność: {paymentOk}  | Wysyłka: {shipping:C}");

        return (allInStock, paymentOk, shipping);
    }

    public async Task ProcessMultipleOrdersAsync(List<Order> orders)
    {
        var semaphore = new SemaphoreSlim(3); 
        int processed = 0;
        int total     = orders.Count;

        var tasks = orders.Select(async order =>
        {
            await semaphore.WaitAsync();
            try
            {
                await ProcessOrderAsync(order);
                int count = Interlocked.Increment(ref processed);
                System.Console.WriteLine($"  [Postęp] Przetworzono {count}/{total} zamówień");
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }
}
