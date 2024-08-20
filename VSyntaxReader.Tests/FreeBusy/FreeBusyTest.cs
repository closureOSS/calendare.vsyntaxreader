using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Operations;
using Calendare.VSyntaxReader.Parsers;
using Calendare.VSyntaxReader.Properties;
using NodaTime;
using Xunit.Abstractions;

namespace VSyntaxReader.Tests.FreeBusy;


public class FreeBusyTest
{
    private readonly ITestOutputHelper Output;
    private readonly ICalendarBuilder Builder = new CalendarBuilder();

    public FreeBusyTest(ITestOutputHelper output)
    {
        Output = output;
    }



    [Fact]
    public void CreateFreeBusyCalendar()
    {
        var vcalendar = Builder.CreateCalendar();
        Assert.NotNull(vcalendar);
        vcalendar.ProductIdentifier = "-//closure.ch//NONSGML Calendare Testsuite/EN";

        var freebusy = vcalendar.CreateChild<VFreebusy>();
        Assert.NotNull(freebusy);

        freebusy.Uid = Guid.NewGuid().ToString();
        freebusy.DateStamp = default;
        var sop = new ZonedDateTime(new LocalDateTime(2024, 08, 01, 0, 0), DateTimeZone.Utc, Offset.Zero);
        freebusy.DateStart = sop.ToInstant();
        freebusy.DateEnd = sop.PlusHours(31 * 24).ToInstant();

        List<FreeBusyEntry> freeBusyEntries = [];
        freeBusyEntries.Add(new FreeBusyEntry { Period = new Interval(sop.PlusHours(6).PlusSeconds(30).ToInstant(), sop.PlusHours(8).ToInstant()) });
        freeBusyEntries.Add(new FreeBusyEntry { Period = new Interval(sop.PlusHours(9).ToInstant(), sop.PlusHours(10).ToInstant()) });
        freeBusyEntries.Add(new FreeBusyEntry { Period = new Interval(sop.PlusHours(9).PlusMinutes(10).ToInstant(), sop.PlusHours(10).PlusMinutes(-10).ToInstant()), Status = FreeBusyStatus.BusyTentative });
        freeBusyEntries.Add(new FreeBusyEntry { Period = new Interval(sop.PlusHours(24).ToInstant(), sop.PlusHours(28).ToInstant()), Status = FreeBusyStatus.BusyUnavailable });
        freebusy.SetFreeBusyEntries(freeBusyEntries);
        var result = vcalendar.Serialize();
    }

    [Fact]
    public void ReplyToFreeBusy()
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
        var rangeFrom = freeBusyRequest.DateStart;
        var rangeTo = freeBusyRequest.DateEnd;

        var requestMethodProp = vcal.FindOneProperty<TextProperty>(PropertyName.Method);
        if (requestMethodProp is null)
        {
            Output.WriteLine($"{PropertyName.Method} is null: {vcal.Properties.Count}/{string.Join(';', vcal.Properties.Select(x => x.Name))}");
            var againProp = vcal.FindFirstProperty<TextProperty>(PropertyName.Method);
            Output.WriteLine($"{PropertyName.Method} => {againProp?.Name}");
        }
        Assert.NotNull(requestMethodProp);
        var organizerProp = freeBusyRequest.FindOneProperty<OrganizerProperty>(PropertyName.Organizer);
        Assert.NotNull(organizerProp);
        var requestUid = freeBusyRequest.Uid;
        var attendeProps = freeBusyRequest.FindAllProperties<AttendeeProperty>(PropertyName.Attendee);
        Assert.NotNull(attendeProps);
        Assert.NotEmpty(attendeProps);
        var maskUid = freeBusyRequest.MaskUid;
        // TODO: Set vcal.Method
    }

    [Theory]
    [InlineData("scenario1.txt", "scenario1.result")]
    [InlineData("scenario2.txt", "scenario2.result")]
    [InlineData("scenario3.txt", "scenario3.result")]
    [InlineData("scenario4.txt", "scenario4.result")]
    [InlineData("scenario5.txt", "scenario5.result")]
    [InlineData("scenario6.txt", "scenario6.result")]
    [InlineData("scenario7.txt", "scenario7.result")]
    public void CalculateFreeBusyTest(string scenarioFile, string verificationFilename)
    {
        var calendars = LoadCalendars(scenarioFile);
        var sop = new ZonedDateTime(new LocalDateTime(2024, 01, 01, 0, 0), DateTimeZone.Utc, Offset.Zero);
        var eop = new ZonedDateTime(new LocalDateTime(2024, 01, 31, 23, 59), DateTimeZone.Utc, Offset.Zero);
        var evalPeriod = new Interval(sop.ToInstant(), eop.ToInstant());
        Assert.True(TimezoneParser.TryReadTimezone("Asia/Bangkok", out var calendarTimeZone));
        calendarTimeZone ??= DateTimeZone.Utc;
        List<ICalendarComponent> components = [];
        foreach (var vcal in calendars)
        {
            components.AddRange(vcal.Children);
        }
        var freeBusyEntries = components.GetFreeBusyEntries(evalPeriod, calendarTimeZone: calendarTimeZone);
        Assert.NotNull(freeBusyEntries);
        var content = ToFreeBusy(freeBusyEntries);
        // FileExtensions.WriteFileAsString(content, verificationFilename); // REMOVE, override any verification result
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

    [Theory]
    [InlineData("calendar01.ics", "calendar01.result", "20060930T120000Z", "20070630T115959Z", "Europe/Zurich")]
    [InlineData("calendar02.ics", "calendar02.result", "20241018T110000Z", "20241019T110000Z")]
    [InlineData("calendar03.ics", "calendar03.result", "20241018T110000Z", "20241019T110000Z")]
    [InlineData("calendar04.ics", "calendar04.result", "20241001T000000Z", "20241201T000000Z")]
    [InlineData("calendar04ava.ics", "calendar04ava.result", "20241001T000000Z", "20241201T000000Z")]
    [InlineData("calendar04ava.ics", "calendar04ava-2.result", "20241008T220000Z", "20241011T220000Z")]
    [InlineData("calendar05.ics", "calendar05.result", "20241008T220000Z", "20241011T220000Z")]
    [InlineData("calendar06.ics", "calendar06.result", "20241112T000000Z", "20241205T121500Z")]
    [InlineData("availability01.ics", "availability01.result", "20240101T000000Z", "20240301T000000Z")]
    [InlineData("availability02.ics", "availability02.result", "20240101T000000Z", "20240301T000000Z")]
    public void CalendarFreeBusyTest(string calendarFile, string verificationFilename, string rangeStart, string rangeEnd, string? calTzId = null)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(calendarFile);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        if (!parseResult || vcalendar is null)
        {
            Assert.True(parseResult, parseResult.ErrorMessage);
        }
        Assert.NotNull(vcalendar);
        var sop = ParseDateTime(rangeStart);
        var eop = ParseDateTime(rangeEnd);
        var evalPeriod = new Interval(sop.ToInstant(), eop.ToInstant());
        var calendarTimeZone = DateTimeZone.Utc;
        if (calTzId is not null)
        {
            Assert.True(TimezoneParser.TryReadTimezone("Europe/Zurich", out calendarTimeZone));
        }
        var components = vcalendar.Children;
        var freeBusyEntries = components.GetFreeBusyEntries(evalPeriod, calendarTimeZone: calendarTimeZone);
        Assert.NotNull(freeBusyEntries);
        var content = ToFreeBusy(freeBusyEntries);
        // FileExtensions.WriteFileAsString(content, verificationFilename); // REMOVE, override any verification result
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

    [Theory]
    [InlineData("calendar01.ics", "calendar01-masked.result", "4aaf8f37-f232-4c8e-a72e-e171d4c4fe54", "20060930T120000Z", "20070630T115959Z", "Europe/Zurich")]
    [InlineData("calendar02.ics", "calendar02-masked.result", "caa9758c-f1ed-48b0-99c3-2ba1dfd3ccb4", "20241018T110000Z", "20241019T110000Z")]
    [InlineData("calendar03.ics", "calendar03-masked.result", "E9F98477-A9C2-43F9-8371-CBA0CBCB03EE", "20241018T110000Z", "20241019T110000Z")]
    [InlineData("calendar04.ics", "calendar04.result", "DOES-NOT-EXIST", "20241001T000000Z", "20241201T000000Z")]
    public void CalendarFreeBusyMaskUidTest(string calendarFile, string verificationFilename, string maskUid, string rangeStart, string rangeEnd, string? calTzId = null)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(calendarFile);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        if (!parseResult || vcalendar is null)
        {
            Assert.True(parseResult, parseResult.ErrorMessage);
        }
        Assert.NotNull(vcalendar);
        var sop = ParseDateTime(rangeStart);
        var eop = ParseDateTime(rangeEnd);
        var evalPeriod = new Interval(sop.ToInstant(), eop.ToInstant());
        var calendarTimeZone = DateTimeZone.Utc;
        if (calTzId is not null)
        {
            Assert.True(TimezoneParser.TryReadTimezone("Europe/Zurich", out calendarTimeZone));
        }
        var components = vcalendar.Children;
        var freeBusyEntries = components.GetFreeBusyEntries(evalPeriod, maskUid: maskUid, calendarTimeZone: calendarTimeZone);
        Assert.NotNull(freeBusyEntries);
        var content = ToFreeBusy(freeBusyEntries);
        // FileExtensions.WriteFileAsString(content, verificationFilename); // REMOVE, override any verification result
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

    [Theory]
    [InlineData("calendar01.ics", "timezoneids01.result")]
    [InlineData("calendar02.ics", "timezoneids02.result")]
    [InlineData("calendar03.ics", "timezoneids03.result")]
    public void DetectTimezonesTest(string calendarFile, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(calendarFile);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        if (!parseResult || vcalendar is null)
        {
            Assert.True(parseResult, parseResult.ErrorMessage);
        }
        Assert.NotNull(vcalendar);
        HashSet<string> timezones = [];
        foreach (var component in vcalendar.Children)
        {
            var tzProps = component.Properties.OfType<IPropertyTimezoneId>();
            Assert.NotNull(tzProps);
            foreach (var tzProp in tzProps)
            {
                if (tzProp.TzId is not null)
                {
                    timezones.Add(tzProp.TzId);
                }
            }
        }
        var content = string.Join(Environment.NewLine, timezones.OrderBy(x => x.ToLowerInvariant()));
        // FileExtensions.WriteFileAsString(content, verificationFilename); // REMOVE, override any verification result
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

    private static ZonedDateTime ParseDateTime(string stringDate)
    {
        Assert.True(DateTimeParser.TryReadDateTime(stringDate, DateTimeZone.Utc, out var datetime));
        Assert.NotNull(datetime.Dt);
        return datetime.Dt.Value;
    }

    private List<VCalendar> LoadCalendars(string scenarioFile)
    {
        var calendarFiles = FileExtensions.ReadFileAllLines(scenarioFile);
        return LoadCalendars(calendarFiles.ToArray());
    }

    private List<VCalendar> LoadCalendars(string[] calfiles)
    {
        List<VCalendar> result = [];
        foreach (var cf in calfiles)
        {
            var sourceFilename = FileExtensions.BuildSourceFilename(cf);
            var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
            if (parseResult && vcalendar is not null)
            {
                result.Add(vcalendar);
            }
            else
            {
                Assert.True(parseResult, parseResult.ErrorMessage);
            }
        }
        return result;
    }

    private static string ToFreeBusy(List<FreeBusyEntry> freeBusyEntries)
    {
        var sb = new StringBuilder();
        foreach (var fbe in freeBusyEntries)
        {
            sb.AppendLine($"{fbe.Period} {fbe.Status:g}");
        }
        return sb.ToString();
    }
}
