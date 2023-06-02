using Refit;

namespace Lab.Todo.SmokeTests.Helpers
{
    public static class ApiResponseExtensions
    {
        public static void EnsureSuccessStatusCode(this IApiResponse apiResponse)
        {
            if (!apiResponse.IsSuccessStatusCode)
            {
                throw apiResponse.Error!;
            }
        }
    }
}