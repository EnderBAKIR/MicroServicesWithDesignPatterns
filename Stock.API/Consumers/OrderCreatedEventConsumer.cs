﻿using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.API.Models;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly AppDbContext _appDbContext;
        private ILogger<OrderCreatedEventConsumer> _logger;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderCreatedEventConsumer(AppDbContext appDbContext, ILogger<OrderCreatedEventConsumer> logger, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
        {
            _appDbContext = appDbContext;
            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var stockResult = new List<bool>();//check all orderItems 


            foreach (var item in context.Message.OrderItems)
            {
                stockResult.Add(await _appDbContext.Stocks.AnyAsync(x => x.ProductId == item.ProductId && x.Count > item.Count));
            }
            if (stockResult.All(x => x.Equals(true)))
            {
                foreach (var item in context.Message.OrderItems)
                {
                    var stock = await _appDbContext.Stocks.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);


                    if (stock != null)
                    {
                        stock.Count -= item.Count;
                    }

                    await _appDbContext.SaveChangesAsync();


                }

                _logger.LogInformation($"Stock was reserved for Buyer Id :{context.Message.BuyerId}");

                var sendEndPoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettingsConst.StockReservedEventQueueName}"));


                StockReservedEvent stockReservedEvent = new()
                {
                    Payment = context.Message.Payment,
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    OrderItems = context.Message.OrderItems
                };


                await sendEndPoint.Send(stockReservedEvent);

            }
            else
            {
                await _publishEndpoint.Publish(new StockNotReservedEvent()
                {
                    
                    OrderId = context.Message.OrderId,
                    Message = "Not Enough Stock"

                });

                _logger.LogInformation($"Not Enough Stock for Buyer Id :{context.Message.BuyerId}");
            }
        }
    }
}
