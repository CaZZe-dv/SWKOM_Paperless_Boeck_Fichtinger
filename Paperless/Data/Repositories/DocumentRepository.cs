using Paperless.Data.Entities;

namespace Paperless.Data.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddDocument(Document document)
        {
            _context.Documents.Add(document);
            _context.SaveChanges();
        }

        public IEnumerable<Document> GetAllDocuments()
        {
            return _context.Documents.ToList();
        }
    }
}
