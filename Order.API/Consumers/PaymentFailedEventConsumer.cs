﻿using MassTransit;
using Order.API.Models;
using Shared;

namespace Order.API.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedlEvent>
    {

        private readonly AppDbContext _context;

        private readonly ILogger<PaymentCompletedEventConsumer> _logger;

        public PaymentFailedEventConsumer(AppDbContext context, ILogger<PaymentCompletedEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentFailedlEvent> context)
        {
            var order = await _context.Orders.FindAsync(context.Message.OrderId);

            if (order != null)
            {
                order.Status = OrderStatus.Fail;
                order.FailMessage = context.Message.Message;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Order (Id = {context.Message.OrderId}) status changed : {order.Status}");
            }
            else
            {
                _logger.LogError($"Order (Id = {context.Message.OrderId} not found");
            }
        }
    }
}
