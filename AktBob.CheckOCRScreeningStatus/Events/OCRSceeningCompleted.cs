using JNJ.MessageBus;

namespace AktBob.CheckOCRScreeningStatus.Events;
internal record OCRSceeningCompleted(Guid CaseId) : Event;