namespace OrderFlow.Console.Models;

public class OrderItem
{
    public required Product Product { get; set; }
    public int Quantity { get; set; }

    public decimal TotalPrice => Product.Price * Quantity;
}