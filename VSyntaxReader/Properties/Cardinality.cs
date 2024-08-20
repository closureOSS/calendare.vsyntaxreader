namespace Calendare.VSyntaxReader.Properties;

public enum Cardinality
{
    /// <summary>
    /// Forbidden
    /// </summary>
    Zero,

    /// <summary>
    /// Required, but MOST NOT occur more than once
    /// </summary>
    One,

    /// <summary>
    /// Optional, but MUST NOT occur more than once
    /// </summary>
    ZeroOrOne,

    /// <summary>
    /// Required, MAY appear more than once
    /// </summary>
    OneOrMany,

    /// <summary>
    /// Optional, MAY appear more than once
    /// </summary>
    Many,

    /// <summary>
    /// Custom or X property with undefined cardinality
    /// </summary>
    Undefined
}
