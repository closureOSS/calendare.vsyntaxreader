using System.Collections.Generic;
using System.Text;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Operations;
using Calendare.VSyntaxReader.Parsers;
using NodaTime;
using Xunit.Abstractions;

namespace VSyntaxReader.Tests.Occurrences;

public class OccurrenceCalculationTest
{
    private readonly ITestOutputHelper Output;
    private readonly CalendarBuilder Builder = new();

    public OccurrenceCalculationTest(ITestOutputHelper output)
    {
        Output = output;
    }

    [Theory]
    [InlineData("rdate.ics")]
    [InlineData("synthetic01.ics")]
    [InlineData("extensive-recurrence.ics")]
    public void CheckOccurrences(string filename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(filename);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        var tzZH = TimezoneParser.TryReadTimezone("Europe/Zurich", out var timezoneZH);
        var tzLA = TimezoneParser.TryReadTimezone("America/Los_Angeles", out var timezoneLA);
        Assert.True(tzZH);
        Assert.True(tzLA);
        Assert.NotNull(timezoneZH);
        Assert.NotNull(timezoneLA);
        var sop = new ZonedDateTime(new LocalDateTime(2024, 08, 01, 0, 0), DateTimeZone.Utc, Offset.Zero);
        var eop = new ZonedDateTime(new LocalDateTime(2024, 08, 31, 23, 59), DateTimeZone.Utc, Offset.Zero);
        var evalPeriod = new Interval(sop.ToInstant(), eop.ToInstant());
        var occurrences = vcalendar.GetOccurrences(evalPeriod, DateTimeZone.Utc);
        Assert.NotNull(occurrences);
        // var content = GetOccurrenceList(occurrences);
        // 2024-08-20T09:00:00Z
        var is1 = occurrences[4].Interval.Start;
        var d1 = new CaldavDateTime(is1.InUtc());
        var c1 = vcalendar.IsValidRecurrenceDate(d1);
        Assert.True(c1);

        var d2 = new CaldavDateTime(is1.InZone(timezoneLA));
        var c2 = vcalendar.IsValidRecurrenceDate(d2);
        Assert.True(c2);

        var d3 = new CaldavDateTime(occurrences[3].Interval.Start.Plus(Duration.FromHours(-2)).InUtc());
        var c3 = vcalendar.IsValidRecurrenceDate(d3);
        Assert.False(c3);
        // var verificationContent = FileExtensions.ReadFileAsString(verificationFilename);
        // if (!string.IsNullOrEmpty(verificationContent))
        // {
        //     Assert.Equal(verificationContent, content);
        // }
        // else
        // {
        //     FileExtensions.WriteFileAsString(content, verificationFilename);
        // }
    }

    [Theory]
    [InlineData("synthetic02.ics", "synthetic02.result")]
    public void VerifyOccurrenceTest(string filename, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(filename);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        var tzZH = TimezoneParser.TryReadTimezone("Europe/Zurich", out var timezoneZH);
        var tzLA = TimezoneParser.TryReadTimezone("America/Los_Angeles", out var timezoneLA);
        Assert.True(tzZH);
        Assert.True(tzLA);
        Assert.NotNull(timezoneZH);
        Assert.NotNull(timezoneLA);
        var sop = new ZonedDateTime(new LocalDateTime(2024, 01, 01, 0, 0), DateTimeZone.Utc, Offset.Zero);
        var eop = new ZonedDateTime(new LocalDateTime(2025, 08, 31, 23, 59), DateTimeZone.Utc, Offset.Zero);
        var evalPeriod = new Interval(sop.ToInstant(), eop.ToInstant());
        var occurrences = vcalendar.GetOccurrences(evalPeriod, timezoneLA);
        Assert.NotNull(occurrences);
        var content = GetOccurrenceList(occurrences);
        var xx = vcalendar.TryGetUniqueId(out var uid);
        Assert.True(xx);
        Assert.NotNull(uid);
        StringBuilder sb = new();
        sb.AppendLine($"Calendar {filename} with {uid}");
        sb.AppendLine(content);
        foreach (var rc in vcalendar.GetRecurringComponents(uid))
        {
            if (rc.RecurrenceId is not null)
            {
                var isValidRecurrenceDate = vcalendar.IsValidRecurrenceDate(rc.RecurrenceId);
                Assert.True(isValidRecurrenceDate);
                var co = rc.ToOccurrence(vCalendar: vcalendar);
                Assert.NotNull(co);
                sb.AppendLine($"{co.Interval} -> {rc.RecurrenceId} {(isValidRecurrenceDate ? "" : "(INVALID)")} {(co?.IsSynthetic == true ? "Synthetic" : "")}");
            }
        }
        sb.AppendLine("--eof--");
        var verificationContent = FileExtensions.ReadFileAsString(verificationFilename);
        if (!string.IsNullOrEmpty(verificationContent))
        {
            Assert.Equal(verificationContent, sb.ToString());
        }
        else
        {
            FileExtensions.WriteFileAsString(sb.ToString(), verificationFilename);
        }
    }


    private static string GetOccurrenceList(List<Occurrence> occurrences)
    {
        StringBuilder sb = new();
        var instanceCnt = 0;
        foreach (var occ in occurrences)
        {
            if (occ.Source.RecurrenceId is not null)
            {
                sb.AppendLine($"{occ.Interval} -- {occ.Source.RecurrenceId} {(occ.IsReccurring ? "" : "Single ")}{(occ.IsSynthetic is not null && occ.IsSynthetic.Value ? "Synthetic " : "")}");
                instanceCnt++;
            }
        }
        sb.AppendLine($"# {instanceCnt} of {occurrences.Count} occurrences");
        return sb.ToString();
    }
}
