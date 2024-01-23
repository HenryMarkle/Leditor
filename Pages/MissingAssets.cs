using static Raylib_CsLo.Raylib;

namespace Leditor;

public class MissingAssetsPage(Serilog.Core.Logger logger) : IPage
{
    private readonly Serilog.Core.Logger _logger = logger;
    
    private const string MissingAssetsSubfoldersWarnTitleText = "The editor is missing some essential assets";
    private const string MissingAssetsSubfoldersWarnSubtitleText = "The program cannot function without them; Please restore them before trying again.";

    public void Draw()
    {
        var sWidth = GetScreenWidth();
        var sHeight = GetScreenHeight();

        BeginDrawing();
        ClearBackground(BLACK);

        DrawText(
            MissingAssetsSubfoldersWarnTitleText,
            (sWidth - MeasureText(MissingAssetsSubfoldersWarnTitleText, 50)) / 2,
            200,
            50,
            WHITE
        );
        DrawText(MissingAssetsSubfoldersWarnSubtitleText,
            (sWidth - MeasureText(MissingAssetsSubfoldersWarnSubtitleText, 20)) / 2,
            400,
            20,
            WHITE
        );

        if (GLOBALS.Paths.DirectoryIntegrity.Any(d => !d.Item2))
        {
            DrawText(
                "Missing folders:\n\n" + 
                string.Join(
                    "\n\t-", 
                    GLOBALS.Paths.DirectoryIntegrity.Where(d => !d.Item2).Select(d => d.Item1)
                ),
                200,
                500,
                20,
                WHITE
            );
        }

        if (GLOBALS.Paths.FileIntegrity.Any(d => !d.Item2))
        {
            DrawText(
                "Missing files:\n\n" + 
                string.Join(
                    "\n\t-", 
                    GLOBALS.Paths.FileIntegrity.Where(d => !d.Item2).Select(d => d.Item1)
                ),
                600,
                500,
                20,
                WHITE
            );
        }
        
        EndDrawing();
    }
}