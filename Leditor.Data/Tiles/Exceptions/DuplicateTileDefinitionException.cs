namespace Leditor.Data.Tiles.Exceptions;

/// <summary>
/// The exception that is thrown when attempting to define more than one tile with the same name
/// </summary>
public class DuplicateTileDefinitionException : Exception
{
    public string Name { get; }

    public DuplicateTileDefinitionException(string name) : base($"Tile \"{name}\" is already defined")
    {
        Name = name;
    }
    
    public DuplicateTileDefinitionException(string name, Exception? innerException) 
        : base($"Tile \"{name}\" is already defined", innerException)
    {
        Name = name;
    }
}
