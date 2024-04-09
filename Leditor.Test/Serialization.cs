namespace Leditor.Test;

using Leditor.Serialization;
using System.IO;

public class Serialization
{
    [Fact]
    public void TileInitImportTest()
    {
        _ = TileImporter.ParseInit(Path.Combine("..", "..", "..", "assets", "tiles", "Init.txt"));
    }

    [Fact]
    public void PropInitImportTest()
    {
        _ = PropImporter.ParseInit(Path.Combine("..", "..", "..", "assets", "props", "Init.txt"));
    }
}