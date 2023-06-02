using AutoMapper;
using Lab.Todo.BLL.MappingProfiles;

namespace Lab.Todo.BLL.Tests.Helpers
{
    internal static class BLLTestsHelper
    {
        internal static IMapper SetupMapper()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AllowNullCollections = true;
                cfg.AddMaps(typeof(ToDoItemProfile));
            });
            return new Mapper(mapperConfig);
        }
    }
}
