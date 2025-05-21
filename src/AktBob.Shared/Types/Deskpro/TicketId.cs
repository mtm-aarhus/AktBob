using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace AktBob.Shared.Types.Deskpro;

public struct TicketId : IParsable<TicketId>, IEquatable<TicketId>
{
    public int Value { get; }

    public TicketId(int value) => Value = value;

    public static TicketId Create(int value) => new TicketId(value);
    public static implicit operator int(TicketId value) => value.Value;
    public static explicit operator TicketId(int? value)
    {
        if (value != null)
        {
            return Create((int)value);
        }

        return default;
    }
    public static explicit operator TicketId(string? value)
    {
        if (int.TryParse(value, out int result))
        {
            return Create(int.Parse(value));
        }

        return default;
    }

    public override string ToString() => Value.ToString();

    public static TicketId Parse(string s, IFormatProvider? provider)
    {
        return Create(int.Parse(s));
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out TicketId result)
    {
        if (int.TryParse(s, out var value))
        {
            result = Create(value);
            return true;
        }

        result = default;
        return false;
    }

    public bool Equals(TicketId other) => Value == other.Value;
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}