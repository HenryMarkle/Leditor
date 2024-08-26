namespace Leditor.Data.Effects;

public class Effect
{
    required public string Name              { get; set; }
    required public EffectOptions[] Options  { get; set; }
    required public double[,] Matrix         { get; set; }
}
