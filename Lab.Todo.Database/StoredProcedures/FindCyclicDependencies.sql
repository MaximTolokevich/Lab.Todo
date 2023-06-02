CREATE PROCEDURE [dbo].[FindCyclicDependencies]
	@toDoItemId INT,
	@dependenciesIds IDs READONLY
AS
BEGIN
	DECLARE @Cycle TABLE (CycleId INT IDENTITY(1,1) NOT NULL, Cycle VARCHAR(MAX) NOT NULL);

	WITH PathCTE (DependencyId, [Path], HasCycle)
	AS
	(
		SELECT Id, CAST(CONCAT(@toDoItemId, ' ', Id) AS VARCHAR(MAX)),
			CASE WHEN Id = @toDoItemId THEN 1 ELSE 0 END
		FROM @dependenciesIds

		UNION ALL

		SELECT ToDoItemDependency.DependsOnToDoItemId, CONCAT([Path], ' ', ToDoItemDependency.DependsOnToDoItemId),
			CASE WHEN ToDoItemDependency.DependsOnToDoItemId = @toDoItemId THEN 1 ELSE 0 END
		FROM PathCTE
		JOIN ToDoItemDependency
		ON PathCTE.DependencyId = ToDoItemDependency.ToDoItemId
		WHERE HasCycle = 0
	)

	INSERT INTO @Cycle
	SELECT [Path] FROM PathCTE
	WHERE HasCycle = 1

	SELECT CycleId, CAST(value AS INT) AS DependencyId
	FROM @Cycle
	CROSS APPLY STRING_SPLIT(Cycle, ' ')
END