using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Paperless.Database;
using Paperless.Models;
using System.Threading.Tasks;

namespace Paperless.Controllers
{
    /// <summary>
    /// Stellt Endpunkte für Paperless bereit.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly DatabaseDbContext _context;
        public TestController(DatabaseDbContext context)
        {
            _context = context;
        }

        // ================== CREATE ==================

        /// <summary>
        /// Lädt ein neues Dokument hoch.
        /// </summary>
        /// <returns>Eine Bestätigung des Uploads</returns>
        [HttpPost("UPLOAD")]
        public async Task<IActionResult> UploadDocument([FromBody] Document document)
        {
            // Ensure the document is not null
            if (document == null)
            {
                return BadRequest(new { message = "Kein Dokument bereitgestellt." });
            }

            // Add the document to the database
            _context.Add(document);
            await _context.SaveChangesAsync();

            // Return the added document with a success message
            return Ok(new { message = "Dokument erfolgreich hochgeladen.", document });
        }

        // ================== READ ==================

        /// <summary>
        /// Gibt alle Dokumente zurück.
        /// </summary>
        /// <returns>Eine Liste von Dokumenten</returns>
        [HttpGet("ALL")]
        public async Task<IActionResult> GetAllDocuments()
        {
            var documents = await _context.Documents.ToListAsync();
            return Ok(documents);
        }

        /// <summary>
        /// Gibt ein Dokument basierend auf der ID zurück.
        /// </summary>
        /// <param name="id">Die ID des Dokuments</param>
        /// <returns>Das Dokument mit der angegebenen ID</returns>
        [HttpGet("GET/{id}")]
        public async Task<IActionResult> GetDocument(int id)
        {
            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (document == null)
            {
                return NotFound(new { message = $"Dokument mit ID {id} nicht gefunden." });
            }
            return Ok(document);
        }

        // ================== UPDATE ==================

        /// <summary>
        /// Aktualisiert die Metadaten eines Dokuments.
        /// </summary>
        /// <param name="id">Die ID des Dokuments</param>
        /// <param name="document">Das aktualisierte Dokument</param>
        /// <returns>Eine Bestätigung der Aktualisierung</returns>
        [HttpPut("UPDATE/{id}")]
        public async Task<IActionResult> UpdateDocument(int id, [FromBody] Document document)
        {
            // Check if document exists
            var existingDocument = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (existingDocument == null)
            {
                return NotFound(new { message = $"Dokument mit ID {id} nicht gefunden." });
            }

            // Update the document's properties
            existingDocument.Name = document.Name ?? existingDocument.Name;
            existingDocument.Content = document.Content ?? existingDocument.Content;
            existingDocument.UploadDate = document.UploadDate != default ? document.UploadDate : existingDocument.UploadDate;

            // Save changes
            await _context.SaveChangesAsync();

            return Ok(new { message = "Dokument erfolgreich aktualisiert.", document = existingDocument });
        }

        // ================== DELETE ==================

        /// <summary>
        /// Löscht ein Dokument.
        /// </summary>
        /// <param name="id">Die ID des zu löschenden Dokuments</param>
        /// <returns>Eine Bestätigung der Löschung</returns>
        [HttpDelete("DELETE/{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (document == null)
            {
                return NotFound(new { message = $"Dokument mit ID {id} nicht gefunden." });
            }

            // Remove the document from the database
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Dokument mit ID {id} erfolgreich gelöscht." });
        }
    }
}
