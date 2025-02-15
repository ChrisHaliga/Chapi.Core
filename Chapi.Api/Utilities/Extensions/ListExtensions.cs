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

        public static (List<string> UniqueToList1, List<string> UniqueToList2, List<string> common) Diff(this List<string> list1, List<string>? list2)
        {
            if(list2 == null)
            {
                return (list1, [], []);
            }

            List<string> common = new List<string>();
            List<string> uniqueToList1 = new List<string>();

            for (var i = 0; i < list1.Count; i++)
            {
                bool match = false;
                for (var j = 0; j < list2.Count; j++)
                {
                    if(list1[i] == list2[j])
                    {
                        common.Add(list1[i]);
                        list2.RemoveAt(j);
                        match = true;
                        break;
                    }
                }
                if (!match)
                {
                    uniqueToList1.Add(list1[i]);
                }
            }

            return (uniqueToList1, list2, common);
        }
    } 
}
