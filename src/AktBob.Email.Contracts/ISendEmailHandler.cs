namespace AktBob.Email.Contracts;
public interface ISendEmailHandler
{
    Task Handle(string to, string subject, string body, bool isBodyHtml, CancellationToken cancellationToken);
}