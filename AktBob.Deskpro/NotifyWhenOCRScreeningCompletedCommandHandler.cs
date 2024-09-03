//using AAK.Deskpro;
//using AktBob.Deskpro.Contracts;
//using Ardalis.GuardClauses;
//using MediatR;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;

//namespace AktBob.Deskpro;

//internal class NotifyWhenOCRScreeningCompletedCommandHandler : IRequestHandler<NotifyWhenOCRScreeningCompletedCommand>
//{
//    private readonly IDeskproClient _deskproClient;
//    private readonly IConfiguration _configuration;
//    private readonly ILogger<NotifyWhenOCRScreeningCompletedCommandHandler> _logger;

//    public NotifyWhenOCRScreeningCompletedCommandHandler(IDeskproClient deskproClient, IConfiguration configuration, ILogger<NotifyWhenOCRScreeningCompletedCommandHandler> logger)
//    {
//        _deskproClient = deskproClient;
//        _configuration = configuration;
//        _logger = logger;
//    }

//    public async Task Handle(NotifyWhenOCRScreeningCompletedCommand request, CancellationToken cancellationToken)
//    {
//        try
//        {
//            var webhookId = Guard.Against.NullOrEmpty(_configuration.GetValue<string>("Deskpro:InboundWebhooks:NotifyOCRScreeningCompleted"));
//            Guard.Against.Null(request.DeskproId);
//            Guard.Against.NullOrEmpty(request.Sagsnummer);
//            Guard.Against.NullOrEmpty(request.PodioLink);
//            Guard.Against.NullOrEmpty(request.FilArkivLink);

//            var payload = new
//            {
//                request.DeskproId,
//                CaseNumber = request.Sagsnummer,
//                request.PodioLink,
//                request.FilArkivLink
//            };

//            await _deskproClient.PostWebhook(webhookId, payload, cancellationToken);

//            _logger.LogInformation("Deskpro inbound webhook {id} requested successfully.", webhookId);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError("Error requesting Deskpro inbound webhook. {error}", ex.Message);
//        }
//    }
//}
