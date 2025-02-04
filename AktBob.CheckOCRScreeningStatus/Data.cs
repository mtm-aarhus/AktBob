namespace AktBob.CheckOCRScreeningStatus;

public class Data : IData
{
    private List<Case> _cases;

    public Data()
    {
        _cases = new List<Case>();
    }


    public bool AddCase(Guid caseId, long podioItemId)
    {
        lock (_cases)
        {
            if (_cases.FirstOrDefault(c => c.CaseId == caseId) is null)
            {
                _cases.Add(new Case(caseId, podioItemId));
                return true;
            }
            
            return false;
        }
    }


    public Case? GetCase(Guid caseId)
    {
        lock (_cases)
        {
            return _cases.FirstOrDefault(c => c.CaseId.Equals(caseId));
        }
    }


    public Case? GetCaseByFileId(Guid fileId)
    {
        lock (_cases)
        {
            return _cases.FirstOrDefault(c => c.Files.Any(f => f.FileId == fileId));
        }
    }


    public void AddFilesToCase(Case @case, IEnumerable<File> files)
    {
        lock ( _cases)
        {
            if (_cases.Contains(@case))
            {
                @case.Files.AddRange(files);
            }
        }
    }



    public File? GetFile(Guid fileId)
    {
        lock (_cases)
        {
            return _cases.SelectMany(d => d.Files).FirstOrDefault(f => f.FileId == fileId);
        }
    }


    public void RemoveCase(Case @case)
    {
        lock (_cases)
        {
            _cases.Remove(@case);
        }
    }


    public void FileHasBeenScreened(File file)
    {
        lock (_cases)
        {
            file.HasBeenScreened = true;
        }
    }
}
