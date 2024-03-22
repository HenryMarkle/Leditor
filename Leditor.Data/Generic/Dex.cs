using System.Collections.Immutable;
using Leditor.RL.Managed;

namespace Leditor.Data.Generic;

#nullable disable

public class Dex<TKey, TDef> : IDisposable
    where TKey : IEquatable<TKey>
    where TDef : class, IIdentifiable<TKey>
{
    public bool Disposed { get; private set; }
    
    /// <summary>
    /// Name -> Definition
    /// </summary>
    private readonly ImmutableDictionary<string, TDef> _definitions;
    
    /// <summary>
    /// Category -> Definition[]
    /// </summary>
    private readonly ImmutableDictionary<string, TDef[]> _categories;
    
    /// <summary>
    /// Category -> Color
    /// </summary>
    private readonly ImmutableDictionary<string, Color> _colors;
    
    /// <summary>
    /// Name -> Category
    /// </summary>
    private readonly ImmutableDictionary<string, string> _definitionCategory;
    
    /// <summary>
    /// Name -> Color
    /// </summary>
    private readonly ImmutableDictionary<string, Color> _definitionColor;

    /// <summary>
    /// Name -> Texture
    /// </summary>
    private readonly ImmutableDictionary<string, Texture2D> _textures;
    
    //

    /// <summary>
    /// Get the definition associated to a string
    /// </summary>
    /// <param name="name">The name of the definition</param>
    /// <returns>A pointer to the definition object</returns>
    /// <exception cref="KeyNotFoundException">The definition with <paramref name="name"/> is not found</exception>
    public TDef GetDefinition(string name) => _definitions[name];
    
    /// <summary>
    /// Get all definitions that belong to the same category <paramref name="name"/>
    /// </summary>
    /// <param name="name">The name of the common category</param>
    /// <returns>An array of the definitions</returns>
    /// <exception cref="KeyNotFoundException">The category <paramref name="name"/> is not found</exception>
    public TDef[] GetDefinitionsOfCategory(string name) => _categories[name];
    
    /// <summary>
    /// Get the color associated to the category
    /// </summary>
    /// <param name="name">The <param name="name"></param> of the category</param>
    /// <returns>A color struct</returns>
    /// <exception cref="KeyNotFoundException">The category <paramref name="name"/> is not found</exception>
    public Color GetCategoryColor(string name) => _colors[name];
    
    /// <summary>
    /// Get the category of a definition
    /// </summary>
    /// <param name="name">The name of the definition</param>
    /// <returns>The name of the category that the definition belongs to</returns>
    /// <exception cref="KeyNotFoundException">The definition name is not found</exception>
    public string GetCategory(string name) => _definitionCategory[name];
    
    /// <summary>
    /// Get the color of the category of the definition
    /// </summary>
    /// <param name="name">The name of the definition</param>
    /// <returns>A color struct</returns>
    /// <exception cref="KeyNotFoundException">The definition name is not found</exception>
    public Color GetDefinitionColor(string name) => _definitionColor[name];

    /// <summary>
    /// Check if a definition with <paramref name="name"/> exists
    /// </summary>
    /// <param name="name">The name of the definition to check</param>
    /// <returns>true if the definition exists; otherwise <c>false</c></returns>
    public bool DefinitionExists(string name) => _definitions.ContainsKey(name);
    
    /// <summary>
    /// Check if a category with <paramref name="name"/> exists
    /// </summary>
    /// <param name="name">The name of the category to check</param>
    /// <returns>true if the category exists; otherwise false</returns>
    public bool CategoryExists(string name) => _categories.ContainsKey(name);

    /// <summary>
    /// Get a managed reference to the associated texture
    /// </summary>
    /// <param name="name">The name of the definition</param>
    /// <returns>A managed texture object</returns>
    /// <exception cref="KeyNotFoundException">The definition name is not found</exception>
    public Texture2D GetTexture(string name) => _textures[name];
    
    //

    public void Dispose()
    {
        if (Disposed) return;
        
        foreach (var (_, texture) in _textures) texture.Dispose();

        Disposed = true;
    }
    
    // ReSharper disable once UnusedMember.Local
    private Dex() {}

    public Dex(
        Dictionary<string, TDef> definitions,
        Dictionary<string, TDef[]> categories,
        Dictionary<string, Color> colors,
        Dictionary<string, string> propCategory,
        Dictionary<string, Color> propColor,
        Dictionary<string, Texture2D> textures)
    {
        _definitions = definitions.ToImmutableDictionary();
        _categories = categories.ToImmutableDictionary();
        _colors = colors.ToImmutableDictionary();
        _definitionCategory = propCategory.ToImmutableDictionary();
        _definitionColor = propColor.ToImmutableDictionary();
        _textures = textures.ToImmutableDictionary();
    }

    ~Dex()
    {
        if (!Disposed) throw new Exception("Dex was not disposed by the consumer");
    }
}