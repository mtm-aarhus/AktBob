CREATE PROCEDURE [dbo].[spCase_UpdateById]
	@Id INT,
	@TicketId INT,
	@PodioItemId BIGINT,
	@CaseNumber NVARCHAR(50),
	@FilArkivCaseId UNIQUEIDENTIFIER NULL,
	@SharepointFolderName NVARCHAR(2048) NULL
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			UPDATE Cases
			SET
				TicketId = @TicketId,
				PodioItemId = @PodioItemId,
				CaseNumber = @CaseNumber,
				FilArkivCaseId = @FilArkivCaseId,
				SharepointFolderName = @SharepointFolderName
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