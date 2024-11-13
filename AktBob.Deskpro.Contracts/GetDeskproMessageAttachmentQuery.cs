using Ardalis.Result;
using MediatR;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproMessageAttachmentQuery(string DownloadUrl) : IRequest<Result<Stream>>;