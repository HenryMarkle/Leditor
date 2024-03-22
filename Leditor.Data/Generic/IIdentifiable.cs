namespace Leditor.Data.Generic;

public interface IIdentifiable<out T>
    where T : IEquatable<T>
{
    T Name { get; }
}