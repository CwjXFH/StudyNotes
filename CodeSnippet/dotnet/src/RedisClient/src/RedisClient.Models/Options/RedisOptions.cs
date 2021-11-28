namespace RedisClient.Models.Options
{
    public class RedisOptions
    {
        public string Host { set; get; } = "localhost";

        public int Port { set; get; } = 6379;

        public string Password { set; get; } = "";

        public int DbIndex { set; get; } = 0;
    }
}
