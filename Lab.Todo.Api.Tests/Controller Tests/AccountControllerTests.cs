using FluentAssertions;
using Lab.Todo.Api.Controllers;
using Lab.Todo.Api.DTOs.Requests;
using Lab.Todo.Api.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Lab.Todo.BLL.Services.TokenHandler;
using Xunit;

namespace Lab.Todo.Api.Tests.Controller_Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<ISecurityTokenHandler> _mockTokenManager;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _mockTokenManager = new Mock<ISecurityTokenHandler>();

            _controller = new AccountController(_mockTokenManager.Object);
        }

        [Fact]
        public void Authenticate_Should_SendOkResponse_WithUserData_When_RequestBody_ContainsUsername()
        {
            // Arrange         
            const string token = "Token";
            const string username = "UserName";

            _mockTokenManager
                .Setup(s => s.CreateToken(It.IsAny<IEnumerable<Claim>>()))
                .Returns(token);

            var authenticateRequest = new AuthenticateRequest
            {
                Username = username
            };

            // Act
            var actual = _controller.Authenticate(authenticateRequest);

            var (isObjectValid, validationResults) = TryValidateModel(authenticateRequest);

            // Assert
            actual.Should().BeOfType<ActionResult<AuthenticateResponse>>();

            actual.Result.Should().BeAssignableTo<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(new AuthenticateResponse
                {
                    Username = username,
                    Token = token
                });

            validationResults.Should().BeEmpty();
            isObjectValid.Should().BeTrue();

            _mockTokenManager.VerifyAll();
        }

        [Fact]
        public void Authenticate_Should_SendBadRequest_When_ModelState_IsNotValid()
        {
            // Arrange
            _controller.ModelState.AddModelError("errorKey", "errorMessage");

            // Act
            var actual = _controller.Authenticate(It.IsAny<AuthenticateRequest>());

            // Assert
            actual.Should().BeOfType<ActionResult<AuthenticateResponse>>();

            actual.Result.Should().BeAssignableTo<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void Authenticate_Should_ThrowException_When_AuthenticateRequest_Username_IsNullOrWhiteSpace(string username)
        {
            // Arrange         
            var authenticateRequest = new AuthenticateRequest
            {
                Username = username
            };

            // Act
            var (isObjectValid, validationResults) = TryValidateModel(authenticateRequest);

            // Assert
            validationResults.Should().NotBeEmpty()
                .And.Contain(c => c.MemberNames.Any(a => a == nameof(AuthenticateRequest.Username)));

            isObjectValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("BadUser")]
        [InlineData("   BadUser   ")]
        [InlineData("BaDUseR")]
        public void Authenticate_Should_Return_Unauthorized_When_AuthenticateRequest_Username_IsBadUser(string username)
        {
            // Arrange         
            var authenticateRequest = new AuthenticateRequest
            {
                Username = username
            };

            // Act
            var actual = _controller.Authenticate(authenticateRequest);

            // Assert
            actual.Should().BeOfType<ActionResult<AuthenticateResponse>>()
                .Which.Result.Should().BeAssignableTo<UnauthorizedResult>();
        }

        [Fact]
        public void Authenticate_Method_Should_BeMarked_AllowAnonymousAttribute()
        {
            // Arrange
            var methodsWithAttribute = _controller
                .GetType()
                .GetMethods()
                .Where(w => w.Name == nameof(_controller.Authenticate))
                .Select(s => s.GetCustomAttribute<AllowAnonymousAttribute>(false))
                .Where(w => w is not null);

            // Assert
            methodsWithAttribute.Should().HaveCountGreaterOrEqualTo(1,
                $"at least one method should be marked {nameof(AllowAnonymousAttribute)}");
        }

        private static (bool isObjectValid, IEnumerable<ValidationResult> validationResults) TryValidateModel(
            AuthenticateRequest authenticateRequest)
        {
            var validationContext = new ValidationContext(authenticateRequest);

            var validationResults = new List<ValidationResult>();

            var isObjectValid = Validator.TryValidateObject(authenticateRequest,
                validationContext,
                validationResults,
                true);

            return (isObjectValid, validationResults);
        }
    }
}
