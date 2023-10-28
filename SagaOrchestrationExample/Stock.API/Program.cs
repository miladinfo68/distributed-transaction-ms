using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stock.API.Services;
using MongoDB.Driver;

namespace Stock.API
{
    public abstract class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using var scope = host.Services.CreateScope();
            var mongodbService = scope.ServiceProvider.GetRequiredService<MongodbService>();

            var productCounts = await mongodbService.GetCollection<Models.Product>()
                .CountDocumentsAsync(FilterDefinition<Models.Product>.Empty);

            //if (productCount == 0)
            if (!await mongodbService.GetCollection<Models.Product>().FindSync(x => true).AnyAsync())
            {
                await mongodbService.GetCollection<Stock.API.Models.Product>().InsertOneAsync(new Models.Product
                {
                    ProductId = 21,
                    Count = 200
                });
                await mongodbService.GetCollection<Stock.API.Models.Product>().InsertOneAsync(new Models.Product
                {
                    ProductId = 22,
                    Count = 100
                });
                await mongodbService.GetCollection<Stock.API.Models.Product>().InsertOneAsync(new Models.Product
                {
                    ProductId = 23,
                    Count = 50
                });
                await mongodbService.GetCollection<Stock.API.Models.Product>().InsertOneAsync(new Models.Product
                {
                    ProductId = 24,
                    Count = 10
                });
                await mongodbService.GetCollection<Stock.API.Models.Product>().InsertOneAsync(new Models.Product
                {
                    ProductId = 25,
                    Count = 30
                });
            }

            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}