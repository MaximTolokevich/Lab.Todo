namespace Lab.Todo.BLL.Services.AttachmentManagers.Models
{
    public class Attachment
    {
        public int Id { get; set; }
        public string UniqueFileName { get; set; }
        public string ProvidedFileName { get; set; }
        public byte[] Content { get; set; }
    }
}