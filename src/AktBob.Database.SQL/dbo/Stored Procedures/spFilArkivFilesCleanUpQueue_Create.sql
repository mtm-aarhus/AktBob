CREATE PROCEDURE [dbo].[spFilArkivFilesCleanUpQueue_Create]
	@FilArkivFileId UNIQUEIDENTIFIER
AS
BEGIN
	INSERT INTO FilArkivFilesCleanUpQueue (FilArkivFileId) VALUES (@FilArkivFileId)
END
