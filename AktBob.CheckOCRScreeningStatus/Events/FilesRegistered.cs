using JNJ.MessageBus;

namespace AktBob.CheckOCRScreeningStatus.Events;
internal record FilesRegistered(Guid CaseId) : Event;
