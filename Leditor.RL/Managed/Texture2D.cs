namespace Leditor.RL.Managed;

public sealed class Texture2D : IDisposable
{
    private bool _disposed;

    // ReSharper disable once MemberCanBePrivate.Global
    public Raylib_cs.Texture2D Raw;

    public Texture2D()
    {
        
    }

    public Texture2D(string path)
    {
        Raw = Raylib.LoadTexture(path);
    }

    public Texture2D(Raylib_cs.Texture2D texture)
    {
        Raw = texture;
    }

    public static implicit operator Raylib_cs.Texture2D(Texture2D t) => t.Raw;

    private void Dispose(bool fromConsumer)
    {
        if (_disposed) return;
        
        if (fromConsumer) {}
        
        Raylib.UnloadTexture(Raw);

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Texture2D()
    {
        Dispose(false);
    }
}