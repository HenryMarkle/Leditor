using System.Numerics;
using ImGuiNET;
using rlImGui_cs;
using static Raylib_cs.Raylib;
using Leditor.Types;

namespace Leditor.Pages;

internal class CamerasEditorPage : EditorPage
{
    public override void Dispose()
    {
        Disposed = true;
    }
    
    Camera2D _camera = new() { Zoom = 0.8f, Target = new(-100, -100) };
    bool clickTracker;
    int draggedCamera = -1;

    private bool _showTiles;
    private bool _showProps;
    
    private readonly CameraShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.CameraEditor;

    private int _currentCamera;
    
    private bool _isShortcutsWinHovered;
    private bool _isShortcutsWinDragged;

    private bool _isCamsWinHovered;

    private bool _shouldRedrawLevel = true;
    
    private bool _alignment = GLOBALS.Settings.CameraSettings.Alignment;
    private bool _snap = GLOBALS.Settings.CameraSettings.Snap;

    public void OnPageUpdated(int previous, int @next) {
        if (@next == 4) _shouldRedrawLevel = true;
    }

    public override void Draw()
    {
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera;

        var worldMouse = GetScreenToWorld2D(GetMousePosition(), _camera);

        #region CamerasInputHandlers

        var ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
        var shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
        var alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);

        // handle mouse drag
        if (_shortcuts.DragLevel.Check(ctrl, shift, alt, true))
        {
            var delta = GetMouseDelta();
            delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
            _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
        }

        // handle zoom
        var cameraWheel = GetMouseWheelMove();
        if (!_isCamsWinHovered && !_isShortcutsWinHovered && cameraWheel != 0)
        {
            var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
            _camera.Offset = GetMousePosition();
            _camera.Target = mouseWorldPosition;
            _camera.Zoom += cameraWheel * GLOBALS.ZoomIncrement;
            if (!GLOBALS.Settings.GeneralSettings.LinearZooming && _camera.Zoom < GLOBALS.ZoomIncrement) _camera.Zoom = GLOBALS.ZoomIncrement;
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
            Utils.Restrict(ref _currentCamera, 0, GLOBALS.Level.Cameras.Count-1);
        }

        if (_shortcuts.DeleteCamera.Check(ctrl, shift, alt) && draggedCamera != -1)
        {
            GLOBALS.Level.Cameras.RemoveAt(draggedCamera);
            GLOBALS.CamQuadLocks = GLOBALS.CamQuadLocks[..^1];
            draggedCamera = -1;
            Utils.Restrict(ref _currentCamera, 0, GLOBALS.Level.Cameras.Count-1);
        }

        if (!_isCamsWinHovered && _shortcuts.CreateAndDeleteCamera.Check(ctrl, shift, alt) || _shortcuts.CreateAndDeleteCameraAlt.Check(ctrl, shift, alt))
        {
            if (draggedCamera == -1)
            {
                GLOBALS.Level.Cameras = [.. GLOBALS.Level.Cameras, new RenderCamera() { Coords = new Vector2(0, 0), Quad = new(new(), new(), new(), new()) }];
                GLOBALS.CamQuadLocks = [..GLOBALS.CamQuadLocks, 0];
                draggedCamera = GLOBALS.Level.Cameras.Count - 1;
                _currentCamera = draggedCamera;
            }
            else
            {
                GLOBALS.Level.Cameras.RemoveAt(draggedCamera);
                GLOBALS.CamQuadLocks = GLOBALS.CamQuadLocks[..^1];
                draggedCamera = -1;
            }
            Utils.Restrict(ref _currentCamera, 0, GLOBALS.Level.Cameras.Count-1);
        }

        #endregion

        BeginDrawing();
        {
            ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.Black : new Color(170, 170, 170, 255));

            if (_shouldRedrawLevel)
            {
                Printers.DrawLevelIntoBuffer(GLOBALS.Textures.GeneralLevel, new Printers.DrawLevelParams
                {
                    DarkTheme = GLOBALS.Settings.GeneralSettings.DarkTheme,
                    TilesLayer1 = _showTiles,
                    TilesLayer2 = _showTiles,
                    TilesLayer3 = _showTiles,
                    PropsLayer1 = _showProps,
                    PropsLayer2 = _showProps,
                    PropsLayer3 = _showProps,
                    PropDrawMode = GLOBALS.Settings.GeneralSettings.DrawPropMode,
                    TileDrawMode = GLOBALS.Settings.GeneralSettings.DrawTileMode,
                    Palette = GLOBALS.SelectedPalette,
                    CurrentLayer = 0
                });
                _shouldRedrawLevel = false;
            }

            BeginMode2D(_camera);
            {
                #region CamerasLevelBackground

                BeginShaderMode(GLOBALS.Shaders.VFlip);
                SetShaderValueTexture(GLOBALS.Shaders.VFlip, GetShaderLocation(GLOBALS.Shaders.VFlip, "inputTexture"), GLOBALS.Textures.GeneralLevel.Texture);
                DrawTexture(GLOBALS.Textures.GeneralLevel.Texture, 0, 0, Color.White);
                EndShaderMode();
                
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
                            _currentCamera = index;
                            Utils.Restrict(ref _currentCamera, 0, GLOBALS.Level.Cameras.Count-1);
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
            
            // Navigation bar
                
            if (GLOBALS.Settings.GeneralSettings.Navbar) GLOBALS.NavSignal = Printers.ImGui.Nav(out _);

            // Cameras

            var camWinOpenned = ImGui.Begin("Cameras##CamerasEditorCamerasWindow");
            
            var camWinPos = ImGui.GetWindowPos();
            var camWinSpace = ImGui.GetWindowSize();

            if (CheckCollisionPointRec(GetMousePosition(), new(camWinPos.X - 5, camWinPos.Y-5, camWinSpace.X + 10, camWinSpace.Y+10)))
            {
                _isCamsWinHovered = true;

            }
            else
            {
                _isCamsWinHovered = false;
            }

            if (camWinOpenned) {

                var listBoxAvail = ImGui.GetContentRegionAvail();
                if (ImGui.BeginListBox("##CamerasSelector", listBoxAvail with { Y = listBoxAvail.Y - 170 })) {
                    
                    for (var i = 0; i < GLOBALS.Level.Cameras.Count; i++) {

                        if (ImGui.Selectable($"Camera #{i}", _currentCamera == i)) {
                            _currentCamera = i;
                        }

                    }
                    
                    ImGui.EndListBox();
                }

                //

                if (_currentCamera >= 0 && _currentCamera < GLOBALS.Level.Cameras.Count && ImGui.BeginChild("Camera Quad")) {

                    var currentCam = GLOBALS.Level.Cameras[_currentCamera];

                    ImGui.SeparatorText("Camera Quad Points");
                    
                    ImGui.Columns(2);

                    ImGui.SeparatorText("Top Left");
                    {
                        var (angle, radius) = currentCam.Quad.TopLeft;
                        ImGui.SetNextItemWidth(100);
                        if (ImGui.InputInt("Angle##TopLeftAngle", ref angle)) currentCam.Quad.TopLeft = (angle, radius);
                        ImGui.SetNextItemWidth(100);
                        if (ImGui.InputFloat("Radius##TopLeftRadius", ref radius, 0.1f)) {
                            Utils.Restrict(ref radius, 0);
                            currentCam.Quad.TopLeft = (angle, radius);
                        }
                    }

                    ImGui.SeparatorText("Bottom Left");
                    {
                        var (angle, radius) = currentCam.Quad.BottomLeft;
                        ImGui.SetNextItemWidth(100);
                        if (ImGui.InputInt("Angle##BottomLeftAngle", ref angle)) currentCam.Quad.BottomLeft = (angle, radius);
                        ImGui.SetNextItemWidth(100);
                        if (ImGui.InputFloat("Radius##BottomLeftRadius", ref radius, 0.1f)) {
                            Utils.Restrict(ref radius, 0);
                            currentCam.Quad.BottomLeft = (angle, radius);
                        }
                    }

                    ImGui.NextColumn();
                    
                    ImGui.SeparatorText("Top Right");
                    {
                        var (angle, radius) = currentCam.Quad.TopRight;
                        ImGui.SetNextItemWidth(100);
                        if (ImGui.InputInt("Angle##TopRightAngle", ref angle)) currentCam.Quad.TopRight = (angle, radius);
                        ImGui.SetNextItemWidth(100);
                        if (ImGui.InputFloat("Radius##TopRightRadius", ref radius, 0.1f)) {
                            Utils.Restrict(ref radius, 0);
                            currentCam.Quad.TopRight = (angle, radius);
                        }
                    }

                    ImGui.SeparatorText("Bottom Right");
                    {
                        var (angle, radius) = currentCam.Quad.BottomRight;
                        ImGui.SetNextItemWidth(100);
                        if (ImGui.InputInt("Angle##BottomRightAngle", ref angle)) currentCam.Quad.BottomRight = (angle, radius);
                        ImGui.SetNextItemWidth(100);
                        if (ImGui.InputFloat("Radius##BottomRightRadius", ref radius, 0.1f)) {
                            Utils.Restrict(ref radius, 0);
                            currentCam.Quad.BottomRight = (angle, radius);
                        }
                    }
                    
                    ImGui.EndChild();
                }

                ImGui.End();
            }
            
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
                
                ImGui.Spacing();

                if (ImGui.Checkbox("Tiles", ref _showTiles)) _shouldRedrawLevel = true;
                if (ImGui.Checkbox("Props", ref _showProps)) _shouldRedrawLevel = true;
                
                ImGui.End();
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
