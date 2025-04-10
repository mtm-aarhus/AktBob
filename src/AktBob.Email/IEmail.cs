namespace AktBob.Email;
internal interface IEmail
{
    Task Send(string to, string subject, string body);
}
