using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace Lab.Todo.Api.Tests.EntityTests
{
    public class EntityNamingTests
    {
        private readonly string _pascalCaseRegexString = @"^[A-Z][a-z]+(?:[A-Z][a-z]+)*$";

        [Theory]
        [MemberData(nameof(GetRequestDTOsNames))]
        public void RequestDTONamedCorrectly(Type model)
        {
            // Assert
            model.Name.Should().MatchRegex(_pascalCaseRegexString);
            model.Name.Should().EndWith("Request");
        }

        [Theory]
        [MemberData(nameof(GetResponseDTOsNames))]
        public void ResponseDTONamedCorrectly(Type model)
        {
            // Assert
            model.Name.Should().MatchRegex(_pascalCaseRegexString);
            model.Name.Should().EndWith("Response");
        }

        public static IEnumerable<object[]> GetRequestDTOsNames()
        {
            var namespaceName = "Lab.Todo.Api.DTOs.Requests";
            var requests = from t in Assembly.GetAssembly(typeof(Program))?.GetTypes()
                           where t.IsClass && t.Namespace == namespaceName
                           select t;

            foreach (var request in requests)
            {
                yield return new object[] { request };
            }
        }

        public static IEnumerable<object[]> GetResponseDTOsNames()
        {
            var namespaceName = "Lab.Todo.Api.DTOs.Responses";
            var responses = from t in Assembly.GetAssembly(typeof(Program))?.GetTypes()
                            where t.IsClass && t.Namespace == namespaceName
                            select t;

            foreach (var response in responses)
            {
                yield return new object[] { response };
            }
        }
    }
}
