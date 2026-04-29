using System.Xml.Linq;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Persistence;

public class XmlReportBuilder
{
    public XDocument BuildReport(IEnumerable<Order> orders)
    {
        var orderList = orders.ToList();

        var byStatus = orderList
            .GroupBy(o => o.Status)
            .Select(g => new XElement("status",
                new XAttribute("name",    g.Key.ToString()),
                new XAttribute("count",   g.Count()),
                new XAttribute("revenue", g.Sum(o => o.TotalAmount).ToString("F2"))));

        var byCustomer = orderList
            .GroupBy(o => o.Customer.Id)
            .Select(g =>
            {
                var c = g.First().Customer;
                return new XElement("customer",
                    new XAttribute("id",    c.Id),
                    new XAttribute("name",  c.Name),
                    new XAttribute("isVip", c.IsVip.ToString().ToLower()),
                    new XElement("orderCount", g.Count()),
                    new XElement("totalSpent", g.Sum(o => o.TotalAmount).ToString("F2")),
                    new XElement("orders",
                        g.Select(o => new XElement("orderRef",
                            new XAttribute("id",    o.Id),
                            new XAttribute("total", o.TotalAmount.ToString("F2"))))));
            });

        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("report",
                new XAttribute("generated", DateTime.Now.ToString("s")),
                new XElement("summary",
                    new XAttribute("totalOrders",  orderList.Count),
                    new XAttribute("totalRevenue", orderList.Sum(o => o.TotalAmount).ToString("F2"))),
                new XElement("byStatus",   byStatus),
                new XElement("byCustomer", byCustomer)));
    }

    public async Task SaveReportAsync(XDocument report, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var stream = new FileStream(
            path, FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize: 4096, useAsync: true);

        await using var writer = new StreamWriter(stream, System.Text.Encoding.UTF8);
        await writer.WriteAsync(report.ToString());
    }

    public async Task<IEnumerable<int>> FindHighValueOrderIdsAsync(
        string reportPath, decimal threshold)
    {
        await using var stream = new FileStream(
            reportPath, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, useAsync: true);

        using var reader = new StreamReader(stream);
        var xml = await reader.ReadToEndAsync();

        var doc = XDocument.Parse(xml);

        return doc
            .Descendants("orderRef")
            .Where(el => decimal.Parse(
                el.Attribute("total")!.Value,
                System.Globalization.CultureInfo.InvariantCulture) > threshold)
            .Select(el => (int)el.Attribute("id")!)
            .Distinct()
            .OrderBy(id => id)
            .ToList();
    }
}
