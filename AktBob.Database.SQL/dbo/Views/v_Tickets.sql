CREATE VIEW [dbo].[v_Tickets]
AS

SELECT
	Id
	,DeskproId
	,CaseNumber
	,SharepointFolderName
	,JournalizedAt
	,TicketClosedAt

FROM Tickets