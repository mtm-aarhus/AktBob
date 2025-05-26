namespace AktBob.Email.Contracts;

internal record SendEmailJob(string To, string Base64Subject, string Base64Body, bool bodyIsHtml = false);