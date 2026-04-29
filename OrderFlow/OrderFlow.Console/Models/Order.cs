namespace OrderFlow.Console.Models;

public class Order
{
    public int Id { get; set; }
    public required Customer Customer { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public List<OrderItem> Items { get; set; } = new();

    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
}