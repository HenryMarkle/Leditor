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
    public (string, EffectOptions[], double[,])[] Effects { get; init; } = [];

    public RunCell[,,]? GeoMatrix { get; init; } = null;
    public TileCell[,,]? TileMatrix { get; init; } = null;
    public Color[,,]? MaterialColorMatrix { get; init; } = null;
    public (InitPropType type, (int category, int index) position, Prop prop)[]? PropsArray { get; init; } = null;

    public Image LightMapImage { get; init; }
    
    public (int angle, int flatness) LightSettings { get; init; }

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

public class EffectOptions(string name, IEnumerable<string> options, dynamic choice)
{
    public string Name { get; set; } = name;
    public string[] Options { get; set; } = [.. options];
    public dynamic Choice { get; set; } = choice;
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

    public MouseShortcut Draw { get; set; } = new(MouseButton.MOUSE_BUTTON_LEFT);
    public MouseShortcut DragLevel { get; set; } = new(MouseButton.MOUSE_BUTTON_RIGHT);

    public KeyboardShortcut AltDraw { get; set; } = new(KeyboardKey.KEY_Z);
    public KeyboardShortcut AltDrag { get; set; } = new(KeyboardKey.KEY_F);
    public KeyboardShortcut Undo { get; set; } = new(Ctrl:true, Key:KeyboardKey.KEY_Z);
    public KeyboardShortcut Redo { get; set; } = new(Ctrl:true, Shift:true, Key:KeyboardKey.KEY_Z);
}

public class ExperimentalGeoShortcuts
{
    public KeyboardShortcut ToRightGeo { get; set; } = new(KeyboardKey.KEY_D);
    public KeyboardShortcut ToLeftGeo { get; set; } = new(KeyboardKey.KEY_A);
    public KeyboardShortcut ToTopGeo { get; set; } = new(KeyboardKey.KEY_W);
    public KeyboardShortcut ToBottomGeo { get; set; } = new(KeyboardKey.KEY_S);
    public KeyboardShortcut CycleLayer { get; set; } = new(KeyboardKey.KEY_L);
    public KeyboardShortcut ToggleGrid { get; set; } = new(KeyboardKey.KEY_M);
    public KeyboardShortcut ShowCameras { get; set; } = new(KeyboardKey.KEY_K);
    public KeyboardShortcut ToggleMultiSelect { get; set; } = new(KeyboardKey.KEY_E);
    public KeyboardShortcut EraseEverything { get; set; } = new(KeyboardKey.KEY_X);
    
    public KeyboardShortcut ToggleTileVisibility { get; set; } = new(KeyboardKey.KEY_T);
    
    
    public KeyboardShortcut ToggleMemoryLoadMode { get; set; } = new(KeyboardKey.KEY_C, Ctrl:true);
    public KeyboardShortcut ToggleMemoryDumbMode { get; set; } = new(KeyboardKey.KEY_V, Ctrl:true);
    
    public MouseShortcut Draw { get; set; } = new(MouseButton.MOUSE_BUTTON_LEFT);
    public MouseShortcut Erase { get; set; } = new(MouseButton.MOUSE_BUTTON_RIGHT);
    
    public MouseShortcut DragLevel { get; set; } = new(MouseButton.MOUSE_BUTTON_MIDDLE);

    public KeyboardShortcut DrawAlt { get; set; } = new(KeyboardKey.KEY_NULL);
    public KeyboardShortcut EraseAlt { get; set; } = new(KeyboardKey.KEY_F);
    public KeyboardShortcut DragLevelAlt { get; set; } = new(KeyboardKey.KEY_G);
    
    public KeyboardShortcut Undo { get; set; } = new(Ctrl:true, Shift:false, Key:KeyboardKey.KEY_Z);
    public KeyboardShortcut Redo { get; set; } = new(Ctrl:true, Shift:true, Key:KeyboardKey.KEY_Z);
}

public record TileShortcuts
{
    public KeyboardShortcut FocusOnTileMenu { get; set; } = new(KeyboardKey.KEY_D);
    public KeyboardShortcut FocusOnTileCategoryMenu { get; set; } = new(KeyboardKey.KEY_A);
    public KeyboardShortcut MoveDown { get; set; } = new(KeyboardKey.KEY_S);
    public KeyboardShortcut MoveUp { get; set; } = new(KeyboardKey.KEY_W);
    public KeyboardShortcut CycleLayer { get; set; } = new(KeyboardKey.KEY_L);
    public KeyboardShortcut ToggleTileSpecs { get; set; } = new(KeyboardKey.KEY_T);
    public KeyboardShortcut PickupItem { get; set; } = new(KeyboardKey.KEY_Q);
    public KeyboardShortcut ForcePlaceTileWithGeo { get; set; } = new(KeyboardKey.KEY_G);
    public KeyboardShortcut ForcePlaceTileWithoutGeo { get; set; } = new(KeyboardKey.KEY_F);
    public KeyboardShortcut TileMaterialSwitch { get; set; } = new(KeyboardKey.KEY_M);
    public KeyboardShortcut HoveredItemInfo { get; set; } = new(KeyboardKey.KEY_P);

    public KeyboardShortcut ToggleLayer1 { get; set; } = new(KeyboardKey.KEY_Z);
    public KeyboardShortcut ToggleLayer2 { get; set; } = new(KeyboardKey.KEY_X);
    public KeyboardShortcut ToggleLayer3 { get; set; } = new(KeyboardKey.KEY_C);

    public KeyboardShortcut ToggleLayer1Tiles { get; set; } = new(KeyboardKey.KEY_Z, Shift:true);
    public KeyboardShortcut ToggleLayer2Tiles { get; set; } = new(KeyboardKey.KEY_X, Shift:true);
    public KeyboardShortcut ToggleLayer3Tiles { get; set; } = new(KeyboardKey.KEY_C, Shift:true);

    public MouseShortcut Draw { get; set; } = new(MouseButton.MOUSE_BUTTON_LEFT);
    public MouseShortcut Erase { get; set; } = new(MouseButton.MOUSE_BUTTON_RIGHT);
    public MouseShortcut DragLevel { get; set; } = new(MouseButton.MOUSE_BUTTON_MIDDLE);
}

public class CameraShortcuts
{
    public MouseShortcut DragLevel { get; set; } = MouseButton.MOUSE_BUTTON_RIGHT;
    public MouseShortcut GrabCamera { get; set; } = MouseButton.MOUSE_BUTTON_LEFT;
    public MouseShortcut ManipulateCamera { get; set; } = MouseButton.MOUSE_BUTTON_LEFT;
    
    public KeyboardShortcut DragLevelAlt { get; set; } = KeyboardKey.KEY_G;
    public KeyboardShortcut GrabCameraAlt { get; set; } = KeyboardKey.KEY_P;
    public KeyboardShortcut CreateCamera { get; set; } = KeyboardKey.KEY_N;
    public KeyboardShortcut DeleteCamera { get; set; } = KeyboardKey.KEY_D;
    public KeyboardShortcut CreateAndDeleteCamera { get; set; } = KeyboardKey.KEY_SPACE;
}

public class LightShortcuts
{
    public KeyboardShortcut IncreaseFlatness { get; set; } = KeyboardKey.KEY_I;
    public KeyboardShortcut DecreaseFlatness { get; set; } = KeyboardKey.KEY_K;
    public KeyboardShortcut IncreaseAngle { get; set; } = KeyboardKey.KEY_L;
    public KeyboardShortcut DecreaseAngle { get; set; } = KeyboardKey.KEY_J;

    public KeyboardShortcut NextBrush { get; set; } = KeyboardKey.KEY_F;
    public KeyboardShortcut PreviousBrush { get; set; } = KeyboardKey.KEY_R;

    public KeyboardShortcut RotateBrushCounterClockwise { get; set; } = KeyboardKey.KEY_Q;
    public KeyboardShortcut FastRotateBrushCounterClockwise { get; set; } = new(KeyboardKey.KEY_Q, Shift:true);
    
    public KeyboardShortcut RotateBrushClockwise { get; set; } = KeyboardKey.KEY_E;
    public KeyboardShortcut FastRotateBrushClockwise { get; set; } = new(KeyboardKey.KEY_E, Shift:true);

    public KeyboardShortcut StretchBrushVertically { get; set; } = KeyboardKey.KEY_W;
    public KeyboardShortcut FastStretchBrushVertically { get; set; } = new(KeyboardKey.KEY_W, Shift:true);
    
    public KeyboardShortcut SqueezeBrushVertically { get; set; } = KeyboardKey.KEY_S;
    public KeyboardShortcut FastSqueezeBrushVertically { get; set; } = new(KeyboardKey.KEY_S, Shift:true);
    
    public KeyboardShortcut StretchBrushHorizontally { get; set; } = KeyboardKey.KEY_D;
    public KeyboardShortcut FastStretchBrushHorizontally { get; set; } = new(KeyboardKey.KEY_D, Shift:true);
    
    public KeyboardShortcut SqueezeBrushHorizontally { get; set; } = KeyboardKey.KEY_A;
    public KeyboardShortcut FastSqueezeBrushHorizontally { get; set; } = new(KeyboardKey.KEY_A, Shift:true);
    

    public KeyboardShortcut SlowWarpSpeed { get; set; } = KeyboardKey.KEY_SPACE;

    public KeyboardShortcut ToggleShadow { get; set; } = KeyboardKey.KEY_C;

    public KeyboardShortcut DragLevelAlt { get; set; } = KeyboardKey.KEY_G;
    public KeyboardShortcut PaintAlt { get; set; } = KeyboardKey.KEY_P;
    public MouseShortcut DragLevel { get; set; } = MouseButton.MOUSE_BUTTON_RIGHT;
    public MouseShortcut Paint { get; set; } = MouseButton.MOUSE_BUTTON_LEFT;
}

public class EffectsShortcuts
{
    public KeyboardShortcut NewEffect { get; set; } = KeyboardKey.KEY_N;

    public KeyboardShortcut NewEffectMenuCategoryNavigation { get; set; } = new(KeyboardKey.KEY_LEFT_SHIFT, Shift: true);    
    public KeyboardShortcut MoveDownInNewEffectMenu { get; set; } = KeyboardKey.KEY_S;
    public KeyboardShortcut MoveUpInNewEffectMenu { get; set; } = KeyboardKey.KEY_W;

    public KeyboardShortcut AcceptNewEffect { get; set; } = KeyboardKey.KEY_SPACE;
    public KeyboardShortcut AcceptNewEffectAlt { get; set; } = KeyboardKey.KEY_ENTER;

    public KeyboardShortcut ShiftAppliedEffectUp { get; set; } = new(KeyboardKey.KEY_W, Shift: true);
    public KeyboardShortcut ShiftAppliedEffectDown { get; set; } = new(KeyboardKey.KEY_S, Shift: true);

    public KeyboardShortcut CycleAppliedEffectUp { get; set; } = KeyboardKey.KEY_W;
    public KeyboardShortcut CycleAppliedEffectDown { get; set; } = KeyboardKey.KEY_S;

    public KeyboardShortcut DeleteAppliedEffect { get; set; } = KeyboardKey.KEY_X;
    
    public KeyboardShortcut CycleEffectOptionsUp { get; set; } = new(KeyboardKey.KEY_W, Alt: true);
    public KeyboardShortcut CycleEffectOptionsDown { get; set; } = new(KeyboardKey.KEY_S, Alt: true);
    
    public KeyboardShortcut CycleEffectOptionChoicesRight { get; set; } = new(KeyboardKey.KEY_D, Alt: true);
    public KeyboardShortcut CycleEffectOptionChoicesLeft { get; set; } = new(KeyboardKey.KEY_A, Alt: true);

    public KeyboardShortcut ToggleOptionsVisibility { get; set; } = KeyboardKey.KEY_O;
    public KeyboardShortcut ToggleBrushEraseMode { get; set; } = KeyboardKey.KEY_Q;

    public KeyboardShortcut DragLevelAlt { get; set; } = KeyboardKey.KEY_G;
    public KeyboardShortcut PaintAlt { get; set; } = KeyboardKey.KEY_P;
    
    public MouseShortcut DragLevel { get; set; } = MouseButton.MOUSE_BUTTON_RIGHT;
    public MouseShortcut Paint { get; set; } = MouseButton.MOUSE_BUTTON_LEFT;
}

public class PropsShortcuts
{
    public KeyboardShortcut EscapeSpinnerControl { get; set; } = KeyboardKey.KEY_ESCAPE;
    public KeyboardShortcut CycleLayers { get; set; } = KeyboardKey.KEY_L;

    public KeyboardShortcut CycleModeRight { get; set; } = new(KeyboardKey.KEY_E, Shift:true);
    public KeyboardShortcut CycleModeLeft { get; set; } = new(KeyboardKey.KEY_Q, Shift:true);

    public KeyboardShortcut ToggleLayer1 { get; set; } = KeyboardKey.KEY_Z;
    public KeyboardShortcut ToggleLayer2 { get; set; } = KeyboardKey.KEY_X;
    public KeyboardShortcut ToggleLayer3 { get; set; } = KeyboardKey.KEY_C;

    public KeyboardShortcut ToggleLayer1Tiles { get; set; } = new(KeyboardKey.KEY_Z, Shift:true);
    public KeyboardShortcut ToggleLayer2Tiles { get; set; } = new(KeyboardKey.KEY_X, Shift:true);
    public KeyboardShortcut ToggleLayer3Tiles { get; set; } = new(KeyboardKey.KEY_C, Shift:true);
    
    public KeyboardShortcut CycleCategoriesRight { get; set; } = new(KeyboardKey.KEY_D, Shift: true);
    public KeyboardShortcut CycleCategoriesLeft { get; set; } = new(KeyboardKey.KEY_A, Shift: true);

    public KeyboardShortcut InnerCategoryFocusRight { get; set; } = KeyboardKey.KEY_D;
    public KeyboardShortcut InnerCategoryFocusLeft { get; set; } = KeyboardKey.KEY_A;

    public KeyboardShortcut NavigateMenuUp { get; set; } = KeyboardKey.KEY_W;
    public KeyboardShortcut NavigateMenuDown { get; set; } = KeyboardKey.KEY_S;

    public KeyboardShortcut PickupProp { get; set; } = KeyboardKey.KEY_Q;
    
    public KeyboardShortcut PlacePropAlt { get; set; } = KeyboardKey.KEY_NULL;
    
    public KeyboardShortcut DraLevelAlt { get; set; } = KeyboardKey.KEY_G;

    
    public KeyboardShortcut ToggleMovingPropsMode { get; set; } = KeyboardKey.KEY_F;
    public KeyboardShortcut ToggleRotatingPropsMode { get; set; } = KeyboardKey.KEY_R;
    public KeyboardShortcut ToggleScalingPropsMode { get; set; } = KeyboardKey.KEY_S;
    public KeyboardShortcut TogglePropsVisibility { get; set; } = KeyboardKey.KEY_H;
    public KeyboardShortcut ToggleEditingPropQuadsMode { get; set; } = KeyboardKey.KEY_Q;
    public KeyboardShortcut DeleteSelectedProps { get; set; } = KeyboardKey.KEY_D;
    public KeyboardShortcut ToggleRopePointsEditingMode { get; set; } = KeyboardKey.KEY_P;
    public KeyboardShortcut ToggleRopeEditingMode { get; set; } = KeyboardKey.KEY_B;

    public KeyboardShortcut PropSelectionModifier { get; set; } = new(KeyboardKey.KEY_LEFT_CONTROL, Ctrl: true);
    
    public KeyboardShortcut SelectPropsAlt { get; set; } = KeyboardKey.KEY_NULL;
    
    public MouseShortcut SelectProps { get; set; } = MouseButton.MOUSE_BUTTON_LEFT;

    public MouseShortcut PlaceProp { get; set; } = MouseButton.MOUSE_BUTTON_LEFT;
    
    public MouseShortcut DragLevel { get; set; } = MouseButton.MOUSE_BUTTON_RIGHT;
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
    ExperimentalGeoShortcuts experimentalGeoShortcuts,
    TileShortcuts tileEditor,
    CameraShortcuts cameraEditor,
    LightShortcuts lightEditor,
    EffectsShortcuts effectsEditor,
    PropsShortcuts propsEditor
)
{
    public GlobalShortcuts GlobalShortcuts { get; set; } = globalShortcuts;
    public GeoShortcuts GeoEditor { get; set; } = geoEditor;
    
    public ExperimentalGeoShortcuts ExperimentalGeoShortcuts { get; set; } = experimentalGeoShortcuts;
    public TileShortcuts TileEditor { get; set; } = tileEditor;
    public CameraShortcuts CameraEditor { get; set; } = cameraEditor;
    public LightShortcuts LightEditor { get; set; } = lightEditor;
    public EffectsShortcuts EffectsEditor { get; set; } = effectsEditor;
    public PropsShortcuts PropsEditor { get; set; } = propsEditor;
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
    bool showTiles = true
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
    bool? Ctrl { get; } 
    bool? Shift { get; }
    bool? Alt { get; }
    bool Check(bool ctrl = false, bool shift = false, bool alt = false, bool hold = false);
}

public record KeyboardShortcut(
    KeyboardKey Key, 
    bool? Ctrl = null, 
    bool? Shift = null,
    bool? Alt = null
) : IShortcut
{
    public static implicit operator KeyboardKey(KeyboardShortcut k) => k.Key;
    public static implicit operator KeyboardShortcut(KeyboardKey k) => new(k);
    
    public bool Check(bool ctrl = false, bool shift = false, bool alt = false, bool hold = false)
    {
        return (Ctrl is null || Ctrl == ctrl) && (Shift is null || Shift == shift) && (Alt is null || Alt == alt) && (hold ? Raylib.IsKeyDown(Key) : Raylib.IsKeyPressed(Key));
    }
}

public record MouseShortcut(MouseButton Button, bool? Ctrl = null, bool? Shift = null, bool? Alt = null) : IShortcut
{
    public static implicit operator MouseButton(MouseShortcut b) => b.Button;
    public static implicit operator MouseShortcut(MouseButton m) => new(m);
    
    public bool Check(bool ctrl = false, bool shift = false, bool alt = false, bool hold = false)
    {
        return (Ctrl is null || Ctrl == ctrl) && (Shift is null || Shift == shift) && (Alt is null || Alt == alt) &&
               (hold ? Raylib.IsMouseButtonDown(Button) : Raylib.IsMouseButtonPressed(Button));
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

public class Prop(int depth, string name, bool isTile, PropQuads quads)
{
    public int Depth { get; set; } = depth;
    public string Name { get; set; } = name;
    public bool IsTile { get; set; } = isTile;
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

public class PropRopeSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, PropRopeRelease release = PropRopeRelease.None, float? thickness = null, int? applyColor = null) : BasicPropSettings(renderOrder, seed, renderTime)
{
    public PropRopeRelease Release { get; set; } = release;
    public float? Thickness { get; set; } = thickness;
    public int? ApplyColor { get; set; } = applyColor;
}

public class PropVariedDecalSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int variation = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime), IVariable
{
    public int Variation { get; set; } = variation;
    public int CustomDepth { get; set; } = customDepth;
}
public class PropVariedSoftSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int variation = 0, int customDepth = 0, int? applyColor = null) : BasicPropSettings(renderOrder, seed, renderTime), IVariable
{
    public int Variation { get; set; } = variation;
    public int CustomDepth { get; set; } = customDepth;
    public int? ApplyColor { get; set; } = applyColor;
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

