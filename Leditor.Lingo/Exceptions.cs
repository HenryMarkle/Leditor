using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leditor.Leditor.Lingo;

public abstract class ParseException(string message) : Exception(message);

public class PropParseException(string message) : ParseException(message);

public class PropNotFoundException(string message, string name) : PropParseException(message)
{
    public string Name { get; init; } = name;
}

public abstract class InitParseException(string message, string init) : ParseException(message)
{
    public string Init { get; init; } = init;
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

public class PropInitParseException(string message, string init) : InitParseException(message, init);

public class InvalidPropTypeException(string message, string init, string type) : PropInitParseException(message, init)
{
    public string Type { get; init; } = type;
}

