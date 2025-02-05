CREATE PROCEDURE [dbo].[spCase_GetByTicketId]
	@TicketId INT
AS
BEGIN
	SELECT *
	FROM v_Cases
	WHERE TicketId = @TicketId
END