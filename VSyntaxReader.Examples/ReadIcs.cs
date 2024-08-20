using System;
using System.Diagnostics;
using System.Linq;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Examples.Utils;
using Calendare.VSyntaxReader.Parsers;
using Calendare.VSyntaxReader.Properties;

namespace Calendare.VSyntaxReader.Examples;

public class ReadIcs
{
    private readonly CalendarBuilder Builder = new();

    public ReadIcs()
    {
    }

    public void BasicRead(ExampleRunner library)
    {
        var fn = library.Combine("read.ics");
        var parser = new CalendarParser(Builder);
        var result = parser.TryParseFile(fn, out var vcalendar);
        if (result == false || vcalendar is null)
        {
            Console.WriteLine($"Failed to parse {fn}: {result.ErrorMessage}");
            return;
        }
        Console.WriteLine($"{vcalendar.Name}; {vcalendar.CalendarDescription}");
        foreach (var component in vcalendar.Children)
        {
            Console.WriteLine($"  {component.Name}");
            foreach (var props in component.Properties)
            {
                Console.WriteLine($"    {props.Name}");
            }
        }
    }

    public void StructuredRead(ExampleRunner library)
    {
        var fn = library.Combine("read.ics");
        var parser = new CalendarParser(Builder);
        var result = parser.TryParseFile(fn, out var vcalendar);
        if (result == false || vcalendar is null)
        {
            Console.WriteLine($"Failed to parse {fn}: {result.ErrorMessage}");
            return;
        }
        Console.WriteLine($"{vcalendar.Name}; {vcalendar.CalendarDescription}");
        foreach (var component in vcalendar.Children)
        {
            switch (component)
            {
                case VEvent vEvent:
                    Console.WriteLine($"Summary = {ReadTextProperty(vEvent, PropertyName.Summary)}");
                    Console.WriteLine($"UID = {vEvent.Uid}");
                    Console.WriteLine($"Start = {vEvent.DateStart}");
                    Console.WriteLine($"Comment = {vEvent.Description.Text()}");
                    break;

                default:
                    break;
            }
        }
    }

    private static string? ReadTextProperty(ICalendarComponent component, string propertyName)
    {
        var prop = component.FindFirstProperty<TextMultilanguageProperty>(propertyName);
        if (prop is not null && prop.Value is not null)
        {
            return prop.Value.Value;
        }
        return null;
    }

    public void HugeRead(ExampleRunner library)
    {
        var fn = library.Combine("fosdem2025.ics");
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var parser = new CalendarParser(Builder);
        var result = parser.TryParseFile(fn, out var vcalendar);
        if (result == false || vcalendar is null)
        {
            Console.WriteLine($"Failed to parse {fn}: {result.ErrorMessage}");
            return;
        }
        stopwatch.Stop();
        var events = vcalendar.Children.OfType<VEvent>();
        Console.WriteLine($"Reading and parsing {events.Count()} events took {stopwatch.ElapsedMilliseconds}ms");
        var earliest = events.MinBy(c => c.DateStart);
        if (earliest != null)
        {
            Console.WriteLine($"Earliest event starts at {earliest.DateStart}");
        }
        var latest = events.MaxBy(c => c.DateEnd);
        if (latest != null)
        {
            Console.WriteLine($"Latest event ends at {latest.DateEnd}, UTC {latest.GetInterval(latest.DateStart?.Zone)}");
        }
    }
}
