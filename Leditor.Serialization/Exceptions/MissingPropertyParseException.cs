namespace Leditor.Serialization.Exceptions;

public class MissingPropertyParseException : ParseException
{
    public string Property { get; }
    
    public MissingPropertyParseException(string property) : base($"Missing property \"{property}\" while parsing")
    {
        Property = property;
    }
    
    public MissingPropertyParseException(string property, Exception? innerException) 
        : base($"Missing property \"{property}\" while parsing", innerException)
    {
        Property = property;
    }
}