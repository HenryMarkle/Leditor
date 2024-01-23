using System.Data;
using static Raylib_CsLo.Raylib;

namespace Leditor;

#nullable enable

internal class DeathScreen(Serilog.Core.Logger logger, Texture? screenshot, Exception? exception) : IPage
{
    readonly Serilog.Core.Logger logger = logger;

    readonly Texture? screenshot = screenshot;
    readonly Exception? exception = exception;

    ~DeathScreen()
    {
        if (screenshot is not null) UnloadTexture((Texture)screenshot);
    }

    public void Draw()
    {
        var sWidth = GetScreenWidth();

        if (GLOBALS.Settings.Misc.FunnyDeathScreen)
        {
            BeginDrawing();
            ClearBackground(BLACK);

            if (screenshot is not null) DrawTexturePro(
                (Texture)screenshot,
                new(0, 0, screenshot?.width ?? 0, screenshot?.height ?? 0),
                new(screenshot?.width * 0.2f ?? 0, 50, screenshot?.width * 0.6f ?? 0, screenshot?.height * 0.6f ?? 0),
                new(0, 0),
                0,
                WHITE
            );

            DrawRectangleLinesEx(
                new((screenshot?.width * 0.2f ?? 0) - 20, 30, (screenshot?.width * 0.6f ?? 0) + 40, (screenshot?.height * 0.6f ?? 0) + 40),
                4.0f,
                WHITE
            );

            DrawText("The Editor Crashed", (sWidth - MeasureText("The Editor Crashed", 40)) / 2, (screenshot?.height * 0.6f ?? 0) + 100, 40, WHITE);

            EndDrawing();
        }
        else
        {
            BeginDrawing();
            ClearBackground(BLACK);

            DrawText("The editor has crashed", (sWidth - MeasureText("The editor has crashed", 50)) / 2, 50, 50, WHITE);

            if (exception is null)
            {
                DrawText("There's no information to display unfortunately", (sWidth - MeasureText("There's no information to display unfortunately", 40)), 200, 40, WHITE);
            }
            else
            {
                DrawText($"{exception}", 50, 200, 16, WHITE);
            }
            EndDrawing();
        }
    }
}

#nullable disable
