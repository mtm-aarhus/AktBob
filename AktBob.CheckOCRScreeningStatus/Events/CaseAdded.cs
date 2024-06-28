using JNJ.MessageBus;

namespace AktBob.CheckOCRScreeningStatus.Events;
internal record CaseAdded(Guid CaseId) : Event;