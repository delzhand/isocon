using System.Collections.Generic;
using System.Linq;

public class CollectionUtility
{
    public static T[] AddToArray<T>(T[] array, T item, bool unique = false)
    {
        List<T> list = array.ToList();
        if (!unique || !list.Contains(item))
        {
            list.Add(item);
        }
        return list.ToArray();
    }

    public static T[] RemoveFromArray<T>(T[] array, T item)
    {
        List<T> list = array.ToList();
        list.Remove(item);
        return list.ToArray();
    }

    public static T[] RemoveAllFromArray<T>(T[] array, T item)
    {
        List<T> list = array.ToList();
        while (list.Contains(item))
        {
            list.Remove(item);
        }
        return list.ToArray();
    }

    public static int CountInArray<T>(T[] array, T item)
    {
        int i = 0;
        foreach (T t in array)
        {
            if (t.Equals(item))
            {
                i++;
            }
        }
        return i;
    }

    public static bool CheckInList<T>(T first, params T[] rest)
    {
        foreach (T t in rest)
        {
            if (t.Equals(first))
            {
                return true;
            }
        }
        return false;
    }
}
