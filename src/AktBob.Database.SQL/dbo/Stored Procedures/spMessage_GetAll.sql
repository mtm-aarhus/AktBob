CREATE PROCEDURE [dbo].[spMessage_GetAll]
AS
BEGIN
	SELECT *
	FROM v_Messages
END