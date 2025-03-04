namespace AktBob.Podio.Contracts;
internal interface IPostCommentHandler
{
    Task Handle(int appId, long itemId, string comment, CancellationToken cancellationToken);
}