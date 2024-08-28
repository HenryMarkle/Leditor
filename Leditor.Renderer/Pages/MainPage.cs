namespace Leditor.Renderer.Pages;

using ImGuiNET;
using rlImGui_cs;

using Raylib_cs;
using static Raylib_cs.Raylib;

using System.Numerics;

using Leditor.Serialization;
using Leditor.Data.Geometry;

public class MainPage
{
    public Serilog.ILogger? Logger { get; init; }
    public required Context Context { get; init; }

    public void Draw()
    {
        ClearBackground(Color.DarkGray);

        rlImGui.Begin();
        {
            if (ImGui.Begin($"{Context.Level?.ProjectName ?? "NULL"}##LevelWindow", ImGuiWindowFlags.NoCollapse | 
                ImGuiWindowFlags.NoResize | 
                ImGuiWindowFlags.NoMove))
            {
                ImGui.SetWindowSize(new Vector2(GetScreenWidth() - 80, GetScreenHeight() - 80));
                ImGui.SetWindowPos(new Vector2(40, 40));

                ImGui.End();
            }
        }
        rlImGui.End();
    }
}