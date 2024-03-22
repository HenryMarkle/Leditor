namespace Leditor.Data.Tiles.Exceptions;

/// <summary>
/// The exception that is thrown when attempting to register a category that is already registered
/// </summary>
public class DuplicateTileCategoryException : Exception
{
    public string Category { get; }

    public DuplicateTileCategoryException(string category) : base($"Category \"{category}\" already registered")
    {
        Category = category;
    }
    
    public DuplicateTileCategoryException(string category, Exception? innerException) 
        : base($"Category \"{category}\" already registered", innerException)
    {
        Category = category;
    }
}