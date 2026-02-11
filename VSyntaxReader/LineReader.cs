using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Calendare.VSyntaxReader;

sealed class LineReader
{
    private readonly Stream Stream;
    private readonly int BufferSize = 256;
    public int PhysicalLineNo { get; private set; }
    public int LineNo { get; private set; }

    public LineReader(Stream stream)
    {
        Stream = stream;
    }

    public IEnumerable<Line> ReadLine()
    {
        using var streamReader = new StreamReader(Stream, Encoding.UTF8, true, BufferSize);
        string? physicalLine;
        Line? line = null;
        while ((physicalLine = streamReader.ReadLine()) is not null)
        {
            var isEmpty = string.IsNullOrEmpty(physicalLine);
            var isContinuation = !isEmpty && (physicalLine.StartsWith(' ') || physicalLine.StartsWith('\t') || physicalLine.StartsWith("\\n"));
            var continuationLength = !isEmpty && physicalLine.StartsWith("\\n") ? 2 : 1;
            if (isContinuation)
            {
                if (line is null || isEmpty)
                {
                    throw new FileLoadException("Malformed");
                }
                line.PhysicalLineCount++;
                line.Raw += physicalLine[continuationLength..];
            }
            else
            {
                if (isEmpty)
                {
                    PhysicalLineNo++;
                    continue;
                }
                if (line is not null)
                {
                    yield return line;
                    line = null;
                    LineNo++;
                }
                line = new Line
                {
                    LineNo = LineNo,
                    PhysicalLineNo = PhysicalLineNo,
                    PhysicalLineCount = 1,
                    Raw = physicalLine,
                };
            }
            PhysicalLineNo++;
        }
        if (line is not null)
        {
            yield return line;
        }
    }

    public void Close()
    {
        Stream.Close();
    }
}
