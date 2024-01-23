using static Raylib_CsLo.Raylib;

namespace Leditor;

public class MissingTexturesPage(Serilog.Core.Logger logger) : IPage
{
    private readonly Serilog.Core.Logger _logger = logger;
    
    public void Draw() 
    {
        var width = GetScreenWidth();

        BeginDrawing();
        ClearBackground(BLACK);

        DrawText(
            "The editor is missing tile textures",
            (width - MeasureText("The editor is missing tile textures", 50)) / 2,
            200,
            50,
            WHITE
        );

        DrawText(
            "Check the logs for the list of missing textures; Please restore them and try again",
            (width - MeasureText("Check the logs for the list of missing textures; Please restore them and try again", 20)) / 2,
            400,
            20,
            WHITE
        );
        EndDrawing();
    }
}