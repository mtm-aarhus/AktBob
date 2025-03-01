using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public interface IGetDeskproMessageHandler
{
    Task<Result<MessageDto>> Handle(int ticketId, int messageId, CancellationToken cancellationToken);
}