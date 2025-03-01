namespace AktBob.GetOrganized.Contracts;
public interface IGetOrganizedHandlers
{
    ICreateGetOrganizedCaseHandler CreateGetOrganizedCase { get; }
    IFinalizeGetOrganizedDocumentHandler FinalizeGetOrganizedDocument { get; }
    IRelateGetOrganizedDocumentsHandler RelateGetOrganizedDocuments { get; }
    IUploadGetOrganizedDocumentHandler UploadGetOrganziedDocument { get; }
}
