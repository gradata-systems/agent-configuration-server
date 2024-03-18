using System.ComponentModel;
using System.Text.Json;

namespace ACS.Api.Models
{
    public class ConfigQueryRequestParams
    {
        [DisplayName("Agent Name")]
        public required string AgentName { get; set; }

        [DisplayName("Agent Version")]
        public required string AgentVersion { get; set; }

        [DisplayName("User Name")]
        public required string UserName { get; set; }

        [DisplayName("Host Name (FQDN)")]
        public required string HostName { get; set; }

        [DisplayName("Environment Name")]
        public required string EnvironmentName { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
