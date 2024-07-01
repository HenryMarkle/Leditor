using Leditor.Data.Props.Settings;
using Leditor.Data.Tiles;

namespace Leditor.Data.Props.Definitions;

public sealed class TileAsProp(string name, int depth, TileDefinition tileDefinition) : PropDefinition(name, depth)
{
    public TileAsProp(TileDefinition tile) : this(tile.Name, tile.Repeat.Length, tile) { }

    public TileDefinition TileDefinition { get; } = tileDefinition;

    public static implicit operator TileDefinition(TileAsProp prop) => prop.TileDefinition;
    
    public override PropSettings NewSettings(int renderOrder = 0, int seed = 0, int renderTime = 0)
    {
        return new TileAsPropSettings(renderOrder, seed, renderTime);
    }
}