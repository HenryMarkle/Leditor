using System.Text;
using ImGuiNET;
using rlImGui_cs;
using static Raylib_cs.Raylib;

namespace Leditor.Pages;

internal class GeoEditorPage : EditorPage
{
    private Camera2D _camera = new() { Zoom = 1.0f };

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
        "Dragon's den",
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
    private static ReadOnlySpan<int> GeoMenuIndexToUiTexture => [
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

    private bool _isShortcutsWinHovered;
    private bool _isShortcutsWinDragged;


    public override void Draw()
    {
        GLOBALS.PreviousPage = 2;
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera;

        var scale = GLOBALS.Scale;

        var uiMouse = GetMousePosition();
        var mouse = GetScreenToWorld2D(uiMouse, _camera);

        //                        v this was done to avoid rounding errors
        var matrixY = mouse.Y < 0 ? -1 : (int)mouse.Y / GLOBALS.Scale;
        var matrixX = mouse.X < 0 ? -1 : (int)mouse.X / GLOBALS.Scale;


        uint geoIndex = 4*geoSelectionY + geoSelectionX;
        
        var sWidth = GetScreenWidth();
        var sHeight = GetScreenHeight();
        
        var layer1Rect = new Rectangle(30, sHeight - 70, 40, 40);
        var layer2Rect = new Rectangle(20, sHeight - 60, 40, 40);
        var layer3Rect = new Rectangle(10, sHeight - 50, 40, 40);

        var canDrawGeo = !_isShortcutsWinHovered && 
                         !_isShortcutsWinDragged && 
                         !CheckCollisionPointRec(uiMouse, layer3Rect) && 
                         (GLOBALS.Layer != 1 || !CheckCollisionPointRec(uiMouse, layer2Rect)) &&
                         (GLOBALS.Layer != 0 || !CheckCollisionPointRec(uiMouse, layer1Rect)) &&
                         !CheckCollisionPointRec(GetMousePosition(), new(GetScreenWidth() - 210, 50, 200, GetScreenHeight() - 100));
        
        
        var ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
        var shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
        var alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);
        
        if (_gShortcuts.ToMainPage.Check(ctrl, shift, alt))
        {
            #if DEBUG
            Logger.Debug($"Going to page 1");
            #endif
            GLOBALS.Page = 1;
        }
        // if (_gShortcuts.ToGeometryEditor.Check(ctrl, shift)) GLOBALS.Page = 2;
        if (_gShortcuts.ToTileEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            Logger.Debug($"Going to page 3");
            #endif
            GLOBALS.Page = 3;
        }
        if (_gShortcuts.ToCameraEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            Logger.Debug($"Going to page 4");
            #endif
            GLOBALS.Page = 4;
        }
        if (_gShortcuts.ToLightEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            Logger.Debug($"Going to page 5");
            #endif
            GLOBALS.Page = 5;
        }

        if (_gShortcuts.ToDimensionsEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            Logger.Debug($"Going to page 6");
            #endif
            GLOBALS.NewFlag = false;
            GLOBALS.ResizeFlag = true; 
            GLOBALS.Page = 6;
        }
        if (_gShortcuts.ToEffectsEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            Logger.Debug($"Going to page 7");
            #endif
            GLOBALS.Page = 7;
        }
        if (_gShortcuts.ToPropsEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            Logger.Debug($"Going to page 8");
            #endif
            GLOBALS.Page = 8;
        }
        if (_gShortcuts.ToSettingsPage.Check(ctrl, shift, alt))
        {
            #if DEBUG
            Logger.Debug($"Going to page 9");
            #endif
            GLOBALS.Page = 9;
        }

        if (_shortcuts.CycleLayers.Check(ctrl, shift, alt)) GLOBALS.Layer = ++GLOBALS.Layer % 3;
        if (_shortcuts.ToggleGrid.Check(ctrl, shift, alt)) gridContrast = !gridContrast;
        if (_shortcuts.ShowCameras.Check(ctrl, shift, alt)) GLOBALS.Settings.GeometryEditor.ShowCameras = !GLOBALS.Settings.GeometryEditor.ShowCameras;

        // Menu Navigation
        if (_shortcuts.ToLeftGeo.Check(ctrl, shift, alt))
        {
            geoSelectionX = --geoSelectionX % 4;

            multiselect = false;
            prevCoordsX = -1;
            prevCoordsY = -1;
        }
        else if (_shortcuts.ToTopGeo.Check(ctrl, shift, alt))
        {
            geoSelectionY = --geoSelectionY % 8;

            multiselect = false;
            prevCoordsX = -1;
            prevCoordsY = -1;
        }
        else if (_shortcuts.ToRightGeo.Check(ctrl, shift, alt))
        {
            geoSelectionX = ++geoSelectionX % 4;

            multiselect = false;
            prevCoordsX = -1;
            prevCoordsY = -1;
        }
        else if (_shortcuts.ToBottomGeo.Check(ctrl, shift, alt))
        {
            geoSelectionY = (++geoSelectionY) % 8;

            multiselect = false;
            prevCoordsX = -1;
            prevCoordsY = -1;
        }

        // Undo/Redo
        if (_shortcuts.Undo.Check(ctrl, shift, alt))
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
                        var my = y + r.Position.Y;
                        if (my >= 0 && my < GLOBALS.Level.Height) {
                            for (var x = 0; x < r.Previous.GetLength(1); x++)
                            {
                                var mx = x + r.Position.X;
                                if (mx >= 0 && mx < GLOBALS.Level.Width) {
                                    var prevCell = r.Previous[y, x];
                                    
                                    GLOBALS.Level.GeoMatrix[my, mx, r.Position.Z].Stackables = prevCell.Stackables;
                                    if (prevCell.Geo != 0 || r.FillAir) GLOBALS.Level.GeoMatrix[my, mx, r.Position.Z].Geo = prevCell.Geo;
                                }

                            }
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
        if (_shortcuts.Redo.Check(ctrl, shift, alt))
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
                        var my = y + r.Position.Y;
                        if (my >= 0 && my < GLOBALS.Level.Height) {
                            for (var x = 0; x < r.Previous.GetLength(1); x++)
                            {
                                var mx = x + r.Position.X;

                                if (mx >= 0 && mx < GLOBALS.Level.Width) {    
                                    var nextCell = r.Next[y, x];
                                    GLOBALS.Level.GeoMatrix[my, mx, r.Position.Z].Stackables = nextCell.Stackables;
                                    if (nextCell.Geo != 0 || r.FillAir) GLOBALS.Level.GeoMatrix[my, mx, r.Position.Z].Geo = nextCell.Geo;
                                }

                            }
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

        // handle mouse drag
        if (_shortcuts.DragLevel.Check(ctrl, shift, alt, true))
        {
            var delta = GetMouseDelta();
            delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
            _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
        }


        // handle zoom
        var wheel = GetMouseWheelMove();
        if (wheel != 0)
        {
            var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
            _camera.Offset = GetMousePosition();
            _camera.Target = mouseWorldPosition;
            _camera.Zoom += wheel * GLOBALS.ZoomIncrement;
            if (_camera.Zoom < GLOBALS.ZoomIncrement) _camera.Zoom = GLOBALS.ZoomIncrement;
        }

        // handle placing geo

        if (_shortcuts.Draw.Check(ctrl, shift, alt, true))
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
                                 matrixX * scale >= GLOBALS.Level.Border.Width + GLOBALS.Level.Border.X ||
                                 matrixY * scale < GLOBALS.Level.Border.Y ||
                                 matrixY * scale >= GLOBALS.Level.Border.Height + GLOBALS.Level.Border.Y)
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
                                    
                                    if (id == 4)
                                    {
                                        /*var context = Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width,
                                            GLOBALS.Level.Height, matrixY, matrixX, GLOBALS.Layer);
                                        var isConnected = Utils.IsConnectionEntranceConnected(context);*/
                                                
                                        cell_.Geo = 0;
                                    }
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
                                matrixX * scale >= GLOBALS.Level.Border.Width + GLOBALS.Level.Border.X ||
                                matrixY * scale < GLOBALS.Level.Border.Y ||
                                matrixY * scale >= GLOBALS.Level.Border.Height + GLOBALS.Level.Border.Y)
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
                        
                        break;
                }
            }

            clickTracker = true;
        }

        if (_shortcuts.Draw.Check(ctrl, shift, alt) && canDrawGeo && matrixY >= 0 && matrixY < GLOBALS.Level.Height && matrixX >= 0 && matrixX < GLOBALS.Level.Width){
            switch (geoIndex) {
                case 13:
                {
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
                        _gram.Proceed((matrixX, matrixY, GLOBALS.Layer), oldCopy, newCopy, false);
                }
                    break;
            }
            
        }

        if (IsMouseButtonReleased(_shortcuts.Draw.Button))
        {
            clickTracker = false;
            if (_groupedActions.Count != 0) _gram.Proceed([.._groupedActions]);
            _groupedActions.Clear();
        }

        if (IsMouseButtonPressed(_shortcuts.Draw.Button))
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
                    case 8: // back copy
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

                            var value = geoIndex == 4 ? 1 : 0;
                            
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
                            

                            for (var x = 0; x < savedChunk.GetLength(1); x++)
                            {
                                for (var y = 0; y < savedChunk.GetLength(0); y++)
                                {
                                    var xx = x + startX;
                                    var yy = y + startY;

                                    var _cell = GLOBALS.Level.GeoMatrix[yy, xx, GLOBALS.Layer];
                                    var newStackables = new bool[22];
                                    _cell.Stackables.CopyTo(newStackables, 0);
                                    _cell.Stackables = newStackables;

                                    savedChunk[y, x] = _cell;
                                }
                            }

                            prevCoordsX = -1;
                            prevCoordsY = -1;
                        }
                        break;
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

                if (showLayer1) Printers.DrawGeoLayer(
                    0,
                    GLOBALS.Scale,
                    false,
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer1,
                    true,
                    false
                );

                if (showLayer2)
                    Printers.DrawGeoLayer(
                    1,
                    GLOBALS.Scale,
                    false, 
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer2,
                    _layerStackableFilter
                );
                
                if (showLayer3) Printers.DrawGeoLayer(
                    2, 
                    GLOBALS.Scale,
                    false, 
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer3,
                    _layerStackableFilter
                );
                
                // Water

                if (GLOBALS.Level.WaterLevel > -1)
                {
                    DrawRectangle(
                        0,
                        (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel - GLOBALS.Level.Padding.bottom) * GLOBALS.Scale,
                        GLOBALS.Level.Width*GLOBALS.Scale,
                        (GLOBALS.Level.WaterLevel + GLOBALS.Level.Padding.bottom) * GLOBALS.Scale,
                        GLOBALS.Settings.GeometryEditor.WaterColor
                    );
                }

                // draw stackables
                
                if (showLayer1) Printers.DrawGeoLayer(
                    0,
                    GLOBALS.Scale,
                    false,
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer1,
                    false,
                    true
                );

                // load from memory preview
                if (geoIndex == 13)
                {
                    for (var x = 0; x < savedChunk.GetLength(1); x++)
                    {
                        for (var y = 0; y < savedChunk.GetLength(0); y++)
                        {
                            var cell = savedChunk[y, x];

                            var texture = Utils.GetBlockIndex(cell.Geo);

                            if (texture >= 0)
                            {
                                DrawTexture(GLOBALS.Textures.GeoBlocks[texture], (matrixX + x) * scale, (matrixY + y) * scale, new(89, 7, 222, 200));
                            }


                            for (var s = 1; s < cell.Stackables.Length; s++)
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
                                            DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], (matrixX + x) * scale, (matrixY + y) * scale, Color.White); // TODO: remove opacity from entrances
                                            break;

                                        
                                        case 11:    // crack
                                            DrawTexture(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(savedChunk, x, y))],
                                                x * scale,
                                                y * scale,
                                                Color.White
                                            );
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                
                Printers.DrawGrid(GLOBALS.Scale);

                // the outbound border
                DrawRectangleLinesEx(new(0, 0, GLOBALS.Level.Width * scale, GLOBALS.Level.Height * scale), 2, new(0, 0, 0, 255));

                // the border
                DrawRectangleLinesEx(GLOBALS.Level.Border, _camera.Zoom < GLOBALS.ZoomIncrement ? 5 : 2, new(255, 255, 255, 255));
                
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
                            Color.Pink
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

            DrawRectangleRec(new(GetScreenWidth() - 210, 50, 200, GetScreenHeight() - 100), Color.White);

            for (var w = 0; w < 4; w++)
            {
                for (var h = 0; h < 8; h++)
                {
                    var index = (4 * h) + w;
                    if (index < GeoMenuIndexToUiTexture.Length)
                    {
                        var textureIndex = GeoMenuIndexToUiTexture[index];

                        // A really bad and lazy solution

                        if (textureIndex is 7 or 8 or 12 && !GLOBALS.Settings.GeometryEditor.LegacyGeoTools)
                        {

                        }
                        else
                        {
                            if (textureIndex != -1)
                            {
                                var toolRect = new Rectangle(
                                    GetScreenWidth() - 195 + w * GLOBALS.UiScale + 5, 
                                    h * GLOBALS.UiScale + 100, 
                                    GLOBALS.UiScale, 
                                    GLOBALS.UiScale
                                );

                                var toolHovered = CheckCollisionPointRec(GetMousePosition(), toolRect);
                                
                                DrawTexture(
                                    GLOBALS.Textures.GeoMenu[textureIndex],
                                    GetScreenWidth() - 195 + w * GLOBALS.UiScale + 5,
                                    h * GLOBALS.UiScale + 100,
                                    new(0, 0, 0, 255)
                                );

                                if (toolHovered)
                                {
                                    DrawRectangleLinesEx(toolRect, 3f, Color.Blue with { A = 100 });

                                    if (IsMouseButtonPressed(MouseButton.Left))
                                    {
                                        geoSelectionX = (uint)w;
                                        geoSelectionY = (uint)h;
                                    }
                                }
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

            // showLayer1 = RayGui.GuiCheckBox(
            //     new(sWidth - 190, 8 * GLOBALS.UiScale + 190, 20, 20),
            //     "Layer 1",
            //     showLayer1
            // );
            //
            // showLayer2 = RayGui.GuiCheckBox(
            //     new(sWidth - 190, 8 * GLOBALS.UiScale + 210, 20, 20),
            //     "Layer 2",
            //     showLayer2
            // );
            //
            // showLayer3 = RayGui.GuiCheckBox(
            //     new(sWidth - 190, 8 * GLOBALS.UiScale + 230, 20, 20),
            //     "Layer 3",
            //     showLayer3
            // );
            //
            // GLOBALS.Settings.GeometryEditor.ShowCameras = RayGui.GuiCheckBox(
            //     new (sWidth - 190, 8 * GLOBALS.UiScale + 270, 20, 20),
            //     "Show Cameras",
            //     GLOBALS.Settings.GeometryEditor.ShowCameras
            // );

            // Layer indicator
            
            var newLayer = GLOBALS.Layer;

            
            var layer3Hovered = GLOBALS.Layer == 2 && CheckCollisionPointRec(GetMousePosition(), layer3Rect);

            if (layer3Hovered)
            {
                DrawRectangleRec(layer3Rect, Color.Blue with { A = 100 });

                if (IsMouseButtonPressed(MouseButton.Left)) newLayer = 0;
            }

            DrawRectangleRec(
                layer3Rect,
                Color.White
            );
            
            DrawRectangleLines(10, sHeight - 50, 40, 40, Color.Gray);

            if (GLOBALS.Layer == 2) DrawText("3", 26, sHeight - 40, 22, Color.Black);
            
            if (GLOBALS.Layer is 1 or 0)
            {
                var layer2Hovered = GLOBALS.Layer == 1 && CheckCollisionPointRec(GetMousePosition(), layer2Rect);

                if (layer2Hovered)
                {
                    DrawRectangleRec(layer2Rect, Color.Blue with { A = 100 });

                    if (IsMouseButtonPressed(MouseButton.Left)) newLayer = 2;
                }
                
                DrawRectangleRec(
                    layer2Rect,
                    Color.White
                );

                DrawRectangleLines(20, sHeight - 60, 40, 40, Color.Gray);

                if (GLOBALS.Layer == 1) DrawText("2", 35, sHeight - 50, 22, Color.Black);
            }

            if (GLOBALS.Layer == 0)
            {
                var layer1Hovered = CheckCollisionPointRec(GetMousePosition(), layer1Rect);

                if (layer1Hovered)
                {
                    DrawRectangleRec(layer1Rect, Color.Blue with { A = 100 });
                    if (IsMouseButtonPressed(MouseButton.Left)) newLayer = 1;
                }
                
                DrawRectangleRec(
                    layer1Rect,
                    Color.White
                );

                DrawRectangleLines(
                    30, sHeight - 70, 40, 40, Color.Gray);

                DrawText("1", 48, sHeight - 60, 22, Color.Black);
            }

            if (newLayer != GLOBALS.Layer) GLOBALS.Layer = newLayer;
        }
        
        // Shortcuts Window

        if (GLOBALS.Settings.GeneralSettings.ShortcutWindow)
        {
            rlImGui.Begin();
            
            ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
            
            var shortcutWindowRect = Printers.ImGui.ShortcutsWindow(GLOBALS.Settings.Shortcuts.GeoEditor);
            
            _isShortcutsWinHovered = CheckCollisionPointRec(
                uiMouse, 
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
            rlImGui.End();
        }
        EndDrawing();

        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera;
    }
}