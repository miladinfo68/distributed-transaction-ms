using CommonService;
using PaymentService.Jobs;
using PaymentService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.RegisterCommonServices(builder.Configuration);
builder.Services.Configure<MongodbConfigs>(builder.Configuration.GetSection("MongodbConfigs"));

builder.Services.AddScoped<MongodbService>();
builder.Services.AddScoped<IPaymentService, PaymentService.Services.PaymentService>();

builder.Services.AddHostedService<PaymentCheckerListener>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
