using NSubstitute;
using System.Text;

namespace AktBob.Email.Tests.Unit;
public class SendEmailJobHandlerTests
{
    private readonly SendEmailJobHandler _sut;
    private readonly IEmail _email = Substitute.For<IEmail>();

    public SendEmailJobHandlerTests()
    {
        _sut = new SendEmailJobHandler(_email);
    }

    [Fact]
    public async Task Handle_ShouldCallEmailSendWithDecodedValues_WhenJobIsHandled()
    {
        // Arrange
        var to = "to";
        var subject = "subject";
        var base64Subject = Convert.ToBase64String(Encoding.UTF8.GetBytes(subject));
        var body = "body";
        var base64Body = Convert.ToBase64String(Encoding.UTF8.GetBytes(body));
        var job = new SendEmailJob(to, base64Subject, base64Body);

        // Act
        await _sut.Handle(job, CancellationToken.None);

        // Assert
        await _email.Received(1).Send(Arg.Is(to), Arg.Is(subject), Arg.Is(body));
    }
}
