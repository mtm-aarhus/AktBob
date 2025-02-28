namespace AktBob.Database.UseCases.Cases.AddCase;
public record AddCaseCommand(int TicketId, long PodioItemId, string CaseNumber, Guid? FilArkivCaseId) : ICommand<Result<CaseDto>>;
