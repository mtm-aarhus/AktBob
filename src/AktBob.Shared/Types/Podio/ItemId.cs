using System.Diagnostics.CodeAnalysis;

namespace AktBob.Shared.Types.Podio;

public readonly struct ItemId : IEquatable<ItemId>
{
    public ItemId(int appId, long id)
    {
        AppId = appId;
        Id = id;
    }

    public int AppId { get; }
    public long Id { get; }
    public static ItemId Create(int appId, long id) => new ItemId(appId, id);
    public override string ToString() => $"(AppId = {AppId}, ItemId = {Id})";
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ItemId other && Equals(other);
    public bool Equals(ItemId other) => AppId == other.AppId && Id == other.Id;
    public override int GetHashCode()
    {
        return HashCode.Combine(AppId, Id);
    }
}