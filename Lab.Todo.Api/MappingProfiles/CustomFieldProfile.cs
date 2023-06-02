using AutoMapper;
using Lab.Todo.Api.DTOs.Requests;
using Lab.Todo.Api.DTOs.Responses;
using Lab.Todo.Api.Models;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;
using Lab.Todo.Web.Common.MappingHelpers;
using System;
using System.Text.Json;

namespace Lab.Todo.Api.MappingProfiles
{
    public class CustomFieldProfile : Profile
    {
        public CustomFieldProfile()
        {
            CreateMap<CustomFieldCreateRequest, CustomFieldBase>()
                .ConstructUsing(src => ConstructGenericICustomFieldClass(src))
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<CustomFieldUpdateRequest, CustomFieldBase>()
                .ConstructUsing(src => ConstructGenericICustomFieldClass(src));
            
            CreateMap<CustomFieldSearchModel, CustomFieldsSearchOptionsBase>()
                .ConstructUsing(src => ConstructGenericCustomFieldSearchOptions(src))
                .ForMember(dest => dest.Text, opt => opt.Ignore());
            
            CreateMap<CustomFieldBase, CustomFieldGetResponse>()
                .AfterMap(AfterMapICustomFieldToCustomFieldGetResponse)
                .ForMember(dest => dest.Type, opt => opt.Ignore())
                .ForMember(dest => dest.Value, opt => opt.Ignore());
        }

        private static void MapProperties<T>(CustomField<T> source, CustomFieldTypes type,
            CustomFieldGetResponse destination)
        {
            destination.Value = source.Value;
            destination.Type = type;
        }

        private static void AfterMapICustomFieldToCustomFieldGetResponse(CustomFieldBase source, CustomFieldGetResponse destination)
        {
            switch (source)
            {
                case CustomField<int> customField:
                    MapProperties(customField, CustomFieldTypes.Number, destination);
                    break;
                case CustomField<DateTime> customField:
                    MapProperties(customField, CustomFieldTypes.DateTime, destination);
                    break;
                case CustomField<string> customField:
                    MapProperties(customField, CustomFieldTypes.Text, destination);
                    break;
                default:
                    throw new ApplicationException($"Passed an unexpected CustomField<> type - {source.GetType()}.");
            }
        }

        private static CustomFieldsSearchOptionsBase ConstructGenericCustomFieldSearchOptions(CustomFieldSearchModel source)
        {
            CustomFieldsSearchOptionsBase customFieldsSearchOptions = source.Type switch
            {
                CustomFieldTypes.Number => new CustomFieldsSearchOptions<int?>
                {
                    From = ((JsonElement?)source.Value.From)?.GetInt32(),
                    To = ((JsonElement?)source.Value.To)?.GetInt32()
                },

                CustomFieldTypes.DateTime => new CustomFieldsSearchOptions<DateTime?>
                {
                    From = ((JsonElement?)source.Value.From)?.GetDateTime(),
                    To = ((JsonElement?)source.Value.To)?.GetDateTime()
                },

                CustomFieldTypes.Text => new CustomFieldsSearchOptions<string>
                {
                    From = source.Value.From?.ToString(),
                    To = source.Value.To?.ToString()
                },

                _ => throw new ApplicationException("Passed an invalid CustomFieldType enum value.")
            };

            customFieldsSearchOptions.Name = source.Name;

            return customFieldsSearchOptions;
        }

        private static CustomFieldBase ConstructGenericICustomFieldClass(CustomFieldCreateRequest source)
        {
            CustomFieldBase customField = source.Type switch
            {
                CustomFieldTypes.Number => new CustomField<int>
                    { Value = ((JsonElement)source.Value).GetInt32() },
                CustomFieldTypes.DateTime => new CustomField<DateTime>
                    { Value = ((JsonElement)source.Value).GetDateTime() },
                CustomFieldTypes.Text => new CustomField<string>
                    { Value = source.Value.ToString() },
                _ => throw new ApplicationException("Passed an invalid CustomFieldType enum value.")
            };

            customField.Name = source.Name;
            customField.Order = source.Order;

            return customField;
        }

        private static CustomFieldBase ConstructGenericICustomFieldClass(CustomFieldUpdateRequest source)
        {
            CustomFieldBase customField = source.Type switch
            {
                CustomFieldTypes.Number => new CustomField<int>
                    { Value = ((JsonElement)source.Value).GetInt32() },
                CustomFieldTypes.DateTime => new CustomField<DateTime>
                    { Value = ((JsonElement)source.Value).GetDateTime() },
                CustomFieldTypes.Text => new CustomField<string>
                    { Value = source.Value.ToString() },
                _ => throw new ApplicationException("Passed an invalid CustomFieldType enum value.")
            };

            customField.Name = source.Name;
            customField.Order = source.Order;
            customField.Id = source.Id;

            return customField;
        }
    }
}