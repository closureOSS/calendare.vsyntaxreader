using System.Collections.Generic;

namespace Calendare.VSyntaxReader.Properties;

public enum ScheduleAgent
{
    Server,
    Client,
    None,
}

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6638#section-7.1
/// </summary>
public class ScheduleAgentParameter : PropertyEnumParameter<ScheduleAgent>
{
    public ScheduleAgentParameter(IProperty property)
       : base(property, ScheduleAgentValues.Name, ScheduleAgentValues.GetCodes(), ScheduleAgent.Server)
    {
    }
}

public static class ScheduleAgentValues
{
    public const string Name = "SCHEDULE-AGENT";
    private static readonly Dictionary<ScheduleAgent, string> Codes = [];
    public static Dictionary<ScheduleAgent, string> GetCodes() => Codes;
    static ScheduleAgentValues()
    {
        Codes[ScheduleAgent.Server] = "SERVER";
        Codes[ScheduleAgent.Client] = "CLIENT";
        Codes[ScheduleAgent.None] = "NONE";
    }
    public static string? FromToken(this ScheduleAgent? value)
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
