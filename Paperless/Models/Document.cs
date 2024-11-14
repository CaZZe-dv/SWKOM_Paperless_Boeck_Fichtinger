using System.ComponentModel.DataAnnotations.Schema;

namespace Paperless.Models
{
    [Table("document")]
    public class Document
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("content")]
        public string Content { get; set; }
        [Column("uploaddate")]
        public DateTime UploadDate { get; set; }

    }
}
