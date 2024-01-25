using System.Collections.Generic;

public class StringUtility
{
    public static bool CheckInList(string first, params string[] rest)
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

    public static string[] CreateArray(params string[] items)
    {
        List<string> list = new();
        foreach (var str in items)
        {
            list.Add(str);
        }
        return list.ToArray();
    }

    public static string ConvertIntToAlpha(int x)
    {
        const int Base = 26;
        const int Offset = 64; // ASCII offset for uppercase letters
        string column = "";
        while (x > 0)
        {
            int remainder = x % Base;
            char letter = (char)(remainder + Offset);

            column = letter + column;
            x = (x - 1) / Base; // Adjust number for next iteration
        }
        return column;
    }

}
