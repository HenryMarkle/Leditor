namespace Leditor.Data.Tiles.Exceptions;

/// <summary>
/// The exception that is thrown when a category could not be found on lookup
/// </summary>
public class TileCategoryNotFoundException : Exception
{
    public string Category { get; }
    public string Tile { get; set;}

    public TileCategoryNotFoundException(string category, string tile) : base($"Tile category \"{category}\" not found")
    {
        Category = category;
        Tile = tile;
    }

    public TileCategoryNotFoundException(string category, string tile, Exception? innerException = null) 
        : base($"Tile category \"{category}\" not found", innerException)
    {
        Category = category;
        Tile = tile;
    }
}