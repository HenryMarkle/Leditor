using Leditor.Data.Generic;
using Raylib_cs;

namespace Leditor.Data.Tiles;

public enum TileType { 
    Box, 
    VoxelStruct, 
    VoxelStructRandomDisplaceHorizontal,
    VoxelStructRandomDisplaceVertical,
    VoxelStructRockType,
    VoxelStructSandType
}

/// <summary>
/// Encapsulates the definition of a tile loaded from "Init.txt" files
/// </summary>
/// <param name="name">A string identifier</param>
/// <param name="size">The dimensions of the tile without accounting for the <paramref name="bufferTiles"/></param>
/// <param name="type">Denotes the type of the tile</param>
/// <param name="bufferTiles">The extra space the tile texture takes from all directions</param>
/// <param name="specs1">The first layer geometry requirement per unit tile</param>
/// <param name="specs2">The second layer geometry requirement per unit tile</param>
/// <param name="specs3">The third layer geometry requirement per unit tile</param>
/// <param name="repeat">How many times each layer is rendered</param>
/// <param name="texture">The texture associated with the tile - Do not construct manually</param>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class TileDefinition(
    string name,
    (int Width, int Height) size,
    TileType type,
    int bufferTiles,
    int[] specs1,
    int[] specs2,
    int[] specs3,
    int[] repeat
) : IIdentifiable<string>
{

    public string Name { get; } = name;
    public (int Width, int Height) Size { get; } = size;
    public TileType Type { get; } = type;
    public int BufferTiles { get; } = bufferTiles;
    public int[] Specs1 { get; } = specs1;
    public int[] Specs2 { get; } = specs2;
    public int[] Specs3 { get; } = specs3;
    public int[] Repeat { get; } = repeat;
    
    /// <summary>
    /// A weak reference to the associated texture.
    /// <para>
    /// This class is not responsible for managing the texture and should be disposed separately.
    /// </para>
    /// </summary>
    public Texture2D Texture { get; internal set; }
    
    //

    public static bool operator ==(TileDefinition t1, TileDefinition t2) => string.Equals(t1.Name, t2.Name, StringComparison.InvariantCultureIgnoreCase);
    public static bool operator !=(TileDefinition t1, TileDefinition t2) => !string.Equals(t1.Name, t2.Name, StringComparison.InvariantCultureIgnoreCase);

    public override bool Equals(object? obj) =>
        (obj is TileDefinition t) && string.Equals(Name, t.Name, StringComparison.InvariantCultureIgnoreCase);

    public override int GetHashCode() => Name.GetHashCode();
}