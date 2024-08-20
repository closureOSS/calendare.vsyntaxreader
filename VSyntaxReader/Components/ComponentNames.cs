namespace Calendare.VSyntaxReader.Components;

public static class ComponentName
{
    public const string VCalendar = "VCALENDAR";
    public const string VEvent = "VEVENT";
    public const string VTodo = "VTODO";
    public const string VJournal = "VJOURNAL";
    public const string VAlarm = "VALARM";
    public const string VAvailability = "VAVAILABILITY";
    public const string VAvailabilityAvailable = "AVAILABLE";
    public const string VTimezone = "VTIMEZONE";
    public const string VTimezoneStandard = "STANDARD";
    public const string VTimezoneDaylight = "DAYLIGHT";
    public const string VFreebusy = "VFREEBUSY";

    // https://datatracker.ietf.org/doc/draft-ietf-calext-vpoll/
    public const string VPoll = "VPOLL";
    public const string VParticipant = "PARTICIPANT";
    public const string VPollVote = "VOTE";
    public const string VLocation = "VLOCATION";
    public const string VResource = "VRESOURCE";

    public const string UnknownComponent = "UNKNOWN";
}
