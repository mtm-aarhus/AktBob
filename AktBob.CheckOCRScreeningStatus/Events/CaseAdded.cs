using JNJ.MessageBus;

namespace AktBob.CheckOCRScreeningStatus.Events;
public record CaseAdded(Guid CaseId) : Event;