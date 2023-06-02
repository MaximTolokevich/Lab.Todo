CREATE TABLE [dbo].[ToDoItem]
(
	[ToDoItemId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [Title] NVARCHAR(250) NOT NULL, 
    [Description] NVARCHAR(MAX) NULL, 
    [ToDoItemStatusId] INT NOT NULL DEFAULT 1,
    [ParentTaskId] INT NULL,
    [Duration] TIME NULL, 
    [ActualStartTime] DATETIME2 NULL, 
    [ActualEndTime] DATETIME2 NULL, 
    [AssignedTo] NVARCHAR(250) NOT NULL,
    [CreatedBy] NVARCHAR(250) NOT NULL, 
    [CreatedDate] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(), 
    [ModifiedBy] NVARCHAR(250) NULL, 
    [ModifiedDate] DATETIME2(7) NULL DEFAULT SYSUTCDATETIME(),
    [Deadline] DATETIME2 NULL, 
    [PlannedStartTime] DATETIME2 NULL,
    CONSTRAINT [FK_ToDoItem_ToToDoItemStatus] FOREIGN KEY ([ToDoItemStatusId]) REFERENCES [ToDoItemStatus]([ToDoItemStatusId]),
    CONSTRAINT [FK_ToDoItem_ToToDoItem] FOREIGN KEY ([ParentTaskId]) REFERENCES [ToDoItem]([ToDoItemId])
)