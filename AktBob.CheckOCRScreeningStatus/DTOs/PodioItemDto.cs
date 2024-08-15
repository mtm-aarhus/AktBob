namespace AktBob.CheckOCRScreeningStatus.DTOs;
internal record PodioItemDto
{
    public int ItemId { get; set; }
    public string ItemIdFormatted { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public IEnumerable<PodioItemFieldDto> Fields { get; set; } = new List<PodioItemFieldDto>();
}