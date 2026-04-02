using OrderFlow.Console.Models;

namespace OrderFlow.Console.Data;

public static class SampleData
{
    public static List<Product>  Products  { get; }
    public static List<Customer> Customers { get; }
    public static List<Order>    Orders    { get; }

    static SampleData()
    {
        Products = new List<Product>
        {
            new() { Id = 1, Name = "Laptop",          Price = 4500m,  Category = "Elektronika" },
            new() { Id = 2, Name = "Myszka",           Price = 150m,   Category = "Elektronika" },
            new() { Id = 3, Name = "Klawiatura",       Price = 300m,   Category = "Elektronika" },
            new() { Id = 4, Name = "Krzesło biurowe",  Price = 800m,   Category = "Meble"       },
            new() { Id = 5, Name = "Biurko",           Price = 1200m,  Category = "Meble"       },
        };

        Customers = new List<Customer>
        {
            new() { Id = 1, Name = "Jan Kowalski",       City = "Warszawa", IsVip = false },
            new() { Id = 2, Name = "Anna Nowak",         City = "Kraków",   IsVip = true  }, // VIP
            new() { Id = 3, Name = "Piotr Wiśniewski",   City = "Warszawa", IsVip = false },
            new() { Id = 4, Name = "Katarzyna Wójcik",   City = "Poznań",   IsVip = false },
        };

        Orders = new List<Order>
        {
            new() { Id = 1, Customer = Customers[0], OrderDate = DateTime.Now.AddDays(-2),
                    Status = OrderStatus.Completed,
                    Items = { new() { Product = Products[0], Quantity = 1 },
                              new() { Product = Products[1], Quantity = 1 } } },

            new() { Id = 2, Customer = Customers[1], OrderDate = DateTime.Now.AddDays(-1),
                    Status = OrderStatus.New,
                    Items = { new() { Product = Products[3], Quantity = 2 },
                              new() { Product = Products[4], Quantity = 1 } } },

            new() { Id = 3, Customer = Customers[2], OrderDate = DateTime.Now.AddHours(-5),
                    Status = OrderStatus.Processing,
                    Items = { new() { Product = Products[2], Quantity = 1 } } },

            new() { Id = 4, Customer = Customers[1], OrderDate = DateTime.Now,
                    Status = OrderStatus.New,
                    Items = { new() { Product = Products[0], Quantity = 2 } } },

            new() { Id = 5, Customer = Customers[3], OrderDate = DateTime.Now.AddDays(-10),
                    Status = OrderStatus.Cancelled,
                    Items = { new() { Product = Products[1], Quantity = 5 } } },

            new() { Id = 6, Customer = Customers[0], OrderDate = DateTime.Now.AddDays(1),
                    Status = OrderStatus.New,
                    Items = new List<OrderItem>() },
        };
    }
    public static List<Order> FreshOrders() => new List<Order>
    {
        new() { Id = 1, Customer = Customers[0], OrderDate = DateTime.Now.AddDays(-2),
                Status = OrderStatus.New,
                Items = { new() { Product = Products[0], Quantity = 1 },
                          new() { Product = Products[1], Quantity = 1 } } },

        new() { Id = 2, Customer = Customers[1], OrderDate = DateTime.Now.AddDays(-1),
                Status = OrderStatus.New,
                Items = { new() { Product = Products[3], Quantity = 2 },
                          new() { Product = Products[4], Quantity = 1 } } },

        new() { Id = 3, Customer = Customers[2], OrderDate = DateTime.Now.AddHours(-5),
                Status = OrderStatus.New,
                Items = { new() { Product = Products[2], Quantity = 1 } } },

        new() { Id = 4, Customer = Customers[1], OrderDate = DateTime.Now,
                Status = OrderStatus.New,
                Items = { new() { Product = Products[0], Quantity = 2 } } },

        new() { Id = 5, Customer = Customers[3], OrderDate = DateTime.Now.AddDays(-10),
                Status = OrderStatus.Cancelled,
                Items = { new() { Product = Products[1], Quantity = 5 } } },

        new() { Id = 6, Customer = Customers[0], OrderDate = DateTime.Now.AddDays(1),
                Status = OrderStatus.New,
                Items = new List<OrderItem>() },
    };
}
