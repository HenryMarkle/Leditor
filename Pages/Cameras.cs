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

        Raylib.BeginDrawing();
        {
            Raylib.ClearBackground(new(170, 170, 170, 255));

            Raylib.BeginMode2D(_camera);
            {

                Raylib.DrawRectangle(
                    0, 0,
                    GLOBALS.Level.Width * GLOBALS.Scale,
                    GLOBALS.Level.Height * GLOBALS.Scale,
                    new(255, 255, 255, 255)
                );

                #region CamerasLevelBackground

                for (int y = 0; y < GLOBALS.Level.Height; y++)
                {
                    for (int x = 0; x < GLOBALS.Level.Width; x++)
                    {
                        for (int z = 1; z < 3; z++)
                        {
                            var cell = GLOBALS.Level.GeoMatrix[y, x, z];

                            var texture = Utils.GetBlockIndex(cell.Geo);

                            if (texture >= 0)
                            {
                                Raylib.DrawTexture(GLOBALS.Textures.GeoBlocks[texture], x * GLOBALS.Scale, y * GLOBALS.Scale, new(0, 0, 0, 170));
                            }
                        }
                    }
                }

                if (!GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1)
                {
                    Raylib.DrawRectangle(
                        (-1) * GLOBALS.Scale,
                        (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel) * GLOBALS.Scale,
                        (GLOBALS.Level.Width + 2) * GLOBALS.Scale,
                        GLOBALS.Level.WaterLevel * GLOBALS.Scale,
                        new(0, 0, 255, 255)
                    );
                }

                for (int y = 0; y < GLOBALS.Level.Height; y++)
                {
                    for (int x = 0; x < GLOBALS.Level.Width; x++)
                    {
                        var cell = GLOBALS.Level.GeoMatrix[y, x, 0];

                        var texture = Utils.GetBlockIndex(cell.Geo);

                        if (texture >= 0)
                        {
                            DrawTexture(GLOBALS.Textures.GeoBlocks[texture], x * GLOBALS.Scale, y * GLOBALS.Scale, new(0, 0, 0, 225));
                        }

                        for (int s = 1; s < cell.Stackables.Length; s++)
                        {
                            if (cell.Stackables[s])
                            {
                                switch (s)
                                {
                                    // dump placement
                                    case 1:     // ph
                                    case 2:     // pv
                                        Raylib.DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], x * GLOBALS.Scale, y * GLOBALS.Scale, BLACK);
                                        break;
                                    case 3:     // bathive
                                    case 5:     // entrance
                                    case 6:     // passage
                                    case 7:     // den
                                    case 9:     // rock
                                    case 10:    // spear
                                    case 12:    // forbidflychains
                                    case 13:    // garbagewormhole
                                    case 18:    // waterfall
                                    case 19:    // wac
                                    case 20:    // worm
                                    case 21:    // scav
                                        Raylib.DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], x * GLOBALS.Scale, y * GLOBALS.Scale, WHITE);
                                        break;

                                    // directional placement
                                    case 4:     // entrance
                                        var index = Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, 0));

                                        if (index is 22 or 23 or 24 or 25)
                                        {
                                            GLOBALS.Level.GeoMatrix[y, x, 0].Geo = 7;
                                        }

                                        DrawTexture(GLOBALS.Textures.GeoStackables[index], x * GLOBALS.Scale, y * GLOBALS.Scale, WHITE);
                                        break;
                                    case 11:    // crack
                                        DrawTexture(
                                            GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, 0))],
                                            x * GLOBALS.Scale,
                                            y * GLOBALS.Scale,
                                            BLACK
                                        );
                                        break;
                                }
                            }
                        }
                    }
                }

                if (GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1)
                {
                    Raylib.DrawRectangle(
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
            }
            Raylib.EndMode2D();

            #region CameraEditorUI

            // Shortcuts window
            if (GLOBALS.Settings.GeneralSettings.ShortcutWindow)
            {
                rlImGui.Begin();
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


                rlImGui.End();
            }

            #endregion
        }
        EndDrawing();
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera;
    }
}
