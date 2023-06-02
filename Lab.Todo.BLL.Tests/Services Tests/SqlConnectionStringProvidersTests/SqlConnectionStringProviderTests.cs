using System;
using System.Collections.Generic;
using FluentAssertions;
using Lab.Todo.BLL.Services.SqlConnectionStringProviders;
using Lab.Todo.BLL.Services.SqlConnectionStringProviders.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Lab.Todo.BLL.Tests.ServicesTests.SqlConnectionStringProvidersTests
{
    public class SqlConnectionStringProviderTests
    {
        private readonly ILogger<SqlConnectionStringProvider> _mockLogger;

        public SqlConnectionStringProviderTests()
        { 
            _mockLogger = Mock.Of<ILogger<SqlConnectionStringProvider>>();
        }

        [Fact]
        public void Should_ProvideConnectionString_When_PassedFilledSqlConnectionStringOptions()
        {
            // Arrange
            var options = new SqlConnectionStringOptions
            {
                DataSource = "Local",
                InitialCatalog = "Catalog",
                UserId = "admin",
                Password = "12345kappa"
            };
            var stringProvider = new SqlConnectionStringProvider(Options.Create(options), _mockLogger);

            // Act
            var resultingConnectionString = stringProvider.GetSqlDatabaseConnectionString();

            // Assert
            var expectedConnectionString = $"Data Source={options.DataSource};Initial Catalog={options.InitialCatalog};Integrated Security=False;User ID={options.UserId};Password={options.Password}";
            resultingConnectionString.Should().BeEquivalentTo(expectedConnectionString);
        }

        [Theory]
        [MemberData(nameof(GetSqlConnectionStringOptions_UserID_Password))]
        public void Should_ProvideConnectionString_IntegratedSecurityTrue_When_PassedInvalidOrMissing_UserID_AndOr_Password(SqlConnectionStringOptions options)
        {
            var stringProvider = new SqlConnectionStringProvider(Options.Create(options), _mockLogger);

            // Act
            var connectionString = stringProvider.GetSqlDatabaseConnectionString();

            // Assert
            var expectedConnectionString = $"Data Source={options.DataSource};Initial Catalog={options.InitialCatalog};Integrated Security=true";
            connectionString.Should().BeEquivalentTo(expectedConnectionString);
        }

        public static IEnumerable<object[]> GetSqlConnectionStringOptions_UserID_Password()
        {
            yield return new object[] { new SqlConnectionStringOptions { DataSource = "Local", InitialCatalog = "Catalog", UserId = string.Empty, Password = "12345kappa" } };
            yield return new object[] { new SqlConnectionStringOptions { DataSource = "Local", InitialCatalog = "Catalog", UserId = "", Password = "12345kappa" } };
            yield return new object[] { new SqlConnectionStringOptions { DataSource = "Local", InitialCatalog = "Catalog", UserId = null, Password = "12345kappa" } };
            yield return new object[] { new SqlConnectionStringOptions { DataSource = "Local", InitialCatalog = "Catalog", UserId = "admin", Password = string.Empty } };
            yield return new object[] { new SqlConnectionStringOptions { DataSource = "Local", InitialCatalog = "Catalog", UserId = "admin", Password = "" } };
            yield return new object[] { new SqlConnectionStringOptions { DataSource = "Local", InitialCatalog = "Catalog", UserId = "admin", Password = null } };
        }

        [Theory]
        [MemberData(nameof(GetSqlConnectionStringOptions_DataSource))]
        public void Should_ThrowArgumentExceptions_When_PassedInvalidOrMissing_DataSource(SqlConnectionStringOptions options)
        {
            // Arrange
            var stringProvider = new SqlConnectionStringProvider(Options.Create(options), _mockLogger);

            // Act
            Action provideConnectionString = () => stringProvider.GetSqlDatabaseConnectionString();

            // Assert
            provideConnectionString.Should().Throw<ArgumentException>()
                .WithMessage("Data Source for connection string can't be empty or null.");
        }

        public static IEnumerable<object[]> GetSqlConnectionStringOptions_DataSource()
        {
            yield return new object[] { new SqlConnectionStringOptions { DataSource = string.Empty, InitialCatalog = "Catalog", UserId = "admin", Password = "12345kappa" } };
            yield return new object[] { new SqlConnectionStringOptions { DataSource = "", InitialCatalog = "Catalog", UserId = "admin", Password = "12345kappa" } };
            yield return new object[] { new SqlConnectionStringOptions { DataSource = null, InitialCatalog = "Catalog", UserId = "admin", Password = "12345kappa" } };
        }

        [Theory]
        [MemberData(nameof(GetSqlConnectionStringOptions_InitialCatalog))]
        public void Should_ThrowArgumentExceptions_When_PassedInvalidOrMissing_InitialCatalog(SqlConnectionStringOptions options)
        {
            // Arrange
            var stringProvider = new SqlConnectionStringProvider(Options.Create(options), _mockLogger);

            // Act
            Action provideConnectionString = () => stringProvider.GetSqlDatabaseConnectionString();

            // Assert
            provideConnectionString.Should().Throw<ArgumentException>()
                .WithMessage("Initial Catalog for connection string can't be empty or null.");
        }

        public static IEnumerable<object[]> GetSqlConnectionStringOptions_InitialCatalog()
        {
            yield return new object[] { new SqlConnectionStringOptions { DataSource = "Local", InitialCatalog = "", UserId = "admin", Password = "12345kappa" } };
            yield return new object[] { new SqlConnectionStringOptions { DataSource = "Local", InitialCatalog = string.Empty, UserId = "admin", Password = "12345kappa" } };
            yield return new object[] { new SqlConnectionStringOptions { DataSource = "Local", InitialCatalog = null, UserId = "admin", Password = "12345kappa" } };
        }
    }
}