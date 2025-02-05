namespace AktBob.Database.Endpoints.Cases.Get;
internal record GetCasesRequest(int? DeskproId, long? PodioItemId, Guid? FilArkivCaseId);