namespace Calendare.VSyntaxReader.Models;

public record CaldavText(string Value, string? Language = null)
{
    public static implicit operator string(CaldavText ts) => ts.Value;
}
