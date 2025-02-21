CREATE PROCEDURE [dbo].[spMessage_Create]
	@TicketId INT,
	@DeskproId INT,
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
				INSERT INTO [Messages] (TicketId, DeskproId, MessageNumber)
				VALUES (@TicketId, @DeskproId, (SELECT COUNT(Id) + 1 FROM [Messages] WHERE TicketId = @TicketId))
				SELECT @Id = SCOPE_IDENTITY()
			END

			ELSE

			BEGIN
				SELECT @Id = Id
				FROM [Messages]
				WHERE
					TicketId = @TicketId
					AND DeskproId = @DeskproId
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