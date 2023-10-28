namespace CommonService.Entities;

public class MessageDelivery
{
    public int Id { get; set; }
    public int BuyerId { get; set; }
    public int OrderId { get; set; }
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Completed;
    public string? Message { get; set; } = String.Empty;
    public DateTime SendDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpireDate { get; set; } = DateTime.UtcNow.AddHours(24);
    public bool IsActive { get; set; } = true;
}