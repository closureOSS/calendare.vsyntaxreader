using System;
using System.Collections.Generic;
using System.Linq;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Operations;
using Calendare.VSyntaxReader.Properties;
using NodaTime;
using Xunit.Abstractions;

namespace VSyntaxReader.Tests.PropertiesGeneric;

public class PropertiesGenericTest
{
    private readonly ITestOutputHelper Output;
    private readonly CalendarBuilder Builder = new();

    public PropertiesGenericTest(ITestOutputHelper output)
    {
        Output = output;
    }

    [Fact]
    public void CloningProperties()
    {
        var bodyContent = @"
BEGIN:VCALENDAR
PRODID:-//Mozilla.org/NONSGML Mozilla Calendar V1.1//EN
VERSION:2.0
METHOD:REQUEST
BEGIN:VFREEBUSY
DTSTAMP:20081022T062945Z
ORGANIZER:mailto:user1@example.com
DTSTART:20060930T120000Z
DTEND:20070630T115959Z
X-CLOSURE-DUMMY:Hello World
UID:c5bd82ea-cd89-4f58-8d31-336f47e44f97
ATTENDEE;PARTSTAT=NEEDS-ACTION;ROLE=REQ-PARTICIPANT;CUTYPE=INDIVIDUAL:
 mailto:user1@example.com
END:VFREEBUSY
END:VCALENDAR
";
        var parseResult = Builder.Parser.TryParse(bodyContent, out var vcal);
        Assert.True(parseResult, $"Parsing request failed {parseResult.ErrorMessage}");
        Assert.NotNull(vcal);
        Assert.Equal([PropertyName.ProductIdentifier, PropertyName.Version, PropertyName.Method], vcal.Properties.Select(x => x.Name));
        var freeBusyRequest = vcal.Children.OfType<VFreebusy>().FirstOrDefault();
        Assert.NotNull(freeBusyRequest);

        // Attendee
        var attendee = freeBusyRequest.Attendees.Value.FirstOrDefault();
        Assert.NotNull(attendee);
        var cloneattendee = attendee.DeepClone() as AttendeeProperty;
        Assert.NotNull(cloneattendee);
        cloneattendee.ParticipationStatus.Value = EventParticipationStatus.Delegated;
        cloneattendee.Role.Value = null;
        cloneattendee.Value = new("mailto:anotheruser@example.com");
        freeBusyRequest.Properties.Add(cloneattendee);
        Assert.Equal("NEEDS-ACTION", attendee.ParticipationStatus.Value.FromToken());
        Assert.Equal("DELEGATED", cloneattendee.ParticipationStatus.Value.FromToken());
        Assert.Equal("REQ-PARTICIPANT", cloneattendee.Role.Value.FromToken());
        // Assert.Null(cloneattendee.Role.Value);

        // DateTime
        var startdate = freeBusyRequest.DateStart;
        var startDateProperty = freeBusyRequest.FindOneProperty<DateTimeProperty>(PropertyName.DateStart);
        Assert.NotNull(startDateProperty);
        var clonedDateProperty = startDateProperty.DeepClone() as DateTimeProperty;
        Assert.NotNull(clonedDateProperty);
        var cloneStartdate = clonedDateProperty?.Value?.ToInstant() ?? Instant.MinValue;
        Assert.Equal(startdate, cloneStartdate);
        freeBusyRequest.DateStart = freeBusyRequest.DateStart.Plus(Duration.FromDays(1000));

        var otherXProperty = freeBusyRequest.CreateProperty<XProperty>("X-CLOSURE-OTHERDUMMY");
        Assert.NotNull(otherXProperty);
        otherXProperty.Value = "Hello Globe";
        var otherXPropertyClone = otherXProperty.DeepClone() as XProperty;
        Assert.NotNull(otherXPropertyClone);
        otherXPropertyClone.Value = startDateProperty.Raw.Value;
        freeBusyRequest.Properties.Add(otherXPropertyClone);

        var result = vcal.Serialize();
        Assert.NotNull(result);
        Output.WriteLine(result);
    }

    [Fact]
    public void CreateComponents()
    {
        var vcalendar = Builder.CreateCalendar();
        Assert.NotNull(vcalendar);
        var vevent = vcalendar.CreateChild<VEvent>();
        Assert.NotNull(vevent);
        Assert.IsType<VEvent>(vevent);
        var vunknown = vcalendar.CreateChild<UnknownComponent>();
        Assert.NotNull(vunknown);
    }

    [Fact]
    public void CopyCalendar()
    {
        var sourceFilename = FileExtensions.BuildSourceFilename("base.ics");
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        var clonedCalendar = vcalendar.CopyTo<VCalendar>();
        Assert.NotNull(clonedCalendar);
        AssertVerificationFile(clonedCalendar, "base.clone.result.ics");
    }

    [Fact]
    public void CreateNewOccurrence()
    {
        var sourceFilename = FileExtensions.BuildSourceFilename("base.ics");
        var sop = new ZonedDateTime(new LocalDateTime(2024, 08, 01, 0, 0), DateTimeZone.Utc, Offset.Zero);
        var eop = new ZonedDateTime(new LocalDateTime(2024, 08, 31, 23, 59), DateTimeZone.Utc, Offset.Zero);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);

        var sourceEvent = vcalendar.Children.OfType<VEvent>().FirstOrDefault(ev => ev.RecurrenceId is null);
        Assert.NotNull(sourceEvent);
        Assert.NotNull(sourceEvent.DateStart);

        var oc2 = sourceEvent.CopyTo<VEvent>();
        Assert.NotNull(oc2);
        oc2.RemoveProperties([PropertyName.RecurrenceRule, PropertyName.RecurrenceDate, PropertyName.RecurrenceExceptionDate, PropertyName.RecurrenceExceptionRule]);
        Assert.Null(oc2.RecurrenceId);

        var occurrences = vcalendar.GetOccurrences(new Interval(sop.ToInstant(), eop.ToInstant()));
        Assert.NotEmpty(occurrences);
        var oc2Id = occurrences[15];

        oc2.DateStart = new CaldavDateTime(oc2Id.Interval.Start.InZone(sourceEvent.DateStart.Zone ?? DateTimeZone.Utc).PlusMinutes(-3));
        oc2.RecurrenceId = new CaldavDateTime(oc2Id.Interval.Start.InUtc());
        Assert.NotNull(oc2.RecurrenceId);

        AssertVerificationFile(vcalendar, "base.oc2.result.ics");
    }

    [Fact]
    public void TextMultilanguagePropertyTest()
    {
        var bodyContent = @"
BEGIN:VCALENDAR
PRODID:-//Mozilla.org/NONSGML Mozilla Calendar V1.1//EN
VERSION:2.0
METHOD:REQUEST
BEGIN:VEVENT
DTSTAMP:20081022T062945Z
DTSTART:20060930T120000Z
DTEND:20070630T115959Z
UID:c5bd82ea-cd89-4f58-8d31-336f47e44f97
SUMMARY:This is a simple summary in the default language
SUMMARY;LANGUAGE=de-DE:Das ist eine einfache Zusammenfassung in der deutschen Sprache
END:VEVENT
END:VCALENDAR
";
        var parseResult = Builder.Parser.TryParse(bodyContent, out var vCalendar);
        Assert.True(parseResult, $"Parsing request failed {parseResult.ErrorMessage}");
        Assert.NotNull(vCalendar);
        Assert.Equal([PropertyName.ProductIdentifier, PropertyName.Version, PropertyName.Method], vCalendar.Properties.Select(x => x.Name));
        var vevent = vCalendar.Children.OfType<VEvent>().FirstOrDefault();
        Assert.NotNull(vevent);

        Assert.Equal("This is a simple summary in the default language", vevent.Summary.Text());
        Assert.Equal("Das ist eine einfache Zusammenfassung in der deutschen Sprache", vevent.Summary.Text("de-DE"));
        Assert.Null(vevent.Summary.Get("fr-FR"));

        vevent.Summary.Set("Ceci est un simple résumé en langue française", "fr-FR");
        Assert.Equal("Ceci est un simple résumé en langue française", vevent.Summary.Text("fr-FR"));
        AssertVerificationFile(vCalendar, "textmultilanguage.1.result.ics");

        vevent.Summary.Set("", "de-DE");
        Assert.Null(vevent.Summary.Get("de-DE"));
        Assert.Equal("This is a simple summary in the default language", vevent.Summary.Text());

        vevent.Summary.Set("Busy");
        AssertVerificationFile(vCalendar, "textmultilanguage.2.result.ics");
    }

    [Fact]
    public void PropertyParameterEscapingTest()
    {
        var bodyContent = @"
BEGIN:VCALENDAR
PRODID:-//Mozilla.org/NONSGML Mozilla Calendar V1.1//EN
VERSION:2.0
METHOD:REQUEST
BEGIN:VEVENT
DTSTAMP:20081022T062945Z
DTSTART:20060930T120000Z
DTEND:20070630T115959Z
UID:c5bd82ea-cd89-4f58-8d31-336f47e44f97
ATTENDEE;PARTSTAT=NEEDS-ACTION;ROLE=REQ-PARTICIPANT;DELEGATED-FROM=""mailto:user3@example.com"";CUTYPE=INDIVIDUAL:
 mailto:user1@example.com
GEO;X-ADDRESS=""Pittsburgh Pirates^n115 Federal St^nPitt
 sburgh, PA 15212"":geo:40.446816,-80.00566
END:VEVENT
END:VCALENDAR
";
        var parseResult = Builder.Parser.TryParse(bodyContent, out var vCalendar);
        Assert.True(parseResult, $"Parsing request failed {parseResult.ErrorMessage}");
        Assert.NotNull(vCalendar);
        Assert.Equal([PropertyName.ProductIdentifier, PropertyName.Version, PropertyName.Method], vCalendar.Properties.Select(x => x.Name));
        var vevent = vCalendar.Children.OfType<VEvent>().FirstOrDefault();
        Assert.NotNull(vevent);

        var attendees = vevent.FindAllProperties<AttendeeProperty>(PropertyName.Attendee);
        Assert.NotNull(attendees);
        Assert.NotEmpty(attendees);
        var attendee = attendees.First();
        attendee.CommonName = "George Herman \"Babe\" Ruth";
        attendee.SentBy = "mailto:babe@example.com";

        AssertVerificationFile(vCalendar, "propertyescaping.1.result.ics");
    }


    [Fact]
    public void PropertyLabelEscapingTest()
    {
        var sourceFilename = FileExtensions.BuildSourceFilename("base.ics");
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);

        var sourceEvent = vcalendar.Children.OfType<VEvent>().FirstOrDefault(ev => ev.RecurrenceId is null);
        Assert.NotNull(sourceEvent);
        Assert.NotNull(sourceEvent.Description);
    }


    [Fact]
    public void TextValueEscapingTest()
    {
        var sourceFilename = FileExtensions.BuildSourceFilename("base.ics");
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);

        var sourceEvent = vcalendar.Children.OfType<VEvent>().FirstOrDefault(ev => ev.RecurrenceId is null);
        Assert.NotNull(sourceEvent);
        Assert.NotEmpty(sourceEvent.Description.Value);
        sourceEvent.Summary.Set("Some free text, with commas and secicolons; added with no reasonable sense at all!");
        sourceEvent.Description.Set(sourceEvent.Description.Text());
        AssertVerificationFile(vcalendar, "base.escaping.result.ics");
    }

    [Theory]
    [InlineData("expand.ics", "expand.result.ics")]
    [InlineData("expand-emptyduration.ics", "expand-emptyduration.result.ics")]
    public void ExpandOccurrenceForPeriodTest(string source, string verification)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var sop = new ZonedDateTime(new LocalDateTime(2024, 08, 05, 10, 0), DateTimeZone.Utc, Offset.Zero);
        var eop = new ZonedDateTime(new LocalDateTime(2024, 08, 06, 0, 0), DateTimeZone.Utc, Offset.Zero);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);

        var combinedCalendar = ExpandOccurrences(vcalendar, new Interval(sop.ToInstant(), eop.ToInstant()));
        Assert.NotNull(vcalendar);

        AssertVerificationFile(combinedCalendar, verification);
    }

    [Theory]
    [InlineData("expand-tz.ics", "expand-tz.result.ics")]
    public void ExpandOccurrenceAdvancedTest(string source, string verification)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var sop = new ZonedDateTime(new LocalDateTime(2024, 08, 18, 0, 0), DateTimeZone.Utc, Offset.Zero);
        var eop = new ZonedDateTime(new LocalDateTime(2025, 12, 18, 0, 0), DateTimeZone.Utc, Offset.Zero);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);

        var combinedCalendar = ExpandOccurrences(vcalendar, new Interval(sop.ToInstant(), eop.ToInstant()));
        Assert.NotNull(vcalendar);

        AssertVerificationFile(combinedCalendar, verification);
    }



    [Theory]
    [InlineData("expand-dayonly.ics", "expand-dayonly.result.ics")]
    [InlineData("expand-holiday.ics", "expand-holiday.result.ics")]
    public void ExpandOccurrenceDayOnlyForPeriodTest(string source, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var sop = new ZonedDateTime(new LocalDateTime(2024, 01, 01, 0, 0), DateTimeZone.Utc, Offset.Zero);
        var eop = new ZonedDateTime(new LocalDateTime(2025, 12, 31, 0, 0), DateTimeZone.Utc, Offset.Zero);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);

        var combinedCalendar = ExpandOccurrences(vcalendar, new Interval(sop.ToInstant(), eop.ToInstant()));
        Assert.NotNull(vcalendar);

        AssertVerificationFile(combinedCalendar, verificationFilename);
    }

    private VCalendar ExpandOccurrences(VCalendar vcalendar, Interval range)
    {
        var occurrences = vcalendar.GetOccurrences(range);
        Assert.NotEmpty(occurrences);

        var combinedCalendar = Builder.CreateCalendar();
        foreach (var occurrenceItem in occurrences)
        {
            if (occurrenceItem.IsReccurring)
            {
                var occurrence = occurrenceItem.Source.CopyTo<RecurringComponent>(combinedCalendar);
                if (occurrence.RecurrenceId is null)
                {
                    occurrence.RemoveProperties([PropertyName.RecurrenceRule, PropertyName.RecurrenceDate, PropertyName.RecurrenceExceptionDate, PropertyName.RecurrenceExceptionRule]);
                    if (occurrenceItem.Source.DateStart?.IsDateOnly == true)
                    {
                        occurrence.RecurrenceId = new CaldavDateTime(occurrenceItem.Interval.Start.InZone(occurrenceItem.Source.DateStart?.Zone ?? DateTimeZone.Utc).LocalDateTime.Date);
                    }
                    else
                    {
                        occurrence.RecurrenceId = new CaldavDateTime(occurrenceItem.Interval.Start.InUtc());
                    }
                    // occurrence.DateStart = new CaldavDateTime(occ.Interval.Start.InUtc());
                    occurrence.DateStart = new CaldavDateTime(occurrenceItem.Interval.Start.InZone(occurrenceItem.Source.DateStart?.Zone ?? DateTimeZone.Utc), occurrenceItem.Source.DateStart?.IsDateOnly ?? false);
                    if (occurrence is VEvent vEvent)
                    {
                        if (vEvent.DateEnd is not null)
                        {
                            // vEvent.DateEnd = new CaldavDateTime(occ.Interval.End.InUtc());
                            vEvent.DateEnd = new CaldavDateTime(occurrenceItem.Interval.End.InZone(vEvent.DateEnd.Zone ?? DateTimeZone.Utc), occurrenceItem.Source.DateStart?.IsDateOnly ?? false);
                        }
                        else if (vEvent.Duration is not null)
                        {
                            // TODO: Handle duration changed in RDATE ...
                            var durationInSeconds = Period.FromSeconds(Convert.ToInt64(occurrenceItem.Interval.Duration.TotalSeconds));
                            var durationNormalized = durationInSeconds.Normalize();
                            if (durationNormalized != vEvent.Duration)
                            {
                                vEvent.Duration = durationNormalized;
                            }
                        }
                    }
                }
            }
        }
        return combinedCalendar;
    }

    [Theory]
    [InlineData("range.ics", "range.result.ics")]
    [InlineData("range-count.ics", "range-count.result.ics")]
    [InlineData("range-until.ics", "range-until.result.ics")]
    [InlineData("range-holiday.ics", "range-holiday.result.ics")]
    public void OccurenceRangeTest(string source, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);

        var range = vcalendar.Children.GetOccurrencesRange();
        // Output.WriteLine($"{source} -> RANGE {range}");
        AssertVerificationFile($"{source} -> RANGE {range}", verificationFilename);
    }


    [Theory]
    [InlineData("interval-vtodo-1.ics", "interval-vtodo-1.result.ics")]
    public void IntervalTest(string source, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);

        var rc = vcalendar.Children.FirstOrDefault() as RecurringComponent;
        Assert.NotNull(rc);
        var interval = rc.GetInterval(DateTimeZone.Utc);
        // Output.WriteLine($"{source} -> INTERVAL {interval}");
        AssertVerificationFile($"{source} -> INTERVAL {interval}", verificationFilename);
    }

    [Theory]
    [InlineData("attendee.ics", "attendee.reply.ics")]
    public void CreateSchedulingReplyTest(string source, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        Assert.NotNull(vcalendar.Builder);
        var replyCalendar = vcalendar.Builder.CreateCalendar();
        Assert.NotNull(replyCalendar);
        replyCalendar.Method = "REPLY"; // CalendarMethods.Reply;
        var attendeeEmail = "user2@example.com";
        var replyProperties = new List<string>{
            PropertyName.DateStart, PropertyName.DateEnd,
            PropertyName.DateStamp, PropertyName.Due, PropertyName.Duration,
            PropertyName.Uid,PropertyName.TimeTransparency,PropertyName.Created,
            PropertyName.RecurrenceDate,PropertyName.RecurrenceExceptionDate,
            PropertyName.RecurrenceExceptionRule,
            PropertyName.RecurrenceId, PropertyName.Organizer,
            PropertyName.Sequence, PropertyName.RequestStatus,
        };
        foreach (var mc in vcalendar.Children.OfType<RecurringComponent>())
        {
            var attendee = mc.Attendees.Get(attendeeEmail)?.Copy();
            Assert.NotNull(attendee);
            attendee.ParticipationStatus.Value = EventParticipationStatus.Accepted;
            var reply = replyCalendar.CreateChild(mc.GetType()) as RecurringComponent;
            Assert.NotNull(reply);
            reply.Properties.AddRange(mc.Properties
                .Where(p => replyProperties.Contains(p.Name))
                .Select(p => p.DeepClone())
            );
            reply.Attendees.Add(attendee);
            if (reply.Organizer is not null)
            {
                reply.Organizer.ScheduleStatus = "1.2;FAKE";
            }
        }

        AssertVerificationFile(replyCalendar, verificationFilename);
        AssertVerificationFile(vcalendar, source);
    }

    [Theory]
    [InlineData("attendee.result.ics")]
    [InlineData("base.escaping.result.ics")]
    public void ReadBackMutatedCalendarTest(string source)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        AssertVerificationFile(vcalendar, source);
    }

    [Fact]
    public void EnumGeneratorTest()
    {
        var request = "REQUEST";
        Assert.Equal("REQUEST", request);
        var requestNormal = CalendarMethods.Request.ToString();
        Assert.Equal("Request", requestNormal);
        // var someThing = ScheduleAgentParameter.ToToken("SomeThing");
        // Assert.Equal(ScheduleAgent.Server, someThing);
    }

    [Theory]
    [InlineData("oneuid.ics")]
    [InlineData("oneuid2.ics")]
    public void VCalendarOneUid(string source)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        Assert.NotNull(vcalendar.Builder);
        var occurrences = vcalendar.Children.OfType<RecurringComponent>();
        Assert.NotEmpty(occurrences);
        var hasOnlyOneUniqueUid = occurrences.Select(x => x.Uid).Distinct().Count() == 1;
        Assert.True(hasOnlyOneUniqueUid, "VCalendar contains more than one unique Id");
        var uids = occurrences.OrderBy(x => x.RecurrenceId);
        // Assert.Single(uids);
        // var uniqueGroup = uids.First();
        string? aUid = null;
        var recurrenceDate = Instant.MinValue;
        foreach (var oc in uids)
        {
            aUid = oc.Uid;
            if (oc.RecurrenceId is not null)
            {
                var crd = oc.RecurrenceId.ToInstant();
                Assert.NotNull(crd);
                if (recurrenceDate > crd)
                {
                    Assert.Fail("Wrongly sorted");
                }
                recurrenceDate = crd.Value;
            }
        }
        if (!vcalendar.TryGetUniqueId(out var mainUid))
        {
            Assert.Fail("Unique UID not found?");
        }
        Assert.Equal(aUid, mainUid);
        var gsc = vcalendar.GetRecurringComponents(mainUid);
        Assert.NotEmpty(gsc);
        var gfail = vcalendar.GetRecurringComponents(null);
        Assert.Empty(gfail);
        var gfail2 = vcalendar.GetRecurringComponents("not valid uid");
        Assert.Empty(gfail2);
    }

    [Theory]
    [InlineData("simple-vjournal-4.ics")]
    public void VCalendarNoUid(string source)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        Assert.NotNull(vcalendar.Builder);
        var occurrences = vcalendar.Children.OfType<RecurringComponent>();
        Assert.NotEmpty(occurrences);
        var hasOnlyOneUniqueUid = occurrences.Select(x => x.Uid).Distinct().Count() == 1;
        Assert.True(hasOnlyOneUniqueUid, "VCalendar contains more than one unique Id");
        var uids = occurrences.OrderBy(x => x.RecurrenceId);
        // Assert.Single(uids);
        // var uniqueGroup = uids.First();
        string? aUid = null;
        var recurrenceDate = Instant.MinValue;
        foreach (var oc in uids)
        {
            aUid = oc.Uid;
            if (oc.RecurrenceId is not null)
            {
                var crd = oc.RecurrenceId.ToInstant();
                Assert.NotNull(crd);
                if (recurrenceDate > crd)
                {
                    Assert.Fail("Wrongly sorted");
                }
                recurrenceDate = crd.Value;
            }
        }
        if (!vcalendar.TryGetUniqueId(out var mainUid))
        {
            Assert.Fail("Unique UID not found?");
        }
        Assert.Equal(aUid, mainUid);
    }

    private static void AssertVerificationFile(VCalendar vCalendar, string verificationFilename)
    {
        AssertVerificationFile(vCalendar.Serialize(), verificationFilename);
    }

    private static void AssertVerificationFile(string content, string verificationFilename)
    {
        var verificationContent = FileExtensions.ReadFileAsString(verificationFilename);
        if (!string.IsNullOrEmpty(verificationContent))
        {
            Assert.Equal(verificationContent, content);
        }
        else
        {
            FileExtensions.WriteFileAsString(content, verificationFilename);
        }
    }
}
