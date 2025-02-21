CREATE PROCEDURE [dbo].[spMessage_GetById]
	@Id int
AS
BEGIN
	SELECT *
	FROM v_Messages
	WHERE Id = @Id
END