using MediatR;

namespace AktBob.Podio.Contracts;
public record PostItemCommentCommand(int AppId, long ItemId, string Comment) : IRequest;