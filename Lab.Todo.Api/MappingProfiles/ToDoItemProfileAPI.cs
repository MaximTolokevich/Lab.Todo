using AutoMapper;
using Lab.Todo.Api.DTOs.Queries;
using Lab.Todo.Api.DTOs.Requests;
using Lab.Todo.Api.DTOs.Responses;
using Lab.Todo.BLL.Services.ToDoItemManagers.Enums;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;

namespace Lab.Todo.Api.MappingProfiles
{
    public class ToDoItemProfileApi : Profile
    {
        public ToDoItemProfileApi()
        {
            CreateMap<ToDoItemCreateRequest, ToDoItemCreateData>();

            CreateMap<ToDoItemUpdateRequest, ToDoItemUpdateData>();

            CreateMap<ToDoItem, ToDoItemGetResponse>();

            CreateMap<ToDoItem, ParentTaskGetResponse>();

            CreateMap<ToDoItemDependency, ToDoItemDependencyGetResponse>();

            CreateMap<int, ToDoItemDependencyCreateUpdateData>()
                .ForMember(dest => dest.ToDoItemId, opt => opt.MapFrom(src => src));

            CreateMap<ToDoItemDependency, int>()
                .ConstructUsing(src => src.ToDoItemId);

            CreateMap<ToDoItemStatus, ToDoItemStatusGetResponse>()
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src));

            CreateMap<ToDoItem, ToDoItemCreateResponse>();

            CreateMap<ToDoItem, ToDoItemUpdateResponse>();

            CreateMap<ToDoItemSearchQuery, SearchOptions>()
                .ForMember(dest => dest.DurationFrom, opt => opt.MapFrom(src => src.Duration.From))
                .ForMember(dest => dest.DurationTo, opt => opt.MapFrom(src => src.Duration.To))
                .ForMember(dest => dest.ActualStartTimeFrom, opt => opt.MapFrom(src => src.ActualStartTime.From))
                .ForMember(dest => dest.ActualStartTimeTo, opt => opt.MapFrom(src => src.ActualStartTime.To))
                .ForMember(dest => dest.ActualEndTimeFrom, opt => opt.MapFrom(src => src.ActualEndTime.From))
                .ForMember(dest => dest.ActualEndTimeTo, opt => opt.MapFrom(src => src.ActualEndTime.To))
                .ForMember(dest => dest.DeadlineFrom, opt => opt.MapFrom(src => src.Deadline.From))
                .ForMember(dest => dest.DeadlineTo, opt => opt.MapFrom(src => src.Deadline.To))
                .ForMember(dest => dest.PlannedStartTimeFrom, opt => opt.MapFrom(src => src.PlannedStartTime.From))
                .ForMember(dest => dest.PlannedStartTimeTo, opt => opt.MapFrom(src => src.PlannedStartTime.To));
        }
    }
}