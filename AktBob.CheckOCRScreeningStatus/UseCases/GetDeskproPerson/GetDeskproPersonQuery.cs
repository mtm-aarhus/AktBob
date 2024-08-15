using AAK.Deskpro.Models;
using Ardalis.Result;
using MediatR;

namespace AktBob.CheckOCRScreeningStatus.UseCases.GetDeskproPerson;
internal record GetDeskproPersonQuery(int PersonId) : IRequest<Result<Person>>;