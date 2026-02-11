namespace Calendare.VSyntaxReader;

sealed class Line
{
    public required string Raw { get; set; }
    public int LineNo { get; set; }
    public int? PhysicalLineNo { get; set; }
    public int? PhysicalLineCount { get; set; }
}
