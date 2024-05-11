namespace Leditor.Serialization.Exceptions;

public class TileDefinitionParseException : ParseException
{
    public TileDefinitionParseException(string message) : base(message) { }
    public TileDefinitionParseException(string message, Exception? innerException) 
        : base(message, innerException) { }

    public TileDefinitionParseException(string tileName, string message) : base($"Failed to parse tile \"{tileName}\": {message}")
    { }
    
    public TileDefinitionParseException(string tileName, string message, Exception? innerException) : base($"Failed to parse tile \"{tileName}\": {message}", innerException)
    { }
}