using AAK.Podio;
using AktBob.Podio.Contracts;
using Ardalis.GuardClauses;
using Ardalis.Result;
using MediatR;

namespace AktBob.Podio;
internal class PostItemCommentCommandHandler : IRequestHandler<PostItemCommentCommand, Result>
{
    private readonly IPodio _podio;

    public PostItemCommentCommandHandler(IPodio podio)
    {
        _podio = podio;
    }

    public async Task<Result> Handle(PostItemCommentCommand request, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrEmpty(request.Comment);
        return await _podio.PostItemComment(request.AppId, request.ItemId, request.Comment, cancellationToken);
    }
}
