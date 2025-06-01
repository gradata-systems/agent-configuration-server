using ACS.Shared.Models;
using Serilog;
using System.Text.Json;

namespace ACS.Shared.Services
{
    public class TargetMatchingService : ITargetMatchingService
    {
        public bool IsMatch(CompiledTarget target, ConfigQueryRequestParams requestParams)
        {
            if (!Version.TryParse(requestParams.AgentVersion, out Version? agentVersion) || agentVersion == null)
            {
                throw new InvalidVersionException("Invalid version: " + requestParams.AgentVersion);
            }

            // Agent version is less than the minimum required version
            if (target.AgentMinVersion != null && agentVersion < target.AgentMinVersion)
            {
                return false;
            }

            // Agent version is greater than the maximum allowed version
            if (target.AgentMaxVersion != null && agentVersion > target.AgentMaxVersion)
            {
                return false;
            }

            // Username does not match the pattern
            if (target.UserName != null)
            {
                if (requestParams.UserName == null || !target.UserName.IsMatch(requestParams.UserName))
                {
                    return false;
                }
            }

            // None of the active users on the host match the pattern
            if (target.ActiveUserName != null)
            {
                if (requestParams.ActiveUsers == null || !requestParams.ActiveUsers.Any(target.ActiveUserName.IsMatch))
                {
                    return false;
                }
            }

            // Hostname does not match the pattern
            if (target.HostName != null)
            {
                if (requestParams.HostName == null || !target.HostName.IsMatch(requestParams.HostName))
                {
                    return false;
                }
            }

            // None of the host roles match the pattern
            if (target.HostRole != null)
            {
                if (requestParams.HostRoles == null || !requestParams.HostRoles.Any(target.HostRole.IsMatch))
                {
                    return false;
                }
            }

            // Environment name does not match the pattern
            if (target.EnvironmentName != null)
            {
                if (requestParams.EnvironmentName == null || !target.EnvironmentName.IsMatch(requestParams.EnvironmentName))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// For each target/fragment record, test whether it matches the specified client context data in the request params.
        /// If true, include it in the fragment map sent to the client.
        /// </summary>
        /// <returns>Map of each fragment ID and value that match the client context</returns>
        public ConfigQueryResponse GetMatchingFragments(List<CompiledCacheEntry>? entries, ConfigQueryRequestParams requestParams)
        {
            Dictionary<string, Fragment> fragments = [];

            if (entries != null)
            {
                // Group by fragment name, taking the first matching unique fragment (by name) by highest priority.
                // Fragments with no value are excluded from the final result set.
                // Finally, order by the fragment priority.
                fragments = entries
                    .Where(entry =>
                        IsMatch(entry.Target, requestParams) &&
                        (entry.Fragment.Context == requestParams.Context || string.IsNullOrEmpty(entry.Fragment.Context) && string.IsNullOrEmpty(requestParams.Context))
                    )
                    .GroupBy(entry => entry.Fragment.Name, entry => entry, (fragmentName, entries) => entries.OrderByDescending(entry => entry.Fragment.Priority).First())
                    .Where(entry => !string.IsNullOrEmpty(entry.Fragment.Value))
                    .OrderByDescending(entry => entry.Fragment.Priority)
                    .ThenBy(entry => entry.Fragment.Name)
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
