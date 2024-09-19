using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Paperless.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Route("/")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Paperless project response" });
        }
    }
}
