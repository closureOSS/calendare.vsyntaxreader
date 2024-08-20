using NodaTime;

namespace Calendare.VSyntaxReader.Models;

public record CaldavPeriodUtc(Instant Start, Instant? End = null, Period? Period = null);
