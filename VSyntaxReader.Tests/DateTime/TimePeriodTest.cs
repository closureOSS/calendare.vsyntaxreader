using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Parsers;
using NodaTime;
using Xunit.Abstractions;

namespace VSyntaxReader.Tests.DateTime;

public class TimePeriodTest
{
    private readonly CalendarBuilder builder = new();
    private readonly ITestOutputHelper Output;

    public TimePeriodTest(ITestOutputHelper output)
    {
        Output = output;
    }

    [Fact]
    public void BaseCaldavDurationTest()
    {
        var one = new CaldavLengthOfTime(Period.FromDays(2), Duration.Zero);
        var two = new CaldavLengthOfTime(Period.FromHours(48), Duration.Zero);
        var three = new CaldavLengthOfTime(null, Duration.FromDays(2));
        var equalOneTwo = one.IsSameTimeLength(two);
        Output.WriteLine($"{one} <=> {two} -> {equalOneTwo}");
        Assert.True(equalOneTwo);
        var equalOneThree = one.IsSameTimeLength(three);
        Output.WriteLine($"{one} <=> {three} -> {equalOneThree}");
        Assert.False(equalOneThree);
    }

    [Fact]
    public void CaldavDurationAddTest()
    {
        Assert.True(TimezoneParser.TryReadTimezone("Europe/Zurich", out var timeZone));
        Assert.NotNull(timeZone);
        var sop = new LocalDateTime(2024, 10, 26, 23, 0, 0).InZoneLeniently(timeZone);

        var sopPD1 = sop.Plus(new CaldavLengthOfTime(Period.FromDays(1), Duration.Zero));
        var sopPH24 = sop.Plus(new CaldavLengthOfTime(Period.FromHours(24), Duration.Zero));
        var sopPH5 = sop.Plus(new CaldavLengthOfTime(Period.FromHours(5), Duration.Zero));
        var sopDH5 = sop.Plus(new CaldavLengthOfTime(null, Duration.FromHours(5)));
        var sopDD1 = sop.Plus(new CaldavLengthOfTime(null, Duration.FromDays(1)));
        var sopDH24 = sop.Plus(new CaldavLengthOfTime(null, Duration.FromHours(24)));
        var pb = new PeriodBuilder
        {
            Days = 1,
            Hours = 5
        };
        var sopPD1H5 = sop.Plus(new CaldavLengthOfTime(pb.Build(), Duration.Zero));

        var ref1 = new LocalDateTime(2024, 10, 27, 23, 0, 0).InZoneLeniently(timeZone);
        var ref2 = new LocalDateTime(2024, 10, 27, 22, 0, 0).InZoneLeniently(timeZone);
        var ref3 = new LocalDateTime(2024, 10, 27, 3, 0, 0).InZoneLeniently(timeZone);
        var ref4 = new LocalDateTime(2024, 10, 28, 4, 0, 0).InZoneLeniently(timeZone);
        Assert.Equal(sopPD1, ref1);
        Assert.Equal(sopPH24, ref2);
        Assert.Equal(sopPH5, ref3);
        Assert.Equal(sopDH5, ref3);
        Assert.Equal(sopDD1, ref2);
        Assert.Equal(sopDH24, ref2);
        Assert.Equal(sopPD1H5, ref4);
    }
}
