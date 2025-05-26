namespace AktBob.Email.Handler;
internal interface ISendEmailHandler
{
    void Handle(string to, string subject, string body, bool bodyIsHtml = false);
}
