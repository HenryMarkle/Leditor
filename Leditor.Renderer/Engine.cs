using ImGuiNET;
using rlImGui_cs;

using Raylib_cs;
using static Raylib_cs.Raylib;

using System.Numerics;

using Leditor.Data;
using Leditor.Data.Geometry;

namespace Leditor.Renderer;

/// <summary>
/// The main class responsible for rendering the whole level.
/// </summary>
public class Engine
{
    private readonly RenderTexture2D[] _layers;
    private readonly RenderTexture2D _canvas;
    private Texture2D _lightmap;

    public RenderTexture2D Canvas => _canvas;

    /// <summary>
    /// The level's state is not managed by <see cref="Engine"/>
    /// </summary>
    public LevelState? Level { get; protected set; }

    public Serilog.ILogger? Logger { get; init; }

    public bool Disposed { get; protected set; }



    public Engine()
    {
        _layers = new RenderTexture2D[30];

        foreach (var layer in _layers)
        {
            BeginTextureMode(layer);
            ClearBackground(Raylib_cs.Color.White);
            EndTextureMode();
        }

        _canvas = LoadRenderTexture(2000, 1200);
        
        BeginTextureMode(_canvas);
        ClearBackground(Raylib_cs.Color.White);
        EndTextureMode();
    }

    ~Engine()
    {
        if (!Disposed) throw new InvalidOperationException("Engine was not disposed by consumer");
    }

    private void Clear()
    {
        Logger?.Information("[Engine::Clear] Clearing engine's data");

        foreach (var layer in _layers)
        {
            BeginTextureMode(layer);
            ClearBackground(Raylib_cs.Color.White);
            EndTextureMode();
        }

        BeginTextureMode(_canvas);
        ClearBackground(Raylib_cs.Color.White);
        EndTextureMode();
        
        if (_lightmap.Id != 0)
        {
            UnloadTexture(_lightmap);
            _lightmap.Id = 0;
        }

        Level = null;
    }

    public void Dispose()
    {
        Logger?.Information("[Engine::Dispose] Disposing engine");

        if (Disposed) return;
        Disposed = true;

        UnloadRenderTexture(_canvas);
        foreach (var layer in _layers) UnloadRenderTexture(layer);
        if (_lightmap.Id != 0) UnloadTexture(_lightmap);
        Level = null;
    }

    public void Load(in LevelState level)
    {
        Logger?.Information("[Engine::Load] Loading a new level \"{name}\"", level.ProjectName);

        Clear();

        Level = level;
        _lightmap = LoadTextureFromImage(Level.LightMap);
    }

    public void Render()
    {

    }
}