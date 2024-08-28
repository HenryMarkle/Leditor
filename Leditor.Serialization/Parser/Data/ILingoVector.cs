namespace Leditor.Serialization.Parser.Data;

public interface ILingoVector
{
    int CountElems { get; }
    object? this[int index] { get; }
}