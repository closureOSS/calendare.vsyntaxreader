using System.Diagnostics.CodeAnalysis;
using NodaTime;
using NodaTime.Text;

namespace Calendare.VSyntaxReader.Parsers;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.3.6
/// </summary>
public static class DurationParser
{
    public static bool TryReadDuration(string? input, [NotNullWhen(true)] out Period? result)
    {
        result = null;
        if (input is null)
        {
            return false;
        }
        var match = PeriodPattern.NormalizingIso.Parse(input);
        if (match.Success)
        {
            result = match.Value;
            return true;
        }
        return false;
    }
}
