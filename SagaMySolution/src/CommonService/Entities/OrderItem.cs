namespace CommonService.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Count { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; } = string.Empty;
}