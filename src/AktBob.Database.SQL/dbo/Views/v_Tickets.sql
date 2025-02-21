CREATE VIEW [dbo].[v_Tickets]
AS

SELECT
	Id
	,DeskproId
	,CaseNumber
	,CaseUrl
	,SharepointFolderName
	,JournalizedAt
	,TicketClosedAt

FROM Tickets