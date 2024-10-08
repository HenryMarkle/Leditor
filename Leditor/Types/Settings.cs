using System.Numerics;
using System.Security.Authentication.ExtendedProtection;
using System.Text.Json.Serialization;
using Leditor.Data.Props.Legacy;
namespace Leditor.Types;

public class LayerColors(Data.Color layer1, Data.Color layer2, Data.Color layer3)
{
    public Data.Color Layer1 { get; set; } = layer1;
    public Data.Color Layer2 { get; set; } = layer2;
    public Data.Color Layer3 { get; set; } = layer3;
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
        LayerIndicatorPosition = ScreenRelativePosition.TopLeft;
    }

    public GeoEditor(
        LayerColors layerColors,
        Data.Color waterColor,
        bool legacyGeoTools = false,
        bool allowOutboundsPlacement = false,
        bool showCameras = false,
        bool showTiles = false,
        bool showProps = false,
        bool showCurrentGeoIndicator = true,
        bool legacyInterface = false,
        bool pasteAir = false,
        bool basicView = true,
        ScreenRelativePosition layerIndicatorPosition = ScreenRelativePosition.TopLeft
    )
    {
        LayerColors = layerColors;
        WaterColor = waterColor;
        LegacyGeoTools = legacyGeoTools;
        ShowCameras = showCameras;
        ShowTiles = showTiles;
        ShowProps = showProps;
        AllowOutboundsPlacement = allowOutboundsPlacement;
        ShowCurrentGeoIndicator = showCurrentGeoIndicator;
        LegacyInterface = legacyInterface;
        PasteAir = pasteAir;
        BasicView = basicView;
        LayerIndicatorPosition = layerIndicatorPosition;
    }
    
    public LayerColors LayerColors { get; set; }

    [SettingName("Water Color")]
    public Data.Color WaterColor { get; set; }

    [SettingName("Lgacy Geo Tools", Hidden = true)]
    public bool LegacyGeoTools { get; set; }

    [SettingName("Show/Hide Cameras")]
    public bool ShowCameras { get; set; }

    [SettingName("Camera's Inner Boundries")]
    [SettingDependancy("ShowCameras")]
    public bool CameraInnerBoundires { get; set; }

    [SettingName("Show/Hide Tiles")]
    public bool ShowTiles { get; set; }

    [SettingName("Show/Hide Props")]
    public bool ShowProps { get; set; }

    [SettingName("Allow Placing Features Out of Bounds")]
    public bool AllowOutboundsPlacement { get; set; }

    [SettingName("Currently-Held Geometry Indicator")]
    public bool ShowCurrentGeoIndicator { get; set; }

    [SettingName("Legacy Interface")]
    public bool LegacyInterface { get; set; }

    [SettingName("Paste Air")]
    public bool PasteAir { get; set; }

    [SettingName("Basic View")]
    public bool BasicView { get; set; }

    [SettingName("Ruler")]
    public bool IndexHint { get; set; } = true;



    public enum ScreenRelativePosition { 
        TopLeft, 
        TopRight, 
        BottomLeft, 
        BottomRight, 
        MiddleBottom, 
        MiddleTop 
    }

    public ScreenRelativePosition LayerIndicatorPosition { get; set; }
}

public class TileEditorSettings
{    
    [SettingName("Hovered Tile Debug Info", Group = "Brush")]
    public bool HoveredTileInfo { get; set; } 

    [SettingName("Allow Undefined Tiles", Description = "When disabled, the level won't load if it has undefined tiles")]
    public bool AllowUndefinedTiles { get; set; }

    [SettingName("Visible Stray Tile Fragments")]
    public bool ShowStrayTileFragments { get; set; } = true;

    [SettingName("Show/Hide Grid")]
    public bool Grid { get; set; }

    [SettingName("Show/Hide Cameras")]
    public bool ShowCameras { get; set; }

    [SettingName("Camera's Inner Boundries")]
    [SettingDependancy("ShowCameras")]
    public bool CameraInnerBoundires { get; set; }

    [SettingName("Show/Hide Currently-Held Tile", Group = "Brush")]
    public bool DrawCurrentTile { get; set; }

    [SettingName("Show/Hide Currently-Held Tile Specs", Group = "Brush")]
    public bool DrawCurrentSpecs { get; set; }

    [SettingName("Tooltips Use Tile Textures")]
    public bool UseTexturesInTooltip { get; set; }

    [SettingName("Paint Over Materials", Group = "Brush")]
    public bool ImplicitOverrideMaterials { get; set; }

    [SettingName("Eraser Tool Erases Both Tiles and Materials", Group = "Brush")]
    public bool UnifiedDeletion { get; set; }

    [SettingName("Only Start Deleting When There's a Tile or a Material Under The Cursor", Group = "Brush")]
    public bool ExactHoverDeletion { get; set; }

    [SettingName("Restore Original Deletion Behavior", Description = "Will disable exact hover and unified deletion settings", Group = "Brush")]
    public bool OriginalDeletionBehavior { get; set; }

    [SettingName("Unified Placement Preview Color")]
    public bool UnifiedInlinePreviewColor { get; set; }

    [SettingName("Unified Preview Color")]
    public bool UnifiedPreviewColor { get; set; }

    [SettingName("Use Textures For Placement Preview")]
    public bool InlineTextures { get; set; }

    [SettingName("Ruler")]
    public bool IndexHint { get; set; } = true;

    [SettingName("Show Props")]
    public bool ShowProps { get; set; } = false;

    [SettingName("Deep Tile Copy", Description = "Copies two layer")]
    public bool DeepTileCopy { get; set; } = false;

    public enum ScreenRelativePosition { 
        TopLeft, 
        TopRight, 
        BottomLeft, 
        BottomRight, 
        MiddleBottom, 
        MiddleTop 
    }

    public ScreenRelativePosition LayerIndicatorPosition { get; set; }
    

    public IEnumerable<AutoTiler.PathPackMeta> AutoTilerPathPacks { get; set; }

    public IEnumerable<AutoTiler.BoxPackMeta> AutoTilerBoxPacks { get; set; }

    public TileEditorSettings() {
        HoveredTileInfo = false;
        AllowUndefinedTiles = true;
        Grid = false;
        ShowCameras = false;
        DrawCurrentTile = true;
        DrawCurrentSpecs = false;
        UseTexturesInTooltip = false;
        ImplicitOverrideMaterials = true;
        UnifiedDeletion = true;
        ExactHoverDeletion = false;
        OriginalDeletionBehavior = true;

        LayerIndicatorPosition = ScreenRelativePosition.TopLeft;

        AutoTilerPathPacks = [
            new("Thin Pipes", "Vertical Pipe", "Horizontal Pipe", "Pipe ES", "Pipe WS", "Pipe WN", "Pipe EN", "Pipe XJunct", "Pipe TJunct S", "Pipe TJunct W", "Pipe TJunct N", "Pipe TJunct E"),
            new("Thin Plain Pipes", "Vertical Plain Pipe", "Horizontal Plain Pipe", "Pipe ES", "Pipe WS", "Pipe WN", "Pipe EN", "Pipe XJunct", "Pipe TJunct S", "Pipe TJunct W", "Pipe TJunct N", "Pipe TJunct E"),
            new("Wall Wires",  "WallWires Vertical A", "WallWires Horizontal A", "WallWires Square SE", "WallWires Square SW", "WallWires Square NW", "WallWires Square NE", "WallWires X Section", "WallWires T Section S", "WallWires T Section W", "WallWires T Section N", "WallWires T Section E"),
            new("Inside Thin Pipes", "insidePipeVertical", "insidePipeHorizontal", "insidePipeRD", "insidePipeLD", "insidePipeLU", "insidePipeRU", "", "", "", "", ""),
        ];

        AutoTilerBoxPacks = [
            new("Su Patterns", "Block Edge W", "Block Edge N", "Block Edge E", "Block Edge S", "Block Corner NW", "Block Corner NE", "Block Corner SE", "Block Corner SW", [])
        ];
    }

    public TileEditorSettings(
        bool hoveredTileInfo = false, 
        bool allowUndefinedTiles = true,
        bool grid = false,
        bool showCameras = false,
        bool drawCurrentTile = true,
        bool drawCurrentSpecs = false,
        bool useTexturesInTooltip = false,
        bool implicitOverrideMaterials = true,
        bool unifiedDeletion = true,
        bool exactHoverDeletion = false,
        bool originalDeletionBhavior = true,
        ScreenRelativePosition layerIndicatorPosition = ScreenRelativePosition.TopLeft
    ) {
        HoveredTileInfo = hoveredTileInfo;
        AllowUndefinedTiles = allowUndefinedTiles;
        Grid = grid;
        ShowCameras = showCameras;
        DrawCurrentTile = drawCurrentTile;
        DrawCurrentSpecs = drawCurrentSpecs;
        UseTexturesInTooltip = useTexturesInTooltip;
        ImplicitOverrideMaterials = implicitOverrideMaterials;
        UnifiedDeletion = unifiedDeletion;
        ExactHoverDeletion = exactHoverDeletion;
        OriginalDeletionBehavior = originalDeletionBhavior;

        LayerIndicatorPosition = layerIndicatorPosition;

        AutoTilerPathPacks = [
            new("Thin Pipes", "Vertical Pipe", "Horizontal Pipe", "Pipe ES", "Pipe WS", "Pipe WN", "Pipe EN", "Pipe XJunct", "Pipe TJunct S", "Pipe TJunct W", "Pipe TJunct N", "Pipe TJunct E"),
            new("Thin Plain Pipes", "Vertical Plain Pipe", "Horizontal Plain Pipe", "Pipe ES", "Pipe WS", "Pipe WN", "Pipe EN", "Pipe XJunct", "Pipe TJunct S", "Pipe TJunct W", "Pipe TJunct N", "Pipe TJunct E"),
            new("Wall Wires",  "WallWires Vertical A", "WallWires Horizontal A", "WallWires Square SE", "WallWires Square SW", "WallWires Square NW", "WallWires Square NE", "WallWires X Section", "WallWires T Section S", "WallWires T Section W", "WallWires T Section N", "WallWires T Section E"),
            new("Inside Thin Pipes", "insidePipeVertical", "insidePipeHorizontal", "insidePipeRD", "insidePipeLD", "insidePipeLU", "insidePipeRU", "", "", "", "", ""),
        ];

        AutoTilerBoxPacks = [
            new("Su Patterns", "Block Edge W", "Block Edge N", "Block Edge E", "Block Edge S", "Block Corner NW", "Block Corner NE", "Block Corner SE", "Block Corner SW", [])
        ];
    }
}

public class LightEditorSettings
{
    [SettingName("View Background Color")]
    public Data.Color Background { get; set; }

    [SettingName("Level Background Color (Light Mode)")]
    public Data.Color LevelBackgroundLight { get; set; }
    
    [SettingName("Level Background Color (Dark Mode)")]
    public Data.Color LevelBackgroundDark { get; set; }

    public enum ScreenRelativePosition { 
        TopLeft, 
        TopRight, 
        BottomLeft, 
        BottomRight, 
        MiddleBottom, 
        MiddleTop 
    }

    public ScreenRelativePosition LightIndicatorPosition { get; set; }

    public enum LightProjection { 
        None, 
        Basic, 
        ThreeLayers 
    }

    public LightProjection Projection { get; set; } 

    [SettingName("Undo Limit", Description = "The maximum the number of actions you can reverse")]
    public int UndoLimit { get; set; }

    public LightEditorSettings()
    {
        Background = Color.Blue;
        LevelBackgroundLight = Color.Green;
        LevelBackgroundDark = Color.Yellow;

        LightIndicatorPosition = ScreenRelativePosition.TopLeft;
        Projection = LightProjection.Basic;

        UndoLimit = 30;
    }

    public LightEditorSettings(
        Data.Color background, 
        Data.Color levelBackgroundLight, 
        Data.Color levelBackgroundDark,
        ScreenRelativePosition lightIndicatorPos,
        LightProjection projection,
        int undoLimit
    )
    {
        Background = background;
        LevelBackgroundLight = levelBackgroundLight;
        LevelBackgroundDark = levelBackgroundDark;

        LightIndicatorPosition = lightIndicatorPos;
        Projection = projection;

        UndoLimit = undoLimit;
    }
}

public class Experimental(bool newGeometryEditor = false)
{
    public bool NewGeometryEditor { get; set; } = newGeometryEditor;
}

public class PropEditor
{
    public enum ScreenRelativePosition { 
        TopLeft, 
        TopRight, 
        BottomLeft, 
        BottomRight, 
        MiddleBottom, 
        MiddleTop 
    }

    public ScreenRelativePosition LayerIndicatorPosition { get; set; }

    [SettingName("Show/Hide Cameras' Boundries")]
    public bool Cameras { get; set; }

    [SettingName("Show Cameras' Inner boundries")]
    [SettingDependancy("Cameras")]
    public bool CamerasInnerBoundries { get; set; }

    [SettingName("Cross-Layer Selection")]
    public bool CrossLayerSelection { get; set; }

    [SettingName("Auto-Simulate Selected Ropes")]
    public bool SimulateSelectedRopes { get; set; }

    [SettingName("Show Presets Window")]
    public bool PresetsWindow { get; set; }

    [SettingName("Saved Presets", Hidden = true)]
    [JsonInclude]
    public List<PropPresetMeta> SavedPresets { get; set; }

    //

    public PropEditor()
    {
        LayerIndicatorPosition = ScreenRelativePosition.TopLeft;
        PresetsWindow = false;
        SavedPresets = new();
    }

    public PropEditor(ScreenRelativePosition layerIndicatorPosition)
    {
        LayerIndicatorPosition = layerIndicatorPosition;
        SavedPresets = new();
    }
}

public class CameraEditorSettings(bool snap = true, bool alignment = false)
{
    public bool Snap { get; set; } = snap;
    public bool Alignment { get; set; } = alignment;
}

public class EffectsSettings
{
    public EffectsSettings(
        Data.Color effectColorLight,
        Data.Color effectColorDark,
        Data.Color effectCanvasColorLight,
        Data.Color effectCanvasColorDark,
        bool blockyBrush)
    {
        EffectColorLight = effectColorLight;
        EffectColorDark = effectColorDark;
    
        EffectsCanvasColorLight = effectCanvasColorLight;
        EffectsCanvasColorDark = effectCanvasColorDark;

        BlockyBrush = blockyBrush;
    }
    
    public EffectsSettings()
    {
        EffectColorLight = new Data.Color(0, 255, 0, 255);
        EffectColorDark = Color.Yellow;
        EffectsCanvasColorLight = new Data.Color(215, 66, 245, 100);
        EffectsCanvasColorDark = new Data.Color(0, 0, 0, 0);
        BlockyBrush = true;
    }
    
    public Data.Color EffectColorLight { get; set; }
    public Data.Color EffectColorDark { get; set; }
    public Data.Color EffectsCanvasColorLight { get; set; }
    public Data.Color EffectsCanvasColorDark { get; set; }

    [SettingName("Blocky Brush")]
    public bool BlockyBrush { get; set; }
}

public enum TileDrawMode { Preview, Tinted, Palette }
public enum PropDrawMode { Untinted, Tinted, Palette }

public class GeneralSettings
{
    [SettingName("Developer Mode", Hidden = true)]
    public bool DebugScreen { get; set; } = false;

    [SettingName("Default Font")]
    public bool DefaultFont { get; set; } = false;
    
    [SettingName("Global Camera")]
    public bool GlobalCamera { get; set; } = true;

    [SettingName("Shortcuts Window")]
    public bool ShortcutWindow { get; set; } = true;

    [SettingName("Auto-Save Before Rendering", Description = "If you render before saving your level, your new changes won't appear in the final render. This option was made to automatically apply your changes before rendering.")]
    public bool AutoSaveBeforeRendering { get; set; } = true;
    
    [SettingName("Dark Theme")]
    public bool DarkTheme { get; set; } = false;
    
    [SettingName("Cache Rendering Runtime", Description = "Initializes the renderer ahead of time for faster rendering, at the cost of longer program startup time.")]
    public bool CacheRendererRuntime { get; set; } = false;
    
    [SettingName("Linear Zooming", Disabled = true)]
    public bool LinearZooming { get; set; } = false;

    [SettingName("Default Zoom")]
    public float DefaultZoom { get; set; } = 1;

    [SettingName("Cycle Menus")]
    public bool CycleMenus { get; set; } = true;

    [SettingName("Changing Categories Resets Index")]
    public bool ChangingCategoriesResetsIndex { get; set; } = true;

    [SettingName("Draw Tile Mode", Hidden = true)]
    public TileDrawMode DrawTileMode { get; set; } = TileDrawMode.Preview;

    [SettingName("Draw Prop Mode", Hidden = true)]
    public PropDrawMode DrawPropMode { get; set; } = PropDrawMode.Untinted;

    [SettingName("Keyboard Movement Steps", Group = "Movement")]
    public int KeyboardMovementSteps { get; set; } = 1;

    [SettingName("Fast Keyboard Movement Steps", Group = "Movement")]
    public int FastKeyboardMovementSteps { get; set; } = 5;

    [SettingName("Really Keyboard Movement Steps", Group = "Movement")]
    public int ReallyKeyboardMovementSteps { get; set; } = 10;

    [SettingName("Colorful Cameras")]
    public bool ColorfulCameras { get; set; }

    [SettingName("High Layer Contrast", Hidden = true)]
    public bool HighLayerContrast { get; set; } = true;

    [SettingName("Show Preceeding Layers", Hidden = true)]
    public bool VisiblePrecedingUnfocusedLayers { get; set; } = true;

    [SettingName("Crop Tile Previews", Hidden = true)]
    public bool CropTilePreviews { get; set; } = false;

    [SettingName("Auto-Save")]
    public bool AutoSave { get; set; } = false;

    [SettingName("Auto-Save (in seconds)")]
    [IntBounds(30)]
    public int AutoSaveSeconds { get; set; } = 30;
    
    [SettingName("Water Opacity", Hidden = true)]
    public byte WaterOpacity { get; set; } = 70;

    [SettingName("Water", Hidden = true)]
    public bool Water { get; set; } = true;

    [SettingName("Navigation Bar")]
    public bool Navbar { get; set; } = true;

    [SettingName("Rendering Assets Path", Description = "Leave empty for default directory")]
    [StringBounds(0, MaxLength = 256)]
    public string RenderingAssetsPath { get; set; } = "";

    [SettingName("Pressing Esc Crashes The Program")]
    public bool PressingEscCrashes { get; set; }

    [SettingName("Never Show Missing Tile Textures Alert Again", Hidden = true)]
    public bool NeverShowMissingTileTexturesAlertAgain { get; set; }

    [SettingName("Material White Space", Description = "The space between material rectangles")]
    public int MaterialWhiteSpace { get; set; } = 6;

    [SettingName("Current Layer At Front", Description = "Render the current layer above all other layers")]
    public bool CurrentLayerAtFront { get; set; } = false;

    [SettingName("Inverse Bilinear Interpolation", Description = "Heavy on performance and does not affect rendering")]
    public bool InverseBilinearInterpolation { get; set; } = true;

    [SettingName("Prop Opacity")]
    public int PropOpacity { get; set; } = 255;
}


public class L4MakerSettings {

    [SettingName("Background Color")]
    public Data.Color BackgroundColor { get; set; } = new(0, 0, 0, 255);

    [SettingName("Layer 3 Color")]
    public Data.Color Layer3Color { get; set; } = new(50, 50, 50, 255);
    
    [SettingName("Layer 2 Color")]
    public Data.Color Layer2Color { get; set; } = new(100, 100, 100, 255);
    
    [SettingName("Layer 1 Color")]
    public Data.Color Layer1Color { get; set; } = new(150, 150, 150, 255);
}

public class Settings
{
    public GeneralSettings GeneralSettings { get; set; }
    public Shortcuts Shortcuts { get; set; }
    public Misc Misc { get; set; }
    public GeoEditor GeometryEditor { get; set; }
    public TileEditorSettings TileEditor { get; set; }
    public CameraEditorSettings CameraSettings { get; set; }
    public LightEditorSettings LightEditor { get; set; }
    public EffectsSettings EffectsSettings { get; set; }
    public PropEditor PropEditor { get; set; }
    public L4MakerSettings L4Maker { get; set; }
    public Experimental Experimental { get; set; }

    [JsonConstructor]
    public Settings()
    {
        GeneralSettings = new();
        Shortcuts = new();
        Misc = new();
        GeometryEditor = new(new LayerColors(Color.Black with { A = 255 }, Color.Green with { A = 70 }, Color.Red with { A = 70 }), Color.Blue with { A = 50 });
        TileEditor = new();
        CameraSettings = new();
        LightEditor = new LightEditorSettings(
            background: new Data.Color(66, 108, 245, 255),
            levelBackgroundLight: Color.White,
            levelBackgroundDark: new Color(200, 0, 0, 255),
            LightEditorSettings.ScreenRelativePosition.TopLeft,
            LightEditorSettings.LightProjection.Basic,
            30
        );
        EffectsSettings = new(
            Color.Green, 
            Color.Yellow, 
            new Data.Color(215, 66, 245, 100), 
            new Data.Color(0, 0, 0, 0),
            true
        );
        PropEditor = new();
        L4Maker = new();
        Experimental = new();
    }
    
    public Settings(
        GeneralSettings generalSettings,
        Shortcuts shortcuts,
        Misc misc,
        GeoEditor geometryEditor,
        TileEditorSettings tileEditor,
        CameraEditorSettings cameraEditorSettings,
        LightEditorSettings lightEditor,
        EffectsSettings effectsSettings,
        PropEditor propEditor,
        L4MakerSettings l4Maker,
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
         L4Maker = l4Maker;
         Experimental = experimental;
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
        TileViewer = new TileViewerShortcuts();
    }
    
    public Shortcuts(
        GlobalShortcuts globalShortcuts,
        GeoShortcuts geoEditor,
        ExperimentalGeoShortcuts experimentalGeoShortcuts,
        TileShortcuts tileEditor,
        CameraShortcuts cameraEditor,
        LightShortcuts lightEditor,
        EffectsShortcuts effectsEditor,
        PropsShortcuts propsEditor,
        TileViewerShortcuts tileViewer
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
        TileViewer = tileViewer;
    }
    
    public GlobalShortcuts GlobalShortcuts { get; set; }
    public GeoShortcuts GeoEditor { get; set; }
    
    public ExperimentalGeoShortcuts ExperimentalGeoShortcuts { get; set; }
    public TileShortcuts TileEditor { get; set; }
    public CameraShortcuts CameraEditor { get; set; }
    public LightShortcuts LightEditor { get; set; }
    public EffectsShortcuts EffectsEditor { get; set; }
    public PropsShortcuts PropsEditor { get; set; }
    public TileViewerShortcuts TileViewer { get; set; }
}

public class Misc(
    bool splashScreen = false,
    int tileImageScansPerFrame = 15,
    int fps = 60,
    bool funnyDeathScreen = false
)
{
    public bool SplashScreen { get; set; } = splashScreen;
    public int TileImageScansPerFrame { get; set; } = tileImageScansPerFrame;
    public int FPS { get; set; } = fps;
    public bool FunnyDeathScreen { get; set; } = funnyDeathScreen;
}