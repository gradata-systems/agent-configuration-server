using ACS.Admin.Auth;
using ACS.Admin.Models;
using ACS.Shared.Models;
using ACS.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ACS.Admin.Controllers
{
    [Authorize(Roles = UserRole.Administrator + "," + UserRole.Editor + "," + UserRole.ReadonlyUser)]
    public class SimulateController : Controller
    {
        private readonly ICacheService _cacheService;
        private readonly ITargetMatchingService _targetMatchingService;

        public SimulateController(ICacheService cacheService, ITargetMatchingService targetMatchingService)
        {
            _cacheService = cacheService;
            _targetMatchingService = targetMatchingService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([ModelBinder(typeof(ConfigQueryRequestParamsModelBinder))] ConfigQueryRequestParams requestParams)
        {
            List<CompiledCacheEntry>? cacheEntries = await _cacheService.GetAsync(requestParams.AgentName);

            Log
                .ForContext("RequestParams", requestParams)
                .Information("Performed target simulation");

            return View(_targetMatchingService.GetMatchingFragments(cacheEntries, requestParams));
        }

        public IActionResult NoFragments()
        {
            return View();
        }
    }
}
