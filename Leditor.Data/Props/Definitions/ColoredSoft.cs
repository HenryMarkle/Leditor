using Leditor.Data.Props.Settings;
using Raylib_cs;

namespace Leditor.Data.Props.Definitions;

public sealed class ColoredSoft(string name, int depth, (int width, int height) sizeInPixels, bool colorize, Texture2D texture) 
    : PropDefinition(name, depth, texture), IColored, IPixelSized
{
    public override PropSettings NewSettings(int renderOrder = 0, int seed = 0, int renderTime = 0)
        => new ColoredSoftSettings(renderOrder, seed, renderTime);
    
    //

    public (int Width, int Height) SizeInPixels { get; } = sizeInPixels;
    public bool Colorize { get; } = colorize;
}
