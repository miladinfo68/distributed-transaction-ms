using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StockService.Models;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement(Order = 0)]
    public ObjectId Id { get; set; }

    [BsonRepresentation(BsonType.Int64)]
    [BsonElement(Order = 1)]
    // public Product ProductId { get; set; }
    public int ProductId { get; set; }
        
    [BsonRepresentation(BsonType.Int64)]
    [BsonElement(Order = 2)]
    public int Count { get; set; }
        
    [BsonRepresentation(BsonType.DateTime)]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonElement(Order = 3)]
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}