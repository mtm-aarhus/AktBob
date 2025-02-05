CREATE VIEW [dbo].[v_Messages]
AS
	
SELECT
	[Messages].Id 'Id'
	,[Messages].QueuedForJournalizationAt 'QueuedForJournalizationAt'
	,[Messages].[Hash] 'Hash'
	,Tickets.Id 'TicketId'
	,Tickets.DeskproId 'DeskproTicketId'
	,Tickets.CaseNumber 'GOCaseNumber'
	,[Messages].DeskproId 'DeskproMessageId'
	,[Messages].GODocumentId 'GODocumentId'
	,[Messages].MessageNumber 'MessageNumber'
	
FROM [Messages]
INNER JOIN Tickets ON [Messages].TicketId = Tickets.Id

WHERE
	[Messages].Deleted = 0