MERGE INTO [dbo].[ToDoItemStatus] as t
    USING (
        VALUES(1,'Planned')
             ,(2,'In Progress')
             ,(3,'Paused')
             ,(4,'Completed')
             ,(5,'Cancelled')
             /* Enter Additional Data Here */
    ) s ([ToDoItemStatusId],[Name])
        ON t.[ToDoItemStatusId] = s.[ToDoItemStatusId]
WHEN MATCHED AND (
        s.[Name] <> t.[Name]
    )
    THEN 
        UPDATE
            SET [Name] = s.[Name]
WHEN NOT MATCHED BY TARGET
    THEN
        INSERT 
        (
            [ToDoItemStatusId]
            ,[Name]
        )
        VALUES 
        (
            [ToDoItemStatusId]
            ,[Name]
        )
WHEN NOT MATCHED BY SOURCE 
   THEN 
       DELETE;

GO