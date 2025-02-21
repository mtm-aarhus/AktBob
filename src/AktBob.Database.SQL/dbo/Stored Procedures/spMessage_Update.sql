CREATE PROCEDURE [dbo].[spMessage_Update]
	@DeskproId INT,
	@GODocumentId INT NULL
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			UPDATE [Messages] SET GODocumentId = @GODocumentId
			WHERE DeskproId = @DeskproId
		COMMIT
	END TRY
	BEGIN CATCH
		IF (@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK
		END
	END CATCH
END