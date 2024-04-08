﻿using Leditor.Data.Generic;
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
/// <param name="specs">The first layer geometry requirement per unit tile</param>
/// <param name="repeat">How many times each layer is rendered</param>
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class TileDefinition(
    string name,
    (int Width, int Height) size,
    TileType type,
    int bufferTiles,
    int[,,] specs,
    int[] repeat
) : IIdentifiable<string>
{
    /// <summary>
    /// The name of the tile - Must be unique.
    /// </summary>
    public string Name { get; } = name;
    
    /// <summary>
    /// The size of the tile, in matrix units (20 pixels).
    /// </summary>
    public (int Width, int Height) Size { get; } = size;
    
    /// <summary>
    /// The type of the tile - Determines the rendering method.
    /// </summary>
    public TileType Type { get; } = type;
    
    /// <summary>
    /// The extra space surrounding the tile texture from all sides - in matrix units (20 pixels).
    /// </summary>
    public int BufferTiles { get; } = bufferTiles;
    
    /// <summary>
    /// A three-dimensional array of Geometry tile IDs specifying the geometry requirements of the tile.
    /// <para>Syntax: [y, x, z]</para>
    /// </summary>
    public int[,,] Specs { get; } = specs;
    
    /// <summary>
    /// An array specifying the number of layers and the number of
    /// times each layer gets rendered repeatedly during render time.
    /// </summary>
    public int[] Repeat { get; } = repeat;

    private readonly int _hashCode  = name.GetHashCode();
    
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

    public override bool Equals(object? obj) => obj is TileDefinition t && _hashCode == t.GetHashCode();

    public static implicit operator string(TileDefinition d) => d.Name;

    public override int GetHashCode() => _hashCode;
}