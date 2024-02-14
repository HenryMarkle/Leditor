using System.Numerics;
using System.Text;
using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class DimensionsEditorPage(Serilog.Core.Logger logger) : IPage
{
    internal event EventHandler ProjectCreated;
    
    private readonly Serilog.Core.Logger _logger = logger;
    private readonly byte[] _panelBytes = "Configuring Dimensions"u8.ToArray();

    private int _matrixWidthValue = GLOBALS.InitialMatrixWidth;
    private int _matrixHeightValue = GLOBALS.InitialMatrixHeight;
    
    private int _leftPadding = 12;
    private int _rightPadding = 12;
    private int _topPadding = 3;
    private int _bottomPadding = 5;
    private int _editControl;

    private int _rows = 1;
    private int _columns = 1;

    private bool _simpleFillLayer1 = true;
    private bool _simpleFillLayer2 = true;
    private bool _simpleFillLayer3;

    private bool _createCameras = true;

    private bool _fillLayer1;
    private bool _fillLayer2;
    private bool _fillLayer3;

    private bool _advanced;

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
            fixed (byte* pt = _panelBytes)
            {
                RayGui.GuiPanel(panelRect, (sbyte*)pt);
            }
        }

        if (_advanced)
        {
            RayGui.GuiLine(new(50, 200, Raylib.GetScreenWidth() - 100, 40), "Padding");

            unsafe
            {
                fixed (int* width = &_matrixWidthValue)
                {
                    Raylib.DrawText("Width", 50, 110, 20, new(0, 0, 0, 255));
                    if (RayGui.GuiSpinner(new(130, 100, 300, 40), "", width, 72, 999, _editControl == 0))
                    {
                        _editControl = 0;
                    }
                }

                fixed (int* height = &_matrixHeightValue)
                {

                    DrawText("Height", 50, 160, 20, new(0, 0, 0, 255));
                    if (RayGui.GuiSpinner(new(130, 150, 300, 40), "", height, 43, 999, _editControl == 1))
                    {
                        _editControl = 1;
                    }
                }

                fixed (int* left = &_leftPadding)
                {
                    DrawText("Left", 50, 260, 20, new(0, 0, 0, 255));
                    if (RayGui.GuiSpinner(new(130, 250, 300, 40), "", left, 0, 333, _editControl == 2))
                    {
                        _editControl = 2;
                    }
                }

                fixed (int* top = &_topPadding)
                {
                    DrawText("Top", 50, 360, 20, new(0, 0, 0, 255));
                    if (RayGui.GuiSpinner(new(130, 350, 300, 40), "", top, 0, 111, _editControl == 4))
                    {
                        _editControl = 4;
                    }
                }

                fixed (int* right = &_rightPadding)
                {
                    DrawText("Right", 50, 310, 20, new(0, 0, 0, 255));
                    if (RayGui.GuiSpinner(new(130, 300, 300, 40), "", right, 0, 333, _editControl == 3))
                    {
                        _editControl = 3;
                    }
                }

                fixed (int* bottom = &_bottomPadding)
                {
                    DrawText("Bottom", 50, 410, 20, new(0, 0, 0, 255));
                    if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 400, 300, 40), "", bottom, 0, 111, _editControl == 5))
                    {
                        _editControl = 5;
                    }
                }
            }

            var isBecomingLarger = _matrixWidthValue > GLOBALS.Level.Width || _matrixHeightValue > GLOBALS.Level.Height;

            if (isBecomingLarger)
            {
                RayGui.GuiLine(new(600, 100, 400, 20), "Fill extra space");
                _fillLayer1 = RayGui.GuiCheckBox(new(600, 130, 28, 28), "Fill Layer 1", _fillLayer1);
                _fillLayer2 = RayGui.GuiCheckBox(new(750, 130, 28, 28), "Fill Layer 2", _fillLayer2);
                _fillLayer3 = RayGui.GuiCheckBox(new(900, 130, 28, 28), "Fill Layer 3", _fillLayer3);
            }
            
            if (GLOBALS.NewFlag) if (RayGui.GuiButton(new(panelRect.x + 20, panelRect.height - 92, 260, 40), "Simple Options")) _advanced = false;

            if (RayGui.GuiButton(new(360, GetScreenHeight() - 160, 300, 40), "Ok") || Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
            {
                _logger.Debug("page 6: Ok button clicked");

                if (GLOBALS.ResizeFlag)
                {
                    _logger.Debug("resize flag detected");

                    if (
                        GLOBALS.Level.Height != _matrixHeightValue ||
                        GLOBALS.Level.Width != _matrixWidthValue)
                    {
                        _logger.Debug("dimensions don't match; resizing");

                        _logger.Debug("resizing geometry matrix");


                        // I know this can be simplified, but I'm keeping it in case 
                        // it becomes useful in the future
                        if (isBecomingLarger)
                        {
                            var fillCell = new RunCell { Geo = 1, Stackables = [] };
                            var emptyCell = new RunCell { Geo = 0, Stackables = [] };

                            GLOBALS.Level.Resize(
                                _matrixWidthValue,
                                _matrixHeightValue,
                                (_leftPadding, _topPadding, _rightPadding, _bottomPadding),
                                [
                                    _fillLayer1 ? 1 : 0,
                                    _fillLayer2 ? 1 : 0,
                                    _fillLayer3 ? 1 : 0
                                ]
                            );
                        }
                        else
                        {
                            GLOBALS.Level.Resize(
                                _matrixWidthValue,
                                _matrixHeightValue,
                                (_leftPadding, _topPadding, _rightPadding, _bottomPadding),
                                [ 0, 0, 0 ]
                            );
                        }


                        _logger.Debug("resizing light map");

                        ResizeLightMap(_matrixWidthValue, _matrixHeightValue);
                    }

                    GLOBALS.ResizeFlag = false;
                }
                else if (GLOBALS.NewFlag)
                {
                    _logger.Debug("new flag detected; creating a new level");
                    
                    GLOBALS.Level.New(
                        _matrixWidthValue,
                        _matrixHeightValue,
                        (_leftPadding, _topPadding, _rightPadding, _bottomPadding),
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

            if (RayGui.GuiButton(new(50, Raylib.GetScreenHeight() - 160, 300, 40), "Cancel"))
            {
                _logger.Debug("page 6: Cancel button clicked");

                _leftPadding = GLOBALS.Level.Padding.left;
                _rightPadding = GLOBALS.Level.Padding.right;
                _topPadding = GLOBALS.Level.Padding.top;
                _bottomPadding = GLOBALS.Level.Padding.bottom;

                _matrixWidthValue = GLOBALS.Level.Width;
                _matrixHeightValue = GLOBALS.Level.Height;

                GLOBALS.Page = GLOBALS.PreviousPage;
            }
        }
        else
        {
            unsafe
            {
                fixed (int* columnsPtr = &_columns)
                {
                    RayGui.GuiSpinner(new(panelRect.x + 20, panelRect.y + 50, 100, 40), "", columnsPtr, 1, 4, false);
                }
                
                fixed (int* rowsPtr = &_rows)
                {
                    RayGui.GuiSpinner(new(panelRect.x + 180, panelRect.y + 50, 100, 40), "", rowsPtr, 1, 4, false);
                }
            }

            RayGui.GuiLine(new(panelRect.x + 20, panelRect.y + 100, 260, 30), "Fill Layers");

            _simpleFillLayer1 = RayGui.GuiCheckBox(new(panelRect.x + 20, panelRect.y + 150, 15, 15), "Layer 1", _simpleFillLayer1);
            _simpleFillLayer2 = RayGui.GuiCheckBox(new(panelRect.x + 20, panelRect.y + 170, 15, 15), "Layer 2", _simpleFillLayer2);
            _simpleFillLayer3 = RayGui.GuiCheckBox(new(panelRect.x + 20, panelRect.y + 190, 15, 15), "Layer 3", _simpleFillLayer3);

            RayGui.GuiLine(new(panelRect.x + 20, panelRect.y + 225, 260, 30), "Cameras");

            if (GLOBALS.NewFlag) _createCameras = RayGui.GuiCheckBox(new(panelRect.x + 20, panelRect.y + 275, 15, 15), "Auto-Create Cameras", _createCameras);

            DrawLineEx(new(panelRect.x + 140, panelRect.y + 60), new(panelRect.x + 160, panelRect.y + 80), 2f, GRAY);
            DrawLineEx(new(panelRect.x + 140, panelRect.y + 80), new(panelRect.x + 160, panelRect.y + 60), 2f, GRAY);

            if (RayGui.GuiButton(new(panelRect.x + 20, panelRect.height - 92, 260, 40), "Advanced Options")) _advanced = true;
            
            if (RayGui.GuiButton(new(panelRect.x + 20, panelRect.height - 51, 260, 40), GLOBALS.NewFlag ? "Create" : "Resize"))
            {
                if (GLOBALS.NewFlag)
                {
                    _advanced = true;
                    
                    _logger.Debug("new flag detected; creating a new level");

                    GLOBALS.Level.New(
                        _columns * 52 + 20,
                        _rows * 40 + 3,
                        (6, 3, 6, 5),
                        [_simpleFillLayer1 ? 1 : 0, _simpleFillLayer2 ? 1 : 0, _simpleFillLayer3 ? 1 : 0]
                    );
                    
                    UnloadRenderTexture(GLOBALS.Textures.LightMap);
                    GLOBALS.Textures.LightMap = LoadRenderTexture((GLOBALS.Level.Width * GLOBALS.Scale) + 300, (GLOBALS.Level.Height * GLOBALS.Scale) + 300);

                    BeginTextureMode(GLOBALS.Textures.LightMap);
                    ClearBackground(new(255, 255, 255, 255));
                    EndTextureMode();

                    // create cameras

                    if (_createCameras)
                    {
                        for (int i = 0; i < _rows; i++)
                        {
                            for (int j = 0; j < _columns; j++)
                            {
                                if (i == 0 && j == 0) continue;

                                GLOBALS.Level.Cameras.Add(new RenderCamera() { Coords = new Vector2(20f + j*(GLOBALS.EditorCameraWidth - 380), 30f + i*(GLOBALS.EditorCameraHeight - 40)), Quad = new(new(), new(), new(), new()) });
                            }
                        }
                        GLOBALS.CamQuadLocks = new int[_rows*_columns];
                    }
                    else GLOBALS.CamQuadLocks = new int[1];

                    GLOBALS.NewFlag = false;
                    ProjectCreated?.Invoke(this, EventArgs.Empty);
                }
                else if (GLOBALS.ResizeFlag)
                {
                    _logger.Debug("resize flag detected");

                    if (
                        GLOBALS.Level.Height != _matrixHeightValue ||
                        GLOBALS.Level.Width != _matrixWidthValue)
                    {
                        _logger.Debug("dimensions don't match; resizing");


                        GLOBALS.Level.Resize(
                            _matrixWidthValue,
                            _matrixHeightValue,
                            (_leftPadding, _topPadding, _rightPadding, _bottomPadding),
                            [
                                _simpleFillLayer1 ? 1 : 0, 
                                _simpleFillLayer2 ? 1 : 0, 
                                _simpleFillLayer3 ? 1 : 0, 
                            ]
                        );


                        _logger.Debug("resizing light map");

                        ResizeLightMap(_matrixWidthValue, _matrixHeightValue);
                    }

                    GLOBALS.ResizeFlag = false;
                }

                GLOBALS.Page = 1;
            }
            if (RayGui.GuiButton(new(panelRect.x + 20, panelRect.height - 10, 260, 40), "Cancel"))
            {
                _logger.Debug("page 6: Cancel button clicked");

                _leftPadding = GLOBALS.Level.Padding.left;
                _rightPadding = GLOBALS.Level.Padding.right;
                _topPadding = GLOBALS.Level.Padding.top;
                _bottomPadding = GLOBALS.Level.Padding.bottom;

                _matrixWidthValue = GLOBALS.Level.Width;
                _matrixHeightValue = GLOBALS.Level.Height;

                GLOBALS.Page = GLOBALS.PreviousPage;
            }


            // Visualizer

            if (_simpleFillLayer1) DrawRectangleRec(visualRect, GLOBALS.Settings.GeometryEditor.LayerColors.Layer1);
            if (_simpleFillLayer2) DrawRectangleRec(visualRect, GLOBALS.Settings.GeometryEditor.LayerColors.Layer2);
            if (_simpleFillLayer3) DrawRectangleRec(visualRect, GLOBALS.Settings.GeometryEditor.LayerColors.Layer3);

            var scale = Math.Min(visualRect.width / (_columns * 1400), visualRect.height / (_rows * 800));
            var width = 1400 * scale;
            var height = 800 * scale;


            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    DrawRectangleLinesEx(
                        new(visualRect.x + (visualRect.width - width*_columns)/2 + j * width + 20, visualRect.y + (visualRect.height - height*_rows)/2 + i * height + 20, width - 40, height - 40), 
                        4f, WHITE
                    );

                    if (GLOBALS.NewFlag && _createCameras)
                    {
                        DrawCircleLines(
                            (int)(visualRect.x + (visualRect.width - width*_columns)/2 + j * width + width/2), 
                            (int)(visualRect.y + (visualRect.height - height*_rows)/2 + i * height + height/2),
                            50 * scale, WHITE
                        );
                    }
                }
            }
        }

        EndDrawing();
    }
}
