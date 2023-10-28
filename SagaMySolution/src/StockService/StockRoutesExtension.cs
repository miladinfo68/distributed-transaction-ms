using MongoDB.Driver;
using StockService.Models;
using StockService.Services;
using StockService.ViewModels;

namespace StockService;

public static class StockRoutesExtension
{
    public static WebApplication MapStockRoutes(this WebApplication app)
    {
        var stockGroup = app.MapGroup("/api/stocks");

        stockGroup.MapGet("", async (IProductService productService) => await productService.ProductListAsync());
        
        stockGroup.MapGet("/{productId:int}", async (IProductService productService, int productId) =>
            await productService.GetProductByIdAsync(productId));

        stockGroup.MapPost("", async (IProductService  productService, ProductVm model) =>
        {
            await productService.AddProductAsync(model);
            return Results.Ok(true);
        });
        
        //g.MapPut("/{productId}",)

        return app;
    }


}

public static class StockDbExtensionSeedData
{
    public static async Task<IApplicationBuilder> AddSeedData(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app, nameof(app));

        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var mongodbService = services.GetRequiredService<MongodbService>();

            //var productCounts = await mongodbService.GetCollection<Product>()
            //    .CountDocumentsAsync(FilterDefinition<Product>.Empty);

            //if (productCount == 0)
            if (!await mongodbService.GetCollection<Product>().FindSync(x => true).AnyAsync())
            {
                await mongodbService.GetCollection<Product>().InsertOneAsync(new Product
                {
                    ProductId = 21,
                    Count = 200
                });
                await mongodbService.GetCollection<Product>().InsertOneAsync(new Product
                {
                    ProductId = 22,
                    Count = 100
                });
                await mongodbService.GetCollection<Product>().InsertOneAsync(new Product
                {
                    ProductId = 23,
                    Count = 50
                });
                await mongodbService.GetCollection<Product>().InsertOneAsync(new Product
                {
                    ProductId = 24,
                    Count = 10
                });
                await mongodbService.GetCollection<Product>().InsertOneAsync(new Product
                {
                    ProductId = 25,
                    Count = 30
                });
            }
        }
        catch
        {
            // ignored
        }

        return app;
    }
}


//https://medium.com/@etiennerouzeaud/play-databases-with-adminer-and-docker-53dc7789f35f