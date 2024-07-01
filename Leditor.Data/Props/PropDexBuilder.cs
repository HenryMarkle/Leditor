using System.ComponentModel;
using Leditor.Data.Props.Definitions;
using Leditor.Data.Props.Exceptions;
using Leditor.Data.Tiles;
using Leditor.Data.Tiles.Exceptions;
using Leditor.RL.Managed;
using Microsoft.VisualBasic;

namespace Leditor.Data.Props;

public class PropDexBuilder
{
    private readonly HashSet<PropDefinition> _props = [];
    private readonly Dictionary<string, Color> _categoryColors = [];
    private readonly Dictionary<string, List<PropDefinition>> _categoryProps = [];


    private Rope[] _ropes = [];
    private Long[] _longs = [];
    
    private string[] _tileAsPropCategories = [];
    private TileAsProp[][] _tilesAsProps = [];
    

    /// <summary>
    /// Register a prop category
    /// </summary>
    /// <param name="category">The name of the category</param>
    /// <param name="color">The associated color</param>
    /// <exception cref="DuplicateTileCategoryException">When attempting to register the same category</exception>
    public void Register(string category, Color color)
    {
        if (_categoryColors.ContainsKey(category) || _categoryProps.ContainsKey(category))
            throw new DuplicatePropCategoryException(category);

        _categoryColors[category] = color;
        _categoryProps[category] = [];
    }

    public void RegisterTiles(string[] categories, TileDefinition[][] tiles)
    {
        _tileAsPropCategories = categories;
        _tilesAsProps = tiles.Select(tiles => tiles.Select(t => new TileAsProp(t)).ToArray()).ToArray();

        foreach (var category in _tilesAsProps) {
            foreach (var tile in category) {
                if (!_props.Add(tile)) throw new TileAsPropNamingConflictException(tile.Name);
            }
        }
    }

    public void RegisterRopes(Rope[] ropes)
    {
        _ropes = ropes;

        foreach (var rope in _ropes)
        {
            if (!_props.Add(rope)) throw new DuplicatePropDefinitionException(rope.Name);
        }
    }

    public void RegisterLong(Long[] longs)
    {
        _longs = longs;

        foreach (var l in _longs)
        {
            if (!_props.Add(l)) throw new DuplicatePropDefinitionException(l.Name);
        }
    }

    /// <summary>
    /// Register a prop belonging to a <paramref name="category"/> along with a texture.
    /// </summary>
    /// <param name="category">The name of the prop category</param>
    /// <param name="definition">A pointer to the prop definition object</param>
    /// <param name="texture">The associated texture</param>
    /// <exception cref="DuplicateTileDefinitionException">When attempting to register the same prop definition more than once</exception>
    /// <exception cref="TileCategoryNotFoundException">When the <paramref name="category"/> is not registered</exception>
    public void Register(
        string category, 
        PropDefinition definition, 
        bool force = false
    )
    {
        if (!_props.Add(definition))
        {
            if (force)
            {
                _props.Remove(definition);
                _props.Add(definition);
            }
            else throw new DuplicatePropDefinitionException(definition.Name);
        }

        if (_categoryProps.TryGetValue(category, out var list))
        {
            list.Add(definition);
        }
        else
        {
            if (force)
            {
                _categoryProps[category] = [ definition ];
                _categoryColors[category] = new(0, 0, 0, 255);
            }
            else
            {
                throw new PropCategoryNotFoundException(category);
            }
        }
    }

    public PropDex Build()
    {
        Dictionary<string, PropDefinition> definitions = new();
        Dictionary<string, PropDefinition[]> categories = new();
        Dictionary<string, Color> colors = _categoryColors;
        Dictionary<PropDefinition, string> propCategory = new();
        Dictionary<string, Color> propColor = new();

        foreach (var prop in _props) definitions.Add(prop.Name, prop);

        foreach (var (category, props) in _categoryProps)
        {
            categories[category] = [ ..props ];

            var color = colors[category];

            foreach (var prop in props) propColor[prop.Name] = color;
        }

        return new PropDex(
            definitions, 
            categories, 
            colors, 
            propCategory, 
            propColor,
            _ropes,
            _longs,
            _tileAsPropCategories,
            _tilesAsProps
        );
    }
}