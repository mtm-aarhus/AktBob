CREATE PROCEDURE [dbo].[spMessage_Update]
	@Id INT,
	@TicketId INT,
	@DeskproId INT,
	@GODocumentId INT NULL
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			UPDATE [Messages]
				SET 
					TicketId = @TicketId,
					DeskproId = @DeskproId,
					GODocumentId = @GODocumentId
			WHERE Id = @Id
		COMMIT
	END TRY
	BEGIN CATCH
		IF (@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK
		END
	END CATCH
END