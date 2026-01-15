//using StackExchange.Redis;
//using System.Text.Json;
//using TODO_List.Application.Interfaces;

//public class RedisService : IRedisService
//{
//    private readonly IDatabase _db;
//    private readonly JsonSerializerOptions _jsonOptions;
//    public RedisService(IConnectionMultiplexer connection)
//    {
//        _db = connection.GetDatabase();
//        _jsonOptions = new JsonSerializerOptions
//        {
//            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//            WriteIndented = false
//        };
//    }
//    public async Task SetAsync<T>(string key, T value, Expiration expiry)
//    {
//        try
//        {
//            var json = JsonSerializer.Serialize(value, _jsonOptions);
//            await _db.StringSetAsync(key, json, expiry);
//        }
//        catch (Exception ex)
//        { 
//            throw new Exception($"Redis SetAsync failed for key: {key}", ex);
//        }
//    }
//    public async Task<T?> GetAsync<T>(string key)
//    {
//        try
//        {
//            var json = await _db.StringGetAsync(key);
//            if (json.IsNullOrEmpty)
//                return default;
//            return JsonSerializer.Deserialize<T>(json!, _jsonOptions);
//        }
//        catch (Exception ex)
//        {
//            throw new Exception($"Redis GetAsync failed for key: {key}", ex);
//        }
//    }
//    public async Task DeleteAsync(string key)
//    {
//        try
//        {
//            await _db.KeyDeleteAsync(key);
//        }
//        catch (Exception ex)
//        {
//            throw new Exception($"Redis DeleteAsync failed for key: {key}", ex);
//        }
//    }
//}
