namespace Leditor.Data.Props.Exceptions;

/// <summary>
/// The exception that is thrown when attempting to define more than one prop with the same name
/// </summary>
public class DuplicatePropDefinitionException : Exception
{
    public string Name { get; }

    public DuplicatePropDefinitionException(string name) : base($"Prop \"{name}\" is already defined")
    {
        Name = name;
    }
    
    public DuplicatePropDefinitionException(string name, Exception? innerException) 
        : base($"Prop \"{name}\" is already defined", innerException)
    {
        Name = name;
    }
}