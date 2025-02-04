using AktBob.DatabaseAPI.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.DatabaseAPI.Contracts.Queries;
public record GetMessagesNotJournalizedQuery() : Request<Result<IEnumerable<MessageDto>>>;
