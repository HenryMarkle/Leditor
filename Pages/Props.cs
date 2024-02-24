using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using rlImGui_cs;
using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class PropsEditorPage : IPage
{
    private readonly Serilog.Core.Logger _logger;

    private Camera2D _camera;

    private readonly PropsShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.PropsEditor;
    
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

    private int _propsMenuRopesScrollIndex;
    private int _propsMenuRopesIndex;

    private int _propsMenuLongsScrollIndex;
    private int _propsMenuLongsIndex;

    private int _propsMenuOthersCategoryScrollIndex;
    private int _propsMenuOthersScrollIndex;
    private int _propsMenuOthersCategoryIndex;
    private int _propsMenuOthersIndex;

    private int _propsMenuTilesItemFocus;
    private int _propsMenuTilesCategoryItemFocus;

    private int _propsMenuRopesItemFocus;
    private int _propsMenuLongItemFocus;
    
    private int _propsMenuOtherItemFocus;
    private int _propsMenuOtherCategoryItemFocus;
    
    private int _spinnerLock;
    
    private const float PropScale = 0.4f;

    private int _snapMode = 0;

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

    private bool _noCollisionPropPlacement;

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

    private readonly (int index, string category)[] _tilesAsPropsCategoryIndices = [];
    private readonly (int index, InitTile init)[][] _tilesAsPropsIndices = [];

    private readonly string[] _tilesAsPropsCategoryNames;
    private readonly string[][] _tilesAsPropsNames;
    private readonly string[] _ropeNames = [..GLOBALS.RopeProps.Select(p => p.Name)];
    private readonly string[] _longNames = [..GLOBALS.LongProps.Select(l => l.Name)];
    private readonly string[] _otherCategoryNames;
    private readonly string[][] _otherNames;

    private BasicPropSettings _copiedPropSettings = new();
    private Vector2[] _copiedRopePoints = [];
    private int _copiedDepth;
    private bool _copiedIsTileAsProp;
    private bool _newlyCopied; // to signify that the copied properties should be used

    private int _defaultDepth;

    private void UpdateDefaultDepth()
    {
        _defaultDepth = -GLOBALS.Layer * 10;
    }
    
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
    
    private bool _isShortcutsWinHovered;
    private bool _isShortcutsWinDragged;

    internal PropsEditorPage(Serilog.Core.Logger logger, Camera2D? camera = null)
    {
        _logger = logger;
        _camera = camera ?? new() { zoom = 0.8f };

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

        _tilesAsPropsCategoryNames = [..from c in _tilesAsPropsCategoryIndices select c.category];
        _tilesAsPropsNames = _tilesAsPropsIndices.Select(c => c.Select(t => t.init.Name).ToArray()).ToArray();
        _otherCategoryNames = [..from c in _propCategoriesOnly select c.name];
        _otherNames = GLOBALS.Props.Select(c => c.Select(p => p.Name).ToArray()).ToArray();
    }

    #nullable enable
    internal void OnProjectLoaded(object? sender, EventArgs e)
    {
        ImportRopeModels();
        _selected = new bool[GLOBALS.Level.Props.Length];
    }

    internal void OnProjectCreated(object? sender, EventArgs e)
    {
        ImportRopeModels();
        _selected = new bool[GLOBALS.Level.Props.Length];
    }
    #nullable disable

    private void ImportRopeModels()
    {
        List<(int, bool, RopeModel, Vector2[])> models = [];
        
        for (var r = 0; r < GLOBALS.Level.Props.Length; r++)
        {
            var current = GLOBALS.Level.Props[r];
            
            if (current.type != InitPropType.Rope) continue;
            
            var newModel = new RopeModel(current.prop, GLOBALS.RopeProps[current.position.index], current.prop.Extras.RopePoints.Length);
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
                    
            case 1: // Ropes
                _propsMenuRopesIndex--;
                if (_propsMenuRopesIndex < 0) _propsMenuRopesIndex = GLOBALS.RopeProps.Length - 1;
                break;
            
            case 2: // Longs 
                _propsMenuLongsIndex--;
                if (_propsMenuLongsIndex < 0) _propsMenuLongsIndex = GLOBALS.LongProps.Length - 1;
                break;
                    
            case 3: // props
                if (_propCategoryFocus)
                {
                    _propsMenuOthersCategoryIndex--;
                    if (_propsMenuOthersCategoryIndex < 0) _propsMenuOthersCategoryIndex = GLOBALS.Props.Length - 1;
                    _propsMenuOthersIndex = 0;
                }
                else
                {
                    _propsMenuOthersIndex--;
                    if (_propsMenuOthersIndex < 0) _propsMenuOthersIndex = GLOBALS.Props[_propsMenuOthersCategoryIndex].Length - 1;
                }
                break;
        }
    }

    private void IncrementMenuIndex()
    {
        switch (_menuRootCategoryIndex)
        {
            case 0: // Tiles as props
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
            
            case 1: // Ropes
                _propsMenuRopesIndex = ++_propsMenuRopesIndex % GLOBALS.RopeProps.Length;
                break;
            
            case 2: // Longs 
                _propsMenuLongsIndex = ++_propsMenuLongsIndex % GLOBALS.LongProps.Length;
                break;
            
            case 3: // Props
                if (_propCategoryFocus)
                {
                    _propsMenuOthersCategoryIndex++;
                    if (_propsMenuOthersCategoryIndex > GLOBALS.Props.Length - 1) _propsMenuOthersCategoryIndex = 0;
                    _propsMenuOthersIndex = 0;
                }
                else
                {
                    _propsMenuOthersIndex++;
                    if (_propsMenuOthersIndex > GLOBALS.Props[_propsMenuOthersCategoryIndex].Length - 1) _propsMenuOthersIndex = 0;
                }
                break;
        }
    }

    public void Draw()
    {
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera;
        
        var ctrl = IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);
        var shift = IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
        var alt = IsKeyDown(KeyboardKey.KEY_LEFT_ALT);
            
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
        
        var layer3Rect = new Rectangle(10, sHeight - 50, 40, 40);
        var layer2Rect = new Rectangle(20, sHeight - 60, 40, 40);
        var layer1Rect = new Rectangle(30, sHeight - 70, 40, 40);

        var tileMouse = GetMousePosition();
        var tileMouseWorld = GetScreenToWorld2D(tileMouse, _camera);

        Rectangle menuPanelRect = new(sWidth - 360, 0, 360, sHeight);
        Rectangle ropePanelRect = new(0, 0, 250, 280);


        //                        v this was done to avoid rounding errors
        var tileMatrixY = tileMouseWorld.Y < 0 ? -1 : (int)tileMouseWorld.Y / previewScale;
        var tileMatrixX = tileMouseWorld.X < 0 ? -1 : (int)tileMouseWorld.X / previewScale;

        var canDrawTile = !CheckCollisionPointRec(tileMouse, menuPanelRect) &&
                          !CheckCollisionPointRec(tileMouse, layer3Rect) &&
                          (GLOBALS.Layer != 1 || !CheckCollisionPointRec(tileMouse, layer2Rect)) &&
                          (GLOBALS.Layer != 0 || !CheckCollisionPointRec(tileMouse, layer1Rect)) &&
                           (!CheckCollisionPointRec(tileMouse, ropePanelRect) || !_showRopePanel);

        var inMatrixBounds = tileMatrixX >= 0 && tileMatrixX < GLOBALS.Level.Width && tileMatrixY >= 0 && tileMatrixY < GLOBALS.Level.Height;

        var currentTileAsPropCategory = _tilesAsPropsCategoryIndices[_propsMenuTilesCategoryIndex];

        if (_spinnerLock == 0)
        {
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToMainPage.Check(ctrl, shift, alt)) GLOBALS.Page = 1;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToGeometryEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 2;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToTileEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 3;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToCameraEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 4;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToLightEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 5;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToDimensionsEditor.Check(ctrl, shift, alt))
            {
                GLOBALS.ResizeFlag = true;
                GLOBALS.NewFlag = false;
                GLOBALS.Page = 6;
                _logger.Debug("go from GLOBALS.Page 8 to GLOBALS.Page 6");
            }
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToEffectsEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 7;
            // if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToPropsEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 8;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToSettingsPage.Check(ctrl, shift, alt)) GLOBALS.Page = 9;
        }
        else
        {
            if (_shortcuts.EscapeSpinnerControl.Check(ctrl, shift, alt)) _spinnerLock = 0;
        }

        // handle mouse drag
        if (_shortcuts.DragLevel.Check(ctrl, shift, alt, true) || _shortcuts.DragLevelAlt.Check(ctrl, shift, alt, true))
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
        if (_shortcuts.CycleLayers.Check(ctrl, shift, alt))
        {
            GLOBALS.Layer++;

            if (GLOBALS.Layer > 2) GLOBALS.Layer = 0;

            UpdateDefaultDepth();
        }

        if (_shortcuts.ToggleLayer1Tiles.Check(ctrl, shift, alt)) _showLayer1Tiles = !_showLayer1Tiles;
        if (_shortcuts.ToggleLayer2Tiles.Check(ctrl, shift, alt)) _showLayer2Tiles = !_showLayer2Tiles;
        if (_shortcuts.ToggleLayer3Tiles.Check(ctrl, shift, alt)) _showLayer3Tiles = !_showLayer3Tiles;
        
        // Cycle Mode
        if (_shortcuts.CycleModeRight.Check(ctrl, shift, alt))
        {
            _mode = ++_mode % 2;
        }
        else if (_shortcuts.CycleModeLeft.Check(ctrl, shift, alt))
        {
            _mode--;
            if (_mode < 0) _mode = 1;
        }
        
        if (_shortcuts.ToggleLayer1.Check(ctrl, shift, alt) && !_scalingProps) _showTileLayer1 = !_showTileLayer1;
        if (_shortcuts.ToggleLayer2.Check(ctrl, shift, alt) && !_scalingProps) _showTileLayer2 = !_showTileLayer2;
        if (_shortcuts.ToggleLayer3.Check(ctrl, shift, alt) && !_scalingProps) _showTileLayer3 = !_showTileLayer3;

        if (_shortcuts.CycleSnapMode.Check(ctrl, shift, alt)) _snapMode = ++_snapMode % 3;

        // Mode-based hotkeys
        switch (_mode)
        {
            case 1: // Place Mode

                // Place Prop
                if (_noCollisionPropPlacement)
                {
                    if (canDrawTile && (_shortcuts.PlaceProp.Check(ctrl, shift, alt, true) ||
                                        _shortcuts.PlacePropAlt.Check(ctrl, shift, alt, true)))
                    {
                        var posV = _snapMode switch
                        {
                            1 => new Vector2(tileMatrixX, tileMatrixY) * GLOBALS.PreviewScale,
                            2 => new Vector2((int)(tileMouseWorld.X / 8f), (int)(tileMouseWorld.Y / 8f)) * 8f,
                            _ => tileMouseWorld
                        };

                        switch (_menuRootCategoryIndex)
                        {
                            case 0: // Tiles as props
                            {
                                var currentTileAsProp = _tilesAsPropsIndices[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
                                var width = (float)(currentTileAsProp.init.Size.Item1 + currentTileAsProp.init.BufferTiles*2) * GLOBALS.PreviewScale / 2;
                                var height = (float)(currentTileAsProp.init.Size.Item2 + currentTileAsProp.init.BufferTiles*2) * GLOBALS.PreviewScale / 2;
                                
                                BasicPropSettings settings;

                                if (_newlyCopied)
                                {
                                    _newlyCopied = false;

                                    settings = _copiedPropSettings;
                                    _defaultDepth = _copiedDepth;
                                }
                                else
                                {
                                    settings = new();
                                }

                                var placementQuad = new PropQuads(
                                    new Vector2(posV.X - width, posV.Y - height),
                                    new Vector2(posV.X + width, posV.Y - height),
                                    new Vector2(posV.X + width, posV.Y + height),
                                    new Vector2(posV.X - width, posV.Y + height)
                                );

                                foreach (var prop in GLOBALS.Level.Props)
                                {
                                    var propRec = Utils.EncloseQuads(prop.prop.Quads);
                                    var newPropRec = Utils.EncloseQuads(placementQuad);
                                    
                                    if (prop.prop.Depth == _defaultDepth && CheckCollisionRecs(newPropRec, propRec)) goto skipPlacement;
                                }
                                
                                GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
                                    (
                                        InitPropType.Tile, 
                                        (currentTileAsPropCategory.index, currentTileAsProp.index),
                                        new Prop(
                                            _defaultDepth, 
                                            currentTileAsProp.init.Name, 
                                            true, 
                                            placementQuad
                                        )
                                        {
                                            Extras = new(settings, [])
                                        }
                                    )
                                ];
                            }
                                break;

                            case 1: // Ropes
                            {
                                var current = GLOBALS.RopeProps[_propsMenuRopesIndex];
                                var newQuads = new PropQuads
                                {
                                    TopLeft = new(posV.X - 100, posV.Y - 30),
                                    BottomLeft = new(posV.X - 100, posV.Y + 30),
                                    TopRight = new(posV.X + 100, tileMouseWorld.Y - 30),
                                    BottomRight = new(posV.X + 100, posV.Y + 30)
                                };

                                var ropeEnds = Utils.RopeEnds(newQuads);

                                
                                PropRopeSettings settings;
                                Vector2[] ropePoints;

                                if (_newlyCopied)
                                {
                                    _newlyCopied = false;

                                    ropePoints = _copiedRopePoints;
                                    settings = (PropRopeSettings)_copiedPropSettings;
                                    _defaultDepth = _copiedDepth;
                                }
                                else
                                {
                                    ropePoints = Utils.GenerateRopePoints(ropeEnds.pA, ropeEnds.pB, 30);
                                    settings = new();
                                }

                                    
                                GLOBALS.Level.Props = [..GLOBALS.Level.Props, 
                                    (
                                        InitPropType.Rope, 
                                        (-1, _propsMenuRopesIndex), 
                                        new Prop(
                                            _defaultDepth, 
                                            current.Name, 
                                            false, 
                                            newQuads
                                        )
                                        {
                                            Extras = new PropExtras(
                                                settings, 
                                                ropePoints
                                            )
                                        }
                                    ) 
                                ];

                                _selected = new bool[GLOBALS.Level.Props.Length];
                                ImportRopeModels();
                            }
                                break;

                            case 2: // Long Props
                            {
                                var current = GLOBALS.LongProps[_propsMenuLongsIndex];
                                ref var texture = ref GLOBALS.Textures.LongProps[_propsMenuLongsIndex];
                                var height = texture.height / 2f;
                                var newQuads = new PropQuads
                                {
                                    TopLeft = new(posV.X - 100, posV.Y - height),
                                    BottomLeft = new(posV.X - 100, posV.Y + height),
                                    TopRight = new(posV.X + 100, posV.Y - height),
                                    BottomRight = new(posV.X + 100, posV.Y + height)
                                };
                                
                                PropLongSettings settings;

                                if (_newlyCopied)
                                {
                                    _newlyCopied = false;

                                    settings = (PropLongSettings)_copiedPropSettings;
                                    _defaultDepth = _copiedDepth;
                                }
                                else
                                {
                                    settings = new();
                                }
                                
                                GLOBALS.Level.Props = [..GLOBALS.Level.Props, 
                                    (
                                        InitPropType.Long, 
                                        (-1, _propsMenuLongsIndex), 
                                        new Prop(
                                            _defaultDepth, 
                                            current.Name, 
                                            false, 
                                            newQuads
                                        )
                                        {
                                            Extras = new PropExtras(settings, [])
                                        }
                                    ) 
                                ];

                                _selected = new bool[GLOBALS.Level.Props.Length];
                            }
                                break;

                            case 3: // Others
                            {
                                var init = GLOBALS.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
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

                                if (_newlyCopied)
                                {
                                    _newlyCopied = false;

                                    settings = (PropLongSettings)_copiedPropSettings;
                                    _defaultDepth = _copiedDepth;
                                }
                                
                                GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
                                    (
                                        init.Type, 
                                        (_propsMenuOthersCategoryIndex, _propsMenuOthersIndex),
                                        new Prop(
                                            _defaultDepth, 
                                            init.Name, 
                                            false, 
                                            new PropQuads(
                                            new(posV.X - width, posV.Y - height), 
                                            new(posV.X + width, posV.Y - height), 
                                            new(posV.X + width, posV.Y + height), 
                                            new(posV.X - width, posV.Y + height))
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
                        
                        skipPlacement:
                        {
                        }
                    }
                }
                else
                {
                    if (canDrawTile && (_shortcuts.PlaceProp.Check(ctrl, shift, alt) || _shortcuts.PlacePropAlt.Check(ctrl, shift, alt)))
                    {
                        var posV = _snapMode switch
                        {
                            1 => new Vector2(tileMatrixX, tileMatrixY) * GLOBALS.PreviewScale,
                            2 => new Vector2((int)(tileMouseWorld.X / 8f), (int)(tileMouseWorld.Y / 8f)) * 8f,
                            _ => tileMouseWorld
                        };

                        switch (_menuRootCategoryIndex)
                        {
                            case 0: // Tiles as props
                            {
                                var currentTileAsProp = _tilesAsPropsIndices[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
                                var width = (float)(currentTileAsProp.init.Size.Item1 + currentTileAsProp.init.BufferTiles*2) * GLOBALS.PreviewScale / 2;
                                var height = (float)(currentTileAsProp.init.Size.Item2 + currentTileAsProp.init.BufferTiles*2) * GLOBALS.PreviewScale / 2;
                                
                                BasicPropSettings settings;

                                if (_newlyCopied)
                                {
                                    _newlyCopied = false;

                                    settings = _copiedPropSettings;
                                    _defaultDepth = _copiedDepth;
                                }
                                else
                                {
                                    settings = new();
                                }
                                
                                GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
                                    (
                                        InitPropType.Tile, 
                                        (currentTileAsPropCategory.index, currentTileAsProp.index),
                                        new Prop(
                                            _defaultDepth, 
                                            currentTileAsProp.init.Name, 
                                            true, 
                                            new PropQuads(
                                            new Vector2(posV.X - width, posV.Y - height), 
                                            new  Vector2(posV.X + width, posV.Y - height), 
                                            new Vector2(posV.X + width, posV.Y + height), 
                                            new Vector2(posV.X - width, posV.Y + height)
                                            )
                                        )
                                        {
                                            Extras = new(settings, [])
                                        }
                                    )
                                ];
                            }
                                break;

                            case 1: // Ropes
                            {
                                var current = GLOBALS.RopeProps[_propsMenuRopesIndex];
                                var newQuads = new PropQuads
                                {
                                    TopLeft = new(posV.X - 100, posV.Y - 30),
                                    BottomLeft = new(posV.X - 100, posV.Y + 30),
                                    TopRight = new(posV.X + 100, tileMouseWorld.Y - 30),
                                    BottomRight = new(posV.X + 100, posV.Y + 30)
                                };

                                var ropeEnds = Utils.RopeEnds(newQuads);

                                
                                PropRopeSettings settings;
                                Vector2[] ropePoints;

                                if (_newlyCopied)
                                {
                                    _newlyCopied = false;

                                    ropePoints = _copiedRopePoints;
                                    settings = (PropRopeSettings)_copiedPropSettings;
                                    _defaultDepth = _copiedDepth;
                                }
                                else
                                {
                                    ropePoints = Utils.GenerateRopePoints(ropeEnds.pA, ropeEnds.pB, 30);
                                    settings = new();
                                }

                                    
                                GLOBALS.Level.Props = [..GLOBALS.Level.Props, 
                                    (
                                        InitPropType.Rope, 
                                        (-1, _propsMenuRopesIndex), 
                                        new Prop(
                                            _defaultDepth, 
                                            current.Name, 
                                            false, 
                                            newQuads
                                        )
                                        {
                                            Extras = new PropExtras(
                                                settings, 
                                                ropePoints
                                            )
                                        }
                                    ) 
                                ];

                                _selected = new bool[GLOBALS.Level.Props.Length];
                                ImportRopeModels();
                            }
                                break;

                            case 2: // Long Props
                            {
                                var current = GLOBALS.LongProps[_propsMenuLongsIndex];
                                ref var texture = ref GLOBALS.Textures.LongProps[_propsMenuLongsIndex];
                                var height = texture.height / 2f;
                                var newQuads = new PropQuads
                                {
                                    TopLeft = new(posV.X - 100, posV.Y - height),
                                    BottomLeft = new(posV.X - 100, posV.Y + height),
                                    TopRight = new(posV.X + 100, posV.Y - height),
                                    BottomRight = new(posV.X + 100, posV.Y + height)
                                };
                                
                                PropLongSettings settings;

                                if (_newlyCopied)
                                {
                                    _newlyCopied = false;

                                    settings = (PropLongSettings)_copiedPropSettings;
                                    _defaultDepth = _copiedDepth;
                                }
                                else
                                {
                                    settings = new();
                                }
                                
                                GLOBALS.Level.Props = [..GLOBALS.Level.Props, 
                                    (
                                        InitPropType.Long, 
                                        (-1, _propsMenuLongsIndex), 
                                        new Prop(
                                            _defaultDepth, 
                                            current.Name, 
                                            false, 
                                            newQuads
                                        )
                                        {
                                            Extras = new PropExtras(settings, [])
                                        }
                                    ) 
                                ];

                                _selected = new bool[GLOBALS.Level.Props.Length];
                            }
                                break;

                            case 3: // Others
                            {
                                var init = GLOBALS.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
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

                                if (_newlyCopied)
                                {
                                    _newlyCopied = false;

                                    settings = (PropLongSettings)_copiedPropSettings;
                                    _defaultDepth = _copiedDepth;
                                }
                                
                                GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
                                    (
                                        init.Type, 
                                        (_propsMenuOthersCategoryIndex, _propsMenuOthersIndex),
                                        new Prop(
                                            _defaultDepth, 
                                            init.Name, 
                                            false, 
                                            new PropQuads(
                                            new(posV.X - width, posV.Y - height), 
                                            new(posV.X + width, posV.Y - height), 
                                            new(posV.X + width, posV.Y + height), 
                                            new(posV.X - width, posV.Y + height))
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
                }
                
                // Cycle categories
                if (_shortcuts.CycleCategoriesRight.Check(ctrl, shift, alt))
                {
                    _menuRootCategoryIndex++;
                    if (_menuRootCategoryIndex > 3) _menuRootCategoryIndex = 0;
                }
                else if (_shortcuts.CycleCategoriesLeft.Check(ctrl, shift, alt))
                {
                    _menuRootCategoryIndex--;
                    if (_menuRootCategoryIndex < 0) _menuRootCategoryIndex = 3;
                }
                
                // Navigate menu
                if (_shortcuts.InnerCategoryFocusRight.Check(ctrl, shift, alt))
                {
                    _propCategoryFocus = false;
                }
                else if (_shortcuts.InnerCategoryFocusLeft.Check(ctrl, shift, alt))
                {
                    _propCategoryFocus = true;
                }
                
                if (_shortcuts.NavigateMenuDown.Check(ctrl, shift, alt))
                {
                    IncrementMenuIndex();
                    currentTileAsPropCategory = _tilesAsPropsCategoryIndices[_propsMenuTilesCategoryIndex];
                }
                else if (_shortcuts.NavigateMenuUp.Check(ctrl, shift, alt))
                {
                    DecrementMenuIndex();
                    currentTileAsPropCategory = _tilesAsPropsCategoryIndices[_propsMenuTilesCategoryIndex];
                }
                
                // Pickup Prop
                if (_shortcuts.PickupProp.Check(ctrl, shift, alt))
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

                                    _copiedPropSettings = current.prop.Extras.Settings;
                                    _copiedIsTileAsProp = true;
                                }
                            }
                        }
                        else if (current.type == InitPropType.Rope)
                        {
                            _copiedRopePoints = [..current.prop.Extras.RopePoints];
                            _copiedPropSettings = current.prop.Extras.Settings;
                            _copiedIsTileAsProp = false;
                        }
                        else if (current.type == InitPropType.Long)
                        {
                            _copiedPropSettings = current.prop.Extras.Settings;
                            _copiedIsTileAsProp = false;
                        }
                        else
                        {
                            (_propsMenuOthersCategoryIndex, _propsMenuOthersIndex) = current.position;
                            _copiedPropSettings = current.prop.Extras.Settings;
                            _copiedIsTileAsProp = false;
                        }

                        _copiedDepth = current.prop.Depth;
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
                if (_shortcuts.ToggleMovingPropsMode.Check(ctrl, shift, alt) && anySelected)
                {
                    _scalingProps = false;
                    _movingProps = !_movingProps;
                    _rotatingProps = false;
                    _stretchingProp = false;
                    _editingPropPoints = false;
                    // _ropeMode = false;
                }
                // Rotate
                else if (_shortcuts.ToggleRotatingPropsMode.Check(ctrl, shift, alt) && anySelected)
                {
                    _scalingProps = false;
                    _movingProps = false;
                    _rotatingProps = !_rotatingProps;
                    _stretchingProp = false;
                    _editingPropPoints = false;
                    // _ropeMode = false;
                }
                // Scale
                else if (_shortcuts.ToggleScalingPropsMode.Check(ctrl, shift, alt) && anySelected)
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
                else if (_shortcuts.TogglePropsVisibility.Check(ctrl, shift, alt) && anySelected)
                {
                    for (var i = 0; i < GLOBALS.Level.Props.Length; i++)
                    {
                        if (_selected[i]) _hidden[i] = !_hidden[i];
                    }
                }
                // Edit Quads
                else if (_shortcuts.ToggleEditingPropQuadsMode.Check(ctrl, shift, alt) && fetchedSelected.Length == 1)
                {
                    _scalingProps = false;
                    _movingProps = false;
                    _rotatingProps = false;
                    _stretchingProp = !_stretchingProp;
                    _editingPropPoints = false;
                    // _ropeMode = false;
                }
                // Delete
                else if (_shortcuts.DeleteSelectedProps.Check(ctrl, shift, alt) && anySelected)
                {
                    _scalingProps = false;
                    _movingProps = false;
                    _rotatingProps = false;
                    _stretchingProp = false;
                    _editingPropPoints = false;
                    _ropeMode = false;
                    
                    GLOBALS.Level.Props = _selected
                        .Select((s, i) => (s, i))
                        .Where(v => !v.Item1)
                        .Select(v => GLOBALS.Level.Props[v.Item2])
                        .ToArray();
                    
                    _selected = new bool[GLOBALS.Level.Props.Length]; // Update selected
                    
                    fetchedSelected = GLOBALS.Level.Props
                        .Select((prop, index) => (prop, index))
                        .Where(p => _selected[p.index])
                        .Select(p => p)
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
                    if (_shortcuts.ToggleRopePointsEditingMode.Check(ctrl, shift, alt))
                    {
                        _scalingProps = false;
                        _movingProps = false;
                        _rotatingProps = false;
                        _stretchingProp = false;
                        _editingPropPoints = !_editingPropPoints;
                        _ropeMode = false;
                    }
                    // Rope mode
                    else if (_shortcuts.ToggleRopeEditingMode.Check(ctrl, shift, alt))
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
                    if (IsMouseButtonDown(_shortcuts.DragLevel.Button))
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
                    
                    var posV = _snapMode switch
                    {
                        1 => new Vector2(tileMatrixX, tileMatrixY) * GLOBALS.PreviewScale,
                        2 => new Vector2((int)(tileMouseWorld.X / 8f), (int)(tileMouseWorld.Y / 8f)) * 8f,
                        _ => tileMouseWorld
                    };

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
                                    posV, middleLeft,
                                    5f
                                ) || _quadLock == 1)
                            {
                                _quadLock = 1;
                                currentQuads.BottomLeft = RayMath.Vector2Add(
                                    posV, 
                                    new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                    );
                                
                                currentQuads.TopLeft = RayMath.Vector2Add(
                                    posV, 
                                    new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                    );
                                
                                currentQuads.BottomRight = RayMath.Vector2Add(
                                    middleRight, 
                                    new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                );
                                
                                currentQuads.TopRight = RayMath.Vector2Add(
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
                                
                                currentQuads.BottomLeft = RayMath.Vector2Add(
                                    middleLeft, 
                                    new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                );
                                
                                currentQuads.TopLeft = RayMath.Vector2Add(
                                    middleLeft, 
                                    new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                );
                                
                                currentQuads.BottomRight = RayMath.Vector2Add(
                                    posV, 
                                    new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                );
                                
                                currentQuads.TopRight = RayMath.Vector2Add(
                                    posV, 
                                    new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                );
                            }
                        }
                        else if (fetchedSelected[0].prop.type == InitPropType.Long)
                        {
                            var (left, top, right, bottom) = Utils.LongSides(fetchedSelected[0].prop.prop.Quads);
                            
                            var beta = RayMath.Vector2Angle(RayMath.Vector2Subtract(left, right), new(1.0f, 0.0f));
                            
                            var r = RayMath.Vector2Length(RayMath.Vector2Subtract(currentQuads.TopLeft, left));

                            if (CheckCollisionPointCircle(tileMouseWorld, left, 5f) && _quadLock == 0)
                            {
                                _quadLock = 1;
                            }
                            else if (CheckCollisionPointCircle(tileMouseWorld, right, 5f) && _quadLock == 0)
                            {
                                _quadLock = 2;
                            }
                            else if (CheckCollisionPointCircle(tileMouseWorld, top, 5f) && _quadLock == 0)
                            {
                                _quadLock = 3;
                            }
                            else if (CheckCollisionPointCircle(tileMouseWorld, bottom, 5f) && _quadLock == 0)
                            {
                                _quadLock = 4;
                            }

                            switch (_quadLock)
                            {
                                case 1: // left
                                    currentQuads.BottomLeft = RayMath.Vector2Add(
                                        posV, 
                                        new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.TopLeft = RayMath.Vector2Add(
                                        posV, 
                                        new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.BottomRight = RayMath.Vector2Add(
                                        right, 
                                        new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.TopRight = RayMath.Vector2Add(
                                        right, 
                                        new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                    );
                                    break;
                                
                                case 2: // right
                                    currentQuads.BottomLeft = RayMath.Vector2Add(
                                        left, 
                                        new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.TopLeft = RayMath.Vector2Add(
                                        left, 
                                        new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.BottomRight = RayMath.Vector2Add(
                                        posV, 
                                        new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.TopRight = RayMath.Vector2Add(
                                        posV, 
                                        new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                    );
                                    break;
                                
                                case 3: // TODO: top
                                    break;
                                
                                case 4: // TODO: bottom
                                    break;
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
                                currentQuads.TopLeft = posV;
                            }
                            // Check Top-Right
                            else if (_quadLock == 2)
                            {
                                currentQuads.TopRight = posV;
                            }
                            // Check Bottom-Right 
                            else if (_quadLock == 3)
                            {
                                currentQuads.BottomRight = posV;
                            }
                            // Check Bottom-Left
                            else if (_quadLock == 4)
                            {
                                currentQuads.BottomLeft = posV;
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
                                        points[p].X, 
                                        points[p].Y
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
                    if ((_shortcuts.SelectProps.Check(ctrl, shift, alt, true) || _shortcuts.SelectPropsAlt.Check(ctrl, shift, alt, true)) && !_clickTracker && canDrawTile)
                    {
                        _selection1 = GetScreenToWorld2D(GetMousePosition(), _camera);
                        _clickTracker = true;
                    }

                    if ((IsMouseButtonReleased(_shortcuts.SelectProps.Button) || IsKeyReleased(_shortcuts.SelectPropsAlt.Key)) && _clickTracker)
                    {
                        _clickTracker = false;
                    
                        // Selection rectangle should be now updated

                        // If selection rectangle is too small, it's treated
                        // like a point

                        for (var i = 0; i < GLOBALS.Level.Props.Length; i++)
                        {
                            var current = GLOBALS.Level.Props[i];
                            var propSelectRect = Utils.EncloseQuads(current.prop.Quads);
                            if (_shortcuts.PropSelectionModifier.Check(ctrl, shift, alt, true))
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
            DrawRectangle(0, 0, GLOBALS.Level.Width * previewScale, GLOBALS.Level.Height * previewScale, GLOBALS.Layer == 2 ? new(100, 100, 100, 100) : WHITE);

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
                        true,
                        false,
                        255
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
                if (GLOBALS.Layer != 2) DrawRectangle(0, 0, GLOBALS.Level.Width * previewScale, GLOBALS.Level.Height * previewScale,  new(100, 100, 100, 150));

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
                        true,
                        false,
                        (byte)(GLOBALS.Layer < 2 ? 255 : 90)
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
                        true,
                        false,
                        (byte)(GLOBALS.Layer < 1 ? 255 : 90)
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
                                
                                /*DrawCircleV(quads.TopLeft, 2f, BLUE);
                                DrawCircleV(quads.TopRight, 2f, BLUE);
                                DrawCircleV(quads.BottomRight, 2f, BLUE);
                                DrawCircleV(quads.BottomLeft, 2f, BLUE);*/
                            }
                            else if (current.type == InitPropType.Long)
                            {
                                var sides = Utils.LongSides(current.prop.Quads);
                                
                                DrawCircleV(sides.left, 5f, BLUE);
                                DrawCircleV(sides.top, 5f, BLUE);
                                DrawCircleV(sides.right, 5f, BLUE);
                                DrawCircleV(sides.bottom, 5f, BLUE);
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

                                {
                                    if (currentTileAsPropCategory.index >= GLOBALS.Textures.Tiles.Length)
                                    {
                                        _logger.Fatal($"failed to fetch tile-as-prop texture from {nameof(GLOBALS.Textures.Tiles)}[{GLOBALS.Textures.Tiles.Length}]: {nameof(currentTileAsPropCategory)}.length ({currentTileAsPropCategory.index}) was outside the bounds of the array.");
                                        throw new IndexOutOfRangeException($"failed to fetch tile-as-prop texture from {nameof(GLOBALS.Textures.Tiles)}[{GLOBALS.Textures.Tiles.Length}]: {nameof(currentTileAsPropCategory)}.length ({currentTileAsPropCategory.index}) was outside the bounds of the array.");
                                    }
                                    
                                    var ind = _tilesAsPropsIndices[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];

                                    if (ind.index >= GLOBALS.Textures.Tiles[currentTileAsPropCategory.index].Length)
                                    {
                                        _logger.Fatal($"failed to fetch tile-as-prop texture from {nameof(GLOBALS.Textures.Tiles)}[{nameof(currentTileAsPropCategory)}][Length: {GLOBALS.Textures.Tiles[currentTileAsPropCategory.index].Length}]: {nameof(_tilesAsPropsIndices)}][{_propsMenuTilesCategoryIndex}][{_propsMenuTilesIndex}] ({ind.index}) was outside of the bounds of the array.");
                                        throw new IndexOutOfRangeException(
                                            $"failed to fetch tile-as-prop texture from {nameof(GLOBALS.Textures.Tiles)}[{nameof(currentTileAsPropCategory)}][{GLOBALS.Textures.Tiles[currentTileAsPropCategory.index].Length}]: {nameof(_tilesAsPropsIndices)}][{_propsMenuTilesCategoryIndex}][{_propsMenuTilesIndex}] ({ind.index}) was outside of the bounds of the array.");
                                    }
                                }
                                #endif

                                var currentTileAsProp = _tilesAsPropsIndices[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
                                var currentTileAsPropTexture = GLOBALS.Textures.Tiles[currentTileAsPropCategory.index][currentTileAsProp.index];
                                
                                var layerHeight = (currentTileAsProp.init.Size.Item2 + (currentTileAsProp.init.BufferTiles * 2)) * scale;
                                var textureCutWidth = (currentTileAsProp.init.Size.Item1 + (currentTileAsProp.init.BufferTiles * 2)) * scale;
                                const float scaleConst = 0.4f;

                                var width = scaleConst * textureCutWidth;
                                var height = scaleConst * layerHeight;

                                switch (_snapMode)
                                {
                                    case 0: // free
                                        Printers.DrawTileAsProp(
                                            ref currentTileAsPropTexture,
                                            ref currentTileAsProp.init,
                                            ref tileMouseWorld,
                                            [
                                                new Vector2(width, -height),
                                                new Vector2(-width, -height),
                                                new Vector2(-width, height),
                                                new Vector2(width, height),
                                                new Vector2(width, -height)
                                            ]
                                        );
                                        break;

                                    case 1: // grid snap
                                    {
                                        var posV = new Vector2(tileMatrixX, tileMatrixY) * GLOBALS.PreviewScale;
                                        
                                        Printers.DrawTileAsProp(
                                            ref currentTileAsPropTexture,
                                            ref currentTileAsProp.init,
                                            ref posV,
                                            [
                                                new Vector2(width, -height),
                                                new Vector2(-width, -height),
                                                new Vector2(-width, height),
                                                new Vector2(width, height),
                                                new Vector2(width, -height)
                                            ]
                                        );
                                    }
                                        break;
                                    
                                    case 2: // precise grid snap
                                    {
                                        var posV = new Vector2((int)(tileMouseWorld.X / 8f), (int)(tileMouseWorld.Y / 8f)) * 8f;
                                        
                                        Printers.DrawTileAsProp(
                                            ref currentTileAsPropTexture,
                                            ref currentTileAsProp.init,
                                            ref posV,
                                            [
                                                new Vector2(width, -height),
                                                new Vector2(-width, -height),
                                                new Vector2(-width, height),
                                                new Vector2(width, height),
                                                new Vector2(width, -height)
                                            ]
                                        );
                                    }
                                        break;
                                }
                            }
                            break;
                        
                        case 1: // Current Rope
                            DrawCircleV(tileMouseWorld, 3f, BLUE);
                            break;

                        case 2: // Current Long Prop
                        {
                            var prop = GLOBALS.LongProps[_propsMenuLongsIndex];
                            var texture = GLOBALS.Textures.LongProps[_propsMenuLongsIndex];
                            var height = texture.height / 2f;

                            var posV = _snapMode switch
                            {
                                1 => new Vector2(tileMatrixX, tileMatrixY) * GLOBALS.PreviewScale,
                                2 => new Vector2((int)(tileMouseWorld.X / 8f), (int)(tileMouseWorld.Y / 8f)) * 8f,
                                _ => tileMouseWorld,
                            };
                            
                            Printers.DrawProp(
                                new PropLongSettings(), 
                                prop, 
                                texture, 
                                new PropQuads
                                {
                                    TopLeft = new(posV.X - 100, posV.Y - height),
                                    BottomLeft = new(posV.X - 100, posV.Y + height),
                                    TopRight = new(posV.X + 100, posV.Y - height),
                                    BottomRight = new(posV.X + 100, posV.Y + height)
                                }, 
                                0
                            );
                        }
                            break;

                        case 3: // Current Prop
                        {
                            // Since I've already seperated regular props from everything else, this can be
                            // considered outdated
                            var prop = GLOBALS.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
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
                            
                            var posV = _snapMode switch
                            {
                                1 => new Vector2(tileMatrixX, tileMatrixY) * GLOBALS.PreviewScale,
                                2 => new Vector2((int)(tileMouseWorld.X / 8f), (int)(tileMouseWorld.Y / 8f)) * 8f,
                                _ => tileMouseWorld,
                            };
                            
                            Printers.DrawProp(settings, prop, texture, new PropQuads(
                                new Vector2(posV.X - width, posV.Y - height), 
                                new Vector2(posV.X + width, posV.Y - height), 
                                new Vector2(posV.X + width, posV.Y + height), 
                                new Vector2(posV.X - width, posV.Y + height)),
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
                    DrawRectangleRec(new Rectangle(menuPanelRect.x + 30 + (_menuRootCategoryIndex * 40), 40, 40, 40), BLUE);

                    var tilesAsPropsRect = new Rectangle(menuPanelRect.x + 30, 40, 40, 40);
                    var ropesRect = new Rectangle(menuPanelRect.x + 30 + (40), 40, 40, 40);
                    var longsRect = new Rectangle(menuPanelRect.x + 30 + (80), 40, 40, 40);
                    var othersRect = new Rectangle(menuPanelRect.x + 30 + (120), 40, 40, 40);

                    var tilesAsPropsHovered = CheckCollisionPointRec(tileMouse, tilesAsPropsRect);
                    var ropesHovered = CheckCollisionPointRec(tileMouse, ropesRect);
                    var longsHovered = CheckCollisionPointRec(tileMouse, longsRect);
                    var othersHovered = CheckCollisionPointRec(tileMouse, othersRect);
                    
                    if (tilesAsPropsHovered)
                    {
                        DrawRectangleRec(tilesAsPropsRect, BLUE with { a = 100 });
                        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) _menuRootCategoryIndex = 0;
                    }
                    if (ropesHovered)
                    {
                        DrawRectangleRec(ropesRect, BLUE with { a = 100 });
                        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) _menuRootCategoryIndex = 1;
                    }
                    if (longsHovered)
                    {
                        DrawRectangleRec(longsRect, BLUE with { a = 100 });
                        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) _menuRootCategoryIndex = 2;
                    }
                    if (othersHovered)
                    {
                        DrawRectangleRec(othersRect, BLUE with { a = 100 });
                        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) _menuRootCategoryIndex = 3;
                    }
                    
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
                                    fixed (int* fc = &_propsMenuTilesCategoryItemFocus)
                                    {
                                        // draw the category list first
                                        newCategoryIndex = RayGui.GuiListViewEx(
                                            categoryRect,
                                            _tilesAsPropsCategoryNames,
                                            _tilesAsPropsCategoryIndices.Length,
                                            fc,
                                            scrollIndex,
                                            _propsMenuTilesCategoryIndex);
                                    }
                                }
                            }

                            if (newCategoryIndex != _propsMenuTilesCategoryIndex && newCategoryIndex != -1)
                            {
                                #if DEBUG
                                _logger.Debug($"New tiles category index: {newCategoryIndex}");
                                #endif
                                
                                _propsMenuTilesIndex = 0;
                                _propsMenuTilesCategoryIndex = newCategoryIndex;
                            }

                            unsafe
                            {
                                fixed (int* scrollIndex = &_propsMenuTilesScrollIndex)
                                {
                                    fixed (int* fc = &_propsMenuTilesItemFocus)
                                    {
                                        // draw the list

                                        var newPropsMenuTilesIndex = RayGui.GuiListViewEx(
                                            listRect,
                                            _tilesAsPropsNames[_propsMenuTilesCategoryIndex],
                                            _tilesAsPropsNames[_propsMenuTilesCategoryIndex].Length,
                                            fc,
                                            scrollIndex,
                                            _propsMenuTilesIndex
                                        );

                                        if (newPropsMenuTilesIndex != _propsMenuTilesIndex && newPropsMenuTilesIndex != -1)
                                        {
                                            #if DEBUG
                                            _logger.Debug($"New tiles index: {newPropsMenuTilesIndex}");
                                            #endif
                                            
                                            _propsMenuTilesIndex = newPropsMenuTilesIndex;
                                        }
                                    }
                                }
                            }
                        }
                            break;

                        case 1: // Ropes
                        {
                            unsafe
                            {
                                int newIndex;

                                fixed (int* scrollIndex = &_propsMenuRopesScrollIndex)
                                {
                                    fixed (int* fc = &_propsMenuRopesItemFocus)
                                    {
                                        newIndex = RayGui.GuiListViewEx(
                                            categoryRect with { width = menuPanelRect.width - 10 },
                                            _ropeNames,
                                            _ropeNames.Length,
                                            fc,
                                            scrollIndex,
                                            _propsMenuRopesIndex
                                        );
                                    }
                                }

                                if (newIndex != _propsMenuRopesIndex && newIndex != -1)
                                {
                                    #if DEBUG
                                    _logger.Debug($"New props Index: {newIndex}");
                                    #endif
                                    
                                    _propsMenuRopesIndex = newIndex;
                                }
                            }
                        }
                            break;

                        case 2: // Long Props
                        {
                            int newIndex;
                            
                            unsafe
                            {
                                fixed (int* scrollIndex = &_propsMenuLongsScrollIndex)
                                {
                                    fixed (int* fc = &_propsMenuLongItemFocus)
                                    {
                                        newIndex = RayGui.GuiListViewEx(
                                            categoryRect with { width = menuPanelRect.width - 10 },
                                            _longNames,
                                            _longNames.Length,
                                            fc,
                                            scrollIndex,
                                            _propsMenuLongsIndex
                                        );
                                    }
                                }
                            }

                            if (newIndex != _propsMenuLongsIndex && newIndex != -1)
                            {
                                #if DEBUG
                                _logger.Debug($"New longs index: {newIndex}");
                                #endif
                                
                                _propsMenuLongsIndex = newIndex;
                            }
                        }

                    break;

                        case 3: // Props
                        {
                            int newCategoryIndex;

                            unsafe
                            {
                                fixed (int* scrollIndex = &_propsMenuOthersCategoryScrollIndex)
                                {
                                    fixed (int* fc = &_propsMenuOtherCategoryItemFocus)
                                    {
                                        // draw the category list first
                                        newCategoryIndex = RayGui.GuiListViewEx(
                                            categoryRect,
                                            _otherCategoryNames,
                                            _otherCategoryNames.Length,
                                            fc,
                                            scrollIndex,
                                            _propsMenuOthersCategoryIndex);
                                    }
                                }
                            }
                            
                            // reset selection index when changing categories
                            if (newCategoryIndex != _propsMenuOthersCategoryIndex && newCategoryIndex != -1)
                            {
                                #if DEBUG
                                _logger.Debug($"New others category index: {_propsMenuOthersCategoryIndex}");
                                #endif
                                
                                _propsMenuOthersIndex = 0;
                                _propsMenuOthersCategoryIndex = newCategoryIndex;
                            }
                            
                            unsafe
                            {
                                fixed (int* scrollIndex = &_propsMenuOthersScrollIndex)
                                {
                                    fixed (int* fc = &_propsMenuOtherItemFocus)
                                    {
                                        // draw the list

                                        var  newPropsMenuOthersIndex = RayGui.GuiListViewEx(
                                            listRect,
                                            _otherNames[_propsMenuOthersCategoryIndex],
                                            _otherNames[_propsMenuOthersCategoryIndex].Length,
                                            fc,
                                            scrollIndex,
                                            _propsMenuOthersIndex
                                        );

                                        if (newPropsMenuOthersIndex != _propsMenuOthersIndex &&
                                            newPropsMenuOthersIndex != -1)
                                        {
                                            #if DEBUG
                                            _logger.Debug($"New other prop index: {newPropsMenuOthersIndex}");
                                            #endif
                                            
                                            _propsMenuOthersIndex = newPropsMenuOthersIndex;
                                        }
                                    }
                                }
                            }
                        }
                            break;
                    }
                    
                    // Depth Indicator

                    unsafe
                    {
                        var d = -_defaultDepth;
                        
                        RayGui.GuiSpinner(
                            new Rectangle(
                                sWidth-205, 
                                categoryRect.Y+categoryRect.height+10, 
                                200, 
                                40
                            ), 
                            "Default Depth",
                            &d,
                            0,
                            29,
                            false
                        );

                        _defaultDepth = -d;
                    }

                    // Focus indicator
                    if (_menuRootCategoryIndex is 0 or 3)
                    {
                        
                        DrawRectangleLinesEx(
                            _propCategoryFocus ? categoryRect : listRect,
                            4f,
                            BLUE
                        );
                    }
                
                    // No-Collision Prop Placement Indicator

                    {
                        var texture = GLOBALS.Textures.PropGenerals[0];
                        var rect = new Rectangle(sWidth - 5 - 100, sHeight - 45, 40, 40);
                        var rectHovered = CheckCollisionPointRec(tileMouse, rect);
                        
                        if (rectHovered)
                        {
                            DrawRectangleRec(rect, BLUE with { a = 100 });

                            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                                _noCollisionPropPlacement = !_noCollisionPropPlacement;
                        }
                        if (_noCollisionPropPlacement) DrawRectangleRec(rect, BLUE);
                        
                        DrawTexturePro(
                            texture,
                            new Rectangle(0, 0, texture.width, texture.height),
                            rect,
                            new Vector2(0, 0),
                            0,
                            _noCollisionPropPlacement ? WHITE : BLACK
                        );
                    }
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
                            if (RayGui.GuiButton(simButton with { Y = 180 },
                                $"{60 / _ropeSimulationFrameCut} FPS")) _ropeSimulationFrameCut = ++_ropeSimulationFrameCut % 3 + 1;
                            
                            var release = (fetchedSelected[0].prop.prop.Extras.Settings as PropRopeSettings).Release;

                            var releaseClicked = RayGui.GuiButton(simButton with { Y = 230 }, release switch
                            {
                                PropRopeRelease.Left => "Release Left",
                                PropRopeRelease.None => "Release None",
                                PropRopeRelease.Right => "Release Right"
                            });

                            if (releaseClicked)
                            {
                                release = (PropRopeRelease)((int)release + 1);
                                if ((int)release > 2) release = (PropRopeRelease)0;

                                (fetchedSelected[0].prop.prop.Extras.Settings as PropRopeSettings).Release = release;
                            }
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
                        (int)(menuPanelRect.height - 450)
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

                        _selected = new bool [GLOBALS.Level.Props.Length];
                        
                        fetchedSelected = GLOBALS.Level.Props
                            .Select((prop, index) => (prop, index))
                            .Where(p => _selected[p.index])
                            .Select(p => p)
                            .ToArray();
                        
                        ImportRopeModels();
                        
                        /*_stretchingProp = false;
                        _movingProps = false;
                        _rotatingProps = false;*/
                    }
                    
                    DrawLineEx(new(menuPanelRect.x + 11, listRect.y - 32), new(menuPanelRect.x + 29, listRect.y - 11), 2f, WHITE);
                    DrawLineEx(new(menuPanelRect.x + 11, listRect.y - 11), new(menuPanelRect.x + 29, listRect.y - 32), 2f, WHITE);
                    
                    // Prop Settings

                    var indicatorOffset = listRect.x + (listRect.width - 290) / 2f;
                    
                    if (fetchedSelected.Length == 1)
                    {
                        // Depth indicator
                        
                        var (c, i) = fetchedSelected[0].prop.position;

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
                                        listRect.y + listRect.height + 40, 
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
                                        listRect.y + listRect.height + 40, 
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
                                        listRect.y + listRect.height + 40, 
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
                            new Rectangle(indicatorOffset, listRect.y + listRect.height + 40, 290, 30),
                            2f,
                            BLACK
                        );
                    
                        DrawLineEx(
                            new Vector2(indicatorOffset + 90, listRect.y + listRect.height + 40),
                            new Vector2(indicatorOffset + 90, listRect.y + listRect.height + 45),
                            2f,
                            BLACK
                        );
                    
                        DrawLineEx(
                            new Vector2(indicatorOffset + 180, listRect.y + listRect.height + 40),
                            new Vector2(indicatorOffset + 180, listRect.y + listRect.height + 45),
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
                                new Rectangle(indicatorOffset, listRect.y + listRect.height + 90, 290, 40),
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

                                RayGui.GuiSpinner(
                                    new Rectangle(
                                        indicatorOffset, 
                                        listRect.y + listRect.height + 140, 
                                        290, 
                                        40
                                    ), 
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
                    
                    // Page buttons
                    if (GLOBALS.Level.Props.Length > pageSize * (_selectedListPage + 1))
                    {
                        var downClicked = RayGui.GuiButton(
                            listRect with
                            {
                                Y = listRect.Y + listRect.height, width = listRect.width / 2f, height = 30
                            },
                            "Down");

                        if (downClicked) _selectedListPage++;
                    }
                    
                    if (_selectedListPage != 0)
                    {
                        var upClicked = RayGui.GuiButton(
                            listRect with
                            {
                                Y = listRect.Y + listRect.height, width = listRect.width / 2f, height = 30,
                                X = listRect.X + listRect.width / 2f
                            },
                            "Up");

                        if (upClicked) _selectedListPage--;
                    }
                    
                    // Edit Mode Indicators

                    {
                        var moveTexture = GLOBALS.Textures.PropEditModes[0];
                        var rotateTexture = GLOBALS.Textures.PropEditModes[1];
                        var scaleTexture = GLOBALS.Textures.PropEditModes[2];
                        var warpTexture = GLOBALS.Textures.PropEditModes[3];
                        var editPointsTexture = GLOBALS.Textures.PropEditModes[4];

                        var moveRect = new Rectangle(menuPanelRect.X + 5 + 90, sHeight - 45, 40, 40);
                        var rotateRect = new Rectangle(menuPanelRect.X + 5 + 130, sHeight - 45, 40, 40);
                        var scaleRect = new Rectangle(menuPanelRect.X + 5 + 170, sHeight - 45, 40, 40);
                        var warpRect = new Rectangle(menuPanelRect.X + 5 + 210, sHeight - 45, 40, 40);
                        var editPointsRect = new Rectangle(menuPanelRect.X + 5 + 250, sHeight - 45, 40, 40);
                        
                        if (_movingProps) DrawRectangleRec(moveRect, BLUE);
                        if (_rotatingProps) DrawRectangleRec(rotateRect, BLUE);
                        if (_scalingProps) DrawRectangleRec(scaleRect, BLUE);
                        if (_stretchingProp) DrawRectangleRec(warpRect, BLUE);
                        if (_editingPropPoints) DrawRectangleRec(editPointsRect, BLUE);
                        
                        DrawTexturePro(
                            moveTexture, 
                            new Rectangle(0, 0, moveTexture.width, moveTexture.height),
                            moveRect,
                            new Vector2(0, 0),
                            0,
                            _movingProps ? WHITE : BLACK);
                        
                        DrawTexturePro(
                            rotateTexture, 
                            new Rectangle(0, 0, rotateTexture.width, rotateTexture.height),
                            rotateRect,
                            new Vector2(0, 0),
                            0,
                            _rotatingProps ? WHITE : BLACK);
                        
                        DrawTexturePro(
                            scaleTexture, 
                            new Rectangle(0, 0, scaleTexture.width, scaleTexture.height),
                            scaleRect,
                            new Vector2(0, 0),
                            0,
                            _scalingProps ? WHITE : BLACK);
                        
                        DrawTexturePro(
                            warpTexture, 
                            new Rectangle(0, 0, warpTexture.width, warpTexture.height),
                            warpRect,
                            new Vector2(0, 0),
                            0,
                            _stretchingProp ? WHITE: BLACK);
                        
                        DrawTexturePro(
                            editPointsTexture, 
                            new Rectangle(0, 0, editPointsTexture.width, editPointsTexture.height),
                            editPointsRect,
                            new Vector2(0, 0),
                            0,
                            _editingPropPoints ? WHITE : BLACK);
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

            var selectModeRect = new Rectangle(menuPanelRect.x + 5, sHeight - 45, 40, 40);
            var selectModeHovered = CheckCollisionPointRec(tileMouse, selectModeRect);

            if (selectModeHovered)
            {
                DrawRectangleRec(selectModeRect, BLUE with { a = 100 });

                if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) _mode = 0;
            }
                
            DrawTexturePro(
                selectModeTexture,
                new Rectangle(0, 0, selectModeTexture.width, selectModeTexture.height),
                selectModeRect,
                new(0, 0),
                0,
                _mode == 0 ? WHITE : BLACK
            );

            var placeModeRect = new Rectangle(menuPanelRect.x + 45, sHeight - 45, 40, 40);
            var placeModeHovered = CheckCollisionPointRec(tileMouse, placeModeRect);
            if (placeModeHovered)
            {
                DrawRectangleRec(placeModeRect, BLUE with { a = 100 });

                if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) _mode = 1;
            }
                
            DrawTexturePro(
                placeModeTexture,
                new Rectangle(0, 0, selectModeTexture.width, selectModeTexture.height),
                placeModeRect,
                new(0, 0),
                0,
                _mode == 1 ? WHITE : BLACK
            );
            
            
            // Snap mode

            var snapButtonRect = new Rectangle(menuPanelRect.X + menuPanelRect.width - 45, sHeight - 45, 40, 40);
            
            DrawRectangleRec(snapButtonRect, BLUE);

            DrawText(
                _snapMode switch { 1 => "1.0", 2 => "0.5", _ => "0.0" }, 
                snapButtonRect.X + 10, 
                snapButtonRect.Y + 10, 
                20, 
                WHITE
            );

            if (CheckCollisionPointRec(tileMouse, snapButtonRect) &&
                IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                _snapMode = ++_snapMode % 3;
            }

            // layer indicator

            var newLayer = GLOBALS.Layer;

            var layer3Hovered = GLOBALS.Layer == 2 && CheckCollisionPointRec(tileMouse, layer3Rect);

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
                var layer2Hovered = GLOBALS.Layer == 1 && CheckCollisionPointRec(tileMouse, layer2Rect);

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
                var layer1Hovered = CheckCollisionPointRec(tileMouse, layer1Rect);

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

            if (newLayer != GLOBALS.Layer)
            {
                GLOBALS.Layer = newLayer;
                UpdateDefaultDepth();
            }
        }
        #endregion
        
        // Shortcuts window
        if (GLOBALS.Settings.GeneralSettings.ShortcutWindow)
        {
            rlImGui.Begin();
            var shortcutWindowRect = Printers.ImGui.ShortcutsWindow(GLOBALS.Settings.Shortcuts.TileEditor);

            _isShortcutsWinHovered = CheckCollisionPointRec(
                tileMouse, 
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

        EndDrawing();
        #endregion
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera;
    }
}
