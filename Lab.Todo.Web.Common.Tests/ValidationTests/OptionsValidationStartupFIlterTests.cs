using FluentAssertions;
using Lab.Todo.BLL.Services.TokenHandler.Options;
using Lab.Todo.Web.Common.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Lab.Todo.Web.Common.Tests.ValidationTests
{
    public class OptionsValidationStartupFilterTests
    {
        private readonly Mock<IApplicationBuilder> _mockApplicationBuilder;
        private readonly IOptions<JwtOptions> _jwtOptions;
        private readonly Action _validationAction;

        public OptionsValidationStartupFilterTests()
        {
            _mockApplicationBuilder = new Mock<IApplicationBuilder>();
            var startupFilter = new OptionsValidationStartupFilter();

            _jwtOptions = Options.Create(new JwtOptions
            {
                SecretValue = new string('s', 16),
                EncryptionAlgorithms = new List<string> { "ALG" },
                TokenExpirationTime = TimeSpan.FromDays(2)
            });

            var configureOptions = new ConfigureOptions<JwtOptions>(act =>
            {
                act.SecretValue = _jwtOptions.Value.SecretValue;
                act.EncryptionAlgorithms = _jwtOptions.Value.EncryptionAlgorithms;
                act.TokenExpirationTime = _jwtOptions.Value.TokenExpirationTime;
            });

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(configureOptions);

            _mockApplicationBuilder.Setup(builder => builder.ApplicationServices.GetService(typeof(IServiceCollection)))
                .Returns(serviceCollection)
                .Verifiable();

            _mockApplicationBuilder.Setup(builder => builder.ApplicationServices.GetService(typeof(IOptions<JwtOptions>)))
                .Returns(_jwtOptions)
                .Verifiable();

            Action<IApplicationBuilder> builderAction = _ => { };

            var configuredAction = startupFilter.Configure(builderAction);

            _validationAction = () => configuredAction.Invoke(_mockApplicationBuilder.Object);
        }

        [Fact]
        public void Should_Not_Throw_Exception_When_Model_Is_Valid()
        {
            // Assert
            _validationAction.Should()
                .NotThrow();

            _mockApplicationBuilder.VerifyAll();
        }

        [Fact]
        public void Should_Throw_OptionsValidationException_When_Model_Is_Not_Valid()
        {
            // Arrange
            _jwtOptions.Value.SecretValue = new string('s', 10);
            _jwtOptions.Value.EncryptionAlgorithms = new List<string>();

            // Assert
            _validationAction.Should()
                .Throw<OptionsValidationException>()
                .Which.OptionsType.Should()
                .Be(typeof(JwtOptions));

            _mockApplicationBuilder.VerifyAll();
        }
    }
}