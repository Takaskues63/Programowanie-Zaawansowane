using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace OrderFlow.Console.Models;

public class Customer
{
    [JsonPropertyName("customerId")]
    [XmlAttribute("id")]
    public int Id { get; set; }

    [JsonPropertyName("fullName")]
    [XmlElement("FullName")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    [XmlElement("City")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("isVip")]
    [XmlElement("IsVip")]
    public bool IsVip { get; set; }
}
