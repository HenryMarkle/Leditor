using static Raylib_CsLo.Raylib;

namespace Leditor;

public class MissingInitFilePage(Serilog.Core.Logger logger) : IPage
{
    private readonly Serilog.Core.Logger _logger = logger;

    public void Draw()
    {
        var width = GetScreenWidth();

        BeginDrawing();
        ClearBackground(BLACK);

        DrawText(
            "The editor is missing the Init.txt file",
            (width - MeasureText("The editor is missing the Init.txt file", 50)) / 2,
            200,
            50,
            WHITE
        );

        DrawText(
            "The editor cannot function without it; Please restore it and try again.",
            (width - MeasureText("The editor cannot function without it; Please restore it and try again.", 20)) / 2,
            400,
            20,
            WHITE
        );
        EndDrawing();
    }
}