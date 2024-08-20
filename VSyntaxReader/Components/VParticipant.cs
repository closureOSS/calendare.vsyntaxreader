using System;

namespace Calendare.VSyntaxReader.Components;
/// <summary>
/// RFC9073
/// </summary>
public class VParticipant : CalendarComponent
{
    public override string Name => ComponentName.VParticipant;

    public VParticipant() { }

    public VParticipant(ICalendarComponent? parent) : base(parent)
    {
    }

    public override ICalendarComponent? CreateChild(Type type)
    {
        if (type == typeof(VLocation)) return new VLocation(this);
        if (type == typeof(VResource)) return new VResource(this);
        if (type == typeof(VPollVote)) return new VPollVote(this);
        return new UnknownComponent(this);
    }
}
