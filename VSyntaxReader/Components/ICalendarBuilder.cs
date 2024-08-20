using System;
using Calendare.VSyntaxReader.Parsers;
using Calendare.VSyntaxReader.Properties;

namespace Calendare.VSyntaxReader.Components;

public interface ICalendarBuilder
{
    public ICalendarParser Parser { get; }
    public VCalendar CreateCalendar();
    public bool TryLookupComponentType(string? componentName, out Type componentType);
    public IProperty? BuildProperty(ICalendarComponent component, CalendarObject calendarObject);
    public bool TryLookupProperty(string propertyName, out PropertyDefinition? property);
}
