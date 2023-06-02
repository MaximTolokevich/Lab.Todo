using AutoMapper;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;
using Lab.Todo.Web.Models;

namespace Lab.Todo.Web.MappingProfiles
{
    public class TagProfile : Profile
    {
        public TagProfile()
        {
            CreateMap<Tag, TagModel>();
        }
    }
}