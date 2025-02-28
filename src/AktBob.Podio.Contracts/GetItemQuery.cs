using AAK.Podio.Models;
using AktBob.Shared.CQRS;
using Ardalis.Result;

namespace AktBob.Podio.Contracts;
public record GetItemQuery(int AppId, long ItemId) : IQuery<Result<Item>>;