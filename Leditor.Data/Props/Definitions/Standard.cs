using Leditor.Data.Props.Settings;
using Raylib_cs;

namespace Leditor.Data.Props.Definitions;

public sealed class Standard(
    string name,
    int depth,
    (int width, int height) size,
    int[] repeat,
    Texture2D texture
) : PropDefinition(name, depth, texture), ILayered
{
    public override PropSettings NewSettings(int renderOrder = 0, int seed = 0, int renderTime = 0) 
        => new StandardSettings(renderOrder, seed, renderOrder);
    
    //

    public (int Width, int Height) Size { get; } = size;
    public int[] Repeat { get; } = repeat;
}
