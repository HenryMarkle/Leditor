using System.Net.Mail;

namespace Leditor.Types;

#nullable enable

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
        return Key is KeyboardKey.Null ? "NONE" : $"{(Ctrl is not null and not false ? "CTRL + " : "")}" +
               $"{(Shift is not null and not false ? "SHIFT + " : "")}" +
               $"{(Alt is not null and not false ? "ALT + " : "")}" +
               $"{Key switch { KeyboardKey.Space => "SPACE", KeyboardKey.Enter => "ENTER", KeyboardKey.LeftAlt => "ALT", KeyboardKey.LeftShift => "SHIFT", KeyboardKey.LeftControl => "CTRL", KeyboardKey.Null => "NONE", KeyboardKey.Escape => "ESCAPE", var k => k }}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (obj is not KeyboardShortcut k) return false;

        return Key == k.Key 
            && Ctrl.Equals(k.Ctrl) 
            && Shift.Equals(k.Shift) 
            && Alt.Equals(k.Alt);
    }

    public static bool operator ==(KeyboardShortcut? left, KeyboardShortcut? right) {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;

        return left.Key == right.Key 
            && left.Ctrl.Equals(right.Ctrl) 
            && left.Shift.Equals(right.Shift) 
            && left.Alt.Equals(right.Alt);
    }

    public static bool operator !=(KeyboardShortcut? left, KeyboardShortcut? right) {
        if (left is null && right is null) return false;
        if (left is null || right is null) return true;

        return !(left.Key == right.Key 
            && left.Ctrl.Equals(right.Ctrl) 
            && left.Shift.Equals(right.Shift) 
            && left.Alt.Equals(right.Alt));
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
    private KeyboardShortcut toRightGeo = new(KeyboardKey.D);
    private KeyboardShortcut toLeftGeo = new(KeyboardKey.A);
    private KeyboardShortcut toTopGeo = new(KeyboardKey.W);
    private KeyboardShortcut toBottomGeo = new(KeyboardKey.S);
    private KeyboardShortcut cycleLayers = new(KeyboardKey.L);
    private KeyboardShortcut toggleGrid = new(KeyboardKey.M);
    private KeyboardShortcut showCameras = new(KeyboardKey.K);
    private KeyboardShortcut toggleMultiSelect = new(KeyboardKey.E);
    private KeyboardShortcut tempActivateMultiSelect = new(KeyboardKey.LeftShift, shift: true, ctrl: false, alt: false);
    private KeyboardShortcut eraseEverything = new(KeyboardKey.X);
    private KeyboardShortcut toggleTileVisibility = new(KeyboardKey.T);
    private KeyboardShortcut togglePropVisibility = new(KeyboardKey.P);
    private KeyboardShortcut toggleBasicView = new(KeyboardKey.R);
    private KeyboardShortcut toggleMemoryLoadMode = new(KeyboardKey.C, ctrl: true);
    private KeyboardShortcut toggleMemoryDumbMode = new(KeyboardKey.V, ctrl: true);
    private KeyboardShortcut pickupGeo = new(KeyboardKey.Q, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut pickupStackable = new(KeyboardKey.Q, ctrl: false, shift: true, alt: false);
    private MouseShortcut draw = new(MouseButton.Left);
    private MouseShortcut erase = new(MouseButton.Right);
    private MouseShortcut dragLevel = new(MouseButton.Middle);
    private KeyboardShortcut altDraw = new(KeyboardKey.Null);
    private KeyboardShortcut altErase = new(KeyboardKey.F);
    private KeyboardShortcut altDragLevel = new(KeyboardKey.G);
    private KeyboardShortcut undo = new(ctrl: true, shift: false, key: KeyboardKey.Z);
    private KeyboardShortcut redo = new(ctrl: true, shift: true, key: KeyboardKey.Z);

    private KeyboardShortcut moveViewLeft = new(KeyboardKey.Left);
    private KeyboardShortcut moveViewTop = new(KeyboardKey.Up);
    private KeyboardShortcut moveViewRight = new(KeyboardKey.Right);
    private KeyboardShortcut moveViewBottom = new(KeyboardKey.Down);

    private KeyboardShortcut fastMoveViewLeft = new(KeyboardKey.Left, shift:true);
    private KeyboardShortcut fastMoveViewTop = new(KeyboardKey.Up, shift:true);
    private KeyboardShortcut fastMoveViewRight = new(KeyboardKey.Right, shift:true);
    private KeyboardShortcut fastMoveViewBottom = new(KeyboardKey.Down, shift:true);

    private KeyboardShortcut reallyFastMoveViewLeft = new(KeyboardKey.Left, alt:true);
    private KeyboardShortcut reallyFastMoveViewTop = new(KeyboardKey.Up, alt:true);
    private KeyboardShortcut reallyFastMoveViewRight = new(KeyboardKey.Right, alt:true);
    private KeyboardShortcut reallyFastMoveViewBottom = new(KeyboardKey.Down, alt:true);

    private KeyboardShortcut toggleIndexHint = new(KeyboardKey.Null);

    private KeyboardShortcut solidBrush = new(KeyboardKey.Null);
    private KeyboardShortcut slopeBrush = new(KeyboardKey.Null);
    private KeyboardShortcut platformBrush = new(KeyboardKey.Null);
    private KeyboardShortcut glassBrush = new(KeyboardKey.Null);

    private KeyboardShortcut verticalPoleBrush = new(KeyboardKey.Null);
    private KeyboardShortcut horizontalPoleBrush = new(KeyboardKey.Null);
    private KeyboardShortcut crackedTerrainBrush = new(KeyboardKey.Null);
    private KeyboardShortcut bathiveBrush = new(KeyboardKey.Null);
    private KeyboardShortcut forbidFlyChainsBrush = new(KeyboardKey.Null);
    private KeyboardShortcut waterfallBrush = new(KeyboardKey.Null);
    private KeyboardShortcut wormGrassBrush = new(KeyboardKey.Null);
    private KeyboardShortcut placeRockBrush = new(KeyboardKey.Null);
    private KeyboardShortcut placeSpearBrush = new(KeyboardKey.Null);
    private KeyboardShortcut shortcutEntranceBrush = new(KeyboardKey.Null);
    private KeyboardShortcut shortcutPathBrush = new(KeyboardKey.Null);
    private KeyboardShortcut roomEntranceBrush = new(KeyboardKey.Null);
    private KeyboardShortcut wackBrush = new(KeyboardKey.Null);
    private KeyboardShortcut scavengerBrush = new(KeyboardKey.Null);
    private KeyboardShortcut garbageWormBrush = new(KeyboardKey.Null);
    
    private KeyboardShortcut toggleVerticalMirror = new(KeyboardKey.V);
    private KeyboardShortcut toggleHorizontalMirror = new(KeyboardKey.H);

    private KeyboardShortcut moveVerticalMirrorRight = new(KeyboardKey.Null);
    private KeyboardShortcut moveVerticalMirrorLeft = new(KeyboardKey.Null);

    private KeyboardShortcut moveHorizontalMirrorUp = new(KeyboardKey.Null);
    private KeyboardShortcut moveHorizontalMirrorDown = new(KeyboardKey.Null);

    //

    [ShortcutName("Navigate To Next Menu Category", Group = "Menu")]
    public KeyboardShortcut ToRightGeo { get => toRightGeo; set {toRightGeo = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Navigate To Previous Menu Category", Group = "Menu")]
    public KeyboardShortcut ToLeftGeo { get => toLeftGeo; set {toLeftGeo = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Navigate Menu Up", Group = "Menu")]
    public KeyboardShortcut ToTopGeo { get => toTopGeo; set {toTopGeo = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Navigate Menu Down", Group = "Menu")]
    public KeyboardShortcut ToBottomGeo { get => toBottomGeo; set {toBottomGeo = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Cycle Layers")]
    public KeyboardShortcut CycleLayers { get => cycleLayers; set {cycleLayers = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Show/Hide Grid")]
    public KeyboardShortcut ToggleGrid { get => toggleGrid; set {toggleGrid = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Show/Hide Cameras")]
    public KeyboardShortcut ShowCameras { get => showCameras; set {showCameras = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    

    [ShortcutName("Toggle Multi-Select", Group = "Brush")]
    public KeyboardShortcut ToggleMultiSelect { get => toggleMultiSelect; set {toggleMultiSelect = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Temporarily Enable Multi-Select", Group = "Brush")]
    public KeyboardShortcut TempActivateMultiSelect { get => tempActivateMultiSelect; set {tempActivateMultiSelect = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Allow Erasing Everything", Group = "Brush")]
    public KeyboardShortcut EraseEverything { get => eraseEverything; set {eraseEverything = value; CachedStrings = Utils.GetShortcutStrings(this);} }

    [ShortcutName("Show/Hide Tiles")]
    public KeyboardShortcut ToggleTileVisibility { get => toggleTileVisibility; set {toggleTileVisibility = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Show/Hide Props")]
    public KeyboardShortcut TogglePropVisibility { get => togglePropVisibility; set {togglePropVisibility = value; CachedStrings = Utils.GetShortcutStrings(this);} }


    [ShortcutName("Toggle Basic View")]
    public KeyboardShortcut ToggleBasicView { get => toggleBasicView; set { toggleBasicView = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    
    [ShortcutName("Toggle Copy Mode", Group = "Brush")]
    public KeyboardShortcut ToggleMemoryLoadMode { get => toggleMemoryLoadMode; set {toggleMemoryLoadMode = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Toggle Paste Mode", Group = "Brush")]
    public KeyboardShortcut ToggleMemoryDumbMode { get => toggleMemoryDumbMode; set {toggleMemoryDumbMode = value; CachedStrings = Utils.GetShortcutStrings(this);} }


    [ShortcutName("Pickup Geometry", Group = "Brush")]
    public KeyboardShortcut PickupGeo { get => pickupGeo; set {pickupGeo = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Pickup Geometry Feature", Group = "Brush")]
    public KeyboardShortcut PickupStackable { get => pickupStackable; set {pickupStackable = value; CachedStrings = Utils.GetShortcutStrings(this);} }


    [ShortcutName("Draw", Group = "Brush")]
    public MouseShortcut Draw { get => draw; set {draw = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Erase", Group = "Brush")]
    public MouseShortcut Erase { get => erase; set {erase = value; CachedStrings = Utils.GetShortcutStrings(this);} }

    [ShortcutName("Drag View")]
    public MouseShortcut DragLevel { get => dragLevel; set {dragLevel = value; CachedStrings = Utils.GetShortcutStrings(this);} }

    [ShortcutName("Draw")]
    public KeyboardShortcut AltDraw { get => altDraw; set {altDraw = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Erase")]
    public KeyboardShortcut AltErase { get => altErase; set {altErase = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Drag View")]
    public KeyboardShortcut AltDragLevel { get => altDragLevel; set {altDragLevel = value; CachedStrings = Utils.GetShortcutStrings(this);} }

    [ShortcutName("Undo")]
    public KeyboardShortcut Undo { get => undo; set {undo = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Redo")]
    public KeyboardShortcut Redo { get => redo; set {redo = value; CachedStrings = Utils.GetShortcutStrings(this);} }


    [ShortcutName("Move View Left", Group = "Movement")]
    public KeyboardShortcut MoveViewLeft { get => moveViewLeft; set { moveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top", Group = "Movement")]
    public KeyboardShortcut MoveViewTop { get => moveViewTop; set { moveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right", Group = "Movement")]
    public KeyboardShortcut MoveViewRight { get => moveViewRight; set { moveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom", Group = "Movement")]
    public KeyboardShortcut MoveViewBottom { get => moveViewBottom; set { moveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    

    [ShortcutName("Move View Left Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewLeft { get => fastMoveViewLeft; set { fastMoveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top Quicky", Group = "Movement")]
    public KeyboardShortcut FastMoveViewTop { get => fastMoveViewTop; set { fastMoveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewRight { get => fastMoveViewRight; set { fastMoveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewBottom { get => fastMoveViewBottom; set { fastMoveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    

    [ShortcutName("Move View Left Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewLeft { get => reallyFastMoveViewLeft; set { reallyFastMoveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top Really Quicky", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewTop { get => reallyFastMoveViewTop; set { reallyFastMoveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewRight { get => reallyFastMoveViewRight; set { reallyFastMoveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewBottom { get => reallyFastMoveViewBottom; set { reallyFastMoveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Toggle Ruler")]
    public KeyboardShortcut ToggleIndexHint { get => toggleIndexHint; set { toggleIndexHint = value; CachedStrings = Utils.GetShortcutStrings(this); } }


    [ShortcutName("Solid", Group = "Instant Access")]
    public KeyboardShortcut SolidBrush { get => solidBrush; set { solidBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Slope", Group = "Instant Access")]
    public KeyboardShortcut SlopeBrush { get => slopeBrush; set { slopeBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Platform", Group = "Instant Access")]
    public KeyboardShortcut PlatformBrush { get => platformBrush; set { platformBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Glass", Group = "Instant Access")]
    public KeyboardShortcut GlassBrush { get => glassBrush; set { glassBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Vertical Pole", Group = "Instant Access")]
    public KeyboardShortcut VerticalPoleBrush { get => verticalPoleBrush; set { verticalPoleBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Horizontal Pole", Group = "Instant Access")]
    public KeyboardShortcut HorizontalPoleBrush { get => horizontalPoleBrush; set { horizontalPoleBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Cracked Terrain", Group = "Instant Access")]
    public KeyboardShortcut CrackedTerrainBrush { get => crackedTerrainBrush; set { crackedTerrainBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Bathive", Group = "Instant Access")]
    public KeyboardShortcut BathiveBrush { get => bathiveBrush; set { bathiveBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Forbid Fly Chains", Group = "Instant Access")]
    public KeyboardShortcut ForbidFlyChainsBrush { get => forbidFlyChainsBrush; set { forbidFlyChainsBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Waterfall", Group = "Instant Access")]
    public KeyboardShortcut WaterfallBrush { get => waterfallBrush; set { waterfallBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Worm Grass", Group = "Instant Access")]
    public KeyboardShortcut WormGrassBrush { get => wormGrassBrush; set { wormGrassBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Place Rock", Group = "Instant Access")]
    public KeyboardShortcut PlaceRockBrush { get => placeRockBrush; set { placeRockBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Place Spear", Group = "Instant Access")]
    public KeyboardShortcut PlaceSpearBrush { get => placeSpearBrush; set { placeSpearBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Shortcut Entrance", Group = "Instant Access")]
    public KeyboardShortcut ShortcutEntranceBrush { get => shortcutEntranceBrush; set { shortcutEntranceBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Shortcut Path", Group = "Instant Access")]
    public KeyboardShortcut ShortcutPathBrush { get => shortcutPathBrush; set { shortcutPathBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Room Entrance", Group = "Instant Access")]
    public KeyboardShortcut RoomEntranceBrush { get => roomEntranceBrush; set { roomEntranceBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Wack-A-Mole Hole", Group = "Instant Access")]
    public KeyboardShortcut WackBrush { get => wackBrush; set { wackBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Scavenger", Group = "Instant Access")]
    public KeyboardShortcut ScavengerBrush { get => scavengerBrush; set { scavengerBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Garbage Worm", Group = "Instant Access")]
    public KeyboardShortcut GarbageWormBrush { get => garbageWormBrush; set { garbageWormBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Toggle Vertical Mirror", Group = "Mirrors")]    
    public KeyboardShortcut ToggleVerticalMirror { get => toggleVerticalMirror; set { toggleVerticalMirror = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Toggle Horizontal Mirror", Group = "Mirrors")]    
    public KeyboardShortcut ToggleHorizontalMirror { get => toggleHorizontalMirror; set { toggleHorizontalMirror = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Move Vertical Mirror to The Right", Group = "Mirrors")]    
    public KeyboardShortcut MoveVerticalMirrorRight { get => moveVerticalMirrorRight; set { moveVerticalMirrorRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move Vertical Mirror to The Left", Group = "Mirrors")]    
    public KeyboardShortcut MoveVerticalMirrorLeft { get => moveVerticalMirrorLeft; set { moveVerticalMirrorLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Move Horizontal Mirror Upwards", Group = "Mirrors")]    
    public KeyboardShortcut MoveHorizontalMirrorUp { get => moveHorizontalMirrorUp; set { moveHorizontalMirrorUp = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move Horizontal Mirror Downwards", Group = "Mirrors")]    
    public KeyboardShortcut MoveHorizontalMirrorDown { get => moveHorizontalMirrorDown; set { moveHorizontalMirrorDown = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }

    public ExperimentalGeoShortcuts()
    {
        CachedStrings = Utils.GetShortcutStrings(this);
    }
}

public record TileShortcuts : IEditorShortcuts
{
    private KeyboardShortcut moveToNextCategory = new(KeyboardKey.D);
    private KeyboardShortcut moveToPreviousCategory = new(KeyboardKey.A);
    private KeyboardShortcut moveDown = new(KeyboardKey.S);
    private KeyboardShortcut moveUp = new(KeyboardKey.W);
    private KeyboardShortcut cycleLayers = new(KeyboardKey.L);
    private KeyboardShortcut pickupItem = new(KeyboardKey.Q);
    private KeyboardShortcut forcePlaceTileWithGeo = new(KeyboardKey.G);
    private KeyboardShortcut forcePlaceTileWithoutGeo = new(KeyboardKey.F, shift: false);
    private KeyboardShortcut tileMaterialSwitch = new(KeyboardKey.M);
    private KeyboardShortcut hoveredItemInfo = new(KeyboardKey.P);
    private KeyboardShortcut setMaterialDefault = new(KeyboardKey.E, shift: false, ctrl: false, alt: false);
    private KeyboardShortcut activateSearch = new(KeyboardKey.F, ctrl: true, shift: false, alt: false);
    private KeyboardShortcut undo = new(KeyboardKey.Z, ctrl: true, shift: false);
    private KeyboardShortcut redo = new(KeyboardKey.Z, ctrl: true, shift: true);
    private KeyboardShortcut showCameras = new(KeyboardKey.K, ctrl: false, shift: false, alt:false);
    private KeyboardShortcut copyTiles = new(KeyboardKey.C, ctrl: true, shift: false);
    private KeyboardShortcut pasteTilesWithGeo = new(KeyboardKey.V, ctrl: true, shift: true);
    private KeyboardShortcut pasteTilesWithoutGeo = new(KeyboardKey.V, ctrl: true);
    private KeyboardShortcut cycleAutoTilerMacroAxisPriority = new(KeyboardKey.Tab, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut toggleLayer1 = new(KeyboardKey.Null, shift: false, ctrl: false);
    private KeyboardShortcut toggleLayer2 = new(KeyboardKey.Null, shift: false, ctrl: false);
    private KeyboardShortcut toggleLayer3 = new(KeyboardKey.Null, shift: false, ctrl: false);
    private KeyboardShortcut toggleLayer1Tiles = new(KeyboardKey.Null, shift: true, ctrl: false);
    private KeyboardShortcut toggleLayer2Tiles = new(KeyboardKey.Null, shift: true, ctrl: false);
    private KeyboardShortcut toggleLayer3Tiles = new(KeyboardKey.Null, shift: true, ctrl: false);
    private KeyboardShortcut resizeMaterialBrush = KeyboardKey.LeftAlt;
    private KeyboardShortcut togglePathsView = new(KeyboardKey.O, ctrl: false, shift: false, alt: false);
    private KeyboardShortcut altDraw = new(KeyboardKey.Null);
    private KeyboardShortcut altErase = new(KeyboardKey.Null);
    private KeyboardShortcut altDragLevel = new(KeyboardKey.Null);
    private KeyboardShortcut enlargeBrush = new(KeyboardKey.Equal);
    private KeyboardShortcut shrinkBrush = new(KeyboardKey.Minus);
    private KeyboardShortcut toggleIndexHint = KeyboardKey.Null;

    private KeyboardShortcut moveViewLeft = new(KeyboardKey.Left);
    private KeyboardShortcut moveViewTop = new(KeyboardKey.Up);
    private KeyboardShortcut moveViewRight = new(KeyboardKey.Right);
    private KeyboardShortcut moveViewBottom = new(KeyboardKey.Down);

    private KeyboardShortcut fastMoveViewLeft = new(KeyboardKey.Left, shift:true);
    private KeyboardShortcut fastMoveViewTop = new(KeyboardKey.Up, shift:true);
    private KeyboardShortcut fastMoveViewRight = new(KeyboardKey.Right, shift:true);
    private KeyboardShortcut fastMoveViewBottom = new(KeyboardKey.Down, shift:true);

    private KeyboardShortcut reallyFastMoveViewLeft = new(KeyboardKey.Left, alt:true);
    private KeyboardShortcut reallyFastMoveViewTop = new(KeyboardKey.Up, alt:true);
    private KeyboardShortcut reallyFastMoveViewRight = new(KeyboardKey.Right, alt:true);
    private KeyboardShortcut reallyFastMoveViewBottom = new(KeyboardKey.Down, alt:true);

    private KeyboardShortcut toggleProps = KeyboardKey.Null;

    private MouseShortcut draw = new(MouseButton.Left);
    private MouseShortcut erase = new(MouseButton.Right);
    private MouseShortcut dragLevel = new(MouseButton.Middle);


    //

    [ShortcutName("Navigate Menu Category Down", Group = "Menu")]
    public KeyboardShortcut MoveToNextCategory { get => moveToNextCategory; set {moveToNextCategory = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Navigate Menu Category Up", Group = "Menu")]
    public KeyboardShortcut MoveToPreviousCategory { get => moveToPreviousCategory; set {moveToPreviousCategory = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Navigate Menu Down", Group = "Menu")]
    public KeyboardShortcut MoveDown { get => moveDown; set {moveDown = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Navigate Menu Up", Group = "Menu")]
    public KeyboardShortcut MoveUp { get => moveUp; set {moveUp = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Cycle Layer")]
    public KeyboardShortcut CycleLayers { get => cycleLayers; set {cycleLayers = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Pickup Tile/Material")]
    public KeyboardShortcut PickupItem { get => pickupItem; set {pickupItem = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Force-Place Tile With Geometry")]
    public KeyboardShortcut ForcePlaceTileWithGeo { get => forcePlaceTileWithGeo; set {forcePlaceTileWithGeo = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Force-Place Tile Without Geometry")]
    public KeyboardShortcut ForcePlaceTileWithoutGeo { get => forcePlaceTileWithoutGeo; set {forcePlaceTileWithoutGeo = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Switch Between Tiles and Materials")]
    public KeyboardShortcut TileMaterialSwitch { get => tileMaterialSwitch; set {tileMaterialSwitch = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Hovered Item Debugging Info")]
    public KeyboardShortcut HoveredItemInfo { get => hoveredItemInfo; set {hoveredItemInfo = value; CachedStrings = Utils.GetShortcutStrings(this); } }


    [ShortcutName("Set Current Material As The Default")]
    public KeyboardShortcut SetMaterialDefault { get => setMaterialDefault; set {setMaterialDefault = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Activate Search")]
    public KeyboardShortcut ActivateSearch { get => activateSearch; set {activateSearch = value; CachedStrings = Utils.GetShortcutStrings(this); } }


    [ShortcutName("Undo")]
    public KeyboardShortcut Undo { get => undo; set {undo = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Redo")]
    public KeyboardShortcut Redo { get => redo; set {redo = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Show/Hide Cameras")]
    public KeyboardShortcut ShowCameras { get => showCameras; set { showCameras = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Toggle Tile Copying")]
    public KeyboardShortcut CopyTiles { get => copyTiles; set {copyTiles = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Toggle Tile Pasting With Geometry")]
    public KeyboardShortcut PasteTilesWithGeo { get => pasteTilesWithGeo; set {pasteTilesWithGeo = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Toggle Tile Pasting Without Geometry")]
    public KeyboardShortcut PasteTilesWithoutGeo { get => pasteTilesWithoutGeo; set {pasteTilesWithoutGeo = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Cycle Auto-Piper Macro Axis Order")]
    public KeyboardShortcut CycleAutoTilerMacroAxisPriority { get => cycleAutoTilerMacroAxisPriority; set {cycleAutoTilerMacroAxisPriority = value; CachedStrings = Utils.GetShortcutStrings(this); } }

 
    [ShortcutName("Show/Hide Layer 1")]
    public KeyboardShortcut ToggleLayer1 { get => toggleLayer1; set {toggleLayer1 = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Show/Hide Layer 2")]
    public KeyboardShortcut ToggleLayer2 { get => toggleLayer2; set {toggleLayer2 = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Show/Hide Layer 3")]
    public KeyboardShortcut ToggleLayer3 { get => toggleLayer3; set {toggleLayer3 = value; CachedStrings = Utils.GetShortcutStrings(this); } }


    [ShortcutName("Show/Hide Layer 1 Tiles")]
    public KeyboardShortcut ToggleLayer1Tiles { get => toggleLayer1Tiles; set {toggleLayer1Tiles = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Show/Hide Layer 2 Tiles")]
    public KeyboardShortcut ToggleLayer2Tiles { get => toggleLayer2Tiles; set {toggleLayer2Tiles = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Show/Hide Layer 3 Tiles")]
    public KeyboardShortcut ToggleLayer3Tiles { get => toggleLayer3Tiles; set {toggleLayer3Tiles = value; CachedStrings = Utils.GetShortcutStrings(this); } }


    [ShortcutName("Resize Material Brush (+ Mouse Wheel)")]
    public KeyboardShortcut ResizeMaterialBrush { get => resizeMaterialBrush; set {resizeMaterialBrush = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Highlight Geometry Shortcuts")]
    public KeyboardShortcut TogglePathsView { get => togglePathsView; set {togglePathsView = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Draw")]
    public KeyboardShortcut AltDraw { get => altDraw; set {altDraw = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Erase")]
    public KeyboardShortcut AltErase { get => altErase; set {altErase = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Drag View")]
    public KeyboardShortcut AltDragLevel { get => altDragLevel; set {altDragLevel = value; CachedStrings = Utils.GetShortcutStrings(this); } }




    [ShortcutName("Draw")]
    public MouseShortcut Draw { get => draw; set {draw = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Erase")]
    public MouseShortcut Erase { get => erase; set {erase = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Drag View")]
    public MouseShortcut DragLevel { get => dragLevel; set {dragLevel = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Increase Brush Size")]
    public KeyboardShortcut EnlargeBrush { get => enlargeBrush; set { enlargeBrush = value; CachedStrings = Utils.GetShortcutStrings(this); }}
    
    [ShortcutName("Decrease Brush Size")]
    public KeyboardShortcut ShrinkBrush { get => shrinkBrush; set { shrinkBrush = value; CachedStrings = Utils.GetShortcutStrings(this); }}


    [ShortcutName("Toggle Ruler")]
    public KeyboardShortcut ToggleIndexHint { get => toggleIndexHint; set { toggleIndexHint = value; CachedStrings = Utils.GetShortcutStrings(this); } }


    [ShortcutName("Move View Left", Group = "Movement")]
    public KeyboardShortcut MoveViewLeft { get => moveViewLeft; set { moveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top", Group = "Movement")]
    public KeyboardShortcut MoveViewTop { get => moveViewTop; set { moveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right", Group = "Movement")]
    public KeyboardShortcut MoveViewRight { get => moveViewRight; set { moveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom", Group = "Movement")]
    public KeyboardShortcut MoveViewBottom { get => moveViewBottom; set { moveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    

    [ShortcutName("Move View Left Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewLeft { get => fastMoveViewLeft; set { fastMoveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top Quicky", Group = "Movement")]
    public KeyboardShortcut FastMoveViewTop { get => fastMoveViewTop; set { fastMoveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewRight { get => fastMoveViewRight; set { fastMoveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewBottom { get => fastMoveViewBottom; set { fastMoveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Toggle Props")]
    public KeyboardShortcut ToggleProps { get => toggleProps; set { toggleProps = value; CachedStrings = Utils.GetShortcutStrings(this); }}
    

    [ShortcutName("Move View Left Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewLeft { get => reallyFastMoveViewLeft; set { reallyFastMoveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top Really Quicky", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewTop { get => reallyFastMoveViewTop; set { reallyFastMoveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewRight { get => reallyFastMoveViewRight; set { reallyFastMoveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewBottom { get => reallyFastMoveViewBottom; set { reallyFastMoveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    

 
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

    private KeyboardShortcut moveViewLeft = new(KeyboardKey.Left);
    private KeyboardShortcut moveViewTop = new(KeyboardKey.Up);
    private KeyboardShortcut moveViewRight = new(KeyboardKey.Right);
    private KeyboardShortcut moveViewBottom = new(KeyboardKey.Down);

    private KeyboardShortcut fastMoveViewLeft = new(KeyboardKey.Left, shift:true);
    private KeyboardShortcut fastMoveViewTop = new(KeyboardKey.Up, shift:true);
    private KeyboardShortcut fastMoveViewRight = new(KeyboardKey.Right, shift:true);
    private KeyboardShortcut fastMoveViewBottom = new(KeyboardKey.Down, shift:true);

    private KeyboardShortcut reallyFastMoveViewLeft = new(KeyboardKey.Left, alt:true);
    private KeyboardShortcut reallyFastMoveViewTop = new(KeyboardKey.Up, alt:true);
    private KeyboardShortcut reallyFastMoveViewRight = new(KeyboardKey.Right, alt:true);
    private KeyboardShortcut reallyFastMoveViewBottom = new(KeyboardKey.Down, alt:true);

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

    [ShortcutName("Move View Left", Group = "Movement")]
    public KeyboardShortcut MoveViewLeft { get => moveViewLeft; set { moveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top", Group = "Movement")]
    public KeyboardShortcut MoveViewTop { get => moveViewTop; set { moveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right", Group = "Movement")]
    public KeyboardShortcut MoveViewRight { get => moveViewRight; set { moveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom", Group = "Movement")]
    public KeyboardShortcut MoveViewBottom { get => moveViewBottom; set { moveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    

    [ShortcutName("Move View Left Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewLeft { get => fastMoveViewLeft; set { fastMoveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top Quicky", Group = "Movement")]
    public KeyboardShortcut FastMoveViewTop { get => fastMoveViewTop; set { fastMoveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewRight { get => fastMoveViewRight; set { fastMoveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewBottom { get => fastMoveViewBottom; set { fastMoveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    

    [ShortcutName("Move View Left Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewLeft { get => reallyFastMoveViewLeft; set { reallyFastMoveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top Really Quicky", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewTop { get => reallyFastMoveViewTop; set { reallyFastMoveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewRight { get => reallyFastMoveViewRight; set { reallyFastMoveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewBottom { get => reallyFastMoveViewBottom; set { reallyFastMoveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    

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

    private KeyboardShortcut moveViewLeft = new(KeyboardKey.Left);
    private KeyboardShortcut moveViewTop = new(KeyboardKey.Up);
    private KeyboardShortcut moveViewRight = new(KeyboardKey.Right);
    private KeyboardShortcut moveViewBottom = new(KeyboardKey.Down);

    private KeyboardShortcut fastMoveViewLeft = new(KeyboardKey.Left, shift:true);
    private KeyboardShortcut fastMoveViewTop = new(KeyboardKey.Up, shift:true);
    private KeyboardShortcut fastMoveViewRight = new(KeyboardKey.Right, shift:true);
    private KeyboardShortcut fastMoveViewBottom = new(KeyboardKey.Down, shift:true);

    private KeyboardShortcut reallyFastMoveViewLeft = new(KeyboardKey.Left, alt:true);
    private KeyboardShortcut reallyFastMoveViewTop = new(KeyboardKey.Up, alt:true);
    private KeyboardShortcut reallyFastMoveViewRight = new(KeyboardKey.Right, alt:true);
    private KeyboardShortcut reallyFastMoveViewBottom = new(KeyboardKey.Down, alt:true);

    private KeyboardShortcut undo = new(KeyboardKey.Z, ctrl:true);
    private KeyboardShortcut redo = new(KeyboardKey.Z, ctrl:true, shift:true);

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

    [ShortcutName("Move View Left", Group = "Movement")]
    public KeyboardShortcut MoveViewLeft { get => moveViewLeft; set { moveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top", Group = "Movement")]
    public KeyboardShortcut MoveViewTop { get => moveViewTop; set { moveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right", Group = "Movement")]
    public KeyboardShortcut MoveViewRight { get => moveViewRight; set { moveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom", Group = "Movement")]
    public KeyboardShortcut MoveViewBottom { get => moveViewBottom; set { moveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    

    [ShortcutName("Move View Left Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewLeft { get => fastMoveViewLeft; set { fastMoveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top Quicky", Group = "Movement")]
    public KeyboardShortcut FastMoveViewTop { get => fastMoveViewTop; set { fastMoveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewRight { get => fastMoveViewRight; set { fastMoveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewBottom { get => fastMoveViewBottom; set { fastMoveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    

    [ShortcutName("Move View Left Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewLeft { get => reallyFastMoveViewLeft; set { reallyFastMoveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top Really Quicky", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewTop { get => reallyFastMoveViewTop; set { reallyFastMoveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewRight { get => reallyFastMoveViewRight; set { reallyFastMoveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewBottom { get => reallyFastMoveViewBottom; set { reallyFastMoveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    

    public KeyboardShortcut Undo { get => undo; set { undo = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    public KeyboardShortcut Redo { get => redo; set { redo = value; CachedStrings = Utils.GetShortcutStrings(this); } }


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
    private KeyboardShortcut enlargeBrush = new(KeyboardKey.Equal);
    private KeyboardShortcut shrinkBrush = new(KeyboardKey.Minus);
    private MouseShortcut dragLevel = MouseButton.Middle;
    private MouseShortcut paint = MouseButton.Left;
    private MouseShortcut erase = MouseButton.Right;

    private KeyboardShortcut moveViewLeft = new(KeyboardKey.Left);
    private KeyboardShortcut moveViewTop = new(KeyboardKey.Up);
    private KeyboardShortcut moveViewRight = new(KeyboardKey.Right);
    private KeyboardShortcut moveViewBottom = new(KeyboardKey.Down);

    private KeyboardShortcut fastMoveViewLeft = new(KeyboardKey.Left, shift:true);
    private KeyboardShortcut fastMoveViewTop = new(KeyboardKey.Up, shift:true);
    private KeyboardShortcut fastMoveViewRight = new(KeyboardKey.Right, shift:true);
    private KeyboardShortcut fastMoveViewBottom = new(KeyboardKey.Down, shift:true);

    private KeyboardShortcut reallyFastMoveViewLeft = new(KeyboardKey.Left, alt:true);
    private KeyboardShortcut reallyFastMoveViewTop = new(KeyboardKey.Up, alt:true);
    private KeyboardShortcut reallyFastMoveViewRight = new(KeyboardKey.Right, alt:true);
    private KeyboardShortcut reallyFastMoveViewBottom = new(KeyboardKey.Down, alt:true);


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
    
    [ShortcutName("Increase Brush Size")]
    public KeyboardShortcut EnlargeBrush { get => enlargeBrush; set { enlargeBrush = value; CachedStrings = Utils.GetShortcutStrings(this); }}
    
    [ShortcutName("Decrease Brush Size")]
    public KeyboardShortcut ShrinkBrush { get => shrinkBrush; set { shrinkBrush = value; CachedStrings = Utils.GetShortcutStrings(this); }}

    [ShortcutName("Drag View")]
    public MouseShortcut DragLevel { get => dragLevel; set {dragLevel = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Paint")]
    public MouseShortcut Paint { get => paint; set {paint = value; CachedStrings = Utils.GetShortcutStrings(this);} }
    
    [ShortcutName("Erase")]
    public MouseShortcut Erase { get => erase; set {erase = value; CachedStrings = Utils.GetShortcutStrings(this);} }

    [ShortcutName("Move View Left", Group = "Movement")]
    public KeyboardShortcut MoveViewLeft { get => moveViewLeft; set { moveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top", Group = "Movement")]
    public KeyboardShortcut MoveViewTop { get => moveViewTop; set { moveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right", Group = "Movement")]
    public KeyboardShortcut MoveViewRight { get => moveViewRight; set { moveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom", Group = "Movement")]
    public KeyboardShortcut MoveViewBottom { get => moveViewBottom; set { moveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    

    [ShortcutName("Move View Left Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewLeft { get => fastMoveViewLeft; set { fastMoveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top Quicky", Group = "Movement")]
    public KeyboardShortcut FastMoveViewTop { get => fastMoveViewTop; set { fastMoveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewRight { get => fastMoveViewRight; set { fastMoveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewBottom { get => fastMoveViewBottom; set { fastMoveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    

    [ShortcutName("Move View Left Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewLeft { get => reallyFastMoveViewLeft; set { reallyFastMoveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top Really Quicky", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewTop { get => reallyFastMoveViewTop; set { reallyFastMoveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewRight { get => reallyFastMoveViewRight; set { reallyFastMoveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewBottom { get => reallyFastMoveViewBottom; set { reallyFastMoveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    


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
    private KeyboardShortcut toggleRopeGravity = new(KeyboardKey.G);
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
    private KeyboardShortcut resetPlacementRotation = new(KeyboardKey.Null);
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
    private KeyboardShortcut showCameras = KeyboardKey.Null;
    private KeyboardShortcut stretchPlacementVertical = KeyboardKey.Null;
    private KeyboardShortcut stretchPlacementHorizontal = KeyboardKey.Null;
    private KeyboardShortcut squeezePlacementVertical = KeyboardKey.Null;
    private KeyboardShortcut squeezePlacementHorizontal = KeyboardKey.Null;
    private KeyboardShortcut fastStretchPlacementVertical = KeyboardKey.Null;
    private KeyboardShortcut fastStretchPlacementHorizontal = KeyboardKey.Null;
    private KeyboardShortcut fastSqueezePlacementVertical = KeyboardKey.Null;
    private KeyboardShortcut fastSqueezePlacementHorizontal = KeyboardKey.Null;
    private KeyboardShortcut resetPlacementStretch = KeyboardKey.Null;

    private KeyboardShortcut verticalFlipPlacement = KeyboardKey.Null;
    private KeyboardShortcut horizontalFlipPlacement = KeyboardKey.Null;

    private KeyboardShortcut rotateRightAnglePlacement = KeyboardKey.Null;

    private KeyboardShortcut resetQuadVertices = KeyboardKey.Null;

    private KeyboardShortcut moveViewLeft = new(KeyboardKey.Left);
    private KeyboardShortcut moveViewTop = new(KeyboardKey.Up);
    private KeyboardShortcut moveViewRight = new(KeyboardKey.Right);
    private KeyboardShortcut moveViewBottom = new(KeyboardKey.Down);

    private KeyboardShortcut fastMoveViewLeft = new(KeyboardKey.Left, shift:true);
    private KeyboardShortcut fastMoveViewTop = new(KeyboardKey.Up, shift:true);
    private KeyboardShortcut fastMoveViewRight = new(KeyboardKey.Right, shift:true);
    private KeyboardShortcut fastMoveViewBottom = new(KeyboardKey.Down, shift:true);

    private KeyboardShortcut reallyFastMoveViewLeft = new(KeyboardKey.Left, alt:true);
    private KeyboardShortcut reallyFastMoveViewTop = new(KeyboardKey.Up, alt:true);
    private KeyboardShortcut reallyFastMoveViewRight = new(KeyboardKey.Right, alt:true);
    private KeyboardShortcut reallyFastMoveViewBottom = new(KeyboardKey.Down, alt:true);

    private KeyboardShortcut addPreset = new(KeyboardKey.Y, shift: true);

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
    [ShortcutName("Move Rope Segments", Group = "Rope Actions")]
    public KeyboardShortcut ToggleRopePointsEditingMode { get => toggleRopePointsEditingMode; set { toggleRopePointsEditingMode = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Simulate Rope", Group = "Rope Actions")]
    public KeyboardShortcut ToggleRopeEditingMode { get => toggleRopeEditingMode; set { toggleRopeEditingMode = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Toggle Rope Simulation Gravity", Group = "Rope Actions")]
    public KeyboardShortcut ToggleRopeGravity { get => toggleRopeGravity; set { toggleRopeGravity = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Cycle Selected", Group = "Selected Prop Actions")]
    public KeyboardShortcut CycleSelected { get => cycleSelected; set { cycleSelected = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Duplicate", Group = "Selected Prop Actions")]
    public KeyboardShortcut DuplicateProps { get => duplicateProps; set { duplicateProps = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Deepen", Group = "Selected Prop Actions")]
    public KeyboardShortcut DeepenSelectedProps { get => deepenSelectedProps; set { deepenSelectedProps = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Undeepen", Group = "Selected Prop Actions")]
    public KeyboardShortcut UndeepenSelectedProps { get => undeepenSelectedProps; set { undeepenSelectedProps = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    
    [ShortcutName("Reset Quad Vertices", Group = "Selected Prop Actions")]
    public KeyboardShortcut ResetQuadVertices { get => resetQuadVertices; set { resetQuadVertices = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    
    [ShortcutName("Rotate Clockwise", Group = "Selection or Placement")]
    public KeyboardShortcut RotateClockwise { get => rotateClockwise; set { rotateClockwise = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Rotate Counter-clockwise", Group = "Selection or Placement")]
    public KeyboardShortcut RotateCounterClockwise { get => rotateCounterClockwise; set { rotateCounterClockwise = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Reset Placement Rotation", Group = "Selection or Placement")]
    public KeyboardShortcut ResetPlacementRotation { get => resetPlacementRotation; set { resetPlacementRotation = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Increase Rope Segments", Group = "Rope Actions")]
    public KeyboardShortcut IncrementRopSegmentCount { get => incrementRopSegmentCount; set { incrementRopSegmentCount = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Decrease Rope Segments", Group = "Rope Actions")]
    public KeyboardShortcut DecrementRopSegmentCount { get => decrementRopSegmentCount; set { decrementRopSegmentCount = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    [ShortcutName("Rope Simulation/Bezier Switch", Group = "Rope Actions")]
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
    
    [ShortcutName("Show/Hide Cameras")]
    public KeyboardShortcut ShowCameras { get => showCameras; set { showCameras = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Stretch Placement Vertically", Group = "Placement")]
    public KeyboardShortcut StretchPlacementVertical { get => stretchPlacementVertical; set { stretchPlacementVertical = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Stretch Placement Horizontally", Group = "Placement")]
    public KeyboardShortcut StretchPlacementHorizontal { get => stretchPlacementHorizontal; set { stretchPlacementHorizontal = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Squeeze Placement Vertically", Group = "Placement")]
    public KeyboardShortcut SqueezePlacementVertical { get => squeezePlacementVertical; set { squeezePlacementVertical = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Squeeze Placement Horizontally", Group = "Placement")]
    public KeyboardShortcut SqueezePlacementHorizontal { get => squeezePlacementHorizontal; set { squeezePlacementHorizontal = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Fast Stretch Placement Vertically", Group = "Placement")]
    public KeyboardShortcut FastStretchPlacementVertical { get => fastStretchPlacementVertical; set { fastStretchPlacementVertical = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Fast Stretch Placement Horizontally", Group = "Placement")]
    public KeyboardShortcut FastStretchPlacementHorizontal { get => fastStretchPlacementHorizontal; set { fastStretchPlacementHorizontal = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Fast Squeeze Placement Vertically", Group = "Placement")]
    public KeyboardShortcut FastSqueezePlacementVertical { get => fastSqueezePlacementVertical; set { fastSqueezePlacementVertical = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Fast Squeeze Placement Horizontally", Group = "Placement")]
    public KeyboardShortcut FastSqueezePlacementHorizontal { get => fastSqueezePlacementHorizontal; set { fastSqueezePlacementHorizontal = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Reset Placement Stretch", Group = "Placement")]
    public KeyboardShortcut ResetPlacementStretch { get => resetPlacementStretch; set { resetPlacementStretch = value; CachedStrings = Utils.GetShortcutStrings(this); }}

    [ShortcutName("Vertically Flip", Group = "Placement")]
    public KeyboardShortcut VerticalFlipPlacement { get => verticalFlipPlacement; set { verticalFlipPlacement = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Horizontally Flip", Group = "Placement")]
    public KeyboardShortcut HorizontalFlipPlacement { get => horizontalFlipPlacement; set { horizontalFlipPlacement = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Rotate 90 Degrees", Group = "Placement")]
    public KeyboardShortcut RotateRightAnglePlacement { get => rotateRightAnglePlacement; set { rotateRightAnglePlacement = value; CachedStrings = Utils.GetShortcutStrings(this); } }

    [ShortcutName("Move View Left", Group = "Movement")]
    public KeyboardShortcut MoveViewLeft { get => moveViewLeft; set { moveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top", Group = "Movement")]
    public KeyboardShortcut MoveViewTop { get => moveViewTop; set { moveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right", Group = "Movement")]
    public KeyboardShortcut MoveViewRight { get => moveViewRight; set { moveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom", Group = "Movement")]
    public KeyboardShortcut MoveViewBottom { get => moveViewBottom; set { moveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    

    [ShortcutName("Move View Left Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewLeft { get => fastMoveViewLeft; set { fastMoveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top Quicky", Group = "Movement")]
    public KeyboardShortcut FastMoveViewTop { get => fastMoveViewTop; set { fastMoveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewRight { get => fastMoveViewRight; set { fastMoveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom Quickly", Group = "Movement")]
    public KeyboardShortcut FastMoveViewBottom { get => fastMoveViewBottom; set { fastMoveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    

    [ShortcutName("Move View Left Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewLeft { get => reallyFastMoveViewLeft; set { reallyFastMoveViewLeft = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Top Really Quicky", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewTop { get => reallyFastMoveViewTop; set { reallyFastMoveViewTop = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Right Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewRight { get => reallyFastMoveViewRight; set { reallyFastMoveViewRight = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    [ShortcutName("Move View Bottom Really Quickly", Group = "Movement")]
    public KeyboardShortcut ReallyFastMoveViewBottom { get => reallyFastMoveViewBottom; set { reallyFastMoveViewBottom = value; CachedStrings = Utils.GetShortcutStrings(this); } }
    
    public KeyboardShortcut AddPreset { get => addPreset; set { addPreset = value; CachedStrings = Utils.GetShortcutStrings(this); } }


    public IEnumerable<(string Name, string Shortcut)> CachedStrings { get; private set; }

    public PropsShortcuts()
    {
        CachedStrings = Utils.GetShortcutStrings(this);
    }
}

public class GlobalShortcuts : IEditorShortcuts
{
    public KeyboardShortcut ToMainPage { get; set; } = new(KeyboardKey.One);
    public KeyboardShortcut ToGeometryEditor { get; set; } = new(KeyboardKey.Two);
    public KeyboardShortcut ToTileEditor { get; set; } = new(KeyboardKey.Three);
    public KeyboardShortcut ToCameraEditor { get; set; } = new(KeyboardKey.Four);
    public KeyboardShortcut ToLightEditor { get; set; } = new(KeyboardKey.Five);
    public KeyboardShortcut ToDimensionsEditor { get; set; } = new(KeyboardKey.Six);
    public KeyboardShortcut ToEffectsEditor { get; set; } = new(KeyboardKey.Seven);
    public KeyboardShortcut ToPropsEditor { get; set; } = new(KeyboardKey.Eight);
    public KeyboardShortcut ToSettingsPage { get; set; } = new(KeyboardKey.Nine);

    public KeyboardShortcut TakeScreenshot { get; set; } = new(KeyboardKey.Null);

    public KeyboardShortcut CycleTileRenderModes { get; set; } = new(KeyboardKey.Null);
    public KeyboardShortcut CyclePropRenderModes { get; set; } = new(KeyboardKey.Null);

    public KeyboardShortcut Open { get; set; } = new(KeyboardKey.O, ctrl: true, shift: false, alt: false);
    public KeyboardShortcut QuickSave { get; set; } = new(KeyboardKey.S, shift: false, ctrl: true, alt: false);
    public KeyboardShortcut QuickSaveAs { get; set; } = new(KeyboardKey.S, shift: true, ctrl: true, alt: false);
    public KeyboardShortcut Render { get; set; } = new(KeyboardKey.Null);

    public KeyboardShortcut ToggleDebugScreen { get; set; } = new(KeyboardKey.F3);

    private KeyboardShortcut _moveToNextLevel = KeyboardKey.Null;
    private KeyboardShortcut _moveToPreviousLevel = KeyboardKey.Null;
    
    [ShortcutName("Move To Next Level")]
    public KeyboardShortcut MoveToNextLevel { get => _moveToNextLevel;
        set { _moveToNextLevel = value; CachedStrings = Utils.GetShortcutStrings(this); }
    }
    
    [ShortcutName("Move To Previous Level")]
    public KeyboardShortcut MoveToPreviousLevel { get => _moveToPreviousLevel;
        set { _moveToPreviousLevel = value; CachedStrings = Utils.GetShortcutStrings(this); }
    }

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