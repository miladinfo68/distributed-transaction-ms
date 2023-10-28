using System.Collections.Generic;

namespace Order.API.ViewModels
{
    public class OrderVM
    {
        public int BuyerId { get; set; }
        public List<OrderItemVM> OrderItems { get; set; }
    }
}
