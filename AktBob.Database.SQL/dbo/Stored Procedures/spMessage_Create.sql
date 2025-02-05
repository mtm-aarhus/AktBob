CREATE PROCEDURE [dbo].[spMessage_Create]
	@TicketId INT,
	@DeskproId INT,
	@Hash NCHAR(64) NULL,
	@Id INT OUTPUT
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			IF NOT EXISTS(
				SELECT 1
				FROM [Messages]
				WHERE
					TicketId = @TicketId
					AND DeskproId = @DeskproId
			)
			BEGIN
				INSERT INTO [Messages] (TicketId, DeskproId, [Hash], QueuedForJournalizationAt, MessageNumber)
				VALUES (@TicketId, @DeskproId, @Hash, GETUTCDATE(), (SELECT COUNT(Id) + 1 FROM [Messages] WHERE TicketId = @TicketId))
			END

			SELECT @Id = SCOPE_IDENTITY()
		COMMIT
	END TRY
	BEGIN CATCH
		IF (@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK
		END
	END CATCH
END