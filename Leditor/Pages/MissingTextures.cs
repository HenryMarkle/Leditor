using static Raylib_cs.Raylib;

namespace Leditor.Pages;

internal class MissingTexturesPage : EditorPage
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
            "The editor is missing tile textures",
            (width - MeasureText("The editor is missing tile textures", 50)) / 2,
            200,
            50,
            Color.White
        );

        DrawText(
            "Check the logs for the list of missing textures; Please restore them and try again",
            (width - MeasureText("Check the logs for the list of missing textures; Please restore them and try again", 20)) / 2,
            400,
            20,
            Color.White
        );
        // EndDrawing();
    }
}