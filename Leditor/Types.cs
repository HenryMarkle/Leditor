using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;
using Leditor.Data.Materials;
using Leditor.Data.Props.Definitions;
using Leditor.Data.Tiles;
using Leditor.Data.Geometry;

namespace Leditor;

#nullable enable

public class LevelLoadedEventArgs(bool undefinedTiles, bool missingTileTextures, bool undefinedMaterials) : EventArgs
{
    public bool UndefinedTiles { get; set; } = undefinedTiles;
    public bool MissingTileTextures { get; set; } = missingTileTextures;
    public bool UndefinedMaterials { get; set; } = undefinedMaterials;
}

public sealed class QuadVectors
{
    public Vector2 TopLeft { get; set; }
    public Vector2 TopRight { get; set; }
    public Vector2 BottomRight { get; set; }
    public Vector2 BottomLeft { get; set; }

    public void Deconstruct(
        out Vector2 topLeft, 
        out Vector2 topRight, 
        out Vector2 bottomRight, 
        out Vector2 bottomLeft)
    {
        topLeft = TopLeft;
        topRight = TopRight;
        bottomRight = BottomRight;
        bottomLeft = BottomLeft;
    }

    public QuadVectors()
    {
        TopLeft = new Vector2(0, 0);
        TopRight = new Vector2(0, 0);
        BottomRight = new Vector2(0, 0);
        BottomLeft = new Vector2(0, 0);
    }

    public QuadVectors(Vector2 topLeft, Vector2 topRight, Vector2 bottomRight, Vector2 bottomLeft)
    {
        TopLeft = topLeft;
        TopRight = topRight;
        BottomRight = bottomRight;
        BottomLeft = bottomLeft;
    }

    public QuadVectors(QuadVectors quadVectors)
    {
        TopLeft = quadVectors.TopLeft;
        TopRight = quadVectors.TopRight;
        BottomRight = quadVectors.BottomRight;
        BottomLeft = quadVectors.BottomLeft;
    }
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
    internal TileCell[,,] TileMatrix { get; private set; } = new TileCell[0, 0, 0];
    internal Color[,,] MaterialColors { get; private set; } = new Color[0, 0, 0];
    internal (string, EffectOptions[], double[,])[] Effects { get; set; } = [];
    internal Prop[] Props { get; set; } = [];

    internal Data.Materials.Material DefaultMaterial { get; set; }
    
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
        TileCell[,,] tileMatrix,
        Color[,,] materialColorMatrix,
        (string, EffectOptions[], double[,])[] effects,
        List<RenderCamera> cameras,
        Prop[] props,
        (int angle, int flatness) lightSettings,
        bool lightMode,
        bool terrainMedium,
        int seed,
        int waterLevel,
        bool waterInFront,
        Data.Materials.Material defaultMaterial,
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
            var matrix = new TileCell[height, width, 3];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    matrix[y, x, 0] = new()
                    {
                        Type = TileType.Default,
                        Data = new TileDefault()
                    };

                    matrix[y, x, 1] = new()
                    {
                        Type = TileType.Default,
                        Data = new TileDefault()
                    };

                    matrix[y, x, 2] = new()
                    {
                        Type = TileType.Default,
                        Data = new TileDefault()
                    };
                }
            }

            TileMatrix = matrix;
        }

        // Material Color Matrix
        {
            var matrix = new Color[height, width, 3];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    matrix[y, x, 0] = Color.Black;
                    matrix[y, x, 1] = Color.Black;
                    matrix[y, x, 2] = Color.Black;
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
        TileMatrix = Utils.Resize(TileMatrix, left, top, right, bottom, new TileCell(), new TileCell(), new TileCell());
        MaterialColors = Utils.Resize(MaterialColors, left, top, right, bottom, Color.Black, Color.Black, Color.Black);

        for (var e = 0; e < Effects.Length; e++) {
            Effects[e].Item3 = Utils.Resize(Effects[e].Item3, left, top, right, bottom);
        }

        for (var p = 0; p < Props.Length; p++) {
            var quad = Props[p].Quad;

            var delta = new Vector2(left, top) * 20;

            quad.TopLeft += delta;
            quad.TopRight += delta;
            quad.BottomRight += delta;
            quad.BottomLeft += delta;

            Props[p].Quad = quad;

            if (Props[p].Type == InitPropType.Rope) {
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
            Effects[m].Item3 = Utils.Resize(Effects[m].Item3, width, height);
        }

        // Tile Matrix
        TileMatrix = Utils.Resize(
            TileMatrix,
            width,
            height,
            [
                new TileCell(),
                new TileCell(),
                new TileCell()
            ]
        );

        // Material Color Matrix
        MaterialColors = Utils.Resize(
            MaterialColors,
            width,
            height,
            [Color.Black, Color.Black, Color.Black]
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

public record struct TileInitLoadInfo(
    (string name, Color color)[] Categories,
    InitTile[][] Tiles,
    string LoadDirectory);



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


public interface IVariableInit { int Variations { get; } }
public interface IVariable { int Variation { get; set; } }

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
    public (string, EffectOptions[], double[,])[] Effects { get; init; } = [];

    public bool LightMode { get; init; }
    public bool DefaultTerrain { get; set; }
    public Data.Materials.Material DefaultMaterial { get; set; }

    public Geo[,,]? GeoMatrix { get; init; } = null;
    public TileCell[,,]? TileMatrix { get; init; } = null;
    public Color[,,]? MaterialColorMatrix { get; init; } = null;
    public Prop[]? PropsArray { get; init; } = null;

    public Image LightMapImage { get; init; }
    
    public (int angle, int flatness) LightSettings { get; init; }

    public List<RenderCamera> Cameras { get; set; } = [];

    public string Name { get; init; } = "New Project";

    public Exception? PropsLoadException { get; init; } = null;
}

/// Effect option for effects that apply to multiple layers
public enum EffectLayer1
{
    All,
    First,
    Second,
    Third,
    FirstAndSecond,
    SecondAndThird
};

/// Effect option for effects that apply to multiple layers
public enum EffectLayer2
{
    First,
    Second,
    Third,
};

public enum EffectColor
{
    Color1,
    Color2,
    Dead
}

public enum EffectFatness
{
    OnePixel,
    TwoPixels,
    ThreePixels,
    Random
}

public enum EffectSize
{
    Small,
    FAT
}

public enum EffectColored
{
    White,
    None
}

public class EffectOptions(string name, IEnumerable<string> options, dynamic choice)
{
    public string Name { get; set; } = name;
    public string[] Options { get; set; } = [.. options];
    public dynamic Choice { get; set; } = choice;
}

public record struct ConColor(
    byte R = 0,
    byte G = 0,
    byte B = 0,
    byte A = 255
    )
{

    public void Deconstruct(out byte r, out byte g, out byte b, out byte a)
    {
        r = R;
        g = G;
        b = B;
        a = A;
    }

    public static implicit operator ConColor(Color c) => new(c.R, c.G, c.B, c.A);
    public static implicit operator Color(ConColor c) => new(c.R, c.G, c.B, c.A);

    public static implicit operator Vector3(ConColor c) => new(c.R, c.G, c.B);
    public static implicit operator ConColor(Vector3 c) => new((byte)c.X, (byte)c.Y, (byte)c.Z);
    
    public static implicit operator Vector4(ConColor c) => new(c.R, c.G, c.B, c.A);
    public static implicit operator ConColor(Vector4 c) => new((byte)c.X, (byte)c.Y, (byte)c.Z, (byte)c.W);

    public static ConColor operator *(ConColor c, int i) => new(R: (byte)(c.R * i), G: (byte)(c.G * i), B: (byte)(c.B * i), A: (byte)(c.A * i));
    public static ConColor operator +(ConColor c, int i) => new(R: (byte)(c.R + i), G: (byte)(c.G + i), B: (byte)(c.B + i), A: (byte)(c.A + i));
    public static ConColor operator -(ConColor c, int i) => new(R: (byte)(c.R - i), G: (byte)(c.G - i), B: (byte)(c.B - i), A: (byte)(c.A - i));

}

// public struct Geo {
//     public int GeoType { get; set; } = 0;
//     public bool[] Stackables { get; set; } = new bool[22];

//     public Geo() {
//         GeoType = 0;
//         Stackables = new bool[22];
//     }

//     public Geo(int geo)
//     {
//         GeoType = geo;
//         Stackables = new bool[22];
//     }

//     public readonly override string ToString()
//     {
//         List<string> stc = new(22);

//         if (Stackables is null) {
//             stc.Add("NULL");
//         }
//         else {
//             for (var i = 0; i < Stackables.Length; i++) {
//                 if (Stackables[i]) stc.Add(i.ToString());
//             }
//         }

//         return $"[ {GeoType}, [ {string.Join(", ", stc)} ] ]";
//     }
// }

public enum TileType { Default, Material, TileHead, TileBody }


// TODO: de-dymic this 
public struct TileCell {
    public TileType Type { get; set; }
    public dynamic Data { get; set; }

    public TileCell()
    {
        Type = TileType.Default;
        Data = new TileDefault();
    }

    public TileCell(TileCell toCopy) {
        Type = toCopy.Type;
        Data = toCopy.Data switch {
            TileDefault => new TileDefault(),
            TileMaterial m => new TileMaterial(m.Name),
            TileHead h => new TileHead(h.Definition) { Name = h.Name },
            TileBody b => new TileBody(b.HeadPosition.x, b.HeadPosition.y, b.HeadPosition.z),
            _ => new TileDefault()
        };
    }

    public readonly override string ToString() => Data.ToString();
}


public struct TileDefault
{
    public override string ToString() => $"TileDefault";
}

public struct TileMaterial(string material)
{
    public string Name { get; set; } = material;

    public override string ToString() => $"TileMaterial(\"{Name}\")";
}

public struct TileHead
{
    // It may seem redundant, but it's necessary for exportation.
    public string Name { get; set; }
    
    public TileDefinition? Definition { get; set; }

    public TileHead(TileDefinition? definition)
    {
        Definition = definition;
    }

    public override string ToString() => $"TileHead({(Definition is null ? "Undefined" : $"\"{Definition.Name}\"")})";
}

public struct TileBody(int x, int y, int z)
{
    private (int x, int y, int z) _data = (x, y, z);
    public (int x, int y, int z) HeadPosition
    {
        get => _data;
        set { _data = value; }
    }

    public override string ToString() => $"TileBody({_data.x-1}, {_data.y-1}, {_data.z-1})";
}

public enum InitTileType { 
    Box, 
    VoxelStruct, 
    VoxelStructRandomDisplaceHorizontal,
    VoxelStructRandomDisplaceVertical,
    VoxelStructRockType,
    VoxelStructSandType
}

public readonly record struct InitTile(
    string Name,
    (int, int) Size,
    int[] Specs,
    int[] Specs2,
    int[] Specs3,
    InitTileType Type,
    int[] Repeat,
    int BufferTiles,
    int Rnd,
    int PtPos,
    string[] Tags
);

#region InitProp

public enum InitPropType
{
    Standard,
    VariedStandard,
    VariedSoft,
    VariedDecal,
    Soft,
    SoftEffect,
    SimpleDecal,
    Antimatter,
    ColoredSoft,
    Rope,
    Long,
    Tile
}

public enum InitPropColorTreatment { Standard, Bevel }


public abstract class InitPropBase(string name, InitPropType type, int depth)
{
    public string Name { get; init; } = name;
    public InitPropType Type { get; init; } = type;
    public int Depth { get; init; } = depth;

    public override string ToString() => 
        $"({Type}) - {Name}\n" +
        $"Depth: {Depth}";

    public abstract BasicPropSettings NewSettings();
}

public class InitStandardProp(
    string name, 
    InitPropType type,
    int depth,
    InitPropColorTreatment colorTreatment,
    int bevel,
    (int, int) size,
    int[] repeat) : InitPropBase(name, type, depth)
{
    public InitPropColorTreatment ColorTreatment { get; init; } = colorTreatment;
    public int Bevel { get; init; } = bevel;
    public (int x, int y) Size { get; init; } = size;
    public int[] Repeat { get; init; } = repeat;

    public override string ToString() =>
        base.ToString() + 
        $"\nColor Treatment: {ColorTreatment}\n" +
        $"Bevel: {Bevel}\n" +
        $"Size: ({Size.x}, {Size.y})\n" +
        $"RepeatL: [ {string.Join(", ", Repeat)} ]";

    public override BasicPropSettings NewSettings()
        => new();
}

public class InitVariedStandardProp(
    string name,
    InitPropType type,
    int depth,
    InitPropColorTreatment colorTreatment,
    int bevel,
    (int, int) size,
    int[] repeat,
    int variations,
    int random) : InitStandardProp(name, type, depth, colorTreatment, bevel, size, repeat), IVariableInit
{
    public int Variations { get; set; } = variations;
    public int Random { get; set; } = random;

    public override string ToString() => base.ToString() +
        $"\nVariations: {Variations}\n" +
        $"Random: {Random}";
    
    public override BasicPropSettings NewSettings()
        => new PropVariedSettings();
}

public class InitSoftProp(
    string name,
    InitPropType type,
    int depth,
    int round,
    float contourExp,
    int selfShade,
    float highlightBorder,
    float depthAffectHilits,
    float shadowBorder,
    int smoothShading) : InitPropBase(name, type, depth)
{
    public int Round { get; init; } = round;
    public float ContourExp { get; init; } = contourExp;
    public int SelfShade { get; init; } = selfShade;
    public float HighlightBorder { get; init; } = highlightBorder;
    public float DepthAffectHilites { get; init; } = depthAffectHilits;
    public float ShadowBorder { get; init; } = shadowBorder;
    public int SmoothShading { get; init; } = smoothShading;

    public override string ToString() => base.ToString() +
        $"\nRound: {Round}\n" +
        $"ContourExp: {ContourExp}\n" +
        $"SelfShade: {SelfShade}\n" +
        $"HighlightBorder: {HighlightBorder}\n" +
        $"DepthAffectHilites: {DepthAffectHilites}\n" +
        $"ShadowBorder: {ShadowBorder}\n" +
        $"SmoothShading: {SmoothShading}";
    
    public override BasicPropSettings NewSettings()
        => new PropSoftSettings();
}

public class InitSoftEffectProp(
    string name,
    InitPropType type,
    int depth) : InitPropBase(name, type, depth)
{
    public override BasicPropSettings NewSettings()
        => new PropSoftEffectSettings();
}

public class InitVariedSoftProp(
    string name,
    InitPropType type,
    int depth,
    int round,
    float contourExp,
    int selfShade,
    float highlightBorder,
    float depthAffectHilits,
    float shadowBorder,
    int smoothShading,
    (int x, int y) sizeInPixels,
    int variations,
    int random,
    int colorize) : InitSoftProp(name, type, depth, round, contourExp, selfShade, highlightBorder, depthAffectHilits, shadowBorder, smoothShading), IVariableInit
{
    public (int x, int y) SizeInPixels { get; init; } = sizeInPixels;
    public int Variations { get; init; } = variations;
    public int Random { get; init; } = random;
    public int Colorize { get; init; } = colorize;

    public override string ToString() => base.ToString() +
                                         $"\nSize in Pixels: ({SizeInPixels.x}, {SizeInPixels.y})\n" +
                                         $"Variations: {Variations}\n" +
                                         $"Random: {Random}\n" +
                                         $"Colorize: {Colorize}";
    
    public override BasicPropSettings NewSettings()
        => new PropVariedSoftSettings();
}

public class InitSimpleDecalProp(string name, InitPropType type, int depth) : InitPropBase(name, type, depth)
{
    public override BasicPropSettings NewSettings()
        => new PropSimpleDecalSettings();
}

public class InitVariedDecalProp(string name, InitPropType type, int depth, (int x, int y) sizeInPixels, int variations, int random) : InitSimpleDecalProp(name, type, depth), IVariableInit
{
    public (int x, int y) SizeInPixels { get; init; } = sizeInPixels;
    public int Variations { get; init; } = variations;
    public int Random { get; init; } = random;

    public override string ToString() => base.ToString() +
        $"\nSize in Pixels: ({SizeInPixels.x}, {SizeInPixels.y})\n" +
        $"Variations: {Variations}\n" +
        $"Random: {Random}";
    
    public override BasicPropSettings NewSettings()
        => new PropVariedDecalSettings();
}

public class InitAntimatterProp(string name, InitPropType type, int depth, float contourExp) : InitPropBase(name, type, depth)
{
    public float ContourExp { get; init; } = contourExp;

    public override string ToString() => base.ToString() +
        $"\nContourExp: {ContourExp}";
    
    public override BasicPropSettings NewSettings()
        => new PropAntimatterSettings();
}

public class InitRopeProp(
    string name,
    InitPropType type,
    int depth,
    int segmentLength,
    int collisionDepth,
    float segmentRadius,
    float gravity,
    float friction,
    float airFriction,
    bool stiff,
    Color previewColor,
    int previewEvery,
    float edgeDirection,
    float rigid,
    float selfPush,
    float sourcePush) : InitPropBase(name, type, depth)
{
    public int SegmentLength { get; } = segmentLength;
    public int CollisionDepth { get; } = collisionDepth;
    public float SegmentRadius { get; } = segmentRadius;
    public float Gravity { get; } = gravity;
    public float Friction { get; } = friction;
    public float AirFriction { get; } = airFriction;
    public bool Stiff { get; } = stiff;
    public Color PreviewColor { get; } = previewColor;
    public int PreviewEvery { get; } = previewEvery;
    public float EdgeDirection { get; } = edgeDirection;
    public float Rigid { get; } = rigid;
    public float SelfPush { get; } = selfPush;
    public float SourcePush { get; } = sourcePush;

    public override string ToString() => base.ToString() + 
                                         $"\nSegment Length: {SegmentLength}\n" +
                                         $"Collision Depth: {CollisionDepth}\n" +
                                         $"Segment Radius: {SegmentRadius}\n" +
                                         $"Gravity: {Gravity}\n" +
                                         $"Friction: {Friction}\n" +
                                         $"Air Friction: {AirFriction}\n" +
                                         $"Stiff: {Stiff}\n" +
                                         $"Preview Color: {PreviewColor}\n" +
                                         $"Preview Every: {PreviewEvery}\n" +
                                         $"Edge Direction: {EdgeDirection}\n" +
                                         $"Rigid: {Rigid}\n" +
                                         $"Self Push: {SelfPush}\n" +
                                         $"Source Push: {SourcePush}";
    
    public override BasicPropSettings NewSettings()
        => new PropRopeSettings();
}

public class InitLongProp(string name, InitPropType type, int depth) : InitPropBase(name, type, depth)
{
    public override BasicPropSettings NewSettings()
        => new PropLongSettings();
}

public class InitColoredSoftProp(
    string name, 
    InitPropType type, 
    int depth,
    (int x, int y) sizeInPixels,
    int round,
    float contourExp,
    int selfShade,
    float highlightBorder,
    float depthAffectHilites,
    float shadowBorder,
    int smoothShading,
    int colorize
    ) : InitPropBase(name, type, depth)
{
    public (int x, int y) SizeInPixels { get; init; } = sizeInPixels;
    public int Round { get; init; } = round;
    public float ContourExp { get; init; } = contourExp;
    public int SelfShade { get; init; } = selfShade;
    public float HighlightBorder { get; init; } = highlightBorder;
    public float DepthAffectHilites { get; init; } = depthAffectHilites;
    public float ShadowBorder { get; init; } = shadowBorder;
    public int SmoothShading { get; init; } = smoothShading;
    public int Colorize { get; init; } = colorize;
    
    public override BasicPropSettings NewSettings()
        => new();
}
#endregion

public struct PropQuad(
    Vector2 topLeft, 
    Vector2 topRight, 
    Vector2 bottomRight, 
    Vector2 bottomLeft)
{
    public Vector2 TopLeft { get; set; } = topLeft;
    public Vector2 TopRight { get; set; } = topRight;
    public Vector2 BottomRight { get; set; } = bottomRight;
    public Vector2 BottomLeft { get; set; } = bottomLeft;

    public PropQuad(PropQuad quad) : this(quad.TopLeft, quad.TopRight, quad.BottomRight, quad.BottomLeft) {}
}

public class Prop(int depth, string name, bool isTile, PropQuad quads)
{
    public InitPropType Type { get; set; }
    public TileDefinition? Tile { get; set; }
    public (int category, int index) Position { get; set; }
    public PropDefinition? Definition { get; set; }

    public int Depth { get; set; } = depth;
    public string Name { get; set; } = name;
    public bool IsTile { get; set; } = isTile;
    public float Rotation { get; set; }
    public PropQuad OriginalQuad { get; init; }
    public PropQuad Quad { get; set; } = quads;
    
    public PropExtras Extras { get; set; } = new(new BasicPropSettings(), []);
}

public class PropExtras(BasicPropSettings settings, Vector2[] ropePoints)
{
    public BasicPropSettings Settings { get; set; } = settings;
    public Vector2[] RopePoints { get; set; } = ropePoints;
}

#region PropSettings

public interface ICustomDepth {
    int CustomDepth { get; set; }
}

public interface IApplyColor {
    int? ApplyColor { get; set; }
}

public class BasicPropSettings(int renderOrder = 0, int seed = 0, int renderTime = 0)
{
    public int RenderOrder { get; set; } = renderOrder;
    public int Seed { get; set; } = seed;
    public int RenderTime { get; set; } = renderTime;

    public virtual BasicPropSettings Clone()
    {
        return new BasicPropSettings(RenderOrder, Seed, RenderTime);
    }
}

public class PropLongSettings(int renderOrder = 0, int seed = 0, int renderTime = 0) : BasicPropSettings(renderOrder, seed, renderTime);

public class PropVariedSettings(int renderOrder = 0, int seed = 200, int renderTime = 0, int variation = 0) : BasicPropSettings(renderOrder, seed, renderTime), IVariable
{
    public int Variation { get; set; } = variation;

    public override PropVariedSettings Clone()
    {
        return new PropVariedSettings(RenderOrder, Seed, RenderTime, Variation);
    }
}

public enum PropRopeRelease { Left, Right, None }

public class PropRopeSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, PropRopeRelease release = PropRopeRelease.None, float? thickness = null, int? applyColor = null) : BasicPropSettings(renderOrder, seed, renderTime), IApplyColor
{
    public PropRopeRelease Release { get; set; } = release;
    public float? Thickness { get; set; } = thickness;
    public int? ApplyColor { get; set; } = applyColor;

    public override PropRopeSettings Clone()
    {
        return new PropRopeSettings(RenderOrder, Seed, RenderTime, Release, Thickness, ApplyColor);
    }
}

public class PropVariedDecalSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int variation = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime), IVariable, ICustomDepth
{
    public int Variation { get; set; } = variation;
    public int CustomDepth { get; set; } = customDepth;

    public override PropVariedDecalSettings Clone()
    {
        return new PropVariedDecalSettings(RenderOrder, Seed, RenderTime, Variation, CustomDepth);
    }
}
public class PropVariedSoftSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int variation = 0, int customDepth = 0, int? applyColor = null) : BasicPropSettings(renderOrder, seed, renderTime), IVariable, ICustomDepth, IApplyColor
{
    public int Variation { get; set; } = variation;
    public int CustomDepth { get; set; } = customDepth;
    public int? ApplyColor { get; set; } = applyColor;

    public override PropVariedSoftSettings Clone()
    {
        return new PropVariedSoftSettings(RenderOrder, Seed, RenderTime, Variation, CustomDepth, ApplyColor);
    }
}

public class PropSimpleDecalSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime), ICustomDepth
{
    public int CustomDepth { get; set; } = customDepth;

    public override PropSimpleDecalSettings Clone()
    {
        return new PropSimpleDecalSettings(RenderOrder, Seed, RenderTime, CustomDepth);
    }
}

public class PropSoftSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime), ICustomDepth
{
    public int CustomDepth { get; set; } = customDepth;
    
    public override PropSoftSettings Clone()
    {
        return new PropSoftSettings(RenderOrder, Seed, RenderTime, CustomDepth);
    }
}
public class PropSoftEffectSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime), ICustomDepth
{
    public int CustomDepth { get; set; } = customDepth;
    
    public override PropSoftEffectSettings Clone()
    {
        return new PropSoftEffectSettings(RenderOrder, Seed, RenderTime, CustomDepth);
    }
}

public class PropAntimatterSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime), ICustomDepth
{
    public int CustomDepth { get; set; } = customDepth;
    
    public override PropAntimatterSettings Clone()
    {
        return new PropAntimatterSettings(RenderOrder, Seed, RenderTime, CustomDepth);
    }
}
#endregion

public readonly record struct BufferTiles(int Left, int Right, int Top, int Bottom);

public class CameraQuad(
    (int angle, float radius) topLeft, 
    (int angle, float radius) topRight, 
    (int angle, float radius) bottomRight, 
    (int angle, float radius) bottomLeft
) {
    public (int angle, float radius) TopLeft { get; set; } = topLeft; 
    public (int angle, float radius) TopRight { get; set; } = topRight;
    public (int angle, float radius) BottomRight { get; set; } = bottomRight; 
    public (int angle, float radius) BottomLeft { get; set; } = bottomLeft;
};

public record CameraQuadsRecord(
    (int Angle, float Radius) TopLeft, 
    (int Angle, float Radius) TopRight, 
    (int Angle, float Radius) BottomRight, 
    (int Angle, float Radius) BottomLeft
);

public class RenderCamera {
    public Vector2 Coords { get; set; }
    public CameraQuad Quad { get; set; }
}

