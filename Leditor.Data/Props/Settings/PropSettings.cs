namespace Leditor.Data.Props.Settings;

public abstract class PropSettings(int renderOrder = 0, int seed = 0, int renderTime = 0)
{
    public int RenderOrder { get; set; } = renderOrder;
    public int Seed { get; set; } = seed;
    public int RenderTime { get; set; } = renderTime;

    public void Deconstruct(out int renderOrder, out int seed, out int renderTime)
    {
        renderOrder = RenderOrder;
        seed = Seed;
        renderTime = RenderTime;
    }
}