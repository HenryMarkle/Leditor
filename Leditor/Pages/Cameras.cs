﻿using System.Numerics;
using ImGuiNET;
using rlImGui_cs;
using static Raylib_cs.Raylib;

namespace Leditor;

internal class CamerasEditorPage(Serilog.Core.Logger logger, Camera2D? camera = null) : IPage
{
    private readonly Serilog.Core.Logger _logger = logger;

    Camera2D _camera = camera ?? new() { Zoom = 0.8f, Target = new(-100, -100) };
    bool clickTracker;
    int draggedCamera = -1;
    private readonly CameraShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.CameraEditor;
    
    private bool _isShortcutsWinHovered;
    private bool _isShortcutsWinDragged;
    
    private bool _isNavigationWinHovered;
    private bool _isNavigationWinDragged;

    private bool _alignment = GLOBALS.Settings.CameraSettings.Alignment;
    private bool _snap = GLOBALS.Settings.CameraSettings.Snap;

    public void Draw()
    {
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera;
        
        GLOBALS.PreviousPage = 4;
        
        var worldMouse = GetScreenToWorld2D(GetMousePosition(), _camera);

        #region CamerasInputHandlers

        var ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
        var shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
        var alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);

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

        var _xAligned = false;
        var _yAligned = false;

        // handle mouse drag
        if (_shortcuts.DragLevel.Check(ctrl, shift, alt, true))
        {
            var delta = GetMouseDelta();
            delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
            _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
        }

        // handle zoom
        var cameraWheel = GetMouseWheelMove();
        if (cameraWheel != 0)
        {
            var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
            _camera.Offset = GetMousePosition();
            _camera.Target = mouseWorldPosition;
            _camera.Zoom += cameraWheel * GLOBALS.ZoomIncrement;
            if (_camera.Zoom < GLOBALS.ZoomIncrement) _camera.Zoom = GLOBALS.ZoomIncrement;
        }

        if (IsMouseButtonReleased(_shortcuts.DragLevel.Button) || IsMouseButtonReleased(_shortcuts.ManipulateCamera.Button) && clickTracker)
        {
            clickTracker = false;
        }

        // Leave dragged camera
        if (_shortcuts.ManipulateCamera.Check(ctrl, shift, alt, true) && !clickTracker && draggedCamera != -1)
        {
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

        if (_shortcuts.CreateAndDeleteCamera.Check(ctrl, shift, alt) || _shortcuts.CreateAndDeleteCameraAlt.Check(ctrl, shift, alt))
        {
            if (draggedCamera == -1)
            {
                GLOBALS.Level.Cameras = [.. GLOBALS.Level.Cameras, new RenderCamera() { Coords = new Vector2(0, 0), Quad = new(new(), new(), new(), new()) }];
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
            ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.Black : new(170, 170, 170, 255));

            BeginMode2D(_camera);
            {
                
                DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.Scale, GLOBALS.Level.Height * GLOBALS.Scale,
                    GLOBALS.Settings.GeneralSettings.DarkTheme
                        ? new Color(50, 50, 50, 255)
                        : Color.White);

                #region CamerasLevelBackground

                Printers.DrawGeoLayer(2, GLOBALS.Scale, false, GLOBALS.Settings.GeneralSettings.DarkTheme ? new Color(150, 150, 150, 255) : Color.Black with { A = 150 });
                Printers.DrawGeoLayer(1, GLOBALS.Scale, false, GLOBALS.Settings.GeneralSettings.DarkTheme ? new Color(100, 100, 100, 255) : Color.Black with { A = 150 });
                    
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
                    
                Printers.DrawGeoLayer(0, GLOBALS.Scale, false, Color.Black);

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
                        var draggedOrigin = worldMouse - new Vector2(
                            (72 * GLOBALS.Scale - 40) / 2f,
                            (43 * GLOBALS.Scale - 60) / 2f);

                        if (_snap || _alignment)
                        {
                            var mouseCritRect = Utils.CameraCriticalRectangle(draggedOrigin);

                            if (GLOBALS.Level.Cameras.Count == 1)
                            {
                                cam.Coords = draggedOrigin;
                            }
                            else
                            {
                                for (var cameraIndex = 0; cameraIndex < GLOBALS.Level.Cameras.Count; cameraIndex++)
                                {
                                    if (cameraIndex == draggedCamera) goto skipInner;
                                    
                                    var camera = GLOBALS.Level.Cameras[cameraIndex];

                                    if (_snap)
                                    {
                                        // var critRect = Utils.CameraCriticalRectangle(camera.Coords);
                        
                                        var distX = draggedOrigin.X - camera.Coords.X;
                                        var distY = draggedOrigin.Y - camera.Coords.Y;
                        
                                        // var dist = new Vector2(distX, distY);
                        
                                        var absDistX = Math.Abs(distX);
                                        var absDistY = Math.Abs(distY);
                                        
                                        var snapX = absDistX <= mouseCritRect.Width + 20 && absDistX >= mouseCritRect.Width - 20;
                                        var snapY = absDistY <= mouseCritRect.Height + 20 && absDistY >= mouseCritRect.Height - 20;
                                        
                                        if (snapX || snapY)
                                        {
                                            if (snapX)
                                            {
                                                if (distX > 0)
                                                {
                                                    cam.Coords = draggedOrigin with { X = camera.Coords.X + mouseCritRect.Width };
                                                }
                                                else
                                                {
                                                    cam.Coords = draggedOrigin with { X = camera.Coords.X - mouseCritRect.Width };
                                                }
                                            }

                                            if (snapY)
                                            {
                                                if (distY > 0)
                                                {
                                                    cam.Coords = draggedOrigin with { Y = camera.Coords.Y + mouseCritRect.Height };
                                                }
                                                else
                                                {
                                                    cam.Coords = draggedOrigin with { Y = camera.Coords.Y - mouseCritRect.Height };
                                                }
                                            }
                                        }
                                        else
                                        {
                                            cam.Coords = draggedOrigin;
                                        }
                                    }
                                    else if (_alignment)
                                    {
                                        var alignX = draggedOrigin.X >= camera.Coords.X - 20 &&
                                                     draggedOrigin.X <= camera.Coords.X + 20;
                                        
                                        var alignY = draggedOrigin.Y >= camera.Coords.Y - 20 &&
                                                     draggedOrigin.Y <= camera.Coords.Y + 20;

                                        if (alignX || alignY)
                                        {
                                            if (alignX)
                                            {
                                                cam.Coords = draggedOrigin with { X = camera.Coords.X };
                                                DrawLineEx(cam.Coords + new Vector2(
                                                    (72 * GLOBALS.Scale - 40) / 2f,
                                                    (43 * GLOBALS.Scale - 60) / 2f), camera.Coords + new Vector2(
                                                    (72 * GLOBALS.Scale - 40) / 2f,
                                                    (43 * GLOBALS.Scale - 60) / 2f), 2f, Color.Red);
                                            }
                                            else if (alignY)
                                            {
                                                cam.Coords = draggedOrigin with { Y = camera.Coords.Y };
                                                DrawLineEx(cam.Coords + new Vector2(
                                                    (72 * GLOBALS.Scale - 40) / 2f,
                                                    (43 * GLOBALS.Scale - 60) / 2f), camera.Coords + new Vector2(
                                                    (72 * GLOBALS.Scale - 40) / 2f,
                                                    (43 * GLOBALS.Scale - 60) / 2f), 2f, Color.Green);
                                            }
                                        }
                                        else
                                        {
                                            cam.Coords = draggedOrigin;
                                        }
                                    }
                                    
                                    skipInner: {}
                                }
                            }
                        }
                        else
                        {
                            cam.Coords = draggedOrigin;
                        }
                        
                        Printers.DrawCameraSprite(cam.Coords, cam.Quad, _camera, index);
                    }
                    else
                    {
                        var (clicked, _) = Printers.DrawCameraSprite(cam.Coords, cam.Quad, _camera, index);

                        if (clicked && !clickTracker)
                        {
                            draggedCamera = index;
                            clickTracker = true;
                        }
                    }
                    
                }

                DrawRectangleLinesEx(
                    GLOBALS.Level.Border,
                    4f,
                    new(200, 66, 245, 255)
                );
                
                if (GLOBALS.Settings.GeneralSettings.DarkTheme)
                {
                    DrawRectangleLines(0, 0, GLOBALS.Level.Width*GLOBALS.Scale, GLOBALS.Level.Height*GLOBALS.Scale, Color.White);
                }
            }
            EndMode2D();

            #region CameraEditorUI

            rlImGui.Begin();
            
            ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
            
            // Settings

            if (ImGui.Begin("Settings##CameraEditorSettings"))
            {
                var availableSpace = ImGui.GetContentRegionAvail();
                
                ImGui.SeparatorText("Helpers");

                var selected = ImGui.Button(_snap 
                    ? "Snap" 
                    : _alignment 
                        ? "Alignment" 
                        : "None", 
                    availableSpace with { Y = 20 }
                );

                if (selected)
                {
                    (_snap, _alignment) = (_snap, _alignment) switch
                    {
                        (true, false) => (false, true),
                        (false, true) => (false, false),
                        (false, false) => (true, false),
                        _ => (true, false)
                    };
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
                var shortcutWindowRect = Printers.ImGui.ShortcutsWindow(GLOBALS.Settings.Shortcuts.CameraEditor);

                _isShortcutsWinHovered = CheckCollisionPointRec(
                    GetMousePosition(), 
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
            #endregion
        }
        EndDrawing();
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera;
    }
}
