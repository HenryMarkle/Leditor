namespace Leditor.Serialization.Exceptions;

public class InvalidTileTypeException : ParseException
{
    public string Type { get; }

    public InvalidTileTypeException(string type) : base($"Invalid tile type \"{type}\"")
    {
        Type = type;
    }
    
    public InvalidTileTypeException(string type, Exception? innerException) 
        : base($"Invalid tile type \"{type}\"", innerException)
    {
        Type = type;
    }
}