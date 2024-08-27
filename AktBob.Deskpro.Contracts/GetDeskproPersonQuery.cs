using AAK.Deskpro.Models;
using Ardalis.Result;
using MediatR;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproPersonQuery(int PersonId) : IRequest<Result<Person>>;