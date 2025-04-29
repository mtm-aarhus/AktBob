CREATE PROCEDURE [dbo].[spOS2FormsSubmission_Create]
	@DeskproTicketId INT,
	@SubmissionId UNIQUEIDENTIFIER,
	@DescriptionFieldValue NVARCHAR(MAX),
	@Id INT OUTPUT
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			INSERT INTO [OS2FormsSubmission] (DeskproTicketId, SubmissionId, DescriptionFieldValue)
			VALUES (@DeskproTicketId, @SubmissionId, @DescriptionFieldValue)

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