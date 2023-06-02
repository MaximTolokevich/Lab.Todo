using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Lab.Todo.BLL.Services.ToDoItemManagers.Enums;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;
using Lab.Todo.Web.Models;

namespace Lab.Todo.Web.MappingProfiles
{
    public class ToDoItemProfile : Profile
    {
        public ToDoItemProfile()
        {
            CreateMap<ToDoItemCreateModel, ToDoItemCreateData>()
                .ForMember(dest => dest.DependsOnItems, opt => opt.MapFrom(src => src.DependencyViewModel.DependsOnItems))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.TagViewModel.Tags))
                .ForMember(dest => dest.DependsOnItems, opt => opt.MapFrom(src => src.DependencyViewModel.DependsOnItems));


            CreateMap<int, ToDoItemDependencyCreateUpdateData>()
                .ForMember(dest => dest.ToDoItemId, opt => opt.MapFrom(src => src));

            CreateMap<ToDoItemUpdateModel, ToDoItemUpdateData>()
                .ForMember(dest => dest.DependsOnItems, opt => opt.MapFrom(src => src.DependencyViewModel.DependsOnItems))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.TagViewModel.Tags));

            CreateMap<ToDoItem, ToDoItemViewModel>()
                .ForMember(dest => dest.DependsOnItems, opt => opt.MapFrom(src => src.DependsOnItems))
                .ForMember(dest => dest.ParentTask, opt => opt.MapFrom(src => src.ParentTask));

            CreateMap<ToDoItem, ParentTaskViewModel>();

            CreateMap<ToDoItemDependency, string>()
                .ConstructUsing(src => src.ToDoItemTitle);

            CreateMap<ToDoItem, ToDoItemUpdateModel>()
                .ForPath(dest => dest.DependencyViewModel.DependsOnItems, opt => opt.MapFrom(src => src.DependsOnItems))
                .ForPath(dest => dest.TagViewModel.Tags, opt => opt.MapFrom(src => src.Tags))
                .ForPath(dest => dest.StatusModel.CurrentStatus, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForPath(dest => dest.StatusModel.StatusSuggestions, opt => opt.MapFrom(src => GetStatusSuggestions()));

            CreateMap<ToDoItemStatus, ToDoItemStatusModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src));

            CreateMap<ToDoItem, DependencyModel>()
                .ForMember(dest => dest.ToDoItemId, opt => opt.MapFrom(src => src.Id));

            CreateMap<ToDoItemDependency, int>().ConstructUsing(src => src.ToDoItemId);

            CreateMap<ToDoItemSearchModel, SearchOptions>();
        }

        private static IEnumerable<string> GetStatusSuggestions()
        {
            var statusSuggestions = Enum.GetNames(typeof(ToDoItemStatus)).ToList();

            return statusSuggestions;
        }
    }
}