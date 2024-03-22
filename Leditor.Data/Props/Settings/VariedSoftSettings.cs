namespace Leditor.Data.Props.Settings;

public class VariedSoftSettings(
    int renderOrder = 0, 
    int seed = 0, 
    int renderTime = 0, 
    int variation = 0, 
    int customDepth = 0, 
    int? applyColor = null
) : PropSettings(renderOrder, seed, renderTime)
{
    public int Variation { get; set; } = variation;
    public int CustomDepth { get; set; } = customDepth;
    public int? ApplyColor { get; set; } = applyColor;
}