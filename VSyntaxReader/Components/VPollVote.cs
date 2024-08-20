namespace Calendare.VSyntaxReader.Components;

public class VPollVote : CalendarComponent
{
    public override string Name => ComponentName.VPollVote;

    public VPollVote() { }

    public VPollVote(ICalendarComponent? parent) : base(parent)
    {
    }
}
