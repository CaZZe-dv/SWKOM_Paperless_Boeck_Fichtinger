using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Paperless.Database;
using Paperless.Models;
using log4net;
using Minio;
using Minio.DataModel.Args;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Minio.ApiEndpoints;
using System.Reactive.Linq;

namespace Paperless.Controllers
{
    /// <summary>
    /// Stellt Endpunkte für Paperless bereit.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TestController));
        private readonly DatabaseDbContext _context;
        private readonly RabbitMqService _rabbitMqService;
        private readonly IMinioClient _minioClient;
        private const string BucketName = "uploads"; // Definierter Bucket-Name für MinIO

        public TestController(DatabaseDbContext context, ILogger<TestController> logger)
        {
            _context = context;
            _rabbitMqService = new RabbitMqService();

            // MinIO Client einrichten
            _minioClient = new MinioClient()
                .WithEndpoint("minio", 9000) // MinIO-Serveradresse
                .WithCredentials("minioadmin", "minioadmin") // MinIO-Anmeldedaten
                .WithSSL(false) // SSL deaktiviert
                .Build();
        }

        // ================== test cases ==================
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
        [HttpGet("UI")]
        public IActionResult GetUIResponse()
        {
            return Ok(new { message = "Seas das geht an die UI" });
        }

        // ================== CREATE ==================
        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument([FromForm] IFormFile file, [FromForm] string name)
        {
            if (file == null || file.Length == 0)
            {
                _logger.Warn("Keine Datei hochgeladen.");
                return BadRequest("Keine Datei hochgeladen.");
            }

            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            var document = new Document
            {
                Name = name,
                Content = fileBytes,
                UploadDate = DateTime.UtcNow
            };

            try
            {
                // Dokument in der lokalen Datenbank speichern
                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                // Senden Sie die Datei an MinIO
                await UploadFileToMinio(file);

                // Nachricht an RabbitMQ senden
                _rabbitMqService.SendMessage($"Dokument '{name}' erfolgreich hochgeladen.");
                _logger.Info($"Dokument {name} erfolgreich hochgeladen.");

                return Ok(new { message = "Dokument erfolgreich hochgeladen." });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.Error("Fehler beim Speichern des Dokuments in der Datenbank.", dbEx);
                return StatusCode(500, "Fehler beim Speichern des Dokuments.");
            }
            catch (Exception ex)
            {
                _logger.Error("Allgemeiner Fehler beim Hochladen eines Dokuments.", ex);
                return StatusCode(500, "Fehler beim Speichern des Dokuments.");
            }
        }

        // ================== MinIO Upload ==================
        private async Task UploadFileToMinio(IFormFile file)
        {
            try
            {
                // Bucket überprüfen und erstellen, falls er nicht existiert
                await EnsureBucketExists();

                var fileName = Path.GetFileName(file.FileName);
                await using var fileStream = file.OpenReadStream();

                // Datei zu MinIO hochladen
                await _minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(fileName)
                    .WithStreamData(fileStream)
                    .WithObjectSize(file.Length)
                    .WithContentType(file.ContentType));

                _logger.Info($"Datei '{fileName}' erfolgreich zu MinIO hochgeladen.");
            }
            catch (Exception ex)
            {
                _logger.Error("Fehler beim Hochladen der Datei zu MinIO.", ex);
                throw;
            }
        }

        // ================== MinIO Bucket sicherstellen ==================
        private async Task EnsureBucketExists()
        {
            try
            {
                bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(BucketName));
                if (!found)
                {
                    await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(BucketName));
                    _logger.Info($"Bucket '{BucketName}' erfolgreich erstellt.");
                }
                else
                {
                    _logger.Info($"Bucket '{BucketName}' existiert bereits.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Fehler beim Überprüfen oder Erstellen des Buckets '{BucketName}'.", ex);
                throw;
            }
        }
        // ================== READ ==================
        [HttpGet("files")]
        public async Task<IActionResult> ListFiles()
        {
            var objects = new List<string>();

            try
            {
                // ListObjectsAsync returns an IObservable
                var observable = _minioClient.ListObjectsAsync(new ListObjectsArgs().WithBucket(BucketName));

                // Convert the observable to a list
                var objectList = await observable.ToList();

                // Collect object keys
                foreach (var item in objectList)
                {
                    objects.Add(item.Key);
                }

                _logger.Info($"Erfolgreich die Liste der Dateien im Bucket '{BucketName}' abgerufen.");
                return Ok(objects);
            }
            catch (Exception ex)
            {
                _logger.Error("Fehler beim Abrufen der Dateien aus MinIO.", ex);
                return StatusCode(500, new { error = $"Fehler beim Abrufen der Dateien: {ex.Message}" });
            }
        }

        // ================== DELETE ==================
        [HttpDelete("delete/{fileName}")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            try
            {
                await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(fileName));

                _logger.Info($"Datei '{fileName}' erfolgreich aus dem Bucket '{BucketName}' gelöscht.");
                return Ok(new { message = $"Datei '{fileName}' erfolgreich gelöscht." });
            }
            catch (Exception ex)
            {
                _logger.Error("Fehler beim Löschen der Datei aus MinIO.", ex);
                return StatusCode(500, new { message = $"Fehler beim Löschen der Datei: {ex.Message}" });
            }
        }





        // ================== READ ==================
        /// <summary>
        /// Gibt alle Dokumente zurück.
        /// </summary>
        /// <returns>Eine Liste von Dokumenten</returns>
        [HttpGet("ALL")]
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                var documents = await _context.Documents.ToListAsync();
                _logger.Info("Alle Dokumente erfolgreich abgerufen.");
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.Error("Fehler beim Abrufen der Dokumente.", ex);
                return StatusCode(500, "Fehler beim Abrufen der Dokumente.");
            }
        }

        /// <summary>
        /// Gibt ein Dokument basierend auf der ID zurück.
        /// </summary>
        /// <param name="id">Die ID des Dokuments</param>
        /// <returns>Das Dokument mit der angegebenen ID</returns>
        [HttpGet("GET/{id}")]
        public async Task<IActionResult> GetDocumentName(int id)
        {
            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (document == null)
            {
                _logger.Info($"Dokument mit ID {id} nicht gefunden.");
                return NotFound(new { message = $"Dokument mit ID {id} nicht gefunden." });
            }
            return Ok(new { document.Name });
        }

        // ================== UPDATE ==================
        /// <summary>
        /// Aktualisiert die Metadaten eines Dokuments.
        /// </summary>
        /// <param name="id">Die ID des Dokuments</param>
        /// <param name="document">Das aktualisierte Dokument</param>
        /// <returns>Eine Bestätigung der Aktualisierung</returns>
        [HttpPut("UPDATE/{id}")]
        public async Task<IActionResult> UpdateDocument(int id, [FromForm] IFormFile file)
        {
            var existingDocument = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (existingDocument == null)
            {
                _logger.Info($"Dokument mit ID {id} nicht gefunden.");
                return NotFound(new { message = $"Dokument mit ID {id} nicht gefunden." });
            }

            if (file != null && file.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    existingDocument.Content = memoryStream.ToArray();
                }
                existingDocument.UploadDate = DateTime.UtcNow;
            }
            else
            {
                _logger.Info("Keine Datei zum Hochladen bereitgestellt.");
                return BadRequest("Keine Datei zum Hochladen bereitgestellt.");
            }

            await _context.SaveChangesAsync();

            // Send message to RabbitMQ
            _rabbitMqService.SendMessage($"Document with ID {id} updated successfully.");
            _logger.Info("Dokument erfolgreich aktualisiert.");
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
                _logger.Info($"Dokument mit ID {id} nicht gefunden.");
                return NotFound(new { message = $"Dokument mit ID {id} nicht gefunden." });
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            // Send message to RabbitMQ
            _rabbitMqService.SendMessage($"Document with ID {id} deleted successfully.");
            _logger.Info($"Dokument mit ID {id} erfolgreich gelöscht.");
            return Ok(new { message = $"Dokument mit ID {id} erfolgreich gelöscht." });
        }
    }
}
