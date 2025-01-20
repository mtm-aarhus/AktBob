namespace AktBob.JournalizeDocuments;
internal static class KeyValuePairExtensions
{
    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp) where TKey : notnull
    {
        return new Dictionary<TKey, TValue>
        {
            { kvp.Key, kvp.Value }
        };
    }
}
