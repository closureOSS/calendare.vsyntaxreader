using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Operations;
using NodaTime;
using NodaTime.TimeZones;
using Xunit.Abstractions;

namespace VSyntaxReader.Tests.Occurrences;

public class OccurrenceTest
{
    private readonly ITestOutputHelper Output;
    private readonly CalendarBuilder Builder = new();

    public OccurrenceTest(ITestOutputHelper output)
    {
        Output = output;
    }

    [Theory]
    [InlineData("normal01.ics", "normal01.result")]
    [InlineData("normal02.ics", "normal02.result")]
    [InlineData("normal-dateonly.ics", "normal-dateonly.result")]
    [InlineData("out-of-evalrange.ics", "out-of-evalrange.result")]
    [InlineData("no-recurrence.ics", "no-recurrence.result")]
    [InlineData("daily-duration.ics", "daily-duration.result")]
    [InlineData("extensive-recurrence.ics", "extensive-recurrence.result")]
    [InlineData("rdate.ics", "rdate.result")]
    [InlineData("rdate-period.ics", "rdate-period.result")]
    [InlineData("synthetic01.ics", "synthetic01.result")]
    public void StandardOccurrence(string filename, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(filename);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        var sop = new ZonedDateTime(new LocalDateTime(2024, 08, 01, 0, 0), DateTimeZone.Utc, Offset.Zero);
        var eop = new ZonedDateTime(new LocalDateTime(2024, 08, 31, 23, 59), DateTimeZone.Utc, Offset.Zero);
        var evalPeriod = new Interval(sop.ToInstant(), eop.ToInstant());
        var occurrences = vcalendar.GetOccurrences(evalPeriod, DateTimeZone.Utc);
        Assert.NotNull(occurrences);
        var content = GetOccurrenceList(occurrences);
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
    [InlineData("normal01.ics", "normal01-exdate.result")]
    [InlineData("normal02.ics", "normal02-exdate.result")]
    [InlineData("rdate.ics", "rdate-exdate.result")]
    [InlineData("recurrence-exdate-nonutc.ics", "recurrence-exdate-nonutc.result")]
    public void ExceptionDateOccurrence(string filename, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(filename);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        var first = vcalendar.Children.OfType<RecurringComponent>().FirstOrDefault();
        Assert.NotNull(first);
        string content = string.Empty;
        foreach (var exdate in first.ExceptionDates.Dates ?? [])
        {
            var exdateNormalized = exdate.ToInstant();
            Assert.NotNull(exdateNormalized);
            var evalPeriod = new Interval(exdateNormalized.Value.Plus(-Duration.FromHours(24)), exdateNormalized.Value.Plus(Duration.FromHours(24)));
            var occurrences = vcalendar.GetOccurrences(evalPeriod, null);
            Assert.NotNull(occurrences);
            content += GetOccurrenceList(occurrences) + Environment.NewLine + Environment.NewLine;
        }
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
    [InlineData("daylight-dateonly.ics", "Europe/Zurich", "daylight-dateonly.zh.result")]
    [InlineData("daylight-dateonly.ics", "America/New_York", "daylight-dateonly.ny.result")]
    [InlineData("daylight-dateonly-floating.ics", "Europe/Zurich", "daylight-dateonly-floating.zh.result")]
    [InlineData("daylight-dateonly-floating.ics", "America/New_York", "daylight-dateonly-floating.ny.result")]
    [InlineData("daylight-hourly.ics", "Europe/Zurich", "daylight-hourly.zh.result")]
    [InlineData("daylight-hourly-floating.ics", "Europe/Zurich", "daylight-hourly-floating.zh.result")]
    [InlineData("daylight-hourly-floating.ics", "America/New_York", "daylight-hourly-floating.ny.result")]
    public void NonUtcOccurrence(string filename, string? tzId, string verificationFilename)
    {

        var sourceFilename = FileExtensions.BuildSourceFilename(filename);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        var sop = new ZonedDateTime(new LocalDateTime(2024, 10, 01, 0, 0), DateTimeZone.Utc, Offset.Zero);
        var eop = new ZonedDateTime(new LocalDateTime(2024, 11, 30, 23, 59), DateTimeZone.Utc, Offset.Zero);
        var evalPeriod = new Interval(sop.ToInstant(), eop.ToInstant());
        DateTimeZone? timezone = null;
        if (tzId is not null)
        {
            timezone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tzId);
            if (timezone is null)
            {
                if (TzdbDateTimeZoneSource.Default.WindowsToTzdbIds.TryGetValue(tzId, out var olsenId))
                {
                    timezone ??= DateTimeZoneProviders.Tzdb.GetZoneOrNull(olsenId);
                }
            }
        }
        timezone ??= DateTimeZone.Utc;
        var occurrences = vcalendar.GetOccurrences(evalPeriod, timezone);
        Assert.NotNull(occurrences);
        var content = GetOccurrenceList(occurrences);
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
    [InlineData("availability01.ics", "availability01.result")]
    [InlineData("availability02.ics", "availability02.result")]
    public void VAvaibilityOccurrence(string filename, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(filename);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        var sop = new ZonedDateTime(new LocalDateTime(2021, 10, 01, 0, 0), DateTimeZone.Utc, Offset.Zero);
        var eop = new ZonedDateTime(new LocalDateTime(2021, 12, 31, 23, 59), DateTimeZone.Utc, Offset.Zero);
        var evalPeriod = new Interval(sop.ToInstant(), eop.ToInstant());
        DateTimeZone? timezone = null;
        timezone ??= DateTimeZone.Utc;
        var occurrences = vcalendar.GetOccurrences(evalPeriod, timezone);
        Assert.NotNull(occurrences);
        var content = GetOccurrenceList(occurrences);
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

    private static string GetOccurrenceList(List<Occurrence> occurrences)
    {
        StringBuilder sb = new();
        foreach (var occ in occurrences)
        {
            sb.AppendLine($"{occ.Interval} {(occ.IsReccurring ? "" : "Single ")}{(occ.IsSynthetic is not null && occ.IsSynthetic.Value ? "Synthetic " : "")}{occ.Source.RecurrenceId}");
        }
        sb.AppendLine($"# {occurrences.Count} occurrences");
        return sb.ToString();
    }
}
