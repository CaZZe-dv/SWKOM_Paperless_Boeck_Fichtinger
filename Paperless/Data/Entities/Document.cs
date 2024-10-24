using System.ComponentModel.DataAnnotations;

namespace Paperless.Data.Entities
{
    public class Document
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; }
        public byte[]? Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
