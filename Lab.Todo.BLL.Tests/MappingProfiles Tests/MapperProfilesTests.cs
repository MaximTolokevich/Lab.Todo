using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Lab.Todo.BLL.MappingProfiles;
using Lab.Todo.DAL.Repositories;
using Xunit;

namespace Lab.Todo.BLL.Tests.MappingProfiles_Tests
{
    public class MapperProfilesTests
    {
        [Fact]
        public void Should_BLLMappingProfilesConfigurationValid()
        {
            // Arrange
            MapperConfiguration config = new (cfg =>
            {
                cfg.AddProfile<AttachmentProfile>();
                cfg.AddProfile<CustomFieldProfile>();
                cfg.AddProfile<TagProfile>();
                cfg.AddProfile<ToDoItemDependencyProfile>();
                cfg.AddProfile<ToDoItemProfile>();
            });

            // Act, Assert
            config.AssertConfigurationIsValid();
        }

    }
}
