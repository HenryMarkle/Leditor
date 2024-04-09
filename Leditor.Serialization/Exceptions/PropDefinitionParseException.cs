namespace Leditor.Serialization.Exceptions;

public class PropDefinitionParseException : ParseException
{
    public PropDefinitionParseException(string message) : base(message) { }
    public PropDefinitionParseException(string message, Exception? innerException) 
        : base(message, innerException) { }
}