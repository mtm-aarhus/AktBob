namespace AktBob.Email.Contracts;
public record SendEmailJob(string To, string Subject, string Body);