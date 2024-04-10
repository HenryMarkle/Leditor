namespace Leditor.Serialization.Exceptions;

/// <summary>
/// Thrown when running into a problem while parsing a prop from the props list
/// </summary>
public class PropParseException : ParseException
{
    public PropParseException(string message) : base(message)
    {
        
    }

    public PropParseException(string message, Exception? innerException) : base(message, innerException)
    {

    }
}