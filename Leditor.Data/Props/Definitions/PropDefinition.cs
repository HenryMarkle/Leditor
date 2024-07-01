using Leditor.Data.Generic;
using Leditor.Data.Props.Settings;
using Raylib_cs;

namespace Leditor.Data.Props.Definitions;

public abstract class PropDefinition(string name, int depth) 
    : IIdentifiable<string>
{
    public abstract PropSettings NewSettings(
        int renderOrder = 0, 
        int seed = 0, 
        int renderTime = 0);
    
    //

    public string Name { get; } = name;
    public int Depth { get; } = depth;

    /// <summary>
    /// A weak reference to the associated texture.
    /// <para>
    /// This class is not responsible for managing the texture and should be disposed separately.
    /// </para>
    /// </summary>
    public Texture2D Texture { get; internal set; }

    private readonly int _hashCode = name.GetHashCode();

    //

    public static bool operator ==(PropDefinition? p1, PropDefinition? p2) 
    {
        if (p1 is null && p2 is null) return true;
        if (p1 is null || p2 is null) return false;

        return p1.GetHashCode() == p2.GetHashCode();
    }
    public static bool operator !=(PropDefinition? p1, PropDefinition? p2) 
    {
        if (p1 is null && p2 is null) return false;
        if (p1 is null || p2 is null) return true;

        return p1.GetHashCode() != p2.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;

        if (obj is not PropDefinition p) return false;

        return _hashCode == p.GetHashCode();
    }

    public override int GetHashCode() => _hashCode;
}
