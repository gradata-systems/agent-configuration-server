namespace AgentConfigurationServer.Configuration
{
    public class LoggingConfiguration
    {
        public ConsoleLoggingConfiguration Console { get; set; }
        public HttpLoggingConfiguration Http { get; set; }
    }
}
