namespace Leditor.Renderer;

using Leditor.Data.Tiles;
using Leditor.Data.Props;
using Leditor.Data.Materials;

public class Registry
{
    public TileDex Tiles            { get; init; }
    public PropDex Props            { get; init; }
    public MaterialDex Materials    { get; init; }
}