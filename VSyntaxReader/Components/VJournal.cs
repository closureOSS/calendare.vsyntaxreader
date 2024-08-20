using System;

namespace Calendare.VSyntaxReader.Components;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.6.3
/// </summary>
public class VJournal : RecurringComponent
{
    public override string Name => ComponentName.VJournal;

    public VJournal() { }

    public VJournal(ICalendarComponent? parent) : base(parent)
    {
    }

    public override ICalendarComponent? CreateChild(Type type)
    {
        if (type == typeof(VParticipant)) return new VParticipant(this);
        if (type == typeof(VLocation)) return new VLocation(this);
        if (type == typeof(VResource)) return new VResource(this);
        return new UnknownComponent(this);
    }
}
