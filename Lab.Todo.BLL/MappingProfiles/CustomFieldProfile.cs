using System;
using AutoMapper;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;
using Lab.Todo.DAL.Entities;
using Lab.Todo.DAL.Helpers;

namespace Lab.Todo.BLL.MappingProfiles
{
    public class CustomFieldProfile : Profile
    {
        public CustomFieldProfile()
        {
            CreateMap<CustomFieldBase, CustomFieldDbEntry>()
                .ForMember(x => x.ToDoItem, x => x.Ignore())
                .ForMember(x => x.Type, x => x.Ignore())
                .ForMember(x => x.DateTimeValue, x => x.Ignore())
                .ForMember(x => x.IntValue, x => x.Ignore())
                .ForMember(x => x.StringValue, x => x.Ignore())
                .ForMember(x => x.ToDoItemId, x => x.Ignore())
                .AfterMap(ConvertICustomFieldToCustomFieldDbEntry);

            CreateMap<CustomFieldDbEntry, CustomFieldBase>()
                .ConstructUsing(src => ConstructGenericICustomFieldClass(src));
        }

        private static void MapProperties<T>(CustomField<T> source, CustomFieldDbEntryTypes type, CustomFieldDbEntry destination)
        {
            destination.Type = type;
            switch (type)
            {
                case CustomFieldDbEntryTypes.IntType:
                {
                    destination.IntValue = source.Value as int?;
                    destination.StringValue = null;
                    destination.DateTimeValue = null;
                    break;
                }
                case CustomFieldDbEntryTypes.DateTimeType:
                {
                    destination.IntValue = null;
                    destination.StringValue = null;
                    destination.DateTimeValue = source.Value as DateTime?;
                    break;
                }
                case CustomFieldDbEntryTypes.StringType:
                {
                    destination.IntValue = null;
                    destination.StringValue = source.Value as string;
                    destination.DateTimeValue = null;
                    break;
                }
                default:
                    throw new ApplicationException("Passed invalid type of CustomField to conversion method");
            }
        }

        private static void ConvertICustomFieldToCustomFieldDbEntry(CustomFieldBase source, CustomFieldDbEntry destination)
        {
            switch (source)
            {
                case CustomField<int> customField:
                    MapProperties(customField, CustomFieldDbEntryTypes.IntType, destination);
                    break;
                case CustomField<DateTime> customField:
                    MapProperties(customField, CustomFieldDbEntryTypes.DateTimeType, destination);
                    break;
                case CustomField<string> customField:
                    MapProperties(customField, CustomFieldDbEntryTypes.StringType, destination);
                    break;
                default:
                    throw new ApplicationException("Passed invalid type of CustomField to conversion method");
            }
        }

        private static CustomFieldBase ConstructGenericICustomFieldClass(CustomFieldDbEntry source)
        {
            return source.Type switch
            {
                CustomFieldDbEntryTypes.IntType => new CustomField<int> { Value = Convert.ToInt32(source.IntValue)  },
                CustomFieldDbEntryTypes.DateTimeType => new CustomField<DateTime> { Value = Convert.ToDateTime(source.DateTimeValue) },
                CustomFieldDbEntryTypes.StringType => new CustomField<string> { Value = source.StringValue },
                _ => throw new ApplicationException($"Passed an unexpected CustomField<> type - { source.GetType() }.")
            };
        }
    }
}