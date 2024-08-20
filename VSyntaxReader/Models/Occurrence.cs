using Calendare.VSyntaxReader.Components;
using NodaTime;

namespace Calendare.VSyntaxReader.Models;

public record Occurrence(Interval Interval, RecurringComponent Source, bool IsReccurring = true, bool? IsSynthetic = null);
