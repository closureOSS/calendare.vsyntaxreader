using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Calendare.VSyntaxReader.Properties;

namespace Calendare.VSyntaxReader.Parsers;

/// <summary>
/// Original source for regex expressions and comments is Ical.Net \src\Ical.Net\Serialization\SimpleDeserializer.cs
/// </summary>
partial class LineParser
{
    private const string NameGroup = "name";
    private const string ValueGroup = "value";
    private const string ParamNameGroup = "paramName";
    private const string ParamValueGroup = "paramValue";

    // name          = iana-token / x-name
    // iana-token    = 1*(ALPHA / DIGIT / "-")
    // x-name        = "X-" [vendorid "-"] 1*(ALPHA / DIGIT / "-")
    // vendorid      = 3*(ALPHA / DIGIT)
    // Add underscore to match behavior of bug 2033495
    private const string Identifier = "[-A-Za-z0-9_]+";

    // param-value   = paramtext / quoted-string
    // paramtext     = *SAFE-CHAR
    // quoted-string = DQUOTE *QSAFE-CHAR DQUOTE
    // QSAFE-CHAR    = WSP / %x21 / %x23-7E / NON-US-ASCII
    // ; Any character except CONTROL and DQUOTE
    // SAFE-CHAR     = WSP / %x21 / %x23-2B / %x2D-39 / %x3C-7E
    //               / NON-US-ASCII
    // ; Any character except CONTROL, DQUOTE, ";", ":", ","
    private const string ParamValue = $"((?<{ParamValueGroup}>[^\\x00-\\x08\\x0A-\\x1F\\x7F\";:,]*)|\"(?<{ParamValueGroup}>[^\\x00-\\x08\\x0A-\\x1F\\x7F\"]*)\")";

    // param         = param-name "=" param-value *("," param-value)
    // param-name    = iana-token / x-name
    private const string ParamName = $"(?<{ParamNameGroup}>{Identifier})";
    private const string Param = $"{ParamName}={ParamValue}(,{ParamValue})*";

    // contentline   = name *(";" param ) ":" value CRLF
    private const string Name = $"(?<{NameGroup}>{Identifier})";
    // value         = *VALUE-CHAR
    private const string Value = $"(?<{ValueGroup}>[^\\x00-\\x08\\x0E-\\x1F\\x7F]*)";

    [GeneratedRegex($"^{Name}(;{Param})*:{Value}$", RegexOptions.Compiled)]
    private static partial Regex ContentLineRegex();

    public static bool TryParse(string line, [NotNullWhen(true)] out CalendarObject? prop)
    {
        prop = null;
        var match = ContentLineRegex().Match(line);
        if (!match.Success)
        {
            return false;
        }
        var name = match.Groups[NameGroup].Value;
        var value = match.Groups[ValueGroup].Value;
        var paramNames = match.Groups[ParamNameGroup].Captures;
        var paramValues = match.Groups[ParamValueGroup].Captures;
        List<CalendarObjectParameter> parameters = [];
        if (paramNames.Count > 0)
        {
            if (paramNames.Count >= paramValues.Count)
            {
                for (var p = 0; p < paramNames.Count; p++)
                {
                    var unescapedParam = ParameterExtensions.Unescape(paramValues[p].Value);
                    parameters.Add(new(paramNames[p].Value, unescapedParam));
                }
            }
            else
            {
                return false;
            }
        }
        prop = new CalendarObject(name, value, parameters);
        return true;
    }

}
