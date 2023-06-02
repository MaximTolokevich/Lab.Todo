CREATE TABLE [dbo].[Attachment]
(
	[AttachmentId] INT IDENTITY(1, 1) PRIMARY KEY NOT NULL, 
    [UniqueFileName] NVARCHAR(50) UNIQUE NOT NULL, 
    [ProvidedFileName] NVARCHAR(200) NOT NULL
)
