using CommonService.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PaymentService.Models;

public class Payment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement(Order = 0)]
    public ObjectId Id { get; set; }
    
    [BsonRepresentation(BsonType.Int64)]
    [BsonElement(Order = 1)]
    public int BuyerId { get; set; }

    [BsonRepresentation(BsonType.Int64)]
    [BsonElement(Order = 2)]
    public int OrderId { get; set; }
        
    [BsonRepresentation(BsonType.Int32)]
    [BsonElement(Order = 3)]
    public OrderStatus OrderStatus { get; set; }
        
    [BsonRepresentation(BsonType.DateTime)]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonElement(Order = 4)]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}