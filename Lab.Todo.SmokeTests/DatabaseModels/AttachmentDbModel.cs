using System.ComponentModel.DataAnnotations.Schema;

namespace Lab.Todo.SmokeTests.DatabaseModels
{
    [Table("Attachment")]
    public class AttachmentDbModel
    {
        public int AttachmentId { get; set; }
        public string? UniqueFileName { get; set; }
        public string? ProvidedFileName { get; set; }
    }
}