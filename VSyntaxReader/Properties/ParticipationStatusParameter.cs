using System.Collections.Generic;

namespace Calendare.VSyntaxReader.Properties;

public enum EventParticipationStatus
{
    /// <summary> Event needs action </summary>
    NeedsAction,
    /// <summary> Event accepted </summary>
    Accepted,
    /// <summary> Event declined </summary>
    Declined,
    /// <summary> Event tentatively accepted </summary>
    Tentative,
    /// <summary> Event delegated </summary>
    Delegated,
    // public static string Default => NeedsAction;
}

public class ParticipationStatusParameter : PropertyEnumParameter<EventParticipationStatus>
{
    public ParticipationStatusParameter(IProperty property)
        : base(property, EventParticipationStatusValues.Name, EventParticipationStatusValues.GetCodes(), null)
    {
    }
}

public static class EventParticipationStatusValues
{
    public const string Name = "PARTSTAT";
    private static readonly Dictionary<EventParticipationStatus, string> Codes = [];
    public static Dictionary<EventParticipationStatus, string> GetCodes() => Codes;
    static EventParticipationStatusValues()
    {
        Codes[EventParticipationStatus.NeedsAction] = "NEEDS-ACTION";
        Codes[EventParticipationStatus.Accepted] = "ACCEPTED";
        Codes[EventParticipationStatus.Declined] = "DECLINED";
        Codes[EventParticipationStatus.Tentative] = "TENTATIVE";
        Codes[EventParticipationStatus.Delegated] = "DELEGATED";
    }
    public static string? FromToken(this EventParticipationStatus? value)
    {
        if (value is not null)
        {
            if (Codes.TryGetValue(value.Value, out string? code))
            {
                return code;
            }
        }
        return null;
    }
}
