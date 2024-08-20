using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace Calendare.VSyntaxReader.Models;

public static class CaldavPeriodExtension
{
    public static List<ZonedDateTime> ToNormalizedInZone(this IEnumerable<CaldavPeriod> dates, DateTimeZone referenceTimeZone)
    {
        return dates.Select(x => x.Start.GetNormalizedInZone(referenceTimeZone)).Where(x => x != null).Select(x => x!.Value).ToList();
    }

    public static List<Instant> ToNormalized(this IEnumerable<CaldavPeriod> dates, DateTimeZone referenceTimeZone)
    {
        return dates.Select(x => x.Start.ToInstant(referenceTimeZone)).Where(x => x != null).Select(x => x!.Value).ToList();
    }
}
