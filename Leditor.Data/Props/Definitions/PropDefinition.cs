using Leditor.Data.Generic;
using Leditor.Data.Props.Settings;
using Raylib_cs;

namespace Leditor.Data.Props.Definitions;

public abstract class PropDefinition(string name, int depth, Texture2D texture) 
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
    public Texture2D Texture { get; internal set; } = texture;

    //

    public static bool operator ==(PropDefinition p1, PropDefinition p2) => string.Equals(p1.Name, p2.Name, StringComparison.InvariantCultureIgnoreCase);
    public static bool operator !=(PropDefinition p1, PropDefinition p2) => !string.Equals(p1.Name, p2.Name, StringComparison.InvariantCultureIgnoreCase);

    public override bool Equals(object? obj)
        => obj is PropDefinition p && string.Equals(Name, p.Name, StringComparison.InvariantCultureIgnoreCase);

    public override int GetHashCode() => Name.GetHashCode();
}
