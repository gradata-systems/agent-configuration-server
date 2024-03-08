using ACS.Api.Models;
using ACS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using static System.Net.Mime.MediaTypeNames;

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
        [Consumes(Application.Json)]
        [Produces(Application.Json)]
        public IActionResult Query([FromBody] ConfigQueryRequestParams requestParams)
        {
            // Lookup the agent name in the cache service. Populate the cache if this is the first query.
            if (_cacheService.TryGet(requestParams.AgentName, out List<CacheEntry>? entries))
            {
                Log.Information("Querying configuration fragments using parameters {RequestParams}", requestParams);

                return Ok(GetMatchingFragments(entries, requestParams));
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// For each target/fragment record, test whether it matches the specified client context data in the request params.
        /// If true, include it in the fragment map sent to the client.
        /// </summary>
        /// <returns>Map of each fragment ID and value that match the client context</returns>
        private ConfigQueryResponse GetMatchingFragments(List<CacheEntry>? entries, ConfigQueryRequestParams requestParams)
        {
            Dictionary<string, string> fragments = [];

            if (entries != null)
            {
                // Group by fragment name, taking the first matching unique fragment (by name) by highest priority
                fragments = entries
                    .Where(entry => _targetMatchingService.IsMatch(entry.Target, requestParams))
                    .GroupBy(entry => entry.Fragment.Name, entry => entry, (fragmentName, entries) => entries.OrderByDescending(entry => entry.Fragment.Priority).First())
                    .ToDictionary(entry => entry.Fragment.Name, entry => entry.Fragment.Value);
            }

            return new ConfigQueryResponse
            {
                Fragments = fragments
            };
        }
    }
}
