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

/// <summary>
/// Stores common data for the current loaded level.
/// </summary>
public sealed class LevelState
{
    private (int left, int top, int right, int bottom) _padding;
    
    internal int Width { get; private set; }
    internal int Height { get; private set; }

    internal (int left, int top, int right, int bottom) Padding
    {
        get => _padding;
        set
        {
            _padding = value;

            Border = new Rectangle(
                _padding.left * GLOBALS.Scale,
                _padding.top * GLOBALS.Scale,
                (Width - (_padding.right + _padding.left)) * GLOBALS.Scale,
                (Height - (_padding.bottom + _padding.top)) * GLOBALS.Scale
            );
        }
    }
    internal Rectangle Border { get; private set; }
    internal int WaterLevel { get; set; } = -1;
    internal bool WaterAtFront { get; set; } = false;
    internal int LightAngle { get; set; } = 180; // 0 - 360
    internal int LightFlatness { get; set; } = 1; // 1 - 10
    
    internal bool LightMode { get; set; } = true;
    internal bool DefaultTerrain { get; set; } = true;

    internal List<RenderCamera> Cameras { get; set; } = [];

    internal Geo[,,] GeoMatrix { get; private set; } = new Geo[0, 0, 0];
    internal Tile[,,] TileMatrix { get; private set; } = new Tile[0, 0, 0];
    internal Data.Color[,,] MaterialColors { get; private set; } = new Data.Color[0, 0, 0];
    internal Effect[] Effects { get; set; } = [];
    internal Prop_Legacy[] Props { get; set; } = [];

    internal MaterialDefinition DefaultMaterial { get; set; }
    
    internal string ProjectName { get; set; } = "New Project";
    
    internal int Seed { get; set; } = new Random().Next(1000);
    
    internal LevelState(int width, int height, (int left, int top, int right, int bottom) padding)
    {
        New(width, height, padding, [GeoType.Solid, GeoType.Solid, GeoType.Air]);
    }

    internal void Import(
        int width,
        int height,
        (int left, int top, int right, int bottom) padding,
        Geo[,,] geoMatrix,
        Tile[,,] tileMatrix,
        Data.Color[,,] materialColorMatrix,
        Effect[] effects,
        List<RenderCamera> cameras,
        Prop_Legacy[] props,
        (int angle, int flatness) lightSettings,
        bool lightMode,
        bool terrainMedium,
        int seed,
        int waterLevel,
        bool waterInFront,
        MaterialDefinition defaultMaterial,
        string projectName = "New Project"
    )
    {
        Width = width;
        Height = height;
        Seed = seed;
        WaterLevel = waterLevel;
        WaterAtFront = waterInFront;
        Padding = padding;
        GeoMatrix = geoMatrix;
        TileMatrix = tileMatrix;
        MaterialColors = materialColorMatrix;
        Effects = effects;
        DefaultMaterial = defaultMaterial;
        ProjectName = projectName;

        LightAngle = lightSettings.angle;
        LightFlatness = lightSettings.flatness;

        Cameras = cameras;
        Props = props;

        LightMode = lightMode;
        DefaultTerrain = terrainMedium;

        LevelCreated?.Invoke();
    }

    internal void New(
        int width,
        int height,
        (int left, int top, int right, int bottom) padding,
        Span<GeoType> geoIdFill
    )
    {
        Width = width;
        Height = height;
        Padding = padding;

        LightFlatness = 1;
        LightAngle = 180;

        Cameras = [new RenderCamera { Coords = new Vector2(20f, 30f), Quad = new(new(), new(), new(), new()) }];

        Effects = [];
        
        // Geo Matrix
        {
            var matrix = new Geo[height, width, 3];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    matrix[y, x, 0] = new(geoIdFill[0]);
                    matrix[y, x, 1] = new(geoIdFill[1]);
                    matrix[y, x, 2] = new(geoIdFill[2]);
                }
            }

            GeoMatrix = matrix;
        }

        // Tile Matrix
        {
            var matrix = new Tile[height, width, 3];

            // Unneeded?
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    matrix[y, x, 0] = new();

                    matrix[y, x, 1] = new();

                    matrix[y, x, 2] = new();
                }
            }

            TileMatrix = matrix;
        }

        // Material Color Matrix
        {
            var matrix = new Data.Color[height, width, 3];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    matrix[y, x, 0] = new Data.Color(0, 0, 0, 255);
                    matrix[y, x, 1] = new Data.Color(0, 0, 0, 255);
                    matrix[y, x, 2] = new Data.Color(0, 0, 0, 255);
                }
            }

            MaterialColors = matrix;
        }
        
        // Props
        Props = [];

        WaterLevel = -1;
        WaterAtFront = false;
        Seed = new Random().Next(10000);
        DefaultMaterial = GLOBALS.Materials[0][1];

        LevelCreated?.Invoke();
    }

    internal void Resize(
        int left,
        int top,
        int right,
        int bottom,

        Geo layer1Fill,
        Geo layer2Fill,
        Geo layer3Fill
    ) {
        LevelResized?.Invoke(Width, Height, Width + left + right, Height + top + bottom);

        GeoMatrix = Utils.Resize(GeoMatrix, left, top, right, bottom, layer1Fill, layer2Fill, layer3Fill);
        TileMatrix = Utils.Resize(TileMatrix, left, top, right, bottom);
        MaterialColors = Utils.Resize(MaterialColors, left, top, right, bottom, new Data.Color(0, 0, 0, 255), new Data.Color(0, 0, 0, 255), new Data.Color(0, 0, 0, 255));

        for (var e = 0; e < Effects.Length; e++) {
            Effects[e].Matrix = Utils.Resize(Effects[e].Matrix, left, top, right, bottom);
        }

        for (var p = 0; p < Props.Length; p++) {
            var quad = Props[p].Quad;

            var delta = new Vector2(left, top) * 20;

            quad.TopLeft += delta;
            quad.TopRight += delta;
            quad.BottomRight += delta;
            quad.BottomLeft += delta;

            Props[p].Quad = quad;

            if (Props[p].Type == InitPropType_Legacy.Rope) {
                for (var s = 0; s < Props[p].Extras.RopePoints.Length; s++) {
                    var segments = Props[p].Extras.RopePoints;

                    segments[s] += delta;
                }
            }
        }

        Width = GeoMatrix.GetLength(1);
        Height = GeoMatrix.GetLength(0);

        Padding = _padding;
    }

    internal void Resize(
        int width,
        int height,
        (int left, int top, int right, int bottom) padding,
        ReadOnlySpan<GeoType> geoIdFill
    )
    {
        // Geo Matrix
        GeoMatrix = Utils.Resize(
            GeoMatrix,
            Width,
            Height,
            width,
            height,
            [
                new(geoIdFill[0]),
                new(geoIdFill[1]),
                new(geoIdFill[2])
            ]
        );

        // The Effects Matrix
        for (var m = 0; m < Effects.Length; m++)
        {
            Effects[m].Matrix = Utils.Resize(Effects[m].Matrix, width, height);
        }

        // Tile Matrix
        TileMatrix = Utils.Resize(
            TileMatrix,
            width,
            height
        );

        // Material Color Matrix
        MaterialColors = Utils.Resize(
            MaterialColors,
            width,
            height,
            [Raylib_cs.Color.Black, Raylib_cs.Color.Black, Raylib_cs.Color.Black]
        );

        // Update Dimensions

        LevelResized?.Invoke(Width, Height, width, height);

        Width = width;
        Height = height;
        Padding = padding;
    }

    internal delegate void LevelCreatedEventHandler();
    internal delegate void LevelResizedEventHandler(int oldWidth, int oldHeight, int newWidth, int newHeight);

    internal event LevelCreatedEventHandler? LevelCreated;
    internal event LevelResizedEventHandler? LevelResized;
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

