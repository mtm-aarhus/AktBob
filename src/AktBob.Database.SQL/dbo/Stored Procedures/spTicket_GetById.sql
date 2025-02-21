CREATE PROCEDURE [dbo].[spTicket_GetById]
	@Id INT
AS
BEGIN
	SELECT *
	FROM v_Tickets
	WHERE Id = @Id
END