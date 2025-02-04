using AAK.Podio;
using AAK.Podio.Models.DTOs;
using AktBob.Podio.Contracts;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Podio;
internal class UpdateItemFieldCommandHandler(IPodio podio) : MediatorRequestHandler<UpdateItemFieldCommand, Result<ItemUpdateResponseDTO>>
{
    private readonly IPodio _podio = podio;

    protected override async Task<Result<ItemUpdateResponseDTO>> Handle(UpdateItemFieldCommand request, CancellationToken cancellationToken) => await _podio.UpdateItemField(request.AppId, request.ItemId, request.FieldId, request.Value, cancellationToken);
}
