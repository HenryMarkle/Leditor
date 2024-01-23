using static Raylib_CsLo.Raylib;

namespace Leditor;

public class SaveProjectPage(Serilog.Core.Logger logger) : IPage
{
    private readonly Serilog.Core.Logger _logger = logger;
    
    private readonly byte[] _saveProjectPanelBytes = "Save Project"u8.ToArray();
    private readonly byte[] _projectNameBufferBytes = System.Text.Encoding.ASCII.GetBytes(GLOBALS.ProjectName);
    
    public void Draw()
    {
        BeginDrawing();
        {
            ClearBackground(new(170, 170, 170, 255));

            unsafe
            {
               fixed (byte* pt = _saveProjectPanelBytes)
               {
                   RayGui.GuiPanel(
                       new(
                           GetScreenWidth() / 2 - 200,
                           GetScreenHeight() / 2 - 150,
                           400,
                           300
                       ),
                       (sbyte*)pt
                   );
               } 
               fixed (byte* bytes = _projectNameBufferBytes)
               {
                   RayGui.GuiTextBox(
                       new(
                           GetScreenWidth() / 2 - 150,
                           GetScreenHeight() / 2 - 90,
                           300,
                           40
                       ),
                       (sbyte*)bytes,
                       20,
                       true
                   );
               }
            }
            
            RayGui.GuiButton(
                new(
                    GetScreenWidth() / 2 - 150,
                    GetScreenHeight() / 2,
                    300,
                    40
                ),
                "Save"
            );

            var cancelSavePressed = RayGui.GuiButton(
                new(
                    GetScreenWidth() / 2 - 150,
                    GetScreenHeight() / 2 + 50,
                    300,
                    40
                ),
                "Cancel"
            );

            if (cancelSavePressed) GLOBALS.Page = 1;
        }
        EndDrawing();
    }
}