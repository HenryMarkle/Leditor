using System.Numerics;

using Leditor.Data;
using Leditor.Data.Effects;
using Leditor.Data.Geometry;
using Leditor.Data.Materials;
using Leditor.Data.Props.Legacy;
using Leditor.Data.Tiles;

namespace Leditor;

public class LevelLoadedEventArgs(bool undefinedTiles, bool missingTileTextures, bool undefinedMaterials) : EventArgs
{
    public bool UndefinedTiles { get; set; } = undefinedTiles;
    public bool MissingTileTextures { get; set; } = missingTileTextures;
    public bool UndefinedMaterials { get; set; } = undefinedMaterials;
}

/// Used to report the tile check status when loading a project
public enum TileCheckResultEnum
{
    Ok, Missing, NotFound, MissingTexture, MissingMaterial
}

public readonly struct TileCheckResult {
    public HashSet<string> MissingTileDefinitions { get; init; }
    public HashSet<string> MissingTileTextures { get; init; }
    public HashSet<string> MissingMaterialDefinitions { get; init; }
}

/// Used to report the tile check status when loading a project
public enum PropCheckResult
{
    Ok, Undefined, MissingTexture, NotOk
}


// TODO: improve the success status reporting
/// Used for loading project files
public class LoadFileResult
{
    public bool Success { get; init; } = false;
    
    public int Seed { get; set; }
    public int WaterLevel { get; set; }
    public bool WaterInFront { get; set; }

    public int Width { get; init; } = 0;
    public int Height { get; init; } = 0;

    public BufferTiles BufferTiles { get; init; } = new();
    public Effect[] Effects { get; init; } = [];

    public bool LightMode { get; init; }
    public bool DefaultTerrain { get; set; }
    public MaterialDefinition DefaultMaterial { get; set; }

    public Geo[,,]? GeoMatrix { get; init; } = null;
    public Tile[,,]? TileMatrix { get; init; } = null;
    public Data.Color[,,]? MaterialColorMatrix { get; init; } = null;
    public Prop_Legacy[]? PropsArray { get; init; } = null;

    public Image LightMapImage { get; init; }
    
    public (int angle, int flatness) LightSettings { get; init; }

    public List<RenderCamera> Cameras { get; set; } = [];

    public string Name { get; init; } = "New Project";

    public Exception? PropsLoadException { get; init; } = null;
}


public readonly record struct BufferTiles(int Left, int Right, int Top, int Bottom);

