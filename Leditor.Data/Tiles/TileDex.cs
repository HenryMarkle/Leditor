using System.Collections.Immutable;
using Leditor.RL.Managed;

namespace Leditor.Data.Tiles;

#nullable disable

/// <summary>
/// <see cref="IDisposable.Dispose"/> must be called by the consumer.
/// </summary>
public class TileDex : IDisposable
{
    /// <summary>
    /// Indicates whether the object has been already disposed
    /// </summary>
    public bool Disposed { get; private set; }
    
    /// <summary>
    /// Tile -> Definition
    /// </summary>
    private readonly ImmutableDictionary<string, TileDefinition> _definitions;
    
    /// <summary>
    /// Category -> Definition[]
    /// </summary>
    private readonly ImmutableDictionary<string, TileDefinition[]> _categories;

    /// <summary>
    /// Category -> Color
    /// </summary>
    private readonly ImmutableDictionary<string, Color> _colors;
    
    /// <summary>
    /// Tile -> Category
    /// </summary>
    private readonly ImmutableDictionary<string, string> _tileCategory;
    
    /// <summary>
    /// Tile -> Color
    /// </summary>
    private readonly ImmutableDictionary<string, Color> _tileColor;

    /// <summary>
    /// Tile -> Texture
    /// </summary>
    private readonly ImmutableDictionary<string, Texture2D> _textures;
    
    //

    /// <summary>
    /// Get the definition of tile
    /// </summary>
    /// <param name="name">The name of the tile</param>
    /// <returns>A pointer to the definition object</returns>
    /// <exception cref="KeyNotFoundException">The tile <paramref name="name"/> is not found</exception>
    public TileDefinition GetDefinition(string name) => _definitions[name];
    
    /// <summary>
    /// Get all tile definitions that belong to the same category <paramref name="name"/>
    /// </summary>
    /// <param name="name">The name of the common category</param>
    /// <returns>An array of the tile definitions</returns>
    /// <exception cref="KeyNotFoundException">The category <paramref name="name"/> is not found</exception>
    public TileDefinition[] GetDefinitionsOfCategory(string name) => _categories[name];
    
    /// <summary>
    /// Get the color associated to the category
    /// </summary>
    /// <param name="name">The <param name="name"></param> of the category</param>
    /// <returns>A color struct</returns>
    /// <exception cref="KeyNotFoundException">The category <paramref name="name"/> is not found</exception>
    public Color GetCategoryColor(string name) => _colors[name];
    
    /// <summary>
    /// Get the category of a tile
    /// </summary>
    /// <param name="name">The name of the tile</param>
    /// <returns>The name of the category that the tile belongs to</returns>
    /// <exception cref="KeyNotFoundException">The tile name is not found</exception>
    public string GetCategory(string name) => _tileCategory[name];
    
    /// <summary>
    /// Get the color of the category of the tile
    /// </summary>
    /// <param name="name">The name of the tile</param>
    /// <returns>A color struct</returns>
    /// <exception cref="KeyNotFoundException">The tile name is not found</exception>
    public Color GetTileColor(string name) => _tileColor[name];

    /// <summary>
    /// Check if a tile with <paramref name="name"/> exists
    /// </summary>
    /// <param name="name">The name of the tile to check</param>
    /// <returns>true if the tile exists; otherwise <c>false</c></returns>
    public bool TileExists(string name) => _definitions.ContainsKey(name);
    
    /// <summary>
    /// Check if a category with <paramref name="name"/> exists
    /// </summary>
    /// <param name="name">The name of the category to check</param>
    /// <returns>true if the category exists; otherwise false</returns>
    public bool CategoryExists(string name) => _categories.ContainsKey(name);

    /// <summary>
    /// An array of category names with a fixed order
    /// </summary>
    public string[] OrderedCategoryNames { get; init; }

    /// <summary>
    /// Gets the texture associated with the tile
    /// </summary>
    /// <param name="name">The name of the tile</param>
    /// <returns>A managed reference to the texture</returns>
    public Texture2D GetTexture(string name) => _textures[name];
    
    /// <inheritdoc cref="IDisposable"/>
    public void Dispose()
    {
        if (Disposed) return;

        foreach (var (_, texture) in _textures) texture.Dispose();

        Disposed = true;
    }
    //
    
    // ReSharper disable once UnusedMember.Local
    private TileDex() {}

    public TileDex(
        Dictionary<string, TileDefinition> definitions,
        Dictionary<string, TileDefinition[]> categories,
        Dictionary<string, Color> colors,
        Dictionary<string, string> tileCategory,
        Dictionary<string, Color> tileColor,
        Dictionary<string, Texture2D> textures,
        string[] orderedCategoryNames)
    {
        _definitions = definitions.ToImmutableDictionary();
        _categories = categories.ToImmutableDictionary();
        _colors = colors.ToImmutableDictionary();
        _tileCategory = tileCategory.ToImmutableDictionary();
        _tileColor = tileColor.ToImmutableDictionary();
        _textures = textures.ToImmutableDictionary();
        OrderedCategoryNames = [..orderedCategoryNames];
    }

    ~TileDex()
    {
        if (!Disposed) throw new Exception("TileDex was not disposed by consumer");
    }
}