namespace AktBob.OS2Forms.Contracts;
public record SubmissionDto(
    Guid Id,
    string WebformId,
    IReadOnlyDictionary<string, string> ParentTypes,
    IReadOnlyDictionary<string, string> Data);