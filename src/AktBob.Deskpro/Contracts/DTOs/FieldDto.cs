namespace AktBob.Deskpro.Contracts.DTOs;
public class FieldDto
{
    public int Id { get; init; }
    public IEnumerable<string> Values { get; init; } = Enumerable.Empty<string>();
    public IReadOnlyDictionary<int, string> Choices { get; set; } = new Dictionary<int, string>();
}
