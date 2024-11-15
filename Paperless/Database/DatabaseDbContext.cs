using log4net;
using Microsoft.EntityFrameworkCore;
using Paperless.Models;

namespace Paperless.Database
{
    public class DatabaseDbContext : DbContext
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DatabaseDbContext));

        public DatabaseDbContext(DbContextOptions<DatabaseDbContext> options) : base(options)
        {
            try
            {
                _logger.Info("DatabaseDbContext initialisiert.");
            }
            catch (Exception ex)
            {
                _logger.Fatal("Fehler bei der Initialisierung des DbContext.", ex);
                throw new DatabaseException("Fehler beim Initialisieren des Datenbank-Kontextes.", ex);
            }
        }

        public DbSet<Document> Documents { get; set; }
    }
    public class DatabaseException : Exception
    {
        public DatabaseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
