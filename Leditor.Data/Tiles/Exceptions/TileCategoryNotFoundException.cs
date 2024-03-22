namespace Leditor.Data.Tiles.Exceptions;

/// <summary>
/// The exception that is thrown when a category could not be found on lookup
/// </summary>
public class TileCategoryNotFoundException : Exception
{
    public string Category { get; }

    public TileCategoryNotFoundException(string category) : base($"Tile category \"{category}\" not found")
    {
        Category = category;
    }

    public TileCategoryNotFoundException(string category, Exception? innerException) 
        : base($"Tile category \"{category}\" not found", innerException)
    {
        Category = category;
    }
}