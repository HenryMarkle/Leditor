namespace Leditor.RL.Managed;

public class RenderTexture2D : IDisposable
{
    public bool Disposed { get; private set; }

    // ReSharper disable once MemberCanBePrivate.Global
    public Raylib_cs.RenderTexture2D Raw { get; set; }

    public RenderTexture2D(int width, int height)
    {
        Raw = Raylib.LoadRenderTexture(width, height);
    }

    public RenderTexture2D(Raylib_cs.RenderTexture2D texture)
    {
        Raw = texture;
    }

    public static implicit operator Raylib_cs.RenderTexture2D(RenderTexture2D r) => r.Raw;

    private void Dispose(bool fromConsumer)
    {
        if (Disposed) return;

        if (fromConsumer)
        {
            // Was moved here to stop GC from unloading on a separate thread
            Raylib.UnloadRenderTexture(Raw);
        }

        Disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~RenderTexture2D()
    {
        if (!Disposed) throw new InvalidOperationException("RenderTexture2D was not disposed by the consumer");
        Dispose(false);
    }
}