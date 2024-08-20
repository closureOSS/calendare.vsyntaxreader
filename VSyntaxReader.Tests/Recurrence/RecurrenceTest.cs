using System.Collections.Generic;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Operations;
using NodaTime;
using Xunit.Abstractions;

namespace VSyntaxReader.Tests.Recurrence;

public class RecurrenceTest
{
    private readonly ITestOutputHelper Output;

    public RecurrenceTest(ITestOutputHelper output)
    {
        Output = output;
    }

    // [Theory]
    // //
    // // [InlineData("20241231T103045Z", "FREQ=MINUTELY;INTERVAL=3;COUNT=10")]
    // // [InlineData("20241231T103045Z", "FREQ=MINUTELY;INTERVAL=10;BYSECOND=22,44;UNTIL=20241231T113044Z")]
    // //
    // // [InlineData("20241231T103045Z", "FREQ=HOURLY;INTERVAL=3;COUNT=12")]
    // // [InlineData("20241231T103045Z", "FREQ=HOURLY;INTERVAL=3;BYMINUTE=0,15,30,45;COUNT=10")]
    // //
    // // [InlineData("20230326T000000Z", "FREQ=DAILY;INTERVAL=3;UNTIL=20240125T000000Z")]
    // // [InlineData("20230326T000000", "FREQ=DAILY;INTERVAL=7;BYHOUR=0,1,2,23;BYMINUTE=30;UNTIL=20230430T020000", "Europe/Zurich")]
    // // [InlineData("20240330T120000", "FREQ=DAILY;BYHOUR=0,1,2,3,4,23;BYMINUTE=30;UNTIL=20240402T050000", "Europe/Zurich")]
    // //
    // //[InlineData("20140802", "FREQ=DAILY;BYSECOND=1;BYMINUTE=1;COUNT=2")]
    // // [InlineData("20241027T025905", "FREQ=SECONDLY;INTERVAL=10;COUNT=12", "Europe/Zurich")]  // GOES BACK IN TIME AT THE END OF DAYLIGHT SAVING
    // public void PrepareRule(string dtStart, string recurrsionRule, string? tzId = null)
    // {
    //     var result = TestRRuleEvaluation(dtStart, recurrsionRule, tzId);
    //     var candidateExpected = result.RecordExpected(new RRuleUsecase(dtStart, recurrsionRule, tzId, new(-1, [])));
    //     Output.WriteLine(candidateExpected);
    // }

    // Use to transform to class in old style tests...
    //     var candidateExpected = result.RecordExpected(new RRuleUsecase(dtStart, recurrsionRule, tzId, new(-1, [])));
    //     Output.WriteLine(candidateExpected);
    [Theory]
    [MemberData(nameof(RecurrenceTestLibrary.WeeklyTestdata), MemberType = typeof(RecurrenceTestLibrary))]
    public void Weekly(RRuleUsecase usecase)
    {
        var result = TestRRuleEvaluation(usecase.ReferenceDate, usecase.RecurrenceRule, usecase.TzId);
        if (usecase.Expected.Count == -1)
        {
            var candidateExpected = result.RecordExpected(usecase);
            Output.WriteLine(candidateExpected);
            return;
        }
        Assert.NotNull(result);
        var (Count, Occurrences) = usecase.Expected();
        Assert.Equal(result, Occurrences);
    }

    [Theory]
    [MemberData(nameof(RecurrenceTestLibrary.MonthlyTestdata), MemberType = typeof(RecurrenceTestLibrary))]
    public void Monthly(RRuleUsecase usecase)
    {
        var result = TestRRuleEvaluation(usecase.ReferenceDate, usecase.RecurrenceRule, usecase.TzId);
        if (usecase.Expected.Count == -1)
        {
            var candidateExpected = result.RecordExpected(usecase);
            Output.WriteLine(candidateExpected);
            return;
        }
        Assert.NotNull(result);
        var (Count, Occurrences) = usecase.Expected();
        Assert.Equal(result, Occurrences);
    }

    [Theory]
    [MemberData(nameof(RecurrenceTestLibrary.YearlyTestdata), MemberType = typeof(RecurrenceTestLibrary))]
    public void Yearly(RRuleUsecase usecase)
    {
        var result = TestRRuleEvaluation(usecase.ReferenceDate, usecase.RecurrenceRule, usecase.TzId);
        if (usecase.Expected.Count == -1)
        {
            var candidateExpected = result.RecordExpected(usecase);
            Output.WriteLine(candidateExpected);
            return;
        }
        Assert.NotNull(result);
        var (Count, Occurrences) = usecase.Expected();
        Assert.Equal(result, Occurrences);
    }

    [Theory]
    [MemberData(nameof(RecurrenceTestLibrary.DailyTestdata), MemberType = typeof(RecurrenceTestLibrary))]
    [MemberData(nameof(RecurrenceTestLibrary.HourlyTestdata), MemberType = typeof(RecurrenceTestLibrary))]
    [MemberData(nameof(RecurrenceTestLibrary.MinutelyTestdata), MemberType = typeof(RecurrenceTestLibrary))]
    [MemberData(nameof(RecurrenceTestLibrary.SecondlyTestdata), MemberType = typeof(RecurrenceTestLibrary))]
    public void OtherFrequencies(RRuleUsecase usecase)
    {
        var result = TestRRuleEvaluation(usecase.ReferenceDate, usecase.RecurrenceRule, usecase.TzId);
        if (usecase.Expected.Count == -1)
        {
            var candidateExpected = result.RecordExpected(usecase);
            Output.WriteLine(candidateExpected);
            return;
        }
        Assert.NotNull(result);
        var (Count, Occurrences) = usecase.Expected();
        Assert.Equal(result, Occurrences);
    }

    public static TheoryData<RRuleUsecase> WorkInProgressTest() => new()
    {
    // ==> (1997 9:00 AM EDT) September 1,3,5,15,17,19,29;
    //                           October 1,3,13,15,17
    //        (1997 9:00 AM EST) October 27,29,31;
    //                           November 10,12,14,24,26,28;
        { new RRuleUsecase("19970901T090000", "FREQ=WEEKLY;INTERVAL=2;UNTIL=19971224T000000Z;WKST=SU;BYDAY=MO,WE,FR", "America/New_York", new(-1, []))},
    };

    [Theory]
    [MemberData(nameof(RecurrenceTest.WorkInProgressTest), MemberType = typeof(RecurrenceTest))]
    public void WorkInProgress(RRuleUsecase usecase)
    {
        var result = TestRRuleEvaluation(usecase.ReferenceDate, usecase.RecurrenceRule, usecase.TzId);
        if (usecase.Expected.Count == -1)
        {
            var candidateExpected = result.RecordExpected(usecase);
            Output.WriteLine(candidateExpected);
            return;
        }
        Assert.NotNull(result);
        var (Count, Occurrences) = usecase.Expected();
        Assert.Equal(result, Occurrences);
    }

    //
    // Helpers
    //
    private List<ZonedDateTime>? TestRRuleEvaluation(string dtStart, string recurrsionRule, string? tzId = null)
    {
        var rrule = BuildRecurrenceRule(dtStart, recurrsionRule, tzId);
        Assert.NotNull(rrule);
        var referenceDate = rrule.Reference?.ToInstant() ?? SystemClock.Instance.GetCurrentInstant();
        var result = GetEvaluation(rrule, referenceDate);
        Assert.True(result is not null, $"RRULE evaluated {dtStart} {recurrsionRule}");
        var rruleString = rrule.ToString();
        var checkRule = BuildRecurrenceRule(dtStart, rruleString, tzId);
        Assert.True(rrule.Equals(checkRule), $"Reparsed RRULE {rruleString} not the same as original RRULE {recurrsionRule}");
        Output.WriteLine($"{recurrsionRule} {rrule.Equals(checkRule)} {rrule}");
        return result;
    }

    private static List<ZonedDateTime> GetEvaluation(CaldavRecurrence rrule, Instant referenceDate, int rangeInDays = 20 * 365)
    {
        var calculator = new RecurrenceCalculator(rrule);
        Assert.NotNull(calculator);
        var interval = new Interval(referenceDate, referenceDate.Plus(Duration.FromDays(rangeInDays)));
        var result = calculator.Evaluate(interval);
        return result;
    }

    private static CaldavRecurrence? BuildRecurrenceRule(string dtStart, string recurrsionRule, string? tzId)
    {
        var vevent = RRuleParsingTest.CreateVEventWithRRule(dtStart, recurrsionRule, tzId);
        vevent.Deserialize();
        Assert.True(vevent.RecurrenceRule != null, $"RRULE parsed {dtStart} {recurrsionRule}");
        return vevent.RecurrenceRule;
    }

}
