using System;

namespace Calendare.VSyntaxReader.Components;

public class VPoll : CalendarComponent
{
    public override string Name => ComponentName.VPoll;

    public VPoll() { }

    public VPoll(ICalendarComponent? parent) : base(parent)
    {
    }

    public override ICalendarComponent? CreateChild(Type type)
    {
        if (type == typeof(VParticipant)) return new VParticipant(this);
        if (type == typeof(VEvent)) return new VEvent(this);
        return new UnknownComponent(this);
    }
}
