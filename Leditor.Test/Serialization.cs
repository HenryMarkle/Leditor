namespace Leditor.Test;

using Leditor.Serialization;
using Leditor.Serialization.Exceptions;
using System.IO;

public class Serialization
{
    private string ExecutableDirectory => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) 
        ?? throw new NullReferenceException("Failed to get executable's directory");

    [Fact]
    public void TileInitImportTest()
    {
        _  = TileImporter.ParseInit(Path.Combine(ExecutableDirectory, "..", "..", "..", "assets", "tiles", "Init.txt"));
        // try {
        // } catch (ParseException e) {
        //     throw new Exception($"Parsing tile init failed: {e.Message}");
        // }
    }

    [Fact]
    public void PropInitImportTest()
    {
        _ = PropImporter.ParseInit(Path.Combine(ExecutableDirectory, "..", "..", "..", "assets", "props", "Init.txt"));
    }

    [Fact]
    public void PropImportTest()
    {
        
    }
}