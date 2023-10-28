using CommonService;
using CommonService.Entities;
using Microsoft.EntityFrameworkCore;
using UserService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContextPool<CustomerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));

builder.Services.RegisterCommonServices(builder.Configuration);

var app = builder.Build();

app.MapGet("/api/Customers", async (CustomerDbContext db) => await db.Customers.ToListAsync());

app.MapGet("/api/Customers/{id:int}", async (CustomerDbContext db, int id) =>
    await db.Customers.FindAsync(id) is not { } c ? Results.NotFound() : Results.Ok(c));

app.MapPost("/api/Customers", async (CustomerDbContext db, decimal balance) =>
{
    await db.Customers.AddAsync(new Customer() { Balance = balance });
    await db.SaveChangesAsync();
    return Results.Ok(true);
});

app.MapPut("/api/Customers/{id:int}/{balance:decimal}",
    async (CustomerDbContext db, int id, decimal balance) =>
    {
        if (await db.Customers.FindAsync(id) is not { } c)
            return Results.NotFound();

        c.Balance = balance;
        await db.SaveChangesAsync();
        return Results.Ok(true);
    });
//--------------------------------------

app.MapGet("/api/Customer/Messages", async (CustomerDbContext db) => 
    await db.MessageDelivery.ToListAsync());

app.MapGet("/api/Customer/Messages/{id:int}", async (CustomerDbContext db, int id) =>
    await db.MessageDelivery.FindAsync(id) is not { } m ? Results.NotFound() : Results.Ok(m));

// app.MapPost("/api/Customer/Messages", async (UserDbContext db, decimal balance) =>
// {
//     await db.Customers.AddAsync(new Customer() { Balance = balance });
//     await db.SaveChangesAsync();
//     return Results.Ok(true);
// });
//
// app.MapPut("/api/Customers/{id:int}/{balance:decimal}",
//     async (UserDbContext db, int id, decimal balance) =>
//     {
//         if (await db.Customers.FindAsync(id) is not { } c)
//             return Results.NotFound();
//
//         c.Balance = balance;
//         await db.SaveChangesAsync();
//         return Results.Ok(true);
//     });



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();