namespace Calendare.VSyntaxReader.Components;

public class UnknownComponent : CalendarComponent
{
    public override string Name => ComponentName.UnknownComponent;

    public UnknownComponent() { }

    public UnknownComponent(ICalendarComponent? parent) : base(parent)
    {
    }

}
