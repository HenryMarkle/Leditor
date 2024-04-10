namespace Leditor.Serialization.Exceptions;

public class InvalidFormatException : ParseException 
{
    public InvalidFormatException(string message) : base(message)
    {
        
    }

    public InvalidFormatException(string message, Exception? innerException) : base(message, innerException)
    {
        
    }
}