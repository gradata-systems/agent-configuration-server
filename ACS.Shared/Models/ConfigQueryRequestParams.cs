using System.ComponentModel;
using System.Text.Json;

namespace ACS.Shared.Models
{
    public class ConfigQueryRequestParams
    {
        [DisplayName("Agent Name")]
        public required string AgentName { get; set; }

        [DisplayName("Agent Version")]
        public required string AgentVersion { get; set; }

        [DisplayName("User Name")]
        public string? UserName { get; set; }

        [DisplayName("Active Users (comma separated)")]
        public IEnumerable<string>? ActiveUsers { get; set; }

        [DisplayName("Host Name (FQDN)")]
        public string? HostName { get; set; }

        [DisplayName("Host Roles (comma separated)")]
        public IEnumerable<string>? HostRoles { get; set; }

        [DisplayName("Environment Name")]
        public string? EnvironmentName { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
