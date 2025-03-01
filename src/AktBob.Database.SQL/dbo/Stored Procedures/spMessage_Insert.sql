CREATE PROCEDURE [dbo].[spMessage_Insert]
	@TicketId INT,
	@DeskproMessageId INT,
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
					AND DeskproMessageId = @DeskproMessageId
			)
			BEGIN
				INSERT INTO [Messages] (TicketId, DeskproMessageId, MessageNumber)
				VALUES (@TicketId, @DeskproMessageId, (SELECT COUNT(Id) + 1 FROM [Messages] WHERE TicketId = @TicketId))
				SELECT @Id = SCOPE_IDENTITY()
			END

			ELSE

			BEGIN
				SELECT @Id = Id
				FROM [Messages]
				WHERE
					TicketId = @TicketId
					AND DeskproMessageId = @DeskproMessageId
			END
		COMMIT
	END TRY
	BEGIN CATCH
		IF (@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK
		END
	END CATCH
END