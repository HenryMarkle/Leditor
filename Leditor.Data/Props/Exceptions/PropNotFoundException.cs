namespace Leditor.Data.Props.Exceptions;

/// <summary>
/// The exception that is thrown when attempting to lookup a prop definition by name and was not found.
/// </summary>
public class PropNotFoundException : Exception
{
    public string PropName { get; }
    
    public PropNotFoundException(string prop) : base(message: $"Prop with name {prop} not found")
    {
        PropName = prop;
    }
    
    public PropNotFoundException(string prop, Exception? innerException) 
        : base(message: $"Prop with name {prop} not found", innerException)
    {
        PropName = prop;
    }
}