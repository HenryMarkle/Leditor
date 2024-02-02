using System.Numerics;
using System.Text;
using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class DimensionsEditorPage(Serilog.Core.Logger logger) : IPage
{
    internal event EventHandler ProjectCreated;
    
    readonly Serilog.Core.Logger logger = logger;
    readonly byte[] panelBytes = Encoding.ASCII.GetBytes("New Level");

    int matrixWidthValue = GLOBALS.InitialMatrixWidth;
    int matrixHeightValue = GLOBALS.InitialMatrixHeight;
    
    int leftPadding = 12;
    int rightPadding = 12;
    int topPadding = 3;
    int bottomPadding = 5;
    int editControl = 0;

    int rows = 1;
    int columns = 1;

    bool simpleFillLayer1 = true;
    bool simpleFillLayer2 = true;
    bool simpleFillLayer3 = false;

    bool createCameras = true;

    bool fillLayer1 = false;
    bool fillLayer2 = false;
    bool fillLayer3 = false;

    bool advanced = false;

    /// <summary>
    /// Only call in drawing mode
    /// </summary>
    static void ResizeLightMap(int newWidth, int newHeight)
    {
        var scale = GLOBALS.Scale;
        var image = LoadImageFromTexture(GLOBALS.Textures.LightMap.texture);
        UnloadRenderTexture(GLOBALS.Textures.LightMap);
        var texture = LoadTextureFromImage(image);
        UnloadImage(image);

        GLOBALS.Textures.LightMap = LoadRenderTexture((newWidth * scale) + 300, (newHeight) * scale + 300);

        BeginTextureMode(GLOBALS.Textures.LightMap);
        ClearBackground(new(255, 255, 255, 255));
        DrawTexture(texture, 0, 0, new(255, 255, 255, 255));
        EndTextureMode();

        UnloadTexture(texture);
    }

    public void Draw()
    {
        BeginDrawing();

        ClearBackground(new(170, 170, 170, 255));

        Rectangle panelRect = new(30, 60, Raylib.GetScreenWidth() - 60, Raylib.GetScreenHeight() - 120);
        Rectangle visualRect = new(panelRect.x + 300, panelRect.y + 50, panelRect.width - 320, panelRect.height - 70);

        unsafe
        {
            fixed (byte* pt = panelBytes)
            {
                RayGui.GuiPanel(panelRect, (sbyte*)pt);
            }
        }

        if (advanced)
        {
            RayGui.GuiLine(new(50, 200, Raylib.GetScreenWidth() - 100, 40), "Padding");

            unsafe
            {
                fixed (int* width = &matrixWidthValue)
                {
                    Raylib.DrawText("Width", 50, 110, 20, new(0, 0, 0, 255));
                    if (RayGui.GuiSpinner(new(130, 100, 300, 40), "", width, 72, 999, editControl == 0))
                    {
                        editControl = 0;
                    }
                }

                fixed (int* height = &matrixHeightValue)
                {

                    DrawText("Height", 50, 160, 20, new(0, 0, 0, 255));
                    if (RayGui.GuiSpinner(new(130, 150, 300, 40), "", height, 43, 999, editControl == 1))
                    {
                        editControl = 1;
                    }
                }

                fixed (int* left = &leftPadding)
                {
                    DrawText("Left", 50, 260, 20, new(0, 0, 0, 255));
                    if (RayGui.GuiSpinner(new(130, 250, 300, 40), "", left, 0, 333, editControl == 2))
                    {
                        editControl = 2;
                    }
                }

                fixed (int* top = &topPadding)
                {
                    DrawText("Top", 50, 360, 20, new(0, 0, 0, 255));
                    if (RayGui.GuiSpinner(new(130, 350, 300, 40), "", top, 0, 111, editControl == 4))
                    {
                        editControl = 4;
                    }
                }

                fixed (int* right = &rightPadding)
                {
                    DrawText("Right", 50, 310, 20, new(0, 0, 0, 255));
                    if (RayGui.GuiSpinner(new(130, 300, 300, 40), "", right, 0, 333, editControl == 3))
                    {
                        editControl = 3;
                    }
                }

                fixed (int* bottom = &bottomPadding)
                {
                    DrawText("Bottom", 50, 410, 20, new(0, 0, 0, 255));
                    if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 400, 300, 40), "", bottom, 0, 111, editControl == 5))
                    {
                        editControl = 5;
                    }
                }
            }

            var isBecomingLarger = matrixWidthValue > GLOBALS.Level.Width || matrixHeightValue > GLOBALS.Level.Height;

            if (isBecomingLarger)
            {
                Raylib_CsLo.RayGui.GuiLine(new(600, 100, 400, 20), "Fill extra space");
                fillLayer1 = Raylib_CsLo.RayGui.GuiCheckBox(new(600, 130, 28, 28), "Fill Layer 1", fillLayer1);
                fillLayer2 = Raylib_CsLo.RayGui.GuiCheckBox(new(750, 130, 28, 28), "Fill Layer 2", fillLayer2);
                fillLayer3 = Raylib_CsLo.RayGui.GuiCheckBox(new(900, 130, 28, 28), "Fill Layer 3", fillLayer3);
            }

            if (Raylib_CsLo.RayGui.GuiButton(new(360, Raylib.GetScreenHeight() - 160, 300, 40), "Ok") || Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
            {
                logger.Debug("page 6: Ok button clicked");

                if (GLOBALS.ResizeFlag)
                {
                    logger.Debug("resize flag detected");

                    if (
                        GLOBALS.Level.Height != matrixHeightValue ||
                        GLOBALS.Level.Width != matrixWidthValue)
                    {
                        logger.Debug("dimensions don't match; resizing");

                        logger.Debug("resizing geometry matrix");


                        // I know this can be simplified, but I'm keeping it in case 
                        // it becomes useful in the future
                        if (isBecomingLarger)
                        {
                            var fillCell = new RunCell { Geo = 1, Stackables = [] };
                            var emptyCell = new RunCell { Geo = 0, Stackables = [] };

                            GLOBALS.Level.Resize(
                                matrixWidthValue,
                                matrixHeightValue,
                                (leftPadding, topPadding, rightPadding, bottomPadding),
                                [
                                    fillLayer1 ? 1 : 0,
                                    fillLayer2 ? 1 : 0,
                                    fillLayer3 ? 1 : 0
                                ]
                            );
                        }
                        else
                        {
                            GLOBALS.Level.Resize(
                                matrixWidthValue,
                                matrixHeightValue,
                                (leftPadding, topPadding, rightPadding, bottomPadding),
                                [ 0, 0, 0 ]
                            );
                        }


                        logger.Debug("resizing light map");

                        ResizeLightMap(matrixWidthValue, matrixHeightValue);
                    }

                    GLOBALS.ResizeFlag = false;
                }
                else if (GLOBALS.NewFlag)
                {
                    logger.Debug("new flag detected; creating a new level");

                    GLOBALS.Level.New(
                        matrixWidthValue,
                        matrixHeightValue,
                        (leftPadding, topPadding, rightPadding, bottomPadding),
                        [1, 1, 0]
                    );


                    UnloadRenderTexture(GLOBALS.Textures.LightMap);
                    GLOBALS.Textures.LightMap = LoadRenderTexture((GLOBALS.Level.Width * GLOBALS.Scale) + 300, (GLOBALS.Level.Height * GLOBALS.Scale) + 300);

                    BeginTextureMode(GLOBALS.Textures.LightMap);
                    ClearBackground(new(255, 255, 255, 255));
                    EndTextureMode();

                    GLOBALS.NewFlag = false;
                }

                GLOBALS.Page = 1;
            }

            if (Raylib_CsLo.RayGui.GuiButton(new(50, Raylib.GetScreenHeight() - 160, 300, 40), "Cancel"))
            {
                logger.Debug("page 6: Cancel button clicked");

                leftPadding = GLOBALS.Level.Padding.left;
                rightPadding = GLOBALS.Level.Padding.right;
                topPadding = GLOBALS.Level.Padding.top;
                bottomPadding = GLOBALS.Level.Padding.bottom;

                matrixWidthValue = GLOBALS.Level.Width;
                matrixHeightValue = GLOBALS.Level.Height;

                GLOBALS.Page = GLOBALS.PreviousPage;
            }
        }
        else
        {
            unsafe
            {
                fixed (int* columnsPtr = &columns)
                {
                    RayGui.GuiSpinner(new(panelRect.x + 20, panelRect.y + 50, 100, 40), "", columnsPtr, 1, 4, false);
                }
                
                fixed (int* rowsPtr = &rows)
                {
                    RayGui.GuiSpinner(new(panelRect.x + 180, panelRect.y + 50, 100, 40), "", rowsPtr, 1, 4, false);
                }
            }

            RayGui.GuiLine(new(panelRect.x + 20, panelRect.y + 100, 260, 30), "Fill Layers");

            simpleFillLayer1 = RayGui.GuiCheckBox(new(panelRect.x + 20, panelRect.y + 150, 15, 15), "Layer 1", simpleFillLayer1);
            simpleFillLayer2 = RayGui.GuiCheckBox(new(panelRect.x + 20, panelRect.y + 170, 15, 15), "Layer 2", simpleFillLayer2);
            simpleFillLayer3 = RayGui.GuiCheckBox(new(panelRect.x + 20, panelRect.y + 190, 15, 15), "Layer 3", simpleFillLayer3);

            RayGui.GuiLine(new(panelRect.x + 20, panelRect.y + 225, 260, 30), "Cameras");

            if (GLOBALS.NewFlag) createCameras = RayGui.GuiCheckBox(new(panelRect.x + 20, panelRect.y + 275, 15, 15), "Auto-Create Cameras", createCameras);

            DrawLineEx(new(panelRect.x + 140, panelRect.y + 60), new(panelRect.x + 160, panelRect.y + 80), 2f, GRAY);
            DrawLineEx(new(panelRect.x + 140, panelRect.y + 80), new(panelRect.x + 160, panelRect.y + 60), 2f, GRAY);

            if (RayGui.GuiButton(new(panelRect.x + 20, panelRect.height - 92, 260, 40), "Advanced Options")) advanced = true;
            
            if (RayGui.GuiButton(new(panelRect.x + 20, panelRect.height - 51, 260, 40), GLOBALS.NewFlag ? "Create" : "Resize"))
            {
                if (GLOBALS.NewFlag)
                {
                    logger.Debug("new flag detected; creating a new level");

                    GLOBALS.Level.New(
                        columns * 52 + 20,
                        rows * 40 + 3,
                        (6, 3, 6, 5),
                        [1, 1, 0]
                    );


                    UnloadRenderTexture(GLOBALS.Textures.LightMap);
                    GLOBALS.Textures.LightMap = LoadRenderTexture((GLOBALS.Level.Width * GLOBALS.Scale) + 300, (GLOBALS.Level.Height * GLOBALS.Scale) + 300);

                    BeginTextureMode(GLOBALS.Textures.LightMap);
                    ClearBackground(new(255, 255, 255, 255));
                    EndTextureMode();

                    // create cameras

                    if (createCameras)
                    {
                        for (int i = 0; i < rows; i++)
                        {
                            for (int j = 0; j < columns; j++)
                            {
                                if (i == 0 && j == 0) continue;

                                GLOBALS.Level.Cameras.Add(new RenderCamera() { Coords = new Vector2(20f + j*(GLOBALS.EditorCameraWidth - 380), 30f + i*(GLOBALS.EditorCameraHeight - 40)), Quads = new(new(), new(), new(), new()) });
                            }
                        }
                    }

                    GLOBALS.NewFlag = false;
                    ProjectCreated?.Invoke(this, EventArgs.Empty);
                }
                else if (GLOBALS.ResizeFlag)
                {
                    logger.Debug("resize flag detected");

                    if (
                        GLOBALS.Level.Height != matrixHeightValue ||
                        GLOBALS.Level.Width != matrixWidthValue)
                    {
                        logger.Debug("dimensions don't match; resizing");


                        GLOBALS.Level.Resize(
                            matrixWidthValue,
                            matrixHeightValue,
                            (leftPadding, topPadding, rightPadding, bottomPadding),
                            [
                                simpleFillLayer1 ? 1 : 0, 
                                simpleFillLayer2 ? 1 : 0, 
                                simpleFillLayer3 ? 1 : 0, 
                            ]
                        );


                        logger.Debug("resizing light map");

                        ResizeLightMap(matrixWidthValue, matrixHeightValue);
                    }

                    GLOBALS.ResizeFlag = false;
                }

                GLOBALS.Page = 1;
            }
            if (RayGui.GuiButton(new(panelRect.x + 20, panelRect.height - 10, 260, 40), "Cancel"))
            {
                logger.Debug("page 6: Cancel button clicked");

                leftPadding = GLOBALS.Level.Padding.left;
                rightPadding = GLOBALS.Level.Padding.right;
                topPadding = GLOBALS.Level.Padding.top;
                bottomPadding = GLOBALS.Level.Padding.bottom;

                matrixWidthValue = GLOBALS.Level.Width;
                matrixHeightValue = GLOBALS.Level.Height;

                GLOBALS.Page = GLOBALS.PreviousPage;
            }


            // Visualizer

            if (simpleFillLayer1) DrawRectangleRec(visualRect, GLOBALS.Settings.GeometryEditor.LayerColors.Layer1);
            if (simpleFillLayer2) DrawRectangleRec(visualRect, GLOBALS.Settings.GeometryEditor.LayerColors.Layer2);
            if (simpleFillLayer3) DrawRectangleRec(visualRect, GLOBALS.Settings.GeometryEditor.LayerColors.Layer3);

            var scale = Math.Min(visualRect.width / (columns * 1400), visualRect.height / (rows * 800));
            var width = 1400 * scale;
            var height = 800 * scale;


            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    DrawRectangleLinesEx(
                        new(visualRect.x + (visualRect.width - width*columns)/2 + j * width + 20, visualRect.y + (visualRect.height - height*rows)/2 + i * height + 20, width - 40, height - 40), 
                        4f, WHITE
                    );

                    if (GLOBALS.NewFlag && createCameras)
                    {
                        DrawCircleLines(
                            (int)(visualRect.x + (visualRect.width - width*columns)/2 + j * width + width/2), 
                            (int)(visualRect.y + (visualRect.height - height*rows)/2 + i * height + height/2),
                            50 * scale, WHITE
                        );
                    }
                }
            }
        }

        EndDrawing();
    }
}
