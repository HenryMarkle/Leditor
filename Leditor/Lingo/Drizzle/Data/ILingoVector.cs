namespace Leditor.Lingo.Drizzle;

public interface ILingoVector
{
    int CountElems { get; }
    object? this[int index] { get; }
}