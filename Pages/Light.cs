using System.Numerics;
using ImGuiNET;
using rlImGui_cs;
using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class LightEditorPage(Serilog.Core.Logger logger, Camera2D? camera = null) : IPage
{
    readonly Serilog.Core.Logger _logger = logger;

    private Camera2D _camera = camera ?? new() { zoom = 0.5f, target = new(-500, -200) };
    private int _lightBrushTextureIndex;
    private float _lightBrushWidth = 200;
    private float _lightBrushHeight = 200;
    private int _lightBrushTexturePage;
    private float _lightBrushRotation;
    private bool _eraseShadow;
    private bool _slowGrowth = true;
    private bool _shading = true;
    private bool _showTiles;
    private bool _tilePreview = true;
    private bool _tintedTileTextures = true;

    private bool _isDraggingIndicator;

    private const float InitialGrowthFactor = 0.01f;
    private float _growthFactor = InitialGrowthFactor;

    private void ResetGrowthFactor()
    {
        _growthFactor = InitialGrowthFactor;
    }
    private void IncreaseGrowthFactor()
    {
        _growthFactor += 0.05f;
    }
    
    private Rectangle _lightBrushSource;
    private Rectangle _lightBrushDest;
    private Vector2 _lightBrushOrigin;

    readonly byte[] _lightBrushMenuPanelBytes = "Brushes"u8.ToArray();

    private readonly LightShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.LightEditor;
    
    private bool _isShortcutsWinHovered;
    private bool _isShortcutsWinDragged;
    
    private bool _isNavigationWinHovered;
    private bool _isNavigationWinDragged;
    
    private bool _isBrushesWinHovered;
    private bool _isBrushesWinDragged;

    public void Draw()
    {
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera with { target = GLOBALS.Camera.target + new Vector2(300, 300)};
        var mouse = GetMousePosition();
        
        var indicatorOrigin = new Vector2(GetScreenWidth() - 100, GetScreenHeight() - 100);

        var indicatorPoint = new Vector2(
            indicatorOrigin.X + (float)((15 + GLOBALS.Level.LightFlatness * 7) * Math.Cos(float.DegreesToRadians(GLOBALS.Level.LightAngle + 90))),
            indicatorOrigin.Y + (float)((15 + GLOBALS.Level.LightFlatness * 7) * Math.Sin(float.DegreesToRadians(GLOBALS.Level.LightAngle + 90)))
        );
        
        var indHovered = CheckCollisionPointCircle(mouse, indicatorPoint, 10f);

        if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && _isDraggingIndicator)
            _isDraggingIndicator = false;

        if (indHovered && IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) _isDraggingIndicator = true;

        var panelHeight = GetScreenHeight() - 100;
        var brushPanel = new Rectangle(10, 50, 120, panelHeight);

        var canPaint = !_isBrushesWinHovered && 
                       !_isBrushesWinDragged && 
                       !_isShortcutsWinHovered && 
                       !_isShortcutsWinDragged && 
                       !_isNavigationWinHovered &&
                       !_isNavigationWinDragged &&
                       !CheckCollisionPointRec(mouse, brushPanel) && !indHovered && 
                       !_isDraggingIndicator;
        
        var ctrl = IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);
        var shift = IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
        var alt = IsKeyDown(KeyboardKey.KEY_LEFT_ALT);

        GLOBALS.PreviousPage = 5;

        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToMainPage.Check(ctrl, shift, alt))
        {
            GLOBALS.Page = 1;
        }
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToGeometryEditor.Check(ctrl, shift, alt))
        {
            GLOBALS.Page = 2;
        }
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToTileEditor.Check(ctrl, shift, alt))
        {
            GLOBALS.Page = 3;
        }
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToCameraEditor.Check(ctrl, shift, alt))
        {
            GLOBALS.Page = 4;
        }
        // if (Raylib.IsKeyReleased(KeyboardKey.KEY_FIVE)) GLOBALS.Page = 5;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToDimensionsEditor.Check(ctrl, shift, alt))
        {
            GLOBALS.ResizeFlag = true;
            GLOBALS.NewFlag = false;
            GLOBALS.Page = 6;
        }
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToEffectsEditor.Check(ctrl, shift, alt))
        {
            GLOBALS.Page = 7;
        }
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToPropsEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 8;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToSettingsPage.Check(ctrl, shift, alt)) GLOBALS.Page = 9;



        if (_shortcuts.IncreaseFlatness.Check(ctrl, shift, alt, true) && GLOBALS.Level.LightFlatness < 10) GLOBALS.Level.LightFlatness++;
        if (_shortcuts.DecreaseFlatness.Check(ctrl, shift, alt, true) && GLOBALS.Level.LightFlatness > 1) GLOBALS.Level.LightFlatness--;

        const int textureSize = 100;


        var pageSize = (panelHeight - 100) / textureSize;

        if (_shortcuts.IncreaseAngle.Check(ctrl, shift, alt, true))
        {
            GLOBALS.Level.LightAngle--;

            if (GLOBALS.Level.LightAngle == 0) GLOBALS.Level.LightAngle = 360;
        }
        if (_shortcuts.DecreaseAngle.Check(ctrl, shift, alt, true))
        {
            GLOBALS.Level.LightAngle = ++GLOBALS.Level.LightAngle % 360;
        }

        // handle mouse drag
        if (_shortcuts.DragLevel.Check(ctrl, shift, alt, true) || _shortcuts.DragLevelAlt.Check(ctrl, shift, alt, true))
        {
            var delta = GetMouseDelta();
            delta = RayMath.Vector2Scale(delta, -1.0f / _camera.zoom);
            _camera.target = RayMath.Vector2Add(_camera.target, delta);
        }

        // handle zoom
        var wheel2 = GetMouseWheelMove();
        if (wheel2 != 0 && canPaint)
        {
            var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
            _camera.offset = GetMousePosition();
            _camera.target = mouseWorldPosition;
            _camera.zoom += wheel2 * GLOBALS.ZoomIncrement;
            if (_camera.zoom < GLOBALS.ZoomIncrement) _camera.zoom = GLOBALS.ZoomIncrement;
        }

        // update light brush

        {
            var texture = GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex];
            var lightMouse = GetScreenToWorld2D(GetMousePosition(), _camera);

            _lightBrushSource = new(0, 0, texture.width, texture.height);
            _lightBrushDest = new(lightMouse.X, lightMouse.Y, _lightBrushWidth, _lightBrushHeight);
            _lightBrushOrigin = new(_lightBrushWidth / 2, _lightBrushHeight / 2);
        }

        if (_shortcuts.NextBrush.Check(ctrl, shift, alt))
        {
            _lightBrushTextureIndex = ++_lightBrushTextureIndex % GLOBALS.Textures.LightBrushes.Length;

            _lightBrushTexturePage = _lightBrushTextureIndex / pageSize;
        }
        else if (_shortcuts.PreviousBrush.Check(ctrl, shift, alt))
        {
            _lightBrushTextureIndex--;

            if (_lightBrushTextureIndex < 0) _lightBrushTextureIndex = GLOBALS.Textures.LightBrushes.Length - 1;

            _lightBrushTexturePage = _lightBrushTextureIndex / pageSize;
        }
        
        if (_shortcuts.RotateBrushCounterClockwise.Check(ctrl, shift, alt, true))
        {
            _lightBrushRotation -= 0.2f;
        }
        if (_shortcuts.RotateBrushClockwise.Check(ctrl, shift, alt, true))
        {
            
            _lightBrushRotation += 0.2f;
        }
        if (_shortcuts.StretchBrushVertically.Check(ctrl, shift, alt, true))
        {
            _lightBrushHeight += 2;
        }
        if (_shortcuts.SqueezeBrushVertically.Check(ctrl, shift, alt, true))
        {
            _lightBrushHeight -= 2;
        }
        if (_shortcuts.StretchBrushHorizontally.Check(ctrl, shift, alt, true))
        {
            _lightBrushWidth += 2;
        }
        if (_shortcuts.SqueezeBrushHorizontally.Check(ctrl, shift, alt, true))
        {
            
            _lightBrushWidth -= 2;
        }
        
        if (_shortcuts.FastRotateBrushCounterClockwise.Check(ctrl, shift, alt, true))
        {
            _lightBrushRotation -= 1 + _growthFactor;
            IncreaseGrowthFactor();
        }
        else if (_shortcuts.FastRotateBrushClockwise.Check(ctrl, shift, alt, true))
        {
            _lightBrushRotation += 1+_growthFactor;
            IncreaseGrowthFactor();
        }
        else if (_shortcuts.FastStretchBrushVertically.Check(ctrl, shift, alt, true))
        {
            _lightBrushHeight += 5+_growthFactor;
            IncreaseGrowthFactor();
        }
        else if (_shortcuts.FastSqueezeBrushVertically.Check(ctrl, shift, alt, true))
        {
            _lightBrushHeight -= 5+_growthFactor;
            IncreaseGrowthFactor();
        }
        else if (_shortcuts.FastStretchBrushHorizontally.Check(ctrl, shift, alt, true))
        {
            _lightBrushWidth += 5+_growthFactor;
            IncreaseGrowthFactor();
        }
        else if (_shortcuts.FastSqueezeBrushHorizontally.Check(ctrl, shift, alt, true))
        {
            _lightBrushWidth -= 5+_growthFactor;
            IncreaseGrowthFactor();
        }
        else ResetGrowthFactor();


        if (_shortcuts.ToggleTileVisibility.Check(ctrl, shift, alt)) _showTiles = !_showTiles;
        if (_shortcuts.ToggleTilePreview.Check(ctrl, shift, alt)) _tilePreview = !_tilePreview;
        if (_shortcuts.ToggleTintedTileTextures.Check(ctrl, shift, alt)) _tintedTileTextures = !_tintedTileTextures;

        //

        if (canPaint && (_shortcuts.Paint.Check(ctrl, shift, alt, true) || _shortcuts.PaintAlt.Check(ctrl, shift, alt, true)))
        {
            BeginTextureMode(GLOBALS.Textures.LightMap);
            {
                BeginShaderMode(GLOBALS.Shaders.ApplyShadowBrush);
                SetShaderValueTexture(GLOBALS.Shaders.ApplyShadowBrush, GetShaderLocation(GLOBALS.Shaders.ApplyShadowBrush, "inputTexture"), GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex]);
                DrawTexturePro(
                    GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex],
                    _lightBrushSource,
                    _lightBrushDest,
                    _lightBrushOrigin,
                    _lightBrushRotation,
                    new(0, 0, 0, 255)
                );
                EndShaderMode();
            }
            Raylib.EndTextureMode();
        }

        if (canPaint && (_shortcuts.Erase.Check(ctrl, shift, alt, true) ||
                         _shortcuts.EraseAlt.Check(ctrl, shift, alt, true)))
        {
            _eraseShadow = true;

            BeginTextureMode(GLOBALS.Textures.LightMap);
            {
                BeginShaderMode(GLOBALS.Shaders.ApplyLightBrush);
                SetShaderValueTexture(GLOBALS.Shaders.ApplyLightBrush,
                    GetShaderLocation(GLOBALS.Shaders.ApplyLightBrush, "inputTexture"),
                    GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex]);
                DrawTexturePro(
                    GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex],
                    _lightBrushSource,
                    _lightBrushDest,
                    _lightBrushOrigin,
                    _lightBrushRotation,
                    new Color(255, 255, 255, 255)
                );
                EndShaderMode();
            }
            EndTextureMode();
        }
        else _eraseShadow = false;

        if (IsKeyPressed(KeyboardKey.KEY_R)) _shading = !_shading;

        BeginDrawing();
        {
            ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme 
                ? BLACK 
                : GLOBALS.Settings.LightEditor.Background);

            BeginMode2D(_camera);
            {
                DrawRectangle(
                    0, 0,
                    GLOBALS.Level.Width * GLOBALS.Scale + 300,
                    GLOBALS.Level.Height * GLOBALS.Scale + 300,
                    GLOBALS.Settings.GeneralSettings.DarkTheme ? new Color(200, 0, 0, 255) : WHITE
                );

                if (GLOBALS.Settings.GeneralSettings.DarkTheme)
                {
                    DrawRectangleLinesEx(
                        new Rectangle(-2, -2, GLOBALS.Level.Width*GLOBALS.Scale+304, GLOBALS.Level.Height*GLOBALS.Scale+304),
                        2f,
                        WHITE);
                }

                Printers.DrawGeoLayer(2, GLOBALS.Scale, false, BLACK with { a = 150 }, new Vector2(300, 300));
                
                if (_showTiles) Printers.DrawTileLayer(2, GLOBALS.Scale, false, _tilePreview, _tintedTileTextures, new Vector2(300, 300));
                
                Printers.DrawGeoLayer(1, GLOBALS.Scale, false, BLACK with { a = 150 }, new Vector2(300, 300));
                
                if (_showTiles) Printers.DrawTileLayer(1, GLOBALS.Scale, false, _tilePreview, _tintedTileTextures, new Vector2(300, 300));

                if (!GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1)
                {
                    DrawRectangle(
                        (-1) * GLOBALS.Scale + 300,
                        (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel) * GLOBALS.Scale + 300,
                        (GLOBALS.Level.Width + 2) * GLOBALS.Scale,
                        GLOBALS.Level.WaterLevel * GLOBALS.Scale,
                        new(0, 0, 255, 255)
                    );
                }

                Printers.DrawGeoLayer(0, GLOBALS.Scale, false, BLACK, new Vector2(300, 300));
                if (_showTiles) Printers.DrawTileLayer(0, GLOBALS.Scale, false, _tilePreview, _tintedTileTextures, new Vector2(300, 300));

                if (GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1)
                {
                    DrawRectangle(
                        (-1) * GLOBALS.Scale,
                        (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel) * GLOBALS.Scale + 300,
                        (GLOBALS.Level.Width + 2) * GLOBALS.Scale + 300,
                        GLOBALS.Level.WaterLevel * GLOBALS.Scale,
                        new(0, 0, 255, 110)
                    );
                }
                
                // Lightmap

                DrawTextureRec(
                    GLOBALS.Textures.LightMap.texture,
                    new Rectangle(0, 0, GLOBALS.Textures.LightMap.texture.width, -GLOBALS.Textures.LightMap.texture.height),
                    new(0, 0),
                    new(255, 255, 255, 150)
                );

                // The brush

                if (!indHovered && !_isDraggingIndicator)
                {
                    if (_eraseShadow)
                    {
                        BeginShaderMode(GLOBALS.Shaders.LightBrush);
                        SetShaderValueTexture(GLOBALS.Shaders.LightBrush,
                            GetShaderLocation(GLOBALS.Shaders.LightBrush, "inputTexture"),
                            GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex]);

                        DrawTexturePro(
                            GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex],
                            _lightBrushSource,
                            _lightBrushDest,
                            _lightBrushOrigin,
                            _lightBrushRotation,
                            WHITE
                        );
                        EndShaderMode();
                    }
                    else
                    {
                        BeginShaderMode(GLOBALS.Shaders.ShadowBrush);
                        SetShaderValueTexture(GLOBALS.Shaders.ShadowBrush,
                            GetShaderLocation(GLOBALS.Shaders.ShadowBrush, "inputTexture"),
                            GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex]);

                        DrawTexturePro(
                            GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex],
                            _lightBrushSource,
                            _lightBrushDest,
                            _lightBrushOrigin,
                            _lightBrushRotation,
                            WHITE
                        );
                        EndShaderMode();
                    }
                }
            }
            EndMode2D();

            #region BrushMenu

            // {
            //     unsafe
            //     {
            //         fixed (byte* pt = _lightBrushMenuPanelBytes)
            //         {
            //             RayGui.GuiPanel(brushPanel, (sbyte*)pt);
            //         }
            //     }
            //
            //     var totalPages = GLOBALS.Textures.LightBrushes.Length / pageSize;
            //
            //     var currentPage = GLOBALS.Textures.LightBrushes
            //         .Select((texture, index) => (index, texture))
            //         .Skip(_lightBrushTexturePage * pageSize)
            //         .Take(pageSize)
            //         .Select((value, index) => (index, value));
            //
            //     // Brush menu
            //
            //     foreach (var (pageIndex, (index, texture)) in currentPage)
            //     {
            //         var textureRect = new Rectangle(25, (textureSize + 1) * pageIndex + 80 + 5, textureSize - 10,
            //             textureSize - 10);
            //
            //         var textureHovered = CheckCollisionPointRec(mouse, textureRect);
            //         
            //         BeginShaderMode(GLOBALS.Shaders.ApplyShadowBrush);
            //         SetShaderValueTexture(GLOBALS.Shaders.ApplyShadowBrush, GetShaderLocation(GLOBALS.Shaders.ApplyShadowBrush, "inputTexture"), texture);
            //         DrawTexturePro(
            //             texture,
            //             new(0, 0, texture.width, texture.height),
            //             textureRect,
            //             new(0, 0),
            //             0,
            //             BLACK
            //             );
            //         EndShaderMode();
            //
            //         if (index == _lightBrushTextureIndex) DrawRectangleLinesEx(
            //             new Rectangle(
            //                 20,
            //                 (textureSize + 1) * pageIndex + 80,
            //                 textureSize,
            //                 textureSize
            //             ),
            //             4.0f,
            //             BLUE
            //         );
            //
            //         if (textureHovered)
            //         {
            //             DrawRectangleLinesEx(
            //                 new Rectangle(
            //                     20,
            //                     (textureSize + 1) * pageIndex + 80,
            //                     textureSize,
            //                     textureSize
            //                 ),
            //                 4.0f,
            //                 BLUE with { a = 100 }
            //             );
            //
            //             if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) _lightBrushTextureIndex = index;
            //         }
            //     }
            //     
            //     if (_lightBrushTexturePage < GLOBALS.Textures.LightBrushes.Length / pageSize)
            //     {
            //         var downClicked = RayGui.GuiButton(
            //             new Rectangle(brushPanel.X + 5, brushPanel.Y + panelHeight - 60, 50, 30), 
            //             "Down"
            //         );
            //
            //         if (downClicked)
            //         {
            //             _lightBrushTextureIndex = (_lightBrushTextureIndex + pageSize);
            //
            //             if (_lightBrushTextureIndex >= GLOBALS.Textures.LightBrushes.Length)
            //                 _lightBrushTextureIndex = GLOBALS.Textures.LightBrushes.Length - 1;
            //                 
            //             _lightBrushTexturePage = _lightBrushTextureIndex / pageSize;
            //         }
            //     }
            //
            //     if (_lightBrushTexturePage > 0)
            //     {
            //         var upClicked = RayGui.GuiButton(
            //             new Rectangle(brushPanel.X + 59, brushPanel.Y + panelHeight - 60, 49, 30), 
            //             "Up"
            //         );
            //             
            //         if (upClicked)
            //         {
            //             _lightBrushTextureIndex -= pageSize;
            //             if (_lightBrushTextureIndex < 0) _lightBrushTextureIndex = 0;
            //                 
            //             _lightBrushTexturePage = _lightBrushTextureIndex / pageSize;
            //         }
            //     }
            //
            //     var indexText = $"{_lightBrushTexturePage + 1}/{totalPages+1}";
            //     
            //     DrawText(
            //         indexText,
            //         (brushPanel.X + brushPanel.width - MeasureText(indexText, 20))/2f, 
            //         brushPanel.Y + panelHeight - 23, 
            //         20, 
            //         BLACK
            //     );
            // }
            
            #endregion

            #region Indicator

            DrawCircleLines(
                GetScreenWidth() - 100,
                GetScreenHeight() - 100,
                50.0f,
                new(255, 0, 0, 255)
            );

            DrawCircleLines(
                GetScreenWidth() - 100,
                GetScreenHeight() - 100,
                15 + (GLOBALS.Level.LightFlatness * 7),
                new(255, 0, 0, 255)
            );

            if (_isDraggingIndicator)
            {
                var radius = (int) RayMath.Vector2Distance(mouse, indicatorOrigin);

                var newAngle = (int)float.RadiansToDegrees(RayMath.Vector2Angle(
                    indicatorOrigin with { Y = indicatorOrigin.Y + 1 } - indicatorOrigin,
                    mouse - indicatorOrigin
                    )
                );

                if (newAngle < 0) newAngle += 360;

                GLOBALS.Level.LightAngle = newAngle;

                if (radius > 85) radius = 85;
                if (radius < 1) radius = 1;

                GLOBALS.Level.LightFlatness = (radius - 15) / 7;
            }

            DrawCircleV(indicatorPoint,
                10.0f,
                RED
            );
            
            #endregion
            
            rlImGui.Begin();
            
            // Brushes Window

            if (ImGui.Begin("Brushes##LightBrushesWindow"))
            {
                var pos = ImGui.GetWindowPos();
                var winSpace = ImGui.GetWindowSize();

                if (CheckCollisionPointRec(GetMousePosition(), new(pos.X - 5, pos.Y-5, winSpace.X + 10, winSpace.Y+10)))
                {
                    _isBrushesWinHovered = true;

                    if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) _isBrushesWinDragged = true;
                }
                else
                {
                    _isBrushesWinHovered = false;
                }

                if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && _isBrushesWinDragged) _isBrushesWinDragged = false;
                
                //
                
                var availableSpace = ImGui.GetContentRegionAvail();
                
                if (ImGui.BeginListBox("##LightBrushes", availableSpace))
                {
                    for (var index = 0; index < GLOBALS.Textures.LightBrushes.Length; index++)
                    {
                        
                        var selected = ImGui.ImageButton(
                            $"Brush {index}",
                            new IntPtr(GLOBALS.Textures.LightBrushes[index].id), 
                            new Vector2(60, 60));

                        ImGui.SameLine();
                        
                        var selected2 = ImGui.Selectable(
                            $"#{index}", 
                            _lightBrushTextureIndex == index, 
                            ImGuiSelectableFlags.None | ImGuiSelectableFlags.AllowOverlap, 
                            new Vector2(60, 60)
                        );
                        
                        if (selected || selected2)
                        {
                            _lightBrushTextureIndex = index;
                        }
                    }
                    ImGui.End();
                }
                ImGui.End();
            }
            
            // Navigation
            
            var navWindowRect = Printers.ImGui.NavigationWindow();

            _isNavigationWinHovered = CheckCollisionPointRec(GetMousePosition(), navWindowRect with
            {
                X = navWindowRect.X - 5, width = navWindowRect.width + 10
            });
                
            if (_isNavigationWinHovered && IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                _isNavigationWinDragged = true;
            }
            else if (_isNavigationWinDragged && IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
            {
                _isNavigationWinDragged = false;
            }
            
            // Shortcuts window
            
            if (GLOBALS.Settings.GeneralSettings.ShortcutWindow)
            {
                var shortcutWindowRect = Printers.ImGui.ShortcutsWindow(GLOBALS.Settings.Shortcuts.TileEditor);

                _isShortcutsWinHovered = CheckCollisionPointRec(
                    mouse, 
                    shortcutWindowRect with
                    {
                        X = shortcutWindowRect.X - 5, width = shortcutWindowRect.width + 10
                    }
                );

                if (_isShortcutsWinHovered && IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    _isShortcutsWinDragged = true;
                }
                else if (_isShortcutsWinDragged && IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    _isShortcutsWinDragged = false;
                }
            }
            
            rlImGui.End();
        }
        EndDrawing();
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera with { target = _camera.target - new Vector2(300, 300)};
    }
}
