using AktBob.DatabaseAPI.Contracts.DTOs;
using Ardalis.Result;
using MediatR;

namespace AktBob.DatabaseAPI.Contracts.Queries;
public record GetMessageByDeskproMessageIdQuery(int DeskproMessageId) : IRequest<Result<MessageDto>>;
