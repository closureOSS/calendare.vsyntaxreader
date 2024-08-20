using System;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Parsers;
using Calendare.VSyntaxReader.Properties;
using NodaTime;

namespace VSyntaxReader.Tests.DateTime;

public enum CaldavDateTimeType { ZonedDateTime, Floating, FloatingDate };

public class ParsingTest
{
    private readonly CalendarBuilder builder = new();

    [Fact]
    public void Internal01()
    {
        var vevent = new VEvent { Builder = builder, };
        var uidProperty = builder.BuildProperty(vevent, new Calendare.VSyntaxReader.CalendarObject(PropertyName.Uid, Guid.NewGuid().ToString(), []));
        Assert.NotNull(uidProperty);
        var dtStartProperty = builder.BuildProperty(vevent, new Calendare.VSyntaxReader.CalendarObject(PropertyName.DateStart, "20241231T103045Z", []));
        Assert.NotNull(dtStartProperty);
        vevent.Properties.AddRange([uidProperty, dtStartProperty]);
        var result = vevent.Deserialize();
        Assert.True(result, "VEVENT parsed properly");
        var dtStart = vevent.DateStart;
        Assert.NotNull(dtStart);
        Assert.NotNull(dtStart.Dt);
        Assert.Null(dtStart.FloatingDate);
        Assert.Null(dtStart.Floating);
        Assert.False(dtStart.IsDateOnly);
        var zdt = dtStart.Dt.Value;
        Assert.Equal(2024, zdt.Year);
        Assert.Equal(12, zdt.Month);
        Assert.Equal(31, zdt.Day);
        Assert.Equal(10, zdt.Hour);
        Assert.Equal(30, zdt.Minute);
        Assert.Equal(45, zdt.Second);
        Assert.Equal("UTC", zdt.Zone.Id);
    }

    [Fact]
    public void Equality()
    {
        var tzZH = TimezoneParser.TryReadTimezone("Europe/Zurich", out var timezoneZH);
        var tzLA = TimezoneParser.TryReadTimezone("America/Los_Angeles", out var timezoneLA);
        Assert.True(tzZH);
        Assert.True(tzLA);
        Assert.NotNull(timezoneZH);
        Assert.NotNull(timezoneLA);
        var now = SystemClock.Instance.GetCurrentInstant();
        var ld = new LocalDate(2025, 1, 2);
        var ldt = new LocalDateTime(2025, 1, 2, 3, 4, 5);
        var zdtZH = now.InZone(timezoneZH);
        var zdtLA = now.InZone(timezoneLA);
        Assert.NotEqual(zdtLA, zdtZH);

        var iZH = zdtZH.ToInstant();
        var iLA = zdtLA.ToInstant();
        Assert.Equal(iZH, iLA);

        var zdonly = timezoneZH.AtStartOfDay(ldt.Date);
        var w = new CaldavDateTime(ld);
        var x1 = new CaldavDateTime(zdtZH);
        var x2 = new CaldavDateTime(zdtLA);
        var y = new CaldavDateTime(zdonly, true);
        var z = new CaldavDateTime(ldt);
        Assert.NotEqual(x1, y);
        Assert.NotEqual(x1, z);
        Assert.NotEqual(y, z);
        Assert.NotEqual(y, w);
        Assert.Equal(w, w);
        Assert.Equal(x1, x1);
        Assert.NotEqual(x1, x2);
        Assert.Equal(0, x1.CompareTo(x2));
    }

    [Theory]
    [InlineData("20241231T103045Z", CaldavDateTimeType.ZonedDateTime, false, null, "DTSTART:20241231T103045Z")]
    [InlineData("20220720T091000", CaldavDateTimeType.ZonedDateTime, false, "America/New_York", "DTSTART;TZID=America/New_York:20220720T091000")]
    [InlineData("20240814T100000", CaldavDateTimeType.ZonedDateTime, false, "GMT Standard Time", "DTSTART;TZID=GMT Standard Time:20240814T100000")]
    [InlineData("00011028T020000", CaldavDateTimeType.Floating, false, null, "DTSTART:00011028T020000")]
    [InlineData("20240803", CaldavDateTimeType.FloatingDate, true, null, "DTSTART;VALUE=DATE:20240803")]
    [InlineData("20240803", CaldavDateTimeType.ZonedDateTime, true, "Europe/Zurich", "DTSTART;VALUE=DATE;TZID=Europe/Zurich:20240803")]
    public void ParsingDtStart(string dateAsString, CaldavDateTimeType dtType, bool isDateOnly, string? tzId, string expected)
    {
        var vevent = new VEvent { Builder = builder, };
        var uidProperty = builder.BuildProperty(vevent, new Calendare.VSyntaxReader.CalendarObject(PropertyName.Uid, Guid.NewGuid().ToString(), []));
        Assert.NotNull(uidProperty);
        var dtStartPropertyObject = new Calendare.VSyntaxReader.CalendarObject(PropertyName.DateStart, dateAsString, []);
        if (isDateOnly)
        {
            dtStartPropertyObject.Parameters.Add(new Calendare.VSyntaxReader.CalendarObjectParameter(DateTimeArrayProperty.ValueTypeParam, ValueDataTypeNames.Date));
        }
        if (tzId is not null)
        {
            dtStartPropertyObject.Parameters.Add(new Calendare.VSyntaxReader.CalendarObjectParameter(DateTimeProperty.TimezoneIdParam, tzId));
        }
        var dtStartProperty = builder.BuildProperty(vevent, dtStartPropertyObject);
        Assert.NotNull(dtStartProperty);
        vevent.Properties.AddRange([uidProperty, dtStartProperty]);
        var result = vevent.Deserialize();
        Assert.True(result, "VEVENT parsed properly");
        var dtStart = vevent.DateStart;
        Assert.NotNull(dtStart);
        switch (dtType)
        {
            case CaldavDateTimeType.ZonedDateTime:
                Assert.NotNull(dtStart.Dt);
                Assert.Null(dtStart.FloatingDate);
                Assert.Null(dtStart.Floating);
                break;
            case CaldavDateTimeType.Floating:
                Assert.Null(dtStart.Dt);
                Assert.Null(dtStart.FloatingDate);
                Assert.NotNull(dtStart.Floating);
                break;
            case CaldavDateTimeType.FloatingDate:
                Assert.Null(dtStart.Dt);
                Assert.NotNull(dtStart.FloatingDate);
                Assert.Null(dtStart.Floating);
                break;
        }
        var normalizedInstant = dtStart.ToInstant(null);
        Assert.NotNull(normalizedInstant);
        var normalizedZoned = dtStart.GetNormalizedInZone(null);
        Assert.NotNull(normalizedZoned);
        var serialized = dtStartProperty.Serialize().ReplaceLineEndings("");
        Assert.Equal(expected, serialized);
    }

    [Theory]
    [InlineData("not-a-date", null, null, "DTSTART:20241231T103045Z")]
    [InlineData("", null, "Europe/Zurich", "DTSTART;VALUE=DATE;TZID=Europe/Zurich:20240803")]
    [InlineData("20241331T103045Z", null, null, "DTSTART:20241231T103045Z")]
    [InlineData("20240320T103045Z", "DATE", null, "DTSTART:20241231T103045Z")]
    [InlineData("20240320T103045Z", "Datum", null, "DTSTART:20241231T103045Z")]
    //[InlineData("20240320T103045Z", "PERIOD", null, "DTSTART:20241231T103045Z")]
    [InlineData("20220720T291000", null, "America/New_York", "DTSTART;TZID=America/New_York:20220720T091000")]
    [InlineData("20240814T100063", null, "GMT Standard Time", "DTSTART;TZID=GMT Standard Time:20240814T100000")]
    [InlineData("00011028t020000", null, null, "DTSTART:00011028T020000")]
    [InlineData("2024083", null, null, "DTSTART;VALUE=DATE:20240803")]
    [InlineData("202408034", null, "Europe/Zurich", "DTSTART;VALUE=DATE;TZID=Europe/Zurich:20240803")]
    [InlineData("20220320T103045Z,20210320T103045Z", null, null, "DTSTART:20241231T103045Z")]
    public void ParsingFailures(string dateAsString, string? valueType, string? tzId, string expected)
    {
        var vevent = new VEvent { Builder = builder, };
        var uidProperty = builder.BuildProperty(vevent, new Calendare.VSyntaxReader.CalendarObject(PropertyName.Uid, Guid.NewGuid().ToString(), []));
        Assert.NotNull(uidProperty);
        var dtStartPropertyObject = new Calendare.VSyntaxReader.CalendarObject(PropertyName.DateStart, dateAsString, []);
        if (valueType is null)
        {
            dtStartPropertyObject.Parameters.Add(new Calendare.VSyntaxReader.CalendarObjectParameter(DateTimeArrayProperty.ValueTypeParam, ValueDataTypeNames.DateTime));
        }
        else
        {
            dtStartPropertyObject.Parameters.Add(new Calendare.VSyntaxReader.CalendarObjectParameter(DateTimeArrayProperty.ValueTypeParam, valueType));
        }
        if (tzId is not null)
        {
            dtStartPropertyObject.Parameters.Add(new Calendare.VSyntaxReader.CalendarObjectParameter(DateTimeProperty.TimezoneIdParam, tzId));
        }
        var dtStartProperty = builder.BuildProperty(vevent, dtStartPropertyObject);
        Assert.NotNull(dtStartProperty);
        vevent.Properties.AddRange([uidProperty, dtStartProperty]);
        var result = vevent.Deserialize();
        Assert.False(result, $"VEVENT not parsed properly {expected}");
        Assert.NotNull(result.ErrorMessage);
        // Assert.Contains("failed", result.ErrorMessage);
        var dtStart = vevent.DateStart;
        Assert.Null(dtStart);
    }
}
