﻿using System.Numerics;
using System.Text;
using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class GeoEditorPage(Serilog.Core.Logger logger) : IPage
{
    private readonly Serilog.Core.Logger _logger = logger;
    private Camera2D camera = new() { zoom = 1.0f };

    private readonly GeoShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.GeoEditor;
    private readonly GlobalShortcuts _gShortcuts = GLOBALS.Settings.Shortcuts.GlobalShortcuts;

    private readonly List<GeoGram.CellAction> _groupedActions = [];

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

    // Layers 2 and 3 do not show geo features like shortcuts and entrances 
    private readonly bool[] _layerStackableFilter =
    [
        false, 
        true, 
        true, 
        true, 
        false, // 5
        false, // 6
        false, // 7
        true, 
        false, // 9
        false, // 10
        true, 
        false, // 12
        false, // 13
        true, 
        true, 
        true, 
        true, 
        false, // 18
        false, // 19
        false, // 20
        false, // 21
        true
    ];


    RunCell[,] savedChunk = new RunCell[0, 0];

    private readonly GeoGram _gram = new(40);

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

    private readonly byte[] _geoMenuPanelBytes = Encoding.ASCII.GetBytes("Menu");


    public void Draw()
    {
        GLOBALS.PreviousPage = 2;

        var scale = GLOBALS.Scale;

        var mouse = GetScreenToWorld2D(GetMousePosition(), camera);

        //                        v this was done to avoid rounding errors
        var matrixY = mouse.Y < 0 ? -1 : (int)mouse.Y / GLOBALS.Scale;
        var matrixX = mouse.X < 0 ? -1 : (int)mouse.X / GLOBALS.Scale;

        var canDrawGeo = !CheckCollisionPointRec(GetMousePosition(), new(GetScreenWidth() - 210, 50, 200, Raylib.GetScreenHeight() - 100));

        uint geoIndex = 4*geoSelectionY + geoSelectionX;

        #region Shortcuts
        
        var ctrl = IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);
        var shift = IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
        
        if (_gShortcuts.ToMainPage.Check(ctrl, shift)) GLOBALS.Page = 1;
        // if (_gShortcuts.ToGeometryEditor.Check(ctrl, shift)) GLOBALS.Page = 2;
        if (_gShortcuts.ToTileEditor.Check(ctrl, shift)) GLOBALS.Page = 3;
        if (_gShortcuts.ToCameraEditor.Check(ctrl, shift)) GLOBALS.Page = 4;
        if (_gShortcuts.ToLightEditor.Check(ctrl, shift)) GLOBALS.Page = 5;
        if (_gShortcuts.ToDimensionsEditor.Check(ctrl, shift)) { GLOBALS.ResizeFlag = true; GLOBALS.Page = 6; }
        if (_gShortcuts.ToEffectsEditor.Check(ctrl, shift)) GLOBALS.Page = 7;
        if (_gShortcuts.ToPropsEditor.Check(ctrl, shift)) GLOBALS.Page = 8;
        if (_gShortcuts.ToSettingsPage.Check(ctrl, shift)) GLOBALS.Page = 9;

        if (_shortcuts.CycleLayer.Check(ctrl, shift)) GLOBALS.Layer = ++GLOBALS.Layer % 3;
        if (_shortcuts.ToggleGrid.Check(ctrl, shift)) gridContrast = !gridContrast;
        if (_shortcuts.ShowCameras.Check(ctrl, shift)) GLOBALS.Settings.GeometryEditor.ShowCameras = !GLOBALS.Settings.GeometryEditor.ShowCameras;

        // Menu Navigation
        if (_shortcuts.ToLeftGeo.Check(ctrl, shift))
        {
            geoSelectionX = --geoSelectionX % 4;

            multiselect = false;
            prevCoordsX = -1;
            prevCoordsY = -1;
        }
        else if (_shortcuts.ToTopGeo.Check(ctrl, shift))
        {
            geoSelectionY = (--geoSelectionY) % 8;

            multiselect = false;
            prevCoordsX = -1;
            prevCoordsY = -1;
        }
        else if (_shortcuts.ToRightGeo.Check(ctrl, shift))
        {
            geoSelectionX = ++geoSelectionX % 4;

            multiselect = false;
            prevCoordsX = -1;
            prevCoordsY = -1;
        }
        else if (_shortcuts.ToBottomGeo.Check(ctrl, shift))
        {
            geoSelectionY = (++geoSelectionY) % 8;

            multiselect = false;
            prevCoordsX = -1;
            prevCoordsY = -1;
        }

        // Undo/Redo
        if (_shortcuts.Undo.Check(ctrl, shift))
        {
            var action = _gram.Current;
            switch (action)
            {
                case GeoGram.CellAction c:
                    GLOBALS.Level.GeoMatrix[c.Position.Y, c.Position.X, c.Position.Z] = c.Previous;
                    break;
                case GeoGram.RectAction r:
                    for (var y = 0; y < r.Previous.GetLength(0); y++)
                    {
                        for (var x = 0; x < r.Previous.GetLength(1); x++)
                        {
                            var prevCell = r.Previous[y, x];
                            GLOBALS.Level.GeoMatrix[y + r.Position.Y, x + r.Position.X, r.Position.Z] = prevCell;
                        }
                    }
                    break;

                case GeoGram.GroupAction g:
                    foreach (var cellAction in g.CellActions)
                    {
                        GLOBALS.Level.GeoMatrix[cellAction.Position.Y, cellAction.Position.X, cellAction.Position.Z] = cellAction.Previous;
                    }
                    break;
            }
                
            _gram.Undo();
        }
        if (_shortcuts.Redo.Check(ctrl, shift))
        {
            _gram.Redo();

            switch (_gram.Current)
            {
                case GeoGram.CellAction c:
                    GLOBALS.Level.GeoMatrix[c.Position.Y, c.Position.X, c.Position.Z] = c.Next;
                    break;
                        
                case GeoGram.RectAction r:
                    for (var y = 0; y < r.Previous.GetLength(0); y++)
                    {
                        for (var x = 0; x < r.Previous.GetLength(1); x++)
                        {
                            var nextCell = r.Next[y, x];
                            GLOBALS.Level.GeoMatrix[y + r.Position.Y, x + r.Position.X, r.Position.Z] = nextCell;
                        }
                    }
                    break;
                
                case GeoGram.GroupAction g:
                    foreach (var cellAction in g.CellActions)
                    {
                        GLOBALS.Level.GeoMatrix[cellAction.Position.Y, cellAction.Position.X, cellAction.Position.Z] = cellAction.Next;
                    }
                    break;
            }
        }

        #endregion
        
        // handle mouse drag
        if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
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

        if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
        {
            if (canDrawGeo && matrixY >= 0 && matrixY < GLOBALS.Level.Height && matrixX >= 0 && matrixX < GLOBALS.Level.Width)
            {
                switch (geoIndex)
                {
                    case 2: // slopebl
                    {
                        var cell = GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer];
                        var oldCell = new RunCell { Geo = cell.Geo, Stackables = [ .. cell.Stackables ]};

                        var slope = Utils.GetCorrectSlopeID(Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, matrixX, matrixY, GLOBALS.Layer));

                        if (slope == -1) break;

                        cell.Geo = slope;
                        GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer] = cell;

                        if ((matrixX != prevMatrixX || matrixY != prevMatrixY || !clickTracker) &&
                            oldCell.Geo != cell.Geo)
                        {
                            _gram.Proceed((matrixX, matrixY, GLOBALS.Layer), oldCell, cell);
                        }
                    }
                        break;

                    case 0: // solid
                    case 1: // air
                    case 6: // platform
                            //case 12: // glass
                        {
                            var cell2 = GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer];
                            var oldCell = new RunCell { Geo = cell2.Geo, Stackables = [ .. cell2.Stackables ]};


                            cell2.Geo = Utils.GetBlockID(geoIndex);
                            GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer] = cell2;

                            if ((matrixX != prevMatrixX || matrixY != prevMatrixY || !clickTracker) &&
                                oldCell.Geo != cell2.Geo)
                            {
                                // _gram.Proceed((matrixX, matrixY, GLOBALS.Layer), oldCell, cell2);
                                _groupedActions.Add(new((matrixX, matrixY, GLOBALS.Layer), oldCell, cell2));
                            }
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
                                !GLOBALS.Settings.GeometryEditor.AllowOutboundsPlacement &&
                                (matrixX * scale < GLOBALS.Level.Border.X ||
                                 matrixX * scale >= GLOBALS.Level.Border.width + GLOBALS.Level.Border.X ||
                                 matrixY * scale < GLOBALS.Level.Border.Y ||
                                 matrixY * scale >= GLOBALS.Level.Border.height + GLOBALS.Level.Border.Y)
                                )
                            {
                                break;
                            }

                            var id = Utils.GetStackableID(geoIndex);
                            var cell_ = GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer];
                            var oldCell = new RunCell { Geo = cell_.Geo, Stackables = [..cell_.Stackables] };

                            var newValue = !cell_.Stackables[id];
                            if (matrixX != prevMatrixX || matrixY != prevMatrixY || !clickTracker)
                            {

                                if (cell_.Stackables[id] != newValue)
                                {
                                    cell_.Stackables[id] = newValue;
                                    if (id == 4) { cell_.Geo = 0; }
                                    GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer] = cell_;
                                    // _gram.Proceed((matrixX, matrixY, GLOBALS.Layer), oldCell, cell_);
                                    _groupedActions.Add(new((matrixX, matrixY, GLOBALS.Layer), oldCell, cell_));

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
                            var oldCell = new RunCell { Geo = cell_.Geo, Stackables = [..cell_.Stackables] };

                            var newValue = !cell_.Stackables[id];
                            if (matrixX != prevMatrixX || matrixY != prevMatrixY || !clickTracker)
                            {

                                if (cell_.Stackables[id] != newValue)
                                {
                                    cell_.Stackables[id] = newValue;
                                    if (id == 4) { cell_.Geo = 0; }
                                    GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer] = cell_;
                                    // _gram.Proceed((matrixX, matrixY, GLOBALS.Layer), oldCell, cell_);
                                    _groupedActions.Add(new((matrixX, matrixY, GLOBALS.Layer), oldCell, cell_));

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
                        var newCopy = new RunCell[savedChunk.GetLength(0), savedChunk.GetLength(1)];
                        var oldCopy = new RunCell[savedChunk.GetLength(0), savedChunk.GetLength(1)];
                        
                        for (var x = 0; x < savedChunk.GetLength(1); x++)
                        {
                            for (var y = 0; y < savedChunk.GetLength(0); y++)
                            {
                                var yy = matrixY + y;
                                var xx = matrixX + x;

                                if (xx >= 0 && xx < GLOBALS.Level.Width &&
                                    yy >= 0 && yy < GLOBALS.Level.Height)
                                {
                                    var cell = savedChunk[y, x];
                                    var oldCell = GLOBALS.Level.GeoMatrix[yy, xx, GLOBALS.Layer];
                                    
                                    // Copy memory to new state
                                    newCopy[y, x] = new RunCell { Geo = cell.Geo, Stackables = [..cell.Stackables] };
                                    // Copy level to old state
                                    oldCopy[y, x] = new RunCell { Geo = oldCell.Geo, Stackables = [..oldCell.Stackables] };
                                    
                                    bool[] newStackables = [..cell.Stackables];
                                    cell.Stackables = newStackables;

                                    if (cell.Geo != 0) GLOBALS.Level.GeoMatrix[yy, xx, GLOBALS.Layer].Geo = cell.Geo;
                                    GLOBALS.Level.GeoMatrix[yy, xx, GLOBALS.Layer].Stackables = cell.Stackables;
                                }
                            }
                        }
                        _gram.Proceed((matrixX, matrixY, GLOBALS.Layer), oldCopy, newCopy);
                        break;
                }
            }

            clickTracker = true;
        }

        if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
        {
            clickTracker = false;
            if (_groupedActions.Count != 0) _gram.Proceed([.._groupedActions]);
            _groupedActions.Clear();
        }

        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            if (canDrawGeo && matrixY >= 0 && matrixY < GLOBALS.Level.Height && matrixX >= 0 && matrixX < GLOBALS.Level.Width)
            {
                switch (geoIndex)
                {
                    case 25: // erase everything
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

                            var revWidth = endX - startX + 1;
                            var revHeight = endY - startY + 1;

                            var newCopy = new RunCell[revHeight, revWidth];
                            
                            var oldCopy1 = new RunCell[revHeight, revWidth];
                            var oldCopy2 = new RunCell[revHeight, revWidth];
                            var oldCopy3 = new RunCell[revHeight, revWidth];

                            for (var y = startY; y <= endY; y++)
                            {
                                for (var x = startX; x <= endX; x++)
                                {
                                    newCopy[y - startY, x - startX] = new RunCell { Geo = 0, Stackables = Utils.NewStackables() };
                                    oldCopy1[y - startY, x - startX] = new RunCell { Geo = GLOBALS.Level.GeoMatrix[y, x, 0].Geo, Stackables = [..GLOBALS.Level.GeoMatrix[y, x, 0].Stackables] };
                                    oldCopy2[y - startY, x - startX] = new RunCell { Geo = GLOBALS.Level.GeoMatrix[y, x, 1].Geo, Stackables = [..GLOBALS.Level.GeoMatrix[y, x, 1].Stackables] };
                                    oldCopy3[y - startY, x - startX] = new RunCell { Geo = GLOBALS.Level.GeoMatrix[y, x, 2].Geo, Stackables = [..GLOBALS.Level.GeoMatrix[y, x, 2].Stackables] };
                                    
                                    GLOBALS.Level.GeoMatrix[y, x, 0].Geo = 0;
                                    GLOBALS.Level.GeoMatrix[y, x, 1].Geo = 0;
                                    GLOBALS.Level.GeoMatrix[y, x, 2].Geo = 0;
                                }
                            }
                            
                            _gram.Proceed((startX, startY, 0), oldCopy1, newCopy);
                            _gram.Proceed((startX, startY, 1), oldCopy2, newCopy);
                            _gram.Proceed((startX, startY, 2), oldCopy3, newCopy);
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
                                var newCopy = new RunCell[endY - startY + 1, endX - startX + 1];
                                var oldCopy = new RunCell[endY - startY + 1, endX - startX + 1];
                                
                                for (var y = startY; y <= endY; y++)
                                {
                                    for (var x = startX; x <= endX; x++)
                                    {
                                        oldCopy[y - startY, x - startX] = new RunCell { Geo = GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer + 1].Geo, Stackables = [..GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer + 1].Stackables] };
                                        newCopy[y - startY, x - startX] = new RunCell { Geo = GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer + 1].Geo, Stackables = [..GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer + 1].Stackables]};
                                        
                                        GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer + 1].Geo = GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer].Geo;
                                    }
                                }
                                
                                _gram.Proceed((startX, startY, GLOBALS.Layer+1), oldCopy, newCopy);
                            }
                        }
                        break;
                    case 4:
                    case 5: // solid/air rect
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
                            
                            var newCopy = new RunCell[endY - startY + 1, endX - startX + 1];
                            var oldCopy = new RunCell[endY - startY + 1, endX - startX + 1];

                            for (var y = startY; y <= endY; y++)
                            {
                                for (var x = startX; x <= endX; x++)
                                {
                                    var cell = GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer];
                                    var oldCell = new RunCell { Geo = cell.Geo, Stackables = [..cell.Stackables] };
                                    cell.Geo = value;

                                    oldCopy[y - startY, x - startX] = oldCell;
                                    newCopy[y - startY, x - startX] = new RunCell { Geo = cell.Geo, Stackables = [..cell.Stackables] };
                                    GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer] = cell;
                                }
                            }

                            _gram.Proceed((startX, startY, GLOBALS.Layer), oldCopy, newCopy);
                            
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

                if (showLayer1) Printers.DrawGeoLayer(
                    0,
                    GLOBALS.Scale,
                    !gridContrast,
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer1,
                    true,
                    false
                );

                if (showLayer2)
                    Printers.DrawGeoLayer(
                    1,
                    GLOBALS.Scale,
                    !gridContrast && !showLayer1 && GLOBALS.Layer == 2, 
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer2,
                    _layerStackableFilter
                );
                
                if (showLayer3) Printers.DrawGeoLayer(
                    2, 
                    GLOBALS.Scale,
                    !gridContrast && !showLayer1 && GLOBALS.Layer == 1, 
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer3,
                    _layerStackableFilter
                );

                // draw stackables
                
                if (showLayer1) Printers.DrawGeoLayer(
                    0,
                    GLOBALS.Scale,
                    !gridContrast,
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer1,
                    false,
                    true
                );

                // if (showLayer1)
                // {
                //     for (int y = 0; y < GLOBALS.Level.Height; y++)
                //     {
                //         for (int x = 0; x < GLOBALS.Level.Width; x++)
                //         {
                //             const int z = 0;
                //
                //             var cell = GLOBALS.Level.GeoMatrix[y, x, z];
                //
                //             for (int s = 1; s < cell.Stackables.Length; s++)
                //             {
                //                 if (cell.Stackables[s])
                //                 {
                //                     switch (s)
                //                     {
                //                         // dump placement
                //                         // TODO: move ph and pv to the back of drawing order
                //                         case 1:     // ph
                //                         case 2:     // pv
                //                             Raylib.DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], x * scale, y * scale, GLOBALS.Settings.GeometryEditor.LayerColors.Layer1);
                //                             break;
                //                         case 3:     // bathive
                //                         case 5:     // entrance
                //                         case 6:     // passage
                //                         case 7:     // den
                //                         case 9:     // rock
                //                         case 10:    // spear
                //                         case 12:    // forbidflychains
                //                         case 13:    // garbagewormhole
                //                         case 18:    // waterfall
                //                         case 19:    // wac
                //                         case 20:    // worm
                //                         case 21:    // scav
                //                             Raylib.DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], x * scale, y * scale, new(255, 255, 255, 255)); // TODO: remove opacity from entrances
                //                             break;
                //
                //                         // directional placement
                //                         case 4:     // entrance
                //                             var index = Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, z));
                //
                //                             if (index is 22 or 23 or 24 or 25)
                //                             {
                //                                 GLOBALS.Level.GeoMatrix[y, x, 0].Geo = 7;
                //                             }
                //
                //                             Raylib.DrawTexture(GLOBALS.Textures.GeoStackables[index], x * scale, y * scale, new(255, 255, 255, 255));
                //                             break;
                //                         case 11:    // crack
                //                             Raylib.DrawTexture(
                //                                 GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, z))],
                //                                 x * scale,
                //                                 y * scale,
                //                                 new(255, 255, 255, 255)
                //                             );
                //                             break;
                //                     }
                //                 }
                //             }
                //         }
                //     }
                // }


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

                // the outbound border
                DrawRectangleLinesEx(new(0, 0, GLOBALS.Level.Width * scale, GLOBALS.Level.Height * scale), 2, new(0, 0, 0, 255));

                // the border
                DrawRectangleLinesEx(GLOBALS.Level.Border, camera.zoom < GLOBALS.ZoomIncrement ? 5 : 2, new(255, 255, 255, 255));
                
                // a lazy way to hide the rest of the grid
                DrawRectangle(GLOBALS.Level.Width * -scale, -3, GLOBALS.Level.Width * scale, GLOBALS.Level.Height * 2 * scale, new(120, 120, 120, 255));
                DrawRectangle(0, GLOBALS.Level.Height * scale, GLOBALS.Level.Width * scale + 2, GLOBALS.Level.Height * scale, new(120, 120, 120, 255));
                
                // Draw Cameras

                if (GLOBALS.Settings.GeometryEditor.ShowCameras)
                {
                    foreach (var cam in GLOBALS.Level.Cameras)
                    {
                        DrawRectangleLinesEx(
                            new(cam.Coords.X, cam.Coords.Y, GLOBALS.EditorCameraWidth, GLOBALS.EditorCameraHeight),
                            4f,
                            PINK
                        );
                    }
                }
                
                // the red selection rectangle

                for (var y = 0; y < GLOBALS.Level.Height; y++)
                {
                    for (var x = 0; x < GLOBALS.Level.Width; x++)
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
            }
            EndMode2D();

            // geo menu

            unsafe
            {
                fixed (byte* pt = _geoMenuPanelBytes)
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

            var sWidth = GetScreenWidth();

            switch (GLOBALS.Layer)
            {
                case 0:
                    Raylib.DrawRectangle(sWidth - 190, 8 * GLOBALS.UiScale + 140, 40, 40, GLOBALS.Settings.GeometryEditor.LayerColors.Layer1);
                    Raylib.DrawText("L1", sWidth - 182, 8 * GLOBALS.UiScale + 148, 26, new(255, 255, 255, 255));
                    break;
                case 1:
                    Raylib.DrawRectangle(sWidth - 190, 8 * GLOBALS.UiScale + 140, 40, 40, GLOBALS.Settings.GeometryEditor.LayerColors.Layer2);
                    Raylib.DrawText("L2", sWidth - 182, 8 * GLOBALS.UiScale + 148, 26, new(255, 255, 255, 255));
                    break;
                case 2:
                    Raylib.DrawRectangle(sWidth - 190, 8 * GLOBALS.UiScale + 140, 40, 40, GLOBALS.Settings.GeometryEditor.LayerColors.Layer3);
                    Raylib.DrawText("L3", sWidth - 182, 8 * GLOBALS.UiScale + 148, 26, new(255, 255, 255, 255));
                    break;
            }

            if (matrixX >= 0 && matrixX < GLOBALS.Level.Width && matrixY >= 0 && matrixY < GLOBALS.Level.Height)
                Raylib.DrawText(
                    $"X = {matrixX:0}\nY = {matrixY:0}",
                    sWidth - 195,
                    sWidth - 100,
                    12,
                    new(0, 0, 0, 255));

            else Raylib.DrawText(
                    $"X = -\nY = -",
                    sWidth - 195,
                    sWidth - 100,
                    12,
                    new(0, 0, 0, 255));

            showLayer1 = RayGui.GuiCheckBox(
                new(sWidth - 190, 8 * GLOBALS.UiScale + 190, 20, 20),
                "Layer 1",
                showLayer1
            );

            showLayer2 = RayGui.GuiCheckBox(
                new(sWidth - 190, 8 * GLOBALS.UiScale + 210, 20, 20),
                "Layer 2",
                showLayer2
            );

            showLayer3 = RayGui.GuiCheckBox(
                new(sWidth - 190, 8 * GLOBALS.UiScale + 230, 20, 20),
                "Layer 3",
                showLayer3
            );

            GLOBALS.Settings.GeometryEditor.ShowCameras = RayGui.GuiCheckBox(
                new (sWidth - 190, 8 * GLOBALS.UiScale + 270, 20, 20),
                "Show Cameras",
                GLOBALS.Settings.GeometryEditor.ShowCameras
            );
        }
        Raylib.EndDrawing();

    }
}