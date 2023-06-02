using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab.Todo.SmokeTests.DatabaseModels;
using Lab.Todo.SmokeTests.Services.Interfaces;

namespace Lab.Todo.SmokeTests.Helpers
{
    public static class DbAccessorExtensions
    {
        public static async Task<T?> GetByFilterAsync<T>(this IToDoItemsDbAccessor toDoItemsDbAccessor, string tableName, object filter)
        {
            var condition = GetConditionFromFilter(filter);

            var item = await toDoItemsDbAccessor.SelectAsync<T>($"select * from {tableName} where {condition}", filter);

            return item.SingleOrDefault();
        }

        public static async Task<IEnumerable<T>> GetAll<T>(this IToDoItemsDbAccessor toDoItemsDbAccessor)
        {
            var tableName = DatabaseInfo.GetTableName<T>();

            return await toDoItemsDbAccessor.SelectAsync<T>($"select * from {tableName}");
        }

        public static async Task<int> DeleteByFilterAsync(this IToDoItemsDbAccessor toDoItemsDbAccessor, string tableName, object filter)
        {
            var condition = GetConditionFromFilter(filter);

            return await toDoItemsDbAccessor.ExecuteAsync($"delete from {tableName} where {condition}", filter);
        }

        public static async Task<int> AddToDoItem(this IToDoItemsDbAccessor toDoItemsDbAccessor, ToDoItemDbModel toDoItem)
        {
            var sqlStringBuilder = new StringBuilder();

            string toDoItemIdColumnName;
            string toDoItemIdParameter;

            if (toDoItem.ToDoItemId != default)
            {
                sqlStringBuilder.Append("set IDENTITY_INSERT ToDoItem on;");
                toDoItemIdColumnName = "ToDoItemId, ";
                toDoItemIdParameter = $"@{toDoItemIdColumnName}";
            }
            else
            {
                toDoItemIdColumnName = toDoItemIdParameter = string.Empty;
            }

            sqlStringBuilder.Append($"insert into ToDoItem ({toDoItemIdColumnName}Title, Description, AssignedTo, CreatedBy) values " +
                                    $"({toDoItemIdParameter}@Title, @Description, @AssignedTo, @CreatedBy)");

            return await toDoItemsDbAccessor.ExecuteAsync(sqlStringBuilder.ToString(), toDoItem);
        }

        public static async Task<int> UpdateToDoItemAsync(this IToDoItemsDbAccessor toDoItemsDbAccessor, ToDoItemDbModel toDoItem) =>
            await toDoItemsDbAccessor.ExecuteAsync("update ToDoItem " +
                                          "set Title=@Title, " +
                                          "Description=@Description " +
                                          "where ToDoItemId=@ToDoItemId", toDoItem);


        public static async Task<int> AddTagsAsync(this IToDoItemsDbAccessor toDoItemsDbAccessor, IEnumerable<TagDbModel> tags)
        {
            var sqlStringBuilder = new StringBuilder();

            string tagIdColumnName;
            string tagIdParameter;

            if (tags.Any(tag => tag.TagId != default))
            {
                sqlStringBuilder.Append("set IDENTITY_INSERT Tag on; ");
                tagIdColumnName = "TagId, ";
                tagIdParameter = $"@{tagIdColumnName}";
            }
            else
            {
                tagIdColumnName = tagIdParameter = string.Empty;
            }

            sqlStringBuilder.Append($"insert into Tag ({tagIdColumnName}Value, IsPredefined) " +
                                    $"values ({tagIdParameter}@Value, @IsPredefined)");

            return await toDoItemsDbAccessor.ExecuteAsync(sqlStringBuilder.ToString(), tags);
        }

        public static async Task<int> UpdateTagsAsync(this IToDoItemsDbAccessor toDoItemsDbAccessor, IEnumerable<TagDbModel> tags) =>
            await toDoItemsDbAccessor.ExecuteAsync("update Tag set Value=@Value, " +
                                          "IsPredefined=@IsPredefined where TagId=@TagId", tags);

        public static async Task<int> AddTagAssociationsAsync(this IToDoItemsDbAccessor toDoItemsDbAccessor, IEnumerable<ToDoItemTagAssociationDbModel> tagAssociations) =>
            await toDoItemsDbAccessor.ExecuteAsync("insert into ToDoItemTagAssociation (ToDoItemId, TagId, [Order]) " +
                                          "values (@ToDoItemId, @TagId, @Order)", tagAssociations);

        public static async Task<int> UpdateTagAssociationsAsync(this IToDoItemsDbAccessor toDoItemsDbAccessor, IEnumerable<ToDoItemTagAssociationDbModel> tagAssociations) =>
            await toDoItemsDbAccessor.ExecuteAsync("update ToDoItemTagAssociation " +
                                          "set [Order]=@Order " +
                                          "where TagId=@TagId and ToDoItemId=@ToDoItemId", tagAssociations);

        public static async Task<ToDoItemTagAssociationDbModel?> GetTagAssociationByIdsAsync(this IToDoItemsDbAccessor toDoItemsDbAccessor, int tagId, int toDoItemId)
        {

            var tagAssociations = await toDoItemsDbAccessor.SelectAsync<ToDoItemTagAssociationDbModel>("select * from ToDoItemTagAssociation " +
                                                                                              "where TagId=@TagId and " +
                                                                                              "ToDoItemId=@ToDoItemId",
                new { TagId = tagId, ToDoItemId = toDoItemId });

            return tagAssociations.SingleOrDefault();
        }

        public static async Task<int> DeleteTagAssociationsAsync(this IToDoItemsDbAccessor toDoItemsDbAccessor, IEnumerable<ToDoItemTagAssociationDbModel> tagAssociations) =>
            await toDoItemsDbAccessor.ExecuteAsync("delete from ToDoItemTagAssociation " +
                                          "where ToDoItemId=@ToDoItemId and TagId=@TagId", tagAssociations);

        public static async Task<int> AddAttachmentAsync(this IToDoItemsDbAccessor toDoItemsDbAccessor, AttachmentDbModel attachment)
        {
            var sqlStringBuilder = new StringBuilder();

            string attachmentIdColumnName;
            string attachmentIdParameter;
            
            if (attachment.AttachmentId != default)
            {
                sqlStringBuilder.Append("set IDENTITY_INSERT Attachment on; ");
                attachmentIdColumnName = "AttachmentId, ";
                attachmentIdParameter = $"@{attachmentIdColumnName}";
            }
            else
            {
                attachmentIdColumnName = attachmentIdParameter = string.Empty;
            }

            sqlStringBuilder.Append($"insert into Attachment ({attachmentIdColumnName}UniqueFileName, ProvidedFileName) " +
                                    $"values ({attachmentIdParameter}@UniqueFileName, @ProvidedFileName)");

            return await toDoItemsDbAccessor.ExecuteAsync(sqlStringBuilder.ToString(), attachment);
        }

        public static async Task<int> UpdateAttachmentAsync(this IToDoItemsDbAccessor toDoItemsDbAccessor, AttachmentDbModel attachment) =>
            await toDoItemsDbAccessor.ExecuteAsync("update Attachment set " +
                                          "UniqueFileName=@UniqueFileName, " +
                                          "ProvidedFileName=@ProvidedFileName " +
                                          "where AttachmentId=@AttachmentId", attachment);

        public static async Task<int> AddCustomFieldsAsync(this IToDoItemsDbAccessor toDoItemsDbAccessor, IEnumerable<CustomFieldDbModel> customFields)
        {
            var sqlStringBuilder = new StringBuilder();

            string customFieldIdColumnName;
            string customFieldIdParameter;

            if (customFields.Any(field => field.CustomFieldId != default))
            {
                sqlStringBuilder.Append("set IDENTITY_INSERT CustomField on; ");
                customFieldIdColumnName = "CustomFieldId, ";
                customFieldIdParameter = $"@{customFieldIdColumnName}";
            }
            else
            {
                customFieldIdColumnName = customFieldIdParameter = string.Empty;
            }

            sqlStringBuilder.Append($"insert into CustomField ({customFieldIdColumnName}[Order], Type, Name, " +
                                    "StringValue, IntValue, DateTimeValue)" +
                                    $"values ({customFieldIdParameter}" +
                                    "@Order, @Type, @Name, @StringValue, " +
                                    "@IntValue, @DateTimeValue)");

            return await toDoItemsDbAccessor.ExecuteAsync(sqlStringBuilder.ToString(), customFields);
        }

        public static async Task<int> AddToDoItemDependencyAsync(this IToDoItemsDbAccessor toDoItemsDbAccessor, ToDoItemDependencyDbModel toDoItemDependency) =>
            await toDoItemsDbAccessor.ExecuteAsync("insert into ToDoItemDependency (ToDoItemId, DependsOnToDoItemId) values " +
                                          "(@ToDoItemId, @DependsOnToDoItemId)", toDoItemDependency);

        public static async Task<int> UpdateToDoItemDependencyAsync(this IToDoItemsDbAccessor toDoItemsDbAccessor, ToDoItemDependencyDbModel toDoItemDependency) =>
            await toDoItemsDbAccessor.ExecuteAsync("update ToDoItemDependency " +
                                          "set DependsOnToDoItemId=@DependsOnToDoItemId " +
                                          "where ToDoItemId=@ToDoItemId", toDoItemDependency);

        public static async Task<ToDoItemDependencyDbModel?> GetToDoItemDependencyByIdsAsync(this IToDoItemsDbAccessor toDoItemsDbAccessor, int toDoItemId, int dependsOnToDoItemId)
        {
            var toDoItemDependency = await toDoItemsDbAccessor.SelectAsync<ToDoItemDependencyDbModel>("select * from ToDoItemDependency " +
                                                                                             "where ToDoItemId=@ToDoItemId and " +
                                                                                             "DependsOnToDoItemId=@DependsOnToDoItemId",
                new { ToDoItemId = toDoItemId, DependsOnToDoItemId = dependsOnToDoItemId });

            return toDoItemDependency.SingleOrDefault();
        }

        public static async Task<int> DeleteToDoItemDependencyAsync(this IToDoItemsDbAccessor toDoItemsDbAccessor, ToDoItemDependencyDbModel toDoItemDependency) =>
            await toDoItemsDbAccessor.ExecuteAsync("delete from ToDoItemDependency " +
                                          "where ToDoItemId=@ToDoItemId " +
                                          "and DependsOnToDoItemId=@DependsOnToDoItemId", toDoItemDependency);

        public static async Task<int> AddToDoItemStatusAsync(this IToDoItemsDbAccessor toDoItemsDbAccessor, ToDoItemStatusDbModel status)
        {
            var sqlStringBuilder = new StringBuilder();

            string statusIdColumnName;
            string statusIdParameter;

            if (status.ToDoItemStatusId != default)
            {
                sqlStringBuilder.Append("set IDENTITY_INSERT ToDoItemStatus on; ");
                statusIdColumnName = "ToDoItemStatusId, ";
                statusIdParameter = $"@{statusIdColumnName}";
            }
            else
            {
                statusIdColumnName = statusIdParameter = string.Empty;
            }

            sqlStringBuilder.Append($"insert into ToDoItemStatus ({statusIdColumnName}Name) values ({statusIdParameter}@Name))");

            return await toDoItemsDbAccessor.ExecuteAsync(sqlStringBuilder.ToString(), status);
        }

        public static async Task<int> UpdateToDoItemStatusAsync(this IToDoItemsDbAccessor toDoItemsDbAccessor, ToDoItemStatusDbModel status) =>
            await toDoItemsDbAccessor.ExecuteAsync("update ToDoItemStatus set Name=@Name " +
                                          "where ToDoItemStatusId=@ToDoItemStatusId", status);

        public static async Task<int> ClearTableAsync<T>(this IToDoItemsDbAccessor toDoItemsDbAccessor)
        {
            var tableName = DatabaseInfo.GetTableName<T>();

            return await toDoItemsDbAccessor.ExecuteAsync($"delete from {tableName}");
        }

        private static string GetConditionFromFilter(object filter)
        {
            var properties = filter.GetType()
                .GetProperties()
                .Where(prop => prop.PropertyType.IsValueType 
                    ? !(prop.GetValue(filter)?
                        .Equals(Activator.CreateInstance(prop.PropertyType)) ?? false) 
                    : prop.GetValue(filter) is not null)
                .Select(prop => $"{prop.Name}=@{prop.Name}")
                .ToList();

            if (!properties.Any())
            {
                throw new ApplicationException("Filter is empty.");
            }

            return string.Join(" and ", properties);
        }
    }
}