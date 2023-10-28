using System.Text.Json;

namespace CommonService.Entities;

public class OutboxOrder
{
    public int Id { get; set; }
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string JsonData { get; set; } = string.Empty;
    public bool Tracked { get; set; } = false;

    public static implicit operator OutboxOrder(OrderRequest order)
    {
        var jsonData = JsonSerializer.Serialize(order);
        return new OutboxOrder { JsonData = jsonData };
    }
}