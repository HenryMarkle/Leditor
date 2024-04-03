using System.Numerics;
using System.Text.Json.Serialization;

namespace Leditor;

#nullable enable

public class LevelLoadedEventArgs(bool undefinedTiles) : EventArgs
{
    public bool UndefinedTiles { get; set; } = undefinedTiles;
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

    internal RunCell[,,] GeoMatrix { get; private set; } = new RunCell[0, 0, 0];
    internal TileCell[,,] TileMatrix { get; private set; } = new TileCell[0, 0, 0];
    internal Color[,,] MaterialColors { get; private set; } = new Color[0, 0, 0];
    internal (string, EffectOptions[], double[,])[] Effects { get; set; } = [];
    internal (InitPropType type, (int category, int index) position, Prop prop)[] Props { get; set; } = [];

    internal string DefaultMaterial { get; set; } = "Concrete";
    
    internal string ProjectName { get; set; } = "New Project";
    
    internal int Seed { get; set; } = new Random().Next(1000);
    
    internal LevelState(int width, int height, (int left, int top, int right, int bottom) padding)
    {
        New(width, height, padding, [1, 1, 0]);
    }

    internal void Import(
        int width,
        int height,
        (int left, int top, int right, int bottom) padding,
        RunCell[,,] geoMatrix,
        TileCell[,,] tileMatrix,
        Color[,,] materialColorMatrix,
        (string, EffectOptions[], double[,])[] effects,
        List<RenderCamera> cameras,
        (InitPropType type, (int category, int index) position, Prop prop)[] props,
        (int angle, int flatness) lightSettings,
        bool lightMode,
        bool terrainMedium,
        int seed,
        int waterLevel,
        bool waterInFront,
        string defaultMaterial = "Concrete",
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
    }

    internal void New(
        int width,
        int height,
        (int left, int top, int right, int bottom) padding,
        Span<int> geoIdFill
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
            var matrix = new RunCell[height, width, 3];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    matrix[y, x, 0] = new() { Geo = geoIdFill[0], Stackables = new bool[22] };
                    matrix[y, x, 1] = new() { Geo = geoIdFill[1], Stackables = new bool[22] };
                    matrix[y, x, 2] = new() { Geo = geoIdFill[2], Stackables = new bool[22] };
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
        DefaultMaterial = "Concrete";
    }

    internal void Resize(
        int width,
        int height,
        (int left, int top, int right, int bottom) padding,
        ReadOnlySpan<int> geoIdFill
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
                new RunCell { Geo = geoIdFill[0], Stackables = new bool[22] },
                new RunCell { Geo = geoIdFill[1], Stackables = new bool[22] },
                new RunCell { Geo = geoIdFill[2], Stackables = new bool[22] }
            ]
        );

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

        Width = width;
        Height = height;
        Padding = padding;

        /*Border = new(
            Padding.left * Scale,
            Padding.top * Scale,
            (Width - (Padding.right + Padding.left)) * Scale,
            (Height - (Padding.bottom + Padding.top)) * Scale
        );*/
    }
}

public record struct TileInitLoadInfo(
    (string name, Color color)[] Categories,
    InitTile[][] Tiles,
    string LoadDirectory);

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
    
    public int Seed { get; set; }
    public int WaterLevel { get; set; }
    public bool WaterInFront { get; set; }

    public int Width { get; init; } = 0;
    public int Height { get; init; } = 0;

    public BufferTiles BufferTiles { get; init; } = new();
    public (string, EffectOptions[], double[,])[] Effects { get; init; } = [];

    public bool LightMode { get; init; }
    public bool DefaultTerrain { get; set; }
    public string DefaultMaterial { get; set; } = string.Empty;

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

public interface IEditorShortcuts
{
    IEnumerable<(string Name, string Shortcut)> CachedStrings { get; }
}

public class GeoShortcuts : IEditorShortcuts
{
    public KeyboardShortcut ToRightGeo { get; set; } = new(KeyboardKey.D);
    public KeyboardShortcut ToLeftGeo { get; set; } = new(KeyboardKey.A);
    public KeyboardShortcut ToTopGeo { get; set; } = new(KeyboardKey.W);
    public KeyboardShortcut ToBottomGeo { get; set; } = new(KeyboardKey.S);
    public KeyboardShortcut CycleLayers { get; set; } = new(KeyboardKey.L);
    public KeyboardShortcut ToggleGrid { get; set; } = new(KeyboardKey.M);
    public KeyboardShortcut ShowCameras { get; set; } = new(KeyboardKey.C);

    public MouseShortcut Draw { get; set; } = new(MouseButton.Left);
    public MouseShortcut DragLevel { get; set; } = new(MouseButton.Middle);

    public KeyboardShortcut AltDraw { get; set; } = new(KeyboardKey.Null);
    public KeyboardShortcut AltDrag { get; set; } = new(KeyboardKey.Null);
    public KeyboardShortcut Undo { get; set; } = new(ctrl:true, shift:false, key:KeyboardKey.Z);
    public KeyboardShortcut Redo { get; set; } = new(ctrl:true, shift:true, key:KeyboardKey.Z);

    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }

    public GeoShortcuts()
    {
        CachedStrings = Utils.GetShortcutStrings(this);
    }
}

public class ExperimentalGeoShortcuts : IEditorShortcuts
{
    public KeyboardShortcut ToRightGeo { get; set; } = new(KeyboardKey.D);
    public KeyboardShortcut ToLeftGeo { get; set; } = new(KeyboardKey.A);
    public KeyboardShortcut ToTopGeo { get; set; } = new(KeyboardKey.W);
    public KeyboardShortcut ToBottomGeo { get; set; } = new(KeyboardKey.S);
    public KeyboardShortcut CycleLayers { get; set; } = new(KeyboardKey.L);
    public KeyboardShortcut ToggleGrid { get; set; } = new(KeyboardKey.M);
    public KeyboardShortcut ShowCameras { get; set; } = new(KeyboardKey.K);
    public KeyboardShortcut ToggleMultiSelect { get; set; } = new(KeyboardKey.E);
    public KeyboardShortcut EraseEverything { get; set; } = new(KeyboardKey.X);
    
    public KeyboardShortcut ToggleTileVisibility { get; set; } = new(KeyboardKey.T);
    
    
    public KeyboardShortcut ToggleMemoryLoadMode { get; set; } = new(KeyboardKey.C, ctrl:true);
    public KeyboardShortcut ToggleMemoryDumbMode { get; set; } = new(KeyboardKey.V, ctrl:true);
    
    public MouseShortcut Draw { get; set; } = new(MouseButton.Left);
    public MouseShortcut Erase { get; set; } = new(MouseButton.Right);
    
    public MouseShortcut DragLevel { get; set; } = new(MouseButton.Middle);

    public KeyboardShortcut AltDraw { get; set; } = new(KeyboardKey.Null);
    public KeyboardShortcut AltErase { get; set; } = new(KeyboardKey.F);
    public KeyboardShortcut AltDragLevel { get; set; } = new(KeyboardKey.G);
    
    public KeyboardShortcut Undo { get; set; } = new(ctrl:true, shift:false, key:KeyboardKey.Z);
    public KeyboardShortcut Redo { get; set; } = new(ctrl:true, shift:true, key:KeyboardKey.Z);
    
    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }

    public ExperimentalGeoShortcuts()
    {
        CachedStrings = Utils.GetShortcutStrings(this);
    }
}

public record TileShortcuts : IEditorShortcuts
{
    public KeyboardShortcut FocusOnTileMenu { get; set; } = new(KeyboardKey.Null);
    public KeyboardShortcut FocusOnTileCategoryMenu { get; set; } = new(KeyboardKey.Null);
    public KeyboardShortcut MoveToNextCategory { get; set; } = new(KeyboardKey.D);
    public KeyboardShortcut MoveToPreviousCategory { get; set; } = new(KeyboardKey.A);
    
    public KeyboardShortcut MoveDown { get; set; } = new(KeyboardKey.S);
    public KeyboardShortcut MoveUp { get; set; } = new(KeyboardKey.W);
    public KeyboardShortcut CycleLayers { get; set; } = new(KeyboardKey.L);
    public KeyboardShortcut PickupItem { get; set; } = new(KeyboardKey.Q);
    public KeyboardShortcut ForcePlaceTileWithGeo { get; set; } = new(KeyboardKey.G);
    public KeyboardShortcut ForcePlaceTileWithoutGeo { get; set; } = new(KeyboardKey.F, shift:false);
    public KeyboardShortcut TileMaterialSwitch { get; set; } = new(KeyboardKey.M);
    public KeyboardShortcut HoveredItemInfo { get; set; } = new(KeyboardKey.P);

    public KeyboardShortcut Undo { get; set; } = new(KeyboardKey.Z, ctrl:true, shift:false);
    public KeyboardShortcut Redo { get; set; } = new(KeyboardKey.Z, ctrl:true, shift: true);

    public KeyboardShortcut CopyTiles { get; set; } = new(KeyboardKey.C, ctrl: true, shift: false);
    public KeyboardShortcut PasteTilesWithGeo { get; set; } = new(KeyboardKey.V, ctrl: true, shift: true);
    public KeyboardShortcut PasteTilesWithoutGeo { get; set; } = new(KeyboardKey.V, ctrl: true);

    public KeyboardShortcut ToggleLayer1 { get; set; } = new(KeyboardKey.Null, shift:false, ctrl: false);
    public KeyboardShortcut ToggleLayer2 { get; set; } = new(KeyboardKey.Null, shift:false, ctrl: false);
    public KeyboardShortcut ToggleLayer3 { get; set; } = new(KeyboardKey.Null, shift:false, ctrl: false);

    public KeyboardShortcut ToggleLayer1Tiles { get; set; } = new(KeyboardKey.Null, shift:true, ctrl: false);
    public KeyboardShortcut ToggleLayer2Tiles { get; set; } = new(KeyboardKey.Null, shift:true, ctrl: false);
    public KeyboardShortcut ToggleLayer3Tiles { get; set; } = new(KeyboardKey.Null, shift:true, ctrl: false);

    public KeyboardShortcut ResizeMaterialBrush { get; set; } = KeyboardKey.LeftAlt;

    public KeyboardShortcut TogglePathsView { get; set; } = new(KeyboardKey.O, ctrl: false, shift:false, alt: false);

    public KeyboardShortcut AltDraw { get; set; } = new(KeyboardKey.Null);
    public KeyboardShortcut AltErase { get; set; } = new(KeyboardKey.Null);
    public KeyboardShortcut AltDragLevel { get; set; } = new(KeyboardKey.Null);
    
    public MouseShortcut Draw { get; set; } = new(MouseButton.Left);
    public MouseShortcut Erase { get; set; } = new(MouseButton.Right);
    public MouseShortcut DragLevel { get; set; } = new(MouseButton.Middle);
    
    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }

    public TileShortcuts()
    {
        CachedStrings = Utils.GetShortcutStrings(this);
    }
}

public class CameraShortcuts : IEditorShortcuts
{
    public MouseShortcut DragLevel { get; set; } = MouseButton.Middle;
    public MouseShortcut GrabCamera { get; set; } = MouseButton.Left;
    public MouseShortcut CreateAndDeleteCamera { get; set; } = MouseButton.Right;
    public MouseShortcut ManipulateCamera { get; set; } = MouseButton.Left;
    
    public KeyboardShortcut ManipulateCameraAlt { get; set; } = KeyboardKey.Null;
    
    
    public KeyboardShortcut DragLevelAlt { get; set; } = KeyboardKey.G;
    public KeyboardShortcut GrabCameraAlt { get; set; } = KeyboardKey.P;
    public KeyboardShortcut CreateCamera { get; set; } = KeyboardKey.N;
    public KeyboardShortcut DeleteCamera { get; set; } = KeyboardKey.D;
    public KeyboardShortcut CreateAndDeleteCameraAlt { get; set; } = KeyboardKey.Space;
    
    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }

    public CameraShortcuts()
    {
        CachedStrings = Utils.GetShortcutStrings(this);
    }
}

public class LightShortcuts : IEditorShortcuts
{
    public KeyboardShortcut IncreaseFlatness { get; set; } = KeyboardKey.I;
    public KeyboardShortcut DecreaseFlatness { get; set; } = KeyboardKey.K;
    public KeyboardShortcut IncreaseAngle { get; set; } = KeyboardKey.J;
    public KeyboardShortcut DecreaseAngle { get; set; } = KeyboardKey.L;

    public KeyboardShortcut NextBrush { get; set; } = KeyboardKey.F;
    public KeyboardShortcut PreviousBrush { get; set; } = KeyboardKey.R;

    public KeyboardShortcut RotateBrushCounterClockwise { get; set; } = new(KeyboardKey.Q, shift:false);
    public KeyboardShortcut FastRotateBrushCounterClockwise { get; set; } = new(KeyboardKey.Q, shift:true);
    
    public KeyboardShortcut RotateBrushClockwise { get; set; } = new(KeyboardKey.E, shift:false);
    public KeyboardShortcut FastRotateBrushClockwise { get; set; } = new(KeyboardKey.E, shift:true);

    public KeyboardShortcut StretchBrushVertically { get; set; } = new(KeyboardKey.W, shift:false);
    public KeyboardShortcut FastStretchBrushVertically { get; set; } = new(KeyboardKey.W, shift:true);
    
    public KeyboardShortcut SqueezeBrushVertically { get; set; } = new(KeyboardKey.S, shift:false);
    public KeyboardShortcut FastSqueezeBrushVertically { get; set; } = new(KeyboardKey.S, shift:true);
    
    public KeyboardShortcut StretchBrushHorizontally { get; set; } = new (KeyboardKey.D, shift:false);
    public KeyboardShortcut FastStretchBrushHorizontally { get; set; } = new(KeyboardKey.D, shift:true);
    
    public KeyboardShortcut SqueezeBrushHorizontally { get; set; } = new (KeyboardKey.A, shift:false);
    public KeyboardShortcut FastSqueezeBrushHorizontally { get; set; } = new(KeyboardKey.A, shift:true);
    
    public KeyboardShortcut ToggleTileVisibility { get; set; } = KeyboardKey.T;

    public KeyboardShortcut ToggleTilePreview { get; set; } = KeyboardKey.P;
    public KeyboardShortcut ToggleTintedTileTextures { get; set; } = KeyboardKey.Y;


    public KeyboardShortcut DragLevelAlt { get; set; } = KeyboardKey.G;
    public KeyboardShortcut PaintAlt { get; set; } = KeyboardKey.V;
    public KeyboardShortcut EraseAlt { get; set; } = KeyboardKey.Null;
    
    public MouseShortcut DragLevel { get; set; } = MouseButton.Middle;
    public MouseShortcut Paint { get; set; } = MouseButton.Left;
    public MouseShortcut Erase { get; set; } = MouseButton.Right;
    
    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }

    public LightShortcuts()
    {
        CachedStrings = Utils.GetShortcutStrings(this);
    }
}

public class EffectsShortcuts : IEditorShortcuts
{
    public KeyboardShortcut NewEffect { get; set; } = KeyboardKey.N;

    public KeyboardShortcut NewEffectMenuCategoryNavigation { get; set; } = new(KeyboardKey.LeftShift, shift: true);    
    public KeyboardShortcut MoveDownInNewEffectMenu { get; set; } = new(KeyboardKey.S, shift:false, alt:false, ctrl:false);
    public KeyboardShortcut MoveUpInNewEffectMenu { get; set; } = new(KeyboardKey.W, shift:false, alt:false, ctrl:false);

    public KeyboardShortcut AcceptNewEffect { get; set; } = new(KeyboardKey.Space, shift:false, alt:false, ctrl:false);
    public KeyboardShortcut AcceptNewEffectAlt { get; set; } = KeyboardKey.Enter;

    public KeyboardShortcut ShiftAppliedEffectUp { get; set; } = new(KeyboardKey.W, shift: true, alt:false, ctrl:false);
    public KeyboardShortcut ShiftAppliedEffectDown { get; set; } = new(KeyboardKey.S, shift: true, alt:false, ctrl:false);

    public KeyboardShortcut CycleAppliedEffectUp { get; set; } = new(KeyboardKey.W, alt:false, shift:false, ctrl:false);
    public KeyboardShortcut CycleAppliedEffectDown { get; set; } = new(KeyboardKey.S, alt:false, shift:false, ctrl:false);

    public KeyboardShortcut DeleteAppliedEffect { get; set; } = KeyboardKey.X;
    
    public KeyboardShortcut CycleEffectOptionsUp { get; set; } = new(KeyboardKey.W, alt: true);
    public KeyboardShortcut CycleEffectOptionsDown { get; set; } = new(KeyboardKey.S, alt: true);
    
    public KeyboardShortcut CycleEffectOptionChoicesRight { get; set; } = new(KeyboardKey.D, alt: true);
    public KeyboardShortcut CycleEffectOptionChoicesLeft { get; set; } = new(KeyboardKey.A, alt: true);

    public KeyboardShortcut ToggleOptionsVisibility { get; set; } = KeyboardKey.O;

    public KeyboardShortcut StrongBrush { get; set; } = KeyboardKey.T;

    public KeyboardShortcut DragLevelAlt { get; set; } = KeyboardKey.G;
    public KeyboardShortcut PaintAlt { get; set; } = KeyboardKey.P;
    public KeyboardShortcut EraseAlt { get; set; } = KeyboardKey.Null;

    public KeyboardShortcut ResizeBrush { get; set; } = KeyboardKey.LeftAlt;
    
    public MouseShortcut DragLevel { get; set; } = MouseButton.Middle;
    public MouseShortcut Paint { get; set; } = MouseButton.Left;
    public MouseShortcut Erase { get; set; } = MouseButton.Right;
    
    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }

    public EffectsShortcuts()
    {
        CachedStrings = Utils.GetShortcutStrings(this);
    }
}

public class PropsShortcuts : IEditorShortcuts
{
    public KeyboardShortcut EscapeSpinnerControl { get; set; } = new(KeyboardKey.Escape, ctrl: false, shift: false, alt: false);
    public KeyboardShortcut CycleLayers { get; set; } = new(KeyboardKey.L, ctrl: false, shift: false, alt: false);

    public KeyboardShortcut CycleModeRight { get; set; } = new(KeyboardKey.E, shift:true);
    public KeyboardShortcut CycleModeLeft { get; set; } = new(KeyboardKey.Q, shift:true);

    public KeyboardShortcut CycleSnapMode { get; set; } = new(KeyboardKey.Null);

    public KeyboardShortcut ToggleLayer1 { get; set; } = new(KeyboardKey.Z, ctrl: false, shift: false, alt: false);
    public KeyboardShortcut ToggleLayer2 { get; set; } = new(KeyboardKey.X, ctrl: false, shift: false, alt: false);
    public KeyboardShortcut ToggleLayer3 { get; set; } = new(KeyboardKey.C, ctrl: false, shift: false, alt: false);

    public KeyboardShortcut ToggleLayer1Tiles { get; set; } = new(KeyboardKey.Z, shift:true);
    public KeyboardShortcut ToggleLayer2Tiles { get; set; } = new(KeyboardKey.X, shift:true);
    public KeyboardShortcut ToggleLayer3Tiles { get; set; } = new(KeyboardKey.C, shift:true);
    
    public KeyboardShortcut CycleCategoriesRight { get; set; } = new(KeyboardKey.D, shift: true);
    public KeyboardShortcut CycleCategoriesLeft { get; set; } = new(KeyboardKey.A, shift: true);

    public KeyboardShortcut ToNextInnerCategory { get; set; } = new(KeyboardKey.D, ctrl: false, shift: false, alt: false);
    public KeyboardShortcut ToPreviousInnerCategory { get; set; } = new(KeyboardKey.A, ctrl: false, shift: false, alt: false);
    
    public KeyboardShortcut NavigateMenuUp { get; set; } = new(KeyboardKey.W, ctrl: false, shift: false, alt: false);
    public KeyboardShortcut NavigateMenuDown { get; set; } = new(KeyboardKey.S, ctrl: false, shift: false, alt: false);

    public KeyboardShortcut PickupProp { get; set; } = new(KeyboardKey.Q, ctrl: false, shift: false, alt: false);
    
    public KeyboardShortcut PlacePropAlt { get; set; } = KeyboardKey.Null;
    
    public KeyboardShortcut DragLevelAlt { get; set; } = new(KeyboardKey.G, ctrl: false, shift: false, alt: false);

    
    public KeyboardShortcut ToggleMovingPropsMode { get; set; } = new(KeyboardKey.F, ctrl: false, shift: false, alt: false);
    public KeyboardShortcut ToggleRotatingPropsMode { get; set; } = new(KeyboardKey.R, ctrl: false, shift: false, alt: false);
    public KeyboardShortcut ToggleScalingPropsMode { get; set; } = new(KeyboardKey.S, ctrl: false, shift: false, alt: false);
    public KeyboardShortcut TogglePropsVisibility { get; set; } = new(KeyboardKey.H, ctrl: false, shift: false, alt: false);
    public KeyboardShortcut ToggleEditingPropQuadsMode { get; set; } = new(KeyboardKey.Q, ctrl: false, shift: false, alt: false);
    public KeyboardShortcut DeleteSelectedProps { get; set; } = new(KeyboardKey.D, ctrl: false, shift: false, alt: false);
    public KeyboardShortcut ToggleRopePointsEditingMode { get; set; } = new(KeyboardKey.P, ctrl: false, shift: false, alt: false);
    public KeyboardShortcut ToggleRopeEditingMode { get; set; } = new(KeyboardKey.B, ctrl: false, shift: false, alt: false);
    public KeyboardShortcut CycleSelected { get; set; } = new(KeyboardKey.Null);
    public KeyboardShortcut DuplicateProps { get; set; } = new(KeyboardKey.Null);
    
    public KeyboardShortcut DeepenSelectedProps { get; } = new(KeyboardKey.Null);
    public KeyboardShortcut UndeepenSelectedProps { get; } = new(KeyboardKey.Null);
    
    public KeyboardShortcut ToggleNoCollisionPropPlacement { get; set; } = KeyboardKey.Null;

    public KeyboardShortcut PropSelectionModifier { get; set; } = new(KeyboardKey.LeftControl, ctrl: true);
    
    public KeyboardShortcut SelectPropsAlt { get; set; } = KeyboardKey.Null;
    
    public MouseShortcut SelectProps { get; set; } = MouseButton.Left;

    public MouseShortcut PlaceProp { get; set; } = MouseButton.Left;
    
    public MouseShortcut DragLevel { get; set; } = MouseButton.Middle;
    
    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }

    public PropsShortcuts()
    {
        CachedStrings = Utils.GetShortcutStrings(this);
    }
}

public class GlobalShortcuts : IEditorShortcuts
{
    public KeyboardShortcut ToMainPage { get; set; } = new(KeyboardKey.One, shift:false, ctrl:false, alt:true);
    public KeyboardShortcut ToGeometryEditor { get; set; } = new(KeyboardKey.Two, shift:false, ctrl:false, alt:true);
    public KeyboardShortcut ToTileEditor { get; set; } = new(KeyboardKey.Three, shift:false, ctrl:false, alt:true);
    public KeyboardShortcut ToCameraEditor { get; set; } = new(KeyboardKey.Four, shift:false, ctrl:false, alt:true);
    public KeyboardShortcut ToLightEditor { get; set; } = new(KeyboardKey.Five, shift:false, ctrl:false, alt:true);
    public KeyboardShortcut ToDimensionsEditor { get; set; } = new(KeyboardKey.Six, shift:false, ctrl:false, alt:true);
    public KeyboardShortcut ToEffectsEditor { get; set; } = new(KeyboardKey.Seven, shift:false, ctrl:false, alt:true);
    public KeyboardShortcut ToPropsEditor { get; set; } = new(KeyboardKey.Eight, shift:false, ctrl:false, alt:true);
    public KeyboardShortcut ToSettingsPage { get; set; } = new(KeyboardKey.Nine, shift:false, ctrl:false, alt:true);

    public KeyboardShortcut QuickSave { get; set; } = new(KeyboardKey.S, shift:false, ctrl:true, alt:false);
    public KeyboardShortcut QuickSaveAs { get; set; } = new(KeyboardKey.S, shift:true, ctrl:true, alt:false);
    public KeyboardShortcut Render { get; set; } = new(KeyboardKey.Null);
    
    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }
    
    public GlobalShortcuts()
    {
        CachedStrings = Utils.GetShortcutStrings(this);
    }
}

public class Shortcuts
{
    public Shortcuts()
    {
        GlobalShortcuts = new GlobalShortcuts();
        GeoEditor = new GeoShortcuts();
        
        ExperimentalGeoShortcuts = new ExperimentalGeoShortcuts();
        TileEditor = new TileShortcuts();
        CameraEditor = new CameraShortcuts();
        LightEditor = new LightShortcuts();
        EffectsEditor = new EffectsShortcuts();
        PropsEditor = new PropsShortcuts();
    }
    
    public Shortcuts(
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
        GlobalShortcuts = globalShortcuts;
        GeoEditor = geoEditor;
        
        ExperimentalGeoShortcuts = experimentalGeoShortcuts;
        TileEditor = tileEditor;
        CameraEditor = cameraEditor;
        LightEditor = lightEditor;
        EffectsEditor = effectsEditor;
        PropsEditor = propsEditor;
    }
    
    public GlobalShortcuts GlobalShortcuts { get; set; }
    public GeoShortcuts GeoEditor { get; set; }
    
    public ExperimentalGeoShortcuts ExperimentalGeoShortcuts { get; set; }
    public TileShortcuts TileEditor { get; set; }
    public CameraShortcuts CameraEditor { get; set; }
    public LightShortcuts LightEditor { get; set; }
    public EffectsShortcuts EffectsEditor { get; set; }
    public PropsShortcuts PropsEditor { get; set; }
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

    public static ConColor operator *(ConColor c, int i) => new(R: (byte)(c.R * i), G: (byte)(c.G * i), B: (byte)(c.B * i), A: (byte)(c.A * i));
    public static ConColor operator +(ConColor c, int i) => new(R: (byte)(c.R + i), G: (byte)(c.G + i), B: (byte)(c.B + i), A: (byte)(c.A + i));
    public static ConColor operator -(ConColor c, int i) => new(R: (byte)(c.R - i), G: (byte)(c.G - i), B: (byte)(c.B - i), A: (byte)(c.A - i));

}

public class LayerColors(ConColor layer1, ConColor layer2, ConColor layer3)
{
    public ConColor Layer1 { get; set; } = layer1;
    public ConColor Layer2 { get; set; } = layer2;
    public ConColor Layer3 { get; set; } = layer3;
}

public class GeoEditor
{
    public GeoEditor()
    {
        LayerColors = new LayerColors(Color.Black, Color.Green, Color.Red);
        WaterColor = Color.Blue;
        LegacyGeoTools = false;
        ShowCameras = false;
        ShowTiles = false;
        AllowOutboundsPlacement = false;
        ShowCurrentGeoIndicator = true;
        LegacyInterface = false;
        PasteAir = false;
    }

    public GeoEditor(
        LayerColors layerColors,
        ConColor waterColor,
        bool legacyGeoTools = false,
        bool allowOutboundsPlacement = false,
        bool showCameras = false,
        bool showTiles = false,
        bool showCurrentGeoIndicator = true,
        bool legacyInterface = false,
        bool pasteAir = false
    )
    {
        LayerColors = layerColors;
        WaterColor = waterColor;
        LegacyGeoTools = legacyGeoTools;
        ShowCameras = showCameras;
        ShowTiles = showTiles;
        AllowOutboundsPlacement = allowOutboundsPlacement;
        ShowCurrentGeoIndicator = showCurrentGeoIndicator;
        LegacyInterface = legacyInterface;
        PasteAir = pasteAir;
    }
    
    public LayerColors LayerColors { get; set; }
    public ConColor WaterColor { get; set; }
    public bool LegacyGeoTools { get; set; }
    public bool ShowCameras { get; set; }
    public bool ShowTiles { get; set; }
    public bool AllowOutboundsPlacement { get; set; }
    public bool ShowCurrentGeoIndicator { get; set; }
    public bool LegacyInterface { get; set; }
    public bool PasteAir { get; set; }
}

public class TileEditor(
    bool hoveredTileInfo = false, 
    bool tintedTiles = false, 
    bool useTextures = false,
    bool allowUndefinedTiles = true,
    bool grid = false
    )
{
    public bool HoveredTileInfo { get; set; } = hoveredTileInfo;
    public bool TintedTiles { get; set; } = tintedTiles;
    public bool UseTextures { get; set; } = useTextures;
    public bool AllowUndefinedTiles { get; set; } = allowUndefinedTiles;
    public bool Grid { get; set; } = grid;
}

public class LightEditor(
    ConColor background, 
    ConColor levelBackgroundLight, 
    ConColor levelBackgroundDark
)
{
    public ConColor Background { get; set; } = background;
    public ConColor LevelBackgroundLight { get; set; } = levelBackgroundLight;
    public ConColor LevelBackgroundDark { get; set; } = levelBackgroundDark;
}

#region ShortcutSystem

public interface IShortcut { 
    bool? Ctrl { get; } 
    bool? Shift { get; }
    bool? Alt { get; }
    bool Check(bool ctrl = false, bool shift = false, bool alt = false, bool hold = false);
}

public class KeyboardShortcut(
    KeyboardKey key, 
    bool? ctrl = false, 
    bool? shift = false,
    bool? alt = false
) : IShortcut
{
    public KeyboardKey Key { get; set; } = key;
    public bool? Ctrl { get; set; } = ctrl;
    public bool? Shift { get; set; } = shift;
    public bool? Alt { get; set; } = alt;
    
    public static implicit operator KeyboardKey(KeyboardShortcut k) => k.Key;
    public static implicit operator KeyboardShortcut(KeyboardKey k) => new(k);
    
    public bool Check(bool ctrl = false, bool shift = false, bool alt = false, bool hold = false)
    {
        return (Ctrl is null || Ctrl == ctrl) && 
               (Shift is null || Shift == shift) && 
               (Alt is null || Alt == alt) && 
               (hold ? Raylib.IsKeyDown(Key) : Raylib.IsKeyPressed(Key));
    }

    public override string ToString()
    {
        return $"{(Ctrl is not null and not false ? "CTRL + " : "")}" +
               $"{(Shift is not null and not false ? "SHIFT + " : "")}" +
               $"{(Alt is not null and not false ? "ALT + " : "")}" +
               $"{Key switch {KeyboardKey.Space => "SPACE", KeyboardKey.Enter => "ENTER", KeyboardKey.LeftAlt => "ALT", KeyboardKey.LeftShift => "SHIT", KeyboardKey.LeftControl => "CTRL", KeyboardKey.Null => "NONE", KeyboardKey.Escape => "ESCAPE", var k => (char)k}}";
    }
}

public class MouseShortcut(MouseButton button, bool? ctrl = null, bool? shift = null, bool? alt = null) : IShortcut
{
    public MouseButton Button { get; set; } = button;
    public bool? Ctrl { get; set; } = ctrl;
    public bool? Shift { get; set; } = shift;
    public bool? Alt { get; set; } = alt;
    
    public static implicit operator MouseButton(MouseShortcut b) => b.Button;
    public static implicit operator MouseShortcut(MouseButton m) => new(m);

    private static string ButtonName(MouseButton button) => button switch
    {
        MouseButton.Left => "Left",
        MouseButton.Middle => "Middle",
        MouseButton.Right => "Right",
        MouseButton.Side => "Side",
        MouseButton.Back => "Back",
        _ => "UNKNOWN"
    };
    
    public bool Check(bool ctrl = false, bool shift = false, bool alt = false, bool hold = false)
    {
        return (Ctrl is null || Ctrl == ctrl) && (Shift is null || Shift == shift) && (Alt is null || Alt == alt) &&
               (hold ? Raylib.IsMouseButtonDown(Button) : Raylib.IsMouseButtonPressed(Button));
    }
    
    public override string ToString()
    {
        return $"{(Ctrl is not null and not false ? "CTRL + " : "")}{(Shift is not null and not false ? "SHIFT + " : "")}{(Alt is not null and not false ? "ALT + " : "")}{ButtonName(Button)}";
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

public class CameraEditorSettings(bool snap = true, bool alignment = false)
{
    public bool Snap { get; set; } = snap;
    public bool Alignment { get; set; } = alignment;
}

public class EffectsSettings(
    ConColor effectColorLight,
    ConColor effectColorDark)
{
    public ConColor EffectColorLight { get; set; } = effectColorLight;
    public ConColor EffectColorDark { get; set; } = effectColorDark;
}

public class GeneralSettings(
    bool developerMode = false, 
    bool defaultFont = false, 
    bool globalCamera = true,
    bool shortcutWindow = true,
    bool darkTheme = false,
    bool cacheRendererRuntime = false,
    bool linearZooming = false,
    float defaultZoom = 1,
    bool cycleMenus = true,
    bool changingCategoriesResetsIndex = true
    )
{
    public bool DeveloperMode { get; set; } = developerMode;
    public bool DefaultFont { get; set; } = defaultFont;
    public bool GlobalCamera { get; set; } = globalCamera;
    public bool ShortcutWindow { get; set; } = shortcutWindow;
    public bool DarkTheme { get; set; } = darkTheme;
    public bool CacheRendererRuntime { get; set; } = cacheRendererRuntime;
    public bool LinearZooming { get; set; } = linearZooming;
    public float DefaultZoom { get; set; } = defaultZoom;
    public bool CycleMenus { get; set; } = cycleMenus;
    public bool ChangingCategoriesResetsIndex { get; set; } = changingCategoriesResetsIndex;
}

public class Settings
{
    public GeneralSettings GeneralSettings { get; set; }
    public Shortcuts Shortcuts { get; set; }
    public Misc Misc { get; set; }
    public GeoEditor GeometryEditor { get; set; }
    public TileEditor TileEditor { get; set; }
    public CameraEditorSettings CameraSettings { get; set; }
    public LightEditor LightEditor { get; set; }
    public EffectsSettings EffectsSettings { get; set; }
    public PropEditor PropEditor { get; set; }
    public Experimental Experimental { get; set; }

    [JsonConstructor]
    public Settings()
    {
        GeneralSettings = new();
        Shortcuts = new();
        Misc = new();
        GeometryEditor = new(new LayerColors(Color.Black, Color.Green, Color.Red), Color.Blue);
        TileEditor = new();
        CameraSettings = new();
        LightEditor = new LightEditor(
            background: new ConColor(66, 108, 245, 255),
            levelBackgroundLight: Color.White,
            levelBackgroundDark: new Color(200, 0, 0, 255));
        EffectsSettings = new(Color.Green, Color.Yellow);
        PropEditor = new();
        Experimental = new();
    }
    
    public Settings(
        GeneralSettings generalSettings,
        Shortcuts shortcuts,
        Misc misc,
        GeoEditor geometryEditor,
        TileEditor tileEditor,
        CameraEditorSettings cameraEditorSettings,
        LightEditor lightEditor,
        EffectsSettings effectsSettings,
        PropEditor propEditor,
        Experimental experimental
    )
    {
         GeneralSettings = generalSettings;
         Shortcuts = shortcuts;
         Misc = misc;
         GeometryEditor = geometryEditor;
         TileEditor = tileEditor;
         CameraSettings = cameraEditorSettings;
         LightEditor = lightEditor;
         EffectsSettings = effectsSettings;
         PropEditor = propEditor;
         Experimental = experimental;
    }
}


public struct RunCell {
    public int Geo { get; set; } = 0;
    public bool[] Stackables { get; set; } = new bool[22];

    public static bool operator ==(RunCell c1, RunCell c2)
    {
        if (c1.Geo != c2.Geo) return false;

        if (c1.Stackables.Length != c2.Stackables.Length) return false;

        for (var i = 0; i < 22; i++)
        {
            if (c1.Stackables[i] != c2.Stackables[i]) return false;
        }

        return true;
    }

    public static bool operator !=(RunCell c1, RunCell c2) => !(c1 == c2);

    public RunCell() {
        Geo = 0;
        Stackables = new bool[22];
    }

    public RunCell(int geo)
    {
        Geo = geo;
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


public struct TileDefault
{
    public int Value => 0;

    public override string ToString() => $"TileDefault";
}

public struct TileMaterial(string material)
{
    private string _data = material;

    public string Name
    {
        get => _data;
        set { _data = value; }
    }

    public override string ToString() => $"TileMaterial(\"{_data}\")";
}

public struct TileHead(int category, int position, string name)
{
    private (int, int, string) _data = (category, position, name);
    
    public (int Category, int Index, string Name) CategoryPostition
    {
        get => _data;
        set { _data = value; }
    }

    public override string ToString() => $"TileHead({_data.Item1}, {_data.Item2}, \"{_data.Item3}\")";
}

public struct TileBody(int x, int y, int z)
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
    VoxelStructSandType
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

// Unused
public record TileDefinition(
    string Name,
    (int Width, int Height) Size,
    InitTileType Type,
    int BufferTiles,
    int Rnd,
    int PtPos,
    int[] Specs,
    int[] Specs2,
    int[] Specs3,
    int[] Repeat,
    string[] Tags
);
// {
//     public string Name { get; set; } = name;
//     public (int Width, int Height) Size { get; set; } = size;
//     public InitTileType Type { get; set; } = type;
//     public int BufferTiles { get; set; } = bufferTiles;
//     public int Rnd { get; set; } = rnd;
//     public int PtPos{ get; set; } = ptPos;
//     public int[] Specs{ get; set; } = specs;
//     public int[] Specs2{ get; set; } = specs2;
//     public int[] Specs3 { get; set; } = specs3;
//     public int[] Repeat { get; set; } = repeat;
//     public string[] Tags { get; set; } = tags;
//
//     public static bool operator ==(TileDefinition t1, TileDefinition t2) => t1.Name == t2.Name;
//     public static bool operator !=(TileDefinition t1, TileDefinition t2) => t1.Name != t2.Name;
// }

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
}
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

public class PropRopeSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, PropRopeRelease release = PropRopeRelease.None, float? thickness = null, int? applyColor = null) : BasicPropSettings(renderOrder, seed, renderTime)
{
    public PropRopeRelease Release { get; set; } = release;
    public float? Thickness { get; set; } = thickness;
    public int? ApplyColor { get; set; } = applyColor;

    public override PropRopeSettings Clone()
    {
        return new PropRopeSettings(RenderOrder, Seed, RenderTime, Release, Thickness, ApplyColor);
    }
}

public class PropVariedDecalSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int variation = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime), IVariable
{
    public int Variation { get; set; } = variation;
    public int CustomDepth { get; set; } = customDepth;

    public override PropVariedDecalSettings Clone()
    {
        return new PropVariedDecalSettings(RenderOrder, Seed, RenderTime, Variation, CustomDepth);
    }
}
public class PropVariedSoftSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int variation = 0, int customDepth = 0, int? applyColor = null) : BasicPropSettings(renderOrder, seed, renderTime), IVariable
{
    public int Variation { get; set; } = variation;
    public int CustomDepth { get; set; } = customDepth;
    public int? ApplyColor { get; set; } = applyColor;

    public override PropVariedSoftSettings Clone()
    {
        return new PropVariedSoftSettings(RenderOrder, Seed, RenderTime, Variation, CustomDepth, ApplyColor);
    }
}

public class PropSimpleDecalSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime)
{
    public int CustomDepth { get; set; } = customDepth;

    public override PropSimpleDecalSettings Clone()
    {
        return new PropSimpleDecalSettings(RenderOrder, Seed, RenderTime, CustomDepth);
    }
}

public class PropSoftSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime)
{
    public int CustomDepth { get; set; } = customDepth;
    
    public override PropSoftSettings Clone()
    {
        return new PropSoftSettings(RenderOrder, Seed, RenderTime, CustomDepth);
    }
}
public class PropSoftEffectSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime)
{
    public int CustomDepth { get; set; } = customDepth;
    
    public override PropSoftEffectSettings Clone()
    {
        return new PropSoftEffectSettings(RenderOrder, Seed, RenderTime, CustomDepth);
    }
}

public class PropAntimatterSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime)
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

