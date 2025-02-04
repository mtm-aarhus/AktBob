namespace AktBob.Email.UseCases.SendEmail;
public record SendEmailCommand(string To, string Subject, string Body, bool IsBodyHtml = true);