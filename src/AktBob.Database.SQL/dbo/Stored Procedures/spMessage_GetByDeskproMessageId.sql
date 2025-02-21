CREATE PROCEDURE [dbo].[spMessage_GetByDeskproMessageId]
@DeskproMessageId INT
AS
BEGIN
	SELECT *
	FROM v_Messages
	WHERE
		DeskproMessageId = @DeskproMessageId
END