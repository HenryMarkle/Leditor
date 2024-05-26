using static Raylib_cs.Raylib;

namespace Leditor.Pages;

internal class FailedTileCheckOnLoadPage : EditorPage
{
    public override void Dispose()
    {
        Disposed = true;
    }
    
    private const string MissingTileWarnTitleText = "Your project seems to contain undefined tiles";
    private const string MissingTileWarnSubtitleText = "If you used custom tiles on this project from a different level editor, please use its Init.txt";
    
    private const string NotFoundTileWarnTitleText = "Your project seems to have old tiles";
    
    private const string MissingTileTextureWarnTitleText = "Your project contains a tile with no texture";
    private const string MissingTileTextureWarnSubTitleText = "If you have appended to the Init.txt file, please make sure you include the textures as well";
    
    private const string MissingMaterialWarnTitleText = "Your project seems to have undefined materials";
    private const string MissingMaterialWarnSubtitleText = "Please update the materials init.txt file before loading this project";

    
    public override void Draw()
    {
        var width = GetScreenWidth();
        var height = GetScreenHeight();

        var okButtonRect = new Rectangle(width / 2 - 100, height - 200, 200, 60);

        BeginDrawing();
        {
            ClearBackground(new(0, 0, 0, 255));

            if (GLOBALS.TileCheck?.Result.MissingTileDefinitions.Count > 0) {
                DrawText(
                    MissingTileWarnTitleText,
                    (width - MeasureText(MissingTileWarnTitleText, 50)) / 2,
                    200,
                    50,
                    new(255, 255, 255, 255)
                );
                DrawText(MissingTileWarnSubtitleText, (width - MeasureText(MissingTileWarnSubtitleText, 20)) / 2, 400, 20, new(255, 255, 255, 255));
            } else if (GLOBALS.TileCheck?.Result.MissingTileTextures.Count > 0) {
                DrawText(
                    MissingTileTextureWarnTitleText,
                    (width - MeasureText(MissingTileTextureWarnTitleText, 50)) / 2,
                    200,
                    50,
                    new(255, 255, 255, 255)
                );
                DrawText(MissingTileTextureWarnSubTitleText, (width - MeasureText(MissingTileTextureWarnSubTitleText, 20)) / 2, 400, 20, new(255, 255, 255, 255));
            } else if (GLOBALS.TileCheck.Result.MissingMaterialDefinitions.Count > 0) {
                DrawText(
                    MissingMaterialWarnTitleText,
                    (width - MeasureText(MissingMaterialWarnTitleText, 50)) / 2,
                    200,
                    50,
                    new(255, 255, 255, 255)
                );

                DrawText(
                    MissingMaterialWarnSubtitleText,
                    (width - MeasureText(MissingMaterialWarnSubtitleText, 20)) / 2,
                    400,
                    20,
                    new(255, 255, 255, 255)
                );
            }


            DrawRectangleRoundedLines(okButtonRect, 3, 6, 3, new(255, 255, 255, 255));
            DrawText("Ok", (int)(okButtonRect.X + (okButtonRect.Width - MeasureText("Ok", 20)) / 2), (int)(okButtonRect
                .Y + 15), 20, new(255, 255, 255, 255));

            if (CheckCollisionPointRec(GetMousePosition(), okButtonRect))
            {
                SetMouseCursor(MouseCursor.PointingHand);

                if (IsMouseButtonPressed(MouseButton.Left))
                {
                    SetMouseCursor(MouseCursor.Default);

                    GLOBALS.TileCheck = null;
                    GLOBALS.Page = 0;
                }
            }
            else SetMouseCursor(MouseCursor.Default);
        }
        EndDrawing();    
    }
}