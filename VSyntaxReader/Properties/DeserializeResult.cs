namespace Calendare.VSyntaxReader.Properties;

public enum DeserializeErrorCategory
{
    Syntax,
    NoContent,
    WrongFormat,
}

public record DeserializeResult(bool Success, string? ErrorMessage = null, DeserializeErrorCategory ErrorCategory = DeserializeErrorCategory.Syntax)
{
    public static implicit operator bool(DeserializeResult dr) => dr.Success;
}
