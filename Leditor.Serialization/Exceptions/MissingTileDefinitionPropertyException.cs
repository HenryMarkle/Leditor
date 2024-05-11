namespace Leditor.Serialization.Exceptions;

public class MissingTileDefinitionPropertyException : TileDefinitionParseException
{
    public string Property { get; }
    
    public MissingTileDefinitionPropertyException(string property) 
        : base($"Tile definition is missing \"{property}\" property")
    {
        Property = property;
    }
    
    public MissingTileDefinitionPropertyException(string property, Exception? innerException) 
        : base($"Tile definition is missing \"{property}\" property", innerException)
    {
        Property = property;
    }

    //

    public MissingTileDefinitionPropertyException(string tileName, string property) 
        : base($"Tile definition of \"{tileName}\" is missing \"{property}\" property")
    {
        Property = property;
    }

    public MissingTileDefinitionPropertyException(string tileName, string property, Exception? innerException) 
        : base($"Tile definition of \"{tileName}\" is missing \"{property}\" property", innerException)
    {
        Property = property;
    }
}