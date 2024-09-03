using AAK.Podio;
using AAK.Podio.Models.DTOs;
using AktBob.Podio.Contracts;
using Ardalis.Result;
using MediatR;

namespace AktBob.Podio;
internal class UpdateItemFieldCommandHandler : IRequestHandler<UpdateItemFieldCommand, Result<ItemUpdateResponseDTO>>
{
    private readonly IPodio _podio;

    public UpdateItemFieldCommandHandler(IPodio podio)
    {
        _podio = podio;
    }

    public async Task<Result<ItemUpdateResponseDTO>> Handle(UpdateItemFieldCommand request, CancellationToken cancellationToken) => await _podio.UpdateItemField(request.AppId, request.ItemId, request.FieldId, request.Value, cancellationToken);
}
