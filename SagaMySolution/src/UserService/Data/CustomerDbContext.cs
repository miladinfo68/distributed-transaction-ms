using CommonService.Entities;
using Microsoft.EntityFrameworkCore;

namespace UserService.Data;

public class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
    {
    }
    public DbSet<Customer> Customers { get; set; }

    public DbSet<MessageDelivery> MessageDelivery { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    base.OnConfiguring(optionsBuilder);
    //    optionsBuilder.UseSqlServer("server=.;database=SagaOrchestrationDB;Trusted_Connection=true;TrustServerCertificate=True");
    //}
}