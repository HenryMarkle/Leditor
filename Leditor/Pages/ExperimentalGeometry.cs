using static Raylib_cs.Raylib;

using System.Numerics;
using ImGuiNET;
using rlImGui_cs;

namespace Leditor.Pages;

internal class ExperimentalGeometryPage : EditorPage
{
    private readonly ExperimentalGeoShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts;

    private Camera2D _camera = new() { Zoom = 1.0f };

    private bool _multiselect;
    private bool _hideGrid;
    private bool _clickTracker;
    
    private bool _showLayer1 = true;
    private bool _showLayer2 = true;
    private bool _showLayer3 = true;

    private bool _eraseMode;
    private bool _eraseAllMode;
    private bool _allowMultiSelect = true;
    
    private int _prevCoordsX = -1;
    private int _prevCoordsY = -1;

    private int _geoMenuScrollIndex;
    private int _geoMenuCategory;
    private int _geoMenuIndex;

    private readonly byte[] _geoMenuPanelBytes = "Menu"u8.ToArray();
    
    private static readonly int[] GeoMenuIndexMaxCount = [4, 3, 6, 7];
    private static readonly int[] GeoMenuIndexToBlockId = [1, 2, 6, 9];
    private static readonly int[] GeoMenuCategory2ToStackableId = [2, 1, 11];
    private static readonly int[] GeoMenuCategory3ToStackableId = [3, 12, 18, 20, 9, 10];
    private static readonly int[] GeoMenuCategory4ToStackableId = [4, 5, 6, 7, 19, 21, 13];
    
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
    
    private RunCell[,] _savedChunk = new RunCell[0, 0];
    
    private bool _memDumbMode;
    private bool _memLoadMode;

    private readonly List<GeoGram.CellAction> _groupedActions = [];
    private readonly GeoGram _gram = new(40);
    
    private bool _isShortcutsWinHovered;
    private bool _isShortcutsWinDragged;

    private bool _isNavigationWinHovered;
    private bool _isNavigationWinDragged;
    
    private bool _isSettingsWinHovered;
    private bool _isSettingsWinDragged;

    private bool _isMenuWinHovered;
    private bool _isMenuWinDragged;
    
    private int _lastChangingMatrixX = -1;
    private int _lastChangingMatrixY = -1;

    private string _selectionSizeString = "";
    private Rectangle _selectionRectangle = new(0, 0, 0, 0);
    
    public override void Draw()
    {
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera;
        
        var ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
        var shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
        var alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);
        
        GLOBALS.PreviousPage = 2;
        var scale = GLOBALS.Scale;
        var settings = GLOBALS.Settings;

        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToMainPage.Check(ctrl, shift, alt)) GLOBALS.Page = 1;
        // if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToGeometryEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 2;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToTileEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 3;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToCameraEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 4;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToLightEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 5;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToDimensionsEditor.Check(ctrl, shift, alt))
        {
            GLOBALS.ResizeFlag = true;
            GLOBALS.NewFlag = false;
            GLOBALS.Page = 6;
            Logger.Debug("go from GLOBALS.Page 2 to GLOBALS.Page 6");
        }
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToEffectsEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 7;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToPropsEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 8;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToSettingsPage.Check(ctrl, shift, alt)) GLOBALS.Page = 9;

        var uiMouse = GetMousePosition();
        var mouse = GetScreenToWorld2D(uiMouse, _camera);

        //                        v this was done to avoid rounding errors
        var matrixY = mouse.Y < 0 ? -1 : (int)mouse.Y / scale;
        var matrixX = mouse.X < 0 ? -1 : (int)mouse.X / scale;

        var sWidth = GetScreenWidth();
        var sHeight = GetScreenHeight();
        
        var layer3Rect = new Rectangle(10, sHeight - 50, 40, 40);
        var layer2Rect = new Rectangle(20, sHeight - 60, 40, 40);
        var layer1Rect = new Rectangle(30, sHeight - 70, 40, 40);

        var toggleCameraRect = new Rectangle(90, sHeight - 60, 50, 50);
        var toggleCameraHovered = CheckCollisionPointRec(uiMouse, toggleCameraRect);

        var canDrawGeo = !toggleCameraHovered &&
                         !_isMenuWinHovered &&
                         !_isMenuWinDragged &&
                         !_isSettingsWinHovered && 
                         !_isSettingsWinDragged &&
                         !_isShortcutsWinHovered && 
                         !_isShortcutsWinDragged && 
                         !_isNavigationWinHovered &&
                         !_isNavigationWinDragged &&
                         !CheckCollisionPointRec(uiMouse, layer3Rect) &&
                         (GLOBALS.Layer != 1 || !CheckCollisionPointRec(uiMouse, layer2Rect)) &&
                         (GLOBALS.Layer != 0 || !CheckCollisionPointRec(uiMouse, layer1Rect));
        
        var inMatrixBounds = matrixY >= 0 && matrixY < GLOBALS.Level.Height && matrixX >= 0 &&
                             matrixX < GLOBALS.Level.Width;

        // handle geo selection

        if (_shortcuts.ToLeftGeo.Check(ctrl, shift, alt))
        {
            _geoMenuCategory--;
            if (_geoMenuCategory < 0) _geoMenuCategory = 3;
            Utils.Restrict(ref _geoMenuIndex, 0, GeoMenuIndexMaxCount[_geoMenuCategory]-1);
        }

        if (_shortcuts.ToRightGeo.Check(ctrl, shift, alt))
        {
            _geoMenuCategory = ++_geoMenuCategory % 4;
            
            Utils.Restrict(ref _geoMenuIndex, 0, GeoMenuIndexMaxCount[_geoMenuCategory]-1);
        }

        if (_shortcuts.ToTopGeo.Check(ctrl, shift, alt))
        {
            _geoMenuIndex--;
            if (_geoMenuIndex < 0) _geoMenuIndex = GeoMenuIndexMaxCount[_geoMenuCategory] - 1;
        }

        if (_shortcuts.ToBottomGeo.Check(ctrl, shift, alt))
        {
            _geoMenuIndex = ++_geoMenuIndex % GeoMenuIndexMaxCount[_geoMenuCategory];
        }

        if (_shortcuts.ToggleMultiSelect.Check(ctrl, shift, alt)) _allowMultiSelect = !_allowMultiSelect;

        _eraseAllMode = _shortcuts.EraseEverything.Check(ctrl, shift, alt, true);

        // handle changing layers

        if (_shortcuts.CycleLayers.Check(ctrl, shift, alt))
        {
            GLOBALS.Layer = ++GLOBALS.Layer % 3;
        }

        if (_shortcuts.ToggleGrid.Check(ctrl, shift, alt))
        {
            _hideGrid = !_hideGrid;
        }

        // handle mouse drag
        if (_shortcuts.DragLevel.Check(ctrl, shift, alt, true) || _shortcuts.AltDragLevel.Check(ctrl, shift, alt, true))
        {
            var delta = GetMouseDelta();
            delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
            _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
        }
        
        // handle zoom
        var wheel = GetMouseWheelMove();
        if (wheel != 0 && canDrawGeo)
        {
            var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
            _camera.Offset = GetMousePosition();
            _camera.Target = mouseWorldPosition;
            _camera.Zoom += wheel * GLOBALS.ZoomIncrement;
            if (_camera.Zoom < GLOBALS.ZoomIncrement) _camera.Zoom = GLOBALS.ZoomIncrement;
        }
        
        // Undo/Redo

        if (_shortcuts.Undo.Check(ctrl, shift, alt))
        {
            var action = _gram.Current;
            switch (action)
            {
                case GeoGram.CellAction c:
                    if (c.Position.X < 0 || c.Position.X >= GLOBALS.Level.Width ||
                        c.Position.Y < 0 || c.Position.Y >= GLOBALS.Level.Height) break;
                    
                    GLOBALS.Level.GeoMatrix[c.Position.Y, c.Position.X, c.Position.Z] = c.Previous;
                    break;
                case GeoGram.RectAction r:
                    for (var y = 0; y < r.Previous.GetLength(0); y++)
                    {
                        for (var x = 0; x < r.Previous.GetLength(1); x++)
                        {
                            if (x + r.Position.X < 0 || x + r.Position.X >= GLOBALS.Level.Width ||
                                y + r.Position.Y < 0 || y + r.Position.Y >= GLOBALS.Level.Height) continue;
                            
                            var prevCell = r.Previous[y, x];
                            GLOBALS.Level.GeoMatrix[y + r.Position.Y, x + r.Position.X, r.Position.Z] = prevCell;
                        }
                    }
                    break;

                case GeoGram.GroupAction g:
                    foreach (var cellAction in g.CellActions)
                    {
                        if (cellAction.Position.X < 0 || cellAction.Position.X >= GLOBALS.Level.Width ||
                            cellAction.Position.Y < 0 || cellAction.Position.Y >= GLOBALS.Level.Height) continue;
                        
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
                        for (var x = 0; x < r.Previous.GetLength(1); x++)
                        {
                            if (x + r.Position.X < 0 || x + r.Position.X >= GLOBALS.Level.Width ||
                                y + r.Position.Y < 0 || y + r.Position.Y >= GLOBALS.Level.Height) continue;
                            
                            var nextCell = r.Next[y, x];
                            GLOBALS.Level.GeoMatrix[y + r.Position.Y, x + r.Position.X, r.Position.Z] = nextCell;
                        }
                    }
                    break;
                
                case GeoGram.GroupAction g:
                    foreach (var cellAction in g.CellActions)
                    {
                        var (x, y) = cellAction.Position;

                        if (x < 0 || x >= GLOBALS.Level.Width || y < 0 || y >= GLOBALS.Level.Height) continue;
                        
                        GLOBALS.Level.GeoMatrix[cellAction.Position.Y, cellAction.Position.X, cellAction.Position.Z] = cellAction.Next;
                    }
                    break;
            }
        }
        
        // Show/Hide Cameras

        if (_shortcuts.ShowCameras.Check(ctrl, shift, alt))
            GLOBALS.Settings.GeometryEditor.ShowCameras = !GLOBALS.Settings.GeometryEditor.ShowCameras;

        if (_shortcuts.ToggleMemoryLoadMode.Check(ctrl, shift, alt)) _memLoadMode = !_memLoadMode;
        if (_shortcuts.ToggleMemoryDumbMode.Check(ctrl, shift, alt)) _memDumbMode = !_memDumbMode;
        
        // Show/Hide tiles

        if (_shortcuts.ToggleTileVisibility.Check(ctrl, shift, alt))
            GLOBALS.Settings.GeometryEditor.ShowTiles = !GLOBALS.Settings.GeometryEditor.ShowTiles;

        // multi-place/erase geos

        if (_allowMultiSelect)
        {
            if ((_shortcuts.Draw.Check(ctrl, shift, alt, true) || _shortcuts.AltDraw.Check(ctrl, shift, alt, true)) && !_clickTracker)
            {
                if (canDrawGeo && matrixY >= 0 && matrixY < GLOBALS.Level.Height && matrixX >= 0 && matrixX < GLOBALS.Level.Width)
                {
                    _clickTracker = true;
                    _multiselect = true;

                    _prevCoordsX = matrixX;
                    _prevCoordsY = matrixY;

                    _eraseMode = false;
                }
            }
            else if ((_shortcuts.Erase.Check(ctrl, shift, alt, true) || _shortcuts.AltErase.Check(ctrl, shift, alt, true)) && canDrawGeo && !_clickTracker)
            {
                if (canDrawGeo && matrixY >= 0 && matrixY < GLOBALS.Level.Height && matrixX >= 0 && matrixX < GLOBALS.Level.Width)
                {
                    _clickTracker = true;
                    _multiselect = true;

                    _prevCoordsX = matrixX;
                    _prevCoordsY = matrixY;

                    _eraseMode = true;
                }
            }

            if ((IsMouseButtonReleased(_shortcuts.Draw.Button) || IsKeyReleased(_shortcuts.AltDraw.Key)) && _prevCoordsX != -1)
            {
                _clickTracker = false;
                _eraseMode = false;

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

                if (_memLoadMode)
                {
                    _savedChunk = new RunCell[endY - startY + 1, endX - startX + 1];

                    for (var x = 0; x < _savedChunk.GetLength(1); x++)
                    {
                        for (var y = 0; y < _savedChunk.GetLength(0); y++)
                        {
                            var xx = x + startX;
                            var yy = y + startY;

                            var cell = GLOBALS.Level.GeoMatrix[yy, xx, GLOBALS.Layer];
                            
                            var newStackables = new bool[22];
                            cell.Stackables.CopyTo(newStackables, 0);
                            cell.Stackables = newStackables;

                            _savedChunk[y, x] = cell;
                        }
                    }
                    
                    _memLoadMode = false;
                }
                else if (_memDumbMode)
                {
                    var newCopy = new RunCell[_savedChunk.GetLength(0), _savedChunk.GetLength(1)];
                    var oldCopy = new RunCell[_savedChunk.GetLength(0), _savedChunk.GetLength(1)];
                        
                    for (var x = 0; x < _savedChunk.GetLength(1); x++)
                    {
                        for (var y = 0; y < _savedChunk.GetLength(0); y++)
                        {
                            var yy = matrixY + y - Utils.GetMiddle(_savedChunk.GetLength(0));
                            var xx = matrixX + x - Utils.GetMiddle(_savedChunk.GetLength(1));

                            if (xx >= 0 && xx < GLOBALS.Level.Width &&
                                yy >= 0 && yy < GLOBALS.Level.Height)
                            {
                                var cell = _savedChunk[y, x];

                                ref var mtxCell = ref GLOBALS.Level.GeoMatrix[yy, xx, GLOBALS.Layer];
                                
                                var oldCell = new RunCell { Geo = mtxCell.Geo, Stackables = [..mtxCell.Stackables]};
                                    
                                // Copy memory to new state
                                newCopy[y, x] = new RunCell { Geo = GLOBALS.Settings.GeometryEditor.PasteAir ? cell.Geo : cell.Geo == 0 ? mtxCell.Geo : cell.Geo, Stackables = [..cell.Stackables] };
                                // Copy level to old state
                                oldCopy[y, x] = new RunCell { Geo = oldCell.Geo, Stackables = [..oldCell.Stackables] };
                                    
                                bool[] newStackables = [..cell.Stackables];
                                cell.Stackables = newStackables;

                                if (GLOBALS.Settings.GeometryEditor.PasteAir || cell.Geo != 0) mtxCell.Geo = cell.Geo;
                                mtxCell.Stackables = cell.Stackables;
                            }
                        }
                    }
                    _gram.Proceed((matrixX - Utils.GetMiddle(_savedChunk.GetLength(1)), matrixY  - Utils.GetMiddle(_savedChunk.GetLength(0)), GLOBALS.Layer), oldCopy, newCopy);
                }
                else
                {
                    var newCopy = new RunCell[endY - startY + 1, endX - startX + 1];
                    var oldCopy = new RunCell[endY - startY + 1, endX - startX + 1];

                    for (var y = startY; y <= endY; y++)
                    {
                        for (var x = startX; x <= endX; x++)
                        {
                            if (x < 0 || x >= GLOBALS.Level.Width || y < 0 || y >= GLOBALS.Level.Height) continue;

                            if (_eraseAllMode)
                            {
                                var cell = GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer];
                                var oldCell = new RunCell { Geo = cell.Geo, Stackables = [..cell.Stackables] };

                                cell.Geo = 0;
                                Array.Fill(cell.Stackables, false);
                                
                                oldCopy[y - startY, x - startX] = oldCell;
                                newCopy[y - startY, x - startX] = new RunCell { Geo = cell.Geo, Stackables = [..cell.Stackables] };

                                GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer] = cell;
                            }
                            else
                            {
                                ref var cell = ref GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer];
                                oldCopy[y - startY, x - startX] = new RunCell { Geo = cell.Geo, Stackables = [..cell.Stackables]};
                                
                                switch (_geoMenuCategory)
                                {
                                    case 0:
                                        {

                                            var id = GeoMenuIndexToBlockId[_geoMenuIndex];

                                            // slope
                                            if (id == 2)
                                            {
                                                var slope = Utils.GetCorrectSlopeID(Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, GLOBALS.Layer));
                                                if (slope != -1) cell.Geo = slope;
                                            }
                                            else
                                            {
                                                // solid, platform, glass
                                                cell.Geo = id;
                                            }
                                        }
                                        break;

                                    case 1:
                                        {
                                            var id = GeoMenuCategory2ToStackableId[_geoMenuIndex];
                                            #if DEBUG
                                            try
                                            {
                                                cell.Stackables[id] = !_eraseMode;
                                            }
                                            catch (IndexOutOfRangeException e)
                                            {
                                                throw new IndexOutOfRangeException(innerException: e, message: $"Geo cell at {x} {y} {GLOBALS.Layer} has stackables array that is not initialized correctly");
                                            }
                                            #else
                                            cell.Stackables[id] = !_eraseMode;
                                            #endif
                                        }
                                        break;

                                    case 2:
                                        {
                                            if (
                                                x * scale < GLOBALS.Level.Border.X ||
                                                x * scale >= GLOBALS.Level.Border.Width + GLOBALS.Level.Border.X ||
                                                y * scale < GLOBALS.Level.Border.Y ||
                                                y * scale >= GLOBALS.Level.Border.Height + GLOBALS.Level.Border.Y) break;

                                            if (_geoMenuIndex == 0 && GLOBALS.Layer != 0 && GLOBALS.Layer != 1) break;

                                            var id = GeoMenuCategory3ToStackableId[_geoMenuIndex];
                                            cell.Stackables[id] = true;
                                        }
                                        break;

                                    case 3:
                                        {
                                            if (
                                                x * scale < GLOBALS.Level.Border.X ||
                                                x * scale >= GLOBALS.Level.Border.Width + GLOBALS.Level.Border.X ||
                                                y * scale < GLOBALS.Level.Border.Y ||
                                                y * scale >= GLOBALS.Level.Border.Height + GLOBALS.Level.Border.Y) break;

                                            if (GLOBALS.Layer != 0) break;

                                            var id = GeoMenuCategory4ToStackableId[_geoMenuIndex];
                                            cell.Stackables[id] = true;

                                            /*if (id == 4)
                                            {
                                                /*var context = Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width,
                                                    GLOBALS.Level.Height, x, y, GLOBALS.Layer);
                                                var isConnected = Utils.IsConnectionEntranceConnected(context);#1#

                                                cell.Geo = 7;
                                            }*/
                                        }
                                        break;
                                }
                                
                                newCopy[y - startY, x - startX] = new RunCell { Geo = cell.Geo, Stackables = [..cell.Stackables] };
                            }
                        }
                    }
                    
                    _gram.Proceed((startX, startY, GLOBALS.Layer), oldCopy, newCopy);
                }

                _prevCoordsX = -1;
                _prevCoordsY = -1;
                
                _multiselect = false;
            }
            else if ((IsMouseButtonReleased(_shortcuts.Erase.Button) || IsKeyReleased(_shortcuts.AltErase.Key)) && _prevCoordsX != -1)
            {
                _eraseMode = false;
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
                
                var newCopy = new RunCell[endY - startY + 1, endX - startX + 1];
                var oldCopy = new RunCell[endY - startY + 1, endX - startX + 1];

                for (var y = startY; y <= endY; y++)
                {
                    for (var x = startX; x <= endX; x++)
                    {
                        if (x < 0 || x >= GLOBALS.Level.Width || y < 0 || y >= GLOBALS.Level.Height) continue;

                        if (_eraseAllMode)
                        {
                            var cell = GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer];
                            var oldCell = new RunCell { Geo = cell.Geo, Stackables = [..cell.Stackables] };

                            cell.Geo = 0;
                            Array.Fill(cell.Stackables, false);

                            oldCopy[y - startY, x - startX] = oldCell;
                            newCopy[y - startY, x - startX] = new RunCell { Geo = cell.Geo, Stackables = [..cell.Stackables] };
                            GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer] = cell;
                        }
                        else
                        {
                            ref var cell = ref GLOBALS.Level.GeoMatrix[y, x, GLOBALS.Layer];
                            oldCopy[y - startY, x - startX] = new RunCell { Geo = cell.Geo, Stackables = [..cell.Stackables]};
                            
                            switch (_geoMenuCategory)
                            {
                                case 0:
                                    {
                                        var id = GeoMenuIndexToBlockId[_geoMenuIndex];

                                        // slope
                                        if (id == 2)
                                        {
                                            if (cell.Geo is 2 or 3 or 4 or 5) cell.Geo = 0;
                                        }
                                        else
                                        {
                                            cell.Geo = 0;
                                        }
                                    }
                                    break;

                                case 1:
                                    {
                                        var id = GeoMenuCategory2ToStackableId[_geoMenuIndex];
                                        cell.Stackables[id] = false;
                                    }
                                    break;

                                case 2:
                                    {
                                        if (
                                            x * scale < GLOBALS.Level.Border.X ||
                                            x * scale >= GLOBALS.Level.Border.Width + GLOBALS.Level.Border.X ||
                                            y * scale < GLOBALS.Level.Border.Y ||
                                            y * scale >= GLOBALS.Level.Border.Height + GLOBALS.Level.Border.Y) break;

                                        // if (_geoMenuIndex == 0 && GLOBALS.Layer != 0) break;

                                        var id = GeoMenuCategory3ToStackableId[_geoMenuIndex];
                                        cell.Stackables[id] = false;
                                    }
                                    break;

                                case 3:
                                    {
                                        if (
                                            x * scale < GLOBALS.Level.Border.X ||
                                            x * scale >= GLOBALS.Level.Border.Width + GLOBALS.Level.Border.X ||
                                            y * scale < GLOBALS.Level.Border.Y ||
                                            y * scale >= GLOBALS.Level.Border.Height + GLOBALS.Level.Border.Y) break;

                                        if (GLOBALS.Layer != 0) break;

                                        var id = GeoMenuCategory4ToStackableId[_geoMenuIndex];
                                        cell.Stackables[id] = false;
                                    }
                                    break;
                            }
                            
                            newCopy[y - startY, x - startX] = new RunCell { Geo = cell.Geo, Stackables = [..cell.Stackables] };
                        }
                    }
                }
                
                _gram.Proceed((startX, startY, GLOBALS.Layer), oldCopy, newCopy);

                _prevCoordsX = -1;
                _prevCoordsY = -1;
                
                _multiselect = false;
            }

            if (_clickTracker)
            {
                if (_lastChangingMatrixX != matrixX || _lastChangingMatrixY != matrixY)
                {
                    _lastChangingMatrixX = matrixX;
                    _lastChangingMatrixY = matrixY;
                    
                    // TODO: Performance issue
                    
                    _selectionRectangle = Utils.RecFromTwoVecs(
                        new Vector2(matrixX, matrixY),
                        new Vector2(_prevCoordsX, _prevCoordsY));

                    _selectionRectangle.X *= scale;
                    _selectionRectangle.Y *= scale;
                    _selectionRectangle.Width = _selectionRectangle.Width * scale + scale;
                    _selectionRectangle.Height = _selectionRectangle.Height * scale + scale;

                    _selectionSizeString = $"{_selectionRectangle.Width / scale:0}w {_selectionRectangle.Height / scale:0}h";
                }
            }
            else
            {
                _lastChangingMatrixX = -1;
                _lastChangingMatrixY = -1;
                _selectionRectangle = new Rectangle(0, 0, 0, 0);
            }
        }
        // handle placing geo
        else
        {
            if ((_shortcuts.Erase.Check(ctrl, shift, alt, true) || _shortcuts.AltErase.Check(ctrl, shift, alt, true)) && canDrawGeo && inMatrixBounds)
            {
                _clickTracker = true;
                var cell = GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer];
                
                var oldCell = new RunCell { Geo = cell.Geo, Stackables = [..cell.Stackables] };

                cell.Geo = 0;
                Array.Fill(cell.Stackables, false);
                
                var newAction = new GeoGram.CellAction((matrixX, matrixY, GLOBALS.Layer), oldCell, cell);

                var equalActions = _groupedActions.Count != 0 
                                   && oldCell.Geo == cell.Geo
                                   && Utils.GeoStackEq(oldCell.Stackables, cell.Stackables);
                
                if (_groupedActions.Count == 0 || !equalActions) _groupedActions.Add(newAction);

                // _groupedActions.Add(new((matrixX, matrixY, GLOBALS.Layer), oldCell, cell));
                
                GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer] = cell;
            }
            else if ((_shortcuts.Draw.Check(ctrl, shift, alt, true) || _shortcuts.AltDraw.Check(ctrl, shift, alt, true)) && canDrawGeo && inMatrixBounds)
            {
                _clickTracker = true;
                
                ref var cell = ref GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer];
                
                var oldCell = new RunCell { Geo = cell.Geo, Stackables = [..cell.Stackables] };
                
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
                                    matrixX * scale >= GLOBALS.Level.Border.Width + GLOBALS.Level.Border.X ||
                                    matrixY * scale < GLOBALS.Level.Border.Y ||
                                    matrixY * scale >= GLOBALS.Level.Border.Height + GLOBALS.Level.Border.Y) break;

                                if (_geoMenuIndex == 0 && GLOBALS.Layer != 0 && GLOBALS.Layer != 1) break;

                                var id = GeoMenuCategory3ToStackableId[_geoMenuIndex];
                                GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer].Stackables[id] = !_eraseMode;
                            }
                            break;

                        case 3:
                            {
                                if (
                                    matrixX * scale < GLOBALS.Level.Border.X ||
                                    matrixX * scale >= GLOBALS.Level.Border.Width + GLOBALS.Level.Border.X ||
                                    matrixY * scale < GLOBALS.Level.Border.Y ||
                                    matrixY * scale >= GLOBALS.Level.Border.Height + GLOBALS.Level.Border.Y) break;

                                if (GLOBALS.Layer != 0) break;

                                var id = GeoMenuCategory4ToStackableId[_geoMenuIndex];
                                GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer].Stackables[id] = !_eraseMode;
                            }
                            break;
                    }

                var newAction = new GeoGram.CellAction((matrixX, matrixY, GLOBALS.Layer), oldCell, cell);

                var equalActions = _groupedActions.Count != 0 
                                   && oldCell.Geo == cell.Geo
                                   && Utils.GeoStackEq(oldCell.Stackables, cell.Stackables);
                
                if (_groupedActions.Count == 0 || !equalActions) _groupedActions.Add(newAction);
            }

            if ((IsMouseButtonReleased(_shortcuts.Erase.Button) || IsKeyReleased(_shortcuts.AltErase.Key)) &&
                _clickTracker)
            {
                _clickTracker = false;
                _gram.Proceed([.._groupedActions]);
                _groupedActions.Clear();
            }
            else if ((IsMouseButtonReleased(_shortcuts.Draw.Button) || IsKeyReleased(_shortcuts.AltDraw.Key)) &&
                     _clickTracker)
            {
                _clickTracker = false;
                _gram.Proceed([.._groupedActions]);
                _groupedActions.Clear();
            }
        }


        BeginDrawing();
        {
            ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme 
                ? Color.Black 
                : new Color(120, 120, 120, 255));

            BeginMode2D(_camera);
            {
                DrawRectangle(
                    0, 
                    0, 
                    GLOBALS.Level.Width * GLOBALS.Scale, 
                    GLOBALS.Level.Height * GLOBALS.Scale, 
                    new Color(120, 120, 120, 255)
                );
                
                // geo matrix

                // first layer without stackables

                #region DrawingMatrix

                if (_showLayer1) Printers.DrawGeoLayer(
                    0,
                    GLOBALS.Scale,
                    false,
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer1,
                    true,
                    false
                );
                
                if (GLOBALS.Settings.GeometryEditor.ShowTiles && _showLayer3) Printers.DrawTileLayer(
                    2, 
                    GLOBALS.Scale, 
                    false, 
                    !GLOBALS.Settings.TileEditor.UseTextures,
                    GLOBALS.Settings.TileEditor.TintedTiles
                );

                if (_showLayer2)
                    Printers.DrawGeoLayer(
                    1,
                    GLOBALS.Scale,
                    false, 
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer2,
                    _layerStackableFilter
                );
                
                if (GLOBALS.Settings.GeometryEditor.ShowTiles && _showLayer2) Printers.DrawTileLayer(
                    1, 
                    GLOBALS.Scale, 
                    false, 
                    !GLOBALS.Settings.TileEditor.UseTextures,
                    GLOBALS.Settings.TileEditor.TintedTiles
                );
                
                if (_showLayer3) Printers.DrawGeoLayer(
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
                        (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel) * GLOBALS.Scale,
                        GLOBALS.Level.Width*GLOBALS.Scale,
                        GLOBALS.Level.WaterLevel*GLOBALS.Scale,
                        GLOBALS.Settings.GeometryEditor.WaterColor
                    );
                }

                // draw stackables
                
                if (_showLayer1) Printers.DrawGeoLayer(
                    0,
                    GLOBALS.Scale,
                    false,
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer1,
                    false,
                    true
                );
                
                if (GLOBALS.Settings.GeometryEditor.ShowTiles && _showLayer1) Printers.DrawTileLayer(
                    0, 
                    GLOBALS.Scale, 
                    false, 
                    !GLOBALS.Settings.TileEditor.UseTextures,
                    GLOBALS.Settings.TileEditor.TintedTiles
                );
                
                // Grid

                Printers.DrawGrid(GLOBALS.Scale);
                
                #endregion

                // load from memory preview
                if (_memDumbMode)
                {
                    for (var x = 0; x < _savedChunk.GetLength(1); x++)
                    {
                        for (var y = 0; y < _savedChunk.GetLength(0); y++)
                        {
                            var cell = _savedChunk[y, x];

                            var texture = Utils.GetBlockIndex(cell.Geo);

                            if (texture >= 0)
                            {
                                DrawTexture(
                                    GLOBALS.Textures.GeoBlocks[texture], 
                                    (matrixX + x - Utils.GetMiddle(_savedChunk.GetLength(1))) * scale, 
                                    (matrixY + y - Utils.GetMiddle(_savedChunk.GetLength(0))) * scale, 
                                    new(89, 7, 222, 200)
                                );
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
                                            DrawTexture(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], 
                                                (matrixX + x - Utils.GetMiddle(_savedChunk.GetLength(1))) * scale, 
                                                (matrixY + y - Utils.GetMiddle(_savedChunk.GetLength(0))) * scale, 
                                                new(89, 7, 222, 200)
                                            );
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
                                            DrawTexture(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], 
                                                (matrixX + x - Utils.GetMiddle(_savedChunk.GetLength(1))) * scale, 
                                                (matrixY + y - Utils.GetMiddle(_savedChunk.GetLength(0))) * scale, 
                                                Color.White
                                            ); // TODO: remove opacity from entrances
                                            break;

                                        
                                        case 11:    // crack
                                            DrawTexture(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(_savedChunk, x, y))],
                                                (matrixX + x - Utils.GetMiddle(_savedChunk.GetLength(1))) * scale,
                                                (matrixY + y - Utils.GetMiddle(_savedChunk.GetLength(0))) * scale,
                                                Color.White
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
                DrawRectangleLinesEx(GLOBALS.Level.Border, _camera.Zoom < GLOBALS.ZoomIncrement ? 5 : 2, new(255, 255, 255, 255));
                
                // a lazy way to hide the rest of the grid
                // DrawRectangle(GLOBALS.Level.Width * -scale, -3, GLOBALS.Level.Width * scale, GLOBALS.Level.Height * 2 * scale, new(120, 120, 120, 255));
                // DrawRectangle(0, GLOBALS.Level.Height * scale, GLOBALS.Level.Width * scale + 2, GLOBALS.Level.Height * scale, new(120, 120, 120, 255));

                // the selection rectangle

                for (var y = 0; y < GLOBALS.Level.Height; y++)
                {
                    for (var x = 0; x < GLOBALS.Level.Width; x++)
                    {
                        if (!_multiselect && _selectionRectangle is not { Width: 0, Height: 0 })
                            _selectionRectangle = new Rectangle(0, 0, 0, 0);
                        
                        
                        if (_memDumbMode)
                        {
                            DrawRectangleLinesEx(
                                new Rectangle(
                                    (matrixX - Utils.GetMiddle(_savedChunk.GetLength(1))) * GLOBALS.Scale,
                                    (matrixY - Utils.GetMiddle(_savedChunk.GetLength(0))) * GLOBALS.Scale, 
                                    _savedChunk.GetLength(1)*GLOBALS.Scale, 
                                    _savedChunk.GetLength(0)*GLOBALS.Scale), 
                                4f, 
                                Color.White
                            );
                        }
                        else if (_multiselect)
                        {
                            DrawRectangleLinesEx(
                                _selectionRectangle,
                                2, 
                                _eraseMode 
                                    ? Color.Red
                                    : (_memLoadMode 
                                        ? new Color(89, 7, 222, 255) 
                                        : Color.White
                                    )
                            );

                            DrawText(
                                _selectionSizeString,
                                (matrixX + 1) * scale,
                                (matrixY - 1) * scale,
                                12,
                                Color.White
                                );
                        }
                        else
                        {
                            if (matrixX == x && matrixY == y)
                            {
                                DrawRectangleLinesEx(
                                    new Rectangle(x * scale, y * scale, scale, scale), 
                                    2, 
                                    _eraseMode 
                                        ? new(255, 0, 0, 255) 
                                        : (_memLoadMode 
                                            ? new Color(89, 7, 222, 255) 
                                            : Color.White
                                        ));

                                // Coordinates

                                if (matrixX >= 0 && matrixX < GLOBALS.Level.Width && matrixY >= 0 && matrixY < GLOBALS.Level.Height)
                                    DrawText(
                                        $"x: {matrixX:0} y: {matrixY:0}",
                                        (matrixX + 1) * scale,
                                        (matrixY - 1) * scale,
                                        12,
                                        Color.White);
                            }
                        }

                        if (GLOBALS.Settings.GeometryEditor.ShowCurrentGeoIndicator && matrixX == x && matrixY == y)
                        {
                            var id = _geoMenuCategory switch
                            {
                                0 => GeoMenuIndexToBlockId[_geoMenuIndex],
                                1 => GeoMenuCategory2ToStackableId[_geoMenuIndex],
                                2 => GeoMenuCategory3ToStackableId[_geoMenuIndex],
                                3 => GeoMenuCategory4ToStackableId[_geoMenuIndex]
                            };
                            
                            if (_geoMenuCategory == 0) Printers.DrawTileSpec(id, new Vector2(matrixX+1, matrixY+1)*scale, 40, Color.White with { A = 100 });
                            else
                            {
                                if (id == 4)
                                {
                                    var textureIndex = 26;

                                    var texture = GLOBALS.Textures.GeoStackables[textureIndex];
                                        
                                    DrawTexturePro(
                                        texture,
                                        new(0, 0, texture.Width, texture.Height),
                                        new ((x+1)*scale, (y+1)*scale, 40, 40),
                                        new(0, 0),
                                        0,
                                        Color.White with { A = 100 });
                                }
                                else if (id == 11)
                                {
                                    var textureIndex = 7;

                                    var texture = GLOBALS.Textures.GeoStackables[textureIndex];
                                        
                                    DrawTexturePro(
                                        texture,
                                        new(0, 0, texture.Width, texture.Height),
                                        new ((x+1)*scale, (y+1)*scale, 40, 40),
                                        new(0, 0),
                                        0,
                                        Color.White with { A = 100 });
                                }
                                else
                                {
                                    var textureIndex = Utils.GetStackableTextureIndex(id);

                                    if (textureIndex != -1)
                                    {
                                        var texture = GLOBALS.Textures.GeoStackables[textureIndex];
                                        
                                        DrawTexturePro(
                                            texture,
                                            new(0, 0, texture.Width, texture.Height),
                                            new ((x+1)*scale, (y+1)*scale, 40, 40),
                                            new(0, 0),
                                            0,
                                            Color.White with { A = 100 });
                                    }
                                }
                            }
                        }
                    }
                }

                // the outbound border
                DrawRectangleLinesEx(
                    new Rectangle(
                        -2, 
                        -2, 
                        GLOBALS.Level.Width * scale + 4, 
                        GLOBALS.Level.Height * scale + 4), 
                    2, 
                    GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : Color.Black);

                // the border
                DrawRectangleLinesEx(GLOBALS.Level.Border, _camera.Zoom < GLOBALS.ZoomIncrement ? 5 : 2, new(255, 255, 255, 255));

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
            }
            EndMode2D();

            // geo menu
            #region RayGuiMenu
            //
            // unsafe
            // {
            //    fixed (byte* pt = _geoMenuPanelBytes)
            //    {
            //        RayGui.GuiPanel(panelRect, (sbyte*)pt);
            //    } 
            // }
            //
            //
            // // Categories
            //
            // DrawRectangleLinesEx(new((_geoMenuCategory * 42) + panelRect.X + 10, 80, 42, 42), 4.0f, Color.Blue);
            //
            // var category1Rect = new Rectangle(panelRect.X + 10, 80, 42, 42);
            // var category2Rect = new Rectangle(42 + panelRect.X + 10, 80, 42, 42);
            // var category3Rect = new Rectangle(84 + panelRect.X + 10, 80, 42, 42);
            // var category4Rect = new Rectangle(126 + panelRect.X + 10, 80, 42, 42);
            //
            // var category1Hovered = CheckCollisionPointRec(uiMouse, category1Rect);
            // var category2Hovered = CheckCollisionPointRec(uiMouse, category2Rect);
            // var category3Hovered = CheckCollisionPointRec(uiMouse, category3Rect);
            // var category4Hovered = CheckCollisionPointRec(uiMouse, category4Rect);
            //
            // if (category1Hovered)
            // {
            //     DrawRectangleRec(category1Rect, Color.Blue with { A = 100 });
            //
            //     if (IsMouseButtonPressed(MouseButton.Left))
            //     {
            //         _geoMenuCategory = 0;
            //         _geoMenuIndex = 0;
            //     }
            // }
            //
            // if (category2Hovered)
            // {
            //     DrawRectangleRec(category2Rect, Color.Blue with { A = 100 });
            //     
            //     if (IsMouseButtonPressed(MouseButton.Left))
            //     {
            //         _geoMenuCategory = 1;
            //         _geoMenuIndex = 0;
            //     }
            // }
            //
            // if (category3Hovered)
            // {
            //     DrawRectangleRec(category3Rect, Color.Blue with { A = 100 });
            //     
            //     if (IsMouseButtonPressed(MouseButton.Left))
            //     {
            //         _geoMenuCategory = 2;
            //         _geoMenuIndex = 0;
            //     }
            //
            // }
            //
            // if (category4Hovered)
            // {
            //     DrawRectangleRec(category4Rect, Color.Blue with { A = 100 });
            //     
            //     if (IsMouseButtonPressed(MouseButton.Left))
            //     {
            //         _geoMenuCategory = 3;
            //         _geoMenuIndex = 0;
            //     }
            // }
            //
            // DrawRectangleLinesEx(category1Rect, 1.0f, Color.Black);
            // DrawRectangleLinesEx(category2Rect, 1.0f, Color.Black);
            // DrawRectangleLinesEx(category3Rect, 1.0f, Color.Black);
            // DrawRectangleLinesEx(category4Rect, 1.0f, Color.Black);
            //
            //
            //
            // DrawTriangle(
            //     new(panelRect.X + 20, 90),
            //     new(panelRect.X + 20, 112),
            //     new(panelRect.X + 42, 112),
            //     Color.Black
            // );
            //
            // DrawRectangleV(
            //     new(panelRect.X + 70, 85),
            //     new(5, 32),
            //     Color.Black
            // );
            //
            // DrawRectangleV(
            //     new(panelRect.X + 57, 100),
            //     new(32, 5),
            //     Color.Black
            // );
            //
            // var placeSpearTexture = GLOBALS.Textures.GeoMenu[8];
            // var entryTexture = GLOBALS.Textures.GeoMenu[14];
            //
            // DrawTexturePro(
            //     placeSpearTexture,
            //     new(0, 0, placeSpearTexture.Width, placeSpearTexture.Height),
            //     new(panelRect.X + 99, 85, 32, 32),
            //     new(0, 0),
            //     0,
            //     Color.Black
            // );
            //
            // DrawTexturePro(
            //     entryTexture,
            //     new(0, 0, entryTexture.Width, entryTexture.Height),
            //     new(panelRect.X + 141, 85, 32, 32),
            //     new(0, 0),
            //     0,
            //     Color.Black
            // );
            //
            // unsafe
            // {
            //     fixed (int* scrollIndex = &_geoMenuScrollIndex)
            //     {
            //         var newGeoMenuIndex = RayGui.GuiListView(
            //             new(panelRect.X + 10, 150, 170, 200),
            //             _geoMenuCategory switch
            //             {
            //                 0 => "Solid;Slope;Platform;Glass",
            //                 1 => "Vertical Pole;Horizontal Pole;Cracked Terrain",
            //                 2 => "Bat Hive;Forbid Fly Chains;Waterfall;Worm Grass;Place Rock;Place Spear",
            //                 3 => "Shortcut Entrance;Shortcut Path;Room Entrance;Dragon Den;Wack-a-mole Hole;Scavenger Hole;Garbage Worm Hole",
            //                 _ => ""
            //             },
            //             scrollIndex,
            //             _geoMenuIndex
            //         );
            //
            //         if (newGeoMenuIndex != _geoMenuIndex && newGeoMenuIndex != -1)
            //         {
            //             #if DEBUG
            //             _logger.Debug($"New geo menu index: {newGeoMenuIndex}");
            //             #endif
            //             
            //             _geoMenuIndex = newGeoMenuIndex;
            //         }
            //     }
            // }
            #endregion
            
            // Current layer indicator

            var newLayer = GLOBALS.Layer;

            var layer3Hovered = GLOBALS.Layer == 2 && CheckCollisionPointRec(uiMouse, layer3Rect);

            if (layer3Hovered)
            {
                DrawRectangleRec(layer3Rect, Color.Blue with { A = 100 });

                if (IsMouseButtonPressed(MouseButton.Left)) newLayer = 0;
            }

            DrawRectangleRec(
                layer3Rect,
                GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.Black with { A = 100 } : Color.White
            );

            DrawRectangleLines(10, sHeight - 50, 40, 40, Color.Gray);

            if (GLOBALS.Layer == 2) DrawText("3", 26, sHeight - 40, 22, GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : Color.Black);
            
            if (GLOBALS.Layer is 1 or 0)
            {
                var layer2Hovered = GLOBALS.Layer == 1 && CheckCollisionPointRec(uiMouse, layer2Rect);

                if (layer2Hovered)
                {
                    DrawRectangleRec(layer2Rect, Color.Blue with { A = 100 });

                    if (IsMouseButtonPressed(MouseButton.Left)) newLayer = 2;
                }
                
                DrawRectangleRec(
                    layer2Rect,
                    GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.Black with { A = 100 } : Color.White
                );

                DrawRectangleLines(20, sHeight - 60, 40, 40, Color.Gray);

                if (GLOBALS.Layer == 1) DrawText("2", 35, sHeight - 50, 22, GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : Color.Black);
            }

            if (GLOBALS.Layer == 0)
            {
                var layer1Hovered = CheckCollisionPointRec(uiMouse, layer1Rect);

                if (layer1Hovered)
                {
                    DrawRectangleRec(layer1Rect, Color.Blue with { A = 100 });
                    if (IsMouseButtonPressed(MouseButton.Left)) newLayer = 1;
                }
                
                DrawRectangleRec(
                    layer1Rect,
                    GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.Black with { A = 100 } : Color.White
                );

                DrawRectangleLines(
                    30, sHeight - 70, 40, 40, Color.Gray);

                DrawText("1", 48, sHeight - 60, 22, GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : Color.Black);
            }

            if (newLayer != GLOBALS.Layer) GLOBALS.Layer = newLayer;
            
            // Show Camera Indicator

            ref var toggleCameraTexture = ref GLOBALS.Textures.GeoInterface[8];
            
            DrawRectangleRec(toggleCameraRect, Color.White);

            if (toggleCameraHovered)
            {
                DrawRectangleRec(toggleCameraRect, Color.Blue with { A = 100 });

                if (toggleCameraHovered && IsMouseButtonPressed(MouseButton.Left))
                {
                    GLOBALS.Settings.GeometryEditor.ShowCameras = !GLOBALS.Settings.GeometryEditor.ShowCameras;
                }
            }
            
            DrawTexturePro(
                toggleCameraTexture, 
                new Rectangle(0, 0, toggleCameraTexture.Width, toggleCameraTexture.Height),
                toggleCameraRect,
                new Vector2(0, 0),
                0,
                Color.Black
            );
            
            rlImGui.Begin();
            
            ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
            
            // Geo Menu

            var menuOpened = ImGui.Begin("Blocks##GeoBlocks");
            
            var menuPos = ImGui.GetWindowPos();
            var menuWinSpace = ImGui.GetWindowSize();
            
            if (CheckCollisionPointRec(GetMousePosition(), new(menuPos.X - 5, menuPos.Y-5, menuWinSpace.X + 10, menuWinSpace.Y+10)))
            {
                _isMenuWinHovered = true;

                if (IsMouseButtonDown(MouseButton.Left)) _isMenuWinDragged = true;
            }
            else
            {
                _isMenuWinHovered = false;
            }
            
            if (IsMouseButtonReleased(MouseButton.Left) && _isMenuWinDragged) _isMenuWinDragged = false;
            
            if (menuOpened)
            {
                var availableSpace = ImGui.GetContentRegionAvail();

                var blockSelected = ImGui.ImageButton(
                    "Blocks", 
                    new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[4].Id : GLOBALS.Textures.GeoInterface[0].Id), 
                    new Vector2(30, 30));
                
                ImGui.SameLine();

                var polesSelected = ImGui.ImageButton(
                    "Poles", 
                    new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[5].Id : GLOBALS.Textures.GeoInterface[1].Id), 
                    new Vector2(30, 30));
                
                ImGui.SameLine();

                var stuffSelected = ImGui.ImageButton(
                    "Stuff", 
                    new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[6].Id : GLOBALS.Textures.GeoInterface[2].Id), 
                    new Vector2(30, 30));
                
                ImGui.SameLine();

                var pathsSelected = ImGui.ImageButton(
                    "Paths", 
                    new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[7].Id : GLOBALS.Textures.GeoInterface[3].Id), 
                    new Vector2(30, 30));

                if (blockSelected) _geoMenuCategory = 0;
                if (polesSelected) _geoMenuCategory = 1;
                if (stuffSelected) _geoMenuCategory = 2;
                if (pathsSelected) _geoMenuCategory = 3;

                if (ImGui.BeginListBox("##GeosList", availableSpace with { Y = availableSpace.Y - 40 }))
                {
                    switch (_geoMenuCategory)
                    {
                        case 0:
                        {
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[11].Id : GLOBALS.Textures.GeoInterface[8].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Block", _geoMenuIndex == 0)) _geoMenuIndex = 0;
                            
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[4].Id : GLOBALS.Textures.GeoInterface[0].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Slope", _geoMenuIndex == 1)) _geoMenuIndex = 1;
                            
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[12].Id : GLOBALS.Textures.GeoInterface[9].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Platform", _geoMenuIndex == 2)) _geoMenuIndex = 2;
                            
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[13].Id : GLOBALS.Textures.GeoInterface[10].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Glass", _geoMenuIndex == 3)) _geoMenuIndex = 3;
                        }
                            break;
                        case 1:
                        {
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[14].Id : GLOBALS.Textures.GeoInterface[17].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Vertical Pole", _geoMenuIndex == 0)) _geoMenuIndex = 0;
                            
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[15].Id : GLOBALS.Textures.GeoInterface[18].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Horizontal Pole", _geoMenuIndex == 1)) _geoMenuIndex = 1;
                            
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[16].Id : GLOBALS.Textures.GeoInterface[19].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Cracked Terrain", _geoMenuIndex == 2)) _geoMenuIndex = 2;
                        }
                            break;
                        case 2:
                        {
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[20].Id : GLOBALS.Textures.GeoInterface[25].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Bat Hive", _geoMenuIndex == 0)) _geoMenuIndex = 0;
                            
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[21].Id : GLOBALS.Textures.GeoInterface[26].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Forbid Fly Chains", _geoMenuIndex == 1)) _geoMenuIndex = 1;
                            
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[22].Id : GLOBALS.Textures.GeoInterface[27].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Waterfall", _geoMenuIndex == 2)) _geoMenuIndex = 2;
                            
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[23].Id : GLOBALS.Textures.GeoInterface[28].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Worm Grass", _geoMenuIndex == 3)) _geoMenuIndex = 3;
                            
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[24].Id : GLOBALS.Textures.GeoInterface[29].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Place Rock", _geoMenuIndex == 4)) _geoMenuIndex = 4;
                            
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[6].Id : GLOBALS.Textures.GeoInterface[2].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Place Spear", _geoMenuIndex == 5)) _geoMenuIndex = 5;
                        }
                            break;
                        case 3:
                        {
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[30].Id : GLOBALS.Textures.GeoInterface[37].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Shortcut Entrance", _geoMenuIndex == 0)) _geoMenuIndex = 0;
                            
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[31].Id : GLOBALS.Textures.GeoInterface[38].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Shortcut Path", _geoMenuIndex == 1)) _geoMenuIndex = 1;
                            
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[32].Id : GLOBALS.Textures.GeoInterface[39].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Room Entrance", _geoMenuIndex == 2)) _geoMenuIndex = 2;
                            
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[33].Id : GLOBALS.Textures.GeoInterface[40].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Dragon Den", _geoMenuIndex == 3)) _geoMenuIndex = 3;
                            
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[34].Id : GLOBALS.Textures.GeoInterface[41].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Wack-a-mole Hole", _geoMenuIndex == 4)) _geoMenuIndex = 4;
                            
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[35].Id : GLOBALS.Textures.GeoInterface[42].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Scavenger Hole", _geoMenuIndex == 5)) _geoMenuIndex = 5;
                            
                            ImGui.Image(new IntPtr(GLOBALS.Settings.GeneralSettings.DarkTheme ? GLOBALS.Textures.GeoInterface[36].Id : GLOBALS.Textures.GeoInterface[43].Id), new(20, 20));
                            ImGui.SameLine();
                            if (ImGui.Selectable("Garbage Worm Hole", _geoMenuIndex == 6)) _geoMenuIndex = 6;
                        }
                            break;
                    }
                    ImGui.EndListBox();
                }
                
                ImGui.End();
            }
            
            // Settings Window

            var settingsOpened = ImGui.Begin("Settings##NewGeoSettings");

            var settingsPos = ImGui.GetWindowPos();
            var settingsWinSpace = ImGui.GetWindowSize();

            if (CheckCollisionPointRec(GetMousePosition(), new(settingsPos.X - 5, settingsPos.Y-5, settingsWinSpace.X + 10, settingsWinSpace.Y+10)))
            {
                _isSettingsWinHovered = true;

                if (IsMouseButtonDown(MouseButton.Left)) _isSettingsWinDragged = true;
            }
            else
            {
                _isSettingsWinHovered = false;
            }

            if (IsMouseButtonReleased(MouseButton.Left) && _isSettingsWinDragged) _isSettingsWinDragged = false;
            
            if (settingsOpened)
            {
                var availableSpace = ImGui.GetContentRegionAvail();
                
                ImGui.SeparatorText("Colors");

                var layer1Color = GLOBALS.Settings.GeometryEditor.LayerColors.Layer1;
                var layer2Color = GLOBALS.Settings.GeometryEditor.LayerColors.Layer2;
                var layer3Color = GLOBALS.Settings.GeometryEditor.LayerColors.Layer3;

                var waterColor = GLOBALS.Settings.GeometryEditor.WaterColor;

                var layer1ColorVec = new Vector3(layer1Color.R / 255f, layer1Color.G / 255f, layer1Color.B / 255f);
                var layer2ColorVec = new Vector3(layer2Color.R / 255f, layer2Color.G / 255f, layer2Color.B / 255f);
                var layer3ColorVec = new Vector3(layer3Color.R / 255f, layer3Color.G / 255f, layer3Color.B / 255f);
                
                var waterColorVec = new Vector3(waterColor.R / 255f, waterColor.G / 255f, waterColor.B / 255f);
                
                ImGui.SetNextItemWidth(250);
                ImGui.ColorEdit3("Layer 1", ref layer1ColorVec);
                
                ImGui.SetNextItemWidth(250);
                ImGui.ColorEdit3("Layer 2", ref layer2ColorVec);
                
                ImGui.SetNextItemWidth(250);
                ImGui.ColorEdit3("Layer 3", ref layer3ColorVec);
                
                ImGui.SetNextItemWidth(250);
                ImGui.ColorEdit3("Water", ref waterColorVec);

                GLOBALS.Settings.GeometryEditor.LayerColors.Layer1 = new ConColor((byte)(layer1ColorVec.X * 255),
                    (byte)(layer1ColorVec.Y * 255), (byte)(layer1ColorVec.Z * 255));
                
                GLOBALS.Settings.GeometryEditor.LayerColors.Layer2 = new ConColor((byte)(layer2ColorVec.X * 255),
                    (byte)(layer2ColorVec.Y * 255), (byte)(layer2ColorVec.Z * 255), 50);
                
                GLOBALS.Settings.GeometryEditor.LayerColors.Layer3 = new ConColor((byte)(layer3ColorVec.X * 255),
                    (byte)(layer3ColorVec.Y * 255), (byte)(layer3ColorVec.Z * 255), 50);
                
                GLOBALS.Settings.GeometryEditor.WaterColor = new ConColor((byte)(waterColorVec.X * 255),
                    (byte)(waterColorVec.Y * 255), (byte)(waterColorVec.Z * 255), 50);

                var resetColorsSelected = ImGui.Button("Reset Colors", availableSpace with { Y = 20 });

                if (resetColorsSelected)
                {
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer1 = Color.Black;
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer2 = new ConColor(0, 255, 0, 50);
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer3 = new ConColor(255, 0, 0, 50);
                    GLOBALS.Settings.GeometryEditor.WaterColor = new ConColor(0, 0, 255, 70);
                }
                
                // Visibility

                ImGui.SeparatorText("Visibility");
                
                var showLayer1 = _showLayer1;
                var showLayer2 = _showLayer2;
                var showLayer3 = _showLayer3;

                ImGui.Checkbox("Layer 1", ref showLayer1);
                ImGui.Checkbox("Layer 2", ref showLayer2);
                ImGui.Checkbox("Layer 3", ref showLayer3);

                _showLayer1 = showLayer1;
                _showLayer2 = showLayer2;
                _showLayer3 = showLayer3;
                
                ImGui.Spacing();

                var showCameras = GLOBALS.Settings.GeometryEditor.ShowCameras;
                
                ImGui.Checkbox("Cameras", ref showCameras);

                GLOBALS.Settings.GeometryEditor.ShowCameras = showCameras;
                
                // Controls
                
                ImGui.SeparatorText("Controls");

                var multiSelect = _allowMultiSelect;
                
                ImGui.Checkbox("Multi-Select", ref multiSelect);

                _allowMultiSelect = multiSelect;

                var pasteAir = GLOBALS.Settings.GeometryEditor.PasteAir;
                ImGui.Checkbox("Paste Air", ref pasteAir);
                if (pasteAir != GLOBALS.Settings.GeometryEditor.PasteAir)
                    GLOBALS.Settings.GeometryEditor.PasteAir = pasteAir;
                
                ImGui.End();
            }
            
            // Navigation
            
            var navWindowRect = Printers.ImGui.NavigationWindow();

            _isNavigationWinHovered = CheckCollisionPointRec(uiMouse, navWindowRect with
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
            
            // Shortcuts Window

            if (GLOBALS.Settings.GeneralSettings.ShortcutWindow)
            {
                var shortcutWindowRect = Printers.ImGui.ShortcutsWindow(GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts);
            
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
            }
            
            rlImGui.End();
        }
        EndDrawing();
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera;
    }
}