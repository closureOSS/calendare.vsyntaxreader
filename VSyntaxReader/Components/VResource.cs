namespace Calendare.VSyntaxReader.Components;

/// <summary>
/// RFC9073
/// </summary>
public class VResource : CalendarComponent
{
    public override string Name => ComponentName.VResource;

    public VResource() { }

    public VResource(ICalendarComponent? parent) : base(parent)
    {
    }
}
