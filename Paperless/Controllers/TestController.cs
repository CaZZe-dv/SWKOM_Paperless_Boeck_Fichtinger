using Microsoft.AspNetCore.Mvc;

namespace Paperless.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        // GET api/manage
        [HttpGet("manage")]
        public IActionResult ManageDocuments()
        {
            return Ok(new { message = "Manage: Dummy data returned" });
        }

        // POST api/upload
        [HttpPost("upload")]
        public IActionResult UploadDocument()
        {
            return Ok(new { message = "Upload: Document uploaded successfully (dummy response)" });
        }

        // DELETE api/delete
        [HttpDelete("delete")]
        public IActionResult DeleteDocument()
        {
            return Ok(new { message = "Delete: Document deleted successfully (dummy response)" });
        }
    }
}
