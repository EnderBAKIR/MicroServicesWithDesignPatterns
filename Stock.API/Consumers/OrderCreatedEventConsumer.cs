﻿using MassTransit;
using Shared;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {

        public Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            throw new NotImplementedException();
        }
    }
}
