using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Lab.Todo.BLL.Services.ToDoItemManagers.Enums;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;
using Lab.Todo.DAL.Entities;
using Lab.Todo.DAL.Helpers;

namespace Lab.Todo.BLL.MappingProfiles
{
    public class ToDoItemProfile : Profile
    {
        public ToDoItemProfile()
        {
            CreateMap<ToDoItemCreateData, ToDoItemDbEntry>()
                .ForMember(dest => dest.DependentItems, opt => opt.Ignore())
                .ForMember(dest => dest.ToDoItemStatusId, opt => opt.Ignore())
                .ForMember(dest => dest.ParentTask, opt => opt.Ignore())
                .ForMember(dest => dest.ActualEndTime, opt => opt.Ignore())
                .ForMember(dest => dest.ActualStartTime, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ToDoItemTagAssociations, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<ToDoItemUpdateData, ToDoItemDbEntry>()
                .ForMember(dest => dest.ToDoItemStatusId, src => src.MapFrom(data => MapTaskStatusIdFromStatusEnum(data.Status)))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ToDoItemTagAssociations, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ActualStartTime, opt => opt.Ignore())
                .ForMember(dest => dest.ActualEndTime, opt => opt.Ignore())
                .ForMember(dest => dest.ParentTask, opt => opt.Ignore())
                .ForMember(dest => dest.DependentItems, opt => opt.Ignore());

            CreateMap<ToDoItemDbEntry, ToDoItem>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.ToDoItemTagAssociations.Select(toDoItemTagAssociation => toDoItemTagAssociation.Tag.Value)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MapStatusEnumFromTaskStatusId(src.ToDoItemStatusId)))
                .ForMember(dest => dest.DependsOnItems, opt => opt.MapFrom(src => src.DependsOnItems))
                .ForMember(dest => dest.DependentItems, opt => opt.MapFrom(src => MapDependOnThisDependency(src.DependentItems)));

            CreateMap<ToDoItemStatusDbEntry, ToDoItemStatus>()
                .ConvertUsing(src => MapStatusEnumFromTaskStatusId(src.Id));

            CreateMap<ToDoItemStatus, int>()
                .ConvertUsing(src => MapTaskStatusIdFromStatusEnum(src));
        }

        private static ToDoItemStatus MapStatusEnumFromTaskStatusId(int taskStatusId)
        {
            return taskStatusId switch
            {
                ToDoItemStatusIds.Planned => ToDoItemStatus.Planned,
                ToDoItemStatusIds.InProgress => ToDoItemStatus.InProgress,
                ToDoItemStatusIds.Paused => ToDoItemStatus.Paused,
                ToDoItemStatusIds.Completed => ToDoItemStatus.Completed,
                ToDoItemStatusIds.Cancelled => ToDoItemStatus.Cancelled,
                _ => throw new ApplicationException($"Failed to map ToDoItemStatusEnum from taskStatusId [{taskStatusId}]")
            };
        }

        private static int MapTaskStatusIdFromStatusEnum(ToDoItemStatus status)
        {
            return status switch
            {
                ToDoItemStatus.Planned => ToDoItemStatusIds.Planned,
                ToDoItemStatus.InProgress => ToDoItemStatusIds.InProgress,
                ToDoItemStatus.Paused => ToDoItemStatusIds.Paused,
                ToDoItemStatus.Completed => ToDoItemStatusIds.Completed,
                ToDoItemStatus.Cancelled => ToDoItemStatusIds.Cancelled,
                _ => throw new ApplicationException($"Failed to map TaskStatusId from ToDoItemStatus enum [{status}]")
            };
        }

        private static IEnumerable<ToDoItemDependency> MapDependOnThisDependency(IEnumerable<ToDoItemDependencyDbEntry> dependencyDbEntries)
        {
            if (dependencyDbEntries is null) return null;
            var list = new List<ToDoItemDependency>();
            foreach (var dependency in dependencyDbEntries)
            {
                list.Add(new ToDoItemDependency
                {
                    ToDoItemId = dependency.ToDoItemId,
                });
            }
            return list;
        }
    }
}