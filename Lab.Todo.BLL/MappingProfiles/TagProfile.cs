using AutoMapper;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;
using Lab.Todo.DAL.Entities;

namespace Lab.Todo.BLL.MappingProfiles
{
    public class TagProfile : Profile
    {
        public TagProfile()
        {
            CreateMap<TagDbEntry, Tag>();
        }
    }
}