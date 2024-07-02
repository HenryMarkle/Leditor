using static Raylib_cs.Raylib;

namespace Leditor.Pages;

internal class MissingAssetsPage : EditorPage
{
    public override void Dispose()
    {
        Disposed = true;
    }
    
    private const string MissingAssetsSubfoldersWarnTitleText = "The editor is missing some essential assets";
    private const string MissingAssetsSubfoldersWarnSubtitleText = "The program cannot function without them.\nCheck the logs and restore the missing resources before trying again.";

    public override void Draw()
    {
        var sWidth = GetScreenWidth();
        var sHeight = GetScreenHeight();

        // BeginDrawing();
        ClearBackground(Color.Black);

        DrawText(
            MissingAssetsSubfoldersWarnTitleText,
            (sWidth - MeasureText(MissingAssetsSubfoldersWarnTitleText, 50)) / 2,
            200,
            50,
            Color.White
        );
        DrawText(MissingAssetsSubfoldersWarnSubtitleText,
            (sWidth - MeasureText(MissingAssetsSubfoldersWarnSubtitleText, 20)) / 2,
            400,
            20,
            Color.White
        );
        
        // EndDrawing();
    }
}