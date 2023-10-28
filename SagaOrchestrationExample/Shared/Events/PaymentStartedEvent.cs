using MassTransit;
using System;
using System.Collections.Generic;

namespace Shared.Events
{
    public class PaymentStartedEvent : CorrelatedBy<Guid>
    {
        public PaymentStartedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; }
        public decimal TotalPrice { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
