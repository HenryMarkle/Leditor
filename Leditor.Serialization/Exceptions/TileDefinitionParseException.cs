namespace Leditor.Serialization.Exceptions;

public class TileDefinitionParseException : ParseException
{
    public TileDefinitionParseException(string message) : base(message) { }
    public TileDefinitionParseException(string message, Exception? innerException) 
        : base(message, innerException) { }
}