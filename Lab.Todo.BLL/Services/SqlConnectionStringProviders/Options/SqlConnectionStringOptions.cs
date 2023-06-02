using System.ComponentModel.DataAnnotations;

namespace Lab.Todo.BLL.Services.SqlConnectionStringProviders.Options
{
    public class SqlConnectionStringOptions
    {
        [Required]
        public string DataSource { get; set; }

        [Required]
        public string InitialCatalog { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
    }
}