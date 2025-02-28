namespace AktBob.Database.UseCases.Cases.GetCases;
public record GetCasesQuery(int? DeskproId, long? PodioItemId, Guid? FilArkivCaseId) : IRequest<Result<IEnumerable<CaseDto>>>;