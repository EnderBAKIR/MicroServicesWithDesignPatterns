using MassTransit;
using Shared;
using Shared.Events;
using Shared.Interfaces;

namespace Payment.API.Consumers
{
    public class StockReservedRequestPaymentConsumer : IConsumer<IStockReservedRequestPayment>
    {

        private readonly ILogger<StockReservedRequestPaymentConsumer> _logger;

        private readonly IPublishEndpoint _publisEndpoint;

        public StockReservedRequestPaymentConsumer(ILogger<StockReservedRequestPaymentConsumer> logger, IPublishEndpoint publisEndpoint)
        {
            _logger = logger;
            _publisEndpoint = publisEndpoint;
        }





        public async Task Consume(ConsumeContext<IStockReservedRequestPayment> context)
        {
            var balance = 3000m;

            if (balance > context.Message.Payment.TotalPrice)
            {
                _logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was withdrawn from credit card for userId = {context.Message.BuyerId}");


                await _publisEndpoint.Publish(new PaymentCompletedEvent(context.Message.CorrelationId));
            }

            else
            {
                _logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was not widthrawn from credit card for user id={context.Message.BuyerId}");


                await _publisEndpoint.Publish(new PaymentFailedlEvent(context.Message.CorrelationId) {  Reason = "not enough balance", OrderItems = context.Message.OrderItems });
            }

        }
    }
}
