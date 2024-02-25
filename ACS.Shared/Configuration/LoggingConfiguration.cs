namespace ACS.Shared.Configuration
{
    public class LoggingConfiguration
    {
        public required ConsoleLoggingConfiguration Console { get; set; }

        public required HttpLoggingConfiguration Http { get; set; }
    }
}
