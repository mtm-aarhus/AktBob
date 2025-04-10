﻿CREATE PROCEDURE [dbo].[spTicket_Create]
	@DeskproId INT,
	@Id INT OUTPUT
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			INSERT INTO Tickets (DeskproId)
			VALUES (@DeskproId)

			SELECT @Id = SCOPE_IDENTITY()
		COMMIT
	END TRY
	BEGIN CATCH
		IF (@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK
		END
	END CATCH
END