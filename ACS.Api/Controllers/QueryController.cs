using Microsoft.AspNetCore.Mvc;

namespace ACS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueryController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok("123");
        }
    }
}
