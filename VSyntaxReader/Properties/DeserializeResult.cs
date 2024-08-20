namespace Calendare.VSyntaxReader.Properties;

public record DeserializeResult(bool Success, string? ErrorMessage = null)
{
    public static implicit operator bool(DeserializeResult dr) => dr.Success;
}
