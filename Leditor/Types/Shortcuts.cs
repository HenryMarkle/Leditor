using System.Net.Mail;

namespace Leditor.Types;

public interface IShortcut
{
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
               $"{Key switch { KeyboardKey.Space => "SPACE", KeyboardKey.Enter => "ENTER", KeyboardKey.LeftAlt => "ALT", KeyboardKey.LeftShift => "SHIFT", KeyboardKey.LeftControl => "CTRL", KeyboardKey.Null => "NONE", KeyboardKey.Escape => "ESCAPE", var k => k }}";
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

//


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
    public KeyboardShortcut Undo { get; set; } = new(ctrl: true, shift: false, key: KeyboardKey.Z);
    public KeyboardShortcut Redo { get; set; } = new(ctrl: true, shift: true, key: KeyboardKey.Z);

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
    public KeyboardShortcut TempActivateMultiSelect { get; set; } = new(KeyboardKey.LeftShift, shift: true, ctrl: false, alt: false);
    public KeyboardShortcut EraseEverything { get; set; } = new(KeyboardKey.X);

    public KeyboardShortcut ToggleTileVisibility { get; set; } = new(KeyboardKey.T);
    public KeyboardShortcut TogglePropVisibility { get; set; } = new(KeyboardKey.P);

    public KeyboardShortcut ToggleMemoryLoadMode { get; set; } = new(KeyboardKey.C, ctrl: true);
    public KeyboardShortcut ToggleMemoryDumbMode { get; set; } = new(KeyboardKey.V, ctrl: true);

    public KeyboardShortcut PickupGeo { get; set; } = new(KeyboardKey.Q, ctrl: false, shift: false, alt: false);
    public KeyboardShortcut PickupStackable { get; set; } = new(KeyboardKey.Q, ctrl: false, shift: true, alt: false);

    public MouseShortcut Draw { get; set; } = new(MouseButton.Left);
    public MouseShortcut Erase { get; set; } = new(MouseButton.Right);

    public MouseShortcut DragLevel { get; set; } = new(MouseButton.Middle);

    public KeyboardShortcut AltDraw { get; set; } = new(KeyboardKey.Null);
    public KeyboardShortcut AltErase { get; set; } = new(KeyboardKey.F);
    public KeyboardShortcut AltDragLevel { get; set; } = new(KeyboardKey.G);

    public KeyboardShortcut Undo { get; set; } = new(ctrl: true, shift: false, key: KeyboardKey.Z);
    public KeyboardShortcut Redo { get; set; } = new(ctrl: true, shift: true, key: KeyboardKey.Z);

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
    public KeyboardShortcut ForcePlaceTileWithoutGeo { get; set; } = new(KeyboardKey.F, shift: false);
    public KeyboardShortcut TileMaterialSwitch { get; set; } = new(KeyboardKey.M);
    public KeyboardShortcut HoveredItemInfo { get; set; } = new(KeyboardKey.P);

    public KeyboardShortcut SetMaterialDefault { get; set; } = new(KeyboardKey.E, shift: false, ctrl: false, alt: false);
    public KeyboardShortcut ActivateSearch { get; set; } = new(KeyboardKey.F, ctrl: true, shift: false, alt: false);

    public KeyboardShortcut Undo { get; set; } = new(KeyboardKey.Z, ctrl: true, shift: false);
    public KeyboardShortcut Redo { get; set; } = new(KeyboardKey.Z, ctrl: true, shift: true);

    public KeyboardShortcut CopyTiles { get; set; } = new(KeyboardKey.C, ctrl: true, shift: false);
    public KeyboardShortcut PasteTilesWithGeo { get; set; } = new(KeyboardKey.V, ctrl: true, shift: true);
    public KeyboardShortcut PasteTilesWithoutGeo { get; set; } = new(KeyboardKey.V, ctrl: true);

    public KeyboardShortcut CycleAutoTilerMacroAxisPriority { get; set; } = new(KeyboardKey.Tab, ctrl: false, shift: false, alt: false);

    public KeyboardShortcut ToggleLayer1 { get; set; } = new(KeyboardKey.Null, shift: false, ctrl: false);
    public KeyboardShortcut ToggleLayer2 { get; set; } = new(KeyboardKey.Null, shift: false, ctrl: false);
    public KeyboardShortcut ToggleLayer3 { get; set; } = new(KeyboardKey.Null, shift: false, ctrl: false);

    public KeyboardShortcut ToggleLayer1Tiles { get; set; } = new(KeyboardKey.Null, shift: true, ctrl: false);
    public KeyboardShortcut ToggleLayer2Tiles { get; set; } = new(KeyboardKey.Null, shift: true, ctrl: false);
    public KeyboardShortcut ToggleLayer3Tiles { get; set; } = new(KeyboardKey.Null, shift: true, ctrl: false);

    public KeyboardShortcut ResizeMaterialBrush { get; set; } = KeyboardKey.LeftAlt;

    public KeyboardShortcut TogglePathsView { get; set; } = new(KeyboardKey.O, ctrl: false, shift: false, alt: false);

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
    private MouseShortcut dragLevel = MouseButton.Middle;
    private MouseShortcut grabCamera = MouseButton.Left;
    private MouseShortcut createAndDeleteCamera = MouseButton.Right;
    private MouseShortcut manipulateCamera = MouseButton.Left;
    private KeyboardShortcut manipulateCameraAlt = KeyboardKey.Null;
    private KeyboardShortcut dragLevelAlt = KeyboardKey.G;
    private KeyboardShortcut grabCameraAlt = KeyboardKey.P;
    private KeyboardShortcut createCamera = KeyboardKey.N;
    private KeyboardShortcut deleteCamera = KeyboardKey.D;
    private KeyboardShortcut createAndDeleteCameraAlt = KeyboardKey.Space;

    //

    [ShortcutName("Drag View")]
    public MouseShortcut DragLevel { get => dragLevel; set {dragLevel = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Grab Camera")]
    public MouseShortcut GrabCamera { get => grabCamera; set {grabCamera = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Create or Delete Camera")]
    public MouseShortcut CreateAndDeleteCamera { get => createAndDeleteCamera; set {createAndDeleteCamera = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Drag Camera Quad Handles")]
    public MouseShortcut ManipulateCamera { get => manipulateCamera; set {manipulateCamera = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Drag Camera Quad Handles")]
    public KeyboardShortcut ManipulateCameraAlt { get => manipulateCameraAlt; set {manipulateCameraAlt = value; CachedStrings = Utils.GetShortcutStrings(this);} }

    [ShortcutName("Drag View")]
    public KeyboardShortcut DragLevelAlt { get => dragLevelAlt; set {dragLevelAlt = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Grab Camera")]
    public KeyboardShortcut GrabCameraAlt { get => grabCameraAlt; set {grabCameraAlt = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Create Camera")]
    public KeyboardShortcut CreateCamera { get => createCamera; set {createCamera = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Delete Grabbed Camera")]
    public KeyboardShortcut DeleteCamera { get => deleteCamera; set {deleteCamera = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Create or Delete Camera")]
    public KeyboardShortcut CreateAndDeleteCameraAlt { get => createAndDeleteCameraAlt; set {createAndDeleteCameraAlt = value; CachedStrings = Utils.GetShortcutStrings(this);} }


    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }

    public CameraShortcuts()
    {
        CachedStrings = Utils.GetShortcutStrings(this);
    }
}

public class LightShortcuts : IEditorShortcuts
{
    private KeyboardShortcut increaseFlatness = KeyboardKey.I;
    private KeyboardShortcut decreaseFlatness = KeyboardKey.K;
    private KeyboardShortcut increaseAngle = KeyboardKey.J;
    private KeyboardShortcut decreaseAngle = KeyboardKey.L;
    private KeyboardShortcut nextBrush = KeyboardKey.F;
    private KeyboardShortcut previousBrush = KeyboardKey.R;
    private KeyboardShortcut rotateBrushCounterClockwise = new(KeyboardKey.Q, shift: false);
    private KeyboardShortcut fastRotateBrushCounterClockwise = new(KeyboardKey.Q, shift: true);
    private KeyboardShortcut rotateBrushClockwise = new(KeyboardKey.E, shift: false);
    private KeyboardShortcut fastRotateBrushClockwise = new(KeyboardKey.E, shift: true);
    private KeyboardShortcut stretchBrushVertically = new(KeyboardKey.W, shift: false);
    private KeyboardShortcut fastStretchBrushVertically = new(KeyboardKey.W, shift: true);
    private KeyboardShortcut squeezeBrushVertically = new(KeyboardKey.S, shift: false);
    private KeyboardShortcut fastSqueezeBrushVertically = new(KeyboardKey.S, shift: true);
    private KeyboardShortcut stretchBrushHorizontally = new(KeyboardKey.D, shift: false);
    private KeyboardShortcut fastStretchBrushHorizontally = new(KeyboardKey.D, shift: true);
    private KeyboardShortcut squeezeBrushHorizontally = new(KeyboardKey.A, shift: false);
    private KeyboardShortcut fastSqueezeBrushHorizontally = new(KeyboardKey.A, shift: true);
    private KeyboardShortcut toggleTileVisibility = KeyboardKey.T;
    private KeyboardShortcut toggleTilePreview = KeyboardKey.P;
    private KeyboardShortcut toggleTintedTileTextures = KeyboardKey.Y;
    private KeyboardShortcut dragLevelAlt = KeyboardKey.G;
    private KeyboardShortcut paintAlt = KeyboardKey.V;
    private KeyboardShortcut eraseAlt = KeyboardKey.Null;
    private MouseShortcut dragLevel = MouseButton.Middle;
    private MouseShortcut paint = MouseButton.Left;
    private MouseShortcut erase = MouseButton.Right;

    //

    [ShortcutName("Increase Flatness", Group = "Light Settings")]
    public KeyboardShortcut IncreaseFlatness { get => increaseFlatness; set {increaseFlatness = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Decrease Flatness", Group = "Light Settings")]
    public KeyboardShortcut DecreaseFlatness { get => decreaseFlatness; set {decreaseFlatness = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Increase Angle", Group = "Light Settings")]
    public KeyboardShortcut IncreaseAngle { get => increaseAngle; set {increaseAngle = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Decrease Angle", Group = "Light Settings")]
    public KeyboardShortcut DecreaseAngle { get => decreaseAngle; set {decreaseAngle = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Next Brush", Group = "Brush")]
    public KeyboardShortcut NextBrush { get => nextBrush; set {nextBrush = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Previous Brush", Group = "Brush")]
    public KeyboardShortcut PreviousBrush { get => previousBrush; set {previousBrush = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Rotate Counter-clockwise", Group = "Brush")]
    public KeyboardShortcut RotateBrushCounterClockwise { get => rotateBrushCounterClockwise; set {rotateBrushCounterClockwise = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Fast Rotate Counter-clockwise", Group = "Brush")]
    public KeyboardShortcut FastRotateBrushCounterClockwise { get => fastRotateBrushCounterClockwise; set {fastRotateBrushCounterClockwise = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Rotate Clockwise", Group = "Brush")]    
    public KeyboardShortcut RotateBrushClockwise { get => rotateBrushClockwise; set {rotateBrushClockwise = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Fast Rotate Clockwise", Group = "Brush")]    
    public KeyboardShortcut FastRotateBrushClockwise { get => fastRotateBrushClockwise; set {fastRotateBrushClockwise = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Stretch Vertically", Group = "Brush")]        
    public KeyboardShortcut StretchBrushVertically { get => stretchBrushVertically; set {stretchBrushVertically = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Fast Stretch Vertically", Group = "Brush")]        
    public KeyboardShortcut FastStretchBrushVertically { get => fastStretchBrushVertically; set {fastStretchBrushVertically = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Squeeze Vertically", Group = "Brush")]        
    public KeyboardShortcut SqueezeBrushVertically { get => squeezeBrushVertically; set {squeezeBrushVertically = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Fast Squeeze Vertically", Group = "Brush")]        
    public KeyboardShortcut FastSqueezeBrushVertically { get => fastSqueezeBrushVertically; set {fastSqueezeBrushVertically = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Stretch Horizontally", Group = "Brush")]        
    public KeyboardShortcut StretchBrushHorizontally { get => stretchBrushHorizontally; set {stretchBrushHorizontally = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Fast Stretch Horizontally", Group = "Brush")]        
    public KeyboardShortcut FastStretchBrushHorizontally { get => fastStretchBrushHorizontally; set {fastStretchBrushHorizontally = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Squeeze Horizontally", Group = "Brush")]        
    public KeyboardShortcut SqueezeBrushHorizontally { get => squeezeBrushHorizontally; set {squeezeBrushHorizontally = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Fast Squeeze Horizontally", Group = "Brush")]        
    public KeyboardShortcut FastSqueezeBrushHorizontally { get => fastSqueezeBrushHorizontally; set {fastSqueezeBrushHorizontally = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Show/Hide Tiles")]        
    public KeyboardShortcut ToggleTileVisibility { get => toggleTileVisibility; set {toggleTileVisibility = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Toggle Tile Preview")]        
    public KeyboardShortcut ToggleTilePreview { get => toggleTilePreview; set {toggleTilePreview = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Tint Tiles")]        
    public KeyboardShortcut ToggleTintedTileTextures { get => toggleTintedTileTextures; set {toggleTintedTileTextures = value; CachedStrings = Utils.GetShortcutStrings(this);} }

    
    [ShortcutName("Drag View")]
    public KeyboardShortcut DragLevelAlt { get => dragLevelAlt; set {dragLevelAlt = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Paint")]
    public KeyboardShortcut PaintAlt { get => paintAlt; set {paintAlt = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Erase")]
    public KeyboardShortcut EraseAlt { get => eraseAlt; set {eraseAlt = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Drag View")]
    public MouseShortcut DragLevel { get => dragLevel; set {dragLevel = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Paint")]
    public MouseShortcut Paint { get => paint; set {paint = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Erase")]
    public MouseShortcut Erase { get => erase; set {erase = value; CachedStrings = Utils.GetShortcutStrings(this);} }


    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }

    public LightShortcuts()
    {
        CachedStrings = Utils.GetShortcutStrings(this);
    }
}

public class EffectsShortcuts : IEditorShortcuts
{
    private KeyboardShortcut newEffect = KeyboardKey.N;
    private KeyboardShortcut moveDownInNewEffectMenu = new(KeyboardKey.S, shift: false, alt: false, ctrl: false);
    private KeyboardShortcut moveUpInNewEffectMenu = new(KeyboardKey.W, shift: false, alt: false, ctrl: false);
    private KeyboardShortcut moveCategoryDownInNewEffectsMenu = new(KeyboardKey.D, shift: false, alt: false, ctrl: false);
    private KeyboardShortcut moveCategoryUpInNewEffectsMenu = new(KeyboardKey.A, shift: false, alt: false, ctrl: false);
    private KeyboardShortcut acceptNewEffect = new(KeyboardKey.Space, shift: false, alt: false, ctrl: false);
    private KeyboardShortcut acceptNewEffectAlt = KeyboardKey.Enter;
    private KeyboardShortcut shiftAppliedEffectUp = new(KeyboardKey.W, shift: true, alt: false, ctrl: false);
    private KeyboardShortcut shiftAppliedEffectDown = new(KeyboardKey.S, shift: true, alt: false, ctrl: false);
    private KeyboardShortcut cycleAppliedEffectUp = new(KeyboardKey.W, alt: false, shift: false, ctrl: false);
    private KeyboardShortcut cycleAppliedEffectDown = new(KeyboardKey.S, alt: false, shift: false, ctrl: false);
    private KeyboardShortcut deleteAppliedEffect = KeyboardKey.X;
    private KeyboardShortcut cycleEffectOptionsUp = new(KeyboardKey.W, alt: true);
    private KeyboardShortcut cycleEffectOptionsDown = new(KeyboardKey.S, alt: true);
    private KeyboardShortcut cycleEffectOptionChoicesRight = new(KeyboardKey.D, alt: true);
    private KeyboardShortcut cycleEffectOptionChoicesLeft = new(KeyboardKey.A, alt: true);
    private KeyboardShortcut strongBrush = KeyboardKey.T;
    private KeyboardShortcut dragLevelAlt = KeyboardKey.G;
    private KeyboardShortcut paintAlt = KeyboardKey.P;
    private KeyboardShortcut eraseAlt = KeyboardKey.Null;
    private KeyboardShortcut resizeBrush = KeyboardKey.LeftAlt;
    private MouseShortcut dragLevel = MouseButton.Middle;
    private MouseShortcut paint = MouseButton.Left;
    private MouseShortcut erase = MouseButton.Right;

    //

    [ShortcutName("New Effect")]
    public KeyboardShortcut NewEffect { get => newEffect; set {newEffect = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Navigate Menu Down", Group = "New Effect Menu")]
    public KeyboardShortcut MoveDownInNewEffectMenu { get => moveDownInNewEffectMenu; set {moveDownInNewEffectMenu = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Navigate Menu Up", Group = "New Effect Menu")]
    public KeyboardShortcut MoveUpInNewEffectMenu { get => moveUpInNewEffectMenu; set {moveUpInNewEffectMenu = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Navigate Menu Categories Down", Group = "New Effect Menu")]
    public KeyboardShortcut MoveCategoryDownInNewEffectsMenu { get => moveCategoryDownInNewEffectsMenu; set {moveCategoryDownInNewEffectsMenu = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Navigate Menu Categories Up", Group = "New Effect Menu")]
    public KeyboardShortcut MoveCategoryUpInNewEffectsMenu { get => moveCategoryUpInNewEffectsMenu; set {moveCategoryUpInNewEffectsMenu = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Accept New Effect", Group = "New Effect Menu")]
    public KeyboardShortcut AcceptNewEffect { get => acceptNewEffect; set {acceptNewEffect = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Accept New Effect", Group = "New Effect Menu")]
    public KeyboardShortcut AcceptNewEffectAlt { get => acceptNewEffectAlt; set {acceptNewEffectAlt = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Shift Applied Effect Up", Group = "Applied Effect Menu")]
    public KeyboardShortcut ShiftAppliedEffectUp { get => shiftAppliedEffectUp; set {shiftAppliedEffectUp = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Shift Applied Effect Down", Group = "Applied Effect Menu")]
    public KeyboardShortcut ShiftAppliedEffectDown { get => shiftAppliedEffectDown; set {shiftAppliedEffectDown = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Navigate Applied Effects Up", Group = "Applied Effect Menu")]
    public KeyboardShortcut CycleAppliedEffectUp { get => cycleAppliedEffectUp; set {cycleAppliedEffectUp = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Shift Applied Effect Down", Group = "Applied Effect Menu")]
    public KeyboardShortcut CycleAppliedEffectDown { get => cycleAppliedEffectDown; set {cycleAppliedEffectDown = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Delete Applied Effect", Group = "Applied Effect Menu")]
    public KeyboardShortcut DeleteAppliedEffect { get => deleteAppliedEffect; set {deleteAppliedEffect = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Navigate Effect Options Up", Group = "Effect Options Menu")]
    public KeyboardShortcut CycleEffectOptionsUp { get => cycleEffectOptionsUp; set {cycleEffectOptionsUp = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Navigate Effect Options Down", Group = "Applied Options Menu")]
    public KeyboardShortcut CycleEffectOptionsDown { get => cycleEffectOptionsDown; set {cycleEffectOptionsDown = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    
    [ShortcutName("Navigate Effect Option Choices Down", Group = "Applied Options Menu")]
    public KeyboardShortcut CycleEffectOptionChoicesRight { get => cycleEffectOptionChoicesRight; set {cycleEffectOptionChoicesRight = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Navigate Effect Options Choices Up", Group = "Applied Options Menu")]    
    public KeyboardShortcut CycleEffectOptionChoicesLeft { get => cycleEffectOptionChoicesLeft; set {cycleEffectOptionChoicesLeft = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    
    [ShortcutName("Strong Brrush")]
    public KeyboardShortcut StrongBrush { get => strongBrush; set {strongBrush = value; CachedStrings = Utils.GetShortcutStrings(this);} }

    [ShortcutName("Drag View")]
    public KeyboardShortcut DragLevelAlt { get => dragLevelAlt; set {dragLevelAlt = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Paint")]
    public KeyboardShortcut PaintAlt { get => paintAlt; set {paintAlt = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Erase")]
    public KeyboardShortcut EraseAlt { get => eraseAlt; set {eraseAlt = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Resize Brush (+ Wheel)")]
    public KeyboardShortcut ResizeBrush { get => resizeBrush; set {resizeBrush = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Drag View")]
    public MouseShortcut DragLevel { get => dragLevel; set {dragLevel = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Paint")]
    public MouseShortcut Paint { get => paint; set {paint = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Erase")]
    public MouseShortcut Erase { get => erase; set {erase = value; CachedStrings = Utils.GetShortcutStrings(this);} }


    // VERY IMPORTANT: The setter must be private or it'll cause a segmentation fault!
    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }

    public EffectsShortcuts()
    {
        CachedStrings = Utils.GetShortcutStrings(this);
    }
}

public class PropsShortcuts : IEditorShortcuts
{
    private KeyboardShortcut cycleLayers = new(KeyboardKey.L, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut cycleModeRight = new(KeyboardKey.E, shift: true);
    private KeyboardShortcut cycleModeLeft = new(KeyboardKey.Q, shift: true);
    private KeyboardShortcut cycleSnapMode = new(KeyboardKey.Null);
    private KeyboardShortcut toggleLayer1 = new(KeyboardKey.Z, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut toggleLayer2 = new(KeyboardKey.X, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut toggleLayer3 = new(KeyboardKey.C, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut toggleLayer1Tiles = new(KeyboardKey.Z, shift: true);
    private KeyboardShortcut toggleLayer2Tiles = new(KeyboardKey.X, shift: true);
    private KeyboardShortcut toggleLayer3Tiles = new(KeyboardKey.C, shift: true);
    private KeyboardShortcut cycleCategoriesRight = new(KeyboardKey.D, shift: true);
    private KeyboardShortcut cycleCategoriesLeft = new(KeyboardKey.A, shift: true);
    private KeyboardShortcut toNextInnerCategory = new(KeyboardKey.D, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut toPreviousInnerCategory = new(KeyboardKey.A, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut navigateMenuUp = new(KeyboardKey.W, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut navigateMenuDown = new(KeyboardKey.S, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut pickupProp = new(KeyboardKey.Q, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut placePropAlt = KeyboardKey.Null;
    private KeyboardShortcut dragLevelAlt = new(KeyboardKey.G, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut toggleMovingPropsMode = new(KeyboardKey.F, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut toggleRotatingPropsMode = new(KeyboardKey.R, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut toggleScalingPropsMode = new(KeyboardKey.S, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut togglePropsVisibility = new(KeyboardKey.H, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut toggleEditingPropQuadsMode = new(KeyboardKey.Q, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut deleteSelectedProps = new(KeyboardKey.D, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut toggleRopePointsEditingMode = new(KeyboardKey.P, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut toggleRopeEditingMode = new(KeyboardKey.B, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut cycleSelected = new(KeyboardKey.Null);
    private KeyboardShortcut duplicateProps = new(KeyboardKey.Null);
    private KeyboardShortcut deepenSelectedProps = new(KeyboardKey.Null);
    private KeyboardShortcut undeepenSelectedProps = new(KeyboardKey.Null);
    private KeyboardShortcut rotateClockwise = new(KeyboardKey.Null);
    private KeyboardShortcut rotateCounterClockwise = new(KeyboardKey.Null);
    private KeyboardShortcut incrementRopSegmentCount = new(KeyboardKey.K);
    private KeyboardShortcut decrementRopSegmentCount = new(KeyboardKey.J);
    private KeyboardShortcut simulationBeizerSwitch = new(KeyboardKey.Null);
    private KeyboardShortcut fastRotateClockwise = new(KeyboardKey.Null);
    private KeyboardShortcut fastRotateCounterClockwise = new(KeyboardKey.Null);
    private KeyboardShortcut cycleVariations = new(KeyboardKey.V, shift: false, alt: false, ctrl: false);
    private KeyboardShortcut toggleNoCollisionPropPlacement = KeyboardKey.Null;
    private KeyboardShortcut propSelectionModifier = new(KeyboardKey.LeftControl, ctrl: true);
    private KeyboardShortcut selectPropsAlt = KeyboardKey.Null;
    private KeyboardShortcut undo = new(KeyboardKey.Z, ctrl: true, shift: false, alt: false);
    private KeyboardShortcut redo = new(KeyboardKey.Z, ctrl: true, shift: true, alt: false);
    private KeyboardShortcut activateSearch = new(KeyboardKey.F, ctrl: true, shift: false, alt: false);
    private MouseShortcut selectProps = MouseButton.Right;
    private MouseShortcut placeProp = MouseButton.Left;
    private MouseShortcut dragLevel = MouseButton.Middle;

    //

    [ShortcutName("Cycle Layers")]
    public KeyboardShortcut CycleLayers { get => cycleLayers; set { cycleLayers = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Cycle Mode")]
    public KeyboardShortcut CycleModeRight { get => cycleModeRight; set { cycleModeRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Reveres Cycle Mode")]
    public KeyboardShortcut CycleModeLeft { get => cycleModeLeft; set { cycleModeLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Cycle Snap Modes")]
    public KeyboardShortcut CycleSnapMode { get => cycleSnapMode; set { cycleSnapMode = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Toggle Layer 1")]
    public KeyboardShortcut ToggleLayer1 { get => toggleLayer1; set { toggleLayer1 = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Toggle Layer 2")]
    public KeyboardShortcut ToggleLayer2 { get => toggleLayer2; set { toggleLayer2 = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Toggle Layer 3")]
    public KeyboardShortcut ToggleLayer3 { get => toggleLayer3; set { toggleLayer3 = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Toggle Layer 1 Tiles")]
    public KeyboardShortcut ToggleLayer1Tiles { get => toggleLayer1Tiles; set { toggleLayer1Tiles = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Toggle Layer 2 Tiles")]
    public KeyboardShortcut ToggleLayer2Tiles { get => toggleLayer2Tiles; set { toggleLayer2Tiles = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Toggle Layer 3 Tiles")]
    public KeyboardShortcut ToggleLayer3Tiles { get => toggleLayer3Tiles; set { toggleLayer3Tiles = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Cycle Categories")]
    public KeyboardShortcut CycleCategoriesRight { get => cycleCategoriesRight; set { cycleCategoriesRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Reverse Cycle Categories")]
    public KeyboardShortcut CycleCategoriesLeft { get => cycleCategoriesLeft; set { cycleCategoriesLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Navigate Subcategory Down")]
    public KeyboardShortcut ToNextInnerCategory { get => toNextInnerCategory; set { toNextInnerCategory = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Navigate Subcategory Up")]
    public KeyboardShortcut ToPreviousInnerCategory { get => toPreviousInnerCategory; set { toPreviousInnerCategory = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Navigate Menu Up")]
    public KeyboardShortcut NavigateMenuUp { get => navigateMenuUp; set { navigateMenuUp = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Navigate Menu Down")]
    public KeyboardShortcut NavigateMenuDown { get => navigateMenuDown; set { navigateMenuDown = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Pickup Prop")]
    public KeyboardShortcut PickupProp { get => pickupProp; set { pickupProp = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Place Prop")]
    public KeyboardShortcut PlacePropAlt { get => placePropAlt; set { placePropAlt = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Drag View")]
    public KeyboardShortcut DragLevelAlt { get => dragLevelAlt; set { dragLevelAlt = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Move", Group = "Selected Prop Actions")]
    public KeyboardShortcut ToggleMovingPropsMode { get => toggleMovingPropsMode; set { toggleMovingPropsMode = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Rotate", Group = "Selected Prop Actions")]
    public KeyboardShortcut ToggleRotatingPropsMode { get => toggleRotatingPropsMode; set { toggleRotatingPropsMode = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Scale", Group = "Selected Prop Actions")]
    public KeyboardShortcut ToggleScalingPropsMode { get => toggleScalingPropsMode; set { toggleScalingPropsMode = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Hide/Show", Group = "Selected Prop Actions")]
    public KeyboardShortcut TogglePropsVisibility { get => togglePropsVisibility; set { togglePropsVisibility = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Edit Vertices", Group = "Selected Prop Actions")]
    public KeyboardShortcut ToggleEditingPropQuadsMode { get => toggleEditingPropQuadsMode; set { toggleEditingPropQuadsMode = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Delete", Group = "Selected Prop Actions")]
    public KeyboardShortcut DeleteSelectedProps { get => deleteSelectedProps; set { deleteSelectedProps = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Move Rope Segments", Group = "Selected Prop Actions")]
    public KeyboardShortcut ToggleRopePointsEditingMode { get => toggleRopePointsEditingMode; set { toggleRopePointsEditingMode = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Simulate Rope", Group = "Selected Prop Actions")]
    public KeyboardShortcut ToggleRopeEditingMode { get => toggleRopeEditingMode; set { toggleRopeEditingMode = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Cycle Selected", Group = "Selected Prop Actions")]
    public KeyboardShortcut CycleSelected { get => cycleSelected; set { cycleSelected = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Duplicate", Group = "Selected Prop Actions")]
    public KeyboardShortcut DuplicateProps { get => duplicateProps; set { duplicateProps = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Deepen", Group = "Selected Prop Actions")]
    public KeyboardShortcut DeepenSelectedProps { get => deepenSelectedProps; set { deepenSelectedProps = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Undeepen", Group = "Selected Prop Actions")]
    public KeyboardShortcut UndeepenSelectedProps { get => undeepenSelectedProps; set { undeepenSelectedProps = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Rotate Clockwise", Group = "Selection or Placement")]
    public KeyboardShortcut RotateClockwise { get => rotateClockwise; set { rotateClockwise = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Rotate Counter-clockwise", Group = "Selection or Placement")]
    public KeyboardShortcut RotateCounterClockwise { get => rotateCounterClockwise; set { rotateCounterClockwise = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Increase Rope Segments", Group = "Selection or Placement")]
    public KeyboardShortcut IncrementRopSegmentCount { get => incrementRopSegmentCount; set { incrementRopSegmentCount = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Decrease Rope Segments", Group = "Selection or Placement")]
    public KeyboardShortcut DecrementRopSegmentCount { get => decrementRopSegmentCount; set { decrementRopSegmentCount = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Rope Simulation/Bezier Switch", Group = "Selected Prop Actions")]
    public KeyboardShortcut SimulationBeizerSwitch { get => simulationBeizerSwitch; set { simulationBeizerSwitch = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Fast Rotate Clockwise", Group = "Selection or Placement")]
    public KeyboardShortcut FastRotateClockwise { get => fastRotateClockwise; set { fastRotateClockwise = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Fast Rotate Counter-Clockwise", Group = "Selection or Placement")]
    public KeyboardShortcut FastRotateCounterClockwise { get => fastRotateCounterClockwise; set { fastRotateCounterClockwise = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Cycle Variations", Group = "Selection or Placement")]
    public KeyboardShortcut CycleVariations { get => cycleVariations; set { cycleVariations = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Toggle No-collision Placement", Group = "Placement")]
    public KeyboardShortcut ToggleNoCollisionPropPlacement { get => toggleNoCollisionPropPlacement; set { toggleNoCollisionPropPlacement = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Selection Modifier")]
    public KeyboardShortcut PropSelectionModifier { get => propSelectionModifier; set { propSelectionModifier = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Select Props")]
    public KeyboardShortcut SelectPropsAlt { get => selectPropsAlt; set { selectPropsAlt = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    public KeyboardShortcut Undo { get => undo; set { undo = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    public KeyboardShortcut Redo { get => redo; set { redo = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Activate Search")]
    public KeyboardShortcut ActivateSearch { get => activateSearch; set { activateSearch = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Select Props")]
    public MouseShortcut SelectProps { get => selectProps; set { selectProps = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Place Props")]
    public MouseShortcut PlaceProp { get => placeProp; set { placeProp = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Drag View")]
    public MouseShortcut DragLevel { get => dragLevel; set { dragLevel = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }

    public PropsShortcuts()
    {
        CachedStrings = Utils.GetShortcutStrings(this);
    }
}

public class GlobalShortcuts : IEditorShortcuts
{
    public KeyboardShortcut ToMainPage { get; set; } = new(KeyboardKey.One, shift: false, ctrl: false, alt: true);
    public KeyboardShortcut ToGeometryEditor { get; set; } = new(KeyboardKey.Two, shift: false, ctrl: false, alt: true);
    public KeyboardShortcut ToTileEditor { get; set; } = new(KeyboardKey.Three, shift: false, ctrl: false, alt: true);
    public KeyboardShortcut ToCameraEditor { get; set; } = new(KeyboardKey.Four, shift: false, ctrl: false, alt: true);
    public KeyboardShortcut ToLightEditor { get; set; } = new(KeyboardKey.Five, shift: false, ctrl: false, alt: true);
    public KeyboardShortcut ToDimensionsEditor { get; set; } = new(KeyboardKey.Six, shift: false, ctrl: false, alt: true);
    public KeyboardShortcut ToEffectsEditor { get; set; } = new(KeyboardKey.Seven, shift: false, ctrl: false, alt: true);
    public KeyboardShortcut ToPropsEditor { get; set; } = new(KeyboardKey.Eight, shift: false, ctrl: false, alt: true);
    public KeyboardShortcut ToSettingsPage { get; set; } = new(KeyboardKey.Nine, shift: false, ctrl: false, alt: true);

    public KeyboardShortcut TakeScreenshot { get; set; } = new(KeyboardKey.Null);

    public KeyboardShortcut CycleTileRenderModes { get; set; } = new(KeyboardKey.Null);
    public KeyboardShortcut CyclePropRenderModes { get; set; } = new(KeyboardKey.Null);

    public KeyboardShortcut Open { get; set; } = new(KeyboardKey.O, ctrl: true, shift: false, alt: false);
    public KeyboardShortcut QuickSave { get; set; } = new(KeyboardKey.S, shift: false, ctrl: true, alt: false);
    public KeyboardShortcut QuickSaveAs { get; set; } = new(KeyboardKey.S, shift: true, ctrl: true, alt: false);
    public KeyboardShortcut Render { get; set; } = new(KeyboardKey.Null);

    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }

    public GlobalShortcuts()
    {
        CachedStrings = Utils.GetShortcutStrings(this);
    }
}

public class TileViewerShortcuts : IEditorShortcuts
{
    private KeyboardShortcut _activateSearch;
    private KeyboardShortcut _moveMenuUp;
    private KeyboardShortcut _moveMenuDown;
    private KeyboardShortcut _moveMenuCategoryDown;
    private KeyboardShortcut _moveMenuCategoryUp;
    private KeyboardShortcut _altDragView;

    private MouseShortcut _dragView;

    //

    public KeyboardShortcut ActivateSearch { get => _activateSearch; set { _activateSearch = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    public KeyboardShortcut MoveMenuUp { get => _moveMenuUp; set { _moveMenuUp = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    public KeyboardShortcut MoveMenuDown { get => _moveMenuDown; set { _moveMenuDown = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    public KeyboardShortcut MoveMenuCategoryUp { get => _moveMenuCategoryUp; set { _moveMenuCategoryUp = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    public KeyboardShortcut MoveMenuCategoryDown { get => _moveMenuCategoryDown; set { _moveMenuCategoryDown = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    public KeyboardShortcut AltDragView { get => _altDragView; set { _altDragView = value; CachedStrings = Utils.GetShortcutStrings(this); } }


    public MouseShortcut DragView { get => _dragView; set { _dragView = value; CachedStrings = Utils.GetShortcutStrings(this); } }


    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }

    public TileViewerShortcuts()
    {
        ActivateSearch = new(KeyboardKey.F, ctrl: true, shift: false, alt: false);

        MoveMenuUp = new(KeyboardKey.Up, ctrl: false, shift: false, alt: false);
        MoveMenuDown = new(KeyboardKey.Down, ctrl: false, shift: false, alt: false);
        MoveMenuCategoryUp = new(KeyboardKey.Left, ctrl: false, shift: false, alt: false);
        MoveMenuCategoryDown = new(KeyboardKey.Right, ctrl: false, shift: false, alt: false);
        AltDragView = new(KeyboardKey.Null, ctrl: false, shift: false, alt: false);

        DragView = new(MouseButton.Middle, ctrl: false, shift: false, alt: false);

        CachedStrings = Utils.GetShortcutStrings(this);
    }
}