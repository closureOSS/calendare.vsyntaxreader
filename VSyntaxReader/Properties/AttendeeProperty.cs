
using System;
using Calendare.VSyntaxReader.Components;

namespace Calendare.VSyntaxReader.Properties;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.4.1
/// </summary>
public class AttendeeProperty : IProperty, ICalAddressValue, IPropertyClone<AttendeeProperty>
{
    public const string CommonNameParam = "CN";
    public string? CommonName
    {
        get => this.ReadTextParameter(CommonNameParam);
        set => this.AmendParameter(CommonNameParam, value);
    }

    public ParticipationRoleParameter Role { get; private set; }

    public ParticipationStatusParameter ParticipationStatus { get; private set; }

    public const string RsvpParam = "RSVP";
    public bool? Rsvp
    {
        get => this.ReadBooleanParameter(RsvpParam);
        set => this.AmendParameter(RsvpParam, value is null ? null : value.Value ? "TRUE" : "FALSE");
    }

    public const string CalendarUserTypeParam = "CUTYPE";
    public string? CalendarUserType
    {
        get => this.ReadTextParameter(CalendarUserTypeParam);
        set => this.AmendParameter(CalendarUserTypeParam, value);
    }

    public const string GroupMemberParam = "MEMBER";
    public string? GroupMember
    {
        get => this.ReadTextParameter(GroupMemberParam);
        set => this.AmendParameter(GroupMemberParam, value);
    }

    public const string SentByParam = "SENT-BY";
    public string? SentBy
    {
        get => this.ReadTextParameter(SentByParam);
        set => this.AmendParameter(SentByParam, value);
    }

    public const string DelegatedToParam = "DELEGATED-TO";
    public string? DelegatedTo
    {
        get => this.ReadTextParameter(DelegatedToParam);
        set => this.AmendParameter(DelegatedToParam, value);
    }

    public const string DelegatedFromParam = "DELEGATED-FROM";
    public string? DelegatedFrom
    {
        get => this.ReadTextParameter(DelegatedFromParam);
        set => this.AmendParameter(DelegatedFromParam, value);
    }

    public const string DirectoryParam = "DIR";
    public string? Directory
    {
        get => this.ReadTextParameter(DirectoryParam);
        set => this.AmendParameter(DirectoryParam, value);
    }

    public const string LanguageParam = "LANGUAGE"; // refers to 'CN'
    public string? Language
    {
        get => this.ReadTextParameter(LanguageParam);
        set => this.AmendParameter(LanguageParam, value);
    }

    // https://datatracker.ietf.org/doc/html/rfc6638#section-7.1
    public ScheduleAgentParameter ScheduleAgent { get; private set; }


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
    public Cardinality Cardinality { get; set; } = Cardinality.Many;
    public CalendarObject Raw { get; private set; }
    public bool IsValid => !string.IsNullOrEmpty(Raw.Value);
    public ValueDataTypes DataType => ValueDataTypes.CalAddress;
    public static Func<AttendeeProperty, bool> Match(string? email)
    {
        return string.IsNullOrEmpty(email)
            ? ((p) => false)
            : ((p) => p.Value.Equals(email, StringComparison.InvariantCultureIgnoreCase) == true);
    }

    public Func<IProperty, bool> Match()
    {
        return (p) =>
        {
            if (p is not AttendeeProperty tmp)
            {
                return false;
            }
            return Value.Equals(tmp?.Value, StringComparison.InvariantCultureIgnoreCase) == true;
        };
    }

    public string Value
    {
        get => this.GetEmail();
        set => Raw = this.AmendEmail(value);
    }


    public AttendeeProperty(CalendarObject calendarObject)
    {
        Raw = calendarObject;
        Name = calendarObject.Name;
        ParticipationStatus = new(this);
        Role = new(this);
        ScheduleAgent = new(this);
    }

    public DeserializeResult Deserialize(ICalendarComponent calendarComponent)
    {
        return new DeserializeResult(IsValid);
    }

    public IProperty DeepClone() => Copy();

    public AttendeeProperty Copy()
    {
        var target = new AttendeeProperty(Raw.CreateCopy())
        {
            Name = Name,
        };
        return target;
    }
}
