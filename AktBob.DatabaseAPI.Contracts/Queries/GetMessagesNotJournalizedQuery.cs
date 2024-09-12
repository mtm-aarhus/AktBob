using AktBob.DatabaseAPI.Contracts.DTOs;
using Ardalis.Result;
using MediatR;

namespace AktBob.DatabaseAPI.Contracts.Queries;
public record GetMessagesNotJournalizedQuery() : IRequest<Result<IEnumerable<MessageDto>>>;
