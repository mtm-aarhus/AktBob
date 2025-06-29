using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetMessage;
public interface IGetMessageHandler
{
    Task<ErrorOr<MessageDto>> Handle(int ticketId, int messageId, CancellationToken cancellationToken);
}