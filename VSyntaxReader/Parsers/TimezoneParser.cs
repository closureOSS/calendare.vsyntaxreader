using System;
using System.Collections.Generic;
using System.Linq;
using Calendare.VSyntaxReader.Properties;
using NodaTime;
using NodaTime.TimeZones;

namespace Calendare.VSyntaxReader.Parsers;


public delegate DateTimeZone? TimezoneResolverFn(string tzId);

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.3.4 for DATE
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.3.5 for DATE-TIME
/// </summary>
public static class TimezoneParser
{
    public static DeserializeResult TryReadTimezone(this List<CalendarObjectParameter> parameters, out DateTimeZone? timeZone, TimezoneResolverFn? resolver = null)
    {
        var tzId = parameters.FirstOrDefault(z => z.Name.Equals(DateTimeProperty.TimezoneIdParam, StringComparison.InvariantCultureIgnoreCase));
        if (tzId is null || tzId.Value is null)
        {
            timeZone = null;
            return new(true);
        }
        if (TryReadTimezone(tzId.Value, out timeZone, resolver))
        {
            return new(true);
        }
        return new(false, $"Parsing timezone [{tzId}] failed");
    }

    public static bool TryReadTimezone(string tzId, out DateTimeZone? timeZone, TimezoneResolverFn? resolver = null)
    {
        timeZone = ResolveTimeZone(tzId);
        if (timeZone is null)
        {
            //US/Eastern is commonly represented as US-Eastern
            var newTzId = tzId.Replace("-", "/");
            timeZone = ResolveTimeZone(newTzId);
        }
        if (timeZone is null)
        {
            var newUri = tzId.Split('/');
            if (newUri.Length > 2)
            {
                var newTzId = string.Join('/', newUri[^2..]);
                timeZone = ResolveTimeZone(newTzId);
            }
        }
        if (timeZone is null && resolver is not null)
        {
            timeZone = resolver(tzId);
        }
        return timeZone is not null;
    }


    public static DateTimeZone? ResolveTimeZone(string tzId)
    {
        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tzId);
        if (timeZone is null && TzdbDateTimeZoneSource.Default.WindowsToTzdbIds.TryGetValue(tzId, out var olsenId))
        {
            timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(olsenId);
        }
        return timeZone;
    }
}
