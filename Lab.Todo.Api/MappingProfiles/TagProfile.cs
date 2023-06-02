using AutoMapper;
using Lab.Todo.Api.DTOs.Responses;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;

namespace Lab.Todo.Api.MappingProfiles
{
    public class TagProfile : Profile
    {
        public TagProfile()
        {
            CreateMap<Tag, TagGetResponse>();
        }
    }
}