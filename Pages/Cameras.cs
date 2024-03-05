using System.Numerics;
using Microsoft.Toolkit.HighPerformance;
using rlImGui_cs;
using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class CamerasEditorPage(Serilog.Core.Logger logger, Camera2D? camera = null) : IPage
{
    private readonly Serilog.Core.Logger _logger = logger;

    Camera2D _camera = camera ?? new() { zoom = 0.8f, target = new(-100, -100) };
    bool clickTracker = false;
    int draggedCamera = -1;
    private readonly CameraShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.CameraEditor;
    
    private bool _isShortcutsWinHovered;
    private bool _isShortcutsWinDragged;
    
    private bool _isNavigationWinHovered;
    private bool _isNavigationWinDragged;

    public void Draw()
    {
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera;
        
        GLOBALS.PreviousPage = 4;

        #region CamerasInputHandlers

        var ctrl = IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);
        var shift = IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
        var alt = IsKeyDown(KeyboardKey.KEY_LEFT_ALT);

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
        //if (Raylib.IsKeyReleased(KeyboardKey.KEY_FOUR)) page = 4;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToLightEditor.Check(ctrl, shift, alt))
        {
            GLOBALS.Page = 5;
        }
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
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToPropsEditor.Check(ctrl, shift, alt))
        {
            GLOBALS.Page = 8;
        }
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToSettingsPage.Check(ctrl, shift, alt))
        {
            GLOBALS.Page = 9;
        }

        // handle mouse drag
        if (_shortcuts.DragLevel.Check(ctrl, shift, alt, true))
        {
            Vector2 delta = Raylib.GetMouseDelta();
            delta = RayMath.Vector2Scale(delta, -1.0f / _camera.zoom);
            _camera.target = RayMath.Vector2Add(_camera.target, delta);
        }

        // handle zoom
        var cameraWheel = Raylib.GetMouseWheelMove();
        if (cameraWheel != 0)
        {
            var mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), _camera);
            _camera.offset = Raylib.GetMousePosition();
            _camera.target = mouseWorldPosition;
            _camera.zoom += cameraWheel * GLOBALS.ZoomIncrement;
            if (_camera.zoom < GLOBALS.ZoomIncrement) _camera.zoom = GLOBALS.ZoomIncrement;
        }

        if (IsMouseButtonReleased(_shortcuts.DragLevel.Button) || IsMouseButtonReleased(_shortcuts.ManipulateCamera.Button) && clickTracker)
        {
            clickTracker = false;
        }

        if (_shortcuts.ManipulateCamera.Check(ctrl, shift, alt, true) && !clickTracker && draggedCamera != -1)
        {
            var pos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), _camera);
            GLOBALS.Level.Cameras[draggedCamera].Coords = new Vector2(pos.X - (72 * GLOBALS.Scale - 40) / 2f, pos.Y - (43 * GLOBALS.Scale - 60) / 2f);
            draggedCamera = -1;
            clickTracker = true;
        }

        if (_shortcuts.CreateCamera.Check(ctrl, shift, alt) && draggedCamera == -1)
        {
            var pos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), _camera);
            GLOBALS.Level.Cameras = [.. GLOBALS.Level.Cameras, new() { Coords = new Vector2(0, 0), Quad = new(new(), new(), new(), new()) }];
            GLOBALS.CamQuadLocks = [..GLOBALS.CamQuadLocks, 0];
            draggedCamera = GLOBALS.Level.Cameras.Count - 1;
        }

        if (_shortcuts.DeleteCamera.Check(ctrl, shift, alt) && draggedCamera != -1)
        {
            GLOBALS.Level.Cameras.RemoveAt(draggedCamera);
            GLOBALS.CamQuadLocks = GLOBALS.CamQuadLocks[..^1];
            draggedCamera = -1;
        }

        if (_shortcuts.CreateAndDeleteCamera.Check(ctrl, shift, alt))
        {
            if (draggedCamera == -1)
            {
                var pos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), _camera);
                GLOBALS.Level.Cameras = [.. GLOBALS.Level.Cameras, new() { Coords = new Vector2(0, 0), Quad = new(new(), new(), new(), new()) }];
                GLOBALS.CamQuadLocks = [..GLOBALS.CamQuadLocks, 0];
                draggedCamera = GLOBALS.Level.Cameras.Count - 1;
            }
            else
            {
                GLOBALS.Level.Cameras.RemoveAt(draggedCamera);
                GLOBALS.CamQuadLocks = GLOBALS.CamQuadLocks[..^1];
                draggedCamera = -1;
            }
        }

        #endregion

        BeginDrawing();
        {
            ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme ? BLACK : new(170, 170, 170, 255));

            BeginMode2D(_camera);
            {
                
                DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.Scale, GLOBALS.Level.Height * GLOBALS.Scale,
                    GLOBALS.Settings.GeneralSettings.DarkTheme
                        ? new Color(50, 50, 50, 255)
                        : WHITE);

                #region CamerasLevelBackground

                Printers.DrawGeoLayer(2, GLOBALS.Scale, false, GLOBALS.Settings.GeneralSettings.DarkTheme ? new Color(150, 150, 150, 255) : BLACK with { a = 150 });
                Printers.DrawGeoLayer(1, GLOBALS.Scale, false, GLOBALS.Settings.GeneralSettings.DarkTheme ? new Color(100, 100, 100, 255) : BLACK with { a = 150 });
                    
                if (!GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1)
                {
                    DrawRectangle(
                        (-1) * GLOBALS.Scale,
                        (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel) * GLOBALS.Scale,
                        (GLOBALS.Level.Width + 2) * GLOBALS.Scale,
                        GLOBALS.Level.WaterLevel * GLOBALS.Scale,
                        new(0, 0, 255, 110)
                    );
                }
                    
                Printers.DrawGeoLayer(0, GLOBALS.Scale, false, BLACK);

                if (GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1)
                {
                    DrawRectangle(
                        (-1) * GLOBALS.Scale,
                        (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel) * GLOBALS.Scale,
                        (GLOBALS.Level.Width + 2) * GLOBALS.Scale,
                        GLOBALS.Level.WaterLevel * GLOBALS.Scale,
                        new(0, 0, 255, 110)
                    );
                }

                #endregion

                foreach (var (index, cam) in GLOBALS.Level.Cameras.Select((camera, index) => (index, camera)))
                {
                    if (index == draggedCamera)
                    {
                        var pos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), _camera);
                        Printers.DrawCameraSprite(new(pos.X - (72 * GLOBALS.Scale - 40) / 2, pos.Y - (43 * GLOBALS.Scale - 60) / 2), cam.Quad, _camera, index);
                        continue;
                    }
                    var (clicked, hovered) = Printers.DrawCameraSprite(cam.Coords, cam.Quad, _camera, index);

                    if (clicked && !clickTracker)
                    {
                        draggedCamera = index;
                        clickTracker = true;
                    }
                }

                DrawRectangleLinesEx(
                    GLOBALS.Level.Border,
                    4f,
                    new(200, 66, 245, 255)
                );
                
                if (GLOBALS.Settings.GeneralSettings.DarkTheme)
                {
                    DrawRectangleLines(0, 0, GLOBALS.Level.Width*GLOBALS.Scale, GLOBALS.Level.Height*GLOBALS.Scale, WHITE);
                }
            }
            EndMode2D();

            #region CameraEditorUI

            rlImGui.Begin();
            
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
                var shortcutWindowRect = Printers.ImGui.ShortcutsWindow(GLOBALS.Settings.Shortcuts.CameraEditor);

                _isShortcutsWinHovered = CheckCollisionPointRec(
                    GetMousePosition(), 
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
            #endregion
        }
        EndDrawing();
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera;
    }
}
