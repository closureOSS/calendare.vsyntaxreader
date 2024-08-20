using System.Diagnostics.CodeAnalysis;
using Calendare.VSyntaxReader.Models;
using NodaTime;
using NodaTime.Text;

namespace Calendare.VSyntaxReader.Parsers;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.3.4 for DATE
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.3.5 for DATE-TIME
/// </summary>
public static class DateTimeParser
{
    public static bool TryReadDateTime(string? input, DateTimeZone? timeZone, [NotNullWhen(true)] out CaldavDateTime? result)
    {
        result = null;
        if (input is null)
        {
            return false;
        }
        var form2UtcPattern = ZonedDateTimePattern.CreateWithInvariantCulture(@"uuuuMMdd\THHmmss\Z", DateTimeZoneProviders.Tzdb);
        var result2UtcPattern = form2UtcPattern.Parse(input);
        if (result2UtcPattern.Success)
        {
            // FORM #2: DATE WITH UTC TIME
            result = new CaldavDateTime(result2UtcPattern.Value);
            return true;
        }
        var form2FloatingPattern = LocalDateTimePattern.CreateWithInvariantCulture(@"uuuuMMdd\THHmmss");
        var result2FloatingPattern = form2FloatingPattern.Parse(input);
        if (result2FloatingPattern.Success)
        {
            if (timeZone is not null)
            {
                // FORM #3: DATE WITH LOCAL TIME AND TIME ZONE REFERENCE (TZID Parameter must exist)
                ZonedDateTime zdt = timeZone.AtLeniently(result2FloatingPattern.Value);
                result = new CaldavDateTime(zdt);
                return true;
            }
            else
            {
                // FORM #1: DATE WITH LOCAL TIME
                result = new CaldavDateTime(result2FloatingPattern.Value);
                return true;
            }
        }
        return false;
    }

    public static bool TryReadDateOnly(string? input, DateTimeZone? timeZone, [NotNullWhen(true)] out CaldavDateTime? result)
    {
        result = null;
        if (input is null)
        {
            return false;
        }
        var form2FloatingDateOnlyPattern = LocalDatePattern.CreateWithInvariantCulture(@"uuuuMMdd");
        var result2FloatingDateOnlyPattern = form2FloatingDateOnlyPattern.Parse(input);
        if (result2FloatingDateOnlyPattern.Success)
        {
            if (timeZone is not null)
            {
                // NOT SPECIFIED IN RFC, OWN INTERPRETATION
                ZonedDateTime zdt = timeZone.AtStartOfDay(result2FloatingDateOnlyPattern.Value);
                result = new CaldavDateTime(zdt, true);
                return true;
            }
            else
            {
                // FLOATING DATE (NO LOCAL TIME)
                result = new CaldavDateTime(result2FloatingDateOnlyPattern.Value);
                return true;
            }
        }
        return false;
    }
}
