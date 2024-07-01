namespace Leditor.Data.Props.Settings;

public class VariedDecalSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int variation = 0, int customDepth = 0) 
    : PropSettings(renderOrder, seed, renderTime), IVariant, ICustomDepth
{
    public int Variation { get; set; } = variation;
    public int CustomDepth { get; set; } = customDepth;

    public override PropSettings Clone() => new VariedDecalSettings(RenderOrder, Seed, RenderTime);
}