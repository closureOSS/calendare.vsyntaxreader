using System.Diagnostics.CodeAnalysis;
using Calendare.VSyntaxReader.Models;
using NodaTime;
using NodaTime.Text;

namespace Calendare.VSyntaxReader.Parsers;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.3.9
///
/// Either
/// 19970101T180000Z/19970102T070000Z
/// or
/// 19970101T180000Z/PT5H30M
/// </summary>
public static class PeriodParser
{
    public static bool TryReadPeriod(string? input, [NotNullWhen(true)] out CaldavPeriod? result)
    {
        result = null;
        if (input is null)
        {
            return false;
        }
        var parts = input.Split('/', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return false;
        }
        // start of period
        var form2UtcPattern = ZonedDateTimePattern.CreateWithInvariantCulture(@"uuuuMMdd\THHmmss\Z", DateTimeZoneProviders.Tzdb);
        var resultSop = form2UtcPattern.Parse(parts[0]);
        if (!resultSop.Success)
        {
            return false;
        }
        if (parts[1].StartsWith("P", System.StringComparison.InvariantCultureIgnoreCase))
        {
            if (DurationParser.TryReadDuration(parts[1], out var period))
            {
                result = new CaldavPeriod(new CaldavDateTime(resultSop.Value), null, period);
                return true;
            }
        }
        else
        {
            var resultEop = form2UtcPattern.Parse(parts[1]);
            if (resultEop.Success)
            {
                result = new CaldavPeriod(new CaldavDateTime(resultSop.Value), new CaldavDateTime(resultEop.Value));
                return true;
            }
        }
        return false;
    }
}
