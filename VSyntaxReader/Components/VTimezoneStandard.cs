namespace Calendare.VSyntaxReader.Components;

public class VTimezoneStandard : CalendarComponent
{
    public override string Name => ComponentName.VTimezoneStandard;

    public VTimezoneStandard() { }

    public VTimezoneStandard(ICalendarComponent? parent) : base(parent)
    {
    }
}
