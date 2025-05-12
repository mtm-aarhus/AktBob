namespace AktBob.Workflows.Processes.CheckOCRScreeningStatus;
internal class File
{
    private bool _isFinished = false;

    public bool IsFinished => _isFinished;
    public void SetStatus(bool isFinished) => _isFinished = isFinished;
}
