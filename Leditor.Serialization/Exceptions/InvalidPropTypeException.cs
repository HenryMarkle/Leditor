namespace Leditor.Serialization.Exceptions;

public class InvalidPropTypeException : PropDefinitionParseException
{
    public string Type { get; init; }
    
    public InvalidPropTypeException(string type) : base($"Invalid prop definition type \"{type}\"")
    {
        Type = type;
    }
    
    public InvalidPropTypeException(string type, Exception? innerException) : base($"Invalid prop definition type \"{type}\"", innerException)
    {
        Type = type;
    }
}