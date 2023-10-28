using System.Collections.Concurrent;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using StackExchange.Redis;

namespace CommonService.Services;

public class CacheService : ICacheService
{
    private readonly IDatabase _cacheDb;
    private readonly IConnectionMultiplexer _connection;
    private readonly DateTimeOffset _defaultExpireTime;

    public CacheService(IConnectionMultiplexer connectionMultiplexer, DateTimeOffset defaultExpireTime)
    {
        _connection = connectionMultiplexer;
        _cacheDb = connectionMultiplexer.GetDatabase();
        _defaultExpireTime = defaultExpireTime;
    }

    private TimeSpan ExpirationTime(DateTimeOffset? expireTime)
    {
        var expiryTime = expireTime is not null
            ? expireTime.Value.DateTime.Subtract(DateTime.UtcNow)
            : _defaultExpireTime.DateTime.Subtract(DateTime.UtcNow);
        return expiryTime;
    }

    private byte[] CompressData(string data)
    {
        using var outputStream = new MemoryStream();
        using var gzipStream = new GZipStream(outputStream, CompressionMode.Compress);
        var jsonBytes = Encoding.UTF8.GetBytes(data);
        gzipStream.Write(jsonBytes, 0, jsonBytes.Length);
        return outputStream.ToArray();
    }

    private string DecompressData(byte[] compressedData)
    {
        using var inputStream = new MemoryStream(compressedData);
        using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
        using var streamReader = new StreamReader(gzipStream, Encoding.UTF8);
        return streamReader.ReadToEnd();
    }


    //----------------------------------
    public async ValueTask<T?> GetData<T>(string key, CancellationToken token = default)
    {
        var value = await _cacheDb.StringGetAsync(key);
        return string.IsNullOrEmpty(value) ? default : JsonSerializer.Deserialize<T>(value!);


        // var compressedData = await _cacheDb.StringGetAsync(key);
        // if (!compressedData.HasValue) return default;
        // var decompressedData = DecompressData(compressedData!);
        // var deserializedData = JsonSerializer.Deserialize<T>(decompressedData);
        // return deserializedData;
    }

    //----------------------------------
    public async ValueTask<bool> SetData<T>(string key, T value, CancellationToken token = default,
        DateTimeOffset? expireTime = default)
    {
        var expiryTime = ExpirationTime(expireTime);
        return await _cacheDb.StringSetAsync(key, JsonSerializer.Serialize<T>(value), expiryTime);

        // var expiryTime = ExpirationTime(expireTime);
        // var serializedData = JsonSerializer.Serialize<T>(value);
        // var compressedData = CompressData(serializedData);
        // return await _cacheDb.StringSetAsync(key, compressedData, expiryTime);
    }

    //----------------------------------
    public async ValueTask<bool> RemoveData(string key, CancellationToken token = default)
    {
        var isExists = _cacheDb.KeyExists(key);
        return isExists && await _cacheDb.KeyDeleteAsync(key);
    }

    public ValueTask<bool> IsExist(string key, CancellationToken token = default)
    {
        var isExists = _cacheDb.KeyExists(key);
        return ValueTask.FromResult(isExists);
    }

    public ValueTask<bool> IsExistAnyKey(string keyPattern, CancellationToken token = default)
    {
        var endpoint = _connection.GetEndPoints()[0];
        var server = _connection.GetServer(endpoint);
        var isExists = server.Keys(pattern: keyPattern).Any();
        return ValueTask.FromResult(isExists);
    }

    //----------------------------------
    public async ValueTask<ConcurrentDictionary<string, T>> GetKeysAndValuesByPattern<T>(string keyPattern,
        CancellationToken token = default)
    {
        var result = new ConcurrentDictionary<string, T>();
        var endpoints = _connection.GetEndPoints();

        await Parallel.ForEachAsync(endpoints, token, async (endpoint, _) =>
        {
            var server = _connection.GetServer(endpoint);
            foreach (var dbIndex in Enumerable.Range(0, 16))
            {
                var redisKeys = server.Keys(database: dbIndex, pattern: keyPattern).ToArray();
                if (!redisKeys.Any()) continue;
                foreach (var rkey in redisKeys)
                {
                    var redisValue = await _cacheDb.StringGetAsync(rkey);
                    var stringRedisValue = redisValue.ToString();
                    var deserializeRedisValue = JsonSerializer.Deserialize<T>(stringRedisValue!);
                    result.TryAdd<string, T>(rkey!, deserializeRedisValue!);
                }
            }
        });

        return await ValueTask.FromResult(result);
    }

    //----------------------------------
    public async IAsyncEnumerable<(string, T)> GetKeysAndValuesByPattern2<T>(string keyPattern,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        var endpoints = _connection.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = _connection.GetServer(endpoint);
            foreach (var dbIndex in Enumerable.Range(0, 16))
            {
                var redisKeys = server.Keys(database: dbIndex, pattern: keyPattern).ToArray();
                if (!redisKeys.Any()) continue;
                foreach (var rkey in redisKeys)
                {
                    var redisValue = await _cacheDb.StringGetAsync(rkey);
                    var stringRedisValue = redisValue.ToString();
                    var deserializeRedisValue = JsonSerializer.Deserialize<T>(stringRedisValue!);
                    yield return (rkey.ToString(), deserializeRedisValue!);
                }
            }
        }
    }

    //---------------------------------- using version field to handle optimistic concurrency
    public async ValueTask<bool> UpdateField<T, TU>(string redisKey, string fieldName, TU newValue,
        CancellationToken token = default)
    {
        var serializedObject = await _cacheDb.StringGetAsync(redisKey);
        if (serializedObject.IsNullOrEmpty)
        {
            return false;
        }

        var deserializedObject = JsonSerializer.Deserialize<T>(serializedObject);
        var property = typeof(T).GetProperty(fieldName);
        property?.SetValue(deserializedObject, newValue);
        var updatedSerializedObject = JsonSerializer.Serialize(deserializedObject);
        return await _cacheDb.StringSetAsync(redisKey, updatedSerializedObject);
    }

    public async ValueTask<bool> UpdateFields<T>(string redisKey, Dictionary<string, object?> fieldsToUpdate,
        CancellationToken token = default)
    {
        var serializedObject = await _cacheDb.StringGetAsync(redisKey);
        var deserializedObject = JsonSerializer.Deserialize<T>(serializedObject);
        if (serializedObject.IsNullOrEmpty)
        {
            return false;
        }

        foreach (var (fieldName, newValue) in fieldsToUpdate)
        {
            var property = typeof(T).GetProperty(fieldName);
            property?.SetValue(deserializedObject, newValue);
        }

        var updatedSerializedObject = JsonSerializer.Serialize(deserializedObject);
        return await _cacheDb.StringSetAsync(redisKey, updatedSerializedObject);
    }


    // public async ValueTask<bool> UpdateField<T, TU>(string redisKey, string fieldName, TU newValue,
    //     CancellationToken token = default)
    // {
    //     try
    //     {
    //         var transaction = _cacheDb.CreateTransaction();
    //         var redisValue =await  transaction.StringGetAsync(redisKey);
    //         if (redisValue.IsNullOrEmpty)
    //         {
    //             return false;
    //         }
    //
    //         var updatedObject = JsonSerializer.Deserialize<T>(redisValue!);
    //         var field = updatedObject!.GetType().GetProperty(fieldName);
    //
    //         if (field == null)
    //         {
    //             return false;
    //         }
    //
    //         field.SetValue(updatedObject, newValue);
    //
    //         var versionField = updatedObject.GetType().GetProperty("Version");
    //         var currentVersion = versionField!.GetValue(updatedObject) as int?;
    //         var newVersion = currentVersion.GetValueOrDefault() + 1;
    //
    //         versionField.SetValue(updatedObject, newVersion);
    //
    //         var serializedValue = JsonSerializer.Serialize(updatedObject);
    //
    //         transaction.AddCondition(Condition.StringEqual(redisKey, redisValue));
    //
    //         await transaction.StringSetAsync(redisKey, serializedValue);
    //
    //         return await transaction.ExecuteAsync();
    //     }
    //     catch (Exception e)
    //     {
    //         return await Task.FromResult(false);
    //     }
    // }

//     public async ValueTask<bool> UpdateFields<T, TU>(string redisKey, Dictionary<string, TU> fieldsToUpdate,
//         CancellationToken token = default)
//     {
//         var transaction = _cacheDb.CreateTransaction();
//         var redisValue = await transaction.StringGetAsync(redisKey);
//
//         if (redisValue.IsNull)
//         {
//             return false;
//         }
//
//         var objectToUpdate = JsonSerializer.Deserialize<T>(redisValue!);
//
//         foreach (var (fieldName, newValue) in fieldsToUpdate)
//         {
//             var field = objectToUpdate!.GetType().GetProperty(fieldName);
//             field?.SetValue(objectToUpdate, newValue);
//         }
//
//         var versionField = objectToUpdate!.GetType().GetProperty("Version");
//         var currentVersion = versionField!.GetValue(objectToUpdate) as int?;
//         var newVersion = currentVersion.GetValueOrDefault() + 1;
//
//         versionField.SetValue(objectToUpdate, newVersion);
//
//         var serializedValue = JsonSerializer.Serialize(objectToUpdate);
//
//         transaction.AddCondition(Condition.StringEqual(redisKey, redisValue));
//         await transaction.StringSetAsync(redisKey, serializedValue);
//
//         return await transaction.ExecuteAsync();
//     }
}