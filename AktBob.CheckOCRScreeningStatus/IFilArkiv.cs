using FilArkivCore.Web.Client;

namespace AktBob.CheckOCRScreeningStatus;
public interface IFilArkiv
{
    FilArkivCoreClient FilArkivCoreClient { get; }
}