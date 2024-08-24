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
    public TileCellType Type { get; } = TileCellType.Default;

    public MaterialDefinition? MaterialDefinition { get; } = null;
    public TileDefinition? TileDefinition { get; } = null;
    public (int X, int Y, int Z) HeadPosition { get; } = (0, 0, 0);

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
    public Tile(MaterialDefinition definition)
    {
        Type = TileCellType.Material;

        MaterialDefinition = definition;
        
        TileDefinition = null;
        HeadPosition = (0, 0, 0);
    }
    
    
    /// <summary>
    /// Creates a new head tile cell
    /// </summary>
    /// <param name="definition">A pointer to a <see cref="TileDefinition"/></param>
    /// <exception cref="NullReferenceException">Tile definition is a null pointer</exception>
    public Tile(TileDefinition definition)
    {
        Type = TileCellType.Head;

        MaterialDefinition = null;
        TileDefinition = definition
                         ?? throw new NullReferenceException("Could not create a new head tile cell");
        HeadPosition = (0, 0, 0);
    }

    /// <summary>
    /// Creates a new body tile cell pointing to the associated tile head
    /// </summary>
    /// <param name="x">Tile head's X coordinates</param>
    /// <param name="y">Tile head's Y coordinates</param>
    /// <param name="z">Tile head's Z coordinates (layer)</param>
    /// <param name="definition">An optional definition of the tile head</param>
    public Tile(int x, int y, int z, TileDefinition? definition = null)
    {
        Type = TileCellType.Body;
        
        MaterialDefinition = null;
        TileDefinition = definition;
        HeadPosition = (x, y, z);
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