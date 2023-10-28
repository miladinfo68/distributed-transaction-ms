namespace OrderService.ViewModels
{
    public class OrderVm
    {
        public int BuyerId { get; set; }
        public List<OrderItemVm> OrderItems { get; set; } = new();
    }
}
