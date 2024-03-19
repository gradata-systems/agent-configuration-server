using System.Text.RegularExpressions;

namespace ACS.Shared.Models
{
    public class CompiledTarget
    {
        public readonly string AgentName;
        public readonly Version? AgentMinVersion;
        public readonly Version? AgentMaxVersion;
        public readonly Regex? UserName;
        public readonly Regex? ActiveUserName;
        public readonly Regex? HostName;
        public readonly Regex? HostRole;
        public readonly Regex? EnvironmentName;

        public CompiledTarget(Target target)
        {
            AgentName = target.AgentName;

            if (!string.IsNullOrEmpty(target.AgentMinVersion))
            {
                Version.TryParse(target.AgentMinVersion, out AgentMinVersion);
            }

            if (!string.IsNullOrEmpty(target.AgentMaxVersion))
            {
                Version.TryParse(target.AgentMaxVersion, out AgentMaxVersion);
            }

            if (!string.IsNullOrEmpty(target.UserNamePattern))
            {
                UserName = new Regex(target.UserNamePattern, RegexOptions.Compiled);
            }

            if (!string.IsNullOrEmpty(target.ActiveUserNamePattern))
            {
                ActiveUserName = new Regex(target.ActiveUserNamePattern, RegexOptions.Compiled);
            }

            if (!string.IsNullOrEmpty(target.HostNamePattern))
            {
                HostName = new Regex(target.HostNamePattern, RegexOptions.Compiled);
            }

            if (!string.IsNullOrEmpty(target.HostRolePattern))
            {
                HostRole = new Regex(target.HostRolePattern, RegexOptions.Compiled);
            }

            if (!string.IsNullOrEmpty(target.EnvironmentNamePattern))
            {
                EnvironmentName = new Regex(target.EnvironmentNamePattern, RegexOptions.Compiled);
            }
        }
    }
}
