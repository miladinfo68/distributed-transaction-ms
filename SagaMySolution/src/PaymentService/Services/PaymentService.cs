using MongoDB.Driver;
using PaymentService.Models;

namespace PaymentService.Services;

public interface IPaymentService
{
    public Task<List<Payment>> PaymentsAsync(IEnumerable<int>? orderIds = null);
    public Task<Payment?> GetPaymentByOrderIdAsync(int orderId);
    public Task AddPaymentAsync(Payment payment);
    public Task UpdatePaymentAsync(int orderId, Payment payment);
    public Task DeletePaymentAsync(int orderId);
}

public class PaymentService : IPaymentService
{
    private readonly IMongoCollection<Payment> _collection;

    public PaymentService(MongodbService mongodbService)
    {
        _collection = mongodbService.GetCollection<Payment>();
    }

    public async Task<List<Payment>> PaymentsAsync(IEnumerable<int>? orderIds = null)
    {
        if (orderIds is null) 
            return await _collection.Find(_ => true).ToListAsync();
        var filter = Builders<Payment>.Filter.In(p => p.OrderId, orderIds!);
        return await _collection.Find(filter).ToListAsync();
    }


    public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
    {
        return await _collection.Find(x => x.OrderId == orderId).FirstOrDefaultAsync();
    }

    public async Task AddPaymentAsync(Payment payment)
    {
        await _collection.InsertOneAsync(payment);
    }

    public async Task UpdatePaymentAsync(int orderId, Payment payment)
    {
        await _collection.ReplaceOneAsync(x => x.OrderId == orderId, payment);
    }

    public async Task DeletePaymentAsync(int orderId)
    {
        await _collection.DeleteOneAsync(x => x.OrderId == orderId);
    }
    
}