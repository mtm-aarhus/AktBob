using AAK.Podio;
using AktBob.Podio.Contracts;
using MassTransit.Mediator;

namespace AktBob.Podio.UseCases;
public class UpdateFieldCommandHandler(IPodio podio) : MediatorRequestHandler<UpdateFieldCommand>
{
    private readonly IPodio _podio = podio;

    protected override async Task Handle(UpdateFieldCommand request, CancellationToken cancellationToken)
    {
        await _podio.UpdateItemField(request.AppId, request.ItemId, request.FieldId, request.Value, cancellationToken);
    }
}
