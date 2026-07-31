using System.Diagnostics.CodeAnalysis;
using System.Text;

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
        // https://datatracker.ietf.org/doc/html/rfc5545#section-3.3.11
        var unescaped = new StringBuilder(value.Length);
        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] == '\\' && i + 1 < value.Length)
            {
                var next = value[++i];
                switch (next)
                {
                    case 'n':
                    case 'N':
                        unescaped.Append('\n');
                        break;
                    case '\\':
                    case ';':
                    case ',':
                        unescaped.Append(next);
                        break;
                    default:
                        unescaped.Append('\\').Append(next);
                        break;
                }
            }
            else
            {
                unescaped.Append(value[i]);
            }
        }
        return unescaped.ToString();
    }
}
