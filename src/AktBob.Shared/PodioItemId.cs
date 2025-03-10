using System.Diagnostics.CodeAnalysis;

namespace AktBob.Shared;

public readonly struct PodioItemId : IEquatable<PodioItemId>
{
    public PodioItemId(int appId, long id)
    {
        AppId = appId;
        Id = id;
    }

    public int AppId { get; }
    public long Id { get; }

    public override string ToString() => $"(AppId = {AppId}, ItemId = {Id})";
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is PodioItemId other && Equals(other);
    public bool Equals(PodioItemId other) => AppId == other.AppId && Id == other.Id;
    public override int GetHashCode()
    {
        return HashCode.Combine(AppId, Id);
    }
}