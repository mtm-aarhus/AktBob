namespace AktBob.Deskpro.Contracts;
public record InvokeWebhookCommand(string WebhookId, object Payload);