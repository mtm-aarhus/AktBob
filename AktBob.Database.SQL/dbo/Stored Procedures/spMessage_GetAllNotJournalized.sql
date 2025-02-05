CREATE PROCEDURE [dbo].[spMessage_GetAllNotJournalized]
AS
BEGIN
	SELECT *
	FROM v_Messages
	WHERE v_Messages.QueuedForJournalizationAt IS NOT NULL
END