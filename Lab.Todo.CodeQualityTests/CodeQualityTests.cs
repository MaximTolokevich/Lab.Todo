using FluentAssertions;
using Lab.Todo.CodeQualityTests.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Xunit;

namespace Lab.Todo.CodeQualityTests
{
    public class CodeQualityTests : IClassFixture<CodeQualityTestsFixture>
    {
        private readonly CodeQualityTestsFixture _codeQualityTestsFixture;

        public CodeQualityTests(CodeQualityTestsFixture codeQualityTestsFixture)
        {
            _codeQualityTestsFixture = codeQualityTestsFixture;
        }

        [Theory]
        [InlineData("Lab.Todo.sln")]
        public void Solution_Code_Should_Comply_With_The_Rules_Of_ReSharper(string solutionFileName)
        {
            // Arrange
            var solutionFilePath = GetFilePath($"*{solutionFileName}");
            var profilePath = GetFilePath("*.DotSettings");
            var outputFilePath = _codeQualityTestsFixture.OutputFilePath;

            var command =
                $"tool run jb inspectcode --no-build --output={outputFilePath} --profile={profilePath} --format=Xml {solutionFilePath}";

            // Act
            ProcessPerformer.Perform("dotnet", command);

            var codeIssuesList = GetCodeIssuesFromOutputFile(outputFilePath);

            // Assert
            var codeIssuesString = string.Join(Environment.NewLine, codeIssuesList);

            codeIssuesString.Should()
                .BeEmpty();
        }

        private static IEnumerable<string> GetCodeIssuesFromOutputFile(string outputFilePath)
        {
            var xmlDocument = XDocument.Load(outputFilePath);

            var codeIssues = xmlDocument.XPathSelectElements("Report")
                .SelectMany(report => report.XPathSelectElements("Issues"))
                .SelectMany(issues => issues.XPathSelectElements("Project"))
                .SelectMany(project => project.XPathSelectElements("Issue"))
                .Select(issue => issue.ToString());

            return codeIssues;
        }

        private static string GetFilePath(string searchPattern)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var directory = new DirectoryInfo(currentDirectory);

            while (directory is not null)
            {
                var directoryFiles = directory.GetFiles(searchPattern);

                if (directoryFiles.Any())
                {
                    return directoryFiles[0].FullName;
                }

                directory = directory.Parent;
            }

            throw new ApplicationException("The file does not exist.");
        }
    }
}