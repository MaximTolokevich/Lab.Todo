using AutoMapper;
using Lab.Todo.Web.MappingProfiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Lab.Todo.Web.Tests.Mapping_profiles_tests
{
    public class MapperProfilesTests
    {
        [Fact]
        public void Should_WebMappingProfilesConfigurationValid()
        {
            // Assert
            MapperConfiguration config = new(cfg =>
            {
                cfg.AddProfile<AttachmentProfile>();
                cfg.AddProfile<CustomFieldProfile>();
                cfg.AddProfile<TagProfile>();
                cfg.AddProfile<ToDoItemProfile>();
            });

            // Act, Assert
            config.AssertConfigurationIsValid();
        }
    }
}
