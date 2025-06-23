using AktBob.Database.Entities;

namespace AktBob.Database.Contracts;

public interface IOcrScreeningStatusRepository
{
    Task<bool> Add(OcrScreeningStatus ocrScreeningStatus);
    Task<bool> Update(OcrScreeningStatus ocrScreeningStatus);
    Task<OcrScreeningStatus?> Get(Guid filArkivFileId);
    Task RemoveByCaseId(Guid filArkivCaseId);
    Task<bool> AnyByCaseId(Guid filarkivCaseId);
    Task<bool> AllFilesAreProcessed(Guid filarkivCaseId);
}