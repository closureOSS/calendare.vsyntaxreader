using NodaTime;

namespace Calendare.VSyntaxReader.Models;

public sealed class FreeBusyEntry
{
    public FreeBusyStatus Status { get; set; } = FreeBusyStatus.Busy;
    public Interval Period { get; set; }
    public int Priority { get; set; }
}
