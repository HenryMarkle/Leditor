using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class HelpPage(Serilog.Core.Logger logger) : IPage
{
    private readonly Serilog.Core.Logger _logger = logger;
    
    private int _helpScrollIndex;
    private int _helpSubSection;
    
    private readonly byte[] _helpPanelBytes = "Shortcuts"u8.ToArray();
    
    public void Draw()
    {
        GLOBALS.Page = 9;

        if (IsKeyPressed(KeyboardKey.KEY_ONE)) GLOBALS.Page = 1;
        if (IsKeyReleased(KeyboardKey.KEY_TWO)) GLOBALS.Page = 2;
        if (IsKeyReleased(KeyboardKey.KEY_THREE)) GLOBALS.Page = 3;
        if (IsKeyReleased(KeyboardKey.KEY_FOUR)) GLOBALS.Page = 4;
        if (IsKeyReleased(KeyboardKey.KEY_FIVE)) GLOBALS.Page = 5;
        if (IsKeyReleased(KeyboardKey.KEY_SIX))
        {
            GLOBALS.ResizeFlag = true;
            GLOBALS.Page = 6;
        }
        if (IsKeyReleased(KeyboardKey.KEY_SEVEN)) GLOBALS.Page = 7;
        if (IsKeyReleased(KeyboardKey.KEY_EIGHT)) GLOBALS.Page = 8;
        //if (Raylib.IsKeyReleased(KeyboardKey.KEY_NINE)) page = 9;

        BeginDrawing();
        {
            ClearBackground(new(170, 170, 170, 255));

            unsafe
            {
                fixed (byte* pt = _helpPanelBytes)
                {
                    RayGui.GuiPanel(
                        new(100, 100, Raylib.GetScreenWidth() - 200, Raylib.GetScreenHeight() - 200),
                        (sbyte*)pt
                    );
                }
                
                fixed (int* scrollIndex = &_helpScrollIndex)
                    _helpSubSection = RayGui.GuiListView(
                        new Rectangle(120, 150, 250, Raylib.GetScreenHeight() - 270),
                        "Main Screen;Geometry Editor;Cameras Editor;Light Editor;Effects Editor;Tiles Editor; Props Editor",
                        scrollIndex,
                        _helpSubSection
                    );
            }

            Raylib.DrawRectangleLines(
                390,
                150,
                GetScreenWidth() - 510,
                GetScreenHeight() - 270,
                new(170, 170, 170, 255)
            );

            switch (_helpSubSection)
            {
                case 0: // main screen
                    DrawText(
                        " [1] - Main screen\n[2] - Geometry editor\n[3] - Tiles editor\n[4] - Cameras editor\n" +
                        "[5] - Light editor\n[6] - Edit dimensions\n[7] - Effects editor\n[8] - Props editor",
                        400,
                        160,
                        20,
                        new(0, 0, 0, 255)
                    );
                    break;

                case 1: // geometry editor
                    DrawText(
                        "[W] [A] [S] [D] - Navigate the geometry tiles menu\n" +
                        "[L] - Change current layer\n" +
                        "[M] - Toggle grid (contrast)",
                        400,
                        160,
                        20,
                        new(0, 0, 0, 255)
                    );
                    break;

                case 2: // cameras editor
                    DrawText(
                        "[N] - New Camera\n" +
                        "[D] - Delete dragged camera\n" +
                        "[SPACE] - Do both\n" +
                        "[LEFT CLICK]  - Move a camera around\n" +
                        "[RIGHT CLICK] - Move around",
                        400,
                        160,
                        20,
                        new(0, 0, 0, 255)
                    );
                    break;

                case 3: // light editor
                    DrawText(
                        "[Q] [E] - Rotate brush (counter-)clockwise\n" +
                        "[SHIFT] + [Q] [R] - Rotate brush faster\n" +
                        "[W] [S] - Resize brush vertically\n" +
                        "[A] [D] - Resize brush horizontally\n" +
                        "[R] [F] - Change brush\n" +
                        "[C] - Toggle shadow eraser\n",
                        400,
                        160,
                        20,
                        new(0, 0, 0, 255)
                    );
                    break;

                case 4: // effects editor
                    DrawText(
                        "[Right Click] - Drag level\n" +
                        "[Left Click] - Paint/erase effect\n" +
                        "[Mouse Wheel] - Resize brush\n" +
                        "[W] [S] - Move to next/previous effect\n" +
                        "[SHIFT] + [W] [S] - Change applied effect order\n" +
                        "[N] - Add new effect\n" +
                        "[O] - Show/hide effect options",
                        400,
                        160,
                        20,
                        new(0, 0, 0, 255)
                    );
                    break;

                case 5:
                    break;

                case 6:
                    break;
            }
        }
        Raylib.EndDrawing();
    }
}