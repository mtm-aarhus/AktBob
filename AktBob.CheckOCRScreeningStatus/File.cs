namespace AktBob.CheckOCRScreeningStatus;
internal class File
{
    public Guid FileId { get; }
    public TimeSpan DelayBetweenChecks { get; }
    public bool HasBeenScreened { get; set; } = false;

    public File(Guid fileId, TimeSpan delayBetweenChecks)
    {
        FileId = fileId;
        DelayBetweenChecks = delayBetweenChecks;
    }
}
