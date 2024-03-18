using ACS.Shared.Models;
using Serilog;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ACS.Shared.Services
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

        /// <summary>
        /// For each target/fragment record, test whether it matches the specified client context data in the request params.
        /// If true, include it in the fragment map sent to the client.
        /// </summary>
        /// <returns>Map of each fragment ID and value that match the client context</returns>
        public ConfigQueryResponse GetMatchingFragments(List<CacheEntry>? entries, ConfigQueryRequestParams requestParams)
        {
            Dictionary<string, Fragment> fragments = [];

            if (entries != null)
            {
                // Group by fragment name, taking the first matching unique fragment (by name) by highest priority.
                // Fragments with no value are excluded from the final result set.
                fragments = entries
                    .Where(entry => IsMatch(entry.Target, requestParams))
                    .GroupBy(entry => entry.Fragment.Name, entry => entry, (fragmentName, entries) => entries.OrderByDescending(entry => entry.Fragment.Priority).First())
                    .Where(entry => !string.IsNullOrEmpty(entry.Fragment.Value))
                    .ToDictionary(entry => entry.Fragment.Name, entry => entry.Fragment);

                Log
                    .ForContext("RequestParams", requestParams)
                    .ForContext("Fragments", JsonSerializer.Serialize(fragments.Values.Select(fragment => new
                    {
                        fragment.Id,
                        fragment.Name,
                        fragment.Priority,
                        fragment.Description
                    })))
                    .Information("Returned {FragmentCount} fragments to client", fragments.Count);
            }

            return new ConfigQueryResponse
            {
                Fragments = fragments.ToDictionary(
                    fragment => fragment.Key,
                    fragment => fragment.Value.Value)
            };
        }

        public class InvalidVersionException(string message) : Exception(message)
        { }
    }
}
