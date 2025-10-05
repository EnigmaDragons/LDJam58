using System;
using System.Linq;

public static class StringExtensions
{
    public static string WithSpaceBetweenWords(this string s) => string.Concat(s.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

    public static string FileFriendlyName(this string s)
    {
        if (string.IsNullOrEmpty(s))
            return "";

        var result = s.Replace(" ", "")
                               .Replace("'", "")
                               .Replace("\"", "")
                               .Replace(",", "")
                               .Replace("(", "")
                               .Replace(")", "")
                               .Replace("[", "")
                               .Replace("]", "")
                               .Replace("{", "")
                               .Replace("}", "")
                               .Replace("!", "")
                               .Replace("?", "")
                               .Replace(":", "")
                               .Replace(";", "")
                               .Replace("/", "_")
                               .Replace("\\", "_")
                               .Replace(".", "_")
                               .Replace("-", "_");

        return result;
    }
}
