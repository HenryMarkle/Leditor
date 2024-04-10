namespace Leditor.Data.Props.Settings;

public abstract class PropSettings(int renderOrder = 0, int seed = 0, int renderTime = 0) : IClone<PropSettings>
{
    public int RenderOrder { get; set; } = renderOrder;
    public int Seed { get; set; } = seed;
    public int RenderTime { get; set; } = renderTime;

    public abstract PropSettings Clone();

    public void Deconstruct(out int renderOrder, out int seed, out int renderTime)
    {
        renderOrder = RenderOrder;
        seed = Seed;
        renderTime = RenderTime;
    }
}