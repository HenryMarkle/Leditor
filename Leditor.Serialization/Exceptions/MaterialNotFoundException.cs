namespace Leditor.Serialization.Exceptions;

public class MaterialNotFoundException : ParseException
{
    public string Name { get; set; }

    public MaterialNotFoundException(string name) : base($"Material not found \"{name}\"") { Name = name; }
    public MaterialNotFoundException(string name, Exception? innerException) : base($"Material not found \"{name}\"", innerException) { Name = name; }
}