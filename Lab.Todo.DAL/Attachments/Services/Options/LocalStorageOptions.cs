using System.ComponentModel.DataAnnotations;

namespace Lab.Todo.DAL.Attachments.Services.Options
{
    public class LocalStorageOptions
    {
        [Required]
        public string FolderPath { get; set; }
    }
}