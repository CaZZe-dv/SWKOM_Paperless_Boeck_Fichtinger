// Paperless/Data/Repositories/DocumentRepository.cs
using Paperless.Data.Entities;

namespace Paperless.Data.Repositories
{
    public class DocumentRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Document> AddDocument(Document document)
        {
            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();
            return document;
        }
    }
}
