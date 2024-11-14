using Microsoft.EntityFrameworkCore;
using Paperless.Models;

namespace Paperless.Database
{
    public class DatabaseDbContext : DbContext
    {
        public DatabaseDbContext(DbContextOptions<DatabaseDbContext> options) : base(options)
        {
        }

        public DbSet<Document> Documents { get; set; }
    }
}
