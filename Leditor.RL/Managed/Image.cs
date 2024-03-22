namespace Leditor.RL.Managed;

public sealed class Image : IDisposable
{
    public bool Disposed { get; private set; }

    // ReSharper disable once MemberCanBePrivate.Global
    public Raylib_cs.Image Raw { get; }

    public Image(string path)
    {
        Raw = Raylib.LoadImage(path);
    }

    public Image(Raylib_cs.Image image)
    {
        Raw = image;
    }

    public static implicit operator Raylib_cs.Image(Image i) => i.Raw;

    private void Dispose(bool fromConsumer)
    {
        if (Disposed) return;

        if (fromConsumer) { }
        
        Raylib.UnloadImage(Raw);
        
        Disposed = true;
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Image()
    {
        Dispose(false);
    }
}