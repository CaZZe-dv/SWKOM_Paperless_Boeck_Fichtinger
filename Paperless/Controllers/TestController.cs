using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Paperless.Controllers
{
    /// <summary>
    /// Stellt Endpunkte für Paperless bereit.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    //[Route("/")]
    public class TestController : ControllerBase
    {
        //test cases

        /// <summary>
        /// Gibt ersten Response zurück
        /// </summary>
        /// <returns>Ein JSON-Objekt mit einer Nachricht</returns>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Paperless project response" });
        }

        /// <summary>
        /// Gibt die Testnachricht für die UI zurück
        /// </summary>
        /// <returns>Ein JSON-Objekt mit einer UI-spezifischen Nachricht</returns>
        [HttpGet ("UI")]
        public IActionResult GetUIResponse()
        {
            return Ok(new { message = "Seas das geht an die UI" });
        }


        //use cases

        /// <summary>
        /// Lädt ein neues Dokument hoch.
        /// </summary>
        /// <returns>Eine Bestätigung des Uploads</returns>
        [HttpPost("UPLOAD")]
        public IActionResult UploadDocument()
        {
            // Logik wird hier später implementiert
            return Ok(new { message = "Dokument erfolgreich hochgeladen (Platzhalterantwort)" });
        }

        /// <summary>
        /// Sucht nach einem Dokument.
        /// Unterstützt Volltext- und Fuzzy-Suche in ElasticSearch.
        /// </summary>
        /// <param name="query">Die Suchanfrage</param>
        /// <returns>Suchergebnisse als JSON-Objekt</returns>
        [HttpGet("SEARCH")]
        public IActionResult SearchDocument([FromQuery] string query)
        {
            // Logik wird hier später implementiert
            return Ok(new { message = $"Suchergebnisse für '{query}' (Platzhalterantwort)" });
        }

        /// <summary>
        /// Aktualisiert die Metadaten eines Dokuments.
        /// </summary>
        /// <param name="id">Die ID des Dokuments</param>
        /// <returns>Eine Bestätigung der Aktualisierung</returns>
        [HttpPut("UPDATE/{id}")]
        public IActionResult UpdateDocument(int id)
        {
            // Logik wird hier später implementiert
            return Ok(new { message = $"Dokument mit ID {id} erfolgreich aktualisiert (Platzhalterantwort)" });
        }

        /// <summary>
        /// Löscht ein Dokument.
        /// </summary>
        /// <param name="id">Die ID des zu löschenden Dokuments</param>
        /// <returns>Eine Bestätigung der Löschung</returns>
        [HttpDelete("DELETE/{id}")]
        public IActionResult DeleteDocument(int id)
        {
            // Logik wird hier später implementiert
            return Ok(new { message = $"Dokument mit ID {id} erfolgreich gelöscht (Platzhalterantwort)" });
        }

    }
}
