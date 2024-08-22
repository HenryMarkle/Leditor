using System.Numerics;
using ImGuiNET;
using rlImGui_cs;
using static Raylib_cs.Raylib;

using Leditor.Data.Geometry;

namespace Leditor.Pages;

internal class DimensionsEditorPage : EditorPage, IContextListener
{
    public override void Dispose()
    {
        Disposed = true;
    }
    
    private Camera2D _camera = new() { Target = new Vector2(-20, 20), Zoom = 1.0f };
    
    private int _matrixWidthValue = GLOBALS.InitialMatrixWidth;
    private int _matrixHeightValue = GLOBALS.InitialMatrixHeight;
    
    private int _leftPadding = 6;
    private int _rightPadding = 6;
    private int _topPadding = 3;
    private int _bottomPadding = 5;

    private bool _fillLayer1;
    private bool _fillLayer2;
    private bool _fillLayer3;

    private bool _shouldRedrawLevel = true;

    private bool _clickTracker;

    private int _resizeLock;

    private bool _isMainWinHovered;
    private bool _isOptionsWinHovered;

    private bool _grid;

    private Vector2 _levelOrigin = Vector2.Zero;

    private Vector2 _leftSideTop = Vector2.Zero;
    private Vector2 _leftSideBottom = Vector2.Zero;

    private Vector2 _topSideLeft = Vector2.Zero;
    private Vector2 _topSideRight = Vector2.Zero;

    private Vector2 _rightSideTop = Vector2.Zero;
    private Vector2 _rightSideBottom = Vector2.Zero;

    private Vector2 _bottomSideLeft = Vector2.Zero;
    private Vector2 _bottomSideRight = Vector2.Zero;

    private Vector2 _prevFirst;
    private Vector2 _prevSecond;

    private bool _isInputBusy;

    private void ResetSides() {
        var size = new Vector2(GLOBALS.Level.Width, GLOBALS.Level.Height) * 20;

        _leftSideTop = Vector2.Zero;
        _leftSideBottom = _leftSideTop + new Vector2(0, size.Y);
        
        _topSideLeft = Vector2.Zero;
        _topSideRight = _topSideLeft + new Vector2(size.X, 0);
        
        _rightSideTop = new Vector2(size.X, 0);
        _rightSideBottom = size;

        _bottomSideLeft = new Vector2(0, size.Y);
        _bottomSideRight = size;
    }

    private (int x, int y) _originOffset = (0, 0);

    private enum Resizing { Left, Top, Right, Bottom, None }

    private Resizing _resizing = Resizing.None;

    /// <summary>
    /// Only call in drawing mode
    /// </summary>
    static void ResizeLightMap(Vector2 origin, int newWidth, int newHeight)
    {
        var scale = GLOBALS.Scale;
        var image = LoadImageFromTexture(GLOBALS.Textures.LightMap.Texture);
        ImageFlipVertical(ref image);
        UnloadRenderTexture(GLOBALS.Textures.LightMap);
        var texture = LoadTextureFromImage(image);
        UnloadImage(image);

        GLOBALS.Textures.LightMap = LoadRenderTexture((newWidth * scale) + 300, (newHeight) * scale + 300);

        BeginTextureMode(GLOBALS.Textures.LightMap);
        ClearBackground(new(255, 255, 255, 255));
        DrawTextureV(texture, origin, new(255, 255, 255, 255));
        EndTextureMode();

        UnloadTexture(texture);
    }

    #nullable enable
    public void OnProjectLoaded(object? sender, EventArgs e)
    {
        _matrixWidthValue = GLOBALS.Level.Width;
        _matrixHeightValue = GLOBALS.Level.Height;
        (_leftPadding, _topPadding, _rightPadding, _bottomPadding) = GLOBALS.Level.Padding;

        _shouldRedrawLevel = true;

        _camera = new() { Target = new Vector2(-20, 20), Zoom = 1.0f };

        ResetSides();
    }

    public void OnProjectCreated(object? sender, EventArgs e)
    {
        _matrixWidthValue = GLOBALS.Level.Width;
        _matrixHeightValue = GLOBALS.Level.Height;
        (_leftPadding, _topPadding, _rightPadding, _bottomPadding) = GLOBALS.Level.Padding;

        _shouldRedrawLevel = true;

        _camera = new() { Target = new Vector2(-20, 20), Zoom = 1.0f };

        ResetSides();
    }

    public void OnGlobalResourcesUpdated()
    {
        _shouldRedrawLevel = true;
    }

    public void OnPageUpdated(int previous, int @next) {
        if (@next == 6) {
            _matrixWidthValue = GLOBALS.Level.Width;
            _matrixHeightValue = GLOBALS.Level.Height;
            (_leftPadding, _topPadding, _rightPadding, _bottomPadding) = GLOBALS.Level.Padding;
            _shouldRedrawLevel = true;

            // ResetSides();
        }
    }

    public override void Draw()
    {
        var isWinBusy = _isMainWinHovered || _isOptionsWinHovered;

        var mouse = GetMousePosition();
        var worldMouse = GetScreenToWorld2D(mouse, _camera);

        GLOBALS.LockNavigation = _isInputBusy;
        _isInputBusy = false;

        #region Shortcuts
        if (!isWinBusy || _clickTracker)
        {
            // Drag
            if (IsMouseButtonDown(MouseButton.Middle))
            {
                _clickTracker = true;
                var delta = GetMouseDelta();
                delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
                _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
            }

            if (IsMouseButtonReleased(MouseButton.Middle)) _clickTracker = false;


            // Zoom
            var tileWheel = GetMouseWheelMove();
            if (tileWheel != 0)
            {
                var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
                _camera.Offset = GetMousePosition();
                _camera.Target = mouseWorldPosition;
                _camera.Zoom += tileWheel * GLOBALS.ZoomIncrement;
                if (_camera.Zoom < GLOBALS.ZoomIncrement) _camera.Zoom = GLOBALS.ZoomIncrement;
            }

            // Level Border Hover

            const int colThres = 5;

            if (_resizeLock == 0) {
                
                // Left Side
                if (CheckCollisionPointLine(
                    worldMouse, 
                    _leftSideTop, 
                    _leftSideBottom, 
                    colThres
                )) {
                    SetMouseCursor(MouseCursor.ResizeEw);
                    _resizing = Resizing.Left;
                    
                    if (IsMouseButtonDown(MouseButton.Left)) {
                        _resizeLock = 1;

                        _prevFirst = _leftSideTop;
                        _prevSecond = _leftSideBottom;
                    }
                }
                // Top Side
                else if (CheckCollisionPointLine(
                    worldMouse, 
                    _topSideLeft, 
                    _topSideRight, 
                    colThres
                )) {
                    SetMouseCursor(MouseCursor.ResizeNs);
                    _resizing = Resizing.Top;
                    
                    if (IsMouseButtonDown(MouseButton.Left)) {
                        _resizeLock = 2;

                        _prevFirst = _topSideLeft;
                        _prevSecond = _topSideRight;
                    }
                }
                // Right Side
                else if (CheckCollisionPointLine(
                    worldMouse, 
                    _rightSideTop, 
                    _rightSideBottom, 
                    colThres
                )) {
                    SetMouseCursor(MouseCursor.ResizeEw);
                    _resizing = Resizing.Right;

                    if (IsMouseButtonDown(MouseButton.Left)) {
                        _resizeLock = 3;

                        _prevFirst = _rightSideTop;
                        _prevSecond = _rightSideBottom;
                    }
                }
                // Bottom Side
                else if (CheckCollisionPointLine(
                    worldMouse, 
                    _bottomSideLeft, 
                    _bottomSideRight, 
                    colThres
                )) {
                    SetMouseCursor(MouseCursor.ResizeNs);
                    _resizing = Resizing.Bottom;

                    if (IsMouseButtonDown(MouseButton.Left)) {
                        _resizeLock = 4;

                        _prevFirst = _bottomSideLeft;
                        _prevSecond = _bottomSideRight;
                    }
                }
                // Else
                else {
                    SetMouseCursor(MouseCursor.Default);
                    _resizing = Resizing.None;
                }
            }

            // Resizing

            {
                var scaledDown = ((int)worldMouse.X / 20, (int)worldMouse.Y / 20);
                var scaledBack = new Vector2(scaledDown.Item1 * 20, scaledDown.Item2 * 20);

                switch (_resizeLock) {
                    case 1: // Left Side
                    _leftSideTop.X = _leftSideBottom.X = _topSideLeft.X = _bottomSideLeft.X = scaledBack.X;
                    break;

                    case 2: // Top Side
                    _topSideLeft.Y = _topSideRight.Y = _leftSideTop.Y = _rightSideTop.Y = scaledBack.Y;
                    break;

                    case 3: // Right Side
                    _rightSideTop.X = _rightSideBottom.X = _topSideRight.X = _bottomSideRight.X = scaledBack.X;
                    break;
                 
                    case 4: // Bottom
                    _bottomSideLeft.Y = _bottomSideRight.Y = _leftSideBottom.Y = _rightSideBottom.Y = scaledBack.Y;
                    break;
                }
            }

            // Apply Resize
            if (IsMouseButtonReleased(MouseButton.Left) && _resizeLock != 0) {
                var left = (int)(_levelOrigin.X -_leftSideTop.X)/20;
                var top = (int)(_levelOrigin.Y - _topSideLeft.Y)/20;
                var right = (int)_rightSideTop.X/20 - (int)(_levelOrigin.X/20 + GLOBALS.Level.Width);
                var bottom = (int)_bottomSideRight.Y/20 - (int)(_levelOrigin.Y/20 + GLOBALS.Level.Height);

                if (_leftSideTop.X == _rightSideTop.X || _topSideLeft.Y == _bottomSideLeft.Y) {

                    switch (_resizeLock) {
                        case 1: // Left
                        _leftSideTop = _prevFirst;
                        _leftSideBottom = _prevSecond;
                        break;

                        case 2: // Top
                        _topSideLeft = _prevFirst;
                        _topSideRight = _prevSecond;
                        break;

                        case 3: // Right
                        _rightSideTop = _prevFirst;
                        _rightSideBottom = _prevSecond;
                        break;

                        case 4: // Bottom
                        _bottomSideLeft = _prevFirst;
                        _bottomSideRight = _prevSecond;
                        break;
                    }
                        
                } else {

                    GLOBALS.Level.Resize(
                        left, 
                        top, 
                        right, 
                        bottom,
                        _fillLayer1 ? new Geo(GeoType.Solid) : new Geo(),
                        _fillLayer2 ? new Geo(GeoType.Solid) : new Geo(),
                        _fillLayer3 ? new Geo(GeoType.Solid) : new Geo()
                    );

                    ResizeLightMap(
                        _levelOrigin - new Vector2(_leftSideTop.X, _topSideLeft.Y), 
                        GLOBALS.Level.Width, 
                        GLOBALS.Level.Height
                    );

                    _levelOrigin = _leftSideTop;

                    UnloadRenderTexture(GLOBALS.Textures.GeneralLevel);
                    
                    GLOBALS.Textures.GeneralLevel = LoadRenderTexture(GLOBALS.Level.Width * 20, GLOBALS.Level.Height * 20);
                }

                _shouldRedrawLevel = true;

                _resizeLock = 0;
            }
        }
        #endregion

        //
        // BeginDrawing();

        ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme 
            ? new Color(100, 100, 100, 255) 
            :  Color.Gray
        );

        //

        if (_shouldRedrawLevel) {

            Printers.DrawLevelIntoBuffer(GLOBALS.Textures.GeneralLevel, new Printers.DrawLevelParams {
                CurrentLayer = GLOBALS.Layer,

                Water = true,
                WaterAtFront = GLOBALS.Level.WaterAtFront,
                WaterOpacity = GLOBALS.Settings.GeneralSettings.WaterOpacity,

                TileDrawMode = GLOBALS.Settings.GeneralSettings.DrawTileMode,
                PropDrawMode = GLOBALS.Settings.GeneralSettings.DrawPropMode,
                Palette = GLOBALS.SelectedPalette,

                Padding = true,

                MaterialWhiteSpace = GLOBALS.Settings.GeneralSettings.MaterialWhiteSpace
            });

            _shouldRedrawLevel = false;
        }

        BeginMode2D(_camera);
        {
            var shader = GLOBALS.Shaders.VFlip;

            BeginShaderMode(shader);
            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), GLOBALS.Textures.GeneralLevel.Texture);
            DrawTextureV(GLOBALS.Textures.GeneralLevel.Texture, _levelOrigin, Color.White);
            EndShaderMode();

            if (_grid) Printers.DrawGrid(
                _leftSideTop, 
                (int)(_rightSideTop.X - _leftSideTop.X)/20, 
                (int)(_bottomSideLeft.Y - _topSideLeft.Y)/20, 
                20
            );

            //

            DrawRectangleLinesEx(
                new Rectangle(_leftSideTop.X, _topSideLeft.Y, _topSideRight.X - _topSideLeft.X, _leftSideBottom.Y - _leftSideTop.Y),
                4,
                Color.Red
            );

            switch (_resizing) {
                case Resizing.Left:
                DrawLineEx(_leftSideTop, _leftSideBottom, 4, Color.Orange);

                if (_resizeLock == 1) {
                    var text = $"{(int)(-_leftSideTop.X)/20}";
                    
                    if (GLOBALS.Font is not null) {
                        DrawTextEx(GLOBALS.Font!.Value, text, new Vector2(_leftSideTop.X - (_leftSideTop.X + MeasureText(text, 20))/2, (_bottomSideLeft.Y - _topSideLeft.Y)/2), 20, 0, Color.White);
                    } else {
                        DrawText(text, (int)(_leftSideTop.X - (_leftSideTop.X + MeasureText(text, 20))/2), (int)((_bottomSideLeft.Y - _topSideLeft.Y)/2), 20, Color.White);
                    }
                }
                break;

                case Resizing.Top:
                DrawLineEx(_topSideLeft, _topSideRight, 4, Color.Orange);

                if (_topSideLeft.Y == 2) {
                    var text = $"{(int)(-_topSideLeft.Y)/20}";
                    if (GLOBALS.Font is not null) {
                        DrawTextEx(GLOBALS.Font!.Value, text, new Vector2((_rightSideTop.X - _leftSideTop.X)/2, _topSideLeft.Y - (_topSideLeft.Y + MeasureText(text, 20))/2), 20, 0, Color.White);
                    } else {
                        DrawText(text, (int)((_rightSideTop.X - _leftSideTop.X)/2), (int)(_topSideLeft.Y - (_topSideLeft.Y + MeasureText(text, 20))/2), 20, Color.White);
                    }
                }
                break;

                case Resizing.Right:
                {
                    DrawLineEx(_rightSideTop, _rightSideBottom, 4, Color.Orange);
                    
                    var delta = (int)(GLOBALS.Level.Width * 20 - _rightSideTop.X);

                    var text = $"{-delta/20}";

                    if (_resizeLock == 3) {

                        if (GLOBALS.Font is not null) {
                            DrawTextEx(GLOBALS.Font!.Value, text, new Vector2(GLOBALS.Level.Width * 20 - (delta + MeasureText(text, 20))/2, (_bottomSideLeft.Y - _topSideLeft.Y)/2), 20, 0, Color.White);
                        } else {
                            DrawText(text, (int)(GLOBALS.Level.Width * 20 - (delta + MeasureText(text, 20))/2), (int)((_bottomSideLeft.Y - _topSideLeft.Y)/2), 20, Color.White);
                        }
                    }
                }
                break;

                case Resizing.Bottom:
                {
                    DrawLineEx(_bottomSideLeft, _bottomSideRight, 4, Color.Orange);

                    var delta = (int)(GLOBALS.Level.Height * 20 - _bottomSideLeft.Y);

                    var text = $"{-delta/20}";

                    if (_resizeLock == 4) {

                        if (GLOBALS.Font is not null) {
                            DrawTextEx(GLOBALS.Font!.Value, text, new Vector2((_rightSideBottom.X - _leftSideBottom.X)/2, GLOBALS.Level.Height * 20 - (delta + MeasureText(text, 20))/2), 20, 0, Color.White);
                        } else {
                            DrawText(text, (int)((_rightSideBottom.X - _leftSideBottom.X)/2), (int)(GLOBALS.Level.Height * 20 - (delta + MeasureText(text, 20))/2), 20, Color.White);
                        }
                    }
                }
                break;
            }
        }
        EndMode2D();
        

        //

        rlImGui.Begin();
        
        ImGui.DockSpaceOverViewport(ImGui.GetWindowDockID(), ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);

        // Navigation bar
                
        if (GLOBALS.Settings.GeneralSettings.Navbar) GLOBALS.NavSignal = Printers.ImGui.Nav(out _);

        var mainWinOpened = ImGui.Begin("Resizing##DimensionsWindow");

        var mainWinPos = ImGui.GetWindowPos();
        var mainWinSpace = ImGui.GetWindowSize();

        _isMainWinHovered = CheckCollisionPointRec(GetMousePosition(), new(mainWinPos.X - 5, mainWinPos.Y-5, mainWinSpace.X + 10, mainWinSpace.Y+10));

        if (mainWinOpened)
        {
            // ImGui.SetWindowSize(new Vector2(GetScreenWidth() - 80, GetScreenHeight() - 80));
            // ImGui.SetWindowPos(new Vector2(40, 40));

            var col1Space = ImGui.GetContentRegionAvail();
            
            ImGui.SetNextItemWidth(200);
            ImGui.InputInt("Width", ref _matrixWidthValue);

            _isInputBusy = _isInputBusy || ImGui.IsItemActive();
            
            ImGui.SetNextItemWidth(200);
            ImGui.InputInt("Height", ref _matrixHeightValue);

            _isInputBusy = _isInputBusy || ImGui.IsItemActive();
            
            Utils.Restrict(ref _matrixWidthValue, 1);
            Utils.Restrict(ref _matrixHeightValue, 1);
            
            ImGui.SeparatorText("Padding");

            ImGui.SetNextItemWidth(200);
            ImGui.InputInt("Left", ref _leftPadding);

            _isInputBusy = _isInputBusy || ImGui.IsItemActive();
            
            ImGui.SetNextItemWidth(200);
            ImGui.InputInt("Top", ref _topPadding);

            _isInputBusy = _isInputBusy || ImGui.IsItemActive();
            
            ImGui.SetNextItemWidth(200);
            ImGui.InputInt("Right", ref _rightPadding);

            _isInputBusy = _isInputBusy || ImGui.IsItemActive();
            
            ImGui.SetNextItemWidth(200);
            ImGui.InputInt("Bottom", ref _bottomPadding);

            _isInputBusy = _isInputBusy || ImGui.IsItemActive();
            
            Utils.Restrict(ref _leftPadding, 0);
            Utils.Restrict(ref _topPadding, 0);
            Utils.Restrict(ref _rightPadding, 0);
            Utils.Restrict(ref _bottomPadding, 0);
            
            ImGui.Spacing();
            
            // var isBecomingLarger = _matrixWidthValue > GLOBALS.Level.Width || _matrixHeightValue > GLOBALS.Level.Height;
            
            // if (isBecomingLarger)
            // {
            //     ImGui.SeparatorText("Fill Extra Space");
                        
            //     ImGui.Checkbox("Fill Layer 1", ref _fillLayer1);
            //     ImGui.Checkbox("Fill Layer 2", ref _fillLayer2);
            //     ImGui.Checkbox("Fill Layer 3", ref _fillLayer3);
                        
            //     ImGui.Spacing();
            // }

            if (ImGui.Button("Reset Values", col1Space with { Y = 20 }))
            {
                _leftPadding = GLOBALS.Level.Padding.left;
                _rightPadding = GLOBALS.Level.Padding.right;
                _topPadding = GLOBALS.Level.Padding.top;
                _bottomPadding = GLOBALS.Level.Padding.bottom;

                _matrixWidthValue = GLOBALS.Level.Width;
                _matrixHeightValue = GLOBALS.Level.Height;
            }
            
            if (ImGui.Button("Resize", col1Space with { Y = 20 }))
            {
                _shouldRedrawLevel = true;

                Logger.Debug("page 6: Ok button clicked");

                Logger.Debug("resize flag detected");

                if (
                    GLOBALS.Level.Height != _matrixHeightValue ||
                    GLOBALS.Level.Width != _matrixWidthValue)
                {
                    Logger.Debug("dimensions don't match; resizing");

                    Logger.Debug("resizing geometry matrix");
                    
                    // I know this can be simplified, but I'm keeping it in case 
                    // it becomes useful in the future
                    GLOBALS.Level.Resize(
                        _matrixWidthValue,
                        _matrixHeightValue,
                        (_leftPadding, _topPadding, _rightPadding, _bottomPadding),
                        [
                            _fillLayer1 ? GeoType.Solid : GeoType.Air,
                            _fillLayer2 ? GeoType.Solid : GeoType.Air,
                            _fillLayer3 ? GeoType.Solid : GeoType.Air
                        ]
                    );

                    Logger.Debug("resizing light map");

                    ResizeLightMap(Vector2.Zero, _matrixWidthValue, _matrixHeightValue);
                    
                    UnloadRenderTexture(GLOBALS.Textures.GeneralLevel);
                    GLOBALS.Textures.GeneralLevel =
                        LoadRenderTexture(GLOBALS.Level.Width * 20, GLOBALS.Level.Height * 20);
                }
                
                GLOBALS.Level.Padding = (_leftPadding, _topPadding, _rightPadding, _bottomPadding);
            }
            
            ImGui.End();
        }
        
        // Options

        var optionsWinOpened = ImGui.Begin("Options##LevelDimensionsOptionsWin");

        var optionsPos = ImGui.GetWindowPos();
        var optionsSpace = ImGui.GetWindowSize();

        _isOptionsWinHovered = CheckCollisionPointRec(GetMousePosition(), new(optionsPos.X - 5, optionsPos.Y-5, optionsSpace.X + 10, optionsSpace.Y+10));
        
        if (optionsWinOpened) {
            ImGui.Checkbox("Grid", ref _grid);

            ImGui.Spacing();

            ImGui.Checkbox("Fill Layer 1", ref _fillLayer1);
            ImGui.Checkbox("Fill Layer 2", ref _fillLayer2);
            ImGui.Checkbox("Fill Layer 3", ref _fillLayer3);

            ImGui.End();
        }
        
        rlImGui.End();

        // EndDrawing();

        // F3

        Printers.Debug.EnqueueF3(new(_leftPadding) { Name = "LP", SameLine = true });
        Printers.Debug.EnqueueF3(new(_topPadding) { Name = "TP", SameLine = true });
        Printers.Debug.EnqueueF3(new(_rightPadding) { Name = "RP", SameLine = true });
        Printers.Debug.EnqueueF3(new(_bottomPadding) { Name = "BP" });

        Printers.Debug.EnqueueF3(null);

        Printers.Debug.EnqueueF3(new(_fillLayer1) { Name = "Fill Layer 1" });
        Printers.Debug.EnqueueF3(new(_fillLayer2) { Name = "Fill Layer 2" });
        Printers.Debug.EnqueueF3(new(_fillLayer3) { Name = "Fill Layer 3" });

        Printers.Debug.EnqueueF3(null);

        Printers.Debug.EnqueueF3(new(_resizing) { Name = "Resizing" });

        Printers.Debug.EnqueueF3(new(GLOBALS.Level.Width) { Name = "H", SameLine = true });
        Printers.Debug.EnqueueF3(new(_matrixWidthValue) { Name = "MH" });

        Printers.Debug.EnqueueF3(new(GLOBALS.Level.Height) { Name = "V", SameLine = true });
        Printers.Debug.EnqueueF3(new(_matrixHeightValue) { Name = "MV" });

        Printers.Debug.EnqueueF3(new(_leftSideTop) { Name = "LT", SameLine = true });
        Printers.Debug.EnqueueF3(new(_leftSideBottom) { Name = "LB" });

        Printers.Debug.EnqueueF3(new(_topSideLeft) { Name = "TL", SameLine = true });
        Printers.Debug.EnqueueF3(new(_topSideRight) { Name = "TR" });

        Printers.Debug.EnqueueF3(new(_rightSideTop) { Name = "RT", SameLine = true });
        Printers.Debug.EnqueueF3(new(_rightSideBottom) { Name = "RB" });

        Printers.Debug.EnqueueF3(new(_bottomSideLeft) { Name = "BL", SameLine = true });
        Printers.Debug.EnqueueF3(new(_bottomSideRight) { Name = "BR" });
    }
}
