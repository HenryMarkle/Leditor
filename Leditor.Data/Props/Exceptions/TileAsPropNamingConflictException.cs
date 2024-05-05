namespace Leditor.Data.Props.Exceptions;

/// <summary>
/// Is thrown when trying to register a tile as a prop that shares a name with a pre-existing tile.
/// </summary>
public class TileAsPropNamingConflictException : Exception {
    public string Name { get; init; }

    public TileAsPropNamingConflictException(string name) : base($"A tile-as-prop has the same name as an existing prop \"{name}\"")
    {
        Name = name;
    }

    public TileAsPropNamingConflictException(string name, Exception? innerException) : base($"A tile-as-prop has the same name as an existing prop \"{name}\"", innerException)
    {
        Name = name;
    }
}