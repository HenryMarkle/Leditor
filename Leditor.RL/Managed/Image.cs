namespace Leditor.RL.Managed;

public sealed class Image : IDisposable
{
    private bool _disposed;
    
    // ReSharper disable once MemberCanBePrivate.Global
    public Raylib_cs.Image Raw { get; set; }

    public Image() { }
    
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
        if (_disposed) return;
        
        if (fromConsumer) {}
        
        Raylib.UnloadImage(Raw);

        _disposed = true;
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