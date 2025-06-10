using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetMessage;
internal interface IGetMessageHandler
{
    Task<ErrorOr<MessageDto>> Handle(int ticketId, int messageId, CancellationToken cancellationToken);
}