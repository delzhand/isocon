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
}
