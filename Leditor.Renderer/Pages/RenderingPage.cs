using ImGuiNET;
using rlImGui_cs;

using Raylib_cs;
using static Raylib_cs.Raylib;

using System.Numerics;

using Leditor.Data.Geometry;

namespace Leditor.Renderer.Pages;

public class RenderingPage
{
    public Serilog.ILogger? Logger { get; init; }
    public Context Context { get; init; }

    private bool _disposed;

    private void OnLevelLoaded(object? sender, EventArgs e)
    {
    }

    public RenderingPage(Context context)
    {
        Context = context;

        Context.LevelLoaded += OnLevelLoaded;
    }

    ~RenderingPage()
    {
        Context.LevelLoaded -= OnLevelLoaded;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
    }

    public void Draw()
    {
        ClearBackground(Color.DarkGray);

        if (Context.Level is null || Context.Engine is null) return;

        rlImGui.Begin();
        {
            if (ImGui.Begin("Render##LevelRenderingWindow", ImGuiWindowFlags.NoCollapse | 
                ImGuiWindowFlags.NoResize | 
                ImGuiWindowFlags.NoMove))
            {
                ImGui.SetWindowSize(new Vector2(GetScreenWidth() - 80, GetScreenHeight() - 80));
                ImGui.SetWindowPos(new Vector2(40, 40));

                ImGui.Columns(2);

                ImGui.SetColumnWidth(0, ImGui.GetContentRegionAvail().Y / 3);

                ImGui.NextColumn();

                rlImGui.ImageRenderTextureFit(Context.Engine.Canvas, false);

                ImGui.End();
            }
        }
        rlImGui.End();
    }
}