using ACS.Shared.Providers;
using System.ComponentModel;
using System.Text.Json;

namespace ACS.Shared.Models
{
    public class ConfigQueryRequestParams
    {
        [DisplayName("Agent Name")]
        [HelpText("Well-known agent name.")]
        public required string AgentName { get; set; }

        [DisplayName("Agent Version")]
        [HelpText("Version of the deployed agent.")]
        public required string AgentVersion { get; set; }

        [DisplayName("Context")]
        [HelpText("Include fragments in this context.")]
        public string? Context { get; set; }

        [DisplayName("User Name")]
        [HelpText("User account the agent is running under.")]
        public string? UserName { get; set; }

        [DisplayName("Active Users")]
        [HelpText("Comma-separated list of active users on the host.")]
        public IEnumerable<string>? ActiveUsers { get; set; }

        [DisplayName("Host Name")]
        [HelpText("Fully-qualified host name.")]
        public string? HostName { get; set; }

        [DisplayName("Host Roles")]
        [HelpText("Comma-separated list of host names.")]
        public IEnumerable<string>? HostRoles { get; set; }

        [DisplayName("Environment Name")]
        [HelpText("Name of the environment (e.g. Production).")]
        public string? EnvironmentName { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
