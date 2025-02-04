namespace AktBob.CheckOCRScreeningStatus;
public class File
{
    public Guid FileId { get; }
    public bool HasBeenScreened { get; set; } = false;

    public File(Guid fileId)
    {
        FileId = fileId;
    }
}
