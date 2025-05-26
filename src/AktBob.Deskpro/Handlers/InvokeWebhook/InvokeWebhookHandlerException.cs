namespace AktBob.Deskpro.Handlers.InvokeWebhook;
internal class InvokeWebhookHandlerException : IInvokeWebhookHandler
{
    private readonly IInvokeWebhookHandler _inner;
    private readonly ILogger<InvokeWebhookHandler> _logger;

    public InvokeWebhookHandlerException(IInvokeWebhookHandler inner, ILogger<InvokeWebhookHandler> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public Task<ErrorOr<Success>> Handle(string webhookId, string payload, CancellationToken cancellationToken)
    {
        try
        {
            _inner.Handle(webhookId, payload, cancellationToken);
            return Task.FromResult(ErrorOrFactory.From(Result.Success));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(InvokeWebhookHandler));
            return Task.FromResult(Error.Failure("InvokeWebhookHandler.Failure", ex.Message).ToErrorOr<Success>());
        }
    }
}