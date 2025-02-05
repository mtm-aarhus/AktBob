CREATE VIEW [dbo].[v_Cases]
AS 
SELECT
	Cases.Id,
	Cases.TicketId,
	Cases.PodioItemId,
	Cases.CaseNumber,
	Cases.FilArkivCaseId,
	Cases.SharepointFolderName
FROM Cases