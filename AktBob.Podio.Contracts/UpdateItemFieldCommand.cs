using AAK.Podio.Models.DTOs;
using Ardalis.Result;
using MediatR;

namespace AktBob.Podio.Contracts;
public record UpdateItemFieldCommand(int AppId, long ItemId, int FieldId, string Value) : IRequest<Result<ItemUpdateResponseDTO>>;