using System;
using Calendare.VSyntaxReader.Properties;

namespace Calendare.VSyntaxReader.Components;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.4
/// </summary>
public class VCalendar : CalendarComponent
{
    public override string Name => ComponentName.VCalendar;

    public VCalendar() : base()
    {
    }

    public string ProductIdentifier
    {
        get => this.ReadTextProperty(PropertyName.ProductIdentifier) ?? string.Empty;
        set => this.AmendProperty(PropertyName.ProductIdentifier, value ?? string.Empty);
    }

    public string? Method
    {
        get => this.ReadTextProperty(PropertyName.Method);
        set => this.AmendProperty(PropertyName.Method, value);
    }

    public string? CalendarName
    {
        get => this.ReadTextProperty(PropertyName.CalendarName);
        set => this.AmendProperty(PropertyName.CalendarName, value);
    }

    public string? CalendarDescription
    {
        get => this.ReadTextProperty(PropertyName.CalendarDescription);
        set => this.AmendProperty(PropertyName.CalendarDescription, value);
    }

    // public string? CalendarTimezone
    // {
    //     get => this.ReadTextProperty(PropertyName.CalendarTimezone);
    //     set => this.AmendProperty(PropertyName.CalendarTimezone, value);
    // }

    public override ICalendarComponent? CreateChild(Type type)
    {
        if (type == typeof(VTimezone)) return new VTimezone(this);
        if (type == typeof(VEvent)) return new VEvent(this);
        if (type == typeof(VTodo)) return new VTodo(this);
        if (type == typeof(VJournal)) return new VJournal(this);
        if (type == typeof(VFreebusy)) return new VFreebusy(this);
        if (type == typeof(VAvailability)) return new VAvailability(this);
        if (type == typeof(VAlarm)) return new VAlarm(this);
        if (type == typeof(VPoll)) return new VPoll(this);
        return new UnknownComponent(this);
    }

    public override ICalendarComponent CopyTo(ICalendarComponent? _ = null)
    {
        if (Builder is null) throw new NullReferenceException(nameof(ICalendarBuilder));
        var vcalendar = new VCalendar { Builder = Builder };
        return CopyComponentInto(vcalendar);
    }

    public VCalendar Copy()
    {
        return (CopyTo(null) as VCalendar)!;
    }
}
