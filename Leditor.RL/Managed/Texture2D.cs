using Leditor.RL.Exceptions;

namespace Leditor.RL.Managed;

public sealed class Texture2D : IDisposable
{
    public bool Disposed { get; private set; }

    // ReSharper disable once MemberCanBePrivate.Global
    public Raylib_cs.Texture2D Raw;

    /// <summary>
    /// Loads a managed texture from a file path
    /// </summary>
    /// <param name="path">The file path</param>
    /// <exception cref="TextureLoadException">Failure to load the texture</exception>
    public Texture2D(string path)
    {
        Raw = Raylib.LoadTexture(path);

        if (!Raylib.IsTextureReady(Raw)) throw new TextureLoadException(path);
    }

    public Texture2D(Raylib_cs.Texture2D texture)
    {
        Raw = texture;
    }

    public Texture2D(Raylib_cs.Image image)
    {
        Raw = Raylib.LoadTextureFromImage(image);
    }
    
    public Texture2D(Image image)
    {
        Raw = Raylib.LoadTextureFromImage(image);
    }

    public unsafe void Update(Image image)
    {
        Raylib.UpdateTexture(Raw, image.Raw.Data);
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