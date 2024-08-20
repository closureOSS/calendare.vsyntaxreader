namespace Calendare.VSyntaxReader.Properties;

public interface ICalAddressValue
{
    public CalendarObject Raw { get; }
}


public static class CalAddressExtensions
{
    public static string GetEmail(this ICalAddressValue property)
    {
        if (property.Raw.Value is not null)
        {
            string email = property.Raw.Value;
            if (email.StartsWith("mailto:", System.StringComparison.InvariantCultureIgnoreCase))
            {
                email = email["mailto:".Length..];
            }
            return email.ToLowerInvariant();
        }
        return string.Empty;
    }

    public static CalendarObject AmendEmail(this ICalAddressValue property, string? value)
    {
        var email = value ?? string.Empty;

        if (!email.StartsWith("mailto:", System.StringComparison.InvariantCultureIgnoreCase))
        {
            email = $"mailto:{email}";
        }
        return property.Raw with { Value = email.ToLowerInvariant() };
    }
}
