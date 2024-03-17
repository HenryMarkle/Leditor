using System.Numerics;
using ImGuiNET;
using rlImGui_cs;
using static Raylib_cs.Raylib;

namespace Leditor.Pages;

internal class LightEditorPage : EditorPage
{
    private Camera2D _camera = new() { Zoom = 0.5f, Target = new(-500, -200) };
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

    public override void Draw()
    {
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera with { Target = GLOBALS.Camera.Target + new Vector2(300, 300)};
        var mouse = GetMousePosition();
        
        var indicatorOrigin = new Vector2(GetScreenWidth() - 100, GetScreenHeight() - 100);

        var indicatorPoint = new Vector2(
            indicatorOrigin.X + (float)((15 + GLOBALS.Level.LightFlatness * 7) * Math.Cos(float.DegreesToRadians(GLOBALS.Level.LightAngle + 90))),
            indicatorOrigin.Y + (float)((15 + GLOBALS.Level.LightFlatness * 7) * Math.Sin(float.DegreesToRadians(GLOBALS.Level.LightAngle + 90)))
        );
        
        var indHovered = CheckCollisionPointCircle(mouse, indicatorPoint, 10f);

        if (IsMouseButtonReleased(MouseButton.Left) && _isDraggingIndicator)
            _isDraggingIndicator = false;

        if (indHovered && IsMouseButtonDown(MouseButton.Left)) _isDraggingIndicator = true;

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
        
        var ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
        var shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
        var alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);

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
            delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
            _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
        }

        // handle zoom
        var wheel2 = GetMouseWheelMove();
        if (wheel2 != 0 && canPaint)
        {
            var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
            _camera.Offset = GetMousePosition();
            _camera.Target = mouseWorldPosition;
            _camera.Zoom += wheel2 * GLOBALS.ZoomIncrement;
            if (_camera.Zoom < GLOBALS.ZoomIncrement) _camera.Zoom = GLOBALS.ZoomIncrement;
        }

        // update light brush

        {
            var texture = GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex];
            var lightMouse = GetScreenToWorld2D(GetMousePosition(), _camera);

            _lightBrushSource = new(0, 0, texture.Width, texture.Height);
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

        if (IsKeyPressed(KeyboardKey.R)) _shading = !_shading;

        BeginDrawing();
        {
            ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme 
                ? Color.Black 
                : GLOBALS.Settings.LightEditor.Background);

            BeginMode2D(_camera);
            {
                DrawRectangle(
                    0, 0,
                    GLOBALS.Level.Width * GLOBALS.Scale + 300,
                    GLOBALS.Level.Height * GLOBALS.Scale + 300,
                    GLOBALS.Settings.GeneralSettings.DarkTheme ? new Color(200, 0, 0, 255) : Color.White
                );

                if (GLOBALS.Settings.GeneralSettings.DarkTheme)
                {
                    DrawRectangleLinesEx(
                        new Rectangle(-2, -2, GLOBALS.Level.Width*GLOBALS.Scale+304, GLOBALS.Level.Height*GLOBALS.Scale+304),
                        2f,
                        Color.White);
                }

                Printers.DrawGeoLayer(2, GLOBALS.Scale, false, Color.Black with { A = 150 }, new Vector2(300, 300));
                
                if (_showTiles) Printers.DrawTileLayer(2, GLOBALS.Scale, false, _tilePreview, _tintedTileTextures, new Vector2(300, 300));
                
                Printers.DrawGeoLayer(1, GLOBALS.Scale, false, Color.Black with { A = 150 }, new Vector2(300, 300));
                
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

                Printers.DrawGeoLayer(0, GLOBALS.Scale, false, Color.Black, new Vector2(300, 300));
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
                    GLOBALS.Textures.LightMap.Texture,
                    new Rectangle(0, 0, GLOBALS.Textures.LightMap.Texture.Width, -GLOBALS.Textures.LightMap.Texture.Height),
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
                            Color.White
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
                            Color.White
                        );
                        EndShaderMode();
                    }
                }
            }
            EndMode2D();


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
                var radius = (int) Raymath.Vector2Distance(mouse, indicatorOrigin);

                var newAngle = (int)float.RadiansToDegrees(Raymath.Vector2Angle(
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
                Color.Red
            );
            
            #endregion
            
            rlImGui.Begin();
            
            ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
            
            // Brushes Window

            var menuOpened = ImGui.Begin("Brushes##LightBrushesWindow");
            
            var menuPos = ImGui.GetWindowPos();
            var menuWinSpace = ImGui.GetWindowSize();

            if (CheckCollisionPointRec(GetMousePosition(), new(menuPos.X - 5, menuPos.Y-5, menuWinSpace.X + 10, menuWinSpace.Y+10)))
            {
                _isBrushesWinHovered = true;

                if (IsMouseButtonDown(MouseButton.Left)) _isBrushesWinDragged = true;
            }
            else
            {
                _isBrushesWinHovered = false;
            }

            if (IsMouseButtonReleased(MouseButton.Left) && _isBrushesWinDragged) _isBrushesWinDragged = false;
            
            if (menuOpened)
            {
                var availableSpace = ImGui.GetContentRegionAvail();
                
                if (ImGui.BeginListBox("##LightBrushes", availableSpace))
                {
                    for (var index = 0; index < GLOBALS.Textures.LightBrushes.Length; index++)
                    {
                        
                        var selected = ImGui.ImageButton(
                            $"Brush {index}",
                            new IntPtr(GLOBALS.Textures.LightBrushes[index].Id), 
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
                X = navWindowRect.X - 5, Width = navWindowRect.Width + 10
            });
                
            if (_isNavigationWinHovered && IsMouseButtonDown(MouseButton.Left))
            {
                _isNavigationWinDragged = true;
            }
            else if (_isNavigationWinDragged && IsMouseButtonReleased(MouseButton.Left))
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
                        X = shortcutWindowRect.X - 5, Width = shortcutWindowRect.Width + 10
                    }
                );

                if (_isShortcutsWinHovered && IsMouseButtonDown(MouseButton.Left))
                {
                    _isShortcutsWinDragged = true;
                }
                else if (_isShortcutsWinDragged && IsMouseButtonReleased(MouseButton.Left))
                {
                    _isShortcutsWinDragged = false;
                }
            }
            
            rlImGui.End();
        }
        EndDrawing();
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera with { Target = _camera.Target - new Vector2(300, 300)};
    }
}
