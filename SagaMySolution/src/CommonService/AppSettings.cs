namespace CommonService;

public class AppSettings
{
    public RedisConfigs RedisConfigs { get; set; }
    public MongodbConfigs MongodbConfigs { get; set; }
}

public class RedisConfigs
{
    public bool UseCache { get; set; }
    public string Server { get; set; }
    public string CacheKeyPendingFormat { get; set; }
    public string CacheKeyCompleteFormat { get; set; }
    public string CacheKeyPaymentFormat { get; set; }
    public string CacheKeyFailFormat { get; set; }
    public int CacheExpireTimeHour { get; set; }
}

public class MongodbConfigs
{
    public string Server { get; set; }
    public string DbName { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public string User { get; set; }
    public string Password { get; set; }

    public string ConnectionString
    {
        get
        {
            if (string.IsNullOrEmpty(User) || string.IsNullOrEmpty(Password))
                return $@"mongodb://{Host}:{Port}";
            return $@"mongodb://{User}:{Password}@{Host}:{Port}";
        }
    }
}