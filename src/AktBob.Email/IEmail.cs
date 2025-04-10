namespace AktBob.Email;
internal interface IEmail
{
    void Send(string to, string subject, string body, bool bodyIsHtml = false);
}
