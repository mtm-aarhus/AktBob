using AAK.Deskpro;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AktBob.CheckOCRScreeningStatus.UseCases.PostMessageToDeskproTicket;
internal class PostMessageToDeskproTicketCommandHandler : IRequestHandler<PostMessageToDeskproTicketCommand>
{
    private readonly IDeskproClient _deskpro;
    private readonly ILogger<PostMessageToDeskproTicketCommandHandler> _logger;

    public PostMessageToDeskproTicketCommandHandler(IDeskproClient deskpro, ILogger<PostMessageToDeskproTicketCommandHandler> logger)
    {
        _deskpro = deskpro;
        _logger = logger;
    }

    public async Task Handle(PostMessageToDeskproTicketCommand request, CancellationToken cancellationToken)
    {
        await _deskpro.PostNote(request.DeskproTicketId, request.Text, request.isAgentNote, cancellationToken);
        _logger.LogInformation("Agent note on Deskpro ticket #{deskproTicketId} posted", request.DeskproTicketId);
    }
}
