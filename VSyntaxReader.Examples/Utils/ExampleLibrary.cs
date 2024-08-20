using System;
using System.IO;

namespace Calendare.VSyntaxReader.Examples.Utils;

public class ExampleRunner
{
    private readonly string LibraryPath;

    public ExampleRunner()
    {
        LibraryPath = Path.Combine(AppContext.BaseDirectory, "ics");
    }

    public string Combine(string filename)
    {
        return Path.Combine(LibraryPath, filename);
    }

    public void Execute(string comment, Action<ExampleRunner> fn)
    {
        Console.WriteLine($"\n--- START of {comment} ---");
        fn(this);
        Console.WriteLine($"--- END of {comment} ---\n");
    }
}
