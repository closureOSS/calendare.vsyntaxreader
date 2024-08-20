using System.Collections.Generic;

namespace Calendare.VSyntaxReader;

public record CalendarObject(string Name, string? Value, List<CalendarObjectParameter> Parameters)
{
    public string? Group { get; init; }
}
