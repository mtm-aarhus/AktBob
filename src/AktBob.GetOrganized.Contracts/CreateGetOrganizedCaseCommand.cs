namespace AktBob.GetOrganized.Contracts;

public record CreateGetOrganizedCaseCommand(
    string CaseTitle,
    string CaseProfile,
    string Status,
    string Access,
    string Department,
    string Facet,
    string Kle);
