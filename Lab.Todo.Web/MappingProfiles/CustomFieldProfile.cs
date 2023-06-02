using System;
using AutoMapper;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;
using Lab.Todo.Web.Common.MappingHelpers;
using Lab.Todo.Web.Models;

namespace Lab.Todo.Web.MappingProfiles
{
    public class CustomFieldProfile : Profile
    {
        public CustomFieldProfile()
        {
            CreateMap<CustomFieldCreateModel, CustomFieldBase>()
                .ConstructUsing(src => ConstructGenericICustomFieldClass(src))
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<CustomFieldUpdateModel, CustomFieldBase>()
                .ConstructUsing(src => ConstructGenericICustomFieldClass(src));
            
            CreateMap<CustomFieldSearchModel, CustomFieldsSearchOptionsBase>()
                .ConstructUsing(src => ConstructGenericCustomFieldSearchOptions(src));

            CreateMap<CustomFieldBase, CustomFieldViewModel>()
                .ForMember(dest => dest.Type, opt => opt.Ignore())
                .ForMember(dest => dest.Value, opt => opt.Ignore())
                .AfterMap(AfterMapICustomFieldToCustomFieldViewModel);

            CreateMap<CustomFieldBase, CustomFieldUpdateModel>()
                .ForMember(dest => dest.Type, opt => opt.Ignore())
                .ForMember(dest => dest.Value, opt => opt.Ignore())
                .AfterMap(AfterMapICustomFieldToCustomFieldViewModel);
        }

        private static void MapProperties<T>(CustomField<T> source, CustomFieldTypes type,
            CustomFieldViewModel destination)
        {
            destination.Value = source.Value;
            destination.Type = type.ToString();
        }

        private static void MapProperties<T>(CustomField<T> source, CustomFieldTypes type,
            CustomFieldUpdateModel destination)
        {
            destination.Value = source.Value;
            destination.Type = type;
        }

        private static CustomFieldsSearchOptionsBase ConstructGenericCustomFieldSearchOptions(CustomFieldSearchModel source)
        {
            CustomFieldsSearchOptionsBase customFieldsSearchOptions = source.Type switch
            {
                CustomFieldTypes.Number => new CustomFieldsSearchOptions<int?>
                {
                    From = !string.IsNullOrEmpty(source.From) ? int.Parse(source.From) : null,
                    To = !string.IsNullOrEmpty(source.To) ? int.Parse(source.To) : null
                },

                CustomFieldTypes.DateTime => new CustomFieldsSearchOptions<DateTime?>
                {
                    From = !string.IsNullOrEmpty(source.From) ? DateTime.Parse(source.From) : null,
                    To = !string.IsNullOrEmpty(source.To) ? DateTime.Parse(source.To) : null
                },

                CustomFieldTypes.Text => new CustomFieldsSearchOptions<string>
                {
                    From = source.From,
                    To = source.To
                },

                _ => throw new ApplicationException("Passed an invalid CustomFieldType enum value.")
            };

            customFieldsSearchOptions.Name = source.Name;
            customFieldsSearchOptions.Text = source.Text;

            return customFieldsSearchOptions;
        }


        private static void AfterMapICustomFieldToCustomFieldViewModel(CustomFieldBase source, CustomFieldViewModel destination)
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

        private static void AfterMapICustomFieldToCustomFieldViewModel(CustomFieldBase source, CustomFieldUpdateModel destination)
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

        private static CustomFieldBase ConstructGenericICustomFieldClass(CustomFieldCreateModel source)
        {
            CustomFieldBase customField = source.Type switch
            {
                CustomFieldTypes.Number => new CustomField<int>
                    { Value = (int)source.Value },
                CustomFieldTypes.DateTime => new CustomField<DateTime>
                    { Value = (DateTime)source.Value },
                CustomFieldTypes.Text => new CustomField<string>
                    { Value = source.Value.ToString() },
                _ => throw new ApplicationException("Passed an invalid CustomFieldType enum value.")
            };

            customField.Order = source.Order;
            customField.Name = source.Name;

            return customField;
        }

        private static CustomFieldBase ConstructGenericICustomFieldClass(CustomFieldUpdateModel source)
        {
            return source.Type switch
            {
                CustomFieldTypes.Number => new CustomField<int> { Value = (int)source.Value },
                CustomFieldTypes.DateTime => new CustomField<DateTime> { Value = (DateTime)source.Value },
                CustomFieldTypes.Text => new CustomField<string> { Value = source.Value.ToString() },
                _ => throw new ApplicationException("Passed an invalid CustomFieldType enum value.")
            };
        }
    }
}