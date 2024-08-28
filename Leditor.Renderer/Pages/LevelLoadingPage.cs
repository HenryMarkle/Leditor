namespace Leditor.Renderer.Pages;

using ImGuiNET;
using rlImGui_cs;

using Raylib_cs;
using static Raylib_cs.Raylib;

using System.Numerics;

public class LevelLoadingPage
{
    public string Title { get; set; } = "Loading; Please wait..";
    
    public void Draw()
    {
        ClearBackground(Color.Black);

        var width = MeasureText(Title, 30);

        DrawText(Title, (GetScreenWidth() - width) / 2, 100, 30, Color.White);
    }
}