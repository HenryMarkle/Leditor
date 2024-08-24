namespace Leditor.Renderer.RL;

using ImGuiNET;
using rlImGui_cs;

using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

/// <summary>
/// Consist of functions that draw to the enclosing draw mode (viewport).
/// Must be called after BeginDrawMode() and before EndDrawMode().
/// </summary>
public static class Draw
{
    /// <summary>
    /// Draws a simple progress bar.
    /// </summary>
    /// <param name="rect">the dimensions of progress bar</param>
    /// <param name="progress">a precentage from 0 to 1</param>
    /// <param name="color">the filling color</param>
    public static void ProgressBar(Rectangle rect, float progress, Color color)
    {
        DrawRectangleLinesEx(rect, 3, color);
        DrawRectangleRec(rect with { Width = rect.Width * progress }, color);
    }

    /// <summary>
    /// Draws the initial loading screen.
    /// </summary>
    /// <param name="tiles">tiles loading progress</param>
    /// <param name="props">props loading progress</param>
    /// <param name="materials">materials loading progress</param>
    public static void LoadingScreen(float tiles, float props, float materials)
    {
        ClearBackground(Color.Black);

        var tilesRect = new Rectangle((GetScreenWidth() - 200)/2, 200, 100, 30);
        var propsRect = new Rectangle((GetScreenWidth() - 200)/2, 250, 100, 30);
        var materRect = new Rectangle((GetScreenWidth() - 200)/2, 250, 100, 30);

        ProgressBar(tilesRect, tiles, Color.White);
        ProgressBar(propsRect, props, Color.White);
        ProgressBar(materRect, materials, Color.White);
    }

    public static void MainScreen()
    {
        ClearBackground(Color.Gray);

        rlImGui.Begin();
        {
            if (ImGui.Begin("Projects", 
                ImGuiWindowFlags.NoCollapse | 
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
