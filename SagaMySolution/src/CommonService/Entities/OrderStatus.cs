namespace CommonService.Entities;

public enum OrderStatus
{
    Pending,
    Payment ,
    Completed,
    Failed
}

public enum OrderItemStatus
{
    Exist,
    Insufficient,
    NoExist
}


public class OrderRequest
{
    public OrderRequest() { }
    public OrderRequest(int orderId, List<OrderItemVm> orderItems)
    {
        OrderId = orderId;
        OrderItems = orderItems;
    }
    public int OrderId { get; set; }
    public List<OrderItemVm> OrderItems=new ();
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;
    public string? Description { get; set; } = String.Empty;
    public bool Tracked { get; set; } = false;
}

public record OrderResultMessage
{
    public OrderResultMessage(){}

    public OrderResultMessage(int orderId, 
        OrderStatus orderStatus, 
        string message,
        List<OrderItemResultMessage> orderItemResultMessages)
    {
        OrderId = orderId;
        OrderStatus = orderStatus;
        Message = message;
        OrderItemResultMessages = orderItemResultMessages;
    }
    
    public int OrderId { get; set; }
    public List<OrderItemResultMessage> OrderItemResultMessages=new ();
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Completed;
    public string? Message { get; set; } = String.Empty;
}

public record OrderItemResultMessage
{
    public OrderItemResultMessage(){}

    public OrderItemResultMessage(int productId, string text)
    {
        ProductId = productId;
        Text = text;
    }
    public int ProductId { get; set; }
    public string? Text { get; set; } = String.Empty;
}



public class OrderItemVm
{
    
    public OrderItemVm(){}
    public OrderItemVm(int id, int productId, int count, decimal price)
    {
        Id = id;
        ProductId = productId;
        Count = count;
        Price = price;
    }
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Count { get; set; }
    public decimal Price { get; set; }
    
    public OrderItemStatus OrderItemStatus { get; set; } = OrderItemStatus.Exist;
    public string? Description { get; set; } = String.Empty;
}