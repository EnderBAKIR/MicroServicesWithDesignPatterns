﻿using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.DTOs;
using Order.API.Models;
using Shared;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrdersController(AppDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto orderCreate)
        {

            var newOrder = new Models.Order
            {
                BuyerId = orderCreate.BuyerId,
                Status = OrderStatus.Suspend,
                Address = new Address { Line = orderCreate.Address.Line, Province = orderCreate.Address.Province, District = orderCreate.Address.District },
                CreatedDate = DateTime.Now

            };


            orderCreate.orderItems.ForEach(x =>
            {
                newOrder.Items.Add(new OrderItem()
                {
                    Price = x.Price,
                    ProductId = x.ProductId,
                    Count = x.Count,
                });
            });


            await _context.AddAsync(newOrder);

            await _context.SaveChangesAsync();

            var orderCreatedEvent = new OrderCreatedEvent()
            {
                BuyerId = orderCreate.BuyerId,
                OrderId = newOrder.Id,
                Payment = new PaymentMessage
                {
                    CardName = orderCreate.payment.CardName,
                    CardNumber = orderCreate.payment.CardNumber,
                    Expiration = orderCreate.payment.Expiration,
                    CVV = orderCreate.payment.CVV,
                    TotalPrice = orderCreate.orderItems.Sum(x => x.Price * x.Count)



                }

            };


            orderCreate.orderItems.ForEach(item =>
            {

                orderCreatedEvent.OrderItems.Add(new OrderItemMessage {Count = item.Count , ProductId = item.ProductId});
            });




            await _publishEndpoint.Publish(orderCreatedEvent);




            return Ok();


        }



    }
}
