using FluentAssertions;
using Lab.Todo.Api.DTOs.Requests;
using Lab.Todo.Api.DTOs.Responses;
using Lab.Todo.SmokeTests.Options;
using Lab.Todo.SmokeTests.Services.Interfaces;
using Lab.Todo.SmokeTests.Services.DataAccess;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Refit;
using TechTalk.SpecFlow;

namespace Lab.Todo.SmokeTests.Steps
{
    [Binding]
    public class JwtAuthorizationSteps
    {
        private const string SecretValue = "c06623b1-fe5e-481b-9c3c-de6f184d4dd6";
        private readonly ScenarioContext _scenarioContext;
        private readonly IAccountService _accountDataAccess;
        private readonly IToDoItemsService _toDoItemsDataAccess;
        private readonly ITokenHolder _tokenHolder;
        private readonly UserOptions _userOptions;

        public JwtAuthorizationSteps(
            ScenarioContext scenarioContext,
            IAccountService accountDataAccess,
            IToDoItemsService toDoItemsDataAccess,
            ITokenHolder tokenHolder,
            IOptions<UserOptions> userOptions)
        {
            _scenarioContext = scenarioContext;
            _accountDataAccess = accountDataAccess;
            _toDoItemsDataAccess = toDoItemsDataAccess;
            _tokenHolder = tokenHolder;
            _userOptions = userOptions.Value;
        }

        [Given(@"a user with name '([^']*)'")]
        [Given(@"an invalid user with name '([^']*)'")]
        public void GivenAUserWithName(string username)
        {
            _scenarioContext["username"] = username;
        }

        [When(@"smoke user tries to receive a new auth jwt token")]
        [When(@"bad user tries to receive a new auth jwt token")]
        public async Task WhenSmokeUserTriesToReceiveANewAuthJwtToken()
        {
            var authenticateRequest = new AuthenticateRequest
            {
                Username = _scenarioContext["username"].ToString()
            };

            var authenticateResponse = await _accountDataAccess.Authenticate(authenticateRequest);

            _scenarioContext["authResponse"] = authenticateResponse.Content;
            _scenarioContext["statusCode"] = authenticateResponse.StatusCode;
        }

        [Then(@"server returns a (.*) status and a new auth jwt token in response body")]
        public void ThenServerReturnsAStatusAndANewAuthJwtTokenInResponseBody(int expectedStatusCode)
        {
            var authenticateResponse = _scenarioContext["authResponse"] as AuthenticateResponse;
            var statusCode = _scenarioContext["statusCode"];

            statusCode.Should().BeOfType<HttpStatusCode>()
                .And.Be((HttpStatusCode)expectedStatusCode);

            authenticateResponse.Should().NotBeNull();
            authenticateResponse!.Token
                .Should().NotBeNullOrWhiteSpace()
                .And.MatchRegex(JwtConstants.JsonCompactSerializationRegex);
        }

        [Then(@"server returns a (.*) status and no auth token in response body")]
        public void ThenServerReturnsAStatusAndNoAuthTokenInResponseBody(int expectedStatusCode)
        {
            var authenticateResponse = _scenarioContext["authResponse"] as AuthenticateResponse;
            var statusCode = _scenarioContext["statusCode"];

            statusCode.Should().BeOfType<HttpStatusCode>()
                .And.Be((HttpStatusCode)expectedStatusCode);

            authenticateResponse.Should().BeNull();
        }

        [Given(@"a not defined auth token")]
        public void GivenANotDefinedAuthToken()
        {
            _tokenHolder.SetToken("ANotDefinedAuthToken");
        }

        [When(@"I make a request to any anonymous endpoint")]
        public async Task WhenIMakeARequestToAnyAnonymousEndpoint()
        {
            var authenticateRequest = new AuthenticateRequest { Username = _userOptions.Username };

            var authenticateResponse = await _accountDataAccess.Authenticate(authenticateRequest);

            _scenarioContext["statusCode"] = authenticateResponse.StatusCode;
        }

        [Then(@"I receive a success status")]
        public void ThenIReceiveSuccessStatus()
        {
            if (_scenarioContext.TryGetValue("statusCode", out HttpStatusCode statusCode))
            {
                statusCode.Should().Match(code => IsSuccessStatusCode(code));

                return;
            }

            _scenarioContext.Should().NotContainKey("apiException", because:
                $"{nameof(HttpResponseMessage.EnsureSuccessStatusCode)} doesn't throw while response have a success http status code");
        }

        [Given(@"a valid auth jwt token")]
        public async Task GivenAValidAuthJwtToken()
        {
            var authenticateRequest = new AuthenticateRequest { Username = _userOptions.Username };

            var authenticateResponse = await _accountDataAccess.Authenticate(authenticateRequest);

            _tokenHolder.SetToken(authenticateResponse.Content!.Token);
        }

        [When(@"I make a request to any endpoint with required authentication")]
        public async Task WhenIMakeARequestToAnyEndpointWithRequiredAuthentication()
        {
            try
            {
                await _toDoItemsDataAccess.GetToDoItemStatuses();
            }
            catch (ApiException ex)
            {
                _scenarioContext["apiException"] = ex;
            }
        }

        [Given(@"an invalid auth jwt token")]
        public void GivenAnInvalidAuthJwtToken()
        {
            var tokenDescriptor = GetDefaultSecurityTokenDescriptor();

            tokenDescriptor.SigningCredentials = CreateSigningCredentials("ThisIsInvalidSecretValue");

            var invalidToken = CreateToken(tokenDescriptor);

            _tokenHolder.SetToken(invalidToken);
        }

        [Given(@"a broken auth jwt token")]
        public void GivenABrokenAuthJwtTokenE_G_SignatureIsInvalid()
        {
            _tokenHolder.SetToken("ThisIsBlaBlaBlaOrInvalidToken");
        }

        [Given(@"an expired auth jwt token")]
        public void GivenAnExpiredAuthJwtToken()
        {
            var tokenDescriptor = GetDefaultSecurityTokenDescriptor();

            tokenDescriptor.NotBefore = DateTime.UtcNow.Add(TimeSpan.FromHours(-1));
            tokenDescriptor.Expires = DateTime.UtcNow.Add(TimeSpan.FromMinutes(-30));

            var expiredToken = CreateToken(tokenDescriptor);

            _tokenHolder.SetToken(expiredToken);
        }

        [Then(@"I receive status (.*)")]
        public void ThenIReceiveStatus(int expectedStatusCode)
        {
            var exception = _scenarioContext["apiException"] as ApiException;

            exception.Should().NotBeNull(because:
                $"{nameof(HttpResponseMessage.EnsureSuccessStatusCode)} throw while response doesn't have a success http status code");

            exception!.StatusCode.Should().Be((HttpStatusCode)expectedStatusCode);
        }

        private static bool IsSuccessStatusCode(HttpStatusCode? statusCode) =>
            statusCode is not null && (int)statusCode >= 200 && (int)statusCode <= 299;

        private static string CreateToken(SecurityTokenDescriptor tokenDescriptor)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private static SecurityTokenDescriptor GetDefaultSecurityTokenDescriptor()
        {
            var defaultClaims = new List<Claim>
            {
                new(ClaimTypes.Name, "SmokeUsername")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(defaultClaims),
                Expires = DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)),
                SigningCredentials = CreateSigningCredentials(SecretValue)
            };

            return tokenDescriptor;
        }

        private static SigningCredentials CreateSigningCredentials(string secretValue)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretValue));

            return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        }
    }
}
