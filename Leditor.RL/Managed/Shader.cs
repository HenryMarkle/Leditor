namespace Leditor.RL.Managed;

public class Shader : IDisposable
{
    private bool _disposed;

    // ReSharper disable once MemberCanBePrivate.Global
    public Raylib_cs.Shader Raw;

    public Shader()
    {
        
    }

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
        if (_disposed) return;
        
        if (fromConsumer) {}
        
        Raylib.UnloadShader(Raw);

        _disposed = false;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Shader()
    {
        Dispose(false);
    }
}