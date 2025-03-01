namespace AktBob.Podio.Contracts;
public interface IPostPodioItemCommentHandler
{
    Task Handle(int appId, long itemId, string comment, CancellationToken cancellationToken);
}