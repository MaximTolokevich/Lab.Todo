using AutoMapper;
using System.Text;
using Lab.Todo.Api.MappingProfiles;

namespace Lab.Todo.Api.Tests.Helpers
{
    public static class ApiTestsHelper
    {
        public static IMapper SetupMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(ToDoItemProfileApi)));
            return new Mapper(config);
        }

        public static string GenerateString(int length)
        {
            var stringBuilder = new StringBuilder(length);

            for (var i = 0; i < length; i++)
            {
                stringBuilder.Append('a');
            }

            return stringBuilder.ToString();
        }
    }
}
