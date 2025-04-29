CREATE VIEW [dbo].[v_OS2FormsSubmissions]
AS

SELECT
	s.Id,
	s.DeskproTicketId,
	s.SubmissionId,
	s.DescriptionFieldValue

FROM [OS2FormsSubmission] s