using static Raylib_CsLo.Raylib;

using System.Numerics;

namespace Leditor;

public class ExperimentalGeometryPage(Serilog.Core.Logger logger) : IPage
{
    private readonly Serilog.Core.Logger _logger = logger;

    private Camera2D _camera = new() { zoom = 1.0f };
    
    private bool _multiselect;
    private bool _hideGrid;
    private bool _clickTracker;
    
    private bool _showLayer1 = true;
    private bool _showLayer2 = true;
    private bool _showLayer3 = true;

    private bool _eraseMode;
    private bool _eraseAllMode;
    private bool _allowMultiSelect;
    
    private int _prevCoordsX = -1;
    private int _prevCoordsY = -1;

    private int _geoMenuScrollIndex = 0;
    private int _geoMenuCategory;
    private int _geoMenuIndex;
    
    private readonly byte[] _geoMenuPanelBytes = "Menu"u8.ToArray();
    
    private static readonly int[] GeoMenuIndexMaxCount = [4, 3, 6, 7];
    private static readonly int[] GeoMenuIndexToBlockId = [1, 2, 6, 9];
    private static readonly int[] GeoMenuCategory2ToStackableId = [2, 1, 11];
    private static readonly int[] GeoMenuCategory3ToStackableId = [3, 12, 18, 20, 9, 10];
    private static readonly int[] GeoMenuCategory4ToStackableId = [4, 5, 6, 7, 19, 21, 13];
    
    public void Draw()
    {
        GLOBALS.PreviousPage = 2;
        var scale = GLOBALS.Scale;
        var settings = GLOBALS.Settings;
        Span<Color> layerColors = [settings.GeometryEditor.LayerColors.Layer1, settings.GeometryEditor.LayerColors.Layer2, settings.GeometryEditor.LayerColors.Layer3];

        if (IsKeyPressed(KeyboardKey.KEY_ONE))
        {
            GLOBALS.Page = 1;
        }
        // if (IsKeyReleased(KeyboardKey.KEY_TWO)) page = 2;
        if (IsKeyReleased(KeyboardKey.KEY_THREE))
        {
            GLOBALS.Page = 3;
        }
        if (IsKeyReleased(KeyboardKey.KEY_FOUR))
        {
            GLOBALS.Page = 4;
        }
        if (IsKeyReleased(KeyboardKey.KEY_FIVE))
        {
            GLOBALS.Page = 5;
        }
        if (IsKeyReleased(KeyboardKey.KEY_SIX))
        {
            GLOBALS.ResizeFlag = true;
            GLOBALS.Page = 6;
        }
        if (IsKeyReleased(KeyboardKey.KEY_SEVEN))
        {
            GLOBALS.Page = 7;
        }
        if (IsKeyReleased(KeyboardKey.KEY_EIGHT))
        {
            GLOBALS.Page = 8;
        }
        if (IsKeyReleased(KeyboardKey.KEY_NINE))
        {
            GLOBALS.Page = 9;
        }

        Vector2 mouse = GetScreenToWorld2D(GetMousePosition(), _camera);

        //                        v this was done to avoid rounding errors
        int matrixY = mouse.Y < 0 ? -1 : (int)mouse.Y / scale;
        int matrixX = mouse.X < 0 ? -1 : (int)mouse.X / scale;

        var sWidth = GetScreenWidth();
        var sHeight = GetScreenHeight();

        Rectangle panelRect = new(sWidth - 300, 50, 288, 400);

        var canDrawGeo = !CheckCollisionPointRec(GetMousePosition(), panelRect);

        // handle geo selection

        if (IsKeyPressed(KeyboardKey.KEY_A))
        {
            _geoMenuCategory--;
            if (_geoMenuCategory < 0) _geoMenuCategory = 3;
            _geoMenuIndex = 0;
        }

        if (IsKeyPressed(KeyboardKey.KEY_D))
        {
            _geoMenuCategory = ++_geoMenuCategory % 4;
            _geoMenuIndex = 0;
        }

        if (IsKeyPressed(KeyboardKey.KEY_W))
        {
            _geoMenuIndex--;
            if (_geoMenuIndex < 0) _geoMenuIndex = GeoMenuIndexMaxCount[_geoMenuCategory] - 1;
        }

        if (IsKeyPressed(KeyboardKey.KEY_S))
        {
            _geoMenuIndex = ++_geoMenuIndex % GeoMenuIndexMaxCount[_geoMenuCategory];
        }

        if (IsKeyPressed(KeyboardKey.KEY_E)) _eraseMode = !_eraseMode;

        if (IsKeyPressed(KeyboardKey.KEY_Q)) _allowMultiSelect = !_allowMultiSelect;

        if (IsKeyPressed(KeyboardKey.KEY_R)) _eraseAllMode = !_eraseAllMode;

        // handle changing layers

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_L))
        {
            GLOBALS.Layer = ++GLOBALS.Layer % 3;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_M))
        {
            _hideGrid = !_hideGrid;
        }

        // handle mouse drag
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
        {
            Vector2 delta = Raylib.GetMouseDelta();
            delta = RayMath.Vector2Scale(delta, -1.0f / _camera.zoom);
            _camera.target = RayMath.Vector2Add(_camera.target, delta);
        }


        // handle zoom
        var wheel = Raylib.GetMouseWheelMove();
        if (wheel != 0)
        {
            Vector2 mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), _camera);
            _camera.offset = Raylib.GetMousePosition();
            _camera.target = mouseWorldPosition;
            _camera.zoom += wheel * GLOBALS.ZoomIncrement;
            if (_camera.zoom < GLOBALS.ZoomIncrement) _camera.zoom = GLOBALS.ZoomIncrement;
        }



        // multi-place/erase geos

        if (_allowMultiSelect)
        {
            if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT) && !_clickTracker)
            {
                if (canDrawGeo && matrixY >= 0 && matrixY < GLOBALS.Level.Height && matrixX >= 0 && matrixX < GLOBALS.Level.Width)
                {
                    _clickTracker = true;
                    _multiselect = true;

                    _prevCoordsX = matrixX;
                    _prevCoordsY = matrixY;
                }
            }

            if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
            {
                _clickTracker = false;

                int startX, startY, endX, endY;

                if (matrixX > _prevCoordsX)
                {
                    startX = _prevCoordsX;
                    endX = matrixX;
                }
                else
                {
                    startX = matrixX;
                    endX = _prevCoordsX;
                }

                if (matrixY > _prevCoordsY)
                {
                    startY = _prevCoordsY;
                    endY = matrixY;
                }
                else
                {
                    startY = matrixY;
                    endY = _prevCoordsY;
                }

                for (int y = startY; y <= endY; y++)
                {
                    for (int x = startX; x <= endX; x++)
                    {
                        if (x < 0 || x >= GLOBALS.Level.Width || y < 0 || y >= GLOBALS.Level.Height) continue;

                        if (_eraseAllMode)
                        {
                            var cell = GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer];

                            cell.Geo = 0;
                            Array.Fill(cell.Stackables, false);

                            GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer] = cell;
                        }
                        else
                        {
                            switch (_geoMenuCategory)
                            {
                                case 0:
                                    {

                                        var id = GeoMenuIndexToBlockId[_geoMenuIndex];

                                        // slope
                                        if (id == 2)
                                        {
                                            var slope = Utils.GetCorrectSlopeID(Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, GLOBALS.Layer));
                                            if (slope == -1) break;
                                            GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer].Geo = _eraseMode ? 0 : slope;
                                        }
                                        // solid, platform, glass
                                        else
                                        {
                                            GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer].Geo = _eraseMode ? 0 : id;
                                        }
                                    }
                                    break;

                                case 1:
                                    {
                                        var id = GeoMenuCategory2ToStackableId[_geoMenuIndex];
                                        GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer].Stackables[id] = !_eraseMode;
                                    }
                                    break;

                                case 2:
                                    {
                                        if (
                                            x * scale < GLOBALS.Level.Border.X ||
                                            x * scale >= GLOBALS.Level.Border.width + GLOBALS.Level.Border.X ||
                                            y * scale < GLOBALS.Level.Border.Y ||
                                            y * scale >= GLOBALS.Level.Border.height + GLOBALS.Level.Border.Y) break;

                                        if (_geoMenuIndex == 0 && GLOBALS.Layer != 0) break;

                                        var id = GeoMenuCategory3ToStackableId[_geoMenuIndex];
                                        GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer].Stackables[id] = !_eraseMode;
                                    }
                                    break;

                                case 3:
                                    {
                                        if (
                                            x * scale < GLOBALS.Level.Border.X ||
                                            x * scale >= GLOBALS.Level.Border.width + GLOBALS.Level.Border.X ||
                                            y * scale < GLOBALS.Level.Border.Y ||
                                            y * scale >= GLOBALS.Level.Border.height + GLOBALS.Level.Border.Y) break;

                                        if (GLOBALS.Layer != 0) break;

                                        var id = GeoMenuCategory4ToStackableId[_geoMenuIndex];
                                        GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer].Stackables[id] = !_eraseMode;
                                    }
                                    break;
                            }
                        }
                    }
                }

                _multiselect = false;
            }
        }
        // handle placing geo
        else
        {
            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT) && canDrawGeo && matrixY >= 0 && matrixY < GLOBALS.Level.Height && matrixX >= 0 && matrixX < GLOBALS.Level.Width)
            {
                if (_eraseAllMode)
                {
                    var cell = GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer];

                    cell.Geo = 0;
                    Array.Fill(cell.Stackables, false);

                    GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer] = cell;
                }
                else
                {
                    switch (_geoMenuCategory)
                    {
                        case 0:
                            {

                                var id = GeoMenuIndexToBlockId[_geoMenuIndex];

                                // slope
                                if (id == 2)
                                {
                                    var slope = Utils.GetCorrectSlopeID(Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, matrixX, matrixY, GLOBALS.Layer));
                                    if (slope == -1) break;
                                    GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer].Geo = _eraseMode ? 0 : slope;
                                }
                                // solid, platform, glass
                                else
                                {
                                    GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer].Geo = _eraseMode ? 0 : id;
                                }
                            }
                            break;

                        case 1:
                            {
                                var id = GeoMenuCategory2ToStackableId[_geoMenuIndex];
                                GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer].Stackables[id] = !_eraseMode;
                            }
                            break;

                        case 2:
                            {
                                if (
                                    matrixX * scale < GLOBALS.Level.Border.X ||
                                    matrixX * scale >= GLOBALS.Level.Border.width + GLOBALS.Level.Border.X ||
                                    matrixY * scale < GLOBALS.Level.Border.Y ||
                                    matrixY * scale >= GLOBALS.Level.Border.height + GLOBALS.Level.Border.Y) break;

                                if (_geoMenuIndex == 0 && GLOBALS.Layer != 0) break;

                                var id = GeoMenuCategory3ToStackableId[_geoMenuIndex];
                                GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer].Stackables[id] = !_eraseMode;
                            }
                            break;

                        case 3:
                            {
                                if (
                                    matrixX * scale < GLOBALS.Level.Border.X ||
                                    matrixX * scale >= GLOBALS.Level.Border.width + GLOBALS.Level.Border.X ||
                                    matrixY * scale < GLOBALS.Level.Border.Y ||
                                    matrixY * scale >= GLOBALS.Level.Border.height + GLOBALS.Level.Border.Y) break;

                                if (GLOBALS.Layer != 0) break;

                                var id = GeoMenuCategory4ToStackableId[_geoMenuIndex];
                                GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer].Stackables[id] = !_eraseMode;
                            }
                            break;
                    }
                }
            }
        }


        BeginDrawing();
        {
            ClearBackground(new Color(120, 120, 120, 255));


            BeginMode2D(_camera);
            {
                // geo matrix

                // first layer without stackables

                if (_showLayer1)
                {
                    for (int y = 0; y < GLOBALS.Level.Height; y++)
                    {
                        for (int x = 0; x < GLOBALS.Level.Width; x++)
                        {
                            const int z = 0;

                            var cell = GLOBALS.Level.GeoMatrix[y, x, z];

                            var texture = Utils.GetBlockIndex(cell.Geo);

                            if (texture >= 0)
                            {
                                DrawTexture(GLOBALS.Textures.GeoBlocks[texture], x * scale, y * scale, layerColors[z]);
                            }

                            if (!_hideGrid) DrawRectangleLinesEx(
                                new(x * scale, y * scale, scale, scale),
                                0.5f,
                                new(255, 255, 255, 100)
                            );
                        }
                    }
                }

                // the rest of the layers

                for (int y = 0; y < GLOBALS.Level.Height; y++)
                {
                    for (int x = 0; x < GLOBALS.Level.Width; x++)
                    {
                        for (int z = 1; z < 3; z++)
                        {
                            if (z == 1 && !_showLayer2) continue;
                            if (z == 2 && !_showLayer3) continue;

                            var cell = GLOBALS.Level.GeoMatrix[y, x, z];

                            var texture = Utils.GetBlockIndex(cell.Geo);

                            if (texture >= 0)
                            {
                                Raylib.DrawTexture(GLOBALS.Textures.GeoBlocks[texture], x * scale, y * scale, layerColors[z]);
                            }

                            if (!_hideGrid && !_showLayer1 && z == 1) DrawRectangleLinesEx(
                                new(x * scale, y * scale, scale, scale),
                                0.5f,
                                new(255, 255, 255, 100)
                            );

                            if (!_hideGrid && !_showLayer1 && z == 2) DrawRectangleLinesEx(
                                new(x * scale, y * scale, scale, scale),
                                0.5f,
                                new(255, 255, 255, 100)
                            );

                            for (int s = 1; s < cell.Stackables.Length; s++)
                            {
                                if (cell.Stackables[s])
                                {
                                    switch (s)
                                    {
                                        // dump placement
                                        case 1:     // ph
                                        case 2:     // pv
                                            DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], x * scale, y * scale, layerColors[z]);
                                            break;
                                        case 3:     // bathive
                                            /*case 5:     // entrance
                                            case 6:     // passage
                                            case 7:     // den
                                            case 9:     // rock
                                            case 10:    // spear
                                            case 12:    // forbidflychains
                                            case 13:    // garbagewormhole
                                            case 18:    // waterfall
                                            case 19:    // wac
                                            case 20:    // worm
                                            case 21:    // scav*/
                                            DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], x * scale, y * scale, WHITE); // TODO: remove opacity from entrances
                                            break;

                                        // directional placement
                                        /*case 4:     // entrance
                                            var index = GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, z));

                                            if (index is 22 or 23 or 24 or 25)
                                            {
                                                geoMatrix[y, x, 0].Geo = 7;
                                            }

                                            Raylib.DrawTexture(stackableTextures[index], x * scale, y * scale, whiteStackable);
                                            break;*/
                                        case 11:    // crack
                                            Raylib.DrawTexture(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, z))],
                                                x * scale,
                                                y * scale,
                                                WHITE
                                            );
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }

                // draw stackables

                if (_showLayer1)
                {
                    for (int y = 0; y < GLOBALS.Level.Height; y++)
                    {
                        for (int x = 0; x < GLOBALS.Level.Width; x++)
                        {
                            const int z = 0;

                            var cell = GLOBALS.Level.GeoMatrix[y, x, z];

                            for (int s = 1; s < cell.Stackables.Length; s++)
                            {
                                if (cell.Stackables[s])
                                {
                                    switch (s)
                                    {
                                        // dump placement
                                        case 1:     // ph
                                        case 2:     // pv
                                            Raylib.DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], x * scale, y * scale, layerColors[z]);
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
                                            Raylib.DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], x * scale, y * scale, new(255, 255, 255, 255)); // TODO: remove opacity from entrances
                                            break;

                                        // directional placement
                                        case 4:     // entrance
                                            var index = Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, z));

                                            if (index is 22 or 23 or 24 or 25)
                                            {
                                                GLOBALS.Level.GeoMatrix[y, x, 0].Geo = 7;
                                            }

                                            Raylib.DrawTexture(GLOBALS.Textures.GeoStackables[index], x * scale, y * scale, new(255, 255, 255, 255));
                                            break;
                                        case 11:    // crack
                                            Raylib.DrawTexture(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, z))],
                                                x * scale,
                                                y * scale,
                                                new(255, 255, 255, 255)
                                            );
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }


                // the selection rectangle

                for (int y = 0; y < GLOBALS.Level.Height; y++)
                {
                    for (int x = 0; x < GLOBALS.Level.Width; x++)
                    {
                        if (_multiselect)
                        {
                            var XS = matrixX - _prevCoordsX;
                            var YS = matrixY - _prevCoordsY;
                            var width = Math.Abs(XS == 0 ? 1 : XS + (XS > 0 ? 1 : -1)) * scale;
                            var height = Math.Abs(YS == 0 ? 1 : YS + (YS > 0 ? 1 : -1)) * scale;

                            Rectangle rec = (XS >= 0, YS >= 0) switch
                            {
                                // br
                                (true, true) => new(_prevCoordsX * scale, _prevCoordsY * scale, width, height),

                                // tr
                                (true, false) => new(_prevCoordsX * scale, matrixY * scale, width, height),

                                // bl
                                (false, true) => new(matrixX * scale, _prevCoordsY * scale, width, height),

                                // tl
                                (false, false) => new(matrixX * scale, matrixY * scale, width, height)
                            };

                            Raylib.DrawRectangleLinesEx(rec, 2, _eraseMode ? new(255, 0, 0, 255) : new(255, 255, 255, 255));

                            Raylib.DrawText(
                                $"{width / scale:0}x{height / scale:0}",
                                (int)mouse.X + 10,
                                (int)mouse.Y,
                                4,
                                new(255, 255, 255, 255)
                                );
                        }
                        else
                        {
                            if (matrixX == x && matrixY == y)
                            {
                                DrawRectangleLinesEx(new(x * scale, y * scale, scale, scale), 2, _eraseMode ? new(255, 0, 0, 255) : new(255, 255, 255, 255));

                                // Coordinates

                                if (matrixX >= 0 && matrixX < GLOBALS.Level.Width && matrixY >= 0 && matrixY < GLOBALS.Level.Height)
                                    Raylib.DrawText(
                                        $"x: {matrixX:0} y: {matrixY:0}",
                                        (matrixX + 1) * scale,
                                        (matrixY - 1) * scale,
                                        12,
                                        WHITE);
                            }
                        }
                    }
                }

                // the outbound border
                DrawRectangleLinesEx(new(0, 0, GLOBALS.Level.Width * scale, GLOBALS.Level.Height * scale), 2, new(0, 0, 0, 255));

                // the border
                DrawRectangleLinesEx(GLOBALS.Level.Border, _camera.zoom < GLOBALS.ZoomIncrement ? 5 : 2, new(255, 255, 255, 255));

                /*// a lazy way to hide the rest of the grid
                DrawRectangle(matrixWidth * -scale, -3, matrixWidth * scale, matrixHeight * 2 * scale, new(120, 120, 120, 255));
                DrawRectangle(0, matrixHeight * scale, matrixWidth * scale + 2, matrixHeight * scale, new(120, 120, 120, 255));*/
            }
            EndMode2D();

            // geo menu

            unsafe
            {
               fixed (byte* pt = _geoMenuPanelBytes)
               {
                   RayGui.GuiPanel(panelRect, (sbyte*)pt);
               } 
            }
            

            // Categories

            DrawRectangleLinesEx(new((_geoMenuCategory * 67) + panelRect.X + 10, 80, 67, 67), 4.0f, BLUE);

            DrawRectangleLinesEx(new(panelRect.X + 10, 80, 67, 67), 1.0f, BLACK);
            DrawRectangleLinesEx(new(67 + panelRect.X + 10, 80, 67, 67), 1.0f, BLACK);
            DrawRectangleLinesEx(new(134 + panelRect.X + 10, 80, 67, 67), 1.0f, BLACK);
            DrawRectangleLinesEx(new(201 + panelRect.X + 10, 80, 67, 67), 1.0f, BLACK);

            DrawTriangle(
                new(panelRect.X + 20, 90),
                new(panelRect.X + 20, 137),
                new(panelRect.X + 67, 137),
                BLACK
            );

            DrawRectangleV(
                new(panelRect.X + 107, 90),
                new(10, 47),
                BLACK
            );

            DrawRectangleV(
                new(panelRect.X + 87, 108),
                new(47, 10),
                BLACK
            );

            var placeSpearTexture = GLOBALS.Textures.GeoMenu[8];
            var entryTexture = GLOBALS.Textures.GeoMenu[14];

            DrawTexturePro(
                placeSpearTexture,
                new(0, 0, placeSpearTexture.width, placeSpearTexture.height),
                new(panelRect.X + 154, 90, 47, 47),
                new(0, 0),
                0,
                BLACK
            );

            DrawTexturePro(
                entryTexture,
                new(0, 0, entryTexture.width, entryTexture.height),
                new(panelRect.X + 221, 90, 47, 47),
                new(0, 0),
                0,
                BLACK
            );

            unsafe
            {
                fixed (int* scrollIndex = &_geoMenuScrollIndex)
                {
                    var newGeoMenuIndex = RayGui.GuiListView(
                        new(panelRect.X + 10, 150, 270, 200),
                        _geoMenuCategory switch
                        {
                            0 => "Solid;Slope;Platform;Glass",
                            1 => "Vertical Pole;Horizontal Pole;Cracked Terrain",
                            2 => "Bat Hive;Forbid Fly Chains;Waterfall;Worm Grass;Place Rock;Place Spear",
                            3 => "Shortcut Entrance;Shortcut Path;Room Entrance;Dragon Den;Wack-a-mole Hole;Scavenger Hole;Garbage Worm Hole",
                            _ => ""
                        },
                        scrollIndex,
                        _geoMenuIndex
                    );

                    if (newGeoMenuIndex != _geoMenuIndex && newGeoMenuIndex != -1)
                    {
                        #if DEBUG
                        _logger.Debug($"New geo menu index: {newGeoMenuIndex}");
                        #endif
                        
                        _geoMenuCategory = newGeoMenuIndex;
                    }
                }
            }
            

            _allowMultiSelect = RayGui.GuiCheckBox(new(panelRect.X + 10, 360, 15, 15), "Multi-Select", _allowMultiSelect);
            _eraseMode = RayGui.GuiCheckBox(new(panelRect.X + 10, 380, 15, 15), "Erase Mode", _eraseMode);
            _eraseAllMode = RayGui.GuiCheckBox(new(panelRect.X + 10, 400, 15, 15), "Erase Everything", _eraseAllMode);

            _showLayer1 = RayGui.GuiCheckBox(new(panelRect.X + 145, 360, 15, 15), "Layer 1", _showLayer1);
            _showLayer2 = RayGui.GuiCheckBox(new(panelRect.X + 145, 380, 15, 15), "Layer 2", _showLayer2);
            _showLayer3 = RayGui.GuiCheckBox(new(panelRect.X + 145, 400, 15, 15), "Layer 3", _showLayer3);

            // Current layer indicator

            DrawRectangle(
                10, sHeight - 50, 40, 40,
                WHITE
            );

            DrawRectangleLines(10, sHeight - 50, 40, 40, GRAY);

            if (GLOBALS.Layer == 2) DrawText("3", 26, sHeight - 40, 22, BLACK);

            if (GLOBALS.Layer is 1 or 0)
            {
                DrawRectangle(
                    20, sHeight - 60, 40, 40,
                    WHITE
                );

                DrawRectangleLines(20, sHeight - 60, 40, 40, GRAY);

                if (GLOBALS.Layer == 1) DrawText("2", 35, sHeight - 50, 22, BLACK);
            }

            if (GLOBALS.Layer == 0)
            {
                DrawRectangle(
                    30, sHeight - 70, 40, 40,
                    WHITE
                );

                DrawRectangleLines(
                    30, sHeight - 70, 40, 40, GRAY);

                DrawText("1", 48, sHeight - 60, 22, BLACK);
            }
        }
        EndDrawing();
    }
}