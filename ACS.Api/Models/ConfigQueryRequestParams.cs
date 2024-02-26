using System.Text.Json;

namespace ACS.Api.Models
{
    public class ConfigQueryRequestParams
    {
        public required string AgentName { get; set; }

        public required string AgentVersion { get; set; }

        public required string UserName { get; set; }

        public required string HostName { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
