using System;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Examples.Utils;
using Calendare.VSyntaxReader.Operations;
using Calendare.VSyntaxReader.Parsers;
using NodaTime;

namespace Calendare.VSyntaxReader.Examples;

public class OccurrenceExamples
{
    private readonly CalendarBuilder Builder = new();

    public OccurrenceExamples()
    {
    }

    public void Basic(ExampleRunner library)
    {
        var fn = library.Combine("occurrences.ics");
        var parser = new CalendarParser(Builder);
        var result = parser.TryParseFile(fn, out var vcalendar);
        if (result == false || vcalendar is null)
        {
            Console.WriteLine($"Failed to parse {fn}: {result.ErrorMessage}");
            return;
        }
        var sop = new ZonedDateTime(new LocalDateTime(2007, 03, 01, 0, 0), DateTimeZone.Utc, Offset.Zero);
        var eop = new ZonedDateTime(new LocalDateTime(2007, 06, 30, 23, 59), DateTimeZone.Utc, Offset.Zero);
        var evalPeriod = new Interval(sop.ToInstant(), eop.ToInstant());
        var occurrences = vcalendar.Children.GetOccurrences(evalPeriod, DateTimeZone.Utc);
        // the usecase contains additional dates before DTSTART in April and also in May
        // the usecase contains exceptions for 3 regular dates (15th May, 12th+19th June)
        foreach (var dbgOcc in occurrences)
        {
            Console.WriteLine($"{dbgOcc.Interval}");
        }
    }


}
