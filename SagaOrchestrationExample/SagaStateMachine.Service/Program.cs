using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SagaStateMachine.Service.Instruments;
using SagaStateMachine.Service.StateMachines;
using Shared;

namespace SagaStateMachine.Service
{
    public abstract class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMassTransit(configure =>
                    {
                        configure.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>()
                          .EntityFrameworkRepository(options =>
                          {
                              options.AddDbContext<DbContext, OrderStateDbContext>((provider, builder) =>
                              {
                                  builder.UseSqlServer(hostContext.Configuration.GetConnectionString("SQLServer"));
                              });
                          });

                        configure.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                        {
                            cfg.Host(hostContext.Configuration.GetConnectionString("RabbitMQ"));

                            cfg.ReceiveEndpoint(RabbitMQSettings.Order_Orchestrator_Queue, e =>
                            e.ConfigureSaga<OrderStateInstance>(provider));
                        }));
                    });

                    services.AddMassTransitHostedService();

                });
    }
}