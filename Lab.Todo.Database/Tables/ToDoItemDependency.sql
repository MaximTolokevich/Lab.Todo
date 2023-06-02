CREATE TABLE [dbo].[ToDoItemDependency]
(
    [ToDoItemId] INT NOT NULL, 
    [DependsOnToDoItemId] INT NOT NULL, 
    CONSTRAINT [CK_ToDoItemDependency_CheckEqualIds] CHECK (ToDoItemId != [DependsOnToDoItemId]), 
    CONSTRAINT [PK_ToDoItemDependency] PRIMARY KEY ([ToDoItemId], [DependsOnToDoItemId]), 
    CONSTRAINT [FK_ToDoItemDependency_ToDoItemId] FOREIGN KEY ([ToDoItemId]) REFERENCES [ToDoItem]([ToDoItemId]), 
    CONSTRAINT [FK_ToDoItemDependency_DependsOnToDoItemId] FOREIGN KEY ([DependsOnToDoItemId]) REFERENCES [ToDoItem]([ToDoItemId])
)
