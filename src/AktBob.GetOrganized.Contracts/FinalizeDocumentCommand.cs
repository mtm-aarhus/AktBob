namespace AktBob.GetOrganized.Contracts;
public record FinalizeDocumentCommand(int DocumentId, bool ShouldCloseOpenTasks = false);