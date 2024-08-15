using MediatR;

namespace AktBob.Email.UseCases.SendEmail;
internal record SendEmailCommand(string To, string Subject, string Body, bool IsBodyHtml = true) : IRequest;