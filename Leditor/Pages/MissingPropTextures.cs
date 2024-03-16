using static Raylib_cs.Raylib;

namespace Leditor;

public class MissingPropTexturesPage(Serilog.Core.Logger logger) : IPage
{
    private readonly Serilog.Core.Logger _logger = logger;

    private const string Title = "The project contains missing textures";
    private const string Subtitle = "The editor cannot render them without their proper textures\n" +
                                    "Check the logs to view the missing textures";

    private readonly int _titleWidth = MeasureText(Title, 50);
    private readonly int _subtitleWidth = MeasureText(Subtitle, 20);
    
    public void Draw()
    {
        var width = GetScreenWidth();
        
        BeginDrawing();
        
        ClearBackground(Color.Black);
        
        DrawText(Title, (width - _titleWidth)/2, 200, 50, Color.White);
        DrawText(Subtitle, (width - _subtitleWidth)/2, 400, 20, Color.White);
        
        EndDrawing();
    }
}