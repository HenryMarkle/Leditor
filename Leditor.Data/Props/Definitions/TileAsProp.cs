using Leditor.Data.Props.Settings;
using Raylib_cs;

namespace Leditor.Data.Props.Definitions;

public sealed class TileAsProp(string name, int depth) : PropDefinition(name, depth)
{
    public override PropSettings NewSettings(int renderOrder = 0, int seed = 0, int renderTime = 0)
    {
        return new TileAsPropSettings(renderOrder, seed, renderTime);
    }
}