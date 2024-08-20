using NodaTime;

namespace Calendare.VSyntaxReader.Models;
/// <summary>
/// CaldavLengthOfTime contains either a NodaTime Period (human length of time)
/// or NodaTime Duration (calendar independent length of time)
///
/// If Period is not null, Period is set otherwise Duration
///
/// An empty NodaTime Duration is Duration.Zero
/// </summary>
/// <param name="Period"></param>
/// <param name="Duration"></param>
public record CaldavLengthOfTime(Period? Period, Duration Duration)
{
    public override string ToString()
    {
        return Period is null ? Duration.ToString() : Period.ToString();
    }

    public bool IsSameTimeLength(CaldavLengthOfTime? other)
    {
        if (Period is not null)
        {
            var left = Period.Normalize();
            if (other is null || other.Period is null)
            {
                return false;
            }
            var right = other.Period.Normalize();
            return left.Equals(right);
        }
        return Duration.Equals(other!.Duration);
    }
}
