using AktBob.Deskpro.Contracts.DTOs;
using Ardalis.Result;
using MediatR;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproPersonQuery(int PersonId) : IRequest<Result<PersonDto>>;