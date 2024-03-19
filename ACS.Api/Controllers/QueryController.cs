using ACS.Shared.Models;
using ACS.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace ACS.Api.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
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
        public ActionResult<ConfigQueryResponse> Query([FromBody] ConfigQueryRequestParams requestParams)
        {
            // Lookup the agent name in the cache service. Populate the cache if this is the first query.
            if (_cacheService.TryGet(requestParams.AgentName, out List<CompiledCacheEntry>? entries))
            {
                return Ok(_targetMatchingService.GetMatchingFragments(entries, requestParams));
            }
            else
            {
                return Ok(new ConfigQueryResponse
                {
                    Fragments = []
                });
            }
        }
    }
}
