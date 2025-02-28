using AAK.Podio.Models;
using Ardalis.Result;
using MediatR;

namespace AktBob.Podio.Contracts;
public record GetItemQuery(int AppId, long ItemId) : IRequest<Result<Item>>;