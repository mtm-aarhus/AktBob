namespace AktBob.Podio.Contracts;
internal interface IPostCommentHandler
{
    Task Handle(PostCommentCommand command, CancellationToken cancellationToken);
}