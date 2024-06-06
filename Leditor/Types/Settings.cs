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
    public ConColor WaterColor { get; set; }
    public bool LegacyGeoTools { get; set; }
    public bool ShowCameras { get; set; }
    public bool ShowTiles { get; set; }
    public bool ShowProps { get; set; }
    public bool AllowOutboundsPlacement { get; set; }
    public bool ShowCurrentGeoIndicator { get; set; }
    public bool LegacyInterface { get; set; }
    public bool PasteAir { get; set; }
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
    public bool HoveredTileInfo { get; set; } 
    public bool TintedTiles { get; set; }
    public bool UseTextures { get; set; }
    public bool AllowUndefinedTiles { get; set; }
    public bool Grid { get; set; }
    public bool ShowCameras { get; set; }
    public bool DrawCurrentTile { get; set; }
    public bool DrawCurrentSpecs { get; set; }
    public bool UseTexturesInTooltip { get; set; }
    public bool ImplicitOverrideMaterials { get; set; }
    public bool UnifiedDeletion { get; set; }
    public bool ExactHoverDeletion { get; set; }
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
        TintedTiles = false;
        UseTextures = false;
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
        bool tintedTiles = false, 
        bool useTextures = false,
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
        TintedTiles = tintedTiles;
        UseTextures = useTextures;
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
    public ConColor Background { get; set; }
    public ConColor LevelBackgroundLight { get; set; }
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

    public bool BlockyBrush { get; set; }
}

public enum TileDrawMode { Preview, Tinted, Palette }
public enum PropDrawMode { Untinted, Tinted, Palette }

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
    bool changingCategoriesResetsIndex = true,
    TileDrawMode drawTileMode = TileDrawMode.Preview,
    PropDrawMode drawPropMode = PropDrawMode.Untinted,
    bool indexHint = true,
    bool highLayerContrast = true,
    bool visiblePrecedingUnfocusedLayers = true,
    bool cropTilePreviews = false,
    int autoSaveSeconds = 120,
    bool autoSave = false,
    byte waterOpacity = 70,
    bool water = true,
    bool navbar = true,
    string renderingAssetsPath = ""
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
    public TileDrawMode DrawTileMode { get; set; } = drawTileMode;
    public PropDrawMode DrawPropMode { get; set; } = drawPropMode;
    public bool IndexHint { get; set; } = indexHint;
    public bool HighLayerContrast { get; set; } = highLayerContrast;
    public bool VisiblePrecedingUnfocusedLayers { get; set; } = visiblePrecedingUnfocusedLayers;
    public bool CropTilePreviews { get; set; } = cropTilePreviews;
    public bool AutoSave { get; set; } = autoSave;
    public int AutoSaveSeconds { get; set; } = autoSaveSeconds < 30 ? 30 : autoSaveSeconds;
    public byte WaterOpacity { get; set; } = waterOpacity;
    public bool Water { get; set; } = water;
    public bool Navbar { get; set; } = navbar;
    public string RenderingAssetsPath { get; set; } = renderingAssetsPath;
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