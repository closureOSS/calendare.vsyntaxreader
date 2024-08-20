using System.Collections.Generic;
using System.Text;
using NodaTime;
using NodaTime.Text;

namespace VSyntaxReader.Tests.Recurrence;

public record RRuleResult(int Count, List<string> Occurrences)
{
    public override string ToString()
    {
        return $"{Count} occurrences";
    }
}

public record RRuleUsecase(string ReferenceDate, string RecurrenceRule, string? TzId, RRuleResult Expected)
{
    public override string ToString()
    {
        return $"{RecurrenceRule} for {ReferenceDate} {TzId}";
    }
}


public static class RecurrenceTestHelper
{
    public static string RecordExpected(this List<ZonedDateTime>? result, RRuleUsecase usecase)
    {
        var pattern = ZonedDateTimePattern.CreateWithInvariantCulture("G", DateTimeZoneProviders.Tzdb);
        var sb = new StringBuilder();
        foreach (var recurrence in result ?? [])
        {
            sb.AppendLine($"\"{pattern.Format(recurrence)}\",");
            // var zz = pattern.Format(recurrence);
            // var yy = pattern.Parse(zz);
        }
        var ddd = $"{{ new RRuleUsecase(\"{usecase.ReferenceDate}\", \"{usecase.RecurrenceRule}\", {(usecase.TzId is null ? "null" : $"\"{usecase.TzId}\"")}, new({result?.Count ?? 0}, [\n{sb.ToString()}]))}},";
        return ddd;
    }

    public static (int Count, List<ZonedDateTime> Occurrences) Expected(this RRuleUsecase usecase)
    {
        var pattern = ZonedDateTimePattern.CreateWithInvariantCulture("G", DateTimeZoneProviders.Tzdb);
        var result = new List<ZonedDateTime>();
        foreach (var dtString in usecase.Expected.Occurrences)
        {
            var dt = pattern.Parse(dtString);
            if (dt.Success)
            {
                result.Add(dt.Value);
            }
        }
        return (usecase.Expected.Count, result);
    }
}
