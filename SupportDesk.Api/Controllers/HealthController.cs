using Microsoft.AspNetCore.Mvc;

namespace SupportDesk.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "SupportDesk API is running." });
        }
    }
}