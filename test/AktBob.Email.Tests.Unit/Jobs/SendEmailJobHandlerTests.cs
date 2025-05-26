using System.Text;
using AktBob.Email.Contracts;
using AktBob.Email.Handler;
using AktBob.Email.Jobs;
using NSubstitute;

namespace AktBob.Email.Tests.Unit.Jobs;
public class SendEmailJobHandlerTests
{
    private readonly SendEmailJobHandler _sut;
    private readonly ISendEmailHandler _sendEmailHandler = Substitute.For<ISendEmailHandler>();

    public SendEmailJobHandlerTests()
    {
        _sut = new SendEmailJobHandler(_sendEmailHandler);
    }

    [Fact]
    public void Handle_ShouldCallEmailSendWithDecodedValues_WhenJobIsHandled()
    {
        // Arrange
        var to = "to";
        var subject = "subject";
        var base64Subject = Convert.ToBase64String(Encoding.UTF8.GetBytes(subject));
        var body = "body";
        var base64Body = Convert.ToBase64String(Encoding.UTF8.GetBytes(body));
        var job = new SendEmailJob(to, base64Subject, base64Body);

        // Act
        _sut.Handle(job, CancellationToken.None);

        // Assert
        _sendEmailHandler.Received(1).Handle(Arg.Is(to), Arg.Is(subject), Arg.Is(body));
    }
}
