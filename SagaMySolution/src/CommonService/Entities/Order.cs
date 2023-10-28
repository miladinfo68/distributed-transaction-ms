namespace CommonService.Entities;

public class Order
{
    public int Id { get; set; }
    public int BuyerId { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
    public int Version { get; set; } = 1;
    public string? Description { get; set; } = string.Empty;
    public List<OrderItem> OrderItems { get; set; } = new();
}