using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Leditor.Renderer;

public class Shaders : IDisposable
{
    public bool Disposed { get; protected set; }

    public Shader Invb { get; protected set; }
    public Shader Silhoutte { get; protected set; }
    public Shader WhiteRemover { get; protected set; }

    protected Shaders() {}

    public void Dispose()
    {
        if (Disposed) return;
    
        Disposed = true;

        UnloadShader(Invb); 
        UnloadShader(Silhoutte);   
        UnloadShader(WhiteRemover);   
    }

    public static Shaders LoadFrom(string shadersFolder)
    {
        return new()
        {
            Invb = LoadShader(Path.Combine(shadersFolder, "invb.vert"), Path.Combine(shadersFolder, "invb.frag")),
            Silhoutte = LoadShader(null, Path.Combine(shadersFolder, "silhoutte.frag")),
            WhiteRemover = LoadShader(null, Path.Combine(shadersFolder, "white_remover.frag"))
        };
    }
}