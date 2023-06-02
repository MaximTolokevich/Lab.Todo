using Lab.Todo.SmokeTests.Services.Interfaces;

namespace Lab.Todo.SmokeTests.Services
{
    public class TokenHolder : ITokenHolder
    {
        public string Token { get; private set; }

        public void SetToken(string token)
        {
            Token = token;
        }
    }
}