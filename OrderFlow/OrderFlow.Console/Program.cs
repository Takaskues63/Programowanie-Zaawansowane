using OrderFlow.Console.Data;
using OrderFlow.Console.Models;
using OrderFlow.Console.Services;

var orders = SampleData.Orders;
var customers = SampleData.Customers;

System.Console.WriteLine("=== ZADANIE 2: WALIDACJA ZAMÓWIEŃ ===");
var validator = new OrderValidator();

var validOrder = orders.First(o => o.Id == 2);
bool isValid1 = validator.ValidateAll(validOrder, out var errors1);
System.Console.WriteLine($"Walidacja zamówienia #{validOrder.Id}: {(isValid1 ? "Zatwierdzone" : "Odrzucone")}");

var invalidOrder = orders.First(o => o.Id == 6);
bool isValid2 = validator.ValidateAll(invalidOrder, out var errors2);
System.Console.WriteLine($"Walidacja zamówienia #{invalidOrder.Id}: {(isValid2 ? "Zatwierdzone" : "Odrzucone")}");
foreach (var error in errors2)
{
    System.Console.WriteLine($" - {error}");
}

System.Console.WriteLine("\n=== ZADANIE 3: ACTION, FUNC, PREDICATE ===");
var processor = new OrderProcessor(orders);

Predicate<Order> isVipOrder = o => o.Customer.IsVip;
Predicate<Order> isHighValue = o => o.TotalAmount > 1000m;
Predicate<Order> isNew = o => o.Status == OrderStatus.New;

var vipOrders = processor.FilterOrders(isVipOrder);
System.Console.WriteLine($"Znaleziono {vipOrders.Count()} zamówień od VIPów.");

Action<Order> printOrderAction = o => System.Console.WriteLine($"[Akcja] Zamówienie #{o.Id} - {o.TotalAmount:C}");
Action<Order> promoteStatusAction = o => { if (o.Status == OrderStatus.New) o.Status = OrderStatus.Validated; };

processor.ProcessOrders(promoteStatusAction, vipOrders);
processor.ProcessOrders(printOrderAction, vipOrders);

var projections = processor.ProjectOrders(o => new { o.Id, CustomerName = o.Customer.Name }, orders);
System.Console.WriteLine($"Pierwsza projekcja: ID={projections.First().Id}, Klient={projections.First().CustomerName}");

Func<IEnumerable<Order>, decimal> sumAggregator = list => list.Sum(x => x.TotalAmount);
Func<IEnumerable<Order>, decimal> avgAggregator = list => list.Any() ? list.Average(x => x.TotalAmount) : 0;
Func<IEnumerable<Order>, decimal> maxAggregator = list => list.Any() ? list.Max(x => x.TotalAmount) : 0;

System.Console.WriteLine($"Suma wszystkich zamówień: {processor.AggregateOrders(sumAggregator):C}");
System.Console.WriteLine($"Średnia wartość zamówienia: {processor.AggregateOrders(avgAggregator):C}");
System.Console.WriteLine($"Najdroższe zamówienie: {processor.AggregateOrders(maxAggregator):C}");

System.Console.WriteLine("\n--- Pipeline (Chain) ---");
processor.ProcessPipeline(
    filter: o => o.Status != OrderStatus.Cancelled && o.Items.Any(), 
    sortKey: o => o.TotalAmount, 
    topN: 2, 
    finalAction: o => System.Console.WriteLine($"Top Zamówienie: #{o.Id} na kwotę {o.TotalAmount:C}")
);

System.Console.WriteLine("\n=== ZADANIE 4: LINQ ===");

var q1_Join = from o in orders
              join c in customers on o.Customer.Id equals c.Id
              select new { OrderId = o.Id, CustomerName = c.Name, o.TotalAmount };

System.Console.WriteLine("\n1. Join (Zamówienia z Klientami):");
foreach (var item in q1_Join) System.Console.WriteLine($"Zamówienie #{item.OrderId} -> {item.CustomerName} ({item.TotalAmount:C})");

var q2_SelectMany = orders
    .SelectMany(o => o.Items)
    .Select(i => i.Product.Name)
    .Distinct();

System.Console.WriteLine("\n2. SelectMany (Wszystkie unikalne sprzedane produkty):");
System.Console.WriteLine(string.Join(", ", q2_SelectMany));

var q3_GroupBy = orders
    .Where(o => o.Items.Any())
    .GroupBy(o => o.Customer.City)
    .Select(g => new { City = g.Key, TotalCitySales = g.Sum(x => x.TotalAmount) });

System.Console.WriteLine("\n3. GroupBy (Przychód per Miasto):");
foreach (var item in q3_GroupBy) System.Console.WriteLine($"{item.City}: {item.TotalCitySales:C}");

var q4_GroupJoin = from c in customers
                   join o in orders on c.Id equals o.Customer.Id into customerOrders
                   from co in customerOrders.DefaultIfEmpty()
                   select new { Customer = c.Name, OrderId = co?.Id.ToString() ?? "Brak zamówień" };

System.Console.WriteLine("\n4. GroupJoin (Wszyscy klienci i ich zamówienia [Left Join]):");
foreach (var item in q4_GroupJoin) System.Console.WriteLine($"{item.Customer}: Zamówienie #{item.OrderId}");

var q5_Mixed = (from o in orders
                where o.Status != OrderStatus.Cancelled
                select o)
               .GroupBy(o => o.Customer.IsVip)
               .Select(g => new { IsVip = g.Key, AvgOrderValue = g.Average(o => o.TotalAmount) });

System.Console.WriteLine("\n5. Mixed Syntax (Średnia wartość zamówienia VIP vs Zwykły):");
foreach (var item in q5_Mixed) System.Console.WriteLine($"VIP: {item.IsVip} -> {item.AvgOrderValue:C}");

var q6_Extra = orders
    .SelectMany(o => o.Items)
    .GroupBy(i => i.Product.Category)
    .Select(g => new { Category = g.Key, ItemsSold = g.Sum(i => i.Quantity) })
    .OrderByDescending(x => x.ItemsSold)
    .FirstOrDefault();

System.Console.WriteLine("\n6. Najpopularniejsza kategoria (Suma sztuk):");
System.Console.WriteLine(q6_Extra != null ? $"{q6_Extra.Category} ({q6_Extra.ItemsSold} szt.)" : "Brak danych");