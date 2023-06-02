using Lab.Todo.Api.DTOs.Requests;
using Lab.Todo.Api.DTOs.Responses;
using Lab.Todo.SmokeTests.Helpers;
using Lab.Todo.SmokeTests.Options;
using Lab.Todo.SmokeTests.Services.DataAccess;
using Lab.Todo.SmokeTests.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Lab.Todo.SmokeTests.Services
{
    public class AuthorizationManager : IAuthorizationManager
    {
        private readonly IAccountService _accountApiDataAccess;
        private readonly ITokenHolder _tokenHolder;
        private readonly IServiceProvider _serviceProvider;
        private readonly UserOptions _userOptions;

        private string? _authorizationToken;

        public AuthorizationManager(IAccountService accountApiDataAccess, ITokenHolder tokenHolder, IServiceProvider serviceProvider, IOptions<UserOptions> userOptions)
        {
            _accountApiDataAccess = accountApiDataAccess;
            _tokenHolder = tokenHolder;
            _serviceProvider = serviceProvider;
            _userOptions = userOptions.Value;
        }

        public async Task<IUserContext> AuthorizeAsUser(string username)
        {
            var authenticateResponse = await AuthenticateUser(username);

            var previousUserToken = await GetAuthorizationToken();
            _authorizationToken = authenticateResponse.Token;
            _tokenHolder.SetToken(_authorizationToken);

            var userContext = _serviceProvider.GetRequiredService<IUserContext>();

            userContext.PreviousToken = previousUserToken!;

            return userContext;
        }

        public async Task<string?> GetAuthorizationToken()
        {
            if (_tokenHolder.Token is not null) return _tokenHolder.Token;

            if (_authorizationToken is null)
            {
                var authenticateResponse = await AuthenticateUser(_userOptions.Username);

                _authorizationToken = authenticateResponse.Token;
            }

            _tokenHolder.SetToken(_authorizationToken);

            return _tokenHolder.Token;
        }

        private async Task<AuthenticateResponse> AuthenticateUser(string username)
        {
            var authResponse = await _accountApiDataAccess.Authenticate(new AuthenticateRequest { Username = username });

            authResponse.EnsureSuccessStatusCode();

            return authResponse.Content ?? throw new ApplicationException($"User authentication failed: {nameof(AuthenticateResponse)} is null.");
        }
    }
}