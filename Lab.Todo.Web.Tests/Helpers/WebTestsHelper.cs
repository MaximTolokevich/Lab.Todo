using System.Text;
using AutoMapper;
using Lab.Todo.Web.MappingProfiles;

namespace Lab.Todo.Web.Tests.HelpersTests
{
    internal static class WebTestsHelper
    {
        internal static IMapper SetupMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(ToDoItemProfile)));
            return new Mapper(config);
        }

        internal static string GenerateString(int length)
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
