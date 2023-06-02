namespace Lab.Todo.BLL.Services.UniqueFileNameServices
{
    public interface IUniqueFileNameService
    {
        string GetUniqueFileName(string fileExtension);
    }
}
