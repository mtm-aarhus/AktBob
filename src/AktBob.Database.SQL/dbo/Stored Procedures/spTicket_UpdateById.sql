CREATE PROCEDURE [dbo].[spTicket_UpdateById]
	@Id INT,
	@CaseNumber NVARCHAR(50),
	@SharepointFolderName NVARCHAR(2048) NULL,
	@JournalizedAt DATETIME2 NULL,
	@TicketClosedAt DATETIME2 NULL,
	@CaseUrl NVARCHAR(1024) NULL
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			UPDATE Tickets
			SET
				CaseNumber = @CaseNumber,
				SharepointFolderName = @SharepointFolderName,
				JournalizedAt = @JournalizedAt,
				TicketClosedAt = @TicketClosedAt,
				CaseUrl = @CaseUrl
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