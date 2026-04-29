using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace OrderFlow.Console.Models;

[XmlRoot("Order")]
public class Order
{
    [JsonPropertyName("orderId")]
    [XmlAttribute("id")]
    public int Id { get; set; }

    [JsonPropertyName("customer")]
    [XmlElement("Customer")]
    public required Customer Customer { get; set; }

    [JsonPropertyName("orderDate")]
    [XmlElement("OrderDate")]
    public DateTime OrderDate { get; set; }

    [JsonPropertyName("status")]
    [XmlElement("Status")]
    public OrderStatus Status { get; set; } = OrderStatus.New;

    [JsonPropertyName("items")]
    [XmlArray("Items")]
    [XmlArrayItem("Item")]
    public List<OrderItem> Items { get; set; } = new();

    [JsonIgnore]
    [XmlIgnore]
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);

    [JsonIgnore]
    [XmlElement("TotalAmount")]
    public decimal TotalAmountXml
    {
        get => TotalAmount;
        set { }
    }
}
