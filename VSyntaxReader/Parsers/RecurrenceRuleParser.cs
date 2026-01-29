using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Calendare.VSyntaxReader.Models;
using NodaTime;

namespace Calendare.VSyntaxReader.Parsers;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.3.10
/// </summary>
public static partial class RecurrenceRuleParser
{
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
    private const string ParamValue = $"(([^\\x00-\\x08\\x0A-\\x1F\\x7F\",;]*)|\"([^\\x00-\\x08\\x0A-\\x1F\\x7F\",;]*)\")";

    // param         = param-name "=" param-value *("," param-value)
    // param-name    = iana-token / x-name
    private const string ParamName = $"(?<{ParamNameGroup}>{Identifier})";
    private const string Param = $"{ParamName}=(?<{ParamValueGroup}>{ParamValue}(,{ParamValue})*)";


    [GeneratedRegex($"^({Param})+(;{Param})*;?$", RegexOptions.Compiled)]
    public static partial Regex ParamRegex();

    public static bool TryReadRule(string? input, [NotNullWhen(true)] out List<CalendarObjectParameter>? result)
    {
        result = null;
        if (input is null)
        {
            return false;
        }
        var match = ParamRegex().Match(input);
        var attrNames = match.Groups[ParamNameGroup].Captures;
        var attrValues = match.Groups[ParamValueGroup].Captures;
        List<CalendarObjectParameter> parameters = [];
        if (attrNames.Count > 0)
        {
            if (attrNames.Count >= attrValues.Count)
            {
                for (var p = 0; p < attrNames.Count; p++)
                {
                    parameters.Add(new(attrNames[p].Value, attrValues[p].Value));
                }
                result = parameters;
                return true;
            }
        }
        return false;
    }

    public static bool TryReadArray(this string? input, [NotNullWhen(true)] out List<string>? result)
    {
        result = null;
        if (input is null)
        {
            return false;
        }
        var items = input.Split(',', System.StringSplitOptions.TrimEntries | System.StringSplitOptions.RemoveEmptyEntries);
        if (items is null || items.Length == 0)
        {
            return false;
        }
        result = [.. items];
        return true;
    }

    public static bool TryReadIntArray(this string? input, [NotNullWhen(true)] out List<int>? result, int minRange = int.MinValue, int maxRange = int.MaxValue)
    {
        result = null;
        if (!input.TryReadArray(out var items))
        {
            return false;
        }
        bool includeZero = minRange == 0 || maxRange == 0;  // work's for RRULE attributes
        result = [];
        foreach (var item in items ?? [])
        {
            if (int.TryParse(item, null, out var val) && val >= minRange && val <= maxRange && (val != 0 || includeZero))
            {
                result.Add(val);
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    [GeneratedRegex(@"^(?<offset>[-+]?[0-9]{1,2})?(?<weekday>[A-Z]{2})$")]
    private static partial Regex DayOfWeekRegex();

    public static bool TryReadDayOfWeekArray(this string? input, [NotNullWhen(true)] out List<DayOfWeekOffset>? result, bool allowOffset = true)
    {
        result = null;
        if (!input.TryReadArray(out var weekdays))
        {
            return false;
        }
        result = [];
        foreach (var wk in weekdays ?? [])
        {
            var match = DayOfWeekRegex().Match(wk);
            if (!match.Success)
            {
                return false;
            }
            var wkGrp = match.Groups["weekday"].Captures;
            if (wkGrp.Count != 1)
            {
                return false;
            }
            var wkpart = wkGrp[0].ToString();
            if (!wkpart.TryReadDayOfWeek(out var weekday))
            {
                return false;
            }
            var current = new DayOfWeekOffset(weekday);
            var offsetGrp = match.Groups["offset"].Captures;
            if (offsetGrp.Count != 0)
            {
                if (!allowOffset)
                {
                    return false;
                }
                if (int.TryParse(offsetGrp[0].ToString(), null, out var offset) && offset >= -53 && offset <= 53 && offset != 0)
                {
                    current = current with { Offset = offset };
                }
            }
            result.Add(current);
        }
        return true;
    }

    public static bool TryReadDayOfWeek(this string? input, [NotNullWhen(true)] out IsoDayOfWeek result)
    {
        result = input?.ToUpperInvariant() switch
        {
            "MO" => IsoDayOfWeek.Monday,
            "TU" => IsoDayOfWeek.Tuesday,
            "WE" => IsoDayOfWeek.Wednesday,
            "TH" => IsoDayOfWeek.Thursday,
            "FR" => IsoDayOfWeek.Friday,
            "SA" => IsoDayOfWeek.Saturday,
            "SU" => IsoDayOfWeek.Sunday,
            _ => IsoDayOfWeek.None
        };
        return result != IsoDayOfWeek.None;
    }

    public static string ToStringShort(this IsoDayOfWeek dow)
    {
        return dow switch
        {
            IsoDayOfWeek.Monday => "MO",
            IsoDayOfWeek.Tuesday => "TU",
            IsoDayOfWeek.Wednesday => "WE",
            IsoDayOfWeek.Thursday => "TH",
            IsoDayOfWeek.Friday => "FR",
            IsoDayOfWeek.Saturday => "SA",
            IsoDayOfWeek.Sunday => "SU",
            _ => "",
        };
    }

    // source https://stackoverflow.com/questions/3669970/compare-two-listt-objects-for-equality-ignoring-order
    public static bool ScrambledEquals<T>(this IEnumerable<T> list1, IEnumerable<T> list2) where T : struct
    {
        var cnt = new Dictionary<T, int>();
        foreach (T s in list1)
        {
            if (cnt.TryGetValue(s, out var value))
            {
                cnt[s] = ++value;
            }
            else
            {
                cnt.Add(s, 1);
            }
        }
        foreach (T s in list2)
        {
            if (cnt.TryGetValue(s, out var value))
            {
                cnt[s] = --value;
            }
            else
            {
                return false;
            }
        }
        return cnt.Values.All(c => c == 0);
    }
}
