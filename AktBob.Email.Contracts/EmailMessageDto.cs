namespace AktBob.Email.Contracts;

public record EmailMessageDto(string To, string Subject, string Body);