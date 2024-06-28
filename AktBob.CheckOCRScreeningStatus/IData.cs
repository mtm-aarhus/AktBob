
namespace AktBob.CheckOCRScreeningStatus;

internal interface IData
{
    bool AddCase(Guid caseId, long podioItemId);
    void AddFilesToCase(Case @case, IEnumerable<File> files);
    void FileHasBeenScreened(File file);
    Case? GetCase(Guid caseId);
    Case? GetCaseByFileId(Guid fileId);
    File? GetFile(Guid fileId);
    void RemoveCase(Case @case);
}