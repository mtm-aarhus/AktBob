using AAK.Deskpro.Models;
using Ardalis.Result;
using MediatR;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproTicketsByFieldSearchQuery(int[] Fields, string SearchValue) : IRequest<Result<IEnumerable<Ticket>>>;