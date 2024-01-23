using System.Numerics;
using System.Text;
using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class GeoEditorPage(Serilog.Core.Logger logger) : IPage
{
    readonly Serilog.Core.Logger logger = logger;
    Camera2D camera = new() { zoom = 1.0f };

    uint geoSelectionX = 0;
    uint geoSelectionY = 0;
    bool multiselect = false;
    int prevCoordsX = 0;
    int prevCoordsY = 0;
    bool gridContrast = false;
    int prevMatrixX = 0;
    int prevMatrixY = 0;
    bool clickTracker = false;
    bool showLayer1 = true;
    bool showLayer2 = true;
    bool showLayer3 = true;

    RunCell[,] savedChunk = new RunCell[0, 0];

    // This is used to display geo names in the geo editor.
    // Do not alter the indices
    static string[] GeoNames => [
        "Solid",
        "air",
        "Slope",
        "",
        "Rectangle wall",
        "Rectangle air",
        "Platform",
        "Move level",
        "Copy to lower level",
        "Crack terrain",
        "Horizontal pole",
        "Vertical pole",
        "Save To Memory",
        "Copy From Memory",
        "Shortcut entrance",
        "Shortcut",
        "Drangon's den",
        "Passage",
        "Hive",
        "Waterfall",
        "Scavenger hole",
        "Wack-a-mole hole",
        "Garbage worm hole",
        "Worm grass",
        "Forbid Fly Chains",
        "Clear All",
        "",
        "",
        "Place Rock",
        "Place Spear",
        "Glass"
    ];
    static ReadOnlySpan<int> GeoMenuIndexToUITexture => [
         0,
        1,
        2,
        -1,
        3,
        4,
        5,
        6,
        13,
        9,
        10,
        11,
        26,
        27,
        14,
        15,
        16,
        17,
        18,
        19,
        20,
        21,
        22,
        23,
        24,
        25,
        -1,
        -1,
        7,
        8,
        12
    ];

    readonly byte[] geoMenuPanelBytes = Encoding.ASCII.GetBytes("Menu");


    public void Draw()
    {
        GLOBALS.PreviousPage = 2;

        var scale = GLOBALS.Scale;

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

        Vector2 mouse = GetScreenToWorld2D(GetMousePosition(), camera);

        //                        v this was done to avoid rounding errors
        int matrixY = mouse.Y < 0 ? -1 : (int)mouse.Y / GLOBALS.Scale;
        int matrixX = mouse.X < 0 ? -1 : (int)mouse.X / GLOBALS.Scale;

        var canDrawGeo = !Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), new(Raylib.GetScreenWidth() - 210, 50, 200, Raylib.GetScreenHeight() - 100));

        // handle geo selection

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_D))
        {
            geoSelectionX = ++geoSelectionX % 4;

            multiselect = false;
            prevCoordsX = -1;
            prevCoordsY = -1;
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_A))
        {
            geoSelectionX = --geoSelectionX % 4;

            multiselect = false;
            prevCoordsX = -1;
            prevCoordsY = -1;
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_W))
        {
            geoSelectionY = (--geoSelectionY) % 8;

            multiselect = false;
            prevCoordsX = -1;
            prevCoordsY = -1;
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
        {
            geoSelectionY = (++geoSelectionY) % 8;

            multiselect = false;
            prevCoordsX = -1;
            prevCoordsY = -1;
        }

        // handle changing layers

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_L))
        {
            GLOBALS.Layer = ++GLOBALS.Layer % 3;
        }

        uint geoIndex = (4 * geoSelectionY) + geoSelectionX;

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_M))
        {
            gridContrast = !gridContrast;
        }

        // handle mouse drag
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
        {
            Vector2 delta = Raylib.GetMouseDelta();
            delta = RayMath.Vector2Scale(delta, -1.0f / camera.zoom);
            camera.target = RayMath.Vector2Add(camera.target, delta);
        }


        // handle zoom
        var wheel = Raylib.GetMouseWheelMove();
        if (wheel != 0)
        {
            Vector2 mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
            camera.offset = Raylib.GetMousePosition();
            camera.target = mouseWorldPosition;
            camera.zoom += wheel * GLOBALS.ZoomIncrement;
            if (camera.zoom < GLOBALS.ZoomIncrement) camera.zoom = GLOBALS.ZoomIncrement;
        }

        // handle placing geo

        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
        {
            if (canDrawGeo && matrixY >= 0 && matrixY < GLOBALS.Level.Height && matrixX >= 0 && matrixX < GLOBALS.Level.Width)
            {
                switch (geoIndex)
                {
                    case 2: // slopebl
                        var cell = GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer];

                        var slope = Utils.GetCorrectSlopeID(Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, matrixX, matrixY, GLOBALS.Layer));

                        if (slope == -1) break;

                        cell.Geo = slope;
                        GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer] = cell;
                        break;

                    case 0: // solid
                    case 1: // air
                    case 6: // platform
                            //case 12: // glass
                        {
                            var cell2 = GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer];

                            cell2.Geo = Utils.GetBlockID(geoIndex);
                            GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer] = cell2;
                        }
                        break;

                    // multi-select: forward to next if-statement
                    case 4:
                    case 5:
                    case 25:
                        break;

                    // stackables
                    //case 7: // rock
                    //case 8: // spear
                    case 9: // crack
                    case 10: // ph
                    case 11: // pv
                    case 14: // entry
                    case 15: // shortcut
                    case 16: // den
                    case 17: // passage
                    case 18: // bathive
                    case 19: // waterfall
                    case 20: // scav
                    case 21: // wac
                    case 22: // garbageworm
                    case 23: // worm
                    case 24: // forbidflychains
                        {
                            if (geoIndex is 17 or 18 or 19 or 20 or 22 or 23 or 24 or 25 or 26 or 27 && GLOBALS.Layer != 0)
                            {
                                break;
                            }

                            if (
                                matrixX * scale < GLOBALS.Level.Border.X ||
                                matrixX * scale >= GLOBALS.Level.Border.width + GLOBALS.Level.Border.X ||
                                matrixY * scale < GLOBALS.Level.Border.Y ||
                                matrixY * scale >= GLOBALS.Level.Border.height + GLOBALS.Level.Border.Y)
                            {
                                break;
                            }

                            var id = Utils.GetStackableID(geoIndex);
                            var cell_ = GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer];

                            var newValue = !cell_.Stackables[id];
                            if (matrixX != prevMatrixX || matrixY != prevMatrixY || !clickTracker)
                            {

                                if (cell_.Stackables[id] != newValue)
                                {
                                    cell_.Stackables[id] = newValue;
                                    if (id == 4) { cell_.Geo = 0; }
                                    GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer] = cell_;

                                }
                            }

                            prevMatrixX = matrixX;
                            prevMatrixY = matrixY;
                        }
                        break;

                    // repetative code down there

                    case 28: // rock
                    case 29: // spear
                        if (GLOBALS.Settings.GeometryEditor.LegacyGeoTools)
                        {
                            if (geoIndex is 17 or 18 or 19 or 20 or 22 or 23 or 24 or 25 or 26 or 27 && GLOBALS.Layer != 0)
                            {
                                break;
                            }

                            if (
                                matrixX * scale < GLOBALS.Level.Border.X ||
                                matrixX * scale >= GLOBALS.Level.Border.width + GLOBALS.Level.Border.X ||
                                matrixY * scale < GLOBALS.Level.Border.Y ||
                                matrixY * scale >= GLOBALS.Level.Border.height + GLOBALS.Level.Border.Y)
                            {
                                break;
                            }

                            var id = Utils.GetStackableID(geoIndex);
                            var cell_ = GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer];

                            var newValue = !cell_.Stackables[id];
                            if (matrixX != prevMatrixX || matrixY != prevMatrixY || !clickTracker)
                            {

                                if (cell_.Stackables[id] != newValue)
                                {
                                    cell_.Stackables[id] = newValue;
                                    if (id == 4) { cell_.Geo = 0; }
                                    GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer] = cell_;

                                }
                            }

                            prevMatrixX = matrixX;
                            prevMatrixY = matrixY;
                        }
                        break;
                    case 30: // glass
                        if (GLOBALS.Settings.GeometryEditor.LegacyGeoTools)
                        {
                            var cell2 = GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer];

                            cell2.Geo = Utils.GetBlockID(geoIndex);
                            GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer] = cell2;
                        }
                        break;

                    case 13: // load from memory
                        for (var x = 0; x < savedChunk.GetLength(1); x++)
                        {
                            for (var y = 0; y < savedChunk.GetLength(0); y++)
                            {
                                var yy = matrixY + y;
                                var xx = matrixX + x;

                                if (xx >= 0 && xx < GLOBALS.Level.Width &&
                                    yy >= 0 && yy < GLOBALS.Level.Height)
                                {
                                    var _cell = savedChunk[y, x];
                                    
                                    var newStackables = new bool[22];
                                    _cell.Stackables.CopyTo(newStackables, 0);
                                    _cell.Stackables = newStackables;

                                    if (_cell.Geo != 0) GLOBALS.Level.GeoMatrix[yy, xx, GLOBALS.Layer].Geo = _cell.Geo;
                                    GLOBALS.Level.GeoMatrix[yy, xx, GLOBALS.Layer].Stackables = _cell.Stackables;
                                }
                            }
                        }
                        break;
                }
            }

            clickTracker = true;
        }

        if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
        {
            clickTracker = false;
        }

        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            if (canDrawGeo && matrixY >= 0 && matrixY < GLOBALS.Level.Height && matrixX >= 0 && matrixX < GLOBALS.Level.Width)
            {
                switch (geoIndex)
                {
                    case 25:
                        multiselect = !multiselect;

                        if (multiselect)
                        {
                            prevCoordsX = matrixX;
                            prevCoordsY = matrixY;
                        }
                        else
                        {
                            int startX,
                                startY,
                                endX,
                                endY;

                            if (matrixX > prevCoordsX)
                            {
                                startX = prevCoordsX;
                                endX = matrixX;
                            }
                            else
                            {
                                startX = matrixX;
                                endX = prevCoordsX;
                            }

                            if (matrixY > prevCoordsY)
                            {
                                startY = prevCoordsY;
                                endY = matrixY;
                            }
                            else
                            {
                                startY = matrixY;
                                endY = prevCoordsY;
                            }

                            for (int y = startY; y <= endY; y++)
                            {
                                for (int x = startX; x <= endX; x++)
                                {
                                    GLOBALS.Level.GeoMatrix[y, x, 0].Geo = 0;
                                    GLOBALS.Level.GeoMatrix[y, x, 1].Geo = 0;
                                    GLOBALS.Level.GeoMatrix[y, x, 2].Geo = 0;
                                }
                            }
                        }
                        break;
                    case 8:
                        multiselect = !multiselect;

                        if (multiselect)
                        {
                            prevCoordsX = matrixX;
                            prevCoordsY = matrixY;
                        }
                        else
                        {
                            int startX,
                                startY,
                                endX,
                                endY;

                            if (matrixX > prevCoordsX)
                            {
                                startX = prevCoordsX;
                                endX = matrixX;
                            }
                            else
                            {
                                startX = matrixX;
                                endX = prevCoordsX;
                            }

                            if (matrixY > prevCoordsY)
                            {
                                startY = prevCoordsY;
                                endY = matrixY;
                            }
                            else
                            {
                                startY = matrixY;
                                endY = prevCoordsY;
                            }

                            if (GLOBALS.Layer is 0 or 1)
                            {
                                for (int y = startY; y <= endY; y++)
                                {
                                    for (int x = startX; x <= endX; x++)
                                    {
                                        GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer + 1].Geo = GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer].Geo;
                                    }
                                }
                            }
                        }
                        break;
                    case 4:
                    case 5:
                        multiselect = !multiselect;

                        if (multiselect)
                        {
                            prevCoordsX = matrixX;
                            prevCoordsY = matrixY;
                        }
                        else
                        {
                            int startX,
                                startY,
                                endX,
                                endY;

                            if (matrixX > prevCoordsX)
                            {
                                startX = prevCoordsX;
                                endX = matrixX;
                            }
                            else
                            {
                                startX = matrixX;
                                endX = prevCoordsX;
                            }

                            if (matrixY > prevCoordsY)
                            {
                                startY = prevCoordsY;
                                endY = matrixY;
                            }
                            else
                            {
                                startY = matrixY;
                                endY = prevCoordsY;
                            }

                            int value = geoIndex == 4 ? 1 : 0;

                            for (int y = startY; y <= endY; y++)
                            {
                                for (int x = startX; x <= endX; x++)
                                {
                                    var cell = GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer];
                                    cell.Geo = value;
                                    GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer] = cell;
                                }
                            }

                            prevCoordsX = -1;
                            prevCoordsY = -1;
                        }
                        break;

                    case 12: // save to memory
                        multiselect = !multiselect;

                        if (multiselect)
                        {
                            prevCoordsX = matrixX;
                            prevCoordsY = matrixY;
                        }
                        else
                        {
                            int startX,
                                startY,
                                endX,
                                endY;

                            if (matrixX > prevCoordsX)
                            {
                                startX = prevCoordsX;
                                endX = matrixX;
                            }
                            else
                            {
                                startX = matrixX;
                                endX = prevCoordsX;
                            }

                            if (matrixY > prevCoordsY)
                            {
                                startY = prevCoordsY;
                                endY = matrixY;
                            }
                            else
                            {
                                startY = matrixY;
                                endY = prevCoordsY;
                            }

                            savedChunk = new RunCell[endY - startY + 1, endX - startX + 1];

                            for (int x = 0; x < savedChunk.GetLength(1); x++)
                            {
                                for (int y = 0; y < savedChunk.GetLength(0); y++)
                                {
                                    var xx = x + startX;
                                    var yy = y + startY;

                                    RunCell _cell = GLOBALS.Level.GeoMatrix[yy, xx, GLOBALS.Layer];
                                    var newStackables = new bool[22];
                                    _cell.Stackables.CopyTo(newStackables, 0);
                                    _cell.Stackables = newStackables;

                                    savedChunk[y, x] = _cell;
                                }
                            }
                        }
                        break;
                }
            }
        }

        BeginDrawing();
        {
            ClearBackground(new Color(120, 120, 120, 255));


            BeginMode2D(camera);
            {
                // geo matrix

                // first layer without stackables

                if (showLayer1)
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
                                DrawTexture(GLOBALS.Textures.GeoBlocks[texture], x * scale, y * scale, GLOBALS.Settings.GeometryEditor.LayerColors.Layer1);
                            }

                            if (!gridContrast) DrawRectangleLinesEx(
                                new(x * scale, y * scale, scale, scale),
                                0.5f,
                                new(255, 255, 255, 100)
                            );
                        }
                    }
                }

                /*if (gridContrast)
                {
                    // the grid
                    RlGl.rlPushMatrix();
                    {
                        RlGl.rlTranslatef(0, 0, -1);
                        RlGl.rlRotatef(90, 1, 0, 0);
                        DrawGrid(matrixHeight > matrixWidth ? matrixHeight : matrixWidth, scale);
                    }
                    RlGl.rlPopMatrix();
                }*/

                // the rest of the layers

                for (int y = 0; y < GLOBALS.Level.Height; y++)
                {
                    for (int x = 0; x < GLOBALS.Level.Width; x++)
                    {
                        for (int z = 1; z < 3; z++)
                        {
                            if (z == 1 && !showLayer2) continue;
                            if (z == 2 && !showLayer3) continue;

                            var cell = GLOBALS.Level.GeoMatrix[y, x, z];

                            var texture = Utils.GetBlockIndex(cell.Geo);

                            if (texture >= 0)
                            {
                                Raylib.DrawTexture(GLOBALS.Textures.GeoBlocks[texture], x * scale, y * scale, z == 1 ? GLOBALS.Settings.GeometryEditor.LayerColors.Layer2 : GLOBALS.Settings.GeometryEditor.LayerColors.Layer3);
                            }

                            if (!gridContrast && !showLayer1 && z == 1) DrawRectangleLinesEx(
                                new(x * scale, y * scale, scale, scale),
                                0.5f,
                                new(255, 255, 255, 100)
                            );

                            if (!gridContrast && !showLayer1 && z == 2) DrawRectangleLinesEx(
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
                                            Raylib.DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], x * scale, y * scale, z == 1 ? GLOBALS.Settings.GeometryEditor.LayerColors.Layer2 : GLOBALS.Settings.GeometryEditor.LayerColors.Layer3);
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
                                            Raylib.DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], x * scale, y * scale, WHITE); // TODO: remove opacity from entrances
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

                if (showLayer1)
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
                                        // TODO: move ph and pv to the back of drawing order
                                        case 1:     // ph
                                        case 2:     // pv
                                            Raylib.DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], x * scale, y * scale, GLOBALS.Settings.GeometryEditor.LayerColors.Layer1);
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


                // load from memory preview
                if (geoIndex == 13)
                {
                    for (int x = 0; x < savedChunk.GetLength(1); x++)
                    {
                        for (int y = 0; y < savedChunk.GetLength(0); y++)
                        {
                            var cell = savedChunk[y, x];

                            var texture = Utils.GetBlockIndex(cell.Geo);

                            if (texture >= 0)
                            {
                                DrawTexture(GLOBALS.Textures.GeoBlocks[texture], (matrixX + x) * scale, (matrixY + y) * scale, new(89, 7, 222, 200));
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
                                            DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], (matrixX + x) * scale, (matrixY + y) * scale, new(89, 7, 222, 200));
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
                                            DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], (matrixX + x) * scale, (matrixY + y) * scale, WHITE); // TODO: remove opacity from entrances
                                            break;

                                        
                                        case 11:    // crack
                                            DrawTexture(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(savedChunk, x, y))],
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

                // the red selection rectangle

                for (int y = 0; y < GLOBALS.Level.Height; y++)
                {
                    for (int x = 0; x < GLOBALS.Level.Width; x++)
                    {
                        if (multiselect)
                        {
                            var XS = matrixX - prevCoordsX;
                            var YS = matrixY - prevCoordsY;
                            var width = Math.Abs(XS == 0 ? 1 : XS + (XS > 0 ? 1 : -1)) * scale;
                            var height = Math.Abs(YS == 0 ? 1 : YS + (YS > 0 ? 1 : -1)) * scale;

                            Rectangle rec = (XS >= 0, YS >= 0) switch
                            {
                                // br
                                (true, true) => new(prevCoordsX * scale, prevCoordsY * scale, width, height),

                                // tr
                                (true, false) => new(prevCoordsX * scale, matrixY * scale, width, height),

                                // bl
                                (false, true) => new(matrixX * scale, prevCoordsY * scale, width, height),

                                // tl
                                (false, false) => new(matrixX * scale, matrixY * scale, width, height)
                            };

                            Raylib.DrawRectangleLinesEx(rec, 2, new(255, 0, 0, 255));

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
                                Raylib.DrawRectangleLinesEx(new(x * scale, y * scale, scale, scale), 2, new(255, 0, 0, 255));
                            }
                        }
                    }
                }

                // the outbound border
                DrawRectangleLinesEx(new(0, 0, GLOBALS.Level.Width * scale, GLOBALS.Level.Height * scale), 2, new(0, 0, 0, 255));

                // the border
                DrawRectangleLinesEx(GLOBALS.Level.Border, camera.zoom < GLOBALS.ZoomIncrement ? 5 : 2, new(255, 255, 255, 255));

                // a lazy way to hide the rest of the grid
                DrawRectangle(GLOBALS.Level.Width * -scale, -3, GLOBALS.Level.Width * scale, GLOBALS.Level.Height * 2 * scale, new(120, 120, 120, 255));
                DrawRectangle(0, GLOBALS.Level.Height * scale, GLOBALS.Level.Width * scale + 2, GLOBALS.Level.Height * scale, new(120, 120, 120, 255));
            }
            EndMode2D();

            // geo menu

            unsafe
            {
                fixed (byte* pt = geoMenuPanelBytes)
                {
                    RayGui.GuiPanel(
                        new(GetScreenWidth() - 210, 50, 200, GetScreenHeight() - 100),
                        (sbyte*)pt
                    );
                }
            }

            for (int w = 0; w < 4; w++)
            {
                for (int h = 0; h < 8; h++)
                {
                    var index = (4 * h) + w;
                    if (index < GeoMenuIndexToUITexture.Length)
                    {
                        var textureIndex = GeoMenuIndexToUITexture[index];

                        // A really bad and lazy solution

                        if (textureIndex is 7 or 8 or 12 && !GLOBALS.Settings.GeometryEditor.LegacyGeoTools)
                        {

                        }
                        else
                        {
                            if (textureIndex != -1)
                            {
                                DrawTexture(
                                    GLOBALS.Textures.GeoMenu[textureIndex],
                                    GetScreenWidth() - 195 + w * GLOBALS.UiScale + 5,
                                    h * GLOBALS.UiScale + 100,
                                    new(0, 0, 0, 255)
                                );
                            }
                        }

                    }

                    if (w == geoSelectionX && h == geoSelectionY)
                        Raylib.DrawRectangleLinesEx(new(Raylib.GetScreenWidth() - 195 + w * GLOBALS.UiScale + 5, h * GLOBALS.UiScale + 100, GLOBALS.UiScale, GLOBALS.UiScale), 2, new(255, 0, 0, 255));
                    else
                        Raylib.DrawRectangleLinesEx(new(Raylib.GetScreenWidth() - 195 + w * GLOBALS.UiScale + 5, h * GLOBALS.UiScale + 100, GLOBALS.UiScale, GLOBALS.UiScale), 1, new(0, 0, 0, 255));
                }
            }

            if (geoIndex < GLOBALS.Textures.GeoMenu.Length && geoIndex is not 28 or 29 or 30) Raylib.DrawText(GeoNames[geoIndex], Raylib.GetScreenWidth() - 190, 8 * GLOBALS.UiScale + 110, 18, new(0, 0, 0, 255));

            switch (GLOBALS.Layer)
            {
                case 0:
                    Raylib.DrawRectangle(Raylib.GetScreenWidth() - 190, 8 * GLOBALS.UiScale + 140, 40, 40, GLOBALS.Settings.GeometryEditor.LayerColors.Layer1);
                    Raylib.DrawText("L1", Raylib.GetScreenWidth() - 182, 8 * GLOBALS.UiScale + 148, 26, new(255, 255, 255, 255));
                    break;
                case 1:
                    Raylib.DrawRectangle(Raylib.GetScreenWidth() - 190, 8 * GLOBALS.UiScale + 140, 40, 40, GLOBALS.Settings.GeometryEditor.LayerColors.Layer2);
                    Raylib.DrawText("L2", Raylib.GetScreenWidth() - 182, 8 * GLOBALS.UiScale + 148, 26, new(255, 255, 255, 255));
                    break;
                case 2:
                    Raylib.DrawRectangle(Raylib.GetScreenWidth() - 190, 8 * GLOBALS.UiScale + 140, 40, 40, GLOBALS.Settings.GeometryEditor.LayerColors.Layer3);
                    Raylib.DrawText("L3", Raylib.GetScreenWidth() - 182, 8 * GLOBALS.UiScale + 148, 26, new(255, 255, 255, 255));
                    break;
            }

            if (matrixX >= 0 && matrixX < GLOBALS.Level.Width && matrixY >= 0 && matrixY < GLOBALS.Level.Height)
                Raylib.DrawText(
                    $"X = {matrixX:0}\nY = {matrixY:0}",
                    Raylib.GetScreenWidth() - 195,
                    Raylib.GetScreenHeight() - 100,
                    12,
                    new(0, 0, 0, 255));

            else Raylib.DrawText(
                    $"X = -\nY = -",
                    Raylib.GetScreenWidth() - 195,
                    Raylib.GetScreenHeight() - 100,
                    12,
                    new(0, 0, 0, 255));

            showLayer1 = RayGui.GuiCheckBox(
                new(Raylib.GetScreenWidth() - 190, 8 * GLOBALS.UiScale + 190, 20, 20),
                "Layer 1",
                showLayer1
            );

            showLayer2 = RayGui.GuiCheckBox(
                new(Raylib.GetScreenWidth() - 190, 8 * GLOBALS.UiScale + 210, 20, 20),
                "Layer 2",
                showLayer2
            );

            showLayer3 = RayGui.GuiCheckBox(
                new(Raylib.GetScreenWidth() - 190, 8 * GLOBALS.UiScale + 230, 20, 20),
                "Layer 3",
                showLayer3
            );
        }
        Raylib.EndDrawing();

    }
}