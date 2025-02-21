namespace AktBob.Api.Endpoints.CheckOCRScreeningStatus;

internal record CheckOCRScreeningRequest(Guid FilArkivCaseId, long PodioItemId);