using AutoMapper;
using Lab.Todo.Api.MappingProfiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Lab.Todo.Api.Tests.Mapping_Tests
{
    public class MapperProfilesTests
    {
        [Fact]
        public void Should_APIMappingProfilesConfigurationValid()
        {
            // Arrange
            MapperConfiguration config = new(cfg =>
            {
                cfg.AddProfile<AttachmentProfile>();
                cfg.AddProfile<CustomFieldProfile>();
                cfg.AddProfile<TagProfile>();
                cfg.AddProfile<ToDoItemProfileApi>();
            });

            // Act, Assert
            config.AssertConfigurationIsValid();
        }
    }
}
