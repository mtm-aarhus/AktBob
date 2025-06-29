using Aktbob.Processors.SendEmail.Client;
using Aktbob.Processors.SendEmail.Handler;
using AktBob.Shared;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using MimeKit;
using NSubstitute;

namespace AktBob.Email.Tests.Unit.Handler;

public class SendEmailHandlerTests
{
    private readonly SendEmailHandler _sut;
    private readonly string _smtpUrl = "smtpUrl";
    private readonly int _smtpPort = 123;
    private readonly string _from = "from";
    private readonly IAppConfig _appConfig = Substitute.For<IAppConfig>();
    private readonly ISmtpClient _smtpClient = Substitute.For<ISmtpClient>();
    private readonly FakeLogger<SendEmailHandler> _logger = new FakeLogger<SendEmailHandler>();
    

    public SendEmailHandlerTests()
    {
        _appConfig.GetValue<string>("EmailModule:SmtpUrl").Returns(_smtpUrl);
        _appConfig.GetValue<int>("EmailModule:SmtpPort").Returns(_smtpPort);
        _appConfig.GetValue<string>("EmailModule:From").Returns(_from);
        _sut = new SendEmailHandler(_appConfig, _smtpClient, _logger);
    }

    [Fact]
    public void Send_ShouldConnectToSmtpClient_WhenCalled()
    {
        // Arrange
        var to = "to";
        var subject = "subject";
        var body = "body";

        // Act
        _sut.Handle(to, subject, body);

        // Assert
        _smtpClient.Received(1).Connect(Arg.Is(_smtpUrl), Arg.Is(_smtpPort));
    }

    [Fact]
    public void Send_ShouldRethrowException_WhenSmtpClientThrowsException()
    {
        // Arrange
        var to = "to";
        var subject = "subject";
        var body = "body";
        _smtpClient
            .When(x => x.Connect(Arg.Any<string>(), Arg.Any<int>()))
            .Do(call => throw new Exception());

        // Act
        var act = () => _sut.Handle(to, subject, body);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Send_ShouldSendEmailViaSmtpClient_WhenCalled()
    {
        // Arrange
        var to = "to";
        var subject = "subject";
        var body = "body";
        
        // Act
        _sut.Handle(to, subject, body);

        // Assert
         _smtpClient.Received(1).Send(Arg.Any<MimeMessage>());
    }

    [Fact]
    public void Send_ShouldDisconnectFromSmtpClient_WhenSendIsComplete()
    {
        // Arrange
        var to = "to";
        var subject = "subject";
        var body = "body";

        // Act
        _sut.Handle(to, subject, body);

        // Assert
        _smtpClient.Received(1).Disconnect(Arg.Any<bool>());
    }

    [Fact]
    public void Send_ShouldRethrowException_WhenSmtpClientFailsToSend()
    {
        // Arrange
        var to = "to";
        var subject = "subject";
        var body = "body";
        _smtpClient
            .When(x => x.Send(Arg.Any<MimeMessage>()))
            .Do(call => throw new Exception());

        // Act
        var act = () => _sut.Handle(to, subject, body);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Send_ShouldThrowBusinessException_WhenRecipientIsEmpty()
    {
        // Arrange
        var to = string.Empty;
        var subject = "subject";
        var body = "body";

        // Act
        var act = () => _sut.Handle(to, subject, body);

        // Assert
        act.Should().Throw<Exception>();
    }


}
