using ACS.Shared;
using ACS.Shared.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;

namespace ACS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(AppDbContext dbContext, IOptions<DataSourceConfiguration> config)
        {
            if (await dbContext.Database.CanConnectAsync())
            {
                return Ok();
            }
            else
            {
                DataSourceConfiguration dataSourceConfig = config.Value;
                Log.Error("Failed to connect to database {Server} {Port}", dataSourceConfig.Server, dataSourceConfig.Port);

                return Problem("Failed to connect to database");
            }
        }
    }
}
