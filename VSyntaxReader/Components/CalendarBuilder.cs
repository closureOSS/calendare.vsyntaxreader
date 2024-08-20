using System;
using System.Collections.Generic;
using System.Linq;
using Calendare.VSyntaxReader.Parsers;
using Calendare.VSyntaxReader.Properties;

namespace Calendare.VSyntaxReader.Components;

public class CalendarBuilder : ICalendarBuilder
{
    public readonly Dictionary<string, PropertyDefinition> Registry = [];
    public readonly Dictionary<string, Type> ComponentRegistry = [];
    private readonly TimezoneResolverFn? TimezoneResolverFn = null;

    public CalendarBuilder(TimezoneResolverFn? timezoneResolverFn = null)
    {
        TimezoneResolverFn = timezoneResolverFn;
        LoadStockComponents();
        LoadStockProperties();
    }

    public ICalendarParser Parser => new CalendarParser(this);

    public VCalendar CreateCalendar()
    {
        var vcalendar = new VCalendar { Builder = this };
        if (vcalendar.CreateProperty<TextProperty>(PropertyName.ProductIdentifier) is not TextProperty prodId ||
            vcalendar.CreateProperty<StaticProperty>(PropertyName.Version) is null ||
            vcalendar.CreateProperty<StaticProperty>(PropertyName.CalendarScale) is null
            )
        {
            throw new NullReferenceException(nameof(CreateCalendar));
        }
        prodId.Value = "-//closure.ch//NONSGML Calendare//EN";
        return vcalendar;
    }

    public bool TryLookupComponentType(string? componentName, out Type componentType)
    {
        componentType = typeof(UnknownComponent);
        if (!string.IsNullOrEmpty(componentName))
        {
            if (ComponentRegistry.TryGetValue(componentName.ToUpperInvariant(), out var ct) && ct is not null)
            {
                componentType = ct;
                return true;
            }
        }
        return false;
    }

    public IProperty? BuildProperty(ICalendarComponent component, CalendarObject calendarObject)
    {
        string propertyName = calendarObject.Name.ToUpperInvariant();
        switch (propertyName)
        {
            case PropertyName.Begin: return new BeginProperty(calendarObject);
            case PropertyName.End: return new EndProperty(calendarObject);
        }
        if (TryLookupProperty(propertyName, out var hit))
        {
            if (hit is not null)
            {
                var property = hit?.CreateFn(calendarObject);
                if (property is not null && (property.Cardinality == Cardinality.ZeroOrOne || property.Cardinality == Cardinality.One))
                {
                    var existing = component.Properties.FirstOrDefault(x => x.Name.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (existing is not null)
                    {
                        return null;
                    }
                }
                return property;
            }
        }
        else
        {
            if (propertyName.StartsWith("X-"))
            {
                return new XProperty(calendarObject);
            }
            return new OtherProperty(calendarObject);
        }
        return null;
    }

    public bool TryLookupProperty(string? propertyName, out PropertyDefinition? property)
    {
        property = null;
        if (string.IsNullOrEmpty(propertyName)) return false;
        propertyName = propertyName.ToUpperInvariant();
        switch (propertyName)
        {
            case PropertyName.Begin:
            case PropertyName.End:
                return true;    // protected properties are automatically added to components and should not be added externally
        }
        if (Registry.TryGetValue(propertyName, out var hit))
        {
            property = hit;
            return true;
        }
        return false;
    }

    private void LoadStockProperties()
    {
        Registry[PropertyName.Status] = new(static (obj) => new TextProperty(obj, Cardinality.ZeroOrOne), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.1.11");
        Registry[PropertyName.StatusX] = new(static (obj) => new TextProperty(obj, Cardinality.ZeroOrOne));
        Registry[PropertyName.DateStart] = new((obj) => new DateTimeProperty(obj, cardinality: Cardinality.ZeroOrOne, timezoneResolverFn: TimezoneResolverFn), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.2.4");
        Registry[PropertyName.Created] = new((obj) => new DateTimeProperty(obj, cardinality: Cardinality.ZeroOrOne, timezoneResolverFn: TimezoneResolverFn), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.7.1");
        Registry[PropertyName.LastModified] = new((obj) => new DateTimeProperty(obj, cardinality: Cardinality.ZeroOrOne, timezoneResolverFn: TimezoneResolverFn), "https://datatracker.ietf.org/doc/html/rfc7986#section-5.4");
        Registry[PropertyName.DateStamp] = new((obj) => new DateTimeProperty(obj, cardinality: Cardinality.ZeroOrOne, timezoneResolverFn: TimezoneResolverFn), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.7.2");
        Registry[PropertyName.DateEnd] = new((obj) => new DateTimeProperty(obj, PropertyName.DateStart, Cardinality.ZeroOrOne, timezoneResolverFn: TimezoneResolverFn), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.2.2");
        Registry[PropertyName.Due] = new((obj) => new DateTimeProperty(obj, PropertyName.DateStart, Cardinality.ZeroOrOne, timezoneResolverFn: TimezoneResolverFn), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.2.3");
        Registry[PropertyName.Duration] = new((obj) => new DurationProperty(obj, Cardinality.ZeroOrOne), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.2.5");
        Registry[PropertyName.Completed] = new((obj) => new DateTimeProperty(obj, cardinality: Cardinality.ZeroOrOne, timezoneResolverFn: TimezoneResolverFn), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.2.1");
        Registry[PropertyName.TimeTransparency] = new(static (obj) => new TextProperty(obj, Cardinality.ZeroOrOne), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.2.7");

        Registry[PropertyName.Organizer] = new(static (obj) => new OrganizerProperty(obj), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.4.3");
        Registry[PropertyName.Color] = new(static (obj) => new OtherProperty(obj), "https://datatracker.ietf.org/doc/html/rfc7986#section-5.9");
        Registry[PropertyName.Image] = new(static (obj) => new OtherProperty(obj), "https://datatracker.ietf.org/doc/html/rfc7986#section-5.10");
        Registry[PropertyName.Conference] = new(static (obj) => new OtherProperty(obj), "https://datatracker.ietf.org/doc/html/rfc7986#section-5.11");
        Registry[PropertyName.Attendee] = new(static (obj) => new AttendeeProperty(obj), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.4.1");

        Registry[PropertyName.ProductIdentifier] = new(static (obj) => new TextProperty(obj, Cardinality.One), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.7.3");
        Registry[PropertyName.Version] = new(static (obj) => new StaticProperty(obj, "2.0"), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.7.4");
        Registry[PropertyName.CalendarScale] = new(static (obj) => new StaticProperty(obj, "GREGORIAN"), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.7.1");
        Registry[PropertyName.CalendarName] = new(static (obj) => new TextProperty(obj, Cardinality.ZeroOrOne), "");
        Registry[PropertyName.CalendarDescription] = new(static (obj) => new TextProperty(obj, Cardinality.ZeroOrOne), "");
        Registry[PropertyName.Method] = new(static (obj) => new TextProperty(obj, Cardinality.One), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.7.2");
        Registry[PropertyName.Uid] = new(static (obj) => new TextProperty(obj, Cardinality.ZeroOrOne), "https://datatracker.ietf.org/doc/html/rfc7986#section-5.3");
        Registry[PropertyName.Name] = new(static (obj) => new TextMultilanguageProperty(obj), "https://datatracker.ietf.org/doc/html/rfc7986#section-5.1");
        Registry[PropertyName.Description] = new(static (obj) => new TextMultilanguageProperty(obj), "https://datatracker.ietf.org/doc/html/rfc7986#section-5.2");
        Registry[PropertyName.Url] = new(static (obj) => new OtherProperty(obj), "https://datatracker.ietf.org/doc/html/rfc7986#section-5.5");
        Registry[PropertyName.RefreshInterval] = new(static (obj) => new DurationProperty(obj, Cardinality.ZeroOrOne), "https://datatracker.ietf.org/doc/html/rfc7986#section-5.7");
        Registry[PropertyName.Source] = new(static (obj) => new OtherProperty(obj, Cardinality.ZeroOrOne), "https://datatracker.ietf.org/doc/html/rfc7986#section-5.8");
        Registry[PropertyName.Categories] = new(static (obj) => new OtherProperty(obj), "https://datatracker.ietf.org/doc/html/rfc7986#section-5.6");

        Registry[PropertyName.RecurrenceExceptionDate] = new(static (obj) => new DateTimeArrayProperty(obj, PropertyName.DateStart), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.5.1");
        Registry[PropertyName.RecurrenceDate] = new(static (obj) => new RecurrenceDateProperty(obj), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.5.2");
        Registry[PropertyName.RecurrenceRule] = new(static (obj) => new RecurrenceRuleProperty(obj), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.5.3");
        Registry[PropertyName.RecurrenceId] = new((obj) => new DateTimeProperty(obj, cardinality: Cardinality.ZeroOrOne, timezoneResolverFn: TimezoneResolverFn), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.4.4");
        Registry[PropertyName.Sequence] = new(static (obj) => new IntegerProperty(obj, Cardinality.ZeroOrOne, 0, 0), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.7.4");


        Registry[PropertyName.Comment] = new(static (obj) => new TextMultilanguageProperty(obj), "");
        Registry[PropertyName.MaskUid] = new(static (obj) => new TextProperty(obj, Cardinality.ZeroOrOne), "https://github.com/apple/ccs-calendarserver/blob/master/doc/Extensions/icalendar-maskuids.txt");
        Registry[PropertyName.FreeBusy] = new(static (obj) => new FreeBusyProperty(obj), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.2.6");
        Registry[PropertyName.RequestStatus] = new(static (obj) => new RequestStatusProperty(obj), "");
        Registry[PropertyName.Summary] = new(static (obj) => new TextMultilanguageProperty(obj), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.1.12");


        Registry[PropertyName.Percent] = new(static (obj) => new IntegerProperty(obj, Cardinality.ZeroOrOne, 0, 0, 100), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.1.8");
        Registry[PropertyName.Priority] = new(static (obj) => new IntegerProperty(obj, Cardinality.ZeroOrOne, 0, 0, 10), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.1.9");
        Registry[PropertyName.TzId] = new(static (obj) => new TextProperty(obj, Cardinality.One), "https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.3.1");
    }

    private void LoadStockComponents()
    {
        ComponentRegistry[ComponentName.VCalendar] = typeof(VCalendar);
        ComponentRegistry[ComponentName.VEvent] = typeof(VEvent);
        ComponentRegistry[ComponentName.VTodo] = typeof(VTodo);
        ComponentRegistry[ComponentName.VJournal] = typeof(VJournal);
        ComponentRegistry[ComponentName.VAlarm] = typeof(VAlarm);
        ComponentRegistry[ComponentName.VAvailability] = typeof(VAvailability);
        ComponentRegistry[ComponentName.VAvailabilityAvailable] = typeof(VAvailabilityAvailable);
        ComponentRegistry[ComponentName.VTimezone] = typeof(VTimezone);
        ComponentRegistry[ComponentName.VTimezoneStandard] = typeof(VTimezoneStandard);
        ComponentRegistry[ComponentName.VTimezoneDaylight] = typeof(VTimezoneDaylight);
        ComponentRegistry[ComponentName.VFreebusy] = typeof(VFreebusy);
        ComponentRegistry[ComponentName.VParticipant] = typeof(VParticipant);
        ComponentRegistry[ComponentName.VPoll] = typeof(VPoll);
        ComponentRegistry[ComponentName.VPollVote] = typeof(VPollVote);
        ComponentRegistry[ComponentName.VLocation] = typeof(VLocation);
        ComponentRegistry[ComponentName.VResource] = typeof(VResource);
        ComponentRegistry[ComponentName.UnknownComponent] = typeof(UnknownComponent);
    }
}
