﻿using System.Collections.Immutable;
using Leditor.Data.Props.Definitions;
using Leditor.RL.Managed;

namespace Leditor.Data.Props;

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
    private readonly ImmutableDictionary<PropDefinition, string> _propCategory;
    
    /// <summary>
    /// Prop -> Color
    /// </summary>
    private readonly ImmutableDictionary<string, Color> _propColor;


    private Rope[] _ropes = [];
    private Long[] _longs = [];

    private string[] _tilesAsPropsCategories = [];
    private TileAsProp[][] _tilesAsProps = [];

    
    private string[] _orderedCategories = [];

    //

    public Long[] Longs => _longs;
    public Rope[] Ropes => _ropes;

    public string[] TileCategories => _tilesAsPropsCategories;
    public TileAsProp[][] Tiles => _tilesAsProps;

    public string[] OrderedCategories => _orderedCategories;

    /// <summary>
    /// Get the definition of prop
    /// </summary>
    /// <param name="name">The name of the prop</param>
    /// <returns>A pointer to the definition object</returns>
    /// <exception cref="KeyNotFoundException">The prop <paramref name="name"/> is not found</exception>
    public PropDefinition GetProp(string name) => _definitions[name];

    /// <summary>
    /// Get the definition of prop
    /// </summary>
    /// <param name="name">The name of the prop</param>
    /// <returns>true if found; otherwise false</returns>
    public bool TryGetProp(string name, out PropDefinition? definition) => _definitions.TryGetValue(name, out definition);
    
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
    /// <param name="prop">The prop</param>
    /// <returns>The name of the category that the prop belongs to</returns>
    /// <exception cref="KeyNotFoundException">The prop name is not found</exception>
    public string GetCategory(PropDefinition prop) => _propCategory[prop];

    /// <summary>
    /// Get the category of a prop
    /// </summary>
    /// <param name="prop">The prop</param>
    /// <param name="category">The found category name</param>
    /// <returns>true if found; otherwise false</returns>
    /// <exception cref="KeyNotFoundException">The prop name is not found</exception>
    public bool TryGetCategory(PropDefinition prop, out string? category) => _propCategory.TryGetValue(prop, out category);
    
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

    public delegate void DexTextureUpdateEventHandler();

    public event DexTextureUpdateEventHandler? TextureUpdated;

    public void UpdateTexture(string name, Texture2D texture)
    {
        _definitions[name].Texture = texture;

        TextureUpdated?.Invoke();
    }
    
    //

    public void Dispose()
    {
        if (Disposed) return;
        
        foreach (var d in _definitions) if (d.Value.Texture.Id != 0) Raylib_cs.Raylib.UnloadTexture(d.Value.Texture);

        Disposed = true;
    }

    public PropDex(
        Dictionary<string, PropDefinition> definitions,
        Dictionary<string, PropDefinition[]> categories,
        Dictionary<string, Color> colors,
        Dictionary<PropDefinition, string> propCategory,
        Dictionary<string, Color> propColor,
        Rope[] ropes,
        Long[] longs,
        string[] tileCategories,
        TileAsProp[][] tiles
        )
    {
        _definitions = definitions.ToImmutableDictionary();
        _categories = categories.ToImmutableDictionary();
        _colors = colors.ToImmutableDictionary();
        _propCategory = propCategory.ToImmutableDictionary();
        _propColor = propColor.ToImmutableDictionary();

        _ropes = ropes;
        _longs = longs;

        _tilesAsProps = tiles;
        _tilesAsPropsCategories = tileCategories;

        _orderedCategories = [ ..categories.Keys ];
    }

    ~PropDex()
    {
        if (!Disposed) throw new Exception("PropDex was not disposed by consumer");
    }
}