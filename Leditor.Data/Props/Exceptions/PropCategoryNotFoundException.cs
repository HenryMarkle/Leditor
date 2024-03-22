namespace Leditor.Data.Props.Exceptions;

/// <summary>
/// The exception that is thrown when a category could not be found on lookup
/// </summary>
public class PropCategoryNotFoundException : Exception
{
    public string Category { get; }

    public PropCategoryNotFoundException(string category) : base($"Prop category \"{category}\" not found")
    {
        Category = category;
    }

    public PropCategoryNotFoundException(string category, Exception? innerException) 
        : base($"Prop category \"{category}\" not found", innerException)
    {
        Category = category;
    }
}