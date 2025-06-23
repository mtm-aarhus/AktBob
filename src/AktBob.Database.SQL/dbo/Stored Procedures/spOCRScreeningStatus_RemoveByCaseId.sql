CREATE PROCEDURE spOCRScreeningStatus_RemoveByCaseId
    @FilArkivCaseId UNIQUEIDENTIFIER
AS
BEGIN 
   DELETE FROM OCRScreeningStatus WHERE FilArkivCaseId = @FilArkivCaseId 
END