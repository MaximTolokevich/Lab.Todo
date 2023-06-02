CREATE PROCEDURE [dbo].[FindCyclicParentTasks]
	@toDoItemId INT,
	@parentTaskId INT
AS
BEGIN
	DECLARE @Separator CHAR = ' ';
	DECLARE @Cycle TABLE (CycleId INT IDENTITY(1,1) NOT NULL, Cycle VARCHAR(MAX) NOT NULL);

	WITH PathCTE (ParentTaskId, [Path]) AS
	(
		SELECT @parentTaskId AS ParentTaskId, 
			CAST(CONCAT(@parentTaskId, @Separator, @toDoItemId) AS VARCHAR(MAX))

		UNION ALL
   
		SELECT CASE WHEN t.ParentTaskId = @toDoItemId THEN NULL ELSE t.ParentTaskId END,
			CAST(CONCAT(t.ParentTaskId, @Separator, d.Path) AS VARCHAR(MAX))
		FROM ToDoItem t INNER JOIN PathCTE AS d
			ON t.ToDoItemId = d.ParentTaskId
		WHERE t.ParentTaskId IS NOT NULL
	)

	INSERT INTO @Cycle
	SELECT [Path]
    FROM PathCTE
    WHERE ParentTaskId IS NULL

	SELECT CycleId, CAST(value AS INT) AS DependencyId
	FROM @Cycle	CROSS APPLY STRING_SPLIT(Cycle, @Separator)
END