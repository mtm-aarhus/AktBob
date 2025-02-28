namespace AktBob.GetOrganized.Contracts;
public record CreateCaseResponse(string CaseId, string CaseUrl) : ICommand;
