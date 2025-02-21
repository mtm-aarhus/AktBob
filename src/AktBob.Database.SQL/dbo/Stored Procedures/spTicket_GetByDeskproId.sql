CREATE PROCEDURE [dbo].[spTicket_GetByDeskproId]
	@DeskproId INT
AS
BEGIN
	SELECT *
	FROM v_Tickets
	WHERE DeskproId = @DeskproId
END