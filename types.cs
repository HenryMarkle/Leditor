namespace Leditor;

using Leditor.Common;
#nullable enable

// Used to report the tile check staatus when loading a project
public enum TileCheckResult {
    Ok, Missing, NotFound, MissingTexture, MissingMaterial
}

// Used for loading project files
public class LoadFileResult {
    public bool Success { get; init; } = false;

    public int Width { get; init; } = 0;
    public int Height { get; init; } = 0;

    public BufferTiles BufferTiles { get; init; } = new();
    public (string, double[,])[] Effects { get; init; } = [];

    public RunCell[,,]? GeoMatrix { get; init; } = null;
    public TileCell[,,]? TileMatrix { get; init; } = null;
    public Color[,,]? MaterialColorMatrix { get; init; } = null;

    public Image LightMapImage { get; init; }

    public List<RenderCamera> Cameras { get; set; } = [];

    public string Name { get; init; } = "New Project";
}

public enum EffectLayer1 { 
    All, 
    First, 
    Second, 
    Third, 
    FirstAndSecond, 
    SeconsAndThird
};

public enum EffectLayer2 { 
    First, 
    Second, 
    Third, 
};

public enum EffectColor {
    Color1,
    Color2,
    Dead
}

public enum EffectFatness {
    OnePixel,
    TwoPixels,
    ThreePixels,
    Random
}

public enum EffectSize {
    Small,
    FAT
}

public enum EffectColored {
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
) {
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

public static class Effects {
    public static string[][] Names { get; } = [
        [ "Slime", "Melt", "Rust", "Barnacles", "Rubble", "DecalsOnlySlime" ],
        [ "Roughen", "SlimeX3", "Super Melt", "Destructive Melt", "Erode", "Super Erode", "DaddyCorruption" ],
        [ "Wires", "Chains" ],
        [ "Root Grass", "Seed Pods", "Growers", "Cacti", "Rain Moss", "Hang Roots", "Grass" ],
        [ "Arm Growers", "Horse Tails", "Circuit Plants", "Feather Plants", "Thorn Growers", "Rollers", "Garbage Spirals" ],
        [ "Thick Roots", "Shadow Plants" ],
        [ "Fungi Flowers", "Lighthouse Flowers", "Fern", "Giant Mushroom", "Sprawlbush", "featherFern", "Fungus Tree" ],
        [ "BlackGoo", "DarkSlime" ],
        [ "Restore As Scaffolding", "Ceramic Chaos" ],
        [ "Colored Hang Roots", "Colored Thick Roots", "Colored Shadow Plants", "Colored Lighthouse Flowers", "Colored Fungi Flowers", "Root Plants" ],
        [ "Foliage", "Mistletoe", "High Fern", "High Grass", "Little Flowers", "Wastewater Mold" ],
        [ "Spinets", "Small Springs", "Mini Growers", "Clovers", "Reeds", "Lavenders", "Dense Mold" ],
        [ "Ultra Super Erode", "Impacts" ],
        [ "Super BlackGoo", "Stained Glass Properties" ],
        [ "Colored Barnacles", "Colored Rubble", "Fat Slime" ],
        [ "Assorted Trash", "Colored Wires", "Colored Chains", "Ring Chains" ],
        [ "Left Facing Kelp", "Right Facing Kelp", "Mixed Facing Kelp", "Bubble Grower", "Moss Wall", "Club Moss" ],
        [ "Ivy" ],
        [ "Fuzzy Growers" ]
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
    public static EffectOptions GetEffectOptions(string effect) => effect switch {
        "Slime"                 => new(threeD: false),
        "SlimeX3"               => new(threeD: false),
        "Rust"                  => new(threeD: false),
        "Barnacles"             => new(threeD: false),
        "Super Melt"            => new(threeD: false),
        "Destructive Melt"      => new(threeD: false),
        "Rubble"                => new(layers: EffectLayer1.All),
        
        "Fungi Flowers"
        or
        "Lighthouse Flowers"    => new(layers2: EffectLayer2.First),
        
        
        "Fern"
        or
        "Giant Mushroom"
        or
        "sprawlBush"
        or
        "featherFern"
        or
        "Fungus Tree"           => new(layers2: EffectLayer2.First, color: EffectColor.Color2),


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
        "Feather Plants"        => new(layers: EffectLayer1.All, color: EffectColor.Color2),


        "Rollers"
        or
        "Thorn Growers"
        or
        "Garbage Spirals"       => new(layers: EffectLayer1.All, color: EffectColor.Color2),

        
        "Wires"                 => new(layers: EffectLayer1.All, fatness: EffectFatness.TwoPixels),

        "Chains"                => new(layers: EffectLayer1.All, size: EffectSize.Small),

        
        "Hang Roots"
        or
        "Thick Roots"
        or
        "Shadow Plants"         => new(layers: EffectLayer1.All),

        
        "Restore As Scaffolding"=> new(layers: EffectLayer1.All),

        "Restore As Pipes"      => new(layers: EffectLayer1.All),

        "Ceramic Chaos"         => new(colored: EffectColored.White),


        _ => new()
    };
}

public record GeoShortcuts(
    KeyboardKey ToRightGeo  = KeyboardKey.KEY_D,
    KeyboardKey ToLeftGeo   = KeyboardKey.KEY_A,
    KeyboardKey ToTopGeo    = KeyboardKey.KEY_W,
    KeyboardKey ToBottomGeo = KeyboardKey.KEY_S,
    KeyboardKey CycleLayer  = KeyboardKey.KEY_L,
    KeyboardKey ToggleGrid  = KeyboardKey.KEY_M,

    MouseButton Draw = MouseButton.MOUSE_BUTTON_LEFT,
    MouseButton DragLevel = MouseButton.MOUSE_BUTTON_RIGHT
);

public record TileShortcuts(
    KeyboardKey FocusOnTileMenue            = KeyboardKey.KEY_D,
    KeyboardKey FocusOnTileCategoryMenu     = KeyboardKey.KEY_A,
    KeyboardKey MoveDown                    = KeyboardKey.KEY_S,
    KeyboardKey MoveUp                      = KeyboardKey.KEY_W,
    KeyboardKey CycleLayer                  = KeyboardKey.KEY_L,
    KeyboardKey ToggleTileSpecs             = KeyboardKey.KEY_T,
    
    KeyboardKey ToggleLayer1 = KeyboardKey.KEY_Z,
    KeyboardKey ToggleLayer2 = KeyboardKey.KEY_X,
    KeyboardKey ToggleLayer3 = KeyboardKey.KEY_C,

    KeyboardKey ToggleLayer1Tiles = KeyboardKey.KEY_Z,
    KeyboardKey ToggleLayer2Tiles = KeyboardKey.KEY_X,
    KeyboardKey ToggleLayer3Tiles = KeyboardKey.KEY_C,

    MouseButton Draw = MouseButton.MOUSE_BUTTON_LEFT,
    MouseButton DragLevel = MouseButton.MOUSE_BUTTON_RIGHT
);

public record CameraShortcust(
    MouseButton DragButton   = MouseButton.MOUSE_BUTTON_RIGHT,
    KeyboardKey NewCamera    = KeyboardKey.KEY_N,
    KeyboardKey DeleteCamera = KeyboardKey.KEY_D,
    KeyboardKey NewAndDelete = KeyboardKey.KEY_SPACE
);

public record LightShortcuts(
    KeyboardKey IncreaseFlatness = KeyboardKey.KEY_I,
    KeyboardKey DecreaseFlatness = KeyboardKey.KEY_K,
    KeyboardKey IncreaseAngle = KeyboardKey.KEY_L,
    KeyboardKey DecreaseAngle = KeyboardKey.KEY_J,

    KeyboardKey NextBrush = KeyboardKey.KEY_F,
    KeyboardKey PreviousBrush = KeyboardKey.KEY_R,

    KeyboardKey RotateBrushCounterClockwise = KeyboardKey.KEY_Q,
    KeyboardKey RotateBrushClockwise = KeyboardKey.KEY_E,

    KeyboardKey StretchBrushVertically = KeyboardKey.KEY_W,
    KeyboardKey SqueezeBrushVertically = KeyboardKey.KEY_S,
    KeyboardKey StretchBrushHorizontally = KeyboardKey.KEY_D,
    KeyboardKey SqueezeBrushHorizontally = KeyboardKey.KEY_A,

    MouseButton DragLevel = MouseButton.MOUSE_BUTTON_RIGHT
);

public record Shortcuts(
    GeoShortcuts GeoEditor,
    TileShortcuts TileEditor,
    CameraShortcust CameraEditor,
    LightShortcuts LightEditor
);

public record Misc(
    bool SplashScreen = true,
    int TileImageScansPerFrame = 20,
    int FPS = 60
);

public record LayerColor(byte R, byte G, byte B, byte A) {
    public void Deconstruct(out byte r, out byte g, out byte b, out byte a) {
        r = R;
        g = G;
        b = B;
        a = A;
    }

    public static implicit operator LayerColor(Color c) => new(c.r, c.g, c.b, c.a);
    public static implicit operator Color(LayerColor c) => new(c.R, c.G, c.B, c.A);
}

public record LayerColors(LayerColor Layer1, LayerColor Layer2, LayerColor Layer3);

public record GeoEditor(LayerColors LayerColors);

public record TileEditor(bool VisibleSpecs);

public record Settings(
    Shortcuts Shortcuts,
    Misc Misc,
    GeoEditor GeomentryEditor,
    TileEditor TileEditor
);
