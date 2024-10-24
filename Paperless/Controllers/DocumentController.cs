using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Paperless.Data;
using Paperless.Data.Entities;
using Paperless.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Paperless.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DocumentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument([FromBody] DocumentDto documentDto)
        {
            if (documentDto == null || string.IsNullOrEmpty(documentDto.Content))
            {
                return BadRequest("Invalid document data");
            }

            // Konvertiere den Base64-codierten String in byte[]
            byte[] documentBytes;
            try
            {
                documentBytes = Convert.FromBase64String(documentDto.Content);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid content format");
            }

            // Neues Dokument erstellen
            var document = new Document
            {
                Title = documentDto.Title,
                Content = documentBytes
            };

            // Füge das Dokument zur Datenbank hinzu
            _context.Documents.Add(document);

            // Speichere die Änderungen in der Datenbank
            await _context.SaveChangesAsync();

            return Ok("Document uploaded successfully");
        }
    }
}
