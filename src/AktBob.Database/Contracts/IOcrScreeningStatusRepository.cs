using AktBob.Database.Entities;

namespace AktBob.Database.Contracts;

public interface IOcrScreeningStatusRepository
{
    Task<bool> Add(OcrScreeningStatus ocrScreeningStatus);
    Task<bool> Update(OcrScreeningStatus ocrScreeningStatus);
}