// Paperless/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Paperless.Data.Entities;

namespace Paperless.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Weitere Konfigurationen hier (z.B. Indizes, Constraints)
        }
    }
}
