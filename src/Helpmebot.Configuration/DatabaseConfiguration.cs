// ReSharper disable UnusedAutoPropertyAccessor.Global - used by YAML deserialize
namespace Helpmebot.Configuration
{
    public class DatabaseConfiguration
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Schema { get; set; }
        public string CharSet { get; set; }
    }
}