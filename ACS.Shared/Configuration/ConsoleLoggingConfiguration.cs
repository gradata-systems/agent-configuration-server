using Serilog.Events;

namespace ACS.Shared.Configuration
{
    public class ConsoleLoggingConfiguration
    {
        public bool Enabled { get; set; }

        public LogEventLevel? MinimumLogLevel { get; set; }
    }
}
