namespace Calendare.VSyntaxReader.Properties;
/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.2.20
/// </summary>
public enum ValueDataTypes
{
    Binary,
    Boolean,
    CalAddress,
    Date,
    DateTime,
    Duration,
    Float,
    Integer,
    Period,
    Recur,
    Text,
    Time,
    Uri,
    UtcOffset,
    // placeholders
    XNameValueType,
    OtherValueType,
}

public static class ValueDataTypeNames
{
    public const string Binary = "BINARY";
    public const string Boolean = "BOOLEAN";
    public const string CalAddress = "CAL-ADDRESS";
    public const string Date = "DATE";
    public const string DateTime = "DATE-TIME";
    public const string Duration = "DURATION";
    public const string Float = "FLOAT";
    public const string Integer = "INTEGER";
    public const string Period = "PERIOD";
    public const string Recur = "RECUR";
    public const string Text = "TEXT";
    public const string Time = "TIME";
    public const string Uri = "URI";
    public const string UtcOffset = "UTC-OFFSET";

}

public static class ValueDataTypeParser
{
    public static ValueDataTypes Parse(string dataType)
    {
        return dataType.ToUpperInvariant() switch
        {
            ValueDataTypeNames.Binary => ValueDataTypes.Binary,
            ValueDataTypeNames.Boolean => ValueDataTypes.Boolean,
            ValueDataTypeNames.CalAddress => ValueDataTypes.CalAddress,
            ValueDataTypeNames.Date => ValueDataTypes.Date,
            ValueDataTypeNames.DateTime => ValueDataTypes.DateTime,
            ValueDataTypeNames.Duration => ValueDataTypes.Duration,
            ValueDataTypeNames.Float => ValueDataTypes.Float,
            ValueDataTypeNames.Integer => ValueDataTypes.Integer,
            ValueDataTypeNames.Period => ValueDataTypes.Period,
            ValueDataTypeNames.Recur => ValueDataTypes.Recur,
            ValueDataTypeNames.Text => ValueDataTypes.Text,
            ValueDataTypeNames.Time => ValueDataTypes.Time,
            ValueDataTypeNames.Uri => ValueDataTypes.Uri,
            ValueDataTypeNames.UtcOffset => ValueDataTypes.UtcOffset,
            _ => dataType.StartsWith("X-", System.StringComparison.InvariantCultureIgnoreCase) ? ValueDataTypes.XNameValueType : ValueDataTypes.OtherValueType,
        };
    }
}
