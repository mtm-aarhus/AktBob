namespace AktBob.Database.UseCases.Cases.GetCases;
public record GetCasesQuery(int? DeskproId, long? PodioItemId, Guid? FilArkivCaseId) : IQuery<Result<IEnumerable<CaseDto>>>;