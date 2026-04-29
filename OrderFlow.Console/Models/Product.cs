using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace OrderFlow.Console.Models;

public class Product
{
    [JsonPropertyName("productId")]
    [XmlAttribute("id")]
    public int Id { get; set; }

    [JsonPropertyName("productName")]
    [XmlElement("ProductName")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    [XmlElement("Price")]
    public decimal Price { get; set; }

    [JsonPropertyName("category")]
    [XmlElement("Category")]
    public string Category { get; set; } = string.Empty;
}
