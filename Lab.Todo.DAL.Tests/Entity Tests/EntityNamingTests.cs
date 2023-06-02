using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Lab.Todo.DAL.Repositories;
using Xunit;

namespace Lab.Todo.DAL.Tests.EntityTests
{
    public class EntityNamingTests
    {
        private const string PascalCaseRegexString = @"^[A-Z][a-z]+(?:[A-Z][a-z]+)*$";

        [Theory]
        [MemberData(nameof(GetEntityNames))]
        public void EntityNamedCorrectly(Type entity)
        {
            // Arrange
            var tableAttribute = (TableAttribute)entity.GetCustomAttribute(typeof(TableAttribute));

            // Assert
            entity.Name.Should().MatchRegex(PascalCaseRegexString);
            if (tableAttribute is not null)
            {
                entity.Name.Should().StartWith(tableAttribute.Name);
            }
            entity.Name.Should().EndWith("DbEntry");
        }

        public static IEnumerable<object[]> GetEntityNames()
        {
            const string namespaceName = "Lab.Todo.DAL.Entities";
            var entities = from t in Assembly.GetAssembly(typeof(ToDoUnitOfWork))?.GetTypes()
                           where t.IsClass && t.Namespace == namespaceName
                           select t;

            foreach (var entity in entities)
            {
                yield return new object[] { entity };
            }
        }
    }
}
