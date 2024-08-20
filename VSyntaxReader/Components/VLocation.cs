namespace Calendare.VSyntaxReader.Components;

/// <summary>
/// RFC9073
/// </summary>
public class VLocation : CalendarComponent
{
    public override string Name => ComponentName.VLocation;

    public VLocation() { }

    public VLocation(ICalendarComponent? parent) : base(parent)
    {
    }
}
