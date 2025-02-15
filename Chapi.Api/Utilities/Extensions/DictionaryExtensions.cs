namespace Chapi.Api.Utilities.Extensions
{
    public static class DictionaryExtensions
    {
        public static T? TryGetNullable<T>(this Dictionary<string,T> dictionary, string key)
        {
            return dictionary.TryGetValue(key, out var foundItem) == true
            ? foundItem
            : default; 
        }
    }
}
