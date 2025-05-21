using AktBob.Shared.Types.Podio;

namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;
internal class Case
{
    public Guid FilArkivCaseId { get; }
    public ItemId PodioItemId { get; }
    public Dictionary<Guid, File> Files { get; set; } = new();

    public Case(Guid filArkivCaseId, ItemId podioItemId)
    {
        FilArkivCaseId = filArkivCaseId;
        PodioItemId = podioItemId;
    }

    public File? GetFile(Guid id)
    {
        if (Files.TryGetValue(id, out var file))
        {
            return file;
        }

        return null;
    }

    public bool AnyFilesNotFinished => Files.Values.Any(f => !f.IsFinished);
}