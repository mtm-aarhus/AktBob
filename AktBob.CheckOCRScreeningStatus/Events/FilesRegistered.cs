using JNJ.MessageBus;

namespace AktBob.CheckOCRScreeningStatus.Events;
public record FilesRegistered(Guid CaseId) : Event;
