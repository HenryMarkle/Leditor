using Leditor.Data.Materials;

namespace Leditor.Data.Tiles;

public enum TileCellType
{
    Default,
    Material,
    Head,
    Body
}

/// <summary>
/// Represents a single tile cell instance
/// </summary>
public readonly struct Tile
{
    /// <summary>
    /// Check the type before accessing the other properties
    /// </summary>
    public TileCellType Type { get; init; } = TileCellType.Default;

    public MaterialDefinition? MaterialDefinition { get; init; } = null;
    public TileDefinition? TileDefinition { get; init; } = null;
    public (int X, int Y, int Z) HeadPosition { get; init; } = (0, 0, 0);

    public string? UndefinedName { get; init; }

    //

    /// <summary>
    /// Creates a new default tile cell
    /// </summary>
    public Tile()
    {
        Type = TileCellType.Default;
        
        MaterialDefinition = null;
        TileDefinition = null;
        HeadPosition = (0, 0, 0);
    }

    /// <summary>
    /// Creates a new material tile cell
    /// </summary>
    /// <param name="definition">A pointer to a <see cref="MaterialDefinition"/></param>
    /// <exception cref="NullReferenceException">Material definition is a null pointer</exception>
    public Tile(MaterialDefinition? definition, string? undefinedName = null)
    {
        Type = TileCellType.Material;

        MaterialDefinition = definition;
        UndefinedName = undefinedName;
        
        TileDefinition = null;
        HeadPosition = (0, 0, 0);
    }
    
    
    /// <summary>
    /// Creates a new head tile cell
    /// </summary>
    /// <param name="definition">A pointer to a <see cref="TileDefinition"/></param>
    /// <exception cref="NullReferenceException">Tile definition is a null pointer</exception>
    public Tile(TileDefinition? definition, string? undefinedName = null)
    {
        Type = TileCellType.Head;

        MaterialDefinition = null;
        TileDefinition = definition;
        HeadPosition = (0, 0, 0);

        UndefinedName = undefinedName;
    }

    /// <summary>
    /// Creates a new body tile cell pointing to the associated tile head
    /// </summary>
    /// <param name="x">Tile head's X coordinates</param>
    /// <param name="y">Tile head's Y coordinates</param>
    /// <param name="z">Tile head's Z coordinates (layer)</param>
    /// <param name="definition">An optional definition of the tile head</param>
    public Tile(int x, int y, int z, TileDefinition? definition = null, string? undefinedName = null)
    {
        Type = TileCellType.Body;
        
        MaterialDefinition = null;
        TileDefinition = definition;
        HeadPosition = (x, y, z);

        UndefinedName = undefinedName;
    }

    /// <summary>
    /// Clones a pre-existing tile cell
    /// </summary>
    /// <param name="cell">A tile cell to clone</param>
    public Tile(Tile cell)
    {
        Type = cell.Type;

        MaterialDefinition = cell.MaterialDefinition;
        TileDefinition = cell.TileDefinition;
        HeadPosition = cell.HeadPosition;
    }
}