namespace Leditor.Serialization.Exceptions;

public class TileNotFoundException : ParseException
{
    public string Name { get; set; }

    public TileNotFoundException(string name) : base($"Material not found \"{name}\"") { Name = name; }
    public TileNotFoundException(string name, Exception? innerException) : base($"Material not found \"{name}\"", innerException) { Name = name; }
}