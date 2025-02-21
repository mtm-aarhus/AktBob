using AAK.Podio.Models;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Podio.Contracts;
public record GetItemQuery(int AppId, long ItemId) : Request<Result<Item>>;