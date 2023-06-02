using System;

namespace Lab.Todo.BLL.Services.UniqueFileNameServices
{
    public class UniqueFileNameService : IUniqueFileNameService
    {
        public string GetUniqueFileName(string fileExtension) => $"{Guid.NewGuid()}.{fileExtension}";
    }
}
