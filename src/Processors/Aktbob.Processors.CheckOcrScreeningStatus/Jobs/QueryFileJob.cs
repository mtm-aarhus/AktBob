namespace Aktbob.Processors.CheckOcrScreeningStatus.Jobs;

internal record QueryFileJob(Guid FilArkivFileId, int Count = 1);