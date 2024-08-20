using System;
using System.Linq;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Examples.Utils;
using Calendare.VSyntaxReader.Parsers;
using NodaTime;

namespace Calendare.VSyntaxReader.Examples;

public class WriteIcs
{
    private readonly CalendarBuilder Builder = new();
    private readonly LocalDateTime SomeLocalTime = new(2025, 06, 07, 08, 09, 10);

    public WriteIcs()
    {
    }

    public void BasicWrite(ExampleRunner library)
    {
        var fn = library.Combine("read.ics");
        var parser = new CalendarParser(Builder);
        var result = parser.TryParseFile(fn, out var vcalendar);
        if (result == false || vcalendar is null)
        {
            Console.WriteLine($"Failed to parse {fn}: {result.ErrorMessage}");
            return;
        }
        Console.Write(vcalendar.Serialize());
    }

    public void BasicAmendWrite(ExampleRunner library)
    {
        var fn = library.Combine("read.ics");
        var parser = new CalendarParser(Builder);
        var result = parser.TryParseFile(fn, out var vcalendar);
        if (result == false || vcalendar is null)
        {
            Console.WriteLine($"Failed to parse {fn}: {result.ErrorMessage}");
            return;
        }
        var rc = vcalendar.Children.OfType<RecurringComponent>().FirstOrDefault();
        if (rc != null)
        {
            var tz = rc.DateStart?.Zone;
            if (tz != null)
            {
                var zdt = SomeLocalTime.InZoneLeniently(tz);
                rc.DateStart = new Models.CaldavDateTime(zdt);
            }
            // else
            // {
            //     rc.DateStart = new Models.CaldavDateTime(SomeLocalTime);
            // }
            rc.DateStamp = SystemClock.Instance.GetCurrentInstant();
            if (rc is VEvent vEvent)
            {
                vEvent.Description.Set("Um Ihren Palm-Handheld elektronisch zu registrieren, benötigen Sie eine Internetverbindung oder ein an Ihren Computer angeschlossenes Modem.", "de");
                vEvent.Description.Set("Para registrar su dispositivo Palm electrónicamente, debe tener una conexión a Internet o un módem conectado a su computadora.", "es");
                vEvent.Summary.Set("Wiederholdendes Ereignis", "de");
                vEvent.Summary.Set("evento repetido", "es");
            }
            Console.Write(vcalendar.Serialize());
        }
    }

    public void FromScratchWrite(ExampleRunner _)
    {
        var vcalendar = Builder.CreateCalendar();
        vcalendar.CalendarDescription = "A simple calendar generated from scratch";

        var vEvent = vcalendar.CreateChild<VEvent>();
        if (vEvent == null) return;
        if (!TimezoneParser.TryReadTimezone("Europe/Zurich", out var timezoneZH)) return;
        var zdt = SomeLocalTime.InZoneLeniently(timezoneZH!);
        vEvent.DateStart = new Models.CaldavDateTime(zdt);
        vEvent.Duration = Period.FromMinutes(90);
        // OR: vEvent.DateEnd = new Models.CaldavDateTime(zdt.PlusHours(1).PlusMinutes(30));
        vEvent.Summary.Set("Some event");
        Console.WriteLine($"Event starts/ends in UTC at {vEvent.GetInterval(vEvent.DateStart.Zone)}");
        Console.Write(vcalendar.Serialize());
    }
}
