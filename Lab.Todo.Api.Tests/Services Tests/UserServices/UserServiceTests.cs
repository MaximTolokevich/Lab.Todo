using FluentAssertions;
using Lab.Todo.Api.Services.UserServices;
using Lab.Todo.BLL.Services.UserServices.Models;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using Xunit;

namespace Lab.Todo.Api.Tests.Services_Tests.UserServices
{
    public class UserServiceTests
    {
        private readonly Mock<HttpContext> _mockContext;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockContext = new Mock<HttpContext>();
            _mockContext.Setup(s => s.Items).Returns(new Dictionary<object, object>());

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(s => s.HttpContext).Returns(_mockContext.Object);

            _userService = new UserService(mockHttpContextAccessor.Object);
        }

        [Fact]
        public void GetCurrent_Should_Return_Built_User_From_HttpContext_When_User_Claims_ContainHisName()
        {
            // Arrange
            const string userLogin = "AnyLogin";

            var claims = new List<Claim>
            {
                new (ClaimTypes.Name, userLogin)
            };

            var expectedUser = new User
            {
                Email = userLogin,
                Name = userLogin
            };

            _mockContext.Setup(s => s.User.Claims).Returns(claims);

            // Act
            var actual = _userService.Current;

            // Assert
            actual.Should().BeEquivalentTo(expectedUser);
        }

        [Fact]
        public void GetCurrent_Should_Build_User_BaseOn_FirstName_When_Claims_Have_TwoAndMoreNames()
        {
            // Arrange
            const string userLogin = "AnyLogin1";

            var claims = new List<Claim>
            {
                new (ClaimTypes.Name, userLogin),
                new (ClaimTypes.Name, "AnyLogin2"),
                new (ClaimTypes.Name, "AnyLogin3"),
            };

            var expectedUser = new User
            {
                Email = userLogin,
                Name = userLogin
            };

            _mockContext.Setup(s => s.User.Claims).Returns(claims);

            // Act
            var actual = _userService.Current;

            // Assert
            actual.Should().BeEquivalentTo(expectedUser);
        }

        [Fact]
        public void GetCurrent_Should_ThrowException_When_Argument_Claims_IsNull()
        {
            // Arrange
            _mockContext.Setup(s => s.User.Claims).Returns((IEnumerable<Claim>)null);

            Func<User> func = () => _userService.Current;

            // Act & Assert
            func.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void GetCurrent_Should_ThrowException_When_Claims_UserName_IsNullOrWriteSpace(string userLogin)
        {
            // Arrange
            var claims = new List<Claim>
            {
                new (ClaimTypes.Name, userLogin)
            };

            _mockContext.Setup(s => s.User.Claims).Returns(claims);

            Func<User> func = () => _userService.Current;

            // Act & Assert
            func.Should()
                .Throw<BadHttpRequestException>()
                .WithMessage("Token's claims don't contain an user data.")
                .And.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        }
    }
}