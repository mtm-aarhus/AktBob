namespace Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Contracts;
public record SubmissionDto(
    Guid Id,
    string WebformId,
    IReadOnlyDictionary<string, string> ParentTypes,
    IReadOnlyDictionary<string, string> Data);