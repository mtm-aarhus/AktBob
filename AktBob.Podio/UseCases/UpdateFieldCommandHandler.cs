using AAK.Podio;
using AAK.Podio.Models.DTOs;
using AktBob.Podio.Contracts;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Podio.UseCases;
public class UpdateFieldCommandHandler(IPodio podio) : MediatorRequestHandler<UpdateFieldCommand, Result<ItemUpdateResponseDTO>>
{
    private readonly IPodio _podio = podio;

    protected override async Task<Result<ItemUpdateResponseDTO>> Handle(UpdateFieldCommand request, CancellationToken cancellationToken) => await _podio.UpdateItemField(request.AppId, request.ItemId, request.FieldId, request.Value, cancellationToken);
}
