using AktBob.GetOrganized.Contracts;

namespace AktBob.GetOrganized;
internal class GetOrganizedHandlers : IGetOrganizedHandlers
{
    public GetOrganizedHandlers(
        ICreateGetOrganizedCaseHandler createGetOrganizedCaseHandler,
        IFinalizeGetOrganizedDocumentHandler finalizeGetOrganizedDocumentHandler,
        IRelateGetOrganizedDocumentsHandler relateGetOrganizedDocumentsHandler,
        IUploadGetOrganizedDocumentHandler uploadGetOrganizedDocumentHandler)
    {
        CreateGetOrganizedCase = createGetOrganizedCaseHandler;
        FinalizeGetOrganizedDocument = finalizeGetOrganizedDocumentHandler;
        RelateGetOrganizedDocuments = relateGetOrganizedDocumentsHandler;
        UploadGetOrganziedDocument = uploadGetOrganizedDocumentHandler;
    }

    public ICreateGetOrganizedCaseHandler CreateGetOrganizedCase { get; }
    public IFinalizeGetOrganizedDocumentHandler FinalizeGetOrganizedDocument { get; }
    public IRelateGetOrganizedDocumentsHandler RelateGetOrganizedDocuments { get; }
    public IUploadGetOrganizedDocumentHandler UploadGetOrganziedDocument { get; }
}
