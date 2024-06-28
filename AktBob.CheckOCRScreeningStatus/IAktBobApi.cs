using Ardalis.Result;

namespace AktBob.CheckOCRScreeningStatus;

public interface IAktBobApi
{
    Task<Result> UpdatePodioItemFilArkivField(long podioItemId, Guid filArkivCaseId);
}