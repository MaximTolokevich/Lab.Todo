using System.Globalization;

namespace Lab.Todo.BLL.Services.ToDoItemManagers.Extensions
{
    public static class StringExtensions
    {
        public static string ToTitleCase(this string str) =>
            (str is null) ? null : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str.ToLower());
    }
}
