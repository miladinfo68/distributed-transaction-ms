using MassTransit;
using System;
using System.Collections.Generic;

namespace Shared.Events
{
    public class OrderCreatedEvent : CorrelatedBy<Guid>
    {
        public OrderCreatedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; }
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
