CREATE TABLE [dbo].[ToDoItemTagAssociation]
(
	[ToDoItemId] INT NOT NULL,
	[TagId] INT NOT NULL,
	[Order] INT NOT NULL,
    PRIMARY KEY ([ToDoItemId], [TagId]),
	FOREIGN KEY ([TagId]) REFERENCES [dbo].[Tag]([TagId]),
	FOREIGN KEY ([ToDoItemId]) REFERENCES [dbo].[ToDoItem]([ToDoItemId])
)
