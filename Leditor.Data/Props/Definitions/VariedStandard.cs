using Leditor.Data.Props.Settings;
using Raylib_cs;

namespace Leditor.Data.Props.Definitions;

public sealed class VariedStandard(string name, int depth, (int Width, int Height) size, int[] repeat, int variations, bool random, Texture2D texture) 
    : PropDefinition(name, depth, texture), IVaried, ILayered
{
    public override PropSettings NewSettings(int renderOrder = 0, int seed = 0, int renderTime = 0)
        => new VariedStandardSettings(renderOrder, seed, renderTime);
    
    //

    public (int Width, int Height) Size { get; } = size;
    public int[] Repeat { get; } = repeat;
    public int Variations { get; } = variations;
    public bool Random { get; } = random;
}
