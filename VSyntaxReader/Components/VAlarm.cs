namespace Calendare.VSyntaxReader.Components;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.6.6
/// </summary>
public class VAlarm : CalendarComponent
{
    public override string Name => ComponentName.VAlarm;

    public VAlarm() { }

    public VAlarm(ICalendarComponent? parent) : base(parent)
    {
    }
}
