namespace Leditor.RL.Managed;

public class Shader : IDisposable
{
    public bool Disposed { get; private set; }

    // ReSharper disable once MemberCanBePrivate.Global
    public Raylib_cs.Shader Raw;

    public Shader(string vertexPath, string fragmentPath)
    {
        Raw = Raylib.LoadShader(vertexPath, fragmentPath);
    }

    public Shader(Raylib_cs.Shader shader)
    {
        Raw = shader;
    }

    public static implicit operator Raylib_cs.Shader(Shader s) => s.Raw;

    private void Dispose(bool fromConsumer)
    {
        if (Disposed) return;

        if (fromConsumer)
        {
            // Was moved here to prevent GC from unloading on a separate thread        
            Raylib.UnloadShader(Raw);
        }
        

        Disposed = false;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Shader()
    {
        if (!Disposed) throw new InvalidOperationException("Shader was not disposed by the consumer");
        Dispose(false);
    }
}