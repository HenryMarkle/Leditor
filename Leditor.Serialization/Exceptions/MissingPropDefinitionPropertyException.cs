namespace Leditor.Serialization.Exceptions;

public class MissingPropDefinitionPropertyException : PropDefinitionParseException
{
    public string Property { get; init; }
    
    public MissingPropDefinitionPropertyException(string property) : base($"Missing prop definition property: {property}")
    {
        Property = property;
    }
    
    
    public MissingPropDefinitionPropertyException(string property, Exception? innerException) : base($"Missing prop definition property: {property}", innerException)
    {
        Property = property;
    }
}