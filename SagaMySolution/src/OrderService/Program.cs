
using CommonService;
using Microsoft.EntityFrameworkCore;
using OrderService;
using OrderService.Data;
using OrderService.Jobs;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContextPool<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));

builder.Services.RegisterCommonServices(builder.Configuration);

builder.Services.AddHostedService<OutBoxOrdersListener>();
// builder.Services.AddHostedService<NewOrdersListener>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapOrderRoutes();

app.Run();