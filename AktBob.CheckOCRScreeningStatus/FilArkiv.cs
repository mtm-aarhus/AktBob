using FilArkivCore.Web.Client;

namespace AktBob.CheckOCRScreeningStatus;
internal class FilArkiv
{
    public FilArkiv(FilArkivCoreClient filArkivCoreClient)
    {
        FilArkivCoreClient = filArkivCoreClient;
    }

    public FilArkivCoreClient FilArkivCoreClient { get; }   
}
