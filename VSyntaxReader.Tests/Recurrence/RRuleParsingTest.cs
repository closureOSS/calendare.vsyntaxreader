using System;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Properties;

namespace VSyntaxReader.Tests.Recurrence;

public class RRuleParsingTest
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
        var rruleProperty = builder.BuildProperty(vevent, new Calendare.VSyntaxReader.CalendarObject(PropertyName.RecurrenceRule, "FREQ=MINUTELY;INTERVAL=15;BYDAY=MO,TU,WE,TH,FR;BYHOUR=9,10,11,12,13,14,15,16", []));
        Assert.NotNull(rruleProperty);
        vevent.Properties.AddRange([uidProperty, dtStartProperty, rruleProperty]);
        var result = vevent.Deserialize();
        Assert.True(result, "RRULE parsed properly");
    }

    [Fact]
    public void Internal02()
    {
        var vevent = new VEvent { Builder = builder, };
        var uidProperty = builder.BuildProperty(vevent, new Calendare.VSyntaxReader.CalendarObject(PropertyName.Uid, Guid.NewGuid().ToString(), []));
        Assert.NotNull(uidProperty);
        var rruleProperty = builder.BuildProperty(vevent, new Calendare.VSyntaxReader.CalendarObject(PropertyName.RecurrenceRule, "FREQ=MINUTELY;INTERVAL=15;BYDAY=MO,TU,WE,TH,FR;BYHOUR=9,10,11,12,13,14,15,16", []));
        Assert.NotNull(rruleProperty);
        vevent.Properties.AddRange([uidProperty, rruleProperty]);
        var result = vevent.Deserialize();
        Assert.False(result, "RRULE parsed properly");
    }

    [Fact]
    public void RRule01()
    {
        var rrule = CompileVEventWithRRule("20241231T103045Z", "FREQ=HOURLY;INTERVAL=3;COUNT=12");
        Assert.NotNull(rrule);
        if (rrule is not null)
        {
            Assert.True(rrule.Frequency == FrequencyType.Hourly && rrule.Count == 12 && rrule.Interval == 3, "RRULE properly read");
        }
    }

    [Fact]
    public void RRule02()
    {
        var rrule = CompileVEventWithRRule("20230326T000000Z", "FREQ=DAILY;INTERVAL=3;UNTIL=20240125T000000Z");
        Assert.NotNull(rrule);
        if (rrule is not null)
        {
            Assert.True(rrule.Frequency == FrequencyType.Daily && rrule.Interval == 3 && rrule.Until != null, "RRULE properly read");
            Assert.Empty(rrule.BySecond);
            Assert.Empty(rrule.ByMinute);
            Assert.Empty(rrule.ByHour);
            Assert.Empty(rrule.ByDay);
            Assert.Empty(rrule.ByMonthDay);
            Assert.Empty(rrule.ByYearDay);
            Assert.Empty(rrule.ByWeekNo);
            Assert.Empty(rrule.ByMonth);
            Assert.Empty(rrule.BySetPosition);
        }
    }

    [Fact]
    public void RRule03()
    {
        var rrule = CompileVEventWithRRule("20231231T060000Z", "FREQ=DAILY;BYDAY=SA,SU;BYHOUR=6,7");
        Assert.NotNull(rrule);
        if (rrule is not null)
        {
            Assert.True(rrule.Frequency == FrequencyType.Daily && rrule.Interval == 1 && rrule.Until == null, "RRULE properly read");
            Assert.Empty(rrule.BySecond);
            Assert.Empty(rrule.ByMinute);
            Assert.Equal(rrule.ByHour, new int[] { 6, 7 });
            Assert.Equal(rrule.ByDay, new DayOfWeekOffset[] {
                new(NodaTime.IsoDayOfWeek.Saturday),
                new(NodaTime.IsoDayOfWeek.Sunday)
            });
            Assert.Empty(rrule.ByMonthDay);
            Assert.Empty(rrule.ByYearDay);
            Assert.Empty(rrule.ByWeekNo);
            Assert.Empty(rrule.ByMonth);
            Assert.Empty(rrule.BySetPosition);
        }
    }

    [Fact]
    public void RRule04()
    {
        var vevent = CreateVEventWithRRule("20231231T000000Z", "FREQ=WEEKLY;INTERVAL=2;BYDAY=TU,WE,FR;WKST=SU");
        var result = vevent.Deserialize();
        Assert.True(result, "RRULE parsed properly");
    }

    [Fact]
    public void RRule05()
    {
        var rrule = CompileVEventWithRRule("20240103T000000Z", "FREQ=MONTHLY;INTERVAL=2;COUNT=16;BYDAY=MO,-2TU,+1WE,3TH");
        Assert.NotNull(rrule);
        if (rrule is not null)
        {
            Assert.True(rrule.Frequency == FrequencyType.Monthly && rrule.Interval == 2 && rrule.Count == 16, "RRULE properly read");
            Assert.Empty(rrule.BySecond);
            Assert.Empty(rrule.ByMinute);
            Assert.Empty(rrule.ByHour);
            Assert.Equal(rrule.ByDay, new DayOfWeekOffset[] {
                new(NodaTime.IsoDayOfWeek.Monday),
                new(NodaTime.IsoDayOfWeek.Tuesday,-2),
                new(NodaTime.IsoDayOfWeek.Wednesday,1),
                new(NodaTime.IsoDayOfWeek.Thursday,3),
            });
            Assert.Empty(rrule.ByMonthDay);
            Assert.Empty(rrule.ByYearDay);
            Assert.Empty(rrule.ByWeekNo);
            Assert.Empty(rrule.ByMonth);
            Assert.Empty(rrule.BySetPosition);
        }
    }

    [Fact]
    public void RRule06()
    {
        var rrule = CompileVEventWithRRule("20210210T000000Z", "FREQ=MONTHLY;INTERVAL=1;BYDAY=2WE;BYMONTHDAY=2;WKST=WE;UNTIL=20210317T000000Z");
        Assert.NotNull(rrule);
        if (rrule is not null)
        {
            Assert.True(rrule.Frequency == FrequencyType.Monthly && rrule.Interval == 1 && rrule.Until != null && rrule.FirstDayOfWorkWeek == NodaTime.IsoDayOfWeek.Wednesday, "RRULE properly read");
            Assert.Empty(rrule.BySecond);
            Assert.Empty(rrule.ByMinute);
            Assert.Empty(rrule.ByHour);
            Assert.Equal(rrule.ByDay, new DayOfWeekOffset[] {
                new(NodaTime.IsoDayOfWeek.Wednesday,2),
            });
            Assert.Equal(rrule.ByMonthDay, new int[] { 2 });
            Assert.Empty(rrule.ByYearDay);
            Assert.Empty(rrule.ByWeekNo);
            Assert.Empty(rrule.ByMonth);
            Assert.Empty(rrule.BySetPosition);
        }
    }

    [Fact]
    public void RRule07()
    {
        var vevent = CreateVEventWithRRule("20210103T000000Z", "FREQ=MONTHLY;COUNT=10;BYDAY=MO,TU,WE,TH,FR;BYSETPOS=1,-1");
        var result = vevent.Deserialize();
        Assert.True(result, "RRULE parsed properly");
    }

    [Fact]
    public void RRule08()
    {
        var vevent = CreateVEventWithRRule("20210103T000000Z", "FREQ=YEARLY;COUNT=8;INTERVAL=4;BYMONTH=4,10");
        var result = vevent.Deserialize();
        Assert.True(result, "RRULE parsed properly");
    }

    [Fact]
    public void RRule09()
    {
        var vevent = CreateVEventWithRRule("20210101T000000Z", "FREQ=YEARLY;COUNT=3;BYDAY=MO;BYWEEKNO=13,15,50");
        var result = vevent.Deserialize();
        Assert.True(result, "RRULE parsed properly");
    }

    [Fact]
    public void RRule10()
    {
        var vevent = CreateVEventWithRRule("20140802T001500Z", "FREQ=DAILY;BYSECOND=1;BYMINUTE=1;COUNT=2");
        var result = vevent.Deserialize();
        Assert.True(result, "RRULE parsed properly");
    }

    [Fact]
    public void RRule11()
    {
        var vevent = CreateVEventWithRRuleDateOnly("20140802", "FREQ=DAILY;BYSECOND=1;BYMINUTE=1;COUNT=2");
        var result = vevent.Deserialize();
        Assert.True(result, "RRULE parsed properly");
    }

    [Fact]
    public void RRule12()
    {
        var rrule = CompileVEventWithRRule("20210210T000000Z", "FREQ=YEARLY;INTERVAL=3;COUNT=10;BYYEARDAY=1,100,200");
        Assert.NotNull(rrule);
        if (rrule is not null)
        {
            Assert.True(rrule.Frequency == FrequencyType.Yearly && rrule.Interval == 3 && rrule.Count == 10, "RRULE properly read");
            Assert.Empty(rrule.BySecond);
            Assert.Empty(rrule.ByMinute);
            Assert.Empty(rrule.ByHour);
            Assert.Empty(rrule.ByDay);
            Assert.Empty(rrule.ByMonthDay);
            Assert.Equal(rrule.ByYearDay, new int[] { 1, 100, 200 });
            Assert.Empty(rrule.ByWeekNo);
            Assert.Empty(rrule.ByMonth);
            Assert.Empty(rrule.BySetPosition);
        }
    }

    [Fact]
    public void RRule13()
    {
        var rrule = CompileVEventWithRRule("20210210T000000Z", "FREQ=YEARLY;INTERVAL=3;COUNT=10;BYYEARDAY=200,1,100");
        Assert.NotNull(rrule);
        if (rrule is not null)
        {
            Assert.True(rrule.Frequency == FrequencyType.Yearly && rrule.Interval == 3 && rrule.Count == 10, "RRULE properly read");
            Assert.Empty(rrule.BySecond);
            Assert.Empty(rrule.ByMinute);
            Assert.Empty(rrule.ByHour);
            Assert.Empty(rrule.ByDay);
            Assert.Empty(rrule.ByMonthDay);
            Assert.Equal(rrule.ByYearDay, new int[] { 200, 1, 100 });
            Assert.Empty(rrule.ByWeekNo);
            Assert.Empty(rrule.ByMonth);
            Assert.Empty(rrule.BySetPosition);
        }
    }

    [Fact]
    public void RRule14()
    {
        var rrule = CompileVEventWithRRule("20140802T200000Z", "FREQ=DAILY;BYSECOND=1;BYMINUTE=2;COUNT=2");
        Assert.NotNull(rrule);
        if (rrule is not null)
        {
            Assert.True(rrule.Frequency == FrequencyType.Daily && rrule.Interval == 1 && rrule.Count == 2, "RRULE properly read");
            Assert.Equal(rrule.BySecond, new int[] { 1 });
            Assert.Equal(rrule.ByMinute, new int[] { 2 });
            Assert.Empty(rrule.ByHour);
            Assert.Empty(rrule.ByDay);
            Assert.Empty(rrule.ByMonthDay);
            Assert.Empty(rrule.ByYearDay);
            Assert.Empty(rrule.ByWeekNo);
            Assert.Empty(rrule.ByMonth);
            Assert.Empty(rrule.BySetPosition);
        }
    }

    [Fact]
    public void RRuleFailure00()
    {
        var vevent = CreateVEventWithRRule("20140802T001500Z", "bogus");
        var result = vevent.Deserialize();
        Assert.False(result, "RRULE failed to parsed");
    }

    [Fact]
    public void RRuleFailure01()
    {
        var vevent = CreateVEventWithRRule("20140802T001500Z", "FREQ=BOGUS;BYWEEKNO=1;COUNT=2");
        var result = vevent.Deserialize();
        Assert.False(result, "RRULE failed to parsed, FREQ type invalid");
    }


    [Fact]
    public void RRuleFailure02()
    {
        var vevent = CreateVEventWithRRule("20140802T001500Z", "BYWEEKNO=1;COUNT=2");
        var result = vevent.Deserialize();
        Assert.False(result, "RRULE failed to parsed; FREQ missing");
    }

    [Fact]
    public void RRuleFailure03()
    {
        var vevent = CreateVEventWithRRule("20140802T001500Z", "FREQ=DAILY;BYSECOND=1;BYMINUTE=1;BYYEARDAY=1;BYWEEKNO=1;COUNT=");
        var result = vevent.Deserialize();
        Assert.False(result, "RRULE failed to parsed, COUNT value missing");
    }

    [Fact]
    public void RRuleFailure04()
    {
        var vevent = CreateVEventWithRRule("20140802T001500Z", "FREQ=HOURLY;INTERVAL=;COUNT=12");
        var result = vevent.Deserialize();
        Assert.False(result, "RRULE failed to parsed, INTERVAL value missing");
    }

    [Fact]
    public void RRuleFailure05()
    {
        var vevent = CreateVEventWithRRule("20140802T001500Z", "FREQ=HOURLY;INTERVAL=3;COUNT=12;UNTIL=20210317T000000Z");
        var result = vevent.Deserialize();
        Assert.False(result, "RRULE failed to parsed, COUNT and UNTIL in same rrule");
    }

    [Fact]
    public void RRuleFailure06()
    {
        var vevent = CreateVEventWithRRule("20140802T001500Z", "FREQ=YEARLY;COUNT=8;INTERVAL=4;BYYEARDAY=390");
        var result = vevent.Deserialize();
        Assert.False(result, "RRULE failed to parsed, BYYEARDAY out of range (+/-366)");
    }

    [Fact]
    public void RRuleFailure07()
    {
        var vevent = CreateVEventWithRRule("20140802", "FREQ=DAILY;BYSECOND=1;BYMINUTE=1;BYYEARDAY=1;BYWEEKNO=1;COUNT=2");
        var result = vevent.Deserialize();
        Assert.False(result, "RRULE parsed properly, DTSTART must be VALUE=date");
    }

    [Fact]
    public void RRuleFailure09()
    {
        var vevent = CreateVEventWithRRule("20240101T120000", "FREQ=YEARLY;COUNT=10;BYYEARDAY=1,-1,BYDAY=MO", "Europe/Zurich");
        var result = vevent.Deserialize();
        Assert.False(result, "RRULE parsed properly, before BYDAY semicolon missing");
    }

    [Fact]
    public void RRuleFailure10()
    {
        var vevent = CreateVEventWithRRule("20240101T120000", "FREQ=YEARLY;COUNT=10;BYYEARDAY=1,-1,9999;BYDAY=MO", "Europe/Zurich");
        var result = vevent.Deserialize();
        Assert.False(result, "RRULE parsed properly, out of range BYYEARDAY");
    }

    [Fact]
    public void RRuleFailure11()
    {
        var vevent = CreateVEventWithRRule("20240101T120000", "FREQ=YEARLY;COUNT=10;BYYEARDAY=;BYDAY=MO", "Europe/Zurich");
        var result = vevent.Deserialize();
        Assert.False(result, "RRULE parsed properly, empty BYYEARDAY");
    }

    [Fact]
    public void RRuleFailure12()
    {
        var rrule = CompileVEventWithRRule("20140802", "FREQ=DAILY;BYSECOND=1;BYMINUTE=2;COUNT=2");
        Assert.NotNull(rrule);
        if (rrule is not null)
        {
            Assert.True(rrule.Frequency == FrequencyType.Daily && rrule.Interval == 1 && rrule.Count == 2, "RRULE properly read");
            Assert.Empty(rrule.BySecond);
            Assert.Empty(rrule.ByMinute);
            Assert.Empty(rrule.ByHour);
            Assert.Empty(rrule.ByDay);
            Assert.Empty(rrule.ByMonthDay);
            Assert.Empty(rrule.ByYearDay);
            Assert.Empty(rrule.ByWeekNo);
            Assert.Empty(rrule.ByMonth);
            Assert.Empty(rrule.BySetPosition);
        }
    }

    public static CaldavRecurrence? CompileVEventWithRRule(string dtStart, string recurrsionRule, string? tzId = null)
    {
        var vevent = CreateVEventWithRRule(dtStart, recurrsionRule, tzId);
        Assert.NotNull(vevent);
        var result = vevent.Deserialize();
        Assert.True(result, "RRULE parsed properly");
        return vevent.RecurrenceRule;
    }

    public static VEvent CreateVEventWithRRule(string dtStart, string recurrsionRule, string? tzId = null)
    {
        return dtStart.Contains('T') ? CreateVEventWithRRuleDateTime(dtStart, recurrsionRule, tzId) : CreateVEventWithRRuleDateOnly(dtStart, recurrsionRule);
    }

    private static VEvent CreateVEventWithRRuleDateTime(string dtStart, string recurrsionRule, string? tzId = null)
    {
        var builder = new CalendarBuilder();
        var vevent = new VEvent { Builder = builder, };
        var uidProperty = builder.BuildProperty(vevent, new Calendare.VSyntaxReader.CalendarObject(PropertyName.Uid, Guid.NewGuid().ToString(), []));
        Assert.NotNull(uidProperty);
        var dtStartProperty = builder.BuildProperty(vevent, new Calendare.VSyntaxReader.CalendarObject(PropertyName.DateStart, dtStart, tzId is null ? [] : [new("TZID", tzId)]));
        Assert.NotNull(dtStartProperty);
        var rruleProperty = builder.BuildProperty(vevent, new Calendare.VSyntaxReader.CalendarObject(PropertyName.RecurrenceRule, recurrsionRule, []));
        Assert.NotNull(rruleProperty);
        vevent.Properties.AddRange([uidProperty, dtStartProperty, rruleProperty]);
        return vevent;
    }

    private static VEvent CreateVEventWithRRuleDateOnly(string dtStart, string recurrsionRule)
    {
        var builder = new CalendarBuilder();
        var vevent = new VEvent { Builder = builder, };
        var uidProperty = builder.BuildProperty(vevent, new Calendare.VSyntaxReader.CalendarObject(PropertyName.Uid, Guid.NewGuid().ToString(), []));
        Assert.NotNull(uidProperty);
        var dtStartProperty = builder.BuildProperty(vevent, new Calendare.VSyntaxReader.CalendarObject(PropertyName.DateStart, dtStart, [new("VALUE", "DATE")]));
        Assert.NotNull(dtStartProperty);
        var rruleProperty = builder.BuildProperty(vevent, new Calendare.VSyntaxReader.CalendarObject(PropertyName.RecurrenceRule, recurrsionRule, []));
        Assert.NotNull(rruleProperty);
        vevent.Properties.AddRange([uidProperty, dtStartProperty, rruleProperty]);
        return vevent;
    }
}
