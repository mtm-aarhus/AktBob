namespace AktBob.Shared.Processors;

public static class LogSnippets
{
    public static string MessageDeliveryCount(string messageId, int count) => $"Message {messageId} delivery count {count}";
}