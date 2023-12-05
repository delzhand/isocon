using System.Collections.Generic;

public class StringUtility
{
    public static bool InList(string first, params string[] rest)
    {
        foreach (var str in rest)
        {
            if (str.Contains(first))
            {
                return true;
            }
        }
        return false;
    }

    public static string[] Arr(params string[] items) {
        List<string> list = new();
        foreach (var str in items) {
            list.Add(str);
        }
        return list.ToArray();
    }
}
