namespace Leditor.Types;

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
               $"{Key switch {KeyboardKey.Space => "SPACE", KeyboardKey.Enter => "ENTER", KeyboardKey.LeftAlt => "ALT", KeyboardKey.LeftShift => "SHIFT", KeyboardKey.LeftControl => "CTRL", KeyboardKey.Null => "NONE", KeyboardKey.Escape => "ESCAPE", var k => k}}";
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
    public KeyboardShortcut TempActivateMultiSelect { get; set; } = new(KeyboardKey.LeftShift, shift: true, ctrl: false, alt: false);
    public KeyboardShortcut EraseEverything { get; set; } = new(KeyboardKey.X);
    
    public KeyboardShortcut ToggleTileVisibility { get; set; } = new(KeyboardKey.T);
    public KeyboardShortcut TogglePropVisibility { get; set; } = new(KeyboardKey.P);
    
    public KeyboardShortcut ToggleMemoryLoadMode { get; set; } = new(KeyboardKey.C, ctrl:true);
    public KeyboardShortcut ToggleMemoryDumbMode { get; set; } = new(KeyboardKey.V, ctrl:true);
    
    public KeyboardShortcut PickupGeo { get; set; } = new(KeyboardKey.Q, ctrl:false, shift:false, alt:false);
    public KeyboardShortcut PickupStackable { get; set; } = new(KeyboardKey.Q, ctrl:false, shift:true, alt:false);

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

    public KeyboardShortcut SetMaterialDefault { get; set; } = new(KeyboardKey.E, shift:false, ctrl:false, alt:false);
    public KeyboardShortcut ActivateSearch { get; set; } = new(KeyboardKey.F, ctrl:true, shift:false, alt:false);

    public KeyboardShortcut Undo { get; set; } = new(KeyboardKey.Z, ctrl:true, shift:false);
    public KeyboardShortcut Redo { get; set; } = new(KeyboardKey.Z, ctrl:true, shift: true);

    public KeyboardShortcut CopyTiles { get; set; } = new(KeyboardKey.C, ctrl: true, shift: false);
    public KeyboardShortcut PasteTilesWithGeo { get; set; } = new(KeyboardKey.V, ctrl: true, shift: true);
    public KeyboardShortcut PasteTilesWithoutGeo { get; set; } = new(KeyboardKey.V, ctrl: true);

    public KeyboardShortcut CycleAutoTilerMacroAxisPriority { get; set; } = new(KeyboardKey.Tab, ctrl:false, shift:false, alt:false);

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

    public KeyboardShortcut MoveDownInNewEffectMenu { get; set; } = new(KeyboardKey.S, shift:false, alt:false, ctrl:false);
    public KeyboardShortcut MoveUpInNewEffectMenu { get; set; } = new(KeyboardKey.W, shift:false, alt:false, ctrl:false);

    public KeyboardShortcut MoveCategoryDownInNewEffectsMenu { get; set; } = new(KeyboardKey.D, shift:false, alt:false, ctrl:false);
    public KeyboardShortcut MoveCategoryUpInNewEffectsMenu { get; set; } = new(KeyboardKey.A, shift:false, alt:false, ctrl:false);

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
    
    public KeyboardShortcut DeepenSelectedProps { get; set; } = new(KeyboardKey.Null);
    public KeyboardShortcut UndeepenSelectedProps { get; set; } = new(KeyboardKey.Null);
    
    public KeyboardShortcut RotateClockwise { get; set; } = new(KeyboardKey.Null);
    public KeyboardShortcut RotateCounterClockwise { get; set; } = new(KeyboardKey.Null);

    public KeyboardShortcut IncrementRopSegmentCount { get; set; } = new(KeyboardKey.K);
    public KeyboardShortcut DecrementRopSegmentCount { get; set; } = new(KeyboardKey.J);

    public KeyboardShortcut SimulationBeizerSwitch { get; set; } = new(KeyboardKey.Null);
    
    public KeyboardShortcut FastRotateClockwise { get; set; } = new(KeyboardKey.Null);
    public KeyboardShortcut FastRotateCounterClockwise { get; set; } = new(KeyboardKey.Null);

    public KeyboardShortcut CycleVariations { get; set; } = new(KeyboardKey.V, shift: false, alt: false, ctrl: false);

    public KeyboardShortcut ToggleNoCollisionPropPlacement { get; set; } = KeyboardKey.Null;

    public KeyboardShortcut PropSelectionModifier { get; set; } = new(KeyboardKey.LeftControl, ctrl: true);
    
    public KeyboardShortcut SelectPropsAlt { get; set; } = KeyboardKey.Null;
    
    public MouseShortcut SelectProps { get; set; } = MouseButton.Right;

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

    public KeyboardShortcut TakeScreenshot { get; set; } = new(KeyboardKey.Null);

    public KeyboardShortcut CycleTileRenderModes { get; set; } = new(KeyboardKey.Null);
    public KeyboardShortcut CyclePropRenderModes { get; set; } = new(KeyboardKey.Null);

    public KeyboardShortcut Open { get; set; } = new(KeyboardKey.O, ctrl: true, shift: false, alt: false);
    public KeyboardShortcut QuickSave { get; set; } = new(KeyboardKey.S, shift:false, ctrl:true, alt:false);
    public KeyboardShortcut QuickSaveAs { get; set; } = new(KeyboardKey.S, shift:true, ctrl:true, alt:false);
    public KeyboardShortcut Render { get; set; } = new(KeyboardKey.Null);
    
    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }
    
    public GlobalShortcuts()
    {
        CachedStrings = Utils.GetShortcutStrings(this);
    }
}

public class TileViewerShortcuts : IEditorShortcuts {
    private KeyboardShortcut _activateSearch;
    private KeyboardShortcut _moveMenuUp;
    private KeyboardShortcut _moveMenuDown;
    private KeyboardShortcut _moveMenuCategoryDown;
    private KeyboardShortcut _moveMenuCategoryUp;
    private KeyboardShortcut _altDragView;

    private MouseShortcut _dragView;

    //

    public KeyboardShortcut ActivateSearch          { get => _activateSearch;           set { _activateSearch = value;          CachedStrings = Utils.GetShortcutStrings(this); } }
    public KeyboardShortcut MoveMenuUp              { get => _moveMenuUp;               set { _moveMenuUp = value;              CachedStrings = Utils.GetShortcutStrings(this); } }
    public KeyboardShortcut MoveMenuDown            { get => _moveMenuDown;             set { _moveMenuDown = value;            CachedStrings = Utils.GetShortcutStrings(this); } }
    public KeyboardShortcut MoveMenuCategoryUp      { get => _moveMenuCategoryUp;       set { _moveMenuCategoryUp = value;      CachedStrings = Utils.GetShortcutStrings(this); } }
    public KeyboardShortcut MoveMenuCategoryDown    { get => _moveMenuCategoryDown;     set { _moveMenuCategoryDown = value;    CachedStrings = Utils.GetShortcutStrings(this); } }
    public KeyboardShortcut AltDragView             { get => _altDragView;              set { _altDragView = value;             CachedStrings = Utils.GetShortcutStrings(this); } }
    
    
    public MouseShortcut DragView                   { get => _dragView;                 set { _dragView = value;                CachedStrings = Utils.GetShortcutStrings(this); } }


    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }

    public TileViewerShortcuts() {
        ActivateSearch          = new(KeyboardKey.F,     ctrl:true,  shift:false, alt:false);

        MoveMenuUp              = new(KeyboardKey.Up,    ctrl:false, shift:false, alt:false);
        MoveMenuDown            = new(KeyboardKey.Down,  ctrl:false, shift:false, alt:false);
        MoveMenuCategoryUp      = new(KeyboardKey.Left,  ctrl:false, shift:false, alt:false);
        MoveMenuCategoryDown    = new(KeyboardKey.Right, ctrl:false, shift:false, alt:false);
        AltDragView             = new(KeyboardKey.Null,  ctrl:false, shift:false, alt:false);

        DragView = new(MouseButton.Middle, ctrl:false, shift:false, alt:false);

        CachedStrings = Utils.GetShortcutStrings(this);
    }
}