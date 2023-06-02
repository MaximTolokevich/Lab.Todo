MERGE INTO [dbo].Tag AS t
    USING (VALUES ('Study', 1),
                  ('Entertainment', 1),
                  ('Health', 1),
                  ('Self-Improvement', 1),
                  ('Job', 1),
                  ('Urgent', 1)
           ) AS s ([Value], [IsPredefined])
    ON t.[Value] = s.[Value]
    WHEN MATCHED AND t.[IsPredefined] <> s.[IsPredefined]
    THEN UPDATE SET t.[IsPredefined] = s.[IsPredefined]
    WHEN NOT MATCHED BY TARGET
    THEN INSERT ([Value], [IsPredefined]) VALUES (s.[Value], s.[IsPredefined])
    WHEN NOT MATCHED BY SOURCE 
    THEN DELETE;

GO