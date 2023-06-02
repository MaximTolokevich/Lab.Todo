using Lab.Todo.SmokeTests.Services.Interfaces;

namespace Lab.Todo.SmokeTests.Services
{
    public class UserContext : IUserContext
    {
        private readonly ITokenHolder _tokenHolder;

        private bool _isDisposed;

        public UserContext(ITokenHolder tokenHolder)
        {
            _tokenHolder = tokenHolder;
        }

        public string PreviousToken { get; set; } 

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _tokenHolder.SetToken(PreviousToken);
            _isDisposed = true;
        }
    }
}