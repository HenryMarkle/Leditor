using Leditor.Data.Tiles.Exceptions;
using Leditor.RL.Managed;

namespace Leditor.Data.Tiles;

public class TileDexBuilder
{
    private readonly Dictionary<string, (Color color, List<TileDefinition> definitions)> _tiles = new();
    private readonly Dictionary<string, Texture2D> _textures = new();

    /// <summary>
    /// Register a tile category
    /// </summary>
    /// <param name="category">The name of the category</param>
    /// <param name="color">The associated color</param>
    /// <exception cref="DuplicateTileCategoryException">When attempting to register the same category</exception>
    public TileDexBuilder Register(string category, Color color)
    {
        try
        {
            _tiles.Add(category, (color, []));
        }
        catch (ArgumentException e)
        {
            throw new DuplicateTileCategoryException(category, e);
        }
        
        return this;
    }

    /// <summary>
    /// Register a tile belonging to a <paramref name="category"/> along with a texture.
    /// </summary>
    /// <param name="category">The name of the tile category</param>
    /// <param name="definition">A pointer to the tile definition object</param>
    /// <param name="texture">The associated texture</param>
    /// <exception cref="DuplicateTileDefinitionException">When attempting to register the same tile definition more than once</exception>
    /// <exception cref="TileCategoryNotFoundException">When the <paramref name="category"/> is not registered</exception>
    public TileDexBuilder Register(string category, TileDefinition definition, Texture2D texture)
    {
        if (_tiles.TryGetValue(category, out var row))
        {
            row.definitions.Add(definition);
            if (_textures.TryAdd(definition.Name, texture)) definition.Texture = texture.Raw;
            
            return this;
        }

        throw new TileCategoryNotFoundException(category);
    }

    public TileDex Build()
    {
        Dictionary<string, TileDefinition> definitions = new();
        Dictionary<string, TileDefinition[]> categories = new();
        Dictionary<string, Color> colors = new();
        Dictionary<string, string> tileCategory = new();
        Dictionary<string, Color> tileColor = new();

        foreach (var (category, (color, tileDefinitions)) in _tiles)
        {
            foreach (var tileDefinition in tileDefinitions)
            {
                definitions.Add(tileDefinition.Name, tileDefinition);
                tileCategory.Add(tileDefinition.Name, category);
                tileColor.Add(tileDefinition.Name, color);
            }
            
            categories.Add(category, [..tileDefinitions]);
            colors.Add(category, color);
        }

        return new TileDex(definitions, categories, colors, tileCategory, tileColor, _textures);
    }
}