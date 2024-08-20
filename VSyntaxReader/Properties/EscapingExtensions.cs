using System.Diagnostics.CodeAnalysis;

namespace Calendare.VSyntaxReader.Properties;

public static class EscapingExtensions
{
    [return: NotNullIfNotNull(nameof(value))]
    public static string? EscapeText(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        var escaped = value;
        // https://datatracker.ietf.org/doc/html/rfc5545#section-3.3.11
        escaped = escaped.Replace("\\", "\\\\");
        escaped = escaped.Replace(",", "\\,");
        escaped = escaped.Replace(";", "\\;");
        escaped = escaped.Replace("\n", "\\n");
        return escaped;
    }

    [return: NotNullIfNotNull(nameof(value))]
    public static string? UnescapeText(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        var unescaped = value;
        // https://datatracker.ietf.org/doc/html/rfc5545#section-3.3.11
        unescaped = unescaped.Replace("\\\\", "\\");
        unescaped = unescaped.Replace("\\n", "\n");
        unescaped = unescaped.Replace("\\N", "\n");
        unescaped = unescaped.Replace("\\;", ";");
        unescaped = unescaped.Replace("\\,", ",");
        return unescaped;
    }
}
