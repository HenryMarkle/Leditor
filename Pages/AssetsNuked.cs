using static Raylib_cs.Raylib;

namespace Leditor;

public class AssetsNukedPage(Serilog.Core.Logger logger) : IPage
{
    private readonly Serilog.Core.Logger _logger = logger;
    
    private const string MissingAssetsFolderWarnTitleText = "The assets folder is missing";
    private const string MissingAssetsFolderWarnSubtitleText = "The program cannot work without it; Please restore it before trying again.";
    
    public void Draw()
    {
        var width = GetScreenWidth();

        BeginDrawing();
        ClearBackground(Color.Black);

        DrawText(
            MissingAssetsFolderWarnTitleText,
            (width - MeasureText(MissingAssetsFolderWarnTitleText, 50)) / 2,
            200,
            50,
            Color.White
        );
        DrawText(MissingAssetsFolderWarnSubtitleText,
            (width - MeasureText(MissingAssetsFolderWarnSubtitleText, 20)) / 2,
            400,
            20,
            Color.White
        );
        EndDrawing();
    }
}