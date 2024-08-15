using AAK.Deskpro.Models;
using Ardalis.Result;
using MediatR;

namespace AktBob.CheckOCRScreeningStatus.UseCases.GetDeskproTickets;
internal record GetDeskproTicketsByFieldSearchQuery(int[] Fields, string SearchValue) : IRequest<Result<IEnumerable<Ticket>>>;