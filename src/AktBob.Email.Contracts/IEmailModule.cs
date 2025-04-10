namespace AktBob.Email.Contracts;

public interface IEmailModule
{
    void Send(string to, string subject, string body, bool bodyIsHtml = false);
}
