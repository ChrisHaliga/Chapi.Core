namespace Chapi.Api.Utilities.Extensions
{
    public static class ListExtensions
    {
        public static void AddIfNotExists(this List<string> list, string item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }


        public static List<T> AddIfNotExists<T>(this List<T> list, T item, Predicate<T> query)
        {
            var i = list.FindIndex(query);

            if (i == -1)
            {
                list.Add(item);
            }

            return list;
        }

        public static bool Remove<T>(this List<T> list, Predicate<T> query)
        {
            var i = list.FindIndex(query);

            if (i == -1)
            {
                list.RemoveAt(i);
                return true;
            }

            return false;
        }
    } 
}
