using StackExchange.Redis;

namespace SystemGame.Api.Services;

public class RedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task<string?> GetStringAsync(string key)
    {
        return await _database.StringGetAsync(key);
    }

    public async Task SetStringAsync(string key, string value, TimeSpan? expiry = null)
    {
        await _database.StringSetAsync(key, value, expiry);
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }

    public async Task<bool> KeyDeleteAsync(string key)
    {
        return await _database.KeyDeleteAsync(key);
    }

    public ISubscriber GetSubscriber()
    {
        return _redis.GetSubscriber();
    }
}

