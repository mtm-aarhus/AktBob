CREATE PROCEDURE [dbo].[spCase_Create]
	@TicketId INT,
	@PodioItemId BIGINT,
	@CaseNumber NVARCHAR(50),
    @FilArkivCaseId UNIQUEIDENTIFIER NULL,
	@Id INT OUTPUT
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			INSERT INTO Cases (TicketId, PodioItemId, CaseNumber, FilArkivCaseId)
			VALUES (@TicketId, @PodioItemId, @CaseNumber, @FilArkivCaseId)

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