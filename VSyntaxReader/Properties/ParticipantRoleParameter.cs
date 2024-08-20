using System.Collections.Generic;

namespace Calendare.VSyntaxReader.Properties;

public enum EventParticipationRole
{
    /// <summary> Indicates a participant whose participation is required </summary>
    Required,
    /// <summary> Indicates a participant whose participation is optional </summary>
    Optional,
    /// <summary> Indicates the chair of the calendar entity </summary>
    Chair,
    /// <summary> Indicates a participant who is copied for information purposes only </summary>
    Informative,
    /// <summary> Event delegated </summary>
}

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.2.16
/// </summary>
public class ParticipationRoleParameter : PropertyEnumParameter<EventParticipationRole>
{
    public ParticipationRoleParameter(IProperty property)
        : base(property, ParticipationRoleValues.Name, ParticipationRoleValues.GetCodes(), EventParticipationRole.Required)
    {
    }
}

public static class ParticipationRoleValues
{
    public const string Name = "ROLE";
    private static readonly Dictionary<EventParticipationRole, string> Codes = [];
    public static Dictionary<EventParticipationRole, string> GetCodes() => Codes;
    static ParticipationRoleValues()
    {
        Codes[EventParticipationRole.Chair] = "CHAIR";
        Codes[EventParticipationRole.Required] = "REQ-PARTICIPANT";
        Codes[EventParticipationRole.Optional] = "OPT-PARTICIPANT";
        Codes[EventParticipationRole.Informative] = "NON-PARTICIPANT";
    }
    public static string? FromToken(this EventParticipationRole? value)
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
