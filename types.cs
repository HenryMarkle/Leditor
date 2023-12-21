namespace Leditor;

using System.Numerics;

#nullable enable

using Leditor.Common;

public class LoadFileResult {
    public bool Success { get; init; } = false;

    public int Width { get; init; } = 0;
    public int Height { get; init; } = 0;

    public BufferTiles BufferTiles { get; init; } = new();
    public (string, double[,])[] Effects { get; init; } = [];

    public RunCell[,,]? GeoMatrix { get; init; } = null;

    public Image LightMapImage { get; init; }

    public List<RenderCamera> Cameras { get; set; }

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
