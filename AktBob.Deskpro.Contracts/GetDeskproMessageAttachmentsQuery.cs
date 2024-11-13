using AktBob.Deskpro.Contracts.DTOs;
using Ardalis.Result;
using MediatR;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproMessageAttachmentsQuery(int TicketId, int MessageId) : IRequest<Result<IEnumerable<AttachmentDto>>>;