﻿using System.Collections.Generic;

namespace Shared.Events
{
    public class OrderStartedEvent
    {
        public int OrderId { get; set; }
        public int BuyerId { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
