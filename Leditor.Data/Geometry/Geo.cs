using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Leditor.Data.Geometry;

#nullable disable

public enum GeoType
{
    Air                 = 0,
    Solid               = 1,
    SlopeNE             = 2,
    SlopeNW             = 3,
    SlopeES             = 4,
    SlopeSW             = 5,
    Platform            = 6,
    ShortcutEntrance    = 7,
    Glass               = 9
}

[Flags]
public enum GeoFeature 
{
    None                = 0,
    HorizontalPole      = 2,
    VerticalPole        = 4,
    Bathive             = 8,
    ShortcutEntrance    = 16,
    ShortcutPath        = 32,
    RoomEntrance        = 64,
    DragonDen           = 128,
    PlaceRock           = 256,
    PlaceSpear          = 512,
    CrackedTerrain      = 1024,
    ForbidFlyChains     = 2048,
    GarbageWormHole     = 4096,
    Waterfall           = 8192,
    WackAMoleHole       = 16384,
    WormGrass           = 32768,
    ScavengerHole       = 65536,
}

public struct Geo
{
    public static GeoType TypeID(int id) {
        if (id is < 0 or > 9 or 8) return GeoType.Air;

        return (GeoType) id;
    }
    
    /// <summary>
    /// Maps a feature ID to a <see cref="GeoFeature"/>
    /// </summary>
    public static GeoFeature FeatureID(int id) => id switch {
         1 => GeoFeature.HorizontalPole,
         2 => GeoFeature.VerticalPole,
         3 => GeoFeature.Bathive,
         4 => GeoFeature.ShortcutEntrance,
         5 => GeoFeature.ShortcutPath,
         6 => GeoFeature.RoomEntrance,
         7 => GeoFeature.DragonDen,
         9 => GeoFeature.PlaceRock,
        10 => GeoFeature.PlaceSpear,
        11 => GeoFeature.CrackedTerrain,
        12 => GeoFeature.ForbidFlyChains,
        13 => GeoFeature.GarbageWormHole,
        18 => GeoFeature.Waterfall,
        19 => GeoFeature.WackAMoleHole,
        20 => GeoFeature.WormGrass,
        21 => GeoFeature.ScavengerHole,
        
        _ => GeoFeature.None
    };

    public GeoType Type { get; set; } = GeoType.Air;
    public GeoFeature Features { get; set; } = GeoFeature.None;

    public void Switch(GeoFeature feature)
    {
        if ((Features & feature) == feature) Features &= ~feature;
        else Features |= feature;
    }

    public void ToggleWhen(GeoFeature feature, bool value) {
        if (value) Features |= feature;
        else Features &= ~feature;
    }

    public void Enable(GeoFeature feature) {
        Features |= feature;
    }

    public void Disable(GeoFeature feature) {
        Features &= ~feature;
    }

    /// <summary>
    /// Checks geo type equality.
    /// </summary>
    /// <param name="type">The type of the geo.</param>
    /// <returns>true if the geo is equal to the parameter; otherwise false.</returns>
    public readonly bool this[GeoType type] => Type == type;

    /// <summary>
    /// Check if a feature is active.
    /// </summary>
    /// <param name="id">The ID of the feature.</param>
    /// <returns>true if the feature is enabled; otherwise false.</returns>
    public readonly bool this[int id] => (Features & FeatureID(id)) != GeoFeature.None;

    /// <summary>
    /// Check if a feature is active.
    /// </summary>
    /// <returns>true if the feature is enabled; otherwise false.</returns>
    public bool this[GeoFeature feature]
    {
        readonly get => (Features & feature) != GeoFeature.None;
        set {
            if (value) Features |= feature;
            else Features &= ~feature;
        }
    }

    public readonly bool IsSlope => Type is GeoType.SlopeES or GeoType.SlopeNE or GeoType.SlopeNW or GeoType.SlopeSW;

    public readonly override string ToString() {
        List<int> activeFeatures = new(21);

        for (var i = 0; i < 22; i ++) {
            var feature = FeatureID(i);
            
            if (feature != GeoFeature.None) activeFeatures.Add((int)feature);
        }

        return $"[ {(int)Type}, [ {string.Join(", ", activeFeatures.Select(f => $"{f}"))} ] ]";
    }

    public readonly override int GetHashCode() => HashCode.Combine(Type, Features);

    public readonly override bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is not Geo g) return false;

        return Type == g.Type && Features == g.Features;
    }

    public static bool operator ==(Geo g1, Geo g2) => g1.Type == g2.Type && g1.Features == g2.Features;
    public static bool operator !=(Geo g1, Geo g2) => g1.Type != g2.Type || g1.Features != g2.Features;

    public static bool operator ==(Geo g1, GeoType t) => g1.Type == t; 
    public static bool operator !=(Geo g1, GeoType t) => g1.Type != t;

    public Geo()
    {
        Type = GeoType.Air;
        Features = GeoFeature.None;
    }

    public Geo(GeoType type)
    {
        Type = type;
        Features = GeoFeature.None;
    }

    public Geo(GeoType type, GeoFeature features)
    {
        Type = type;
        Features = features;
    }
}