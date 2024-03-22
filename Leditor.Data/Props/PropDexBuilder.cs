using Leditor.Data.Props.Definitions;
using Leditor.Data.Props.Exceptions;
using Leditor.Data.Tiles.Exceptions;
using Leditor.RL.Managed;

namespace Leditor.Data.Props;

public class PropDexBuilder
{
    private readonly Dictionary<string, (Color color, List<PropDefinition> definitions)> _tiles = new();
    private readonly Dictionary<string, Texture2D> _textures = new();

    /// <summary>
    /// Register a prop category
    /// </summary>
    /// <param name="category">The name of the category</param>
    /// <param name="color">The associated color</param>
    /// <exception cref="DuplicateTileCategoryException">When attempting to register the same category</exception>
    public PropDexBuilder Register(string category, Color color)
    {
        try
        {
            _tiles.Add(category, (color, []));
        }
        catch (ArgumentException e)
        {
            throw new DuplicatePropCategoryException(category, e);
        }
        
        return this;
    }

    /// <summary>
    /// Register a prop belonging to a <paramref name="category"/> along with a texture.
    /// </summary>
    /// <param name="category">The name of the prop category</param>
    /// <param name="definition">A pointer to the prop definition object</param>
    /// <param name="texture">The associated texture</param>
    /// <exception cref="DuplicateTileDefinitionException">When attempting to register the same prop definition more than once</exception>
    /// <exception cref="TileCategoryNotFoundException">When the <paramref name="category"/> is not registered</exception>
    public PropDexBuilder Register(string category, PropDefinition definition, Texture2D texture)
    {
        if (_tiles.TryGetValue(category, out var row))
        {
            row.definitions.Add(definition);
            if (_textures.TryAdd(definition.Name, texture)) definition.Texture = texture.Raw;
            
            return this;
        }

        throw new PropCategoryNotFoundException(category);
    }

    public PropDex Build()
    {
        Dictionary<string, PropDefinition> definitions = new();
        Dictionary<string, PropDefinition[]> categories = new();
        Dictionary<string, Color> colors = new();
        Dictionary<string, string> propCategory = new();
        Dictionary<string, Color> propColor = new();

        foreach (var (category, (color, propDefinitions)) in _tiles)
        {
            foreach (var propDefinition in propDefinitions)
            {
                definitions.Add(propDefinition.Name, propDefinition);
                propCategory.Add(propDefinition.Name, category);
                propColor.Add(propDefinition.Name, color);
            }
            
            categories.Add(category, [..propDefinitions]);
            colors.Add(category, color);
        }

        return new PropDex(definitions, categories, colors, propCategory, propColor, _textures);
    }
}