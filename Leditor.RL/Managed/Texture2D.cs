namespace Leditor.RL.Managed;

public sealed class Texture2D : IDisposable
{
    public bool Disposed { get; private set; }

    // ReSharper disable once MemberCanBePrivate.Global
    public Raylib_cs.Texture2D Raw;

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
        if (Disposed) return;

        if (fromConsumer)
        {
            // Was moved here to prevent GC from unloading on a separate thread
            Raylib.UnloadTexture(Raw);
        }
        

        Disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Texture2D()
    {
        if (!Disposed) throw new InvalidOperationException("Texture2D was not disposed by the consumer");
        Dispose(false);
    }
}