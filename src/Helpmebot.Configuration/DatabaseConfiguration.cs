namespace Helpmebot.Configuration
{
    using System;

    public class DatabaseConfiguration
    {
        public DatabaseConfiguration(string hostname, string username, string password, string schema)
        {
            if (hostname == null)
            {
                throw new ArgumentNullException("hostname");
            }

            if (username == null)
            {
                throw new ArgumentNullException("username");
            }

            if (password == null)
            {
                throw new ArgumentNullException("password");
            }

            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }

            this.Hostname = hostname;
            this.Username = username;
            this.Password = password;
            this.Schema = schema;

            this.Port = 3306;
            this.CharSet = "utf8mb4";
        }

        public string Hostname { get; private set; }
        public int Port { get; set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Schema { get; private set; }
        public string CharSet { get; private set; }
    }
}