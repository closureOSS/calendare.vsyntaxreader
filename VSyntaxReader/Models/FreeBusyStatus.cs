namespace Calendare.VSyntaxReader.Models;

public enum FreeBusyStatus
{
    Free = 0,
    BusyTentative = 1,
    BusyUnavailable = 2,
    Busy = 3
}

public static class FreeBusyStatusExtension
{
    public static string Serialize(this FreeBusyStatus status)
    {
        return status switch
        {
            FreeBusyStatus.Free => "FREE",
            FreeBusyStatus.BusyTentative => "BUSY-TENTATIVE",
            FreeBusyStatus.BusyUnavailable => "BUSY-UNAVAILABLE",
            _ => "BUSY",
        };
    }

    public static FreeBusyStatus GetFreeBusyStatus(string? val)
    {
        return val?.ToUpperInvariant() switch
        {
            "FREE" => FreeBusyStatus.Free,
            "BUSY-TENTATIVE" => FreeBusyStatus.BusyTentative,
            "BUSY-UNAVAILABLE" => FreeBusyStatus.BusyUnavailable,
            "BUSY" => FreeBusyStatus.Busy,
            _ => FreeBusyStatus.Busy
        };
    }
}
