using Ardalis.Result;
using MediatR;
using System.ComponentModel;

namespace AktBob.Queue.Contracts;
public record GetQueueMessagesQuery(string ConnectionString, string QueueName, int MaxMessages = 10, int VisibilityvisibilyTimeoutSeconds = 60) : IRequest<Result<IEnumerable<QueueMessageDto>>>;