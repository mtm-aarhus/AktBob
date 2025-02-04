using AAK.Podio;
using AktBob.Podio.Contracts;
using Ardalis.GuardClauses;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Podio;
internal class PostItemCommentCommandHandler(IPodio podio) : MediatorRequestHandler<PostItemCommentCommand, Result>
{
    private readonly IPodio _podio = podio;

    protected override async Task<Result> Handle(PostItemCommentCommand request, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrEmpty(request.Comment);
        return await _podio.PostItemComment(request.AppId, request.ItemId, request.Comment, cancellationToken);
    }
}
