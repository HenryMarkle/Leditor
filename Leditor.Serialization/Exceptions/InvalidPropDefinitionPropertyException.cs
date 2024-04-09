namespace Leditor.Serialization.Exceptions;

/// <summary>
/// Is thrown when receiving a prop definition property with invalid data or invalid type 
/// </summary>
public class InvalidPropDefinitionPropertyException : PropDefinitionParseException
{
    public string Property { get; init; }
    
    public InvalidPropDefinitionPropertyException(string property) : base($"Prop definition property \"{property}\" holds invalid value")
    {
        Property = property;
    }

    public InvalidPropDefinitionPropertyException(string property, Exception? innerException) : base($"Prop definition property \"{property}\" holds invalid value", innerException)
    {
        Property = property;
    }
}