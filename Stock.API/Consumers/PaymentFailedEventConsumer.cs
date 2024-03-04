using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.API.Models;

namespace Stock.API.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedlEvent>
    {
        private readonly AppDbContext _appDbContext;
        private ILogger<OrderCreatedEventConsumer> _logger;

        public PaymentFailedEventConsumer(AppDbContext appDbContext, ILogger<OrderCreatedEventConsumer> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentFailedlEvent> context)
        {
            foreach(var item in context.Message.OrderItems)
            {
                var stock = await _appDbContext.Stocks.FirstOrDefaultAsync(x=>x.ProductId == item.ProductId);

                if (stock != null)
                {
                    stock.Count += item.Count;
                    await _appDbContext.SaveChangesAsync();
                }

            }

            _logger.LogInformation($"Stock was released for Order Id {context.Message.OrderId}");
        }
    }
}
