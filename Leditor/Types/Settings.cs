using System.Numerics;
using System.Security.Authentication.ExtendedProtection;
using System.Text.Json.Serialization;
namespace Leditor.Types;

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
        LayerIndicatorPosition = ScreenRelativePosition.TopLeft;
    }

    public GeoEditor(
        LayerColors layerColors,
        ConColor waterColor,
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
    public ConColor WaterColor { get; set; }

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
    public ConColor Background { get; set; }

    [SettingName("Level Background Color (Light Mode)")]
    public ConColor LevelBackgroundLight { get; set; }
    
    [SettingName("Level Background Color (Dark Mode)")]
    public ConColor LevelBackgroundDark { get; set; }

    public enum ScreenRelativePosition { 
        TopLeft, 
        TopRight, 
        BottomLeft, 
        BottomRight, 
        MiddleBottom, 
        MiddleTop 
    }

    public ScreenRelativePosition LightIndicatorPosition { get; set; }

    public LightEditorSettings()
    {
        Background = Color.Blue;
        LevelBackgroundLight = Color.Green;
        LevelBackgroundDark = Color.Yellow;

        LightIndicatorPosition = ScreenRelativePosition.TopLeft;
    }

    public LightEditorSettings(
        ConColor background, 
        ConColor levelBackgroundLight, 
        ConColor levelBackgroundDark,
        ScreenRelativePosition lightIndicatorPos
    )
    {
        Background = background;
        LevelBackgroundLight = levelBackgroundLight;
        LevelBackgroundDark = levelBackgroundDark;

        LightIndicatorPosition = lightIndicatorPos;
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

    //

    public PropEditor()
    {
        LayerIndicatorPosition = ScreenRelativePosition.TopLeft;
    }

    public PropEditor(ScreenRelativePosition layerIndicatorPosition)
    {
        LayerIndicatorPosition = layerIndicatorPosition;
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
        ConColor effectColorLight,
        ConColor effectColorDark,
        ConColor effectCanvasColorLight,
        ConColor effectCanvasColorDark,
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
        EffectColorLight = new ConColor(0, 255, 0, 255);
        EffectColorDark = Color.Yellow;
        EffectsCanvasColorLight = new ConColor(215, 66, 245, 100);
        EffectsCanvasColorDark = new ConColor(0, 0, 0, 0);
        BlockyBrush = true;
    }
    
    public ConColor EffectColorLight { get; set; }
    public ConColor EffectColorDark { get; set; }
    public ConColor EffectsCanvasColorLight { get; set; }
    public ConColor EffectsCanvasColorDark { get; set; }

    [SettingName("Blocky Brush")]
    public bool BlockyBrush { get; set; }
}

public enum TileDrawMode { Preview, Tinted, Palette }
public enum PropDrawMode { Untinted, Tinted, Palette }

public class GeneralSettings
{
    [SettingName("Developer Mode", Hidden = true)]
    public bool DeveloperMode { get; set; } = false;

    [SettingName("Default Font")]
    public bool DefaultFont { get; set; } = false;
    
    [SettingName("Global Camera")]
    public bool GlobalCamera { get; set; } = true;

    [SettingName("Shortcuts Window")]
    public bool ShortcutWindow { get; set; } = true;
    
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

    [SettingName("Ruler")]
    public bool IndexHint { get; set; } = true;

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
            background: new ConColor(66, 108, 245, 255),
            levelBackgroundLight: Color.White,
            levelBackgroundDark: new Color(200, 0, 0, 255),
            LightEditorSettings.ScreenRelativePosition.TopLeft);
        EffectsSettings = new(
            Color.Green, 
            Color.Yellow, 
            new ConColor(215, 66, 245, 100), 
            new ConColor(0, 0, 0, 0),
            true
        );
        PropEditor = new();
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