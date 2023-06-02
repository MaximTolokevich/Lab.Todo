using AutoMapper;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;
using Lab.Todo.DAL.Entities;

namespace Lab.Todo.BLL.MappingProfiles
{
    public class ToDoItemDependencyProfile : Profile
    {
        public ToDoItemDependencyProfile()
        {
            CreateMap<ToDoItemDependencyDbEntry, ToDoItemDependency>()
                .ForMember(dest => dest.ToDoItemId, opt => opt.MapFrom(src => src.DependsOnToDoItemId))
                .ForMember(dest => dest.ToDoItemTitle, opt => opt.Ignore());

            CreateMap<ToDoItemDependency, ToDoItemDependencyDbEntry>()
                .ForMember(dest => dest.DependsOnToDoItemId, opt => opt.MapFrom(src => src.ToDoItemId))
                .ForMember(dest => dest.ToDoItemId, opt => opt.Ignore())
                .ForMember(dest => dest.ToDoItem, opt => opt.Ignore())
                .ForMember(dest => dest.DependsOnToDoItem, opt => opt.Ignore());

            CreateMap<ToDoItemDependencyCreateUpdateData, ToDoItemDependencyDbEntry>()
                .ForMember(dest => dest.DependsOnToDoItemId, opt => opt.MapFrom(src => src.ToDoItemId))
                .ForMember(dest => dest.ToDoItemId, opt => opt.Ignore())
                .ForMember(dest => dest.ToDoItem, opt => opt.Ignore())
                .ForMember(dest => dest.DependsOnToDoItem, opt => opt.Ignore());
        }
    }
}