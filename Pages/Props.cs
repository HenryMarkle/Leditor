using System.Numerics;
using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class PropsEditorPage : IPage
{
    private readonly Serilog.Core.Logger _logger;

    private Camera2D _camera = new () { zoom = 0.8f };
    
    private const float SelectionMargin = 3f;

    private bool _showLayer1Tiles = true;
    private bool _showLayer2Tiles = true;
    private bool _showLayer3Tiles = true;

    private bool _showTileLayer1 = true;
    private bool _showTileLayer2 = true;
    private bool _showTileLayer3 = true;

    private bool _propCategoryFocus = true;

    private int _menuRootCategoryIndex;
    
    private int _propsMenuTilesCategoryScrollIndex;
    private int _propsMenuTilesCategoryIndex;
    private int _propsMenuTilesScrollIndex;
    private int _propsMenuTilesIndex;

    private int _propsMenuOthersCategoryScrollIndex;
    private int _propsMenuOthersScrollIndex;
    private int _propsMenuOthersCategoryIndex;
    private int _propsMenuOthersIndex;

    private int _spinnerLock;
    
    private const float PropScale = 0.4f;

    private int _selectedListPage;

    private int _quadLock;
    private int _pointLock = -1;
    private int _bezierHandleLock = -1;

    private bool _clickTracker;

    private bool _showRopePanel;

    private int _mode;

    private bool _movingProps;
    private bool _rotatingProps;
    private bool _scalingProps;
    private bool _stretchingProp;
    private bool _editingPropPoints;
    private bool _ropeMode;

    private byte _stretchAxes;

    private int _ropeSimulationFrameCut = 1;
    private int _ropeSimulationFrame;
    
    private Vector2 _selection1 = new(-100, -100);
    private Rectangle _selection;
    private Rectangle _selectedPropsEncloser;
    private Vector2 _selectedPropsCenter;

    private bool[] _selected = [];
    private bool[] _hidden = [];

    private readonly (string name, Color color)[] _propCategoriesOnly = GLOBALS.PropCategories[..^2]; // a risky move..
    private readonly InitPropBase[][] _propsOnly = GLOBALS.Props[..^2]; // a risky move..

    private readonly (int index, string category)[] _tilesAsPropsCategoryIndices = [];
    private readonly (int index, InitTile init)[][] _tilesAsPropsIndices = [];
    
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

    private readonly string[] _menuCategoryNames = [ "Tiles", "Ropes", "Long Props", "Other" ];

    private readonly byte[] _menuPanelBytes = "Menu"u8.ToArray();
    private readonly byte[] _ropePanelBytes = "Rope Settings"u8.ToArray();
    
    private (int index, bool simSwitch, RopeModel model, Vector2[] bezierHandles)[] _models;

    internal PropsEditorPage(Serilog.Core.Logger logger)
    {
        _logger = logger;

        for (var c = 0; c < GLOBALS.Tiles.Length; c++)
        {
            List<(int, InitTile)> tiles = [];

            for (var t = 0; t < GLOBALS.Tiles[c].Length; t++)
            {
                var tile = GLOBALS.Tiles[c][t];

                if (tile.Type == InitTileType.VoxelStruct && !tile.Tags.Contains("notProp")) tiles.Add((t, tile));
            }

            if (tiles.Count != 0)
            {
                _tilesAsPropsCategoryIndices = [.. _tilesAsPropsCategoryIndices, (c, GLOBALS.TileCategories[c].Item1)];
                _tilesAsPropsIndices = [.. _tilesAsPropsIndices, [.. tiles]];
            }
            
            // init rope models
        }
    }

    #nullable enable
    internal void OnProjectLoaded(object? sender, EventArgs e)
    {
        ImportRopeModels();
    }

    internal void OnProjectCreated(object? sender, EventArgs e)
    {
        ImportRopeModels();
    }
    #nullable disable

    private void ImportRopeModels()
    {
        List<(int, bool, RopeModel, Vector2[])> models = [];
        
        for (var r = 0; r < GLOBALS.Level.Props.Length; r++)
        {
            var current = GLOBALS.Level.Props[r];
            
            if (current.type != InitPropType.Rope) continue;
            
            var newModel = new RopeModel(current.prop, GLOBALS.RopeProps[current.position.index], 36);
            models.Add((r, false, newModel, []));
        }

        _models = [..models];
    }

    private void UpdateRopeModelSegments()
    {
        List<(int, bool, RopeModel, Vector2[])> models = [];
        
        for (var r = 0; r < _models.Length; r++)
        {
            var current = _models[r];
            
            models.Add((
                current.index, 
                current.simSwitch, 
                new RopeModel(
                    GLOBALS.Level.Props[current.index].prop, 
                    GLOBALS.RopeProps[GLOBALS.Level.Props[current.index].position.index], 
                    GLOBALS.Level.Props[current.index].prop.Extras.RopePoints.Length), 
                current.bezierHandles
            ));
        }

        _models = [..models];
    }

    private void DecrementMenuIndex()
    {
        switch (_menuRootCategoryIndex)
        {
            case 0: // tiles as props
                if (_propCategoryFocus)
                {
                    _propsMenuTilesCategoryIndex--;
                    if (_propsMenuTilesCategoryIndex < 0) _propsMenuTilesCategoryIndex = _tilesAsPropsCategoryIndices.Length - 1;
                    _propsMenuTilesIndex = 0;
                }
                else
                {
                    _propsMenuTilesIndex--;
                    if (_propsMenuTilesIndex < 0) _propsMenuTilesIndex = _tilesAsPropsIndices[_propsMenuTilesCategoryIndex].Length - 1;
                }
                break;
                    
            case 1: // TODO: ropes 
                break;
                    
            case 2: // props
                if (_propCategoryFocus)
                {
                    _propsMenuOthersCategoryIndex--;
                    if (_propsMenuOthersCategoryIndex < 0) _propsMenuOthersCategoryIndex = _propsOnly.Length - 1;
                    _propsMenuOthersIndex = 0;
                }
                else
                {
                    _propsMenuOthersIndex--;
                    if (_propsMenuOthersIndex < 0) _propsMenuOthersIndex = _propsOnly[_propsMenuOthersCategoryIndex].Length - 1;
                }
                break;
        }
    }

    private void IncrementMenuIndex()
    {
        switch (_menuRootCategoryIndex)
        {
            case 0:
                if (_propCategoryFocus)
                {
                    _propsMenuTilesCategoryIndex = ++_propsMenuTilesCategoryIndex % _tilesAsPropsCategoryIndices.Length;

                    if (_propsMenuTilesCategoryIndex == 0) _propsMenuTilesCategoryScrollIndex = 0;

                    _propsMenuTilesIndex = 0;
                }
                else
                {
                    _propsMenuTilesIndex = ++_propsMenuTilesIndex % _tilesAsPropsIndices[_propsMenuTilesCategoryIndex].Length;

                    if (_propsMenuTilesIndex == 0) _propsMenuTilesScrollIndex = 0;
                }
                break;
            
            case 1: break;
            
            case 2:
                if (_propCategoryFocus)
                {
                    _propsMenuOthersCategoryIndex++;
                    if (_propsMenuOthersCategoryIndex > _propsOnly.Length - 1) _propsMenuOthersCategoryIndex = 0;
                    _propsMenuOthersIndex = 0;
                }
                else
                {
                    _propsMenuOthersIndex++;
                    if (_propsMenuOthersIndex > _propsOnly[_propsMenuOthersCategoryIndex].Length - 1) _propsMenuOthersIndex = 0;
                }
                break;
        }
    }

    public void Draw()
    {
        if (_selected.Length != GLOBALS.Level.Props.Length)
        {
            _selected = new bool[GLOBALS.Level.Props.Length];
        }

        if (_hidden.Length != GLOBALS.Level.Props.Length)
        {
            _hidden = new bool[GLOBALS.Level.Props.Length];
        }

        GLOBALS.PreviousPage = 8;
        var scale = GLOBALS.Scale;
        var previewScale = GLOBALS.PreviewScale;

        var sWidth = GetScreenWidth();
        var sHeight = GetScreenHeight();

        var tileMouse = GetMousePosition();
        var tileMouseWorld = GetScreenToWorld2D(tileMouse, _camera);

        Rectangle menuPanelRect = new(sWidth - 360, 0, 360, sHeight);
        Rectangle ropePanelRect = new(0, 0, 250, 280);


        //                        v this was done to avoid rounding errors
        var tileMatrixY = tileMouseWorld.Y < 0 ? -1 : (int)tileMouseWorld.Y / previewScale;
        var tileMatrixX = tileMouseWorld.X < 0 ? -1 : (int)tileMouseWorld.X / previewScale;

        var canDrawTile = !CheckCollisionPointRec(tileMouse, menuPanelRect) &&
                           (!CheckCollisionPointRec(tileMouse, ropePanelRect) || !_showRopePanel);

        var inMatrixBounds = tileMatrixX >= 0 && tileMatrixX < GLOBALS.Level.Width && tileMatrixY >= 0 && tileMatrixY < GLOBALS.Level.Height;

        var currentTileAsPropCategory = _tilesAsPropsCategoryIndices[_propsMenuTilesCategoryIndex];

        int menuPageSize = (int)menuPanelRect.height / 30;

        if (_spinnerLock == 0)
        {
            if (IsKeyPressed(KeyboardKey.KEY_ONE))
            {
                GLOBALS.Page = 1;
            }
            if (IsKeyPressed(KeyboardKey.KEY_TWO))
            {
                GLOBALS.Page = 2;
            }

            if (IsKeyPressed(KeyboardKey.KEY_THREE))
            {
                GLOBALS.Page = 3;
            }

            if (IsKeyPressed(KeyboardKey.KEY_FOUR))
            {
                GLOBALS.Page = 4;
            }
            if (IsKeyPressed(KeyboardKey.KEY_FIVE))
            {
                GLOBALS.Page = 5;
            }
            if (IsKeyPressed(KeyboardKey.KEY_SIX))
            {
                GLOBALS.ResizeFlag = true;
                GLOBALS.Page = 6;
            }
            if (IsKeyPressed(KeyboardKey.KEY_SEVEN))
            {
                GLOBALS.Page = 7;
            }
            /*if (IsKeyPressed(KeyboardKey.KEY_EIGHT))
            {
                GLOBALS.Page = 8;
            }*/
            if (IsKeyPressed(KeyboardKey.KEY_NINE))
            {
                GLOBALS.Page = 9;
            }
        }
        else
        {
            if (IsKeyPressed(KeyboardKey.KEY_ESCAPE)) _spinnerLock = 0;
        }

        // handle mouse drag
        if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
        {
            var delta = GetMouseDelta();
            delta = RayMath.Vector2Scale(delta, -1.0f / _camera.zoom);
            _camera.target = RayMath.Vector2Add(_camera.target, delta);
        }

        // handle zoom
        var tileWheel = GetMouseWheelMove();
        if (tileWheel != 0 && canDrawTile)
        {
            var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
            _camera.offset = GetMousePosition();
            _camera.target = mouseWorldPosition;
            _camera.zoom += tileWheel * GLOBALS.ZoomIncrement;
            if (_camera.zoom < GLOBALS.ZoomIncrement) _camera.zoom = GLOBALS.ZoomIncrement;
        }
        
        // Cycle layer
        if (IsKeyPressed(KeyboardKey.KEY_L))
        {
            GLOBALS.Layer++;

            if (GLOBALS.Layer > 2) GLOBALS.Layer = 0;
        }

        if (IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
        {
            if (IsKeyPressed(KeyboardKey.KEY_Z)) _showLayer1Tiles = !_showLayer1Tiles;
            if (IsKeyPressed(KeyboardKey.KEY_X)) _showLayer2Tiles = !_showLayer2Tiles;
            if (IsKeyPressed(KeyboardKey.KEY_C)) _showLayer3Tiles = !_showLayer3Tiles;

            // Cycle Mode
            if (IsKeyPressed(KeyboardKey.KEY_E))
            {
                _mode = ++_mode % 2;
            }
            else if (IsKeyPressed(KeyboardKey.KEY_Q))
            {
                _mode--;
                if (_mode < 0) _mode = 1;
            }
        }
        else
        {
            if (IsKeyPressed(KeyboardKey.KEY_Z) && !_scalingProps) _showTileLayer1 = !_showTileLayer1;
            if (IsKeyPressed(KeyboardKey.KEY_X) && !_scalingProps) _showTileLayer2 = !_showTileLayer2;
            if (IsKeyPressed(KeyboardKey.KEY_C) && !_scalingProps) _showTileLayer3 = !_showTileLayer3;
        }

        // Mode-based hotkeys
        switch (_mode)
        {
            case 1: // Place Mode

                // Place Prop
                if (canDrawTile && IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    switch (_menuRootCategoryIndex)
                    {
                        case 0: // Tiles as props
                        {
                            var currentTileAsProp = _tilesAsPropsIndices[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
                            var width = (float)(currentTileAsProp.init.Size.Item1 + currentTileAsProp.init.BufferTiles*2) * GLOBALS.PreviewScale / 2;
                            var height = (float)(currentTileAsProp.init.Size.Item2 + currentTileAsProp.init.BufferTiles*2) * GLOBALS.PreviewScale / 2;
                            
                            GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
                                (
                                    InitPropType.Tile, 
                                    (currentTileAsPropCategory.index, currentTileAsProp.index),
                                    new Prop(
                                        GLOBALS.Layer * -10, 
                                        currentTileAsProp.init.Name, 
                                        true, 
                                        (currentTileAsPropCategory.index, currentTileAsProp.index), 
                                        new PropQuads(
                                        new(tileMouseWorld.X - width, tileMouseWorld.Y - height), 
                                        new(tileMouseWorld.X + width, tileMouseWorld.Y - height), 
                                        new(tileMouseWorld.X + width, tileMouseWorld.Y + height), 
                                        new(tileMouseWorld.X - width, tileMouseWorld.Y + height)
                                        )
                                    )
                                    {
                                        Extras = new(new BasicPropSettings(), [])
                                    }
                                )
                            ];
                        }
                            break;
                        
                        case 1: // Ropes
                            break;
                        
                        case 2: // Long Props
                            break;

                        case 3: // Others
                        {
                            var init = _propsOnly[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
                            var texture = GLOBALS.Textures.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
                            
                            var (width, height, settings) = init switch
                            {
                                InitVariedStandardProp variedStandard => (variedStandard.Size.x * GLOBALS.PreviewScale / 2f, variedStandard.Size.y * GLOBALS.PreviewScale / 2f, new PropVariedSettings()),
                                InitStandardProp standard => (standard.Size.x * GLOBALS.PreviewScale / 2f, standard.Size.y * GLOBALS.PreviewScale / 2f, new BasicPropSettings()),
                                InitVariedSoftProp variedSoft => (variedSoft.SizeInPixels.x  / 2f, variedSoft.SizeInPixels.y / 2f, new PropVariedSoftSettings()),
                                InitSoftProp => (texture.width  / 2f, texture.height  / 2f, new PropSoftSettings()),
                                InitVariedDecalProp variedDecal => (variedDecal.SizeInPixels.x  / 2f, variedDecal.SizeInPixels.y / 2f, new PropVariedDecalSettings()),
                                InitSimpleDecalProp => (texture.width / 2f, texture.height / 2f, new PropSimpleDecalSettings()), 
                                InitSoftEffectProp => (texture.width / 2f, texture.height / 2f, new PropSoftEffectSettings()), 
                                InitAntimatterProp => (texture.width / 2f, texture.height / 2f, new PropAntimatterSettings()),
                                InitLongProp => (texture.width / 2f, texture.height / 2f, new PropLongSettings()), 
                                InitRopeProp => (texture.width / 2f, texture.height / 2f, new PropRopeSettings()),
                                
                                _ => (texture.width / 2f, texture.height / 2f, new BasicPropSettings())
                            };
                            
                            GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
                                (
                                    init.Type, 
                                    (_propsMenuOthersCategoryIndex, _propsMenuOthersIndex),
                                    new Prop(GLOBALS.Layer * -10, init.Name, false, (_propsMenuOthersCategoryIndex, _propsMenuOthersIndex), new PropQuads(
                                        new(tileMouseWorld.X - width, tileMouseWorld.Y - height), 
                                        new(tileMouseWorld.X + width, tileMouseWorld.Y - height), 
                                        new(tileMouseWorld.X + width, tileMouseWorld.Y + height), 
                                        new(tileMouseWorld.X - width, tileMouseWorld.Y + height))
                                    )
                                    {
                                        Extras = new PropExtras(settings, [])
                                    }
                                )
                            ];
                        }
                            break;
                    }
                    
                    // Do not forget to update _selected and _hidden

                    _selected = [.._selected, false];
                    _hidden = [.._hidden, false];
                }
                
                if (IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
                {
                    // Cycle categories
                    if (IsKeyPressed(KeyboardKey.KEY_D))
                    {
                        _menuRootCategoryIndex++;
                        if (_menuRootCategoryIndex > 3) _menuRootCategoryIndex = 0;
                    }
                    else if (IsKeyPressed(KeyboardKey.KEY_A))
                    {
                        _menuRootCategoryIndex--;
                        if (_menuRootCategoryIndex < 0) _menuRootCategoryIndex = 3;
                    }
                }
                else
                {
                    // Navigate menu
                    if (IsKeyPressed(KeyboardKey.KEY_D))
                    {
                        _propCategoryFocus = false;
                    }
                    else if (IsKeyPressed(KeyboardKey.KEY_A))
                    {
                        _propCategoryFocus = true;
                    }

                    if (IsKeyPressed(KeyboardKey.KEY_S))
                    {
                        IncrementMenuIndex();
                    }
                    else if (IsKeyPressed(KeyboardKey.KEY_W))
                    {
                        DecrementMenuIndex();
                    }
                    
                    // Pickup Prop
                    if (IsKeyPressed(KeyboardKey.KEY_Q))
                    {
                        for (var i = 0; i < GLOBALS.Level.Props.Length; i++)
                        {
                            var current = GLOBALS.Level.Props[i];
        
                            if (!CheckCollisionPointRec(tileMouseWorld, Utils.EncloseQuads(current.prop.Quads)) || 
                                current.prop.Depth <= (GLOBALS.Layer + 1) * -10 || 
                                current.prop.Depth > GLOBALS.Layer * -10) 
                                continue;

                            if (current.type == InitPropType.Tile)
                            {
                                for (var c = 0; c < _tilesAsPropsIndices.Length; c++)
                                {
                                    for (var p = 0; p < _tilesAsPropsIndices[c].Length; p++)
                                    {
                                        var currentTileAsProp = _tilesAsPropsIndices[c][p];

                                        if (currentTileAsProp.init.Name != current.prop.Name) continue;

                                        currentTileAsPropCategory = _tilesAsPropsCategoryIndices[c];
                                        _propsMenuTilesCategoryIndex = c;
                                        _propsMenuTilesIndex = p;
                                    }
                                }
                            }
                            else if (current.type == InitPropType.Rope)
                            {
                                
                            }
                            else
                            {
                                (_propsMenuOthersCategoryIndex, _propsMenuOthersIndex) = current.position;
                            }
                        }
                    }
                }
                
                break;
            
            case 0: // Select Mode
                var anySelected = _selected.Any(s => s);
                var fetchedSelected = GLOBALS.Level.Props
                    .Select((prop, index) => (prop, index))
                    .Where(p => _selected[p.index])
                    .Select(p => p)
                    .ToArray();
                
                if (anySelected)
                {
                    _selectedPropsEncloser = Utils.EncloseProps(fetchedSelected.Select(p => p.prop.prop.Quads));
                    _selectedPropsCenter = new Vector2(
                        _selectedPropsEncloser.x + 
                        _selectedPropsEncloser.width/2, 
                        _selectedPropsEncloser.y + 
                        _selectedPropsEncloser.height/2
                    );

                    
                }
                else
                {
                    _selectedPropsEncloser.width = 0;
                    _selectedPropsEncloser.height = 0;

                    _selectedPropsCenter.X = 0;
                    _selectedPropsCenter.Y = 0;
                }
                
                // Move
                if (IsKeyPressed(KeyboardKey.KEY_F) && anySelected)
                {
                    _scalingProps = false;
                    _movingProps = true;
                    _rotatingProps = false;
                    _stretchingProp = false;
                    _editingPropPoints = false;
                    // _ropeMode = false;
                }
                // Rotate
                else if (IsKeyPressed(KeyboardKey.KEY_R) && anySelected)
                {
                    _scalingProps = false;
                    _movingProps = false;
                    _rotatingProps = true;
                    _stretchingProp = false;
                    _editingPropPoints = false;
                    // _ropeMode = false;
                }
                // Scale
                else if (IsKeyPressed(KeyboardKey.KEY_S) && anySelected)
                {
                    _movingProps = false;
                    _rotatingProps = false;
                    _stretchingProp = false;
                    _editingPropPoints = false;
                    _scalingProps = !_scalingProps;
                    _stretchAxes = 0;
                    // _ropeMode = false;
                    
                    SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_NESW);
                }
                // Hide
                else if (IsKeyPressed(KeyboardKey.KEY_H) && anySelected)
                {
                    for (var i = 0; i < GLOBALS.Level.Props.Length; i++)
                    {
                        if (_selected[i]) _hidden[i] = !_hidden[i];
                    }
                }
                // Edit Quads
                else if (IsKeyPressed(KeyboardKey.KEY_Q) && fetchedSelected.Length == 1)
                {
                    _scalingProps = false;
                    _movingProps = false;
                    _rotatingProps = false;
                    _stretchingProp = !_stretchingProp;
                    _editingPropPoints = false;
                    // _ropeMode = false;
                }
                // Delete
                else if (IsKeyPressed(KeyboardKey.KEY_D) && anySelected)
                {
                    _scalingProps = false;
                    _movingProps = false;
                    _rotatingProps = false;
                    _stretchingProp = false;
                    _editingPropPoints = false;
                    // _ropeMode = false;
                    
                    GLOBALS.Level.Props = _selected
                        .Select((s, i) => (s, i))
                        .Where(v => !v.Item1)
                        .Select(v => GLOBALS.Level.Props[v.Item2])
                        .ToArray();
                    
                    ImportRopeModels(); // don't forget to update the list when props list is modified
                }
                // Rope-only actions
                else if (
                    fetchedSelected.Length == 1 &&
                    fetchedSelected[0].prop.type == InitPropType.Rope
                )
                {
                    // Edit Rope Points
                    if (IsKeyPressed(KeyboardKey.KEY_P))
                    {
                        _scalingProps = false;
                        _movingProps = false;
                        _rotatingProps = false;
                        _stretchingProp = false;
                        _editingPropPoints = !_editingPropPoints;
                        _ropeMode = false;
                    }
                    // Rope mode
                    else if (IsKeyPressed(KeyboardKey.KEY_B))
                    {
                        // _scalingProps = false;
                        // _movingProps = false;
                        // _rotatingProps = false;
                        // _stretchingProp = false;
                        // _editingPropPoints = false;
                        _ropeMode = !_ropeMode;
                    }
                }
                else SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);

                if (_ropeMode && fetchedSelected.Length == 1)
                {
                    var foundRope = _models
                        .Single(rope => rope.index == fetchedSelected[0].index);

                    if (foundRope.simSwitch) // simulate
                    {
                        if (++_ropeSimulationFrame % _ropeSimulationFrameCut == 0)
                        {
                            foundRope.model.Update(
                            fetchedSelected[0].prop.prop.Quads, 
                            fetchedSelected[0].prop.prop.Depth switch
                            {
                                < -19 => 2,
                                < -9 => 1,
                                _ => 0
                            });
                        }
                    }
                    else // bezier
                    {
                        var ends = Utils.RopeEnds(fetchedSelected[0].prop.prop.Quads);
                        
                        fetchedSelected[0].prop.prop.Extras.RopePoints = Utils.Casteljau(fetchedSelected[0].prop.prop.Extras.RopePoints.Length, [ ends.pA, ..foundRope.bezierHandles, ends.pB ]);

                        if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                        {
                            for (var b = 0; b < foundRope.bezierHandles.Length; b++)
                            {
                                if (_bezierHandleLock == -1 && CheckCollisionPointCircle(tileMouseWorld, foundRope.bezierHandles[b], 3f))
                                    _bezierHandleLock = b;

                                if (_bezierHandleLock == b) foundRope.bezierHandles[b] = tileMouseWorld;
                            }
                        }

                        if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && _bezierHandleLock != -1)
                            _bezierHandleLock = -1;
                    }
                }
                
                // TODO: switch on enums instead
                if (_movingProps && anySelected)
                {
                    if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) _movingProps = false;
                    var delta = GetMouseDelta(); // TODO: Scale to world2D
                    if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
                    {
                        delta.X = 0;
                        delta.Y = 0;
                    }

                    for (var s = 0; s < _selected.Length; s++)
                    {
                        if (!_selected[s]) continue;
                        
                        var quads = GLOBALS.Level.Props[s].prop.Quads;

                        quads.TopLeft = RayMath.Vector2Add(quads.TopLeft, delta);
                        quads.TopRight = RayMath.Vector2Add(quads.TopRight, delta);
                        quads.BottomRight = RayMath.Vector2Add(quads.BottomRight, delta);
                        quads.BottomLeft = RayMath.Vector2Add(quads.BottomLeft, delta);

                        GLOBALS.Level.Props[s].prop.Quads = quads;

                        if (GLOBALS.Level.Props[s].type == InitPropType.Rope)
                        {
                            if (!_ropeMode)
                            {
                                for (var p = 0; p < GLOBALS.Level.Props[s].prop.Extras.RopePoints.Length; p++)
                                {
                                    GLOBALS.Level.Props[s].prop.Extras.RopePoints[p] = RayMath.Vector2Add(GLOBALS.Level.Props[s].prop.Extras.RopePoints[p], delta);
                                }
                            }

                            for (var r = 0; r < _models.Length; r++)
                            {
                                if (_models[r].index == s)
                                {
                                    for (var h = 0; h < _models[r].bezierHandles.Length; h++)
                                    {
                                        _models[r].bezierHandles[h] += delta;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (_rotatingProps && anySelected)
                {
                    if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) _rotatingProps = false;

                    var delta = GetMouseDelta();
                    
                    // Collective Rotation
                    
                    for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                    {
                        if (!_selected[p]) continue;
                        
                        var quads = GLOBALS.Level.Props[p].prop.Quads;

                        GLOBALS.Level.Props[p].prop.Quads = Utils.RotatePropQuads(quads, delta.X, _selectedPropsCenter);

                        if (GLOBALS.Level.Props[p].type == InitPropType.Rope)
                        {
                            Utils.RotatePoints(delta.X, _selectedPropsCenter, GLOBALS.Level.Props[p].prop.Extras.RopePoints);
                        }
                    }
                }
                else if (_scalingProps && anySelected)
                {
                    if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                    {
                        _stretchAxes = 0;
                        _scalingProps = false;
                        SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
                    }

                    if (IsKeyPressed(KeyboardKey.KEY_X))
                    {
                        _stretchAxes = (byte)(_stretchAxes == 1 ? 0 : 1);
                        SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_EW);
                    }
                    if (IsKeyPressed(KeyboardKey.KEY_Y))
                    {
                        _stretchAxes =  (byte)(_stretchAxes == 2 ? 0 : 2);
                        SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_NS);
                    }

                    var delta = GetMouseDelta();

                    switch (_stretchAxes)
                    {
                        case 0: // Uniform Scaling
                        {
                            var enclose = Utils.EncloseProps(fetchedSelected.Select(s => s.prop.prop.Quads));
                            var center = Utils.RectangleCenter(ref enclose);

                            foreach (var selected in fetchedSelected)
                            {
                                var quads = selected.prop.prop.Quads;
                                Utils.ScaleQuads(ref quads, center, 1f + delta.X*0.01f);
                                GLOBALS.Level.Props[selected.index].prop.Quads = quads;
                            }
                        }
                            break;

                        case 1: // X-axes Scaling
                        {
                            foreach (var selected in fetchedSelected)
                            {
                                var quads = selected.prop.prop.Quads;
                                var center = Utils.QuadsCenter(ref quads);
                                
                                Utils.ScaleQuadsX(ref quads, center, 1f + delta.X * 0.01f);
                                
                                GLOBALS.Level.Props[selected.index].prop.Quads = quads;
                            }
                        }
                            break;

                        case 2: // Y-axes Scaling
                        {
                            foreach (var selected in fetchedSelected)
                            {
                                var quads = selected.prop.prop.Quads;
                                var center = Utils.QuadsCenter(ref quads);

                                Utils.ScaleQuadsY(ref quads, center, 1f - delta.Y * 0.01f);
                                
                                GLOBALS.Level.Props[selected.index].prop.Quads = quads;
                            }
                        }
                            break;
                    }
                }
                else if (_stretchingProp && anySelected)
                {
                    var currentQuads = fetchedSelected[0].prop.prop.Quads; 

                    if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                    {
                        if (fetchedSelected[0].prop.type == InitPropType.Rope)
                        {
                            var middleLeft = RayMath.Vector2Divide(
                                RayMath.Vector2Add(currentQuads.TopLeft, currentQuads.BottomLeft),
                                new(2f, 2f)
                            );

                            var middleRight = RayMath.Vector2Divide(
                                RayMath.Vector2Add(currentQuads.TopRight, currentQuads.BottomRight),
                                new(2f, 2f)
                            );
                            
                            var beta = RayMath.Vector2Angle(RayMath.Vector2Subtract(middleLeft, middleRight), new(1.0f, 0.0f));
                            
                            var r = RayMath.Vector2Length(RayMath.Vector2Subtract(currentQuads.TopLeft, middleLeft));
                            
                            if (
                                CheckCollisionPointCircle(
                                    tileMouseWorld, middleLeft,
                                    5f
                                ) || _quadLock == 1)
                            {
                                _quadLock = 1;
                                currentQuads.TopLeft = RayMath.Vector2Add(
                                    tileMouseWorld, 
                                    new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                    );
                                
                                currentQuads.BottomLeft = RayMath.Vector2Add(
                                    tileMouseWorld, 
                                    new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                    );
                                
                                currentQuads.TopRight = RayMath.Vector2Add(
                                    middleRight, 
                                    new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                );
                                
                                currentQuads.BottomRight = RayMath.Vector2Add(
                                    middleRight, 
                                    new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                );
                            }

                            if (
                                CheckCollisionPointCircle(
                                    tileMouseWorld, middleRight,
                                    5f
                                    ) || _quadLock == 2)
                            {
                                _quadLock = 2;
                                
                                currentQuads.TopLeft = RayMath.Vector2Add(
                                    middleLeft, 
                                    new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                );
                                
                                currentQuads.BottomLeft = RayMath.Vector2Add(
                                    middleLeft, 
                                    new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                );
                                
                                currentQuads.TopRight = RayMath.Vector2Add(
                                    tileMouseWorld, 
                                    new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                );
                                
                                currentQuads.BottomRight = RayMath.Vector2Add(
                                    tileMouseWorld, 
                                    new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                );
                            }
                        }
                        else
                        {
                            if (CheckCollisionPointCircle(tileMouseWorld, currentQuads.TopLeft, 5f) && _quadLock == 0)
                            {
                                _quadLock = 1;
                            }
                            else if (CheckCollisionPointCircle(tileMouseWorld, currentQuads.TopRight, 5f) &&
                                     _quadLock == 0)
                            {
                                _quadLock = 2;
                            }
                            else if (CheckCollisionPointCircle(tileMouseWorld, currentQuads.BottomRight, 5f) &&
                                     _quadLock == 0)
                            {
                                _quadLock = 3;
                            }
                            else if (CheckCollisionPointCircle(tileMouseWorld, currentQuads.BottomLeft, 5f) &&
                                     _quadLock == 0)
                            {
                                _quadLock = 4;
                            }
                            
                            // Check Top-Left
                            if (_quadLock == 1)
                            {
                                currentQuads.TopLeft = tileMouseWorld;
                            }
                            // Check Top-Right
                            else if (_quadLock == 2)
                            {
                                currentQuads.TopRight = tileMouseWorld;
                            }
                            // Check Bottom-Right 
                            else if (_quadLock == 3)
                            {
                                currentQuads.BottomRight = tileMouseWorld;
                            }
                            // Check Bottom-Left
                            else if (_quadLock == 4)
                            {
                                currentQuads.BottomLeft = tileMouseWorld;
                            }
                        }
                        
                        GLOBALS.Level.Props[fetchedSelected[0].index].prop.Quads = currentQuads;
                    }
                    else if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && _quadLock != 0) _quadLock = 0;
                }
                else if (_editingPropPoints && fetchedSelected.Length == 1)
                {
                    var points = fetchedSelected[0].prop.prop.Extras.RopePoints;
                    
                    if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                    {
                        // Check Collision of Each Point

                        for (var p = 0; p < points.Length; p++)
                        {
                            if (CheckCollisionPointCircle(
                                    tileMouseWorld, 
                                    new Vector2(
                                        points[p].X/1.25f, 
                                        points[p].Y/1.25f
                                        ), 
                                    3f) || 
                                _pointLock == p
                            )
                            {
                                _pointLock = p;
                                points[p] = tileMouseWorld;
                            }
                        }
                    }
                    else if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && _pointLock != -1) _pointLock = -1;
                }
                else if (!_ropeMode)
                {
                    if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT) && !_clickTracker && canDrawTile)
                    {
                        _selection1 = GetScreenToWorld2D(GetMousePosition(), _camera);
                        _clickTracker = true;
                    }

                    if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && _clickTracker)
                    {
                        _clickTracker = false;
                    
                        // Selection rectangle should be now updated

                        // If selection rectangle is too small, it's treated
                        // like a point

                        for (var i = 0; i < GLOBALS.Level.Props.Length; i++)
                        {
                            var current = GLOBALS.Level.Props[i];
                            var propSelectRect = Utils.EncloseQuads(current.prop.Quads);
                            if (IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL))
                            {
                                if (CheckCollisionRecs(propSelectRect, _selection) && !(current.prop.Depth <= (GLOBALS.Layer + 1) * -10 || current.prop.Depth > GLOBALS.Layer * -10))
                                {
                                    _selected[i] = !_selected[i];
                                }
                            }
                            else
                            {
                                if (CheckCollisionRecs(propSelectRect, _selection) && !(current.prop.Depth <= (GLOBALS.Layer + 1) * -10 || current.prop.Depth > GLOBALS.Layer * -10))
                                {
                                    _selected[i] = true;
                                }
                                else
                                {
                                    _selected[i] = false;
                                }
                            }
                        }
                    }   
                }
                
                break;
        }


        #region TileEditorDrawing
        BeginDrawing();

        ClearBackground(new(170, 170, 170, 255));

        BeginMode2D(_camera);
        {
            DrawRectangle(0, 0, GLOBALS.Level.Width * previewScale, GLOBALS.Level.Height * previewScale, new Color(215, 215, 215, 255));

            #region TileEditorLayer3
            if (_showTileLayer3)
            {
                // Draw geos first

                Printers.DrawGeoLayer(
                    2, 
                    GLOBALS.PreviewScale, 
                    false, 
                    GLOBALS.Layer == 2 
                        ? BLACK 
                        : new(0, 0, 0, 120), 
                    _layerStackableFilter
                );

                // Then draw the tiles

                if (_showLayer3Tiles)
                {
                    Printers.DrawTileLayer(
                        2, 
                        GLOBALS.PreviewScale, 
                        false, 
                        !GLOBALS.Settings.TileEditor.UseTextures,
                        GLOBALS.Settings.TileEditor.TintedTiles
                    );
                }
                
                // Then draw the props

                for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                {
                    if (_hidden[p]) continue;
                    
                    var current = GLOBALS.Level.Props[p];
                    
                    // Filter based on depth
                    if (current.prop.Depth > -20 || current.type == InitPropType.Rope) continue;

                    var (category, index) = current.position;
                    var quads = current.prop.Quads;
                    
                    // origin must be the center
                    // var origin = new Vector2(tl.X + (tr.X - tl.X)/2f, tl.Y + (bl.Y - tl.Y)/2f);
                    
                    Printers.DrawProp(current.type, category, index, current.prop, GLOBALS.Settings.PropEditor.TintedTextures);
                    
                    // Draw Rope Point
                    if (current.type == InitPropType.Rope)
                    {
                        foreach (var point in current.prop.Extras.RopePoints)
                        {
                            DrawCircleV(point, 3f, WHITE);
                        }
                    }
                    
                    if (_selected[p])
                    {
                        // Side Lines
                        
                        DrawRectangleLinesEx(Utils.EncloseQuads(current.prop.Quads), 1.2f, BLUE);
                        
                        // Quad Points

                        if (_stretchingProp)
                        {
                            if (current.type == InitPropType.Rope)
                            {
                                DrawCircleV(
                                    RayMath.Vector2Divide(RayMath.Vector2Add(quads.TopLeft, quads.BottomLeft), 
                                        new(2f, 2f)), 
                                    5f, 
                                    BLUE
                                );
                                
                                DrawCircleV(
                                    RayMath.Vector2Divide(RayMath.Vector2Add(quads.TopRight, quads.BottomRight), 
                                        new(2f, 2f)), 
                                    5f, 
                                    BLUE
                                );
                                
                                DrawCircleV(quads.TopLeft, 2f, BLUE);
                                DrawCircleV(quads.TopRight, 2f, BLUE);
                                DrawCircleV(quads.BottomRight, 2f, BLUE);
                                DrawCircleV(quads.BottomLeft, 2f, BLUE);
                            }
                            else
                            {
                                DrawCircleV(quads.TopLeft, 5f, BLUE);
                                DrawCircleV(quads.TopRight, 5f, BLUE);
                                DrawCircleV(quads.BottomRight, 5f, BLUE);
                                DrawCircleV(quads.BottomLeft, 5f, BLUE);
                            }
                        }
                        else if (_scalingProps)
                        {
                            var center = Utils.QuadsCenter(ref quads);
                            
                            switch (_stretchAxes)
                            {
                                case 1:
                                    DrawLineEx(
                                        center with { X = -10 }, 
                                        center with { X = GLOBALS.Level.Width*GLOBALS.PreviewScale + 10 }, 
                                        2f, 
                                        RED
                                    );
                                    break;
                                case 2:
                                    DrawLineEx(
                                        center with { Y = -10 },
                                        center with { Y = GLOBALS.Level.Height*GLOBALS.PreviewScale + 10 },
                                        2f,
                                        GREEN
                                    );
                                    break;
                            }
                        }
                        
                        // Draw Rope Point
                        if (current.type == InitPropType.Rope)
                        {
                            if (_editingPropPoints)
                            {
                                foreach (var point in current.prop.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 3f, RED);
                                }
                            }
                            else
                            {
                                foreach (var point in current.prop.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 2f, ORANGE);
                                }
                            }
                            
                            if (_ropeMode)
                            {
                                // p is copied as suggested by the code editor..
                                var p1 = p;
                                var model = _models.Single(r => r.index == p1);

                                if (!model.simSwitch)
                                {
                                    foreach (var handle in model.bezierHandles) DrawCircleV(handle, 3f, GREEN);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region TileEditorLayer2
            if (_showTileLayer2)
            {
                if (GLOBALS.Layer != 2) DrawRectangle(0, 0, GLOBALS.Level.Width * previewScale, GLOBALS.Level.Height * previewScale, new(90, 90, 90, 120));

                Printers.DrawGeoLayer(
                    1, 
                    GLOBALS.PreviewScale, 
                    false, 
                    GLOBALS.Layer == 1 
                        ? BLACK 
                        : new(0, 0, 0, 120), 
                    _layerStackableFilter
                );

                // Draw layer 2 tiles

                if (_showLayer2Tiles)
                {
                    Printers.DrawTileLayer(
                        1, 
                        GLOBALS.PreviewScale, 
                        false, 
                        !GLOBALS.Settings.TileEditor.UseTextures,
                        GLOBALS.Settings.TileEditor.TintedTiles
                    );
                }
                
                // then draw the props

                for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                {
                    if (_hidden[p]) continue;
                    
                    var current = GLOBALS.Level.Props[p];
                    
                    // Filter based on depth
                    if (current.prop.Depth > -10 || current.prop.Depth < -19 || current.type == InitPropType.Rope) continue;

                    var (category, index) = current.position;
                    var quads = current.prop.Quads;
                    
                    // origin must be the center
                    // var origin = new Vector2(tl.X + (tr.X - tl.X)/2f, tl.Y + (bl.Y - tl.Y)/2f);
                    
                    Printers.DrawProp(current.type, category, index, current.prop, GLOBALS.Settings.PropEditor.TintedTextures);
                    
                    // Draw Rope Point
                    if (current.type == InitPropType.Rope)
                    {
                        foreach (var point in current.prop.Extras.RopePoints)
                        {
                            DrawCircleV(point, 3f, WHITE);
                        }
                    }
                    
                    if (_selected[p])
                    {
                        // Side Lines
                        
                        DrawRectangleLinesEx(Utils.EncloseQuads(current.prop.Quads), 1.2f, BLUE);
                        
                        // Quad Points

                        if (_stretchingProp)
                        {
                            if (current.type == InitPropType.Rope)
                            {
                                DrawCircleV(
                                    RayMath.Vector2Divide(RayMath.Vector2Add(quads.TopLeft, quads.BottomLeft), 
                                        new(2f, 2f)), 
                                    5f, 
                                    BLUE
                                );
                                
                                DrawCircleV(
                                    RayMath.Vector2Divide(RayMath.Vector2Add(quads.TopRight, quads.BottomRight), 
                                        new(2f, 2f)), 
                                    5f, 
                                    BLUE
                                );
                                
                                DrawCircleV(quads.TopLeft, 2f, BLUE);
                                DrawCircleV(quads.TopRight, 2f, BLUE);
                                DrawCircleV(quads.BottomRight, 2f, BLUE);
                                DrawCircleV(quads.BottomLeft, 2f, BLUE);
                            }
                            else
                            {
                                DrawCircleV(quads.TopLeft, 5f, BLUE);
                                DrawCircleV(quads.TopRight, 5f, BLUE);
                                DrawCircleV(quads.BottomRight, 5f, BLUE);
                                DrawCircleV(quads.BottomLeft, 5f, BLUE);
                            }
                        }
                        else if (_scalingProps)
                        {
                            var center = Utils.QuadsCenter(ref quads);
                            
                            switch (_stretchAxes)
                            {
                                case 1:
                                    DrawLineEx(
                                        center with { X = -10 }, 
                                        center with { X = GLOBALS.Level.Width*GLOBALS.PreviewScale + 10 }, 
                                        2f, 
                                        RED
                                    );
                                    break;
                                case 2:
                                    DrawLineEx(
                                        center with { Y = -10 },
                                        center with { Y = GLOBALS.Level.Height*GLOBALS.PreviewScale + 10 },
                                        2f,
                                        GREEN
                                    );
                                    break;
                            }
                        }
                        
                        // Draw Rope Point
                        if (current.type == InitPropType.Rope)
                        {
                            if (_editingPropPoints)
                            {
                                foreach (var point in current.prop.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 3f, RED);
                                }
                            }
                            else
                            {
                                foreach (var point in current.prop.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 2f, ORANGE);
                                }
                            }
                            
                            if (_ropeMode)
                            {
                                // p is copied as suggested by the code editor..
                                var p1 = p;
                                var model = _models.Single(r => r.index == p1);

                                if (!model.simSwitch)
                                {
                                    foreach (var handle in model.bezierHandles) DrawCircleV(handle, 3f, GREEN);
                                }
                            }
                        }
                    }
                }
                
            }
            #endregion

            #region TileEditorLayer1
            if (_showTileLayer1)
            {
                if (GLOBALS.Layer != 1 && GLOBALS.Layer != 2) DrawRectangle(0, 0, GLOBALS.Level.Width * previewScale, GLOBALS.Level.Height * previewScale, new(100, 100, 100, 100));

                Printers.DrawGeoLayer(
                    0, 
                    GLOBALS.PreviewScale, 
                    false, 
                    GLOBALS.Layer == 0
                        ? BLACK 
                        : new(0, 0, 0, 120)
                );

                // Draw layer 1 tiles

                if (_showLayer1Tiles)
                {
                    Printers.DrawTileLayer(
                        0, 
                        GLOBALS.PreviewScale, 
                        false, 
                        !GLOBALS.Settings.TileEditor.UseTextures,
                        GLOBALS.Settings.TileEditor.TintedTiles
                    );
                }
                
                // then draw the props

                for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                {
                    if (_hidden[p]) continue;
                    
                    var current = GLOBALS.Level.Props[p];
                    
                    // Filter based on depth
                    if (current.prop.Depth < -9) continue;

                    var (category, index) = current.position;
                    var quads = current.prop.Quads;
                    
                    // origin must be the center
                    // var origin = new Vector2(tl.X + (tr.X - tl.X)/2f, tl.Y + (bl.Y - tl.Y)/2f);
                    
                    Printers.DrawProp(current.type, category, index, current.prop, GLOBALS.Settings.PropEditor.TintedTextures);
                    
                    // Draw Rope Point
                    if (current.type == InitPropType.Rope)
                    {
                        foreach (var point in current.prop.Extras.RopePoints)
                        {
                            DrawCircleV(point, 3f, WHITE);
                        }
                    }
                    
                    if (_selected[p])
                    {
                        // Side Lines
                        
                        DrawRectangleLinesEx(Utils.EncloseQuads(current.prop.Quads), 1.2f, BLUE);
                        
                        // Quad Points

                        if (_stretchingProp)
                        {
                            if (current.type == InitPropType.Rope)
                            {
                                DrawCircleV(
                                    RayMath.Vector2Divide(RayMath.Vector2Add(quads.TopLeft, quads.BottomLeft), 
                                        new(2f, 2f)), 
                                    5f, 
                                    BLUE
                                );
                                
                                DrawCircleV(
                                    RayMath.Vector2Divide(RayMath.Vector2Add(quads.TopRight, quads.BottomRight), 
                                        new(2f, 2f)), 
                                    5f, 
                                    BLUE
                                );
                                
                                DrawCircleV(quads.TopLeft, 2f, BLUE);
                                DrawCircleV(quads.TopRight, 2f, BLUE);
                                DrawCircleV(quads.BottomRight, 2f, BLUE);
                                DrawCircleV(quads.BottomLeft, 2f, BLUE);
                            }
                            else
                            {
                                DrawCircleV(quads.TopLeft, 5f, BLUE);
                                DrawCircleV(quads.TopRight, 5f, BLUE);
                                DrawCircleV(quads.BottomRight, 5f, BLUE);
                                DrawCircleV(quads.BottomLeft, 5f, BLUE);
                            }
                        }
                        else if (_scalingProps)
                        {
                            var center = Utils.QuadsCenter(ref quads);
                            
                            switch (_stretchAxes)
                            {
                                case 1:
                                    DrawLineEx(
                                        center with { X = -10 }, 
                                        center with { X = GLOBALS.Level.Width*GLOBALS.PreviewScale + 10 }, 
                                        2f, 
                                        RED
                                    );
                                    break;
                                case 2:
                                    DrawLineEx(
                                        center with { Y = -10 },
                                        center with { Y = GLOBALS.Level.Height*GLOBALS.PreviewScale + 10 },
                                        2f,
                                        GREEN
                                    );
                                    break;
                            }
                        }
                        
                        // Draw Rope Point
                        if (current.type == InitPropType.Rope)
                        {
                            if (_editingPropPoints)
                            {
                                foreach (var point in current.prop.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 3f, RED);
                                }
                            }
                            else
                            {
                                foreach (var point in current.prop.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 2f, ORANGE);
                                }
                            }

                            if (_ropeMode)
                            {
                                // p is copied as suggested by the code editor..
                                var p1 = p;
                                var model = _models.Single(r => r.index == p1);

                                if (!model.simSwitch)
                                {
                                    foreach (var handle in model.bezierHandles) DrawCircleV(handle, 3f, GREEN);
                                }
                            }
                        }
                    }
                }
                
            }
            #endregion
            
            // Draw the enclosing rectangle for selected props
            // DEBUG: DrawRectangleLinesEx(_selectedPropsEncloser, 3f, WHITE);

            switch (_mode)
            {
                case 1: // Place Mode
                    switch (_menuRootCategoryIndex)
            {
                case 0: // Current Tile-As-Prop
                    {
                        #if DEBUG
                        if (_propsMenuTilesCategoryIndex >= _tilesAsPropsIndices.Length)
                        {
                            _logger.Fatal($"failed to fetch current tile-as-prop from {nameof(_tilesAsPropsIndices)}[{_tilesAsPropsIndices.Length}]: {nameof(_propsMenuTilesCategoryIndex)} ({_propsMenuTilesCategoryIndex} was out of bounds)");
                            throw new IndexOutOfRangeException(message: $"failed to fetch current tile-as-prop from {nameof(_tilesAsPropsIndices)}[{_tilesAsPropsIndices.Length}]: {nameof(_propsMenuTilesCategoryIndex)} ({_propsMenuTilesCategoryIndex} was out of bounds)");
                        }

                        if (_propsMenuTilesIndex >=
                            _tilesAsPropsIndices[_propsMenuTilesCategoryIndex].Length)
                        {
                            _logger.Fatal($"failed to fetch current tile-as-prop from {nameof(_tilesAsPropsIndices)}[{_tilesAsPropsIndices[_propsMenuTilesCategoryIndex].Length}]: {nameof(_propsMenuTilesIndex)} ({_propsMenuTilesIndex} was out of bounds)");
                            throw new IndexOutOfRangeException(message: $"failed to fetch current tile-as-prop from {nameof(_tilesAsPropsIndices)}[{_tilesAsPropsIndices[_propsMenuTilesCategoryIndex].Length}]: {nameof(_propsMenuTilesIndex)} ({_propsMenuTilesIndex} was out of bounds)");
                        }
                        #endif

                        var currentTileAsProp = _tilesAsPropsIndices[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
                        var currentTileAsPropTexture = GLOBALS.Textures.Tiles[currentTileAsPropCategory.index][currentTileAsProp.index];
                        
                        var layerHeight = (currentTileAsProp.init.Size.Item2 + (currentTileAsProp.init.BufferTiles * 2)) * scale;
                        var textureCutWidth = (currentTileAsProp.init.Size.Item1 + (currentTileAsProp.init.BufferTiles * 2)) * scale;
                        const float scaleConst = 0.4f;

                        var width = scaleConst * textureCutWidth;
                        var height = scaleConst * layerHeight;

                        Printers.DrawTileAsProp(
                            ref currentTileAsPropTexture,
                            ref currentTileAsProp.init,
                            ref tileMouseWorld,
                            [
                                new(width, -height),
                                new(-width, -height),
                                new(-width, height),
                                new(width, height),
                                new(width, -height)
                            ]
                        );
                    }
                    break;
                
                case 1: // TODO: Current Rope
                    break;
                
                case 2: // TODO: Current Long Prop
                    break;

                case 3: // Current Prop
                {
                    var prop = _propsOnly[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
                    var texture = GLOBALS.Textures.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
                    
                    var (width, height, settings) = prop switch
                    {
                        InitVariedStandardProp variedStandard => (variedStandard.Size.x * GLOBALS.PreviewScale / 2f, variedStandard.Size.y * GLOBALS.PreviewScale / 2f, new PropVariedSettings()),
                        InitStandardProp standard => (standard.Size.x * GLOBALS.PreviewScale / 2f, standard.Size.y * GLOBALS.PreviewScale / 2f, new BasicPropSettings()),
                        InitVariedSoftProp variedSoft => (variedSoft.SizeInPixels.x  / 2f, variedSoft.SizeInPixels.y / 2f, new PropVariedSoftSettings()),
                        InitSoftProp => (texture.width  / 2f, texture.height  / 2f, new PropSoftSettings()),
                        InitVariedDecalProp variedDecal => (variedDecal.SizeInPixels.x  / 2f, variedDecal.SizeInPixels.y / 2f, new PropVariedDecalSettings()),
                        InitSimpleDecalProp => (texture.width / 2f, texture.height / 2f, new PropSimpleDecalSettings()), 
                        InitSoftEffectProp => (texture.width / 2f, texture.height / 2f, new PropSoftEffectSettings()), 
                        InitAntimatterProp => (texture.width / 2f, texture.height / 2f, new PropAntimatterSettings()),
                        InitLongProp => (texture.width / 2f, texture.height / 2f, new PropLongSettings()), 
                        InitRopeProp => (texture.width / 2f, texture.height / 2f, new PropRopeSettings()),
                                
                        _ => (texture.width / 2f, texture.height / 2f, new BasicPropSettings())
                    };
                    
                    Printers.DrawProp(settings, prop, ref texture, new PropQuads(
                        new Vector2(tileMouseWorld.X - width, tileMouseWorld.Y - height), 
                        new Vector2(tileMouseWorld.X + width, tileMouseWorld.Y - height), 
                        new Vector2(tileMouseWorld.X + width, tileMouseWorld.Y + height), 
                        new Vector2(tileMouseWorld.X - width, tileMouseWorld.Y + height)),
                        0
                    );
                }
                    break;
            }
                    break;
                
                case 0: // Select Mode
                    
                    // TODO: tweak selection cancellation
                    if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT) && _clickTracker)
                    {
                        var mouse = GetScreenToWorld2D(GetMousePosition(), _camera);
                        var diff = RayMath.Vector2Subtract(mouse, _selection1);
                        var position = (diff.X > 0, diff.Y > 0) switch
                        {
                            (true, true) => _selection1,
                            (true, false) => new Vector2(_selection1.X, mouse.Y),
                            (false, true) => new Vector2(mouse.X, _selection1.Y),
                            (false, false) => mouse
                        };

                        _selection = new Rectangle(
                            position.X, 
                            position.Y, 
                            Math.Abs(diff.X), 
                            Math.Abs(diff.Y)
                        );
                        
                        DrawRectangleRec(_selection, new Color(0, 0, 255, 90));
                        
                        DrawRectangleLinesEx(
                            _selection,
                            2f,
                            BLUE
                        );
                    }
                    break;
            }

        }
        EndMode2D();

        #region TileEditorUI
        {
            // Coordinates

            if (GLOBALS.Settings.TileEditor.HoveredTileInfo && canDrawTile)
            {
                if (inMatrixBounds) DrawText(
                    $"x: {tileMatrixX}, y: {tileMatrixY}\n{GLOBALS.Level.TileMatrix[tileMatrixY, tileMatrixX, GLOBALS.Layer]}",
                    tileMouse.X + previewScale,
                    tileMouse.Y + previewScale,
                    15,
                    WHITE
                );
            }
            else
            {
                if (inMatrixBounds) DrawText(
                    $"x: {tileMatrixX}, y: {tileMatrixY}",
                    tileMouse.X + previewScale,
                    tileMouse.Y + previewScale,
                    15,
                    WHITE
                );
            }

            // Menu

            unsafe
            {
                fixed (byte* pt = _menuPanelBytes)
                {
                    RayGui.GuiPanel(
                        menuPanelRect,
                        (sbyte*)pt
                    );
                }
            }
            
            // Panel
            switch (_mode)
            {
                case 1: // Place Mode
                {
                    DrawRectangleRec(new(menuPanelRect.x + 30 + (_menuRootCategoryIndex * 40), 40, 40, 40), BLUE);

                    DrawTexture(
                        GLOBALS.Textures.PropMenuCategories[0], 
                        (int)menuPanelRect.x + 30, 
                        40, 
                        _menuRootCategoryIndex == 0 ? WHITE : BLACK
                    );

                    DrawTexture(
                        GLOBALS.Textures.PropMenuCategories[1], 
                        (int)menuPanelRect.x + 30 + 40, 
                        40, 
                        _menuRootCategoryIndex == 1 ? WHITE : BLACK
                    );
            
                    DrawTexture(
                        GLOBALS.Textures.PropMenuCategories[2], 
                        (int)menuPanelRect.x + 30 + 80, 
                        40, 
                        _menuRootCategoryIndex == 2 ? WHITE : BLACK
                    );
                    
                    DrawTexture(
                        GLOBALS.Textures.PropMenuCategories[3], 
                        (int)menuPanelRect.x + 30 + 120, 
                        40, 
                        _menuRootCategoryIndex == 3 ? WHITE : BLACK
                    );
                    
                    DrawText(
                        _menuCategoryNames[_menuRootCategoryIndex], 
                        (int)menuPanelRect.x + 30 + 190,
                        50,
                        20,
                        BLACK
                    );

            Rectangle categoryRect = new((int)(menuPanelRect.x + 5), 90, 145, (int)(menuPanelRect.height - 300));
            Rectangle listRect = new((int)(menuPanelRect.x + 155), 90, menuPanelRect.width - 160, (int)(menuPanelRect.height - 300));
            
            switch (_menuRootCategoryIndex)
            {
                case 0: // Tiles as props
                    {
                        int newCategoryIndex;

                        unsafe
                        {
                            fixed (int* scrollIndex = &_propsMenuTilesCategoryScrollIndex)
                            {
                                // draw the category list first
                                newCategoryIndex = RayGui.GuiListView(
                                    categoryRect,
                                    string.Join(";", from c in _tilesAsPropsCategoryIndices select c.category),
                                    scrollIndex,
                                    _propsMenuTilesCategoryIndex);
                            }
                        }

                        if (newCategoryIndex != _propsMenuTilesCategoryIndex)
                        {
                            _propsMenuTilesIndex = 0;
                            _propsMenuTilesCategoryIndex = newCategoryIndex;
                        }

                        unsafe
                        {
                            fixed (int* scrollIndex = &_propsMenuTilesScrollIndex)
                            {
                                // draw the list

                                _propsMenuTilesIndex = RayGui.GuiListView(
                                    listRect,
                                    string.Join(";", from t in _tilesAsPropsIndices[_propsMenuTilesCategoryIndex] select t.init.Name),
                                    scrollIndex,
                                    _propsMenuTilesIndex
                                );
                            }
                        }
                    }
                    break;
                
                case 1: // TODO: Ropes
                    break;
                
                case 2: // TODO: Long Props
                    break;

                case 3: // Props
                {
                    int newCategoryIndex;

                    unsafe
                    {
                        fixed (int* scrollIndex = &_propsMenuOthersCategoryScrollIndex)
                        {
                            // draw the category list first
                            newCategoryIndex = RayGui.GuiListView(
                                categoryRect,
                                string.Join(";", from c in _propCategoriesOnly select c.name),
                                scrollIndex,
                                _propsMenuOthersCategoryIndex);
                        }
                    }
                    
                    // reset selection index when changing categories
                    if (newCategoryIndex != _propsMenuOthersCategoryIndex)
                    {
                        _propsMenuOthersIndex = 0;
                        _propsMenuOthersCategoryIndex = newCategoryIndex;
                    }
                    
                    unsafe
                    {
                        fixed (int* scrollIndex = &_propsMenuOthersScrollIndex)
                        {
                            // draw the list

                            _propsMenuOthersIndex = RayGui.GuiListView(
                                listRect,
                                string.Join(";", from t in _propsOnly[_propsMenuOthersCategoryIndex] select t.Name),
                                scrollIndex,
                                _propsMenuOthersIndex
                            );
                        }
                    }
                }
                    break;
            }

            // Focus indicator
            DrawRectangleLinesEx(
                _propCategoryFocus ? categoryRect : listRect,
                4f,
                BLUE
            );
                }
                    break;

                case 0: // Select Mode
                {
                    var fetchedSelected = GLOBALS.Level.Props
                        .Select((prop, index) => (prop, index))
                        .Where(p => _selected[p.index])
                        .Select(p => p)
                        .ToArray();

                    
                    // Rope Panel
                    if (fetchedSelected.Length == 1 && fetchedSelected[0].prop.type == InitPropType.Rope)
                    {
                        _showRopePanel = true;

                        var modelIndex = -1;

                        for (var i = 0; i < _models.Length; i++)
                        {
                            if (_models[i].index == fetchedSelected[0].index) modelIndex = i;
                        }

                        if (modelIndex == -1)
                        {
#if DEBUG
                            _logger.Fatal(
                                $"failed to fetch selected rope from {nameof(_models)}: no element with index [{fetchedSelected[0].index}] was found");
                            throw new Exception(
                                message:
                                $"failed to fetch selected rope from {nameof(_models)}: no element with index [{fetchedSelected[0].index}] was found");
#else
                            goto ropeNotFound;
#endif
                        }

                        ref var currentModel = ref _models[modelIndex];

                        unsafe
                        {
                            fixed (byte* pt = _ropePanelBytes)
                            {
                                RayGui.GuiPanel(ropePanelRect, (sbyte*)pt);
                            }
                        }

                        var simButton = new Rectangle(ropePanelRect.X + 10, ropePanelRect.Y + 40,
                            ropePanelRect.width - 20, 40);

                        if (
                            RayGui.GuiButton(
                                simButton,
                                currentModel.simSwitch ? "Simulation" : "Bezier Path"
                            )
                        ) currentModel.simSwitch = !currentModel.simSwitch;

                        var oldSegmentCount = GLOBALS.Level.Props[currentModel.index].prop.Extras.RopePoints.Length;
                        var segmentCount = oldSegmentCount;

                        unsafe
                        {
                            RayGui.GuiSpinner(
                                simButton with { Y = 100, X = ropePanelRect.X + 90, width = ropePanelRect.width - 100 },
                                "segments",
                                &segmentCount,
                                3,
                                100,
                                false
                            );
                        }

                        // Update segment count if needed


                        if (segmentCount > oldSegmentCount)
                        {
                            GLOBALS.Level.Props[currentModel.index].prop.Extras.RopePoints =
                            [
                                ..GLOBALS.Level.Props[currentModel.index].prop.Extras.RopePoints, new Vector2()
                            ];
                        }
                        else if (segmentCount < oldSegmentCount)
                        {
                            GLOBALS.Level.Props[currentModel.index].prop.Extras.RopePoints =
                                GLOBALS.Level.Props[currentModel.index].prop.Extras.RopePoints[..^1];
                        }

                        if (segmentCount != oldSegmentCount) UpdateRopeModelSegments();

                        //

                        _ropeMode = RayGui.GuiCheckBox(
                            simButton with { Y = 150, X = ropePanelRect.X + 10, width = 20, height = 20 },
                            "Toggle Editing",
                            _ropeMode
                        );

                        if (currentModel.simSwitch) // Simulation mode
                        {
                            if (RayGui.GuiButton(simButton with
                                {
                                    Y = ropePanelRect.Y + ropePanelRect.height - 50
                                },
                                $"{60 / _ropeSimulationFrameCut} FPS")) _ropeSimulationFrameCut = ++_ropeSimulationFrameCut % 3 + 1;
                        }
                        else // Bezier mode
                        {
                            var oldHandlePointNumber = currentModel.bezierHandles.Length;
                            var handlePointNumber = oldHandlePointNumber;

                            unsafe
                            {
                                RayGui.GuiSpinner(
                                    simButton with
                                    {
                                        Y = 180, X = ropePanelRect.X + 90, width = ropePanelRect.width - 100
                                    },
                                    "Control Points",
                                    &handlePointNumber,
                                    1,
                                    4,
                                    false
                                );
                            }

                            var quads = GLOBALS.Level.Props[currentModel.index].prop.Quads;
                            var center = Utils.QuadsCenter(ref quads);

                            if (handlePointNumber > oldHandlePointNumber)
                            {
                                currentModel.bezierHandles = [..currentModel.bezierHandles, center];
                            }
                            else if (handlePointNumber < oldHandlePointNumber)
                            {
                                currentModel.bezierHandles = currentModel.bezierHandles[..^1];
                            }
                        }

                        ropeNotFound:
                        {
                        }
                    }
                    else _showRopePanel = false;
                    //
                    
                    var listRect = new Rectangle(
                        (int)(menuPanelRect.x + 5), 
                        90, 
                        menuPanelRect.width - 10, 
                        (int)(menuPanelRect.height - 400)
                    );
                    
                    DrawRectangleLinesEx(listRect, 1.2f, GRAY);

                    var pageSize = (int) listRect.height / 24;
                    
                    // Hide-all Checkbox

                    var isAllHidden = _hidden.All(h => h);

                    var hideAll = RayGui.GuiCheckBox(
                        new Rectangle(sWidth - 80, listRect.y - 30, 20, 20),
                        "",
                        isAllHidden
                    );
                    
                    if (!hideAll && isAllHidden)
                    {
                        for (var i = 0; i < _hidden.Length; i++) _hidden[i] = false;
                    }
                    else if (hideAll)
                    {
                        for (var i = 0; i < _hidden.Length; i++) _hidden[i] = true;
                    }
                    
                    // Select-all Checkbox

                    var isAllSelected = _selected.All(h => h);
                    
                    var selectAll = RayGui.GuiCheckBox(
                        new Rectangle(sWidth - 35, listRect.y - 30, 20, 20),
                        "",
                        isAllSelected
                    );

                    if (!selectAll && isAllSelected)
                    {
                        for (var i = 0; i < _selected.Length; i++) _selected[i] = false;
                    }
                    else if (selectAll)
                    {
                        for (var i = 0; i < _selected.Length; i++) _selected[i] = true;
                    }
                    
                    // Delete Selected

                    var deleteRect = new Rectangle(menuPanelRect.x + 10, listRect.y - 30, 20, 20);
                    var deleteHovered = CheckCollisionPointRec(tileMouse, deleteRect);
                    
                    DrawRectangleRec(
                        deleteRect,
                        deleteHovered ? new(255, 50, 50, 255) : RED
                    );
                    
                    if (deleteHovered && IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                    {
                        GLOBALS.Level.Props = _selected
                            .Select((s, i) => (s, i))
                            .Where(v => !v.s)
                            .Select(v => GLOBALS.Level.Props[v.i])
                            .ToArray();
                    }
                    
                    DrawLineEx(new(menuPanelRect.x + 11, listRect.y - 32), new(menuPanelRect.x + 29, listRect.y - 11), 2f, WHITE);
                    DrawLineEx(new(menuPanelRect.x + 11, listRect.y - 11), new(menuPanelRect.x + 29, listRect.y - 32), 2f, WHITE);
                    
                    // Prop Settings

                    var indicatorOffset = listRect.x + (listRect.width - 290) / 2f;
                    
                    if (fetchedSelected.Length == 1)
                    {
                        // Depth indicator
                        
                        var (c, i) = fetchedSelected[0].prop.prop.Position;

                        switch (fetchedSelected[0].prop.type)
                        {
                            case InitPropType.Tile:
                            {
                                InitTile init;
                                
                                #if DEBUG
                                try
                                {
                                    init = GLOBALS.Tiles[c][i];
                                }
                                catch (IndexOutOfRangeException e)
                                {
                                    _logger.Fatal($"failed to fetch tile init from {nameof(GLOBALS.Tiles)}[{GLOBALS.Tiles.Length}]: c or i ({c}, {i}) were out of bounds");
                                    throw new IndexOutOfRangeException(
                                        message:
                                        $"failed to fetch tile init from {nameof(GLOBALS.Tiles)}[{GLOBALS.Tiles.Length}]: c or i ({c}, {i}) were out of bounds",
                                        innerException: e);
                                }
                                #else
                                init = GLOBALS.Tiles[c][i];
                                #endif
                                
                                var depth = init.Repeat.Sum() * 10;
                                var offset = indicatorOffset - fetchedSelected[0].prop.prop.Depth * 10;
                                var overflow = offset + depth - (indicatorOffset + 290);
                            
                                DrawRectangleRec(
                                    new Rectangle(
                                        offset, 
                                        listRect.y + listRect.height + 10, 
                                        depth - (overflow > 0 ? overflow : 0), 
                                        30
                                    ),
                                    new Color(100, 100, 180, 255)
                                );
                            }
                                break;
                            
                            case InitPropType.Long:
                                break;
                            
                            case InitPropType.Rope:
                                break;

                            default:
                            {
                                InitPropBase init;
                                #if DEBUG
                                try { init = GLOBALS.Props[c][i]; }
                                catch (IndexOutOfRangeException e)
                                {
                                    _logger.Fatal($"failed to fetch prop init from {nameof(GLOBALS.Props)}[{GLOBALS.Props.Length}]: c, or i ({c}, {i}) were out of bounds");
                                    throw new IndexOutOfRangeException(message: $"failed to fetch prop init from {nameof(GLOBALS.Props)}[{GLOBALS.Props.Length}]: c, or i ({c}, {i}) were out of bounds", innerException: e);
                                }
                                
                                DrawRectangleRec(
                                    new Rectangle(
                                        indicatorOffset - fetchedSelected[0].prop.prop.Depth * 10, 
                                        listRect.y + listRect.height + 10, 
                                        init switch
                                        {
                                            InitVariedStandardProp v => v.Repeat.Length, 
                                            InitStandardProp s => s.Repeat.Length, 
                                            _ => init.Depth
                                        } * 10, 
                                        30
                                    ),
                                    new Color(100, 100, 180, 255)
                                );
                                #else
                                DrawRectangleRec(
                                    new Rectangle(
                                        indicatorOffset - fetchedSelected[0].prop.prop.Depth * 10, 
                                        listRect.y + listRect.height + 10, 
                                        init switch
                                        {
                                            InitVariedStandardProp v => v.Repeat.Length, 
                                            InitStandardProp s => s.Repeat.Length, 
                                            _ => init.Depth
                                        } * 10, 
                                        30
                                    ),
                                    new Color(100, 100, 180, 255)
                                );
                                #endif
                            }
                                break;
                        }
                        
                        DrawRectangleLinesEx(
                            new Rectangle(indicatorOffset, listRect.y + listRect.height + 10, 290, 30),
                            2f,
                            BLACK
                        );
                    
                        DrawLineEx(
                            new Vector2(indicatorOffset + 90, listRect.y + listRect.height + 10),
                            new Vector2(indicatorOffset + 90, listRect.y + listRect.height + 15),
                            2f,
                            BLACK
                        );
                    
                        DrawLineEx(
                            new Vector2(indicatorOffset + 180, listRect.y + listRect.height + 10),
                            new Vector2(indicatorOffset + 180, listRect.y + listRect.height + 15),
                            2f,
                            BLACK
                        );
                        
                        unsafe
                        {
                            int depth;
                            
                            #if DEBUG
                            try
                            {
                                depth = -fetchedSelected[0].prop.prop.Depth;
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                _logger.Fatal($"failed to fetch the depth of the selected prop: {nameof(fetchedSelected)} was empty when expected to hold exactly one element");
                                throw new IndexOutOfRangeException($"failed to fetch the depth of the selected prop: {nameof(fetchedSelected)} was empty when expected to hold exactly one element", innerException: e);
                            }
                            #else
                            depth = -fetchedSelected[0].prop.prop.Depth;
                            #endif

                            if (RayGui.GuiSpinner(
                                new Rectangle(indicatorOffset, listRect.y + listRect.height + 50, 290, 40),
                                "",
                                &depth,
                                0,
                                29,
                                _spinnerLock == 1
                            )) _spinnerLock = 1;
                            
                            #if DEBUG
                            try
                            {
                                fetchedSelected[0].prop.prop.Depth = -depth;
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                _logger.Fatal($"failed to update depth: {nameof(fetchedSelected)} was empty when it was supposed to hold exactly one element");
                                throw new IndexOutOfRangeException(
                                    message: $"failed to update depth: {nameof(fetchedSelected)} was empty when it was supposed to hold exactly one element", 
                                    innerException: e);
                            }

                            try
                            {
                                GLOBALS.Level.Props[fetchedSelected[0].index].prop.Depth = -depth;
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                _logger.Fatal($"failed to update prop in {nameof(GLOBALS.Level.Props)}: fetchedSelected[0].index = {fetchedSelected[0].index} held an out-of-bounds index");
                                throw new IndexOutOfRangeException(
                                    message:
                                    $"failed to update prop in {nameof(GLOBALS.Level.Props)}: fetchedSelected[0].index = {fetchedSelected[0].index} held an out-of-bounds index",
                                    innerException: e);
                            }
                            #else
                            fetchedSelected[0].prop.prop.Depth = -depth;
                            GLOBALS.Level.Props[fetchedSelected[0].index].prop.Depth = -depth;
                            #endif
                            
                            
                            // Variation Selector

                            BasicPropSettings propSettings;
                            
                            #if DEBUG
                            try
                            {
                                propSettings = fetchedSelected[0].prop.prop.Extras.Settings;
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                _logger.Fatal($"failed to fetch prop settings: {nameof(fetchedSelected)} was empty when expected to hold exactly one item");
                                throw new IndexOutOfRangeException(message: $"failed to fetch prop settings: {nameof(fetchedSelected)} was empty when expected to hold exactly one item", innerException: e);
                            }
                            #else
                            propSettings = fetchedSelected[0].prop.prop.Extras.Settings;
                            #endif

                            if (propSettings is IVariable variable)
                            {
                                var init = GLOBALS.Props[c][i];
                                var variations = ((IVariableInit)init).Variations + 1;
                                var variation = variable.Variation + 1;

                                RayGui.GuiSpinner(new Rectangle(indicatorOffset, listRect.y + listRect.height + 100, 290, 40), 
                                    "", 
                                    &variation, 
                                    1, 
                                    variations + 1, 
                                    false
                                );

                                variable.Variation = variation - 1;
                            }
                        }
                    }
                    
                    // Menu
                    // TODO: Add pagination
                    
                    for (var i = 0; i < pageSize; i++)
                    {
                        var listIndex = pageSize * _selectedListPage + i;
                        
                        if (listIndex >= GLOBALS.Level.Props.Length) continue;
                        
                        var current = GLOBALS.Level.Props[listIndex];
                        
                        DrawText(
                            current.prop.Name, 
                            listRect.x + 10, 
                            listRect.y + 10 + 24*i, 
                            20, 
                            BLACK
                        );

                        _selected[listIndex] = RayGui.GuiCheckBox(
                            new Rectangle(sWidth - 35, listRect.y+10+24*i, 20, 20), 
                            "", 
                            _selected[listIndex]);

                        _hidden[listIndex] = RayGui.GuiCheckBox(
                            new Rectangle(sWidth - 80, listRect.y + 10 + 24 * i, 20, 20),
                            "",
                            _hidden[listIndex]
                        );
                    }
                }
                    break;
            }
            
            // modes

            var selectModeTexture = GLOBALS.Textures.PropModes[0];
            var placeModeTexture = GLOBALS.Textures.PropModes[1];
            
            DrawRectangleV(
                new Vector2(menuPanelRect.x + 5 + _mode*40, sHeight - 45), 
                new Vector2(40, 40), BLUE
            );
                
            DrawTexturePro(
                selectModeTexture,
                new Rectangle(0, 0, selectModeTexture.width, selectModeTexture.height),
                new Rectangle(menuPanelRect.x + 5, sHeight - 45, 40, 40),
                new(0, 0),
                0,
                _mode == 0 ? WHITE : BLACK
            );
                
            DrawTexturePro(
                placeModeTexture,
                new Rectangle(0, 0, selectModeTexture.width, selectModeTexture.height),
                new Rectangle(menuPanelRect.x + 45, sHeight - 45, 40, 40),
                new(0, 0),
                0,
                _mode == 1 ? WHITE : BLACK
            );
            
            


            // layer indicator


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
        #endregion

        EndDrawing();
        #endregion
    }
}
