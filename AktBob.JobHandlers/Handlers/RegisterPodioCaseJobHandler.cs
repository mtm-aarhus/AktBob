using AktBob.Database.Contracts;
using AktBob.Database.UseCases.Cases.AddCase;
using AktBob.JobHandlers.Utils;
using AktBob.Podio.Contracts;
using AktBob.Shared;
using AktBob.Shared.Contracts;
using Ardalis.GuardClauses;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AktBob.PodioHookProcessor.UseCases;
internal class RegisterPodioCaseJobHandler(ILogger<RegisterPodioCaseJobHandler> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory) : IJobHandler<RegisterPodioCaseJob>
{
    private readonly ILogger<RegisterPodioCaseJobHandler> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Handle(RegisterPodioCaseJob job, CancellationToken cancellationToken = default)
    {
        var podioAppId = Guard.Against.Null(_configuration.GetValue<int?>("Podio:AppId"));
        var podioFields = Guard.Against.Null(Guard.Against.NullOrEmpty(_configuration.GetSection("Podio:Fields").GetChildren().ToDictionary(x => long.Parse(x.Key), x => x.Get<PodioField>())));
        var podioFieldDeskproId = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value.AppId == podioAppId && x.Value.Label == "DeskproId"));
        var podioFieldCaseNumber = Guard.Against.Null(podioFields.FirstOrDefault(x => x.Value.AppId == podioAppId && x.Value.Label == "CaseNumber"));
        Guard.Against.Null(podioFieldDeskproId.Value);

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // Get metadata from Podio
            var getPodioItemQuery = new GetItemQuery(podioAppId, job.PodioItemId);
            var getPodioItemQueryResult = await mediator.SendRequest(getPodioItemQuery, cancellationToken);

            if (!getPodioItemQueryResult.IsSuccess)
            {
                _logger.LogError("Could not get item {itemId} from Podio", job.PodioItemId);
                return;
            }

            var caseNumber = getPodioItemQueryResult.Value.Fields.FirstOrDefault(x => x.Id == podioFieldCaseNumber.Key)?.Value?.FirstOrDefault();

            if (string.IsNullOrEmpty(caseNumber))
            {
                _logger.LogError("Could not get case number field value from Podio Item {id}", job.PodioItemId);
                return;
            }

            // Get metadata from Deskpro
            var deskproIdString = getPodioItemQueryResult.Value.Fields.FirstOrDefault(x => x.Id == podioFieldDeskproId.Key)?.Value?.FirstOrDefault();
            if (string.IsNullOrEmpty(deskproIdString))
            {
                _logger.LogError("Could not get Deskpro Id field value from Podio Item {itemId}", job.PodioItemId);
                return;
            }

            if (!int.TryParse(deskproIdString, out int deskproId))
            {
                _logger.LogError("Could not parse Deskpro Id field value as integer from Podio Item {itemId}", job.PodioItemId);
                return;
            }

            var ticketQuery = new GetTicketsQuery(deskproId, null, null);
            var ticketResult = await mediator.SendRequest(ticketQuery, cancellationToken);

            if (!ticketResult.IsSuccess || ticketResult.Value.Count() == 0)
            {
                _logger.LogWarning("No tickets found in database for DeskproId '{deskproId}'", deskproId);
                return;
            }

            if (ticketResult.Value.Count() > 1)
            {
                _logger.LogWarning("{count} tickets found in database for DeskproId '{deskproId}'", ticketResult.Value.Count(), deskproId);
                return;
            }

            // Post case to database
            var postCaseCommand = new AddCaseCommand(ticketResult.Value.First().Id, job.PodioItemId, caseNumber, null);
            var postCaseCommandResult = await mediator.SendRequest(postCaseCommand, cancellationToken);

            if (!postCaseCommandResult.IsSuccess)
            {
                _logger.LogError("Error adding case to database");
            }
        }
    }
}
