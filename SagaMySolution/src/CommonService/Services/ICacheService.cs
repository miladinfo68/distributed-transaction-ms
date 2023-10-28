using System.Collections.Concurrent;

namespace CommonService.Services;

public interface ICacheService
{
    
    ValueTask<T?> GetData<T>(string key, CancellationToken token = default);

    ValueTask<bool> SetData<T>(string key, T value, CancellationToken token = default,
        DateTimeOffset? expireTime = default);

    ValueTask<bool> RemoveData(string key,CancellationToken token=default);
    ValueTask<bool> IsExist(string key,CancellationToken token=default);
    ValueTask<bool> IsExistAnyKey(string keyPattern,CancellationToken token=default);

    ValueTask<ConcurrentDictionary<string, T>> GetKeysAndValuesByPattern<T>(string keyPattern,
        CancellationToken token = default);

    ValueTask<bool> UpdateField<T, TU>(string redisKey, string fieldName, TU newValue,
        CancellationToken token = default);

    ValueTask<bool> UpdateFields<T>(string redisKey, Dictionary<string, object?> fieldsToUpdate,
        CancellationToken token = default);

    IAsyncEnumerable<(string, T)> GetKeysAndValuesByPattern2<T>(string keyPattern,
        CancellationToken token = default);
}