﻿using MassTransit;
using Shared.Interfaces;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<IOrderCreatedEvent>
    {









        public Task Consume(ConsumeContext<IOrderCreatedEvent> context)
        {
            throw new NotImplementedException();
        }
    }
}
