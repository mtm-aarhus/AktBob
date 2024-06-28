using AktBob.CreateOCRScreeningStatus.ExternalQueue;

namespace AktBob.CheckOCRScreeningStatus;
internal interface IQueueService
{
    IQueue Queue { get; }
}