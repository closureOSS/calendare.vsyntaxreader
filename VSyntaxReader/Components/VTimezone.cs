using System;
using Calendare.VSyntaxReader.Properties;

namespace Calendare.VSyntaxReader.Components;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.6.5
/// </summary>
public class VTimezone : CalendarComponent
{
    public override string Name => ComponentName.VTimezone;

    public VTimezone() { }

    public VTimezone(ICalendarComponent? parent) : base(parent)
    {
    }

    public string TzId
    {
        get => this.ReadTextProperty(PropertyName.TzId) ?? string.Empty;
        set => this.AmendProperty(PropertyName.TzId, value ?? string.Empty);
    }

    public override ICalendarComponent? CreateChild(Type type)
    {
        if (type == typeof(VTimezoneDaylight)) return new VTimezoneDaylight(this);
        if (type == typeof(VTimezoneStandard)) return new VTimezoneStandard(this);
        return new UnknownComponent(this);
    }
}
