namespace Leditor.RL.Managed;

public sealed class Image : IDisposable
{
    private Raylib_cs.Image _raw;
    
    public bool Disposed { get; private set; }

    // ReSharper disable once MemberCanBePrivate.Global
    public Raylib_cs.Image Raw
    {
        get => _raw; 
        private set => _raw = value;
    }

    public Image(string path)
    {
        Raw = Raylib.LoadImage(path);
    }

    public Image(Raylib_cs.Image image)
    {
        Raw = image;
    }

    public Image(int width, int height, Color color)
    {
        Raw = Raylib.GenImageColor(width, height, color);
    }

    public void Format(PixelFormat format)
    {
        Raylib.ImageFormat(ref _raw, format);
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