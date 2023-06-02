using FluentAssertions;
using Lab.Todo.BLL.Services.TokenHandler;
using Lab.Todo.BLL.Services.TokenHandler.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;

namespace Lab.Todo.BLL.Tests.ServicesTests.TokenHandlerTests
{
    public class SecurityTokenHandlerAdapterTests
    {
        private readonly Mock<SecurityTokenHandler> _securityTokenHandler;
        private readonly SecurityTokenHandlerAdapter _securityTokenHandlerAdapter;

        public SecurityTokenHandlerAdapterTests()
        {
            _securityTokenHandler = new Mock<SecurityTokenHandler>();

            var jwtOptions = new JwtOptions
            {
                TokenExpirationTime = TimeSpan.FromMinutes(5),
                SecretValue = Guid.NewGuid().ToString(),
                EncryptionAlgorithms = new List<string> { "HS256" }
            };

            _securityTokenHandlerAdapter = new SecurityTokenHandlerAdapter(_securityTokenHandler.Object, Options.Create(jwtOptions));
        }

        [Fact]
        public void CreateToken_Should_CreateToken_And_Return_WrittenToken()
        {
            // Arrange
            const string expected = "ExpectedToken";
            var mockToken = new Mock<SecurityToken>();

            _securityTokenHandler.Setup(s => s.CreateToken(It.IsAny<SecurityTokenDescriptor>()))
                .Returns(mockToken.Object);

            _securityTokenHandler.Setup(s => s.WriteToken(mockToken.Object))
                .Returns(expected);

            // Act
            var actual = _securityTokenHandlerAdapter.CreateToken(new List<Claim>());

            // Assert
            actual.Should().Be(expected);

            _securityTokenHandler.VerifyAll();
        }

        [Fact]
        public void GenerateToken_Should_ThrowException_When_ClaimsNull()
        {
            // Arrange
            Func<string> act = () => _securityTokenHandlerAdapter.CreateToken(null);

            // Act & Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}