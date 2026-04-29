using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace OrderFlow.Console.Models;

public class OrderItem
{
    [JsonPropertyName("product")]
    [XmlElement("Product")]
    public required Product Product { get; set; }

    [JsonPropertyName("quantity")]
    [XmlAttribute("qty")]
    public int Quantity { get; set; }

    [JsonIgnore]
    [XmlIgnore]
    public decimal TotalPrice => Product.Price * Quantity;
}
