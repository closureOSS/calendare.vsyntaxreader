using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace Calendare.VSyntaxReader.Models;

public static class CaldavDateTimeExtension
{
    public static List<ZonedDateTime> ToNormalizedInZone(this IEnumerable<CaldavDateTime> dates, DateTimeZone referenceTimeZone)
    {
        return dates.Select(x => x.GetNormalizedInZone(referenceTimeZone)).Where(x => x != null).Select(x => x!.Value).ToList();
    }

    public static List<Instant> ToNormalized(this IEnumerable<CaldavDateTime> dates, DateTimeZone referenceTimeZone)
    {
        return dates.Select(x => x.ToInstant(referenceTimeZone)).Where(x => x != null).Select(x => x!.Value).ToList();
    }

    public static bool Intersects(this Interval first, Interval second)
    {
        return first.Start <= second.End && second.Start <= first.End;
    }

    public static bool Covers(this Interval first, Interval second)
    {
        return first.Start <= second.Start && first.End >= second.End;
    }

}
