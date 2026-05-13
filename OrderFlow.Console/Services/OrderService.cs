using Microsoft.EntityFrameworkCore;
using OrderFlow.Console.Models;
using OrderFlow.Console.Persistence;

namespace OrderFlow.Console.Services;

public class OrderService
{
    public async Task ProcessOrderAsync(OrderFlowContext db, int orderId)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var order = await db.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new InvalidOperationException($"Zamówienie #{orderId} nie istnieje.");

            if (order.Status != OrderStatus.New)
                throw new InvalidOperationException(
                    $"Zamówienie #{orderId} ma status {order.Status}, oczekiwano New.");

            order.Status = OrderStatus.Processing;
            await db.SaveChangesAsync();

            foreach (var item in order.Items)
            {
                if (item.Product.Stock < item.Quantity)
                    throw new InvalidOperationException(
                        $"Brak towaru: {item.Product.Name} " +
                        $"(stan: {item.Product.Stock}, wymagane: {item.Quantity}).");

                item.Product.Stock -= item.Quantity;
            }

            order.Status = OrderStatus.Completed;
            await db.SaveChangesAsync();

            await transaction.CommitAsync();
            System.Console.WriteLine(
                $"  ✓ Zamówienie #{orderId} przetworzone pomyślnie → Completed");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            System.Console.WriteLine($"  ✗ Rollback zamówienia #{orderId}: {ex.Message}");
            throw;
        }
    }
}
