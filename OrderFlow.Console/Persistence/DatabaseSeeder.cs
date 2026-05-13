using Microsoft.EntityFrameworkCore;
using OrderFlow.Console.Data;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(OrderFlowContext db)
    {
        if (await db.Products.AnyAsync()) return;

        var products = SampleData.Products.Select(p => new Product
        {
            Name     = p.Name,
            Price    = p.Price,
            Category = p.Category,
            Stock    = 10
        }).ToList();

        await db.Products.AddRangeAsync(products);
        await db.SaveChangesAsync();

        var customers = SampleData.Customers.Select(c => new Customer
        {
            Name  = c.Name,
            City  = c.City,
            IsVip = c.IsVip,
            Email = c.Email
        }).ToList();

        await db.Customers.AddRangeAsync(customers);
        await db.SaveChangesAsync();

        var orders = new List<Order>
        {
            new() { CustomerId = customers[0].Id, Customer = customers[0],
                    OrderDate = DateTime.Now.AddDays(-2), Status = OrderStatus.Completed,
                    Items = {
                        new() { ProductId = products[0].Id, Product = products[0], Quantity = 1, UnitPrice = products[0].Price },
                        new() { ProductId = products[1].Id, Product = products[1], Quantity = 1, UnitPrice = products[1].Price }
                    }},
            new() { CustomerId = customers[1].Id, Customer = customers[1],
                    OrderDate = DateTime.Now.AddDays(-1), Status = OrderStatus.New, Notes = "Pilne",
                    Items = {
                        new() { ProductId = products[3].Id, Product = products[3], Quantity = 2, UnitPrice = products[3].Price },
                        new() { ProductId = products[4].Id, Product = products[4], Quantity = 1, UnitPrice = products[4].Price }
                    }},
            new() { CustomerId = customers[2].Id, Customer = customers[2],
                    OrderDate = DateTime.Now.AddHours(-5), Status = OrderStatus.Processing,
                    Items = {
                        new() { ProductId = products[2].Id, Product = products[2], Quantity = 1, UnitPrice = products[2].Price }
                    }},
            new() { CustomerId = customers[1].Id, Customer = customers[1],
                    OrderDate = DateTime.Now, Status = OrderStatus.New,
                    Items = {
                        new() { ProductId = products[0].Id, Product = products[0], Quantity = 2, UnitPrice = products[0].Price }
                    }},
            new() { CustomerId = customers[3].Id, Customer = customers[3],
                    OrderDate = DateTime.Now.AddDays(-10), Status = OrderStatus.Cancelled,
                    Items = {
                        new() { ProductId = products[1].Id, Product = products[1], Quantity = 5, UnitPrice = products[1].Price }
                    }},
            new() { CustomerId = customers[0].Id, Customer = customers[0],
                    OrderDate = DateTime.Now.AddDays(-3), Status = OrderStatus.Validated, Notes = "Ekspresowa dostawa",
                    Items = {
                        new() { ProductId = products[4].Id, Product = products[4], Quantity = 1, UnitPrice = products[4].Price },
                        new() { ProductId = products[2].Id, Product = products[2], Quantity = 2, UnitPrice = products[2].Price }
                    }},
        };

        await db.Orders.AddRangeAsync(orders);
        await db.SaveChangesAsync();
    }
}
