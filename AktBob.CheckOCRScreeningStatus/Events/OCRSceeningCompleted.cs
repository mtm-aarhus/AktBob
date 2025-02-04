using JNJ.MessageBus;

namespace AktBob.CheckOCRScreeningStatus.Events;
public record OCRSceeningCompleted(Guid CaseId) : Event;