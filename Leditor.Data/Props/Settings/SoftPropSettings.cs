namespace Leditor.Data.Props.Settings;

public class SoftPropSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0)
    : PropSettings(renderOrder, seed, renderTime)
{
    public int CustomDepth { get; set; } = customDepth;

    public override PropSettings Clone() => new SoftPropSettings(RenderOrder, Seed, RenderTime);
}