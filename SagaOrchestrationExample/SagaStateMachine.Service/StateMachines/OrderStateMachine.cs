using Automatonymous;
using SagaStateMachine.Service.Instruments;
using Shared;
using Shared.Events;
using Shared.Messages;
using System;
using Microsoft.Extensions.Logging;

namespace SagaStateMachine.Service.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        private readonly ILogger<OrderStateMachine> _logger;
        public Event<OrderStartedEvent> OrderStartedEvent { get; set; }
        public Event<StockReservedEvent> StockReservedEvent { get; set; }
        public Event<StockNotReservedEvent> StockNotReservedEvent { get; set; }
        public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
        public Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }
      


        public State OrderCreated { get; set; }
        public State StockReserved { get; set; }
        public State StockNotReserved { get; set; }
        public State PaymentCompleted { get; set; }
        public State PaymentFailed { get; set; }
    

        public OrderStateMachine(ILogger<OrderStateMachine> logger)
        {
            _logger = logger;

            InstanceState(instance => instance.CurrentState);


            // This event is sent when we register a new request
            Event(() => OrderStartedEvent,
                orderStateInstance =>
                orderStateInstance.CorrelateBy<int>(
                        database => database.OrderId,
                        @event => @event.Message.OrderId)
                    .SelectId(e => Guid.NewGuid())
                );


            Event(() => StockReservedEvent,
                orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));


            Event(() => StockNotReservedEvent,
                orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));


            Event(() => PaymentCompletedEvent,
                orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));


            Event(() => PaymentFailedEvent,
                orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));


            // This event is sent when we register a new request
            Initially(When(OrderStartedEvent)
                .Then(context =>
                {
                    context.Instance.BuyerId = context.Data.BuyerId;
                    context.Instance.OrderId = context.Data.OrderId;
                    context.Instance.TotalPrice = context.Data.TotalPrice;
                    context.Instance.OrderDate = DateTime.Now;
                })
                .Then(context => _logger.LogInformation($"before Order [{context.Data.OrderId}] Requested!") )
                .TransitionTo(OrderCreated)
                .Then(context => _logger.LogInformation($"after Order [{context.Data.OrderId}] Requested!"))
                .Send(new Uri($"queue:{RabbitMQSettings.Order_Complete_Queue}"), context => new OrderCreatedEvent(context.Instance.CorrelationId)
                {
                    OrderItems = context.Data.OrderItems
                }));


            During(OrderCreated,
                When(StockReservedEvent)
                .TransitionTo(StockReserved)
                .Send(new Uri($"queue:{RabbitMQSettings.Payment_Queue}"), context => new PaymentStartedEvent(context.Instance.CorrelationId)
                {
                    OrderItems = context.Data.OrderItems,
                    TotalPrice = context.Instance.TotalPrice
                }),

                When(StockNotReservedEvent)
                .TransitionTo(StockNotReserved)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_Fail_Queue}"), context => new OrderFailedEvent()
                {
                    OrderId = context.Instance.OrderId,
                    Message = context.Data.Message
                }));


            During(StockReserved,
                When(PaymentCompletedEvent)
                .TransitionTo(PaymentCompleted)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_Complete_Queue}"), context => new OrderCompletedEvent
                {
                    OrderId = context.Instance.OrderId
                })
                .Finalize(),

                When(PaymentFailedEvent)
                .TransitionTo(PaymentFailed)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_Fail_Queue}"), context => new OrderFailedEvent()
                {
                    OrderId = context.Instance.OrderId,
                    Message = context.Data.Message
                })
                .Send(new Uri($"queue:{RabbitMQSettings.Rollback_Order_Items_Stock_Queue}"), context => new StockRollBackMessage
                {
                    OrderItems = context.Data.OrderItems
                }));

            SetCompletedWhenFinalized();
        }
    }
}
