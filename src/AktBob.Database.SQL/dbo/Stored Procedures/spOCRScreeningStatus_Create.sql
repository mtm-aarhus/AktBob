CREATE PROCEDURE spOCRScreeningStatus_Create
    @PodioItemId BIGINT,
    @FilArkivCaseId UNIQUEIDENTIFIER,
    @FilArkivFileId UNIQUEIDENTIFIER
AS
    BEGIN
        BEGIN TRY
            BEGIN TRAN
                INSERT INTO OCRScreeningStatus (PodioItemId, FilArkivCaseId, FilArkivFileId)
                VALUES (@PodioItemId, @FilArkivCaseId, @FilArkivFileId)
            COMMIT
        END TRY
        BEGIN CATCH
            IF (@@TRANCOUNT > 0)
                BEGIN
                    ROLLBACK
                END
        END CATCH
    END