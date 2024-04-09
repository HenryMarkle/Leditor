using Leditor.Data.Props.Settings;
using Raylib_cs;

namespace Leditor.Data.Props.Definitions;

public sealed class VariedDecal(string name, int depth, (int width, int height) sizeInPixels, int variations, bool random) 
    : PropDefinition(name, depth), IVaried, IPixelSized
{
    public override PropSettings NewSettings(int renderOrder = 0, int seed = 0, int renderTime = 0)
        => new VariedDecalSettings(renderOrder, seed, renderTime);
    
    //

    public (int Width, int Height) SizeInPixels { get; } = sizeInPixels;
    public int Variations { get; } = variations;
    public bool Random { get; } = random;
}
