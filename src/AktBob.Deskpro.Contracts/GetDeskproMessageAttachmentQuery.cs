using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproMessageAttachmentQuery(string DownloadUrl) : Request<Result<Stream>>;