namespace Calendare.VSyntaxReader.Components;

public class VTimezoneDaylight : CalendarComponent
{
    public override string Name => ComponentName.VTimezoneDaylight;

    public VTimezoneDaylight() { }

    public VTimezoneDaylight(ICalendarComponent? parent) : base(parent)
    {
    }
}
