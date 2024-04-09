using Leditor.Data.Props.Settings;
using Raylib_cs;

namespace Leditor.Data.Props.Definitions;

public sealed class VariedSoft(string name, int depth, int variations, bool random, bool colorize, (int Width, int Height) sizeInPixels) 
    : PropDefinition(name, depth), IVaried, IColored, IPixelSized
{
    public override PropSettings NewSettings(int renderOrder = 0, int seed = 0, int renderTime = 0)
        => new VariedSoftSettings(renderOrder, seed, renderTime);
    
    //

    public int Variations { get; } = variations;
    public bool Random { get; } = random;
    public bool Colorize { get; } = colorize;
    public (int Width, int Height) SizeInPixels { get; } = sizeInPixels;
}
