using System.Collections.Generic;

namespace Shared.Messages
{
    public class StockRollBackMessage
    {
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
