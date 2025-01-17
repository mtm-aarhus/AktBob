namespace AktBob.CloudConvert.Contracts.DTOs;

public record JobDto
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Filename { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}