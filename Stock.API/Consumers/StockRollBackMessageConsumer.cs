using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Messages;
using Stock.API.Models;

namespace Stock.API.Consumers
{
    public class StockRollBackMessageConsumer : IConsumer<IStockRollBackMessage>
    {
        private readonly AppDbContext _appDbContext;
        private ILogger<OrderCreatedEventConsumer> _logger;

        public StockRollBackMessageConsumer(AppDbContext appDbContext, ILogger<OrderCreatedEventConsumer> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IStockRollBackMessage> context)
        {
            foreach (var item in context.Message.OrderItems)
            {
                var stock = await _appDbContext.Stocks.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);

                if (stock != null)
                {
                    stock.Count += item.Count;
                    await _appDbContext.SaveChangesAsync();
                }

            }

            _logger.LogInformation($"Stock was released");
        }
    }
}
