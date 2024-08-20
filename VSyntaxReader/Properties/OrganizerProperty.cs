
using Calendare.VSyntaxReader.Components;

namespace Calendare.VSyntaxReader.Properties;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.4.3
/// </summary>
public class OrganizerProperty : IProperty, ICalAddressValue, IPropertyClone<OrganizerProperty>
{
    public const string CommonNameParam = "CN";
    public string? CommonName
    {
        get => this.ReadTextParameter(CommonNameParam);
        set => this.AmendParameter(CommonNameParam, value);
    }
    public const string DirectoryParam = "DIR";
    public string? Directory
    {
        get => this.ReadTextParameter(DirectoryParam);
        set => this.AmendParameter(DirectoryParam, value);
    }

    public const string SentByParam = "SENT-BY";
    public string? SentBy
    {
        get => this.ReadTextParameter(SentByParam);
        set => this.AmendParameter(SentByParam, value);
    }

    public const string LanguageParam = "LANGUAGE"; // refers to 'CN'
    public string? Language
    {
        get => this.ReadTextParameter(LanguageParam);
        set => this.AmendParameter(LanguageParam, value);
    }

    // https://datatracker.ietf.org/doc/html/rfc6638#section-7.1
    public ScheduleAgentParameter ScheduleAgent { get; init; }

    // https://datatracker.ietf.org/doc/html/rfc6638#section-7.2
    public const string ScheduleForceSendParam = "SCHEDULE-FORCE-SEND";
    public string? ScheduleForceSend
    {
        get => this.ReadTextParameter(ScheduleForceSendParam);
        set => this.AmendParameter(ScheduleForceSendParam, value);
    }

    // https://datatracker.ietf.org/doc/html/rfc6638#section-7.3
    public const string ScheduleStatusParam = "SCHEDULE-STATUS";
    public string? ScheduleStatus
    {
        get => this.ReadTextParameter(ScheduleStatusParam);
        set => this.AmendParameter(ScheduleStatusParam, value);
    }

    public string Name { get; init; }
    public Cardinality Cardinality { get; set; } = Cardinality.ZeroOrOne;
    public CalendarObject Raw { get; private set; }
    public bool IsValid => !string.IsNullOrEmpty(Raw.Value);
    public ValueDataTypes DataType => ValueDataTypes.CalAddress;
    public string Value
    {
        get => this.GetEmail();
        set => Raw = this.AmendEmail(value);
    }

    public OrganizerProperty(CalendarObject calendarObject)
    {
        Raw = calendarObject;
        Name = calendarObject.Name;
        ScheduleAgent = new(this);
    }

    public DeserializeResult Deserialize(ICalendarComponent calendarComponent)
    {
        return new DeserializeResult(IsValid);
    }

    public IProperty DeepClone() => Copy();

    public OrganizerProperty Copy()
    {
        var target = new OrganizerProperty(Raw.CreateCopy())
        {
            Name = Name,
        };
        return target;
    }
}
