using AAK.Podio;
using AktBob.Podio.Contracts;
using Ardalis.GuardClauses;
using MassTransit.Mediator;

namespace AktBob.Podio.UseCases;
public class PostItemCommentCommandHandler(IPodio podio) : MediatorRequestHandler<PostItemCommentCommand>
{
    private readonly IPodio _podio = podio;

    protected override async Task Handle(PostItemCommentCommand request, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrEmpty(request.Comment);
        await _podio.PostItemComment(request.AppId, request.ItemId, request.Comment, cancellationToken);
    }
}
