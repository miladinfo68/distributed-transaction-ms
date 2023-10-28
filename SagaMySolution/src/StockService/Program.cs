using CommonService;
using StockService;
using StockService.Jobs;
using StockService.Services;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.RegisterCommonServices(builder.Configuration);
builder.Services.Configure<MongodbConfigs>(builder.Configuration.GetSection("MongodbConfigs"));

builder.Services.AddScoped<MongodbService>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddHostedService<ProductCheckerListener>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapStockRoutes();


await app.AddSeedData();

app.Run();

