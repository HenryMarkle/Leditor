using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class StartPage(Serilog.Core.Logger logger) : IPage
{
    readonly Serilog.Core.Logger logger = logger;
    
    public void Draw()
    {
        GLOBALS.PreviousPage = 0;

        BeginDrawing();
        {
            ClearBackground(new(170, 170, 170, 255));

            if (RayGui.GuiButton(new(GetScreenWidth() / 2 - 150, GetScreenHeight() / 2 - 40, 300, 40), "Create New Project"))
            {
                GLOBALS.NewFlag = true;
                GLOBALS.Page = 6;
            }

            if (RayGui.GuiButton(new(GetScreenWidth() / 2 - 150, GetScreenHeight() / 2, 300, 40), "Load Project"))
            {
                GLOBALS.Page = 11;
            }
        }
        EndDrawing();
    }
}