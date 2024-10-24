using Paperless.Data.Entities;

namespace Paperless.Data.Repositories
{
    public interface IDocumentRepository
    {
        void AddDocument(Document document);
        IEnumerable<Document> GetAllDocuments();
    }
}
