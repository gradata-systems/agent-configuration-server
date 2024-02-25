using Serilog.Events;

namespace AgentConfigurationServer.Configuration
{
    public class HttpLoggingConfiguration
    {
        public bool Enabled { get; set; }

        public required string Url { get; set; }

        public LogEventLevel? MinimumLogLevel { get; set; }

        public IDictionary<string, string> Headers { get; set; }

        public int? PeriodSeconds { get; set; }
        public TimeSpan? Period => PeriodSeconds.HasValue ? TimeSpan.FromSeconds(PeriodSeconds.Value) : null;

        public int? BatchLimit { get; set; }

        public PersistenceConfiguration Persistence { get; set; }

        public class PersistenceConfiguration
        {
            public bool Enabled { get; set; }
            public string Directory { get; set; }
        }
    }
}
