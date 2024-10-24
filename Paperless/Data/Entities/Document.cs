using System.ComponentModel.DataAnnotations;

namespace Paperless.Data.Entities
{
    public class Document
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Content { get; set; }
        // Additional properties can be added here
    }
}
