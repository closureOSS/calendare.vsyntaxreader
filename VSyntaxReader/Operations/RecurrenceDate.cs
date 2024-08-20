using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace Calendare.VSyntaxReader.Operations;

public record RecurrenceDate(ZonedDateTime ZonedDateTime, bool expandOnly = false)
{
    public Instant ToInstant()
    {
        return ZonedDateTime.ToInstant();
    }

    public LocalDateTime LocalDateTime => ZonedDateTime.LocalDateTime;
}

public static class RecurrenceDateExtension
{
    public static void AddRange(this List<RecurrenceDate> list, IEnumerable<ZonedDateTime> dates, bool expandOnly = false)
    {
        list.AddRange(dates.Select(dt => new RecurrenceDate(dt, expandOnly)));
    }

    public static void Add(this List<RecurrenceDate> list, ZonedDateTime date, bool expandOnly = false)
    {
        list.Add(new(date, expandOnly));
    }
}
