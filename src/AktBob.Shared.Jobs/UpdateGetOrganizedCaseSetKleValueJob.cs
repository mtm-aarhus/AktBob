using AktBob.Shared.Types.Deskpro;

namespace AktBob.Shared.Jobs;
public record UpdateGetOrganizedCaseSetKleValueJob(string TargetCaseId, string SourceCaseId);