using System;
using System.Collections.Generic;
using System.Linq;
using Lab.Todo.BLL.Services.ToDoItemManagers.Models;

namespace Lab.Todo.BLL.Services.ToDoItemManagers.Builders
{
    public static class ToDoItemBuilderExtension
    {
        public static IToDoItemQueryBuilder ByTitle(this IToDoItemQueryBuilder builder, string title)
        {
            if (!string.IsNullOrEmpty(title))
            {
                builder.AddFilter(item => item.Title.Contains(title));
            }

            return builder;
        }

        public static IToDoItemQueryBuilder ByCreatorOrPerformer(this IToDoItemQueryBuilder builder, string userName)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                builder.AddFilter(item => item.CreatedBy == userName || item.AssignedTo == userName);
            }

            return builder;
        }

        public static IToDoItemQueryBuilder ByDescription(this IToDoItemQueryBuilder builder, string description)
        {
            if (!string.IsNullOrEmpty(description))
            {
                builder.AddFilter(item => item.Description.Contains(description));
            }

            return builder;
        }

        public static IToDoItemQueryBuilder ByParentId(this IToDoItemQueryBuilder builder, int? parentId)
        {
            if (parentId != null)
            {
                builder.AddFilter(item => item.ParentTaskId == parentId);
            }

            return builder;
        }

        public static IToDoItemQueryBuilder ByStatuses(this IToDoItemQueryBuilder builder, IEnumerable<int> statusesIds)
        {
            var statusesIdsArray = statusesIds as int[] ?? statusesIds.ToArray();

            if (statusesIdsArray.Any())
            {
                builder.AddFilter(item => statusesIdsArray.Contains(item.ToDoItemStatusId));
            }

            return builder;
        }

        public static IToDoItemQueryBuilder ByDuration(this IToDoItemQueryBuilder builder, TimeSpan? durationFrom, TimeSpan? durationTo)
        {
            if (durationFrom != null)
            {
                builder.AddFilter(item => item.Duration >= durationFrom);
            }

            if (durationTo != null)
            {
                builder.AddFilter(item => item.Duration <= durationTo);
            }

            return builder;
        }

        public static IToDoItemQueryBuilder ByActualStartTime(this IToDoItemQueryBuilder builder, DateTime? actualStartTimeFrom, DateTime? actualStartTimeTo)
        {
            if (actualStartTimeFrom != null)
            {
                builder.AddFilter(item => item.ActualStartTime >= actualStartTimeFrom);
            }

            if (actualStartTimeTo != null)
            {
                builder.AddFilter(item => item.ActualStartTime <= actualStartTimeTo);
            }

            return builder;
        }

        public static IToDoItemQueryBuilder ByActualEndTime(this IToDoItemQueryBuilder builder, DateTime? actualEndTimeFrom, DateTime? actualEndTimeTo)
        {
            if (actualEndTimeFrom != null)
            {
                builder.AddFilter(item => item.ActualEndTime >= actualEndTimeFrom);
            }

            if (actualEndTimeTo != null)
            {
                builder.AddFilter(item => item.ActualEndTime <= actualEndTimeTo);
            }

            return builder;
        }

        public static IToDoItemQueryBuilder ByTags(this IToDoItemQueryBuilder builder, IEnumerable<IEnumerable<string>> tags)
        {
            foreach (var tagList in tags)
            {
                builder.AddFilter(item => item.ToDoItemTagAssociations.Any(tagAssociations => tagList.Contains(tagAssociations.Tag.Value)));
            }

            return builder;
        }

        public static IToDoItemQueryBuilder ByCustomFields(this IToDoItemQueryBuilder builder, IEnumerable<CustomFieldsSearchOptionsBase> customFields)
        {
            if (customFields == null)
            {
                return builder;
            }


            foreach (var customField in customFields)
            {
                builder = builder.AddFilter(item => item.CustomFields.Any(field => field.Name == customField.Name));

                builder = customField switch
                {
                    CustomFieldsSearchOptions<int?> optionsBase =>
                        builder.AddFilter(item => item.CustomFields.Any(field => optionsBase.From == null || (field.IntValue != null && field.IntValue >= optionsBase.From)))
                              .AddFilter(item => item.CustomFields.Any(field => optionsBase.To == null || field.IntValue <= optionsBase.To)),

                    CustomFieldsSearchOptions<string> optionsBase =>
                        builder.AddFilter(item => item.CustomFields.Any(field => string.IsNullOrEmpty(optionsBase.Text) || !string.IsNullOrEmpty(field.StringValue) && field.StringValue.Contains(optionsBase.Text))),

                    CustomFieldsSearchOptions<DateTime?> optionsBase =>
                        builder.AddFilter(item => item.CustomFields.Any(field => optionsBase.From == null || field.DateTimeValue != null && field.DateTimeValue >= optionsBase.From))
                              .AddFilter(item => item.CustomFields.Any(field => optionsBase.To == null || field.DateTimeValue <= optionsBase.To)),

                    _ => throw new ArgumentException("Invalid filter options", nameof(customFields)),
                };
            }

            return builder;
        }

        public static IToDoItemQueryBuilder ByDeadLine(this IToDoItemQueryBuilder builder, DateTime? deadlineFrom,
            DateTime? deadlineTo)
        {
            if (deadlineFrom != null)
            {
                builder.AddFilter(item => item.Deadline >= deadlineFrom);
            }

            if (deadlineTo != null)
            {
                builder.AddFilter(item => item.Deadline <= deadlineTo);
            }

            return builder;
        }

        public static IToDoItemQueryBuilder ByPlannedStartTime(this IToDoItemQueryBuilder builder, DateTime? plannedStartTimeFrom, DateTime? plannedStartTimeTo)
        {
            if (plannedStartTimeFrom != null)
            {
                builder.AddFilter(item => item.PlannedStartTime >= plannedStartTimeFrom);
            }

            if (plannedStartTimeTo != null)
            {
                builder.AddFilter(item => item.PlannedStartTime <= plannedStartTimeTo);
            }

            return builder;
        }
    }
}