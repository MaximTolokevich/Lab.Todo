using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace Lab.Todo.Web.Tests.ModelTests
{
    public class ModelNamingTests
    {
        private readonly string _pascalCaseRegexString = @"^[A-Z][a-z]+(?:[A-Z][a-z]+)*$";

        [Theory]
        [MemberData(nameof(GetModelNames))]
        public void ModelNamedCorrectly(Type model)
        {
            // Assert
            model.Name.Should().MatchRegex(_pascalCaseRegexString);
            model.Name.Should().EndWith("Model");
        }

        public static IEnumerable<object[]> GetModelNames()
        {
            var namespaceName = "Lab.Todo.Web.Models";
            var models = from t in Assembly.GetAssembly(typeof(Program))?.GetTypes()
                         where t.IsClass && t.Namespace == namespaceName && !t.IsNested
                         select t;

            foreach (var model in models)
            {
                yield return new object[] { model };
            }
        }
    }
}
