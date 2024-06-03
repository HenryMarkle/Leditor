namespace Leditor.Serialization;

#nullable enable

public abstract class ParseException : Exception {
    public ParseException(string message) : base(message) {}
    public ParseException(string message, Exception? innerException) : base(message, innerException) {}
}

public class MaterialParseException(string message, string init) : ParseException(message)
{
    public string Init { get; set; } = init;
}

public class EffectParseException(string message, string effect) : Exception(message)
{
    public string Effect { get; set; } = effect;
}

public class MissingEffectOptionException(string effect, string option) : EffectParseException("Missing effect option", effect)
{
    public string Option { get; set; } = option;
}

public class InvalidEffectOptionValueException(string effect, string option, string value)
    : MissingEffectOptionException(effect, option)
{
    public string Value { get; set; } = value;
}

public class PropParseException : ParseException {
    public PropParseException(string message) : base(message) {}
    public PropParseException(string message, Exception? innerException) : base(message, innerException) {}
}

public class PropNotFoundException : PropParseException
{
    public string Name { get; init; }

    public PropNotFoundException(string name) : base($"Prop not found: \"{name}\"") {
        Name = name;
    }

    public PropNotFoundException(string name, Exception? innerException) : base($"Prop not found: \"{name}\"", innerException) {
        Name = name;
    }
}

public class InitParseException : ParseException
{
    public string Init { get; init; }

    public InitParseException(string message, string init) : base(message) { Init = init; }
    public InitParseException(string message, string init, Exception? innerException) : base(message, innerException) { Init = init; }
}

public class MissingInitPropertyException(string message, string init, string property) : InitParseException(message, init)
{
    public string Property { get; init; } = property;


}

public class InvalidInitPropertyValueException(string message, string init, string key, string value) : InitParseException(message, init)
{
    public string Key { get; init; } = key;
    public string Value { get; init; } = value;
}

public class PropInitParseException : InitParseException {
    public PropInitParseException(string message, string init) : base($"Failed to parse prop init: {message}", init) {

    }

    public PropInitParseException(string message, string init, Exception? innerException) : base($"Failed to parse prop init: {message}", init, innerException) {

    }
}

public class InvalidPropTypeException : PropInitParseException
{
    public string Type { get; init; }

    public InvalidPropTypeException(string typeName, string init) : base ($"Invalid prop type \"{typeName}\".", init)
    {
        Type = typeName;
    }

    public InvalidPropTypeException(string typeName, string init, Exception? innerException) : base ($"Invalid prop type \"{typeName}\".", init, innerException)
    {
        Type = typeName;
    }
}

