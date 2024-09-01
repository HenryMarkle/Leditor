using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Leditor.Renderer;

/// <summary>
/// Encapsulates the shaders used by the <see cref="Engine"/>
/// </summary>
public class Shaders : IDisposable
{
    public bool Disposed { get; protected set; }

    /// <summary>
    /// Draws a texture using inverse-bilinear interpolation algorithm
    /// </summary>
    public Shader Invb { get; protected set; }

    /// <summary>
    /// Draws a silhoutte of a texture against a white background
    /// </summary>
    public Shader Silhoutte { get; protected set; }

    /// <summary>
    /// Draws a texture without the white background, basically ignoring any white pixels.
    /// </summary>
    public Shader WhiteRemover { get; protected set; }

    /// <summary>
    /// Same as <see cref="WhiteRemover"/> except it vertically flips the texture.
    /// </summary>
    public Shader WhiteRemoverVFlip { get; protected set; }

    /// <summary>
    /// Ignores white pixels and draws a given color otherwise.
    /// </summary>
    public Shader WhiteRemoverApplyColor { get; protected set; }

    protected Shaders() {}

    public void Dispose()
    {
        if (Disposed) return;
    
        Disposed = true;

        UnloadShader(Invb); 
        UnloadShader(Silhoutte);   
        UnloadShader(WhiteRemover);
        UnloadShader(WhiteRemoverVFlip);
        UnloadShader(WhiteRemoverApplyColor);
    }

    public static Shaders LoadFrom(string shadersFolder)
    {
        return new()
        {
            Invb = LoadShader(Path.Combine(shadersFolder, "invb.vert"), Path.Combine(shadersFolder, "invb.frag")),
            Silhoutte = LoadShader(null, Path.Combine(shadersFolder, "silhoutte.frag")),
            WhiteRemover = LoadShader(null, Path.Combine(shadersFolder, "white_remover.frag")),
            WhiteRemoverVFlip = LoadShader(null, Path.Combine(shadersFolder, "white_remover_vflip.frag")),
            WhiteRemoverApplyColor = LoadShader(null, Path.Combine(shadersFolder, "white_remover_colored.frag"))
        };
    }
}