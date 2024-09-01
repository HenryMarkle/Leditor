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
        ren = false;
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

    bool ren = false;

    public void Draw()
    {
        ClearBackground(Color.DarkGray);

        if (Context.Level is null || Context.Engine is null) return;

        if (!ren)
        {
            Context.Engine.Initialize();
            Context.Engine.Load(Context.Level);
            Context.Engine.Render(2);
            Context.Engine.Render(1);
            Context.Engine.Render(0);
            Context.Engine.Compose();
            ren = true;
        }

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

                if (ImGui.Button("Cancel", ImGui.GetContentRegionAvail() with { Y = 20 }))
                {
                    Context.Page = 0;
                }

                ImGui.NextColumn();

                // #if DEBUG
                // if (ImGui.BeginListBox("##Layers", ImGui.GetContentRegionAvail()))
                // {
                //     for (var l = 0; l < 30; l++)
                //     {
                //         rlImGui.ImageButton($"{29 - l}", Context.Engine.Layers[29 - l].Texture);
                //     }

                //     ImGui.EndListBox();
                // }
                // #else
                // #endif
                rlImGui.ImageRenderTextureFit(Context.Engine.Canvas, false);

                ImGui.End();
            }

            if (ImGui.Begin("Layers##LevelRenderingLayers"))
            {
                if (ImGui.BeginListBox("##Layers", ImGui.GetContentRegionAvail()))
                {
                    for (var l = 29; l >= 0; l--)
                    {
                        rlImGui.ImageRenderTexture(Context.Engine.Layers[l]);
                    }

                    ImGui.EndListBox();
                }
                ImGui.End();
            }
        }
        rlImGui.End();
    }
}