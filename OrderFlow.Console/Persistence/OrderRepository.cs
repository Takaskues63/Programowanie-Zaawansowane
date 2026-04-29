using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Serialization;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Persistence;

[XmlRoot("Orders")]
public class OrderListWrapper
{
    [XmlElement("Order")]
    public List<Order> Orders { get; set; } = new();
}

public class OrderRepository
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented        = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder              = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters           = { new JsonStringEnumConverter() }
    };

    public async Task SaveToJsonAsync(IEnumerable<Order> orders, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var stream = new FileStream(
            path, FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize: 4096, useAsync: true);

        await JsonSerializer.SerializeAsync(stream, orders.ToList(), _jsonOptions);
    }

    public async Task<List<Order>> LoadFromJsonAsync(string path)
    {
        if (!File.Exists(path))
            return new List<Order>();

        await using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, useAsync: true);

        return await JsonSerializer.DeserializeAsync<List<Order>>(stream, _jsonOptions)
               ?? new List<Order>();
    }

    public async Task SaveToXmlAsync(IEnumerable<Order> orders, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var wrapper    = new OrderListWrapper { Orders = orders.ToList() };
        var serializer = new XmlSerializer(typeof(OrderListWrapper));

        var settings = new XmlWriterSettings
        {
            Indent    = true,
            Async     = true,
            Encoding  = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        };

        await using var stream = new FileStream(
            path, FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize: 4096, useAsync: true);

        await using var writer = XmlWriter.Create(stream, settings);
        serializer.Serialize(writer, wrapper);
        await writer.FlushAsync();
    }

    public async Task<List<Order>> LoadFromXmlAsync(string path)
    {
        if (!File.Exists(path))
            return new List<Order>();

        await using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, useAsync: true);

        using var reader     = new StreamReader(stream);
        var       xmlContent = await reader.ReadToEndAsync();

        var serializer = new XmlSerializer(typeof(OrderListWrapper));
        using var stringReader = new System.IO.StringReader(xmlContent);

        var wrapper = (OrderListWrapper?)serializer.Deserialize(stringReader);
        return wrapper?.Orders ?? new List<Order>();
    }
}
