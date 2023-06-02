using System.ComponentModel.DataAnnotations;

namespace Lab.Todo.DAL.Attachments.Services.Options
{
    public class AzureStorageOptions
    {
        [Required]
        public string ContainerName { get; set; }

        [Required]
        public string ConnectionStringPrimary { get; set; }

        [Required]
        public string ConnectionStringSecondary { get; set; }
    }
}