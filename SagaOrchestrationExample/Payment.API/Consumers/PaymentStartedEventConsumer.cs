using MassTransit;
using Shared;
using Shared.Events;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Payment.API.Consumers
{
    public  class PaymentStartedEventConsumer : IConsumer<PaymentStartedEvent>
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<PaymentStartedEventConsumer> _logger;

        public PaymentStartedEventConsumer(
            ISendEndpointProvider sendEndpointProvider,
            IPublishEndpoint publishEndpoint,
            ILogger<PaymentStartedEventConsumer> logger)
        {
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentStartedEvent> context)
        {
            var sendEndpoint =
                await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Order_Orchestrator_Queue}"));
            if (context.Message.TotalPrice < 1000)
            {
                await sendEndpoint.Send(new PaymentFailedEvent(context.Message.CorrelationId)
                {
                    Message = "The minimum purchase amount is 1000",
                    OrderItems = context.Message.OrderItems
                });
                _logger.LogInformation($"Error [Payment-Service] --> The minimum purchase amount is 1000");
            }
            else
            {
                await sendEndpoint.Send(new PaymentCompletedEvent(context.Message.CorrelationId));
                _logger.LogInformation($"OK [Payment-Service] Success!");
            }
        }
    }
}