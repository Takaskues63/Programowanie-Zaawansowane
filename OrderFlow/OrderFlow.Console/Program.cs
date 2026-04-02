using System.Diagnostics;
using OrderFlow.Console.Data;
using OrderFlow.Console.Events;
using OrderFlow.Console.Models;
using OrderFlow.Console.Services;


static void Header(string text)
{
    System.Console.WriteLine();
    System.Console.WriteLine("╔" + new string('═', text.Length + 2) + "╗");
    System.Console.WriteLine($"║ {text} ║");
    System.Console.WriteLine("╚" + new string('═', text.Length + 2) + "╝");
}

static void Section(string text)
{
    System.Console.WriteLine($"\n--- {text} ---");
}

var products  = SampleData.Products;
var customers = SampleData.Customers;
var orders    = SampleData.Orders;

Header("LABORATORIUM 1 — Delegaty, LINQ");

Section("Zadanie 2 — Walidacja zamówień");

var validator = new OrderValidator();

var validOrder = orders.First(o => o.Id == 2);
bool ok1 = validator.ValidateAll(validOrder, out var err1);
System.Console.WriteLine($"Walidacja #{validOrder.Id} ({validOrder.Customer.Name}): " +
    (ok1 ? "✓ Zatwierdzone" : "✗ Odrzucone"));

var badOrder = orders.First(o => o.Id == 6);
bool ok2 = validator.ValidateAll(badOrder, out var err2);
System.Console.WriteLine($"Walidacja #{badOrder.Id} ({badOrder.Customer.Name}):  " +
    (ok2 ? "✓ Zatwierdzone" : "✗ Odrzucone"));
foreach (var e in err2)
    System.Console.WriteLine($"  - {e}");

Section("Zadanie 3 — Action / Func / Predicate");

var processor = new OrderProcessor(orders);

Predicate<Order> isVipOrder   = o => o.Customer.IsVip;
Predicate<Order> isHighValue  = o => o.TotalAmount > 1000m;
Predicate<Order> isNewOrder   = o => o.Status == OrderStatus.New;

var vipOrders   = processor.FilterOrders(isVipOrder);
var highOrders  = processor.FilterOrders(isHighValue);
var newOrders   = processor.FilterOrders(isNewOrder);

System.Console.WriteLine($"Zamówienia VIP:          {vipOrders.Count()}");
System.Console.WriteLine($"Zamówienia > 1000 zł:   {highOrders.Count()}");
System.Console.WriteLine($"Nowe zamówienia:         {newOrders.Count()}");

Action<Order> promoteStatus = o =>
{
    if (o.Status == OrderStatus.New) o.Status = OrderStatus.Validated;
};
Action<Order> printOrder = o =>
    System.Console.WriteLine($"  [Akcja] #{o.Id} | {o.Customer.Name,-20} | {o.TotalAmount,10:C}");

processor.ProcessOrders(promoteStatus, vipOrders);
System.Console.WriteLine("Zamówienia VIP po zmianie statusu:");
processor.ProcessOrders(printOrder, vipOrders);

var projections = processor.ProjectOrders(
    o => new { o.Id, Customer = o.Customer.Name, o.TotalAmount },
    orders);
System.Console.WriteLine("Projekcja (ID, Klient, Kwota):");
foreach (var p in projections)
    System.Console.WriteLine($"  #{p.Id}  {p.Customer,-20}  {p.TotalAmount:C}");

Func<IEnumerable<Order>, decimal> sum = list => list.Sum(x => x.TotalAmount);
Func<IEnumerable<Order>, decimal> avg = list => list.Any() ? list.Average(x => x.TotalAmount) : 0;
Func<IEnumerable<Order>, decimal> max = list => list.Any() ? list.Max(x => x.TotalAmount) : 0;

System.Console.WriteLine($"Suma:    {processor.AggregateOrders(sum):C}");
System.Console.WriteLine($"Średnia: {processor.AggregateOrders(avg):C}");
System.Console.WriteLine($"Max:     {processor.AggregateOrders(max):C}");

System.Console.WriteLine("Top 2 zamówienia (nie-anulowane, nie-puste, wg kwoty malejąco):");
processor.ProcessPipeline(
    filter:      o => o.Status != OrderStatus.Cancelled && o.Items.Any(),
    sortKey:     o => o.TotalAmount,
    topN:        2,
    finalAction: o => System.Console.WriteLine($"  TOP #{o.Id}  {o.Customer.Name,-20}  {o.TotalAmount:C}"));

Section("Zadanie 4 — LINQ");

System.Console.WriteLine("\n[1] Join zamówień z klientami (query syntax):");
var q1 = from o in orders
         join c in customers on o.Customer.Id equals c.Id
         select new { OrderId = o.Id, c.Name, c.City, o.TotalAmount };

foreach (var x in q1)
    System.Console.WriteLine($"  #{x.OrderId}  {x.Name,-20}  {x.City,-10}  {x.TotalAmount:C}");

System.Console.WriteLine("\n[2] SelectMany — wszystkie sprzedane produkty (unikalne):");
var q2 = orders
    .SelectMany(o => o.Items)
    .Select(i => i.Product.Name)
    .Distinct()
    .OrderBy(n => n);

System.Console.WriteLine("  " + string.Join(", ", q2));

System.Console.WriteLine("\n[3] GroupBy — przychód per miasto (method syntax):");
var q3 = orders
    .Where(o => o.Items.Any())
    .GroupBy(o => o.Customer.City)
    .Select(g => new { City = g.Key, Total = g.Sum(x => x.TotalAmount), Count = g.Count() })
    .OrderByDescending(x => x.Total);

foreach (var x in q3)
    System.Console.WriteLine($"  {x.City,-12}  {x.Total,10:C}  ({x.Count} zamówień)");

System.Console.WriteLine("\n[4] GroupJoin — wszyscy klienci i ich zamówienia (left join):");
var q4 = from c in customers
         join o in orders on c.Id equals o.Customer.Id into co
         from ord in co.DefaultIfEmpty()
         select new { c.Name, OrderId = ord?.Id.ToString() ?? "—" };

foreach (var x in q4)
    System.Console.WriteLine($"  {x.Name,-20}  Zamówienie #{x.OrderId}");

System.Console.WriteLine("\n[5] Mixed syntax — średnia wartość VIP vs zwykły:");
var q5 = (from o in orders
          where o.Status != OrderStatus.Cancelled
          select o)
         .GroupBy(o => o.Customer.IsVip)
         .Select(g => new { IsVip = g.Key, Avg = g.Average(o => o.TotalAmount) });

foreach (var x in q5)
    System.Console.WriteLine($"  {(x.IsVip ? "VIP    " : "Zwykły")}  średnia: {x.Avg:C}");

System.Console.WriteLine("\n[6] Najpopularniejsza kategoria produktów (method syntax):");
var q6 = orders
    .SelectMany(o => o.Items)
    .GroupBy(i => i.Product.Category)
    .Select(g => new { Category = g.Key, Qty = g.Sum(i => i.Quantity) })
    .OrderByDescending(x => x.Qty);

foreach (var x in q6)
    System.Console.WriteLine($"  {x.Category,-15}  {x.Qty} szt.");


Header("LABORATORIUM 2 — Zdarzenia i asynchroniczność");

Section("Zadanie 1 — Zdarzenia (OrderPipeline)");

var freshOrders = SampleData.FreshOrders();
var pipeline    = new OrderPipeline();

pipeline.StatusChanged += (_, e) =>
    System.Console.WriteLine(
        $"  [LOG]   {e.Timestamp:HH:mm:ss.fff}  #{e.Order.Id}  {e.OldStatus} → {e.NewStatus}");

pipeline.StatusChanged += (_, e) =>
{
    if (e.NewStatus == OrderStatus.Completed)
        System.Console.WriteLine(
            $"  [EMAIL] Potwierdzenie wysłane do: {e.Order.Customer.Name}");
    else if (e.NewStatus == OrderStatus.Cancelled)
        System.Console.WriteLine(
            $"  [EMAIL] Info o anulacji wysłane do: {e.Order.Customer.Name}");
};

int completedLive  = 0;
decimal revenueLive = 0m;
pipeline.StatusChanged += (_, e) =>
{
    if (e.NewStatus == OrderStatus.Completed)
    {
        completedLive++;
        revenueLive += e.Order.TotalAmount;
        System.Console.WriteLine(
            $"  [STATS] Ukończone: {completedLive}  |  Przychód bieżący: {revenueLive:C}");
    }
};

pipeline.ValidationCompleted += (_, e) =>
{
    if (e.IsValid)
        System.Console.WriteLine($"  [VALID] #{e.Order.Id} przeszło walidację ✓");
    else
    {
        System.Console.WriteLine($"  [VALID] #{e.Order.Id} ODRZUCONE ✗");
        foreach (var err in e.Errors)
            System.Console.WriteLine($"           → {err}");
    }
};

foreach (var orderId in new[] { 1, 2, 6 })
{
    var o = freshOrders.First(x => x.Id == orderId);
    pipeline.ProcessOrder(o);
}

Section("Zadanie 2 — Async / Await / Task.WhenAll");

var sim         = new ExternalServiceSimulator();
var asyncOrders = SampleData.FreshOrders()
    .Where(o => o.Items.Any())
    .Take(3)
    .ToList();

System.Console.WriteLine("\n[A] Przetwarzanie SEKWENCYJNE:");
var swSeq = Stopwatch.StartNew();
foreach (var o in asyncOrders)
    await sim.ProcessOrderAsync(o);
swSeq.Stop();
System.Console.WriteLine($"\n  Czas sekwencyjny: {swSeq.ElapsedMilliseconds} ms");

System.Console.WriteLine("\n[B] Przetwarzanie RÓWNOLEGŁE (SemaphoreSlim max 3):");
var allAsyncOrders = SampleData.FreshOrders()
    .Where(o => o.Items.Any())
    .ToList();
var swPar = Stopwatch.StartNew();
await sim.ProcessMultipleOrdersAsync(allAsyncOrders);
swPar.Stop();
System.Console.WriteLine($"\n  Czas równoległy:  {swPar.ElapsedMilliseconds} ms");

double speedup = (double)swSeq.ElapsedMilliseconds / swPar.ElapsedMilliseconds;
System.Console.WriteLine(
    $"  Przyspieszenie:   {speedup:F1}x  " +
    $"(zaoszczędzono {swSeq.ElapsedMilliseconds - swPar.ElapsedMilliseconds} ms)");

Section("Zadanie 3 — Thread Safety");

var threadOrders = SampleData.FreshOrders();

System.Console.WriteLine("\n[UNSAFE] Wersja bez synchronizacji (3 uruchomienia):");
for (int run = 1; run <= 3; run++)
{
    var unsafeStats = new OrderStatisticsUnsafe();
    Parallel.ForEach(threadOrders, order => unsafeStats.Record(order));
    unsafeStats.PrintSummary($"Unsafe run {run}");
}

System.Console.WriteLine("\n[SAFE] Wersja z lock / Interlocked / ConcurrentDictionary (3 uruchomienia):");
for (int run = 1; run <= 3; run++)
{
    var safeStats = new OrderStatistics();
    Parallel.ForEach(threadOrders, order => safeStats.Record(order));
    safeStats.PrintSummary($"Safe   run {run}");
}

System.Console.WriteLine(
    "\n✔ Wersja SAFE daje zawsze identyczne wyniki niezależnie od harmonogramu wątków.");
