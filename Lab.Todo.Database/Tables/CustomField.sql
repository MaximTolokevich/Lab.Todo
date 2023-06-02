CREATE TABLE [dbo].[CustomField] (
    [CustomFieldId] INT            IDENTITY (1, 1) NOT NULL,
    [ToDoItemId]    INT            NOT NULL,
    [Order]         INT            NOT NULL,
    [Type]          INT            NOT NULL,
    [Name]          NVARCHAR (MAX) NOT NULL,
    [StringValue]   NVARCHAR (MAX) NULL,
    [IntValue]      INT            NULL,
    [DateTimeValue]     DATETIME2 (7)  NULL,
    CONSTRAINT [PK_CustomField] PRIMARY KEY CLUSTERED ([CustomFieldId] ASC), 
    CONSTRAINT [FK_CustomField_To_ToDoItem] FOREIGN KEY ([ToDoItemId]) REFERENCES [ToDoItem]([ToDoItemId])
);