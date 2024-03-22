namespace Leditor.RL.Exceptions;

public class TextureLoadException : Exception
{
    public TextureLoadException() : base("Failed to load texture")
    {
        
    }

    public TextureLoadException(string message) : base("Failed to load texture: "+message)
    {
        
    }

    public TextureLoadException(string message, Exception? innerException) 
        : base("Failed to load texture: "+message, innerException)
    {

    }
}