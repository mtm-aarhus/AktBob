using FilArkivCore.Web.Client;

namespace AktBob.CheckOCRScreeningStatus;
internal class FilArkiv : IFilArkiv
{
    public FilArkiv(FilArkivCoreClient filArkivCoreClient)
    {
        FilArkivCoreClient = filArkivCoreClient;
    }

    public FilArkivCoreClient FilArkivCoreClient { get; }   
}
