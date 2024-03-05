using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachineWorkerService;
using SagaStateMachineWorkerService.Models;
using Shared;
using System.Reflection;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddMassTransit(async cfg =>
{
    cfg.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>().EntityFrameworkRepository(opt =>
    {
        opt.AddDbContext<DbContext, OrderStateDbContext>((provider, options) =>
        {

            options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"), m =>
            {
                m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
            });



        });
    });

    cfg.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(configure =>
    {
        configure.Host(builder.Configuration.GetConnectionString("RabbitMQ"));



        configure.ReceiveEndpoint(RabbitMQSettingsConst.OrderSaga, e =>
        {
            e.ConfigureSaga<OrderStateInstance>(provider);
        });
    }));


});








var host = builder.Build();
host.Run();
