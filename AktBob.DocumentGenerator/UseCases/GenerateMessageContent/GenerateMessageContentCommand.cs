using AktBob.DocumentGenerator.Contracts;
using MediatR;
using MigraDoc.DocumentObjectModel;

namespace AktBob.DocumentGenerator.UseCases.GenerateMessageContent;
internal record GenerateMessageContentCommand(Section Section, MessageDetailsDto MessageDetailsDto) : IRequest<Section>;