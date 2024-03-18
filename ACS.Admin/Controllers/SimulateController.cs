using ACS.Admin.Auth;
using ACS.Shared.Models;
using ACS.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Search([Bind("AgentName,AgentVersion,UserName,HostName,EnvironmentName")] ConfigQueryRequestParams requestParams)
        {
            List<CacheEntry>? cacheEntries = await _cacheService.GetAsync(requestParams.AgentName);

            return View(_targetMatchingService.GetMatchingFragments(cacheEntries, requestParams));
        }

        public IActionResult NoFragments()
        {
            return View();
        }
    }
}
