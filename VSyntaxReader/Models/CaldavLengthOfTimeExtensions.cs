using NodaTime;

namespace Calendare.VSyntaxReader.Models;

public static class CaldavLengthOfTimeExtensions
{
    public static ZonedDateTime Plus(this ZonedDateTime sop, CaldavLengthOfTime lengthOfTime)
    {
        if (lengthOfTime.Period is null)
        {
            return sop.Plus(lengthOfTime.Duration);
        }
        else
        {
            return Plus(sop, lengthOfTime.Period);
        }
    }

    public static ZonedDateTime Plus(ZonedDateTime sop, Period period)
    {
        var eop = sop;
        if (period.HasDateComponent)
        {
            sop.Deconstruct(out var sopLdt, out var sopTimeZone, out var sopOffset);
            var dateBuilder = new PeriodBuilder(period);
            dateBuilder.Hours = dateBuilder.Minutes = dateBuilder.Seconds = dateBuilder.Milliseconds = dateBuilder.Nanoseconds = 0;
            var datePeriod = dateBuilder.Build();
            var eopLdt = sopLdt.Plus(datePeriod);
            eop = eopLdt.InZoneLeniently(sopTimeZone);
        }
        if (period.HasTimeComponent)
        {
            var timeBuilder = new PeriodBuilder(period);
            timeBuilder.Days = timeBuilder.Weeks = timeBuilder.Months = 0;
            var timePeriod = timeBuilder.Build().ToDuration();
            return eop.Plus(timePeriod);
        }
        return eop;
    }
}
