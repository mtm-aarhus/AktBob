﻿CREATE PROCEDURE [dbo].[spCase_GetById]
	@Id INT
AS
BEGIN
	SELECT *
	FROM v_Cases
	WHERE Id = @Id
END