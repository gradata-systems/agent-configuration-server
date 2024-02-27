using ACS.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ACS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(AppDbContext dbContext)
        {
            if (await dbContext.Database.CanConnectAsync())
            {
                return Ok();
            }
            else
            {
                return Problem("Failed to connect to database");
            }
        }
    }
}
