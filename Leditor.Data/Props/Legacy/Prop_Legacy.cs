namespace Leditor.Data.Props.Legacy;

using Leditor.Data.Tiles;
using Leditor.Data.Props.Definitions;

using System.Numerics;
using Leditor.Data.Generic;
using Raylib_cs;

public interface IVariableInit { int Variations { get; } }
public interface IVariable { int Variation { get; set; } }

public class Prop_Legacy
{
    public InitPropType_Legacy Type { get; set; }
    public InitPropBase? Init { get; set; }
    public TileDefinition? Tile { get; set; }
    public (int category, int index) Position { get; set; }
    public PropDefinition? Definition { get; set; }

    public int Depth { get; set; }
    public string Name { get; set; }
    public float Rotation { get; set; }
    public Quad OriginalQuad { get; init; }
    public Quad Quad { get; set; }

    public Prop_Legacy(int depth, string name, Quad quad)
    {
        Name = name;
        Depth = depth;
        Quad = quad;
    }

    public PropExtras_Legacy Extras { get; set; } = new(new BasicPropSettings(), []);

    public Prop_Legacy Clone() => new(Depth, Name, Quad)
    {
        Type = Type,
        Init = Init,
        Tile = Tile,
        Position = Position,
        Definition = Definition,
        Rotation = Rotation,
        OriginalQuad = OriginalQuad,
        Extras = Extras.Clone()
    };
}

public class PropExtras_Legacy(BasicPropSettings settings, Vector2[] ropePoints)
{
    public BasicPropSettings Settings { get; set; } = settings;
    public Vector2[] RopePoints { get; set; } = ropePoints;

    public PropExtras_Legacy Clone() => new(Settings.Clone(), [..RopePoints]);
}


// Settings


public interface ICustomDepth {
    int CustomDepth { get; set; }
}

public interface IApplyColor {
    int? ApplyColor { get; set; }
}

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

public class PropRopeSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, PropRopeRelease release = PropRopeRelease.None, float? thickness = null, int? applyColor = null) : BasicPropSettings(renderOrder, seed, renderTime), IApplyColor
{
    public PropRopeRelease Release { get; set; } = release;
    public float? Thickness { get; set; } = thickness;
    public int? ApplyColor { get; set; } = applyColor;

    public override PropRopeSettings Clone()
    {
        return new PropRopeSettings(RenderOrder, Seed, RenderTime, Release, Thickness, ApplyColor);
    }
}

public class PropVariedDecalSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int variation = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime), IVariable, ICustomDepth
{
    public int Variation { get; set; } = variation;
    public int CustomDepth { get; set; } = customDepth;

    public override PropVariedDecalSettings Clone()
    {
        return new PropVariedDecalSettings(RenderOrder, Seed, RenderTime, Variation, CustomDepth);
    }
}
public class PropVariedSoftSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int variation = 0, int customDepth = 0, int? applyColor = null) : BasicPropSettings(renderOrder, seed, renderTime), IVariable, ICustomDepth, IApplyColor
{
    public int Variation { get; set; } = variation;
    public int CustomDepth { get; set; } = customDepth;
    public int? ApplyColor { get; set; } = applyColor;

    public override PropVariedSoftSettings Clone()
    {
        return new PropVariedSoftSettings(RenderOrder, Seed, RenderTime, Variation, CustomDepth, ApplyColor);
    }
}

public class PropSimpleDecalSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime), ICustomDepth
{
    public int CustomDepth { get; set; } = customDepth;

    public override PropSimpleDecalSettings Clone()
    {
        return new PropSimpleDecalSettings(RenderOrder, Seed, RenderTime, CustomDepth);
    }
}

public class PropSoftSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime), ICustomDepth
{
    public int CustomDepth { get; set; } = customDepth;
    
    public override PropSoftSettings Clone()
    {
        return new PropSoftSettings(RenderOrder, Seed, RenderTime, CustomDepth);
    }
}
public class PropSoftEffectSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime), ICustomDepth
{
    public int CustomDepth { get; set; } = customDepth;
    
    public override PropSoftEffectSettings Clone()
    {
        return new PropSoftEffectSettings(RenderOrder, Seed, RenderTime, CustomDepth);
    }
}

public class PropAntimatterSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0) : BasicPropSettings(renderOrder, seed, renderTime), ICustomDepth
{
    public int CustomDepth { get; set; } = customDepth;
    
    public override PropAntimatterSettings Clone()
    {
        return new PropAntimatterSettings(RenderOrder, Seed, RenderTime, CustomDepth);
    }
}

// Init

public enum InitPropType_Legacy
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
    CustomRope,
    Long,
    CustomLong,
    Tile
}

public enum InitPropColorTreatment { Standard, Bevel }


public abstract class InitPropBase(string name, InitPropType_Legacy type, int depth) : IIdentifiable<string>, ITexture
{
    public string Name { get; init; } = name;
    public InitPropType_Legacy Type { get; init; } = type;
    public int Depth { get; init; } = depth;

    public Texture2D Texture { get; set; }

    public override string ToString() => 
        $"({Type}) - {Name}\n" +
        $"Depth: {Depth}";

    public abstract BasicPropSettings NewSettings();
}

public class InitStandardProp(
    string name, 
    InitPropType_Legacy type,
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

    public override BasicPropSettings NewSettings()
        => new();
}

public class InitVariedStandardProp(
    string name,
    InitPropType_Legacy type,
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
    
    public override BasicPropSettings NewSettings()
        => new PropVariedSettings();
}

public class InitSoftProp(
    string name,
    InitPropType_Legacy type,
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
    
    public override BasicPropSettings NewSettings()
        => new PropSoftSettings();
}

public class InitSoftEffectProp(
    string name,
    InitPropType_Legacy type,
    int depth) : InitPropBase(name, type, depth)
{
    public override BasicPropSettings NewSettings()
        => new PropSoftEffectSettings();
}

public class InitVariedSoftProp(
    string name,
    InitPropType_Legacy type,
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
    
    public override BasicPropSettings NewSettings()
        => new PropVariedSoftSettings();
}

public class InitSimpleDecalProp(string name, InitPropType_Legacy type, int depth) : InitPropBase(name, type, depth)
{
    public override BasicPropSettings NewSettings()
        => new PropSimpleDecalSettings();
}

public class InitVariedDecalProp(string name, InitPropType_Legacy type, int depth, (int x, int y) sizeInPixels, int variations, int random) : InitSimpleDecalProp(name, type, depth), IVariableInit
{
    public (int x, int y) SizeInPixels { get; init; } = sizeInPixels;
    public int Variations { get; init; } = variations;
    public int Random { get; init; } = random;

    public override string ToString() => base.ToString() +
        $"\nSize in Pixels: ({SizeInPixels.x}, {SizeInPixels.y})\n" +
        $"Variations: {Variations}\n" +
        $"Random: {Random}";
    
    public override BasicPropSettings NewSettings()
        => new PropVariedDecalSettings();
}

public class InitAntimatterProp(string name, InitPropType_Legacy type, int depth, float contourExp) : InitPropBase(name, type, depth)
{
    public float ContourExp { get; init; } = contourExp;

    public override string ToString() => base.ToString() +
        $"\nContourExp: {ContourExp}";
    
    public override BasicPropSettings NewSettings()
        => new PropAntimatterSettings();
}

public class InitRopeProp(
    string name,
    InitPropType_Legacy type,
    int depth,
    int segmentLength,
    int collisionDepth,
    float segmentRadius,
    float gravity,
    float friction,
    float airFriction,
    bool stiff,
    Raylib_cs.Color previewColor,
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
    public Raylib_cs.Color PreviewColor { get; } = previewColor;
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
    
    public override BasicPropSettings NewSettings()
        => new PropRopeSettings();
}

public class InitCustomRopeProp(
    string name,
    InitPropType_Legacy type,
    int depth,
    int segmentLength,
    int collisionDepth,
    float segmentRadius,
    float gravity,
    float friction,
    float airFriction,
    bool stiff,
    Raylib_cs.Color previewColor,
    int previewEvery,
    float edgeDirection,
    float rigid,
    float selfPush,
    float sourcePush,
    InitPropColorTreatment colorTreatment,
    int bevel
) : InitRopeProp(name,
type,
depth,
segmentLength,
collisionDepth,
segmentRadius,
gravity,
friction,
airFriction,
stiff,
previewColor,
previewEvery,
edgeDirection,
rigid,
selfPush,
sourcePush) {
    public InitPropColorTreatment ColorTreatment { get; } = colorTreatment;
    public int Bevel { get; } = bevel;
}

public class InitLongProp(string name, InitPropType_Legacy type, int depth) : InitPropBase(name, type, depth)
{
    public override BasicPropSettings NewSettings()
        => new PropLongSettings();
}

public class InitCustomLongProp(
    string name, 
    InitPropType_Legacy type, 
    int depth,
    InitPropColorTreatment colorTreatment,
    int bevel,
    int random,
    int[] repeat,
    (int, int) pixelSize,
    int segmentLength,
    int vars
) : InitLongProp(name, type, depth) {
    public InitPropColorTreatment ColorTreatment { get; } = colorTreatment;
    public int Bevel { get; } = bevel;
    public int Random { get; } = random;
    public int[] Repeat { get; } = repeat;
    public (int Width, int Height) PixelSize { get; } = pixelSize;
    public int SegmentLength { get; } = segmentLength;
    public int Vars { get; } = vars;
}

public class InitColoredSoftProp(
    string name, 
    InitPropType_Legacy type, 
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
    
    public override BasicPropSettings NewSettings()
        => new();
}