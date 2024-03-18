using ACS.Api.Models;
using ACS.Api.Services;
using ACS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text.Json;
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
        public ActionResult<List<CacheEntry>> Query([FromBody] ConfigQueryRequestParams requestParams)
        {
            // Lookup the agent name in the cache service. Populate the cache if this is the first query.
            if (_cacheService.TryGet(requestParams.AgentName, out List<CacheEntry>? entries))
            {
                return Ok(GetMatchingFragments(entries, requestParams));
            }
            else
            {
                return Ok(new List<CacheEntry>());
            }
        }

        /// <summary>
        /// For each target/fragment record, test whether it matches the specified client context data in the request params.
        /// If true, include it in the fragment map sent to the client.
        /// </summary>
        /// <returns>Map of each fragment ID and value that match the client context</returns>
        private ConfigQueryResponse GetMatchingFragments(List<CacheEntry>? entries, ConfigQueryRequestParams requestParams)
        {
            Dictionary<string, Fragment> fragments = [];

            if (entries != null)
            {
                // Group by fragment name, taking the first matching unique fragment (by name) by highest priority.
                // Fragments with no value are excluded from the final result set.
                fragments = entries
                    .Where(entry => _targetMatchingService.IsMatch(entry.Target, requestParams))
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
    }
}
