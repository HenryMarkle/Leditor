using static Raylib_CsLo.Raylib;

using System.Numerics;
using ImGuiNET;
using rlImGui_cs;

namespace Leditor;

public class ExperimentalGeometryPage(Serilog.Core.Logger logger, Camera2D? camera = null) : IPage
{
    private readonly Serilog.Core.Logger _logger = logger;

    private readonly ExperimentalGeoShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts;

    private Camera2D _camera = camera ?? new Camera2D() { zoom = 1.0f };

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
    
    private int _lastChangingMatrixX = -1;
    private int _lastChangingMatrixY = -1;

    private string _selectionSizeString = "";
    private Rectangle _selectionRectangle = new(0, 0, 0, 0);
    
    public void Draw()
    {
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera;
        
        var ctrl = IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);
        var shift = IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
        var alt = IsKeyDown(KeyboardKey.KEY_LEFT_ALT);
        
        GLOBALS.PreviousPage = 2;
        var scale = GLOBALS.Scale;
        var settings = GLOBALS.Settings;
        Span<Color> layerColors = [settings.GeometryEditor.LayerColors.Layer1, settings.GeometryEditor.LayerColors.Layer2, settings.GeometryEditor.LayerColors.Layer3];

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
            _logger.Debug("go from GLOBALS.Page 2 to GLOBALS.Page 6");
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

        Rectangle panelRect = new(sWidth - 200, 50, 188, 400);

        var canDrawGeo = !toggleCameraHovered &&
                         !_isShortcutsWinHovered && 
                         !_isShortcutsWinDragged && 
                         !CheckCollisionPointRec(GetMousePosition(), panelRect) &&
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
            _geoMenuIndex = 0;
        }

        if (_shortcuts.ToRightGeo.Check(ctrl, shift, alt))
        {
            _geoMenuCategory = ++_geoMenuCategory % 4;
            _geoMenuIndex = 0;
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
            delta = RayMath.Vector2Scale(delta, -1.0f / _camera.zoom);
            _camera.target = RayMath.Vector2Add(_camera.target, delta);
        }
        
        // handle zoom
        var wheel = GetMouseWheelMove();
        if (wheel != 0)
        {
            var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
            _camera.offset = GetMousePosition();
            _camera.target = mouseWorldPosition;
            _camera.zoom += wheel * GLOBALS.ZoomIncrement;
            if (_camera.zoom < GLOBALS.ZoomIncrement) _camera.zoom = GLOBALS.ZoomIncrement;
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
                                newCopy[y, x] = new RunCell { Geo = cell.Geo == 0 ? mtxCell.Geo : cell.Geo, Stackables = [..cell.Stackables] };
                                // Copy level to old state
                                oldCopy[y, x] = new RunCell { Geo = oldCell.Geo, Stackables = [..oldCell.Stackables] };
                                    
                                bool[] newStackables = [..cell.Stackables];
                                cell.Stackables = newStackables;

                                if (cell.Geo != 0) mtxCell.Geo = cell.Geo;
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
                                                x * scale >= GLOBALS.Level.Border.width + GLOBALS.Level.Border.X ||
                                                y * scale < GLOBALS.Level.Border.Y ||
                                                y * scale >= GLOBALS.Level.Border.height + GLOBALS.Level.Border.Y) break;

                                            if (_geoMenuIndex == 0 && GLOBALS.Layer != 0) break;

                                            var id = GeoMenuCategory3ToStackableId[_geoMenuIndex];
                                            cell.Stackables[id] = true;
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
                                            x * scale >= GLOBALS.Level.Border.width + GLOBALS.Level.Border.X ||
                                            y * scale < GLOBALS.Level.Border.Y ||
                                            y * scale >= GLOBALS.Level.Border.height + GLOBALS.Level.Border.Y) break;

                                        if (_geoMenuIndex == 0 && GLOBALS.Layer != 0) break;

                                        var id = GeoMenuCategory3ToStackableId[_geoMenuIndex];
                                        cell.Stackables[id] = false;
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
                    _selectionRectangle.width = _selectionRectangle.width * scale + scale;
                    _selectionRectangle.height = _selectionRectangle.height * scale + scale;

                    _selectionSizeString = $"{_selectionRectangle.width / scale:0}w {_selectionRectangle.height / scale:0}h";
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
                ? BLACK 
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
                                                WHITE
                                            ); // TODO: remove opacity from entrances
                                            break;

                                        
                                        case 11:    // crack
                                            DrawTexture(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(_savedChunk, x, y))],
                                                (matrixX + x - Utils.GetMiddle(_savedChunk.GetLength(1))) * scale,
                                                (matrixY + y - Utils.GetMiddle(_savedChunk.GetLength(0))) * scale,
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
                DrawRectangleLinesEx(GLOBALS.Level.Border, _camera.zoom < GLOBALS.ZoomIncrement ? 5 : 2, new(255, 255, 255, 255));
                
                // a lazy way to hide the rest of the grid
                // DrawRectangle(GLOBALS.Level.Width * -scale, -3, GLOBALS.Level.Width * scale, GLOBALS.Level.Height * 2 * scale, new(120, 120, 120, 255));
                // DrawRectangle(0, GLOBALS.Level.Height * scale, GLOBALS.Level.Width * scale + 2, GLOBALS.Level.Height * scale, new(120, 120, 120, 255));

                // the selection rectangle

                for (var y = 0; y < GLOBALS.Level.Height; y++)
                {
                    for (var x = 0; x < GLOBALS.Level.Width; x++)
                    {
                        if (!_multiselect && _selectionRectangle is not { width: 0, height: 0 })
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
                                WHITE
                            );
                        }
                        else if (_multiselect)
                        {
                            DrawRectangleLinesEx(
                                _selectionRectangle,
                                2, 
                                _eraseMode 
                                    ? RED
                                    : (_memLoadMode 
                                        ? new Color(89, 7, 222, 255) 
                                        : WHITE
                                    )
                            );

                            DrawText(
                                _selectionSizeString,
                                (matrixX + 1) * scale,
                                (matrixY - 1) * scale,
                                12,
                                WHITE
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
                                            : WHITE
                                        ));

                                // Coordinates

                                if (matrixX >= 0 && matrixX < GLOBALS.Level.Width && matrixY >= 0 && matrixY < GLOBALS.Level.Height)
                                    DrawText(
                                        $"x: {matrixX:0} y: {matrixY:0}",
                                        (matrixX + 1) * scale,
                                        (matrixY - 1) * scale,
                                        12,
                                        WHITE);
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
                            
                            if (_geoMenuCategory == 0) Printers.DrawTileSpec(id, new Vector2(matrixX+1, matrixY+1)*scale, 40, WHITE with { a = 100 });
                            else
                            {
                                if (id == 4)
                                {
                                    var textureIndex = 26;

                                    var texture = GLOBALS.Textures.GeoStackables[textureIndex];
                                        
                                    DrawTexturePro(
                                        texture,
                                        new(0, 0, texture.width, texture.height),
                                        new ((x+1)*scale, (y+1)*scale, 40, 40),
                                        new(0, 0),
                                        0,
                                        WHITE with { a = 100 });
                                }
                                else if (id == 11)
                                {
                                    var textureIndex = 7;

                                    var texture = GLOBALS.Textures.GeoStackables[textureIndex];
                                        
                                    DrawTexturePro(
                                        texture,
                                        new(0, 0, texture.width, texture.height),
                                        new ((x+1)*scale, (y+1)*scale, 40, 40),
                                        new(0, 0),
                                        0,
                                        WHITE with { a = 100 });
                                }
                                else
                                {
                                    var textureIndex = Utils.GetStackableTextureIndex(id);

                                    if (textureIndex != -1)
                                    {
                                        var texture = GLOBALS.Textures.GeoStackables[textureIndex];
                                        
                                        DrawTexturePro(
                                            texture,
                                            new(0, 0, texture.width, texture.height),
                                            new ((x+1)*scale, (y+1)*scale, 40, 40),
                                            new(0, 0),
                                            0,
                                            WHITE with { a = 100 });
                                    }
                                }
                            }
                        }
                    }
                }

                // the outbound border
                DrawRectangleLinesEx(new(0, 0, GLOBALS.Level.Width * scale, GLOBALS.Level.Height * scale), 2, new(0, 0, 0, 255));

                // the border
                DrawRectangleLinesEx(GLOBALS.Level.Border, _camera.zoom < GLOBALS.ZoomIncrement ? 5 : 2, new(255, 255, 255, 255));

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
                
                /*// a lazy way to hide the rest of the grid
                DrawRectangle(matrixWidth * -scale, -3, matrixWidth * scale, matrixHeight * 2 * scale, new(120, 120, 120, 255));
                DrawRectangle(0, matrixHeight * scale, matrixWidth * scale + 2, matrixHeight * scale, new(120, 120, 120, 255));*/
            }
            EndMode2D();

            // geo menu
            
            // ImGui

            unsafe
            {
               fixed (byte* pt = _geoMenuPanelBytes)
               {
                   RayGui.GuiPanel(panelRect, (sbyte*)pt);
               } 
            }
            

            // Categories

            DrawRectangleLinesEx(new((_geoMenuCategory * 42) + panelRect.X + 10, 80, 42, 42), 4.0f, BLUE);

            var category1Rect = new Rectangle(panelRect.X + 10, 80, 42, 42);
            var category2Rect = new Rectangle(42 + panelRect.X + 10, 80, 42, 42);
            var category3Rect = new Rectangle(84 + panelRect.X + 10, 80, 42, 42);
            var category4Rect = new Rectangle(126 + panelRect.X + 10, 80, 42, 42);

            var category1Hovered = CheckCollisionPointRec(uiMouse, category1Rect);
            var category2Hovered = CheckCollisionPointRec(uiMouse, category2Rect);
            var category3Hovered = CheckCollisionPointRec(uiMouse, category3Rect);
            var category4Hovered = CheckCollisionPointRec(uiMouse, category4Rect);

            if (category1Hovered)
            {
                DrawRectangleRec(category1Rect, BLUE with { a = 100 });

                if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    _geoMenuCategory = 0;
                    _geoMenuIndex = 0;
                }
            }
            
            if (category2Hovered)
            {
                DrawRectangleRec(category2Rect, BLUE with { a = 100 });
                
                if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    _geoMenuCategory = 1;
                    _geoMenuIndex = 0;
                }
            }
            
            if (category3Hovered)
            {
                DrawRectangleRec(category3Rect, BLUE with { a = 100 });
                
                if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    _geoMenuCategory = 2;
                    _geoMenuIndex = 0;
                }

            }
            
            if (category4Hovered)
            {
                DrawRectangleRec(category4Rect, BLUE with { a = 100 });
                
                if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    _geoMenuCategory = 3;
                    _geoMenuIndex = 0;
                }
            }
            
            DrawRectangleLinesEx(category1Rect, 1.0f, BLACK);
            DrawRectangleLinesEx(category2Rect, 1.0f, BLACK);
            DrawRectangleLinesEx(category3Rect, 1.0f, BLACK);
            DrawRectangleLinesEx(category4Rect, 1.0f, BLACK);
            
            

            DrawTriangle(
                new(panelRect.X + 20, 90),
                new(panelRect.X + 20, 112),
                new(panelRect.X + 42, 112),
                BLACK
            );

            DrawRectangleV(
                new(panelRect.X + 70, 85),
                new(5, 32),
                BLACK
            );

            DrawRectangleV(
                new(panelRect.X + 57, 100),
                new(32, 5),
                BLACK
            );

            var placeSpearTexture = GLOBALS.Textures.GeoMenu[8];
            var entryTexture = GLOBALS.Textures.GeoMenu[14];

            DrawTexturePro(
                placeSpearTexture,
                new(0, 0, placeSpearTexture.width, placeSpearTexture.height),
                new(panelRect.X + 99, 85, 32, 32),
                new(0, 0),
                0,
                BLACK
            );

            DrawTexturePro(
                entryTexture,
                new(0, 0, entryTexture.width, entryTexture.height),
                new(panelRect.X + 141, 85, 32, 32),
                new(0, 0),
                0,
                BLACK
            );

            unsafe
            {
                fixed (int* scrollIndex = &_geoMenuScrollIndex)
                {
                    var newGeoMenuIndex = RayGui.GuiListView(
                        new(panelRect.X + 10, 150, 170, 200),
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
                        
                        _geoMenuIndex = newGeoMenuIndex;
                    }
                }
            }
            
            #region GeoIcons

            /*switch (_geoMenuCategory)
            {
                case 0:
                {
                    ref var solid = ref GLOBALS.Textures.GeoBlocks[0];
                    ref var slope = ref GLOBALS.Textures.GeoBlocks[1]; 
                    ref var platform = ref GLOBALS.Textures.GeoBlocks[5];
                    
                    DrawTexturePro(
                        solid, 
                        new(0, 0, solid.width, solid.height), 
                        new(panelRect.X + 20, 157, 15, 15),
                        new(0, 0),
                        0,    
                        BLACK);
                    
                    DrawTexturePro(
                        slope, 
                        new(0, 0, slope.width, slope.height), 
                        new(panelRect.X + 20, 183, 15, 15),
                        new(0, 0),
                        0,    
                        BLACK);
                    
                    DrawTexturePro(
                        platform, 
                        new(0, 0, platform.width, platform.height), 
                        new(panelRect.X + 20, 209, 15, 15),
                        new(0, 0),
                        0,    
                        BLACK);
                }
                    break;

                case 1:
                {
                    ref var horizontal = ref GLOBALS.Textures.GeoStackables[0];
                    ref var vertical = ref GLOBALS.Textures.GeoStackables[1]; 
                    ref var cracked = ref GLOBALS.Textures.GeoMenu[9];
                    
                    DrawTexturePro(
                        horizontal, 
                        new(0, 0, horizontal.width, horizontal.height), 
                        new(panelRect.X + 20, 157, 15, 15),
                        new(0, 0),
                        0,    
                        BLACK);
                    
                    DrawTexturePro(
                        vertical, 
                        new(0, 0, vertical.width, vertical.height), 
                        new(panelRect.X + 20, 183, 15, 15),
                        new(0, 0),
                        0,    
                        BLACK);
                    
                    DrawTexturePro(
                        cracked, 
                        new(0, 0, cracked.width, cracked.height), 
                        new(panelRect.X + 20, 209, 15, 15),
                        new(0, 0),
                        0,    
                        BLACK);
                }
                    break;

                case 2:
                {
                    
                }
                    break;
                
                case 3:
                    break;
            }*/
            
            #endregion

            _allowMultiSelect = RayGui.GuiCheckBox(new(panelRect.X + 10, 360, 15, 15), "Multi-Select", _allowMultiSelect);
            _eraseAllMode = RayGui.GuiCheckBox(new(panelRect.X + 10, 380, 15, 15), "Erase Everything", _eraseAllMode);
            GLOBALS.Settings.GeometryEditor.ShowCameras = RayGui.GuiCheckBox(new(panelRect.X + 10, 400, 15, 15), "Show Cameras", GLOBALS.Settings.GeometryEditor.ShowCameras);

            _showLayer1 = RayGui.GuiCheckBox(new(panelRect.X + 145, 360, 15, 15), "L 1", _showLayer1);
            _showLayer2 = RayGui.GuiCheckBox(new(panelRect.X + 145, 380, 15, 15), "L 2", _showLayer2);
            _showLayer3 = RayGui.GuiCheckBox(new(panelRect.X + 145, 400, 15, 15), "L 3", _showLayer3);

            // Current layer indicator

            var newLayer = GLOBALS.Layer;

            var layer3Hovered = GLOBALS.Layer == 2 && CheckCollisionPointRec(uiMouse, layer3Rect);

            if (layer3Hovered)
            {
                DrawRectangleRec(layer3Rect, BLUE with { a = 100 });

                if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) newLayer = 0;
            }

            DrawRectangleRec(
                layer3Rect,
                WHITE
            );

            DrawRectangleLines(10, sHeight - 50, 40, 40, GRAY);

            if (GLOBALS.Layer == 2) DrawText("3", 26, sHeight - 40, 22, BLACK);
            
            if (GLOBALS.Layer is 1 or 0)
            {
                var layer2Hovered = GLOBALS.Layer == 1 && CheckCollisionPointRec(uiMouse, layer2Rect);

                if (layer2Hovered)
                {
                    DrawRectangleRec(layer2Rect, BLUE with { a = 100 });

                    if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) newLayer = 2;
                }
                
                DrawRectangleRec(
                    layer2Rect,
                    WHITE
                );

                DrawRectangleLines(20, sHeight - 60, 40, 40, GRAY);

                if (GLOBALS.Layer == 1) DrawText("2", 35, sHeight - 50, 22, BLACK);
            }

            if (GLOBALS.Layer == 0)
            {
                var layer1Hovered = CheckCollisionPointRec(uiMouse, layer1Rect);

                if (layer1Hovered)
                {
                    DrawRectangleRec(layer1Rect, BLUE with { a = 100 });
                    if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) newLayer = 1;
                }
                
                DrawRectangleRec(
                    layer1Rect,
                    WHITE
                );

                DrawRectangleLines(
                    30, sHeight - 70, 40, 40, GRAY);

                DrawText("1", 48, sHeight - 60, 22, BLACK);
            }

            if (newLayer != GLOBALS.Layer) GLOBALS.Layer = newLayer;
            
            // Show Camera Indicator

            ref var toggleCameraTexture = ref GLOBALS.Textures.GeoInterface[0];
            
            DrawRectangleRec(toggleCameraRect, WHITE);

            if (toggleCameraHovered)
            {
                DrawRectangleRec(toggleCameraRect, BLUE with { a = 100 });

                if (toggleCameraHovered && IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    GLOBALS.Settings.GeometryEditor.ShowCameras = !GLOBALS.Settings.GeometryEditor.ShowCameras;
                }
            }
            
            DrawTexturePro(
                toggleCameraTexture, 
                new Rectangle(0, 0, toggleCameraTexture.width, toggleCameraTexture.height),
                toggleCameraRect,
                new Vector2(0, 0),
                0,
                BLACK
            );
            
            // Shortcuts Window

            if (GLOBALS.Settings.GeneralSettings.ShortcutWindow)
            {
                rlImGui.Begin();
                var shortcutWindowRect = Printers.ImGui.ShortcutsWindow(GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts);
            
                _isShortcutsWinHovered = CheckCollisionPointRec(
                    uiMouse, 
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
        }
        EndDrawing();
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera;
    }
}