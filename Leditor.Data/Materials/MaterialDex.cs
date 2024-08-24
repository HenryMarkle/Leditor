namespace Leditor.Data.Materials;

using System.Collections.Immutable;

/// <summary>
/// <see cref="IDisposable.Dispose"/> must be called by the consumer.
/// </summary>
public class MaterialDex
{
    /// <summary>
    /// Tile -> Definition
    /// </summary>
    private readonly ImmutableDictionary<string, MaterialDefinition> _definitions;
    
    /// <summary>
    /// Category -> Definition[]
    /// </summary>
    private readonly ImmutableDictionary<string, MaterialDefinition[]> _categories;
    
    /// <summary>
    /// Tile -> Category
    /// </summary>
    private readonly ImmutableDictionary<MaterialDefinition, string> _materialCategory;
    
    /// <summary>
    /// Tile -> Color
    /// </summary>
    private readonly ImmutableDictionary<string, Color> _materialColor;
    
    //

    /// <summary>
    /// Gets the number of registered tiles.
    /// </summary>
    public int Count => _definitions.Count;

    /// <summary>
    /// Get the definition of tile
    /// </summary>
    /// <param name="name">The name of the tile</param>
    /// <returns>A pointer to the definition object</returns>
    /// <exception cref="KeyNotFoundException">The tile <paramref name="name"/> is not found</exception>
    public MaterialDefinition GetMaterial(string name) => _definitions[name];

    /// <summary>
    /// Attempts to get the definition of tile
    /// </summary>
    /// <param name="name">The name of the tile</param>
    /// <param name="definition">The found definition</param>
    /// <returns>true if found; otherwise false</returns>
    public bool TryGetMaterial(string name, out MaterialDefinition? definition) =>
        _definitions.TryGetValue(name, out definition);
    
    /// <summary>
    /// Get all tile definitions that belong to the same category <paramref name="name"/>
    /// </summary>
    /// <param name="name">The name of the common category</param>
    /// <returns>An array of the tile definitions</returns>
    /// <exception cref="KeyNotFoundException">The category <paramref name="name"/> is not found</exception>
    public MaterialDefinition[] GetMaterialsOfCategory(string name) => _categories[name];
    
    /// <summary>
    /// Attempts to get all tile definitions that belong to the same category <paramref name="name"/>
    /// </summary>
    /// <param name="name">The name of the common category</param>
    /// <returns>An array of the tile definitions; If not found then an empty array will be returned</returns>
    public bool TryGetMaterialsOfCategory(string name, out MaterialDefinition[]? materials) => _categories.TryGetValue(name, out materials);
    

    
    /// <summary>
    /// Get the category of a tile
    /// </summary>
    /// <param name="tile">The tile</param>
    /// <returns>The name of the category that the tile belongs to</returns>
    /// <exception cref="KeyNotFoundException">The tile name is not found</exception>
    public string GetCategory(MaterialDefinition material) => _materialCategory[material];

    /// <summary>
    /// Get the category of a tile
    /// </summary>
    /// <param name="tile">The name of the tile</param>
    /// <param name="category">The found category</param>
    /// <returns>true if found; otherwise false</returns>
    /// <exception cref="KeyNotFoundException">The tile name is not found</exception>
    public bool TryGetCategory(MaterialDefinition tile, out string? category) => _materialCategory.TryGetValue(tile, out category);
    
    /// <summary>
    /// Get the color of the category of the tile
    /// </summary>
    /// <param name="name">The name of the tile</param>
    /// <returns>A color struct</returns>
    /// <exception cref="KeyNotFoundException">The tile name is not found</exception>
    public Color GetMaterialColor(string name) => _materialColor[name];
    
    /// <summary>
    /// Get the color of the category of the tile
    /// </summary>
    /// <param name="tile">The tile</param>
    /// <returns>A color struct</returns>
    /// <exception cref="KeyNotFoundException">The tile name is not found</exception>
    public Color GetTileColor(MaterialDefinition tile) => _materialColor[tile.Name];

    /// <summary>
    /// Attempts to get the color of the category of the tile
    /// </summary>
    /// <param name="name">The name of the tile</param>
    /// <param name="color">The found color</param>
    /// <returns>true if found; otherwise false</returns>
    public bool TryGetTileColor(string name, out Color color) => _materialColor.TryGetValue(name, out color);
    
    
    /// <summary>
    /// Attempts to get the color of the category of the tile
    /// </summary>
    /// <param name="tile">The tile</param>
    /// <param name="color">The found color</param>
    /// <returns>true if found; otherwise false</returns>
    public bool TryGetTileColor(MaterialDefinition tile, out Color color) => _materialColor.TryGetValue(tile.Name, out color);

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

    public MaterialDex(
        Dictionary<string, MaterialDefinition> definitions,
        Dictionary<string, MaterialDefinition[]> categories,
        Dictionary<MaterialDefinition, string> materialCategory,
        Dictionary<string, Color> materialColor,
        string[] orderedCategoryNames)
    {
        _definitions = definitions.ToImmutableDictionary();
        _categories = categories.ToImmutableDictionary();
        _materialCategory = materialCategory.ToImmutableDictionary();
        _materialColor = materialColor.ToImmutableDictionary();
        OrderedCategoryNames = orderedCategoryNames;
    }
}