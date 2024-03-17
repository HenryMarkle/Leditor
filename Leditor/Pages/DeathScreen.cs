using System.Data;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace Leditor.Pages;

#nullable enable

internal class DeathScreen : ExceptionPage
{
    public Texture2D? Screenshot { get; set; }

    ~DeathScreen()
    {
        if (Screenshot is not null) UnloadTexture((Texture2D)Screenshot);
    }

    public override void Draw()
    {
        var sWidth = GetScreenWidth();

        if (GLOBALS.Settings.Misc.FunnyDeathScreen)
        {
            BeginDrawing();
            ClearBackground(Color.Black);

            if (Screenshot is not null) {
                DrawTexturePro(
                    (Texture2D)Screenshot,
                    new(0, 0, Screenshot?.Width ?? 0, Screenshot?.Height ?? 0),
                    new(Screenshot?.Width * 0.2f ?? 0, 50, Screenshot?.Width * 0.6f ?? 0, Screenshot?.Height * 0.6f ?? 0),
                    new(0, 0),
                    0,
                    Color.White
                );
            }

            DrawRectangleLinesEx(
                new((Screenshot?.Width * 0.2f ?? 0) - 20, 30, (Screenshot?.Width * 0.6f ?? 0) + 40, (Screenshot?.Height * 0.6f ?? 0) + 40),
                4.0f,
                Color.White
            );

            if (GLOBALS.Font is null) {
                DrawText("The Editor Crashed", (sWidth - MeasureText("The Editor Crashed", 40)) / 2, (int) (Screenshot?.Height * 0.6f ?? 0) + 100, 40, Color.White);
            }
            else {
                DrawTextEx(
                    GLOBALS.Font.Value, 
                    "The Editor Crashed", 
                    new Vector2((sWidth - MeasureText("The Editor Crashed", 40)) / 2, (Screenshot?.Height * 0.6f ?? 0) + 100),
                    40f,
                    1f,
                    Color.White
                );
            }


            EndDrawing();
        }
        else
        {
            BeginDrawing();
            ClearBackground(Color.Black);

            if (GLOBALS.Font is null) {
                DrawText("The editor has crashed", (sWidth - MeasureText("The editor has crashed", 50)) / 2, 50, 50, Color.White);
            } else {
                DrawTextEx(
                    GLOBALS.Font.Value, 
                    "The editor has crashed",
                    new ((sWidth - MeasureText("The editor has crashed", 60)) / 2, 60),
                    60,
                    1,
                    Color.White
                );
            }


            if (Exception is null)
            {
                if (GLOBALS.Font is null) {
                    DrawText("There's no information to display unfortunately", (sWidth - MeasureText("There's no information to display unfortunately", 40)), 200, 40, Color.White);
                } else {
                    DrawTextEx(
                        GLOBALS.Font.Value, 
                        "There's no information to display unfortunately", 
                        new((sWidth - MeasureText("There's no information to display unfortunately", 40))/2, 200),
                        40,
                        1,
                        Color.White
                    );
                }
            }
            else
            {
                if (GLOBALS.Font is null) {
                    DrawText($"{Exception}", 50, 200, 16, Color.White);
                } else {
                    DrawTextEx(GLOBALS.Font.Value, $"{Exception}", new(50, 200), 22, 1, Color.White);
                }
            }
            EndDrawing();
        }
    }
}

#nullable disable
