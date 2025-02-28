using AktBob.Shared.CQRS;

namespace AktBob.Podio.Contracts;
public record PostItemCommentCommand(int AppId, long ItemId, string Comment) : ICommand;