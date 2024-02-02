using System.Numerics;

namespace Leditor;

#nullable enable

/// Used to report the tile check status when loading a project
public enum TileCheckResult
{
    Ok, Missing, NotFound, MissingTexture, MissingMaterial
}

/// Used to report the tile check status when loading a project
public enum PropCheckResult
{
    Ok, Undefined, MissingTexture
}

public interface IVariableInit { int Variations { get; } }
public interface IVariable { int Variation { get; set; } }

// TODO: improve the success status reporting
/// Used for loading project files
public class LoadFileResult
{
    public bool Success { get; init; } = false;

    public int Width { get; init; } = 0;
    public int Height { get; init; } = 0;

    public BufferTiles BufferTiles { get; init; } = new();
    public (string, double[,])[] Effects { get; init; } = [];

    public RunCell[,,]? GeoMatrix { get; init; } = null;
    public TileCell[,,]? TileMatrix { get; init; } = null;
    public Color[,,]? MaterialColorMatrix { get; init; } = null;
    public (InitPropType type, (int category, int index) position, Prop prop)[]? PropsArray { get; init; } = null;

    public Image LightMapImage { get; init; }

    public List<RenderCamera> Cameras { get; set; } = [];

    public string Name { get; init; } = "New Project";
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

public class EffectOptions(
    bool? threeD = null,
    EffectLayer1? layers = null,
    EffectLayer2? layers2 = null,
    EffectColor? color = null,
    EffectFatness? fatness = null,
    EffectSize? size = null,
    EffectColored? colored = null,
    bool? affectGradientsAndDecals = null
)
{
    public bool? ThreeD { get; set; } = threeD;
    public EffectLayer1? Layers { get; set; } = layers;
    public EffectLayer2? Layers2 { get; set; } = layers2;
    public EffectColor? Color { get; set; } = color;
    public EffectFatness? Fatness { get; set; } = fatness;
    public EffectSize? Size { get; set; } = size;
    public EffectColored? Colored { get; set; } = colored;
    public int Seed { get; set; } = new Random().Next();
    public bool? AffectGradientsAndDecals { get; set; } = affectGradientsAndDecals;
}

// TODO: move this to GLOBALS
public static class Effects
{
    public static string[][] Names { get; } = [
        ["Slime", "Melt", "Rust", "Barnacles", "Rubble", "DecalsOnlySlime"],
        ["Roughen", "SlimeX3", "Super Melt", "Destructive Melt", "Erode", "Super Erode", "DaddyCorruption"],
        ["Wires", "Chains"],
        ["Root Grass", "Seed Pods", "Growers", "Cacti", "Rain Moss", "Hang Roots", "Grass"],
        ["Arm Growers", "Horse Tails", "Circuit Plants", "Feather Plants", "Thorn Growers", "Rollers", "Garbage Spirals"],
        ["Thick Roots", "Shadow Plants"],
        ["Fungi Flowers", "Lighthouse Flowers", "Fern", "Giant Mushroom", "Sprawlbush", "featherFern", "Fungus Tree"],
        ["BlackGoo", "DarkSlime"],
        ["Restore As Scaffolding", "Ceramic Chaos"],
        ["Colored Hang Roots", "Colored Thick Roots", "Colored Shadow Plants", "Colored Lighthouse Flowers", "Colored Fungi Flowers", "Root Plants"],
        ["Foliage", "Mistletoe", "High Fern", "High Grass", "Little Flowers", "Wastewater Mold"],
        ["Spinets", "Small Springs", "Mini Growers", "Clovers", "Reeds", "Lavenders", "Dense Mold"],
        ["Ultra Super Erode", "Impacts"],
        ["Super BlackGoo", "Stained Glass Properties"],
        ["Colored Barnacles", "Colored Rubble", "Fat Slime"],
        ["Assorted Trash", "Colored Wires", "Colored Chains", "Ring Chains"],
        ["Left Facing Kelp", "Right Facing Kelp", "Mixed Facing Kelp", "Bubble Grower", "Moss Wall", "Club Moss"],
        ["Ivy"],
        ["Fuzzy Growers"]
    ];

    public static string[] Categories { get; } = [
        "Natural",
        "Erosion",
        "Artificial",
        "Plants",
        "Plants2",
        "Plants3",
        "Plants (Individual)",
        "Paint Effects",
        "Restoration",
        "Drought Plants",
        "Drought Plants 2",
        "Drought Plants 3",
        "Drought Erosion",
        "Drought Paint Effects",
        "Drought Natural",
        "Drought Artificial",
        "Dakras Plants",
        "Leo Plants",
        "Nautillo Plants"
    ];

    public static int GetBrushStrength(string effect) => effect switch
    {
        "BlackGoo" or "Fungi Flowers" or "Lighthouse Flowers" or
        "Fern" or "Giant Mushroom" or "Sprawlbush" or
        "featherFern" or "Fungus Tree" or "Restore As Scaffolding" or "Restore As Pipes" => 100,

        _ => 10
    };

    // Used in the effects editor
    public static EffectOptions GetEffectOptions(string effect) => effect switch
    {
        "Slime" => new(threeD: false),
        "SlimeX3" => new(threeD: false),
        "Rust" => new(threeD: false),
        "Barnacles" => new(threeD: false),
        "Super Melt" => new(threeD: false),
        "Destructive Melt" => new(threeD: false),
        "Rubble" => new(layers: EffectLayer1.All),

        "Fungi Flowers"
        or
        "Lighthouse Flowers" => new(layers2: EffectLayer2.First),


        "Fern"
        or
        "Giant Mushroom"
        or
        "sprawlBush"
        or
        "featherFern"
        or
        "Fungus Tree" => new(layers2: EffectLayer2.First, color: EffectColor.Color2),


        "Root Grass"
        or
        "Growers"
        or
        "Cacti"
        or
        "Rain Moss"
        or
        "Seed Pods"
        or
        "Grass"
        or
        "Arm Growers"
        or
        "Horse Tails"
        or
        "Circuit Plants"
        or
        "Feather Plants" => new(layers: EffectLayer1.All, color: EffectColor.Color2),


        "Rollers"
        or
        "Thorn Growers"
        or
        "Garbage Spirals" => new(layers: EffectLayer1.All, color: EffectColor.Color2),


        "Wires" => new(layers: EffectLayer1.All, fatness: EffectFatness.TwoPixels),

        "Chains" => new(layers: EffectLayer1.All, size: EffectSize.Small),


        "Hang Roots"
        or
        "Thick Roots"
        or
        "Shadow Plants" => new(layers: EffectLayer1.All),


        "Restore As Scaffolding" => new(layers: EffectLayer1.All),

        "Restore As Pipes" => new(layers: EffectLayer1.All),

        "Ceramic Chaos" => new(colored: EffectColored.White),


        _ => new()
    };
}

public class GeoShortcuts
{
    public KeyboardShortcut ToRightGeo { get; set; } = new(KeyboardKey.KEY_D);
    public KeyboardShortcut ToLeftGeo { get; set; } = new(KeyboardKey.KEY_A);
    public KeyboardShortcut ToTopGeo { get; set; } = new(KeyboardKey.KEY_W);
    public KeyboardShortcut ToBottomGeo { get; set; } = new(KeyboardKey.KEY_S);
    public KeyboardShortcut CycleLayer { get; set; } = new(KeyboardKey.KEY_L);
    public KeyboardShortcut ToggleGrid { get; set; } = new(KeyboardKey.KEY_M);
    public KeyboardShortcut ShowCameras { get; set; } = new(KeyboardKey.KEY_C);

    public MouseShortcut Draw { get; set; } = new(MouseButton.MOUSE_BUTTON_LEFT, Hold:true);
    public MouseShortcut DragLevel { get; set; } = new(MouseButton.MOUSE_BUTTON_RIGHT, Hold:true);

    public KeyboardShortcut AltDraw { get; set; } = new(KeyboardKey.KEY_Z, Hold:true);
    public KeyboardShortcut AltDrag { get; set; } = new(KeyboardKey.KEY_F, Hold:true);
    public KeyboardShortcut Undo { get; set; } = new(Ctrl:true, Key:KeyboardKey.KEY_Z);
    public KeyboardShortcut Redo { get; set; } = new(Ctrl:true, Shift:true, Key:KeyboardKey.KEY_Z);
}

public record TileShortcuts
{
    public KeyboardKey FocusOnTileMenu { get; set; } = KeyboardKey.KEY_D;
    public KeyboardKey FocusOnTileCategoryMenu { get; set; } = KeyboardKey.KEY_A;
    public KeyboardKey MoveDown { get; set; } = KeyboardKey.KEY_S;
    public KeyboardKey MoveUp { get; set; } = KeyboardKey.KEY_W;
    public KeyboardKey CycleLayer { get; set; } = KeyboardKey.KEY_L;
    public KeyboardKey ToggleTileSpecs { get; set; } = KeyboardKey.KEY_T;

    public KeyboardKey ToggleLayer1 { get; set; } = KeyboardKey.KEY_Z;
    public KeyboardKey ToggleLayer2 { get; set; } = KeyboardKey.KEY_X;
    public KeyboardKey ToggleLayer3 { get; set; } = KeyboardKey.KEY_C;

    public KeyboardKey ToggleLayer1Tiles { get; set; } = KeyboardKey.KEY_Z;
    public KeyboardKey ToggleLayer2Tiles { get; set; } = KeyboardKey.KEY_X;
    public KeyboardKey ToggleLayer3Tiles { get; set; } = KeyboardKey.KEY_C;

    public MouseButton Draw { get; set; } = MouseButton.MOUSE_BUTTON_LEFT;
    public MouseButton DragLevel { get; set; } = MouseButton.MOUSE_BUTTON_RIGHT;
}

public class CameraShortcut
{
    public MouseButton DragButton { get; set; } = MouseButton.MOUSE_BUTTON_RIGHT;
    public KeyboardKey NewCamera { get; set; } = KeyboardKey.KEY_N;
    public KeyboardKey DeleteCamera { get; set; } = KeyboardKey.KEY_D;
    public KeyboardKey NewAndDelete { get; set; } = KeyboardKey.KEY_SPACE;
}

public class LightShortcuts
{
    public KeyboardKey IncreaseFlatness { get; set; } = KeyboardKey.KEY_I;
    public KeyboardKey DecreaseFlatness { get; set; } = KeyboardKey.KEY_K;
    public KeyboardKey IncreaseAngle { get; set; } = KeyboardKey.KEY_L;
    public KeyboardKey DecreaseAngle { get; set; } = KeyboardKey.KEY_J;

    public KeyboardKey NextBrush { get; set; } = KeyboardKey.KEY_F;
    public KeyboardKey PreviousBrush { get; set; } = KeyboardKey.KEY_R;

    public KeyboardKey RotateBrushCounterClockwise { get; set; } = KeyboardKey.KEY_Q;
    public KeyboardKey RotateBrushClockwise { get; set; } = KeyboardKey.KEY_E;

    public KeyboardKey StretchBrushVertically { get; set; } = KeyboardKey.KEY_W;
    public KeyboardKey SqueezeBrushVertically { get; set; } = KeyboardKey.KEY_S;
    public KeyboardKey StretchBrushHorizontally { get; set; } = KeyboardKey.KEY_D;
    public KeyboardKey SqueezeBrushHorizontally { get; set; } = KeyboardKey.KEY_A;

    public MouseButton DragLevel { get; set; } = MouseButton.MOUSE_BUTTON_RIGHT;
}

public class GlobalShortcuts
{
    public KeyboardShortcut ToMainPage { get; set; } = new(KeyboardKey.KEY_ONE);
    public KeyboardShortcut ToGeometryEditor { get; set; } = new(KeyboardKey.KEY_TWO);
    public KeyboardShortcut ToTileEditor { get; set; } = new(KeyboardKey.KEY_THREE);
    public KeyboardShortcut ToCameraEditor { get; set; } = new(KeyboardKey.KEY_FOUR);
    public KeyboardShortcut ToLightEditor { get; set; } = new(KeyboardKey.KEY_FIVE);
    public KeyboardShortcut ToDimensionsEditor { get; set; } = new(KeyboardKey.KEY_SIX);
    public KeyboardShortcut ToEffectsEditor { get; set; } = new(KeyboardKey.KEY_SEVEN);
    public KeyboardShortcut ToPropsEditor { get; set; } = new(KeyboardKey.KEY_EIGHT);
    public KeyboardShortcut ToSettingsPage { get; set; } = new(KeyboardKey.KEY_NINE);
}

public class Shortcuts(
    GlobalShortcuts globalShortcuts,
    GeoShortcuts geoEditor,
    TileShortcuts tileEditor,
    CameraShortcut cameraEditor,
    LightShortcuts lightEditor
)
{
    public GlobalShortcuts GlobalShortcuts { get; set; } = globalShortcuts;
    public GeoShortcuts GeoEditor { get; set; } = geoEditor;
    public TileShortcuts TileEditor { get; set; } = tileEditor;
    public CameraShortcut CameraEditor { get; set; } = cameraEditor;
    public LightShortcuts LightEditor { get; set; } = lightEditor;
}

public class Misc(
    bool splashScreen = true,
    int tileImageScansPerFrame = 20,
    int fps = 60,
    bool funnyDeathScreen = false
)
{
    public bool SplashScreen { get; set; } = splashScreen;
    public int TileImageScansPerFrame { get; set; } = tileImageScansPerFrame;
    public int FPS { get; set; } = fps;
    public bool FunnyDeathScreen { get; set; } = funnyDeathScreen;
}

public record ConColor(
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

    public static implicit operator ConColor(Color c) => new(c.r, c.g, c.b, c.a);
    public static implicit operator Color(ConColor c) => new(c.R, c.G, c.B, c.A);
}

public class LayerColors(ConColor layer1, ConColor layer2, ConColor layer3)
{
    public ConColor Layer1 { get; set; } = layer1;
    public ConColor Layer2 { get; set; } = layer2;
    public ConColor Layer3 { get; set; } = layer3;
}

public class GeoEditor(
    LayerColors layerColors, 
    bool legacyGeoTools = false,
    bool allowOutboundsPlacement = false,
    bool showCameras = false,
    bool showTiles = false
)
{
    public LayerColors LayerColors { get; set; } = layerColors;
    public bool LegacyGeoTools { get; set; } = legacyGeoTools;
    public bool ShowCameras { get; set; } = showCameras;
    public bool ShowTiles { get; set; } = showTiles;
    public bool AllowOutboundsPlacement { get; set; } = allowOutboundsPlacement;
}

public class TileEditor(
    bool visibleSpecs = true, 
    bool hoveredTileInfo = false, 
    bool tintedTiles = false, 
    bool useTextures = false
    )
{
    public bool VisibleSpecs { get; set; } = visibleSpecs;
    public bool HoveredTileInfo { get; set; } = hoveredTileInfo;
    public bool TintedTiles { get; set; } = tintedTiles;
    public bool UseTextures { get; set; } = useTextures;
}

public class LightEditor(ConColor background)
{
    public ConColor Background { get; set; } = background;
}

#region ShortcutSystem

public interface IShortcut { 
    bool Ctrl { get; } 
    bool Shift { get; }
    bool Check(bool ctrl = false, bool shift = false);
}

public record KeyboardShortcut(KeyboardKey Key, bool Ctrl = false, bool Shift = false, bool Hold = false) : IShortcut
{
    public static implicit operator KeyboardKey(KeyboardShortcut k) => k.Key;
    public bool Check(bool ctrl = false, bool shift = false)
    {
        return Ctrl == ctrl && Shift == shift && (Hold ? Raylib.IsKeyDown(Key) : Raylib.IsKeyPressed(Key));
    }
}

public record MouseShortcut(MouseButton Button, bool Ctrl = false, bool Shift = false, bool Hold = false) : IShortcut
{
    public static implicit operator MouseButton(MouseShortcut b) => b.Button;
    public bool Check(bool ctrl = false, bool shift = false)
    {
        return Ctrl == ctrl && 
               Shift == shift && 
               (Hold ? Raylib.IsMouseButtonDown(Button) : Raylib.IsMouseButtonPressed(Button));
    }
}

#endregion

public class Experimental(bool newGeometryEditor = false)
{
    public bool NewGeometryEditor { get; set; } = newGeometryEditor;
}

public class PropEditor(bool tintedTextures = false)
{
    public bool TintedTextures { get; set; } = tintedTextures;
}

public class Settings(
    bool developerMode,
    Shortcuts shortcuts,
    Misc misc,
    GeoEditor geometryEditor,
    TileEditor tileEditor,
    LightEditor lightEditor,
    PropEditor propEditor,
    Experimental experimental
)
{
    public bool DeveloperMode { get; set; } = developerMode;
    public Shortcuts Shortcuts { get; set; } = shortcuts;
    public Misc Misc { get; set; } = misc;
    public GeoEditor GeometryEditor { get; set; } = geometryEditor;
    public TileEditor TileEditor { get; set; } = tileEditor;
    public LightEditor LightEditor { get; set; } = lightEditor;
    public PropEditor PropEditor { get; set; } = propEditor;
    public Experimental Experimental { get; set; } = experimental;
}


public struct RunCell {
    public int Geo { get; set; } = 0;
    public bool[] Stackables { get; set; } = new bool[22];

    public RunCell() {
        Geo = 0;
        Stackables = new bool[22];
    }
}

public enum TileType { Default, Material, TileHead, TileBody }

public struct TileCell {
    public TileType Type { get; set; }
    public dynamic Data { get; set; }

    public TileCell()
    {
        Type = TileType.Default;
        Data = new TileDefault();
    }

    public readonly override string ToString() => Data.ToString();
}


public class TileDefault
{
    public int Value => 0;

    public override string ToString() => $"TileDefault";
}

public class TileMaterial(string material)
{
    private string _data = material;

    public string Name
    {
        get => _data;
        set { _data = value; }
    }

    public override string ToString() => $"TileMaterial(\"{_data}\")";
}

public class TileHead(int category, int position, string name)
{
    private (int, int, string) _data = (category, position, name);
    
    public (int, int, string) CategoryPostition
    {
        get => _data;
        set { _data = value; }
    }

    public override string ToString() => $"TileHead({_data.Item1}, {_data.Item2}, \"{_data.Item3}\")";
}

public class TileBody(int x, int y, int z)
{
    private (int x, int y, int z) _data = (x, y, z);
    public (int x, int y, int z) HeadPosition
    {
        get => _data;
        set { _data = value; }
    }

    public override string ToString() => $"TileBody({_data.x}, {_data.y}, {_data.z})";
}

public enum InitTileType { 
    Box, 
    VoxelStruct, 
    VoxelStructRandomDisplaceHorizontal,
    VoxelStructRandomDisplaceVertical,
    VoxelStructRockType,
    VoxelStructSandtype
}

public readonly record struct InitTile(
    string Name,
    (int, int) Size,
    int[] Specs,
    int[] Specs2,
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
}

public class InitSoftEffectProp(
    string name,
    InitPropType type,
    int depth) : InitPropBase(name, type, depth);

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
}

public class InitSimpleDecalProp(string name, InitPropType type, int depth) : InitPropBase(name, type, depth);

public class InitVariedDecalProp(string name, InitPropType type, int depth, (int x, int y) sizeInPixels, int variations, int random) : InitSimpleDecalProp(name, type, depth), IVariableInit
{
    public (int x, int y) SizeInPixels { get; init; } = sizeInPixels;
    public int Variations { get; init; } = variations;
    public int Random { get; init; } = random;

    public override string ToString() => base.ToString() +
        $"\nSize in Pixels: ({SizeInPixels.x}, {SizeInPixels.y})\n" +
        $"Variations: {Variations}\n" +
        $"Random: {Random}";
}

public class InitAntimatterProp(string name, InitPropType type, int depth, float contourExp) : InitPropBase(name, type, depth)
{
    public float ContourExp { get; init; } = contourExp;

    public override string ToString() => base.ToString() +
        $"\nContourExp: {ContourExp}";
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
}

public class InitLongProp(string name, InitPropType type, int depth) : InitPropBase(name, type, depth);
#endregion

public struct PropQuads(
    Vector2 topLeft, 
    Vector2 topRight, 
    Vector2 bottomRight, 
    Vector2 bottomLeft)
{
    public Vector2 TopLeft { get; set; } = topLeft;
    public Vector2 TopRight { get; set; } = topRight;
    public Vector2 BottomRight { get; set; } = bottomRight;
    public Vector2 BottomLeft { get; set; } = bottomLeft;
}

public class Prop(int depth, string name, bool isTile, (int, int) position, PropQuads quads)
{
    public int Depth { get; set; } = depth;
    public string Name { get; set; } = name;
    public bool IsTile { get; set; } = isTile;
    public (int category, int index) Position { get; set; } = position;
    public PropQuads Quads { get; set; } = quads;
    
    public PropExtras Extras { get; set; }
}

public class PropExtras(BasicPropSettings settings, Vector2[] ropePoints)
{
    public BasicPropSettings Settings { get; set; } = settings;
    public Vector2[] RopePoints { get; set; } = ropePoints;
}

#region PropSettings

public class BasicPropSettings(int renderOrder = 0, int seed = 0, int renderTime = 0)
{
    public int RenderOrder { get; set; } = renderOrder;
    public int Seed { get; set; } = seed;
    public int RenderTime { get; set; } = renderTime;
}

public class PropLongSettings(int renderOrder = 0, int seed = 0, int renderTime = 0) : BasicPropSettings(renderOrder, seed, renderTime);

public class PropVariedSettings(int renderOrder = 0, int seed = 200, int renderTime = 0, int variation = 0) : BasicPropSettings(renderOrder, seed, renderTime), IVariable
{
    public int Variation { get; set; } = variation;
}

public enum PropRopeRelease { Left, Right, None }

public class PropRopeSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, PropRopeRelease release = PropRopeRelease.None, float? thickness = null) : BasicPropSettings(renderOrder, seed, renderTime)
{
    public PropRopeRelease Release { get; set; } = release;
    public float? Thickness { get; set; } = thickness;
}

public class PropVariedDecalSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int variation = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime), IVariable
{
    public int Variation { get; set; } = variation;
    public int CustomDepth { get; set; } = customDepth;
}
public class PropVariedSoftSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int variation = 0, int customDepth = 0, bool applyColor = false) : BasicPropSettings(renderOrder, seed, renderTime), IVariable
{
    public int Variation { get; set; } = variation;
    public int CustomDepth { get; set; } = customDepth;
    public bool ApplyColor { get; set; } = applyColor;
}

public class PropSimpleDecalSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime)
{
    public int CustomDepth { get; set; } = customDepth;
}

public class PropSoftSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime)
{
    public int CustomDepth { get; set; } = customDepth;
}
public class PropSoftEffectSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime)
{
    public int CustomDepth { get; set; } = customDepth;
}

public class PropAntimatterSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime)
{
    public int CustomDepth { get; set; } = customDepth;
}
#endregion

public readonly record struct BufferTiles(int Left, int Right, int Top, int Bottom);

public class CameraQuads(
    Vector2 topLeft, 
    Vector2 topRight, 
    Vector2 bottomRight, 
    Vector2 bottomLeft
) {
    public Vector2 TopLeft { get; set; } = topLeft; 
    public Vector2 TopRight { get; set; } = topRight;
    public Vector2 BottomRight { get; set; } = bottomRight; 
    public Vector2 BottomLeft { get; set; } = bottomLeft;
};

public record CameraQuadsRecord(
    (int Angle, float Radius) TopLeft, 
    (int Angle, float Radius) TopRight, 
    (int Angle, float Radius) BottomRight, 
    (int Angle, float Radius) BottomLeft
);

public class RenderCamera {
    public Vector2 Coords { get; set; }
    public CameraQuads Quads { get; set; }
}

