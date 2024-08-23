namespace Leditor.Renderer.RL;

using Raylib_cs;
using static Raylib_cs.Raylib;

/// <summary>
/// Contains all the shaders used by the renderer.
/// </summary>
public static class Shaders
{
    /// <summary>
    /// For drawing a single layer of a tile. Requires the face's dimensions, 
    /// the layer position, drawing 'ink', and the quad vertices.
    /// </summary>
    public static Shader SingleTileLayer { get; set; }
    
    /// <summary>
    /// For drawing the face of a box-type tile. Requires the face's dimensions.
    /// </summary>
    public static Shader BoxTileFace { get; set; }

    /// <summary>
    /// Makes a silhouette from a texture.
    /// </summary>
    public static Shader Silhouette { get; set; }
}