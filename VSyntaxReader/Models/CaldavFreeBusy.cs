using System.Collections.Generic;

namespace Calendare.VSyntaxReader.Models;

public class CaldavFreeBusy
{
    public FreeBusyStatus Status { get; set; } = FreeBusyStatus.Busy;
    public List<CaldavPeriodUtc> FreeBusyEntries { get; set; } = [];
}
