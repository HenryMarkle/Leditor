namespace Leditor.Data;

using Leditor.Data.Geometry;
using Leditor.Data.Tiles;
using Leditor.Data.Effects;
using Leditor.Data.Props.Legacy;

using Raylib_cs;

using System.Numerics;
using Leditor.Data.Materials;

/// <summary>
/// Stores common data for the current loaded level.
/// </summary>
public sealed class LevelState
{
    private (int left, int top, int right, int bottom) _padding;
    
    public int Width { get; private set; }
    public int Height { get; private set; }

    public (int left, int top, int right, int bottom) Padding
    {
        get => _padding;
        set
        {
            _padding = value;

            Border = new Rectangle(
                _padding.left * 20,
                _padding.top * 20,
                (Width - (_padding.right + _padding.left)) * 20,
                (Height - (_padding.bottom + _padding.top)) * 20
            );
        }
    }
    public Rectangle Border { get; private set; }
    public int WaterLevel { get; set; } = -1;
    public bool WaterAtFront { get; set; } = false;
    public int LightAngle { get; set; } = 180; // 0 - 360
    public int LightFlatness { get; set; } = 1; // 1 - 10
    
    public bool LightMode { get; set; } = true;
    public bool DefaultTerrain { get; set; } = true;

    public List<RenderCamera> Cameras { get; set; } = [];

    public Geo[,,] GeoMatrix { get; private set; } = new Geo[0, 0, 0];
    public Tile[,,] TileMatrix { get; private set; } = new Tile[0, 0, 0];
    public Color[,,] MaterialColors { get; private set; } = new Color[0, 0, 0];
    public Effect[] Effects { get; set; } = [];
    public Prop_Legacy[] Props { get; set; } = [];

    public MaterialDefinition DefaultMaterial { get; set; }
    
    public string ProjectName { get; set; } = "New Project";
    
    public int Seed { get; set; } = new Random().Next(1000);
    
    public LevelState(
        int width, 
        int height, 
        (int left, int top, int right, int bottom) padding,
        MaterialDefinition defaultMaterial
    )
    {
        New(width, height, padding, defaultMaterial, [GeoType.Solid, GeoType.Solid, GeoType.Air]);
    }

    public void Import(
        int width,
        int height,
        (int left, int top, int right, int bottom) padding,
        Geo[,,] geoMatrix,
        Tile[,,] tileMatrix,
        Color[,,] materialColorMatrix,
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

    public void New(
        int width,
        int height,
        (int left, int top, int right, int bottom) padding,
        MaterialDefinition defaultMaterial,
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
        DefaultMaterial = defaultMaterial;

        LevelCreated?.Invoke();
    }

    public void Resize(
        int left,
        int top,
        int right,
        int bottom,

        Geo layer1Fill,
        Geo layer2Fill,
        Geo layer3Fill
    ) {
        LevelResized?.Invoke(Width, Height, Width + left + right, Height + top + bottom);

        GeoMatrix = Utils.Resize(GeoMatrix, left, top, right, bottom, [ layer1Fill, layer2Fill, layer3Fill ]);
        TileMatrix = Utils.Resize(TileMatrix, left, top, right, bottom, [ new(), new(), new() ]);
        MaterialColors = Utils.Resize(MaterialColors, left, top, right, bottom, [ new Color(0, 0, 0, 255), new Color(0, 0, 0, 255), new Color(0, 0, 0, 255) ]);

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

    public void Resize(
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
            [new Data.Color(0, 0, 0, 255), new Data.Color(0, 0, 0, 255), new Data.Color(0, 0, 0, 255)]
        );

        // Update Dimensions

        LevelResized?.Invoke(Width, Height, width, height);

        Width = width;
        Height = height;
        Padding = padding;
    }

    public delegate void LevelCreatedEventHandler();
    public delegate void LevelResizedEventHandler(int oldWidth, int oldHeight, int newWidth, int newHeight);

    public event LevelCreatedEventHandler? LevelCreated;
    public event LevelResizedEventHandler? LevelResized;
}
