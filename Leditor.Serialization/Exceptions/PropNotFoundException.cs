namespace Leditor.Serialization.Exceptions;

public class PropNotFoundException : PropParseException
{
    public string PropName { get; }
    
    public PropNotFoundException(string name) : base($"Prop \"{name}\" not found in the dex")
    {
        PropName = name;
    }
    
    public PropNotFoundException(string name, Exception? innerException) : base($"Prop \"{name}\" not found in the dex", innerException)
    {
        PropName = name;
    }
}