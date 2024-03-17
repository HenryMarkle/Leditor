namespace Leditor.RL.Managed;

public class RenderTexture2D : IDisposable
{
    private bool _disposed;

    // ReSharper disable once MemberCanBePrivate.Global
    public Raylib_cs.RenderTexture2D Raw { get; set; }

    public RenderTexture2D()
    {
        
    }

    public RenderTexture2D(int width, int height)
    {
        Raw = Raylib.LoadRenderTexture(width, height);
    }

    public static implicit operator Raylib_cs.RenderTexture2D(RenderTexture2D r) => r.Raw;

    private void Dispose(bool fromConsumer)
    {
        if (_disposed) return;
        
        if (fromConsumer) {}
        
        Raylib.UnloadRenderTexture(Raw);

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~RenderTexture2D()
    {
        Dispose(false);
    }
}