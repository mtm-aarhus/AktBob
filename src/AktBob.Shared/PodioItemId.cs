namespace AktBob.Shared;

public readonly struct PodioItemId
{
    public PodioItemId(int appId, long id)
    {
        AppId = appId;
        Id = id;
    }

    public int AppId { get; }
    public long Id { get; }

    public override string ToString() => $"(AppId = {AppId}, ItemId = {Id})";
    
}