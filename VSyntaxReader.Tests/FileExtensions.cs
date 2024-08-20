using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace VSyntaxReader.Tests;

public static class FileExtensions
{
    public static string ReadFileAsString(string file, [CallerFilePath] string filePath = "")
    {
        var directoryPath = Path.GetDirectoryName(filePath);
        var fullPath = Path.Join(directoryPath, "data", file);
        if (File.Exists(fullPath))
        {
            return File.ReadAllText(fullPath);
        }
        return string.Empty;
    }

    public static IEnumerable<string> ReadFileAllLines(string file, [CallerFilePath] string filePath = "")
    {
        var directoryPath = Path.GetDirectoryName(filePath);
        var fullPath = Path.Join(directoryPath, "data", file);
        if (File.Exists(fullPath))
        {
            return File.ReadAllLines(fullPath);
        }
        return [];
    }
    public static void WriteFileAsString(string content, string file, [CallerFilePath] string filePath = "")
    {
        var directoryPath = Path.GetDirectoryName(filePath);
        var fullPath = Path.Join(directoryPath, "data", file);
        File.WriteAllText(fullPath, content);
    }


    public static string BuildSourceFilename(string file, [CallerFilePath] string filePath = "")
    {
        var directoryPath = Path.GetDirectoryName(filePath);
        var fullPath = Path.Join(directoryPath, "data", file);
        return fullPath;
    }
}
