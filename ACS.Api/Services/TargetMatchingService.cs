using ACS.Api.Models;
using ACS.Shared.Models;
using System.Text.RegularExpressions;

namespace ACS.Api.Services
{
    public class TargetMatchingService : ITargetMatchingService
    {
        public bool IsMatch(Target target, ConfigQueryRequestParams requestParams)
        {
            if (!Version.TryParse(requestParams.AgentVersion, out Version? agentVersion) || agentVersion == null)
            {
                throw new InvalidVersionException("Invalid version: " + requestParams.AgentVersion);
            }

            // Agent version is less than the minimum required version
            if (target.AgentMinVersion != null && Version.TryParse(target.AgentMinVersion, out Version? agentMinVersion) && agentMinVersion != null &&
                agentVersion < agentMinVersion)
            {
                return false;
            }

            // Agent version is greater than the maximum allowed version
            if (target.AgentMaxVersion != null && Version.TryParse(target.AgentMaxVersion, out Version? agentMaxVersion) && agentMaxVersion != null &&
                agentVersion > agentMaxVersion)
            {
                return false;
            }

            // Username does not match the pattern
            if (!string.IsNullOrEmpty(target.UserNamePattern) && !Regex.IsMatch(requestParams.UserName, target.UserNamePattern))
            {
                return false;
            }

            // Hostname does not match the pattern
            if (!string.IsNullOrEmpty(target.HostNamePattern) && !Regex.IsMatch(requestParams.HostName, target.HostNamePattern))
            {
                return false;
            }

            // Environment name does not match the pattern
            if (!string.IsNullOrEmpty(target.EnvironmentNamePattern) && !Regex.IsMatch(requestParams.EnvironmentName, target.EnvironmentNamePattern))
            {
                return false;
            }

            return true;
        }

        public class InvalidVersionException(string message) : Exception(message)
        { }
    }
}
