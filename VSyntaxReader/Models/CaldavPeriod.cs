using System;
using Calendare.VSyntaxReader.Properties;
using NodaTime;

namespace Calendare.VSyntaxReader.Models;

public record CaldavPeriod(CaldavDateTime Start, CaldavDateTime? End = null, Period? Period = null)
{
    public ValueDataTypes DataType => Period is not null ? ValueDataTypes.Period : Start.DataType;
    public DateTimeZone? Zone => Start?.Zone;

    public ZonedDateTime GetEndInZone(DateTimeZone referenceTimezone)
    {
        var sop = Start.GetNormalizedInZone(referenceTimezone) ?? throw new NullReferenceException(nameof(Start));
        if (Period is not null)
        {
            return CaldavLengthOfTimeExtensions.Plus(sop, Period);
        }
        if (End is not null)
        {
            var eop = End.GetNormalizedInZone(referenceTimezone);
            if (eop is not null)
            {
                return eop.Value;
            }
        }
        throw new Exception("End or Period must be not null");
    }
}
