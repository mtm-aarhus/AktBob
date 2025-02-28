using AktBob.Shared.CQRS;

namespace AktBob.Email.Contracts;
public record SendEmailCommand(string To, string Subject, string Body, bool IsBodyHtml = true) : ICommand;