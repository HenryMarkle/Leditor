﻿using System.Numerics;
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

    private bool _clickTracker;

    private int _mode;

    private bool _movingProps;
    private bool _rotatingProps;
    private bool _stretchingProp;
    
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

    private readonly byte[] _menuPanelBytes = "Menu"u8.ToArray();

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
        }
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

        //                        v this was done to avoid rounding errors
        int tileMatrixY = tileMouseWorld.Y < 0 ? -1 : (int)tileMouseWorld.Y / previewScale;
        int tileMatrixX = tileMouseWorld.X < 0 ? -1 : (int)tileMouseWorld.X / previewScale;

        bool canDrawTile = !CheckCollisionPointRec(tileMouse, menuPanelRect);

        bool inMatrixBounds = tileMatrixX >= 0 && tileMatrixX < GLOBALS.Level.Width && tileMatrixY >= 0 && tileMatrixY < GLOBALS.Level.Height;

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
            if (IsKeyPressed(KeyboardKey.KEY_Z)) _showTileLayer1 = !_showTileLayer1;
            if (IsKeyPressed(KeyboardKey.KEY_X)) _showTileLayer2 = !_showTileLayer2;
            if (IsKeyPressed(KeyboardKey.KEY_C)) _showTileLayer3 = !_showTileLayer3;
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
                                        Extras = new()
                                        {
                                            Settings = new BasicPropSettings(),
                                            RopePoints = []
                                        }
                                    }
                                )
                            ];
                        }
                            break;
                        
                        case 1: // Ropes
                            break;

                        case 2: // Others
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
                                        Extras = new PropExtras
                                        {
                                            Settings = settings,
                                            RopePoints = []
                                        }
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
                        if (_menuRootCategoryIndex > 2) _menuRootCategoryIndex = 0;
                    }
                    else if (IsKeyPressed(KeyboardKey.KEY_A))
                    {
                        _menuRootCategoryIndex--;
                        if (_menuRootCategoryIndex < 0) _menuRootCategoryIndex = 2;
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
                    _movingProps = true;
                    _rotatingProps = false;
                    _stretchingProp = false;
                }
                // Rotate
                else if (IsKeyPressed(KeyboardKey.KEY_R) && anySelected)
                {
                    _movingProps = false;
                    _rotatingProps = true;
                    _stretchingProp = false;
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
                else if (IsKeyPressed(KeyboardKey.KEY_S) && fetchedSelected.Length == 1)
                {
                    _movingProps = false;
                    _rotatingProps = false;
                    _stretchingProp = !_stretchingProp;
                }
                // Delete
                else if (IsKeyPressed(KeyboardKey.KEY_D) && anySelected)
                {
                    GLOBALS.Level.Props = _selected
                        .Select((s, i) => (s, i))
                        .Where(v => !v.Item1)
                        .Select(v => GLOBALS.Level.Props[v.Item2])
                        .ToArray();
                }
                
                
                if (_movingProps)
                {
                    if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) _movingProps = false;
                    var delta = GetMouseDelta();

                    for (var s = 0; s < _selected.Length; s++)
                    {
                        if (!_selected[s]) continue;
                        
                        var quads = GLOBALS.Level.Props[s].prop.Quads;

                        quads.TopLeft = RayMath.Vector2Add(quads.TopLeft, delta);
                        quads.TopRight = RayMath.Vector2Add(quads.TopRight, delta);
                        quads.BottomRight = RayMath.Vector2Add(quads.BottomRight, delta);
                        quads.BottomLeft = RayMath.Vector2Add(quads.BottomLeft, delta);

                        GLOBALS.Level.Props[s].prop.Quads = quads;
                    }
                }
                else if (_rotatingProps)
                {
                    if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) _rotatingProps = false;

                    var delta = GetMouseDelta();
                    
                    // Collective Rotation
                    
                    for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                    {
                        if (!_selected[p]) continue;
                        
                        var quads = GLOBALS.Level.Props[p].prop.Quads;

                        GLOBALS.Level.Props[p].prop.Quads = Utils.RotatePropQuads(quads, delta.X, _selectedPropsCenter);
                    }
                }
                else if (_stretchingProp)
                {
                    var currentQuads = fetchedSelected[0].prop.prop.Quads;
                    
                    if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                    {
                        // Check Top-Left
                        if (CheckCollisionPointCircle(tileMouseWorld, currentQuads.TopLeft, 5f) || _quadLock == 1)
                        {
                            _quadLock = 1;
                            currentQuads.TopLeft = tileMouseWorld;
                        }
                        // Check Top-Right
                        else if (CheckCollisionPointCircle(tileMouseWorld, currentQuads.TopRight, 5f)  || _quadLock == 2)
                        {
                            _quadLock = 2;
                            currentQuads.TopRight = tileMouseWorld;
                        }
                        // Check Bottom-Right 
                        else if (CheckCollisionPointCircle(tileMouseWorld, currentQuads.BottomRight, 5f)  || _quadLock == 3)
                        {
                            _quadLock = 3;
                            currentQuads.BottomRight = tileMouseWorld;
                        }
                        // Check Bottom-Left
                        else if (CheckCollisionPointCircle(tileMouseWorld, currentQuads.BottomLeft, 5f)  || _quadLock == 4)
                        {
                            _quadLock = 4;
                            currentQuads.BottomLeft = tileMouseWorld;
                        }
                        else
                        {
                            _quadLock = 0;
                        }
                        
                        GLOBALS.Level.Props[fetchedSelected[0].index].prop.Quads = currentQuads;
                    }
                    else if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && _quadLock != 0) _quadLock = 0;
                }
                else
                {
                    if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT) && !_clickTracker && canDrawTile)
                    {
                        _selection1 = GetScreenToWorld2D(GetMousePosition(), _camera);
                        _clickTracker = true;
                    }

                    // TODO: Selection feature is incomplete
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
                
                break;
        }


        #region TileEditorDrawing
        BeginDrawing();

        ClearBackground(new(170, 170, 170, 255));

        BeginMode2D(_camera);
        {
            DrawRectangle(0, 0, GLOBALS.Level.Width * previewScale, GLOBALS.Level.Height * previewScale, new Color(215, 215, 215, 255));

            if (_showTileLayer3)
            {
                #region TileEditorLayer3

                // Draw geos first
                for (var y = 0; y < GLOBALS.Level.Height; y++)
                {
                    for (int x = 0; x < GLOBALS.Level.Width; x++)
                    {
                        const int z = 2;

                        var cell = GLOBALS.Level.GeoMatrix[y, x, z];

                        var texture = Utils.GetBlockIndex(cell.Geo);

                        if (texture >= 0)
                        {
                            if (z == GLOBALS.Layer)
                            {

                                DrawTexturePro(
                                    GLOBALS.Textures.GeoBlocks[texture],
                                    new(0, 0, 20, 20),
                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                    new(0, 0),
                                    0,
                                    new(0, 0, 0, 255)
                                );
                            }
                            else
                            {

                                Raylib.DrawTexturePro(
                                    GLOBALS.Textures.GeoBlocks[texture],
                                    new(0, 0, 20, 20),
                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                    new(0, 0),
                                    0,
                                    new(0, 0, 0, 120));
                            }
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

                                        if (z == GLOBALS.Layer)
                                        {

                                            DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(0, 0, 0, 255)
                                            );
                                        }
                                        else
                                        {
                                            DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(0, 0, 0, 170)
                                            );
                                        }
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
                                        case 21:*/    // scav

                                        if (z == GLOBALS.Layer)
                                        {

                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(255, 255, 255, 255)
                                            ); // TODO: remove opacity from entrances
                                        }
                                        else
                                        {
                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(255, 255, 255, 170)
                                            );
                                        }
                                        break;

                                    // directional placement
                                    /*case 4:     // entrance
                                        var index = Utils.GetStackableTextureIndex(s, CommonUtils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, z));

                                        if (index is 22 or 23 or 24 or 25)
                                        {
                                            GLOBALS.Level.GeoMatrix[y, x, 0].Geo = 7;
                                        }

                                        if (z == GLOBALS.Layer) {

                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[index], 
                                                new(0, 0, 20, 20),
                                                new(x*previewScale, y*previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0, 
                                                new(255, 255, 255, 255)
                                            );
                                        }
                                        else {
                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[index], 
                                                new(0, 0, 20, 20),
                                                new(x*previewScale, y*previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0, 
                                                new(255, 255, 255, 170)
                                            );
                                        }

                                        break;*/
                                    case 11:    // crack
                                        if (z == GLOBALS.Layer)
                                        {
                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, z))],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(255, 255, 255, 255)
                                            );
                                        }
                                        else
                                        {
                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, z))],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(255, 255, 255, 170)
                                            );
                                        }
                                        break;
                                }
                            }
                        }

                    }
                }

                // Then draw the tiles

                if (_showLayer3Tiles)
                {
                    for (int y = 0; y < GLOBALS.Level.Height; y++)
                    {
                        for (int x = 0; x < GLOBALS.Level.Width; x++)
                        {
                            const int z = 2;
                            var tileCell = GLOBALS.Level.TileMatrix[y, x, z];

                            if (tileCell.Type == TileType.TileHead)
                            {
                                var data = (TileHead)tileCell.Data;

                                var category = GLOBALS.Textures.Tiles[data.CategoryPostition.Item1 - 5];
                                var tileTexture = category[data.CategoryPostition.Item2 - 1];
                                var color = GLOBALS.TileCategories[data.CategoryPostition.Item1 - 5].Item2;
                                var initTile = GLOBALS.Tiles[data.CategoryPostition.Item1 - 5][data.CategoryPostition.Item2 - 1];


                                Printers.DrawTilePreview(ref initTile, ref tileTexture, ref color, (x, y));
                            }
                            else if (tileCell.Type == TileType.Material)
                            {
                                // var materialName = ((TileMaterial)tileCell.Data).Name;
                                var origin = new Vector2(x * previewScale + 5, y * previewScale + 5);
                                var color = GLOBALS.Level.MaterialColors[y, x, 0];

                                if (z != GLOBALS.Layer) color.a = 120;

                                if (color.r != 0 || color.g != 0 || color.b != 0)
                                {

                                    switch (GLOBALS.Level.GeoMatrix[y, x, 2].Geo)
                                    {
                                        case 1:
                                            DrawRectangle(
                                                x * previewScale + 5,
                                                y * previewScale + 5,
                                                6,
                                                6,
                                                color
                                            );
                                            break;


                                        case 2:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X, origin.Y + previewScale - 10),
                                                new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                color
                                            );
                                            break;


                                        case 3:
                                            DrawTriangle(
                                                new(origin.X + previewScale - 10, origin.Y),
                                                new(origin.X, origin.Y + previewScale - 10),
                                                new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                color
                                            );
                                            break;

                                        case 4:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X, origin.Y + previewScale - 10),
                                                new(origin.X + previewScale - 10, origin.Y),
                                                color
                                            );
                                            break;

                                        case 5:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                new(origin.X + previewScale - 10, origin.Y),
                                                color
                                            );
                                            break;

                                        case 6:
                                            DrawRectangleV(
                                                origin,
                                                new(previewScale - 10, (previewScale - 10) / 2),
                                                color
                                            );
                                            break;
                                    }
                                }
                            }
                        }
                    }
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
                    
                    Printers.DrawProp(category, index, current.prop);
                    
                    // if (current.prop.IsTile) 
                    //     Printers.DrawTileAsProp(
                    //         ref GLOBALS.Textures.Tiles[category][index],
                    //         ref GLOBALS.Tiles[category][index],
                    //         (tr, tl, bl, br)
                    //     );
                    // else 
                    //     Printers.DrawProp(
                    //         GLOBALS.Props[category][index], 
                    //         ref GLOBALS.Textures.Props[category][index],
                    //         ref origin,
                    //         [
                    //             new(tr.X - origin.X, tr.Y - origin.Y),
                    //             new(tl.X - origin.X, tl.Y - origin.Y),
                    //             new(bl.X - origin.X, bl.Y - origin.Y),
                    //             new(br.X - origin.X, br.Y - origin.Y),
                    //             new(tr.X - origin.X, tr.Y - origin.Y),
                    //         ]
                    //     );
                    
                    if (_selected[p])
                    {
                        DrawRectangleLinesEx(Utils.EncloseQuads(current.prop.Quads), 1f, BLUE);
                        
                        // Quad Points

                        if (_stretchingProp)
                        {
                            DrawCircleV(quads.TopLeft, 5f, BLUE);
                            DrawCircleV(quads.TopRight, 5f, BLUE);
                            DrawCircleV(quads.BottomRight, 5f, BLUE);
                            DrawCircleV(quads.BottomLeft, 5f, BLUE);
                        }
                    }
                }
                #endregion
            }

            if (_showTileLayer2)
            {
                #region TileEditorLayer2
                if (GLOBALS.Layer != 2) DrawRectangle(0, 0, GLOBALS.Level.Width * previewScale, GLOBALS.Level.Height * previewScale, new(90, 90, 90, 120));

                for (int y = 0; y < GLOBALS.Level.Height; y++)
                {
                    for (int x = 0; x < GLOBALS.Level.Width; x++)
                    {
                        const int z = 1;

                        var cell = GLOBALS.Level.GeoMatrix[y, x, z];

                        var texture = Utils.GetBlockIndex(cell.Geo);

                        if (texture >= 0)
                        {
                            if (z == GLOBALS.Layer)
                            {

                                Raylib.DrawTexturePro(
                                    GLOBALS.Textures.GeoBlocks[texture],
                                    new(0, 0, 20, 20),
                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                    new(0, 0),
                                    0,
                                    new(0, 0, 0, 255));
                            }
                            else
                            {

                                Raylib.DrawTexturePro(
                                    GLOBALS.Textures.GeoBlocks[texture],
                                    new(0, 0, 20, 20),
                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                    new(0, 0),
                                    0,
                                    new(0, 0, 0, 120));
                            }
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

                                        if (z == GLOBALS.Layer)
                                        {

                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(0, 0, 0, 255)
                                            );
                                        }
                                        else
                                        {
                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(0, 0, 0, 170)
                                            );
                                        }
                                        break;
                                    case 3:     // bathive
                                        /*case 5:*/     // shortcut path
                                        /*case 6:*/     // passage
                                        /*case 7:*/     // den
                                        /*case 9:*/     // rock
                                        /*case 10:*/    // spear
                                        /*case 12:*/    // forbidflychains
                                        /*case 13:*/    // garbagewormhole
                                        /*case 18:*/    // waterfall
                                        /*case 19:*/    // wac
                                        /*case 20:*/    // worm
                                        /*case 21:*/    // scav

                                        if (z == GLOBALS.Layer)
                                        {

                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(255, 255, 255, 255)
                                            ); // TODO: remove opacity from entrances
                                        }
                                        else
                                        {
                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(255, 255, 255, 170)
                                            );
                                        }
                                        break;

                                    // directional placement
                                    /*case 4:     // entrance
                                        var index = Utils.GetStackableTextureIndex(s, CommonUtils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, z));

                                        if (index is 22 or 23 or 24 or 25)
                                        {
                                            GLOBALS.Level.GeoMatrix[y, x, 0].Geo = 7;
                                        }

                                        if (z == GLOBALS.Layer) {

                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[index], 
                                                new(0, 0, 20, 20),
                                                new(x*previewScale, y*previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0, 
                                                new(255, 255, 255, 255)
                                            );
                                        }
                                        else {
                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[index], 
                                                new(0, 0, 20, 20),
                                                new(x*previewScale, y*previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0, 
                                                new(255, 255, 255, 170)
                                            );
                                        }

                                        break;*/
                                    case 11:    // crack
                                        if (z == GLOBALS.Layer)
                                        {
                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, z))],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(255, 255, 255, 255)
                                            );
                                        }
                                        else
                                        {
                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, z))],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(255, 255, 255, 170)
                                            );
                                        }
                                        break;
                                }
                            }
                        }

                    }
                }

                // Draw layer 2 tiles

                if (_showLayer2Tiles)
                {
                    for (int y = 0; y < GLOBALS.Level.Height; y++)
                    {
                        for (int x = 0; x < GLOBALS.Level.Width; x++)
                        {
                            const int z = 1;

                            var tileCell = GLOBALS.Level.TileMatrix[y, x, z];

                            if (tileCell.Type == TileType.TileHead)
                            {
                                var data = (TileHead)tileCell.Data;

                                var category = GLOBALS.Textures.Tiles[data.CategoryPostition.Item1 - 5];
                                var tileTexture = category[data.CategoryPostition.Item2 - 1];
                                var color = GLOBALS.TileCategories[data.CategoryPostition.Item1 - 5].Item2;
                                var initTile = GLOBALS.Tiles[data.CategoryPostition.Item1 - 5][data.CategoryPostition.Item2 - 1];


                                Printers.DrawTilePreview(ref initTile, ref tileTexture, ref color, (x, y));
                            }
                            else if (tileCell.Type == TileType.Material)
                            {
                                // var materialName = ((TileMaterial)tileCell.Data).Name;
                                var origin = new Vector2(x * previewScale + 5, y * previewScale + 5);
                                var color = GLOBALS.Level.MaterialColors[y, x, 0];

                                if (z != GLOBALS.Layer) color.a = 120;

                                if (color.r != 0 || color.g != 0 || color.b != 0)
                                {

                                    switch (GLOBALS.Level.GeoMatrix[y, x, 1].Geo)
                                    {
                                        case 1:
                                            DrawRectangle(
                                                x * previewScale + 5,
                                                y * previewScale + 5,
                                                6,
                                                6,
                                                color
                                            );
                                            break;


                                        case 2:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X, origin.Y + previewScale - 10),
                                                new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                color
                                            );
                                            break;


                                        case 3:
                                            DrawTriangle(
                                                new(origin.X + previewScale - 10, origin.Y),
                                                new(origin.X, origin.Y + previewScale - 10),
                                                new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                color
                                            );
                                            break;

                                        case 4:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X, origin.Y + previewScale - 10),
                                                new(origin.X + previewScale - 10, origin.Y),
                                                color
                                            );
                                            break;

                                        case 5:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                new(origin.X + previewScale - 10, origin.Y),
                                                color
                                            );
                                            break;

                                        case 6:
                                            DrawRectangleV(
                                                origin,
                                                new(previewScale - 10, (previewScale - 10) / 2),
                                                color
                                            );
                                            break;
                                    }
                                }
                            }
                        }
                    }
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
                    
                    Printers.DrawProp(category, index, current.prop);
                    
                    // if (current.prop.IsTile) 
                    //     Printers.DrawTileAsProp(
                    //         ref GLOBALS.Textures.Tiles[category][index],
                    //         ref GLOBALS.Tiles[category][index],
                    //         (tr, tl, bl, br)
                    //     );
                    // else 
                    //     Printers.DrawProp(
                    //         GLOBALS.Props[category][index], 
                    //         ref GLOBALS.Textures.Props[category][index],
                    //         ref origin,
                    //         [
                    //             new(tr.X - origin.X, tr.Y - origin.Y),
                    //             new(tl.X - origin.X, tl.Y - origin.Y),
                    //             new(bl.X - origin.X, bl.Y - origin.Y),
                    //             new(br.X - origin.X, br.Y - origin.Y),
                    //             new(tr.X - origin.X, tr.Y - origin.Y),
                    //         ]
                    //     );
                    
                    if (_selected[p])
                    {
                        DrawRectangleLinesEx(Utils.EncloseQuads(current.prop.Quads), 1f, BLUE);
                        
                        // Quad Points

                        if (_stretchingProp)
                        {
                            DrawCircleV(quads.TopLeft, 5f, BLUE);
                            DrawCircleV(quads.TopRight, 5f, BLUE);
                            DrawCircleV(quads.BottomRight, 5f, BLUE);
                            DrawCircleV(quads.BottomLeft, 5f, BLUE);
                        }
                    }
                }
                
                #endregion
            }

            if (_showTileLayer1)
            {
                #region TileEditorLayer1
                if (GLOBALS.Layer != 1 && GLOBALS.Layer != 2) DrawRectangle(0, 0, GLOBALS.Level.Width * previewScale, GLOBALS.Level.Height * previewScale, new(100, 100, 100, 100));

                for (int y = 0; y < GLOBALS.Level.Height; y++)
                {
                    for (int x = 0; x < GLOBALS.Level.Width; x++)
                    {
                        const int z = 0;

                        var cell = GLOBALS.Level.GeoMatrix[y, x, z];

                        var texture = Utils.GetBlockIndex(cell.Geo);

                        if (texture >= 0)
                        {
                            if (z == GLOBALS.Layer)
                            {

                                Raylib.DrawTexturePro(
                                    GLOBALS.Textures.GeoBlocks[texture],
                                    new(0, 0, 20, 20),
                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                    new(0, 0),
                                    0,
                                    new(0, 0, 0, 255));
                            }
                            else
                            {

                                Raylib.DrawTexturePro(
                                    GLOBALS.Textures.GeoBlocks[texture],
                                    new(0, 0, 20, 20),
                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                    new(0, 0),
                                    0,
                                    new(0, 0, 0, 120));
                            }
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

                                        if (z == GLOBALS.Layer)
                                        {

                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(0, 0, 0, 255)
                                            );
                                        }
                                        else
                                        {
                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(0, 0, 0, 170)
                                            );
                                        }
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

                                        if (z == GLOBALS.Layer)
                                        {

                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(255, 255, 255, 255)
                                            ); // TODO: remove opacity from entrances
                                        }
                                        else
                                        {
                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(255, 255, 255, 170)
                                            );
                                        }
                                        break;

                                    // directional placement
                                    case 4:     // entrance
                                        var index = Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, z));

                                        if (index is 22 or 23 or 24 or 25)
                                        {
                                            GLOBALS.Level.GeoMatrix[y, x, 0].Geo = 7;
                                        }

                                        if (z == GLOBALS.Layer)
                                        {

                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[index],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(255, 255, 255, 255)
                                            );
                                        }
                                        else
                                        {
                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[index],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(255, 255, 255, 170)
                                            );
                                        }

                                        break;
                                    case 11:    // crack
                                        if (z == GLOBALS.Layer)
                                        {
                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, z))],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(255, 255, 255, 255)
                                            );
                                        }
                                        else
                                        {
                                            Raylib.DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, z))],
                                                new(0, 0, 20, 20),
                                                new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0,
                                                new(255, 255, 255, 170)
                                            );
                                        }
                                        break;
                                }
                            }
                        }

                    }
                }

                // Draw layer 1 tiles

                if (_showLayer1Tiles)
                {
                    for (int y = 0; y < GLOBALS.Level.Height; y++)
                    {
                        for (int x = 0; x < GLOBALS.Level.Width; x++)
                        {
                            const int z = 0;

                            var tileCell = GLOBALS.Level.TileMatrix[y, x, z];

                            if (tileCell.Type == TileType.TileHead)
                            {
                                var data = (TileHead)tileCell.Data;

                                /*Console.WriteLine($"Index: {tile.Item1 - 5}; Length: {tilePreviewTextures.Length}; Name = {tile.Item3}");*/
                                var category = GLOBALS.Textures.Tiles[data.CategoryPostition.Item1 - 5];

                                var tileTexture = category[data.CategoryPostition.Item2 - 1];
                                var color = GLOBALS.TileCategories[data.CategoryPostition.Item1 - 5].Item2;
                                var initTile = GLOBALS.Tiles[data.CategoryPostition.Item1 - 5][data.CategoryPostition.Item2 - 1];

                                Printers.DrawTilePreview(ref initTile, ref tileTexture, ref color, (x, y));
                            }
                            else if (tileCell.Type == TileType.Material)
                            {
                                // var materialName = ((TileMaterial)tileCell.Data).Name;
                                var origin = new Vector2(x * previewScale + 5, y * previewScale + 5);
                                var color = GLOBALS.Level.MaterialColors[y, x, 0];

                                if (z != GLOBALS.Layer) color.a = 120;

                                if (color.r != 0 || color.g != 0 || color.b != 0)
                                {

                                    switch (GLOBALS.Level.GeoMatrix[y, x, 0].Geo)
                                    {
                                        case 1:
                                            DrawRectangle(
                                                x * previewScale + 5,
                                                y * previewScale + 5,
                                                6,
                                                6,
                                                color
                                            );
                                            break;


                                        case 2:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X, origin.Y + previewScale - 10),
                                                new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                color
                                            );
                                            break;


                                        case 3:
                                            DrawTriangle(
                                                new(origin.X + previewScale - 10, origin.Y),
                                                new(origin.X, origin.Y + previewScale - 10),
                                                new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                color
                                            );
                                            break;

                                        case 4:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X, origin.Y + previewScale - 10),
                                                new(origin.X + previewScale - 10, origin.Y),
                                                color
                                            );
                                            break;

                                        case 5:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                new(origin.X + previewScale - 10, origin.Y),
                                                color
                                            );
                                            break;

                                        case 6:
                                            DrawRectangleV(
                                                origin,
                                                new(previewScale - 10, (previewScale - 10) / 2),
                                                color
                                            );
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                
                // then draw the props

                for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                {
                    if (_hidden[p]) continue;
                    
                    var current = GLOBALS.Level.Props[p];
                    
                    // Filter based on depth
                    if (current.prop.Depth < -9 || current.type == InitPropType.Rope) continue;

                    var (category, index) = current.position;
                    var quads = current.prop.Quads;
                    
                    // origin must be the center
                    // var origin = new Vector2(tl.X + (tr.X - tl.X)/2f, tl.Y + (bl.Y - tl.Y)/2f);
                    
                    Printers.DrawProp(category, index, current.prop);
                    
                    // if (current.prop.IsTile) 
                    //     Printers.DrawTileAsProp(
                    //         ref GLOBALS.Textures.Tiles[category][index],
                    //         ref GLOBALS.Tiles[category][index],
                    //         (tr, tl, bl, br)
                    //     );
                    // else 
                    //     Printers.DrawProp(
                    //         GLOBALS.Props[category][index], 
                    //         ref GLOBALS.Textures.Props[category][index],
                    //         (tr, tl, bl, br)
                    //     );

                    if (_selected[p])
                    {
                        // Side Lines
                        
                        DrawRectangleLinesEx(Utils.EncloseQuads(current.prop.Quads), 1f, BLUE);
                        
                        // Quad Points

                        if (_stretchingProp)
                        {
                            DrawCircleV(quads.TopLeft, 5f, BLUE);
                            DrawCircleV(quads.TopRight, 5f, BLUE);
                            DrawCircleV(quads.BottomRight, 5f, BLUE);
                            DrawCircleV(quads.BottomLeft, 5f, BLUE);
                        }
                        
                    }
                }
                
                #endregion
            }
            
            // Draw the enclosing rectangle for selected props
            // DEBUG: DrawRectangleLinesEx(_selectedPropsEncloser, 3f, WHITE);

            switch (_mode)
            {
                case 1: // Place Mode
                    switch (_menuRootCategoryIndex)
            {
                case 0: // Current Tile-As-Prop
                    {
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

                case 2: // Current Prop
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
                    DrawTexture(GLOBALS.Textures.PropMenuCategories[0], (int)menuPanelRect.x + 30, 40, BLACK);
            DrawTexture(GLOBALS.Textures.PropMenuCategories[1], (int)menuPanelRect.x + 30 + 100, 40, BLACK);
            DrawTexture(GLOBALS.Textures.PropMenuCategories[2], (int)menuPanelRect.x + 30 + 200, 40, BLACK);

            DrawRectangleLines((int)(menuPanelRect.x + 28), 38, 296, 44, GRAY);

            DrawRectangleLinesEx(new(menuPanelRect.x + 28 + (_menuRootCategoryIndex * 104), 38, 100, 44), 4f, BLUE);

            
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

                case 2: // Props
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
                    
                    var listRect = new Rectangle(
                        (int)(menuPanelRect.x + 5), 
                        90, 
                        menuPanelRect.width - 10, 
                        (int)(menuPanelRect.height - 400)
                    );
                    
                    DrawRectangleLinesEx(listRect, 1f, GRAY);

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
                            .Where(v => !v.Item1)
                            .Select(v => GLOBALS.Level.Props[v.Item2])
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

                        if (fetchedSelected[0].prop.type == InitPropType.Tile)
                        {
                            var init = GLOBALS.Tiles[c][i];
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
                        else
                        {
                            var init = GLOBALS.Props[c][i];
                            
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
                            var depth = -fetchedSelected[0].prop.prop.Depth;

                            if (RayGui.GuiSpinner(
                                new Rectangle(indicatorOffset, listRect.y + listRect.height + 50, 290, 40),
                                "",
                                &depth,
                                0,
                                29,
                                _spinnerLock == 1
                            )) _spinnerLock = 1;
                            
                            fetchedSelected[0].prop.prop.Depth = -depth;
                            GLOBALS.Level.Props[fetchedSelected[0].index].prop.Depth = -depth;
                            
                            // Variation Selector

                            if (fetchedSelected[0].prop.prop.Extras.Settings is IVariable variable)
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
                    
                    //
                    
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
