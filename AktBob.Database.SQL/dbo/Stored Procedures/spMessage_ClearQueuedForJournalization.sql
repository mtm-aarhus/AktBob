CREATE PROCEDURE [dbo].[spMessage_ClearQueuedForJournalization]
	@Id INT
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			UPDATE [Messages]
				SET 
					QueuedForJournalizationAt = NULL
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