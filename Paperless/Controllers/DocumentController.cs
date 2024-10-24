using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Paperless.Data.Repositories;

namespace Paperless.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentRepository _repository;

        public DocumentController(IDocumentRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public IActionResult UploadDocument(IFormFile file)
        {
            // Implement your document upload logic here
            return Ok(new { message = "Document uploaded successfully." });
        }

        [HttpGet]
        public IActionResult GetDocuments()
        {
            // Implement your document retrieval logic here
            return Ok(new { message = "Documents retrieved successfully." });
        }
    }
}
