using static Raylib_cs.Raylib;

namespace Leditor.Pages;

internal class MissingInitFilePage : EditorPage
{
    public override void Dispose()
    {
        Disposed = true;
    }
    public override void Draw()
    {
        var width = GetScreenWidth();

        // BeginDrawing();
        ClearBackground(Color.Black);

        DrawText(
            "The editor is missing the Init.txt file",
            (width - MeasureText("The editor is missing the Init.txt file", 50)) / 2,
            200,
            50,
            Color.White
        );

        DrawText(
            "The editor cannot function without it; Please restore it and try again.",
            (width - MeasureText("The editor cannot function without it; Please restore it and try again.", 20)) / 2,
            400,
            20,
            Color.White
        );
        // EndDrawing();
    }
}