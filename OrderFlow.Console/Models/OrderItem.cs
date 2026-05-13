using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace OrderFlow.Console.Models;

public class OrderItem
{
    [JsonIgnore]
    [XmlIgnore]
    public int Id { get; set; }

    [JsonIgnore]
    [XmlIgnore]
    public int OrderId { get; set; }

    [JsonIgnore]
    [XmlIgnore]
    public int ProductId { get; set; }

    [JsonPropertyName("product")]
    [XmlElement("Product")]
    public required Product Product { get; set; }

    [JsonPropertyName("quantity")]
    [XmlAttribute("qty")]
    public int Quantity { get; set; }

    [JsonPropertyName("unitPrice")]
    [XmlElement("UnitPrice")]
    public decimal UnitPrice { get; set; }

    [JsonIgnore]
    [XmlIgnore]
    public decimal TotalPrice => UnitPrice * Quantity;
}
