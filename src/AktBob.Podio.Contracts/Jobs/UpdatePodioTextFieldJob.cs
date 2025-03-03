namespace AktBob.Podio.Contracts.Jobs;
public record UpdatePodioTextFieldJob(int AppId, long ItemId, int FieldId, string TextValue);