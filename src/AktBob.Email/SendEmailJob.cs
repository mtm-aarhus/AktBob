namespace AktBob.Email.Contracts;
internal record SendEmailJob(string To, string Subject, string Body);