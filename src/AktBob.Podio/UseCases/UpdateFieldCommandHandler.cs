using AAK.Podio;
using AktBob.Podio.Contracts;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio.UseCases;
public class UpdateFieldCommandHandler(IPodioFactory podioFactory, ILogger<UpdateFieldCommandHandler> logger, IConfiguration configuration) : MediatorRequestHandler<UpdateFieldCommand>
{
    private readonly IPodioFactory _podioFactory = podioFactory;
    private readonly ILogger<UpdateFieldCommandHandler> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    protected override async Task Handle(UpdateFieldCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating Podio item. ItemId {itemId} FielId {fieldId} Value: '{value}'", command.ItemId, command.FieldId, command.Value);

        var podio = _podioFactory.Create(command.AppId, ConfigurationHelper.GetAppToken(_configuration, command.AppId), ConfigurationHelper.GetClientId(_configuration), ConfigurationHelper.GetClientSecret(_configuration));
        await podio.UpdateItemField(command.AppId, command.ItemId, command.FieldId, command.Value, cancellationToken);

        _logger.LogInformation("Podio item updated. ItemId {itemId} FieldId {fieldId} Value: '{value}'", command.ItemId, command.FieldId, command.Value);

    }
}
