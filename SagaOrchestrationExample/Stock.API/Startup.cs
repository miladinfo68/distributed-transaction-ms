using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Shared;
using Stock.API.Consumers;
using Stock.API.Services;

namespace Stock.API
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {

            //bind configuration to MongodbConfigs
            services.Configure<MongodbConfigs>(Configuration.GetSection("MongodbConfigs"));

            services.AddScoped<IProductService, ProductService>();
            
            services.AddMassTransit(configure =>
            {
                configure.AddConsumer<OrderCreatedEventConsumer>();
                configure.AddConsumer<StockRollbackMessageConsumer>();

                configure.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(Configuration.GetConnectionString("RabbitMQ"));
                    configurator.ReceiveEndpoint(RabbitMQSettings.Check_Order_Items_Stock_Queue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
                    configurator.ReceiveEndpoint(RabbitMQSettings.Rollback_Order_Items_Stock_Queue, e => e.ConfigureConsumer<StockRollbackMessageConsumer>(context));
                });
            });

            services.AddMassTransitHostedService();

            services.AddSingleton<MongodbService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Stock.API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Stock.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
