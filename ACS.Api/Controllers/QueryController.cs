using ACS.Api.Models;
using ACS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ACS.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class QueryController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly ITargetMatchingService _targetMatchingService;

        public QueryController(ICacheService cacheService, ITargetMatchingService targetMatchingService)
        {
            _cacheService = cacheService;
            _targetMatchingService = targetMatchingService;
        }

        [HttpPost]
        public IActionResult Query([FromBody] ConfigQueryRequestParams requestParams)
        {
            // Lookup the agent name in the cache service. Populate the cache if this is the first query.
            if (_cacheService.TryGet(requestParams.AgentName, out List<CacheEntry>? entries))
            {
                return Ok(GetMatchingFragments(entries, requestParams));
            }
            else
            {
                return NotFound(requestParams.AgentName);
            }
        }

        /// <summary>
        /// For each target/fragment record, test whether it matches the specified client context data in the request params.
        /// If true, include it in the fragment map sent to the client.
        /// </summary>
        /// <returns>Map of each fragment ID and value that match the client context</returns>
        private Dictionary<string, string> GetMatchingFragments(List<CacheEntry>? entries, ConfigQueryRequestParams requestParams)
        {
            Dictionary<string, string> fragments = [];

            if (entries != null)
            {
                foreach (CacheEntry entry in entries)
                {
                    try
                    {
                        if (_targetMatchingService.IsMatch(entry.Target, requestParams))
                        {
                            if (!fragments.ContainsKey(entry.FragmentId))
                            {
                                fragments.Add(entry.FragmentId, entry.FragmentValue);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to match against target {TargetId}, {RequestParams}", entry.Target.Id, requestParams);
                        continue;
                    }
                }
            }

            return fragments;
        }
    }
}
