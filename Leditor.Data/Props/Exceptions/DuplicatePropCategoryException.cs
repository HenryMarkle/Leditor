namespace Leditor.Data.Props.Exceptions;

/// <summary>
/// The exception that is thrown when attempting to register a category that is already registered
/// </summary>
public class DuplicatePropCategoryException : Exception
{
    public string Category { get; }

    public DuplicatePropCategoryException(string category) : base($"Category \"{category}\" already registered")
    {
        Category = category;
    }
    
    public DuplicatePropCategoryException(string category, Exception? innerException) 
        : base($"Category \"{category}\" already registered", innerException)
    {
        Category = category;
    }
}