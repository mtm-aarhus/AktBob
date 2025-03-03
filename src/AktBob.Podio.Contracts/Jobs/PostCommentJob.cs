namespace AktBob.Podio.Contracts.Jobs;
public record PostCommentJob(int AppId, long ItemId, string TextValue);