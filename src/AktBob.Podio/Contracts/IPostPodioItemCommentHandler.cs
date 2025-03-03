namespace AktBob.Podio.Contracts;
internal interface IPostPodioItemCommentHandler
{
    Task Handle(int appId, long itemId, string comment, CancellationToken cancellationToken);
}