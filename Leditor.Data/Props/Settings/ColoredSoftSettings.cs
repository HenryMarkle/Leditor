namespace Leditor.Data.Props.Settings;

public class ColoredSoftSettings(int renderOrder = 0, int seed = 0, int renderTime = 0) 
    : PropSettings(renderOrder, seed, renderTime) 
{
    public override PropSettings Clone() => new ColoredSoftSettings(RenderOrder, Seed, RenderTime);
}