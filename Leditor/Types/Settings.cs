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
        bool basicView = true
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
}

public class TileEditor(
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
    bool originalDeletionBhavior = true
    )
{
    public bool HoveredTileInfo { get; set; } = hoveredTileInfo;
    public bool TintedTiles { get; set; } = tintedTiles;
    public bool UseTextures { get; set; } = useTextures;
    public bool AllowUndefinedTiles { get; set; } = allowUndefinedTiles;
    public bool Grid { get; set; } = grid;
    public bool ShowCameras { get; set; } = showCameras;
    public bool DrawCurrentTile { get; set; } = drawCurrentTile;
    public bool DrawCurrentSpecs { get; set; } = drawCurrentSpecs;
    public bool UseTexturesInTooltip { get; set; } = useTexturesInTooltip;
    public bool ImplicitOverrideMaterials { get; set; } = implicitOverrideMaterials;
    public bool UnifiedDeletion { get; set; } = unifiedDeletion;
    public bool ExactHoverDeletion { get; set; } = exactHoverDeletion;
    public bool OriginalDeletionBehavior { get; set; } = originalDeletionBhavior;
    
    public IEnumerable<AutoTiler.PathPackMeta> AutoTilerPathPacks { get; set; } = [
        new("Thin Pipes", "Vertical Pipe", "Horizontal Pipe", "Pipe ES", "Pipe WS", "Pipe WN", "Pipe EN", "Pipe XJunct", "Pipe TJunct S", "Pipe TJunct W", "Pipe TJunct N", "Pipe TJunct E"),
        new("Thin Plain Pipes", "Vertical Plain Pipe", "Horizontal Plain Pipe", "Pipe ES", "Pipe WS", "Pipe WN", "Pipe EN", "Pipe XJunct", "Pipe TJunct S", "Pipe TJunct W", "Pipe TJunct N", "Pipe TJunct E"),
        new("Wall Wires",  "WallWires Vertical A", "WallWires Horizontal A", "WallWires Square SE", "WallWires Square SW", "WallWires Square NW", "WallWires Square NE", "WallWires X Section", "WallWires T Section S", "WallWires T Section W", "WallWires T Section N", "WallWires T Section E"),
        new("Inside Thin Pipes", "insidePipeVertical", "insidePipeHorizontal", "insidePipeRD", "insidePipeLD", "insidePipeLU", "insidePipeRU", "", "", "", "", ""),
    ];

    public IEnumerable<AutoTiler.BoxPackMeta> AutoTilerBoxPacks { get; set; } = [
        new("Su Patterns", "Block Edge W", "Block Edge N", "Block Edge E", "Block Edge S", "Block Corner NW", "Block Corner NE", "Block Corner SE", "Block Corner SW", [])
    ];
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
        GeometryEditor = new(new LayerColors(Color.Black with { A = 255 }, Color.Green with { A = 70 }, Color.Red with { A = 70 }), Color.Blue with { A = 50 });
        TileEditor = new();
        CameraSettings = new();
        LightEditor = new LightEditor(
            background: new ConColor(66, 108, 245, 255),
            levelBackgroundLight: Color.White,
            levelBackgroundDark: new Color(200, 0, 0, 255));
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