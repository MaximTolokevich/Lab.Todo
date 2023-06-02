using Lab.Todo.DAL.Entities.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab.Todo.DAL.Entities
{
    [Table("Attachment")]
    public class AttachmentDbEntry : IEntity
    {
        [Column("AttachmentId")]
        public int Id { get; set; }
        public string UniqueFileName { get; set; }
        public string ProvidedFileName { get; set; }

        [NotMapped]
        public byte[] Content { get; set; }
    }
}