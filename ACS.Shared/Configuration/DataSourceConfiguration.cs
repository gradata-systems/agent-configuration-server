using MySql.Data.MySqlClient;

namespace ACS.Shared.Configuration
{
    public class DataSourceConfiguration
    {
        public required string Server { get; set; }

        public uint Port { get; set; }

        public required string User { get; set; }

        public required string Password { get; set; }

        public required string Database { get; set; }

        public string GetConnectionString()
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder()
            {
                Server = Server,
                Port = Port,
                UserID = User,
                Password = Password,
                Database = Database
            };

            return builder.ToString();
        }
    }
}
