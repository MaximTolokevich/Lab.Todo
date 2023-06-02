using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Lab.Todo.SmokeTests.Helpers
{
    internal static class ObjectExtensions
    {
        public static string ToQueryString(this object obj, bool excludeNullProperties = false)
        {
            var objectValues = JObject.FromObject(obj)
                .Children()
                .Values();

            return string.Join(null, objectValues.Select(value => value.Type == JTokenType.Null && excludeNullProperties
                ? null
                : GetQueryStringItem(value)))[..^1];
        }

        private static string GetQueryStringItem(JToken valueAsJToken)
        {
            var values = valueAsJToken.Values();

            return values.Any()
                ? string.Join(null, values.Select(GetQueryStringItem))
                : $"{valueAsJToken.Path}={HttpUtility.UrlEncode(valueAsJToken.ToString())}&";
        }
    }
}