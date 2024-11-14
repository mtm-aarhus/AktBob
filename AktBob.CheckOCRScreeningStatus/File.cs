namespace AktBob.CheckOCRScreeningStatus;
internal class File
{
    public Guid FileId { get; }
    public bool HasBeenScreened { get; set; } = false;

    public File(Guid fileId)
    {
        FileId = fileId;
    }
}
