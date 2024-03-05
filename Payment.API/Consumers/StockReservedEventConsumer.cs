using MassTransit;
using RabbitMQ.Client.Events;
using Shared;

namespace Payment.API.Consumers
{
    public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
    {

        private readonly ILogger<StockReservedEventConsumer> _logger;

        private readonly IPublishEndpoint _publisEndpoint;

        public StockReservedEventConsumer(ILogger<StockReservedEventConsumer> logger, IPublishEndpoint publisEndpoint)
        {
            _logger = logger;
            _publisEndpoint = publisEndpoint;
        }

        public async Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            var balance = 3000m;

            if (balance > context.Message.Payment.TotalPrice)
            {
                _logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was withdrawn from credit card for userId = {context.Message.BuyerId}");


                await _publisEndpoint.Publish(new PaymentCompletedEvent { BuyerId = context.Message.BuyerId, OrderId = context.Message.OrderId });
            }

            else
            {
                _logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was not widthrawn from credit card for user id={context.Message.BuyerId}");


                await _publisEndpoint.Publish(new PaymentFailedlEvent { BuyerId=context.Message.BuyerId, OrderId =context.Message.OrderId , Message="not enough balance" , OrderItems=context.Message.OrderItems });
            }


        }
    }
}
