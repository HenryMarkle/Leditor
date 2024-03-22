namespace Leditor.Data.Props.Settings;

public class AntimatterSettings(int renderOrder = 0, int seed = 0, int renderTime = 0, int customDepth = 0)
    : PropSettings(renderOrder, seed, renderTime)
{
    public int CustomDepth { get; set; } = customDepth;
}