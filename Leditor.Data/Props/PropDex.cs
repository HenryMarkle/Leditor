using System.Collections.Immutable;
using Leditor.Data.Props.Definitions;
using Leditor.RL.Managed;

namespace Leditor.Data.Props;

#nullable disable

/// <summary>
/// <see cref="IDisposable.Dispose"/> must be called by the consumer.
/// </summary>
public class PropDex : IDisposable
{
    /// <summary>
    /// Indicates whether the object has been already disposed
    /// </summary>
    public bool Disposed { get; private set; }
    
    /// <summary>
    /// Prop -> Definition
    /// </summary>
    private readonly ImmutableDictionary<string, PropDefinition> _definitions;
    
    /// <summary>
    /// Category -> Definition[]
    /// </summary>
    private readonly ImmutableDictionary<string, PropDefinition[]> _categories;
    
    /// <summary>
    /// Category -> Color
    /// </summary>
    private readonly ImmutableDictionary<string, Color> _colors;
    
    /// <summary>
    /// Prop -> Category
    /// </summary>
    private readonly ImmutableDictionary<string, string> _propCategory;
    
    /// <summary>
    /// Prop -> Color
    /// </summary>
    private readonly ImmutableDictionary<string, Color> _propColor;

    /// <summary>
    /// Prop -> Texture
    /// </summary>
    private readonly ImmutableDictionary<string, Texture2D> _textures;
    
    //

    /// <summary>
    /// Get the definition of prop
    /// </summary>
    /// <param name="name">The name of the prop</param>
    /// <returns>A pointer to the definition object</returns>
    /// <exception cref="KeyNotFoundException">The prop <paramref name="name"/> is not found</exception>
    public PropDefinition GetDefinition(string name) => _definitions[name];
    
    /// <summary>
    /// Get all prop definitions that belong to the same category <paramref name="name"/>
    /// </summary>
    /// <param name="name">The name of the common category</param>
    /// <returns>An array of the prop definitions</returns>
    /// <exception cref="KeyNotFoundException">The category <paramref name="name"/> is not found</exception>
    public PropDefinition[] GetDefinitionsOfCategory(string name) => _categories[name];
    
    /// <summary>
    /// Get the color associated to the category
    /// </summary>
    /// <param name="name">The <param name="name"></param> of the category</param>
    /// <returns>A color struct</returns>
    /// <exception cref="KeyNotFoundException">The category <paramref name="name"/> is not found</exception>
    public Color GetCategoryColor(string name) => _colors[name];
    
    /// <summary>
    /// Get the category of a prop
    /// </summary>
    /// <param name="name">The name of the prop</param>
    /// <returns>The name of the category that the prop belongs to</returns>
    /// <exception cref="KeyNotFoundException">The prop name is not found</exception>
    public string GetCategory(string name) => _propCategory[name];
    
    /// <summary>
    /// Get the color of the category of the prop
    /// </summary>
    /// <param name="name">The name of the prop</param>
    /// <returns>A color struct</returns>
    /// <exception cref="KeyNotFoundException">The prop name is not found</exception>
    public Color GetPropColor(string name) => _propColor[name];

    /// <summary>
    /// Check if a prop with <paramref name="name"/> exists
    /// </summary>
    /// <param name="name">The name of the prop to check</param>
    /// <returns>true if the prop exists; otherwise <c>false</c></returns>
    public bool PropExists(string name) => _definitions.ContainsKey(name);
    
    /// <summary>
    /// Check if a category with <paramref name="name"/> exists
    /// </summary>
    /// <param name="name">The name of the category to check</param>
    /// <returns>true if the category exists; otherwise false</returns>
    public bool CategoryExists(string name) => _categories.ContainsKey(name);

    /// <summary>
    /// Gets the prop's associated texture
    /// </summary>
    /// <param name="name">The name of the prop</param>
    /// <returns>The associated texture</returns>
    public Texture2D GetTexture(string name) => _textures[name];
    
    //

    public void Dispose()
    {
        if (Disposed) return;
        
        foreach (var (_, texture) in _textures) texture.Dispose();

        Disposed = true;
    }
    
    // ReSharper disable once UnusedMember.Local
    private PropDex() {}

    public PropDex(
        Dictionary<string, PropDefinition> definitions,
        Dictionary<string, PropDefinition[]> categories,
        Dictionary<string, Color> colors,
        Dictionary<string, string> propCategory,
        Dictionary<string, Color> propColor,
        Dictionary<string, Texture2D> texture)
    {
        _definitions = definitions.ToImmutableDictionary();
        _categories = categories.ToImmutableDictionary();
        _colors = colors.ToImmutableDictionary();
        _propCategory = propCategory.ToImmutableDictionary();
        _propColor = propColor.ToImmutableDictionary();
        _textures = texture.ToImmutableDictionary();
    }

    ~PropDex()
    {
        if (!Disposed) throw new Exception("PropDex was not disposed by consumer");
    }
}