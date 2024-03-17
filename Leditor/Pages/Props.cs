﻿using System.Numerics;
using ImGuiNET;
using rlImGui_cs;
using static Raylib_cs.Raylib;

namespace Leditor.Pages;

internal class PropsEditorPage : EditorPage
{
    private Camera2D _camera;

    private readonly PropsShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.PropsEditor;
    
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
    
    private int _snapMode;

    private int _selectedListPage;

    private int _quadLock;
    private int _pointLock = -1;
    private int _bezierHandleLock = -1;

    private bool _clickTracker;

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
    private int _defaultVariation;
    private int _defaultSeed;

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
    
    private (int index, bool simSwitch, RopeModel model, Vector2[] bezierHandles)[] _models;
    
    private bool _isShortcutsWinHovered;
    private bool _isShortcutsWinDragged;
    
    private bool _isNavigationWinHovered;
    private bool _isNavigationWinDragged;
    
    private bool _isPropsWinHovered;
    private bool _isPropsWinDragged;

    public PropsEditorPage()
    {
        _camera = new() { Zoom = 0.8f };

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

    public override void Draw()
    {
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera;
        
        var ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
        var shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
        var alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);
            
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

        var menuPanelRect = new Rectangle(sWidth - 360, 0, 360, sHeight);

        //                        v this was done to avoid rounding errors
        var tileMatrixY = tileMouseWorld.Y < 0 ? -1 : (int)tileMouseWorld.Y / previewScale;
        var tileMatrixX = tileMouseWorld.X < 0 ? -1 : (int)tileMouseWorld.X / previewScale;

        var canDrawTile = !_isPropsWinHovered && 
                          !_isPropsWinDragged && 
                          !_isShortcutsWinHovered && 
                          !_isShortcutsWinDragged && 
                          !_isNavigationWinHovered &&
                          !_isNavigationWinDragged &&
                          !CheckCollisionPointRec(tileMouse, menuPanelRect) &&
                          !CheckCollisionPointRec(tileMouse, layer3Rect) &&
                          (GLOBALS.Layer != 1 || !CheckCollisionPointRec(tileMouse, layer2Rect)) &&
                          (GLOBALS.Layer != 0 || !CheckCollisionPointRec(tileMouse, layer1Rect));

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
                Logger.Debug("go from GLOBALS.Page 8 to GLOBALS.Page 6");
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
            delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
            _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
        }

        // handle zoom
        var tileWheel = GetMouseWheelMove();
        if (tileWheel != 0 && canDrawTile)
        {
            var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
            _camera.Offset = GetMousePosition();
            _camera.Target = mouseWorldPosition;
            _camera.Zoom += tileWheel * GLOBALS.ZoomIncrement;
            if (_camera.Zoom < GLOBALS.ZoomIncrement) _camera.Zoom = GLOBALS.ZoomIncrement;
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
                                var height = texture.Height / 2f;
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
                                    InitVariedStandardProp variedStandard => (variedStandard.Size.x * GLOBALS.PreviewScale / 2f, variedStandard.Size.y * GLOBALS.PreviewScale / 2f, new PropVariedSettings(variation:_defaultVariation)),
                                    InitStandardProp standard => (standard.Size.x * GLOBALS.PreviewScale / 2f, standard.Size.y * GLOBALS.PreviewScale / 2f, new BasicPropSettings()),
                                    InitVariedSoftProp variedSoft => (variedSoft.SizeInPixels.x  / 2f, variedSoft.SizeInPixels.y / 2f, new PropVariedSoftSettings(variation:_defaultVariation)),
                                    InitSoftProp => (texture.Width  / 2f, texture.Height  / 2f, new PropSoftSettings()),
                                    InitVariedDecalProp variedDecal => (variedDecal.SizeInPixels.x  / 2f, variedDecal.SizeInPixels.y / 2f, new PropVariedDecalSettings(variation:_defaultVariation)),
                                    InitSimpleDecalProp => (texture.Width / 2f, texture.Height / 2f, new PropSimpleDecalSettings()), 
                                    InitSoftEffectProp => (texture.Width / 2f, texture.Height / 2f, new PropSoftEffectSettings()), 
                                    InitAntimatterProp => (texture.Width / 2f, texture.Height / 2f, new PropAntimatterSettings()),
                                    InitLongProp => (texture.Width / 2f, texture.Height / 2f, new PropLongSettings()), 
                                    InitRopeProp => (texture.Width / 2f, texture.Height / 2f, new PropRopeSettings()),
                                    
                                    _ => (texture.Width / 2f, texture.Height / 2f, new BasicPropSettings())
                                };

                                if (_newlyCopied)
                                {
                                    _newlyCopied = false;
                                
                                    settings = _copiedPropSettings;
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
                                    settings = new(thickness: current.Name is "Wire" or "Zero-G Wire" ? 2 : null);
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
                                var height = texture.Height / 2f;
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
                                    InitVariedStandardProp variedStandard => (variedStandard.Size.x * GLOBALS.PreviewScale / 2f, variedStandard.Size.y * GLOBALS.PreviewScale / 2f, new PropVariedSettings(variation:_defaultVariation)),
                                    InitStandardProp standard => (standard.Size.x * GLOBALS.PreviewScale / 2f, standard.Size.y * GLOBALS.PreviewScale / 2f, new BasicPropSettings()),
                                    InitVariedSoftProp variedSoft => (variedSoft.SizeInPixels.x  / 2f, variedSoft.SizeInPixels.y / 2f, new PropVariedSoftSettings(variation:_defaultVariation)),
                                    InitSoftProp => (texture.Width  / 2f, texture.Height  / 2f, new PropSoftSettings()),
                                    InitVariedDecalProp variedDecal => (variedDecal.SizeInPixels.x  / 2f, variedDecal.SizeInPixels.y / 2f, new PropVariedDecalSettings(variation:_defaultVariation)),
                                    InitSimpleDecalProp => (texture.Width / 2f, texture.Height / 2f, new PropSimpleDecalSettings()), 
                                    InitSoftEffectProp => (texture.Width / 2f, texture.Height / 2f, new PropSoftEffectSettings()), 
                                    InitAntimatterProp => (texture.Width / 2f, texture.Height / 2f, new PropAntimatterSettings()),
                                    InitLongProp => (texture.Width / 2f, texture.Height / 2f, new PropLongSettings()), 
                                    InitRopeProp => (texture.Width / 2f, texture.Height / 2f, new PropRopeSettings()),
                                    
                                    _ => (texture.Width / 2f, texture.Height / 2f, new BasicPropSettings())
                                };

                                if (_newlyCopied)
                                {
                                    _newlyCopied = false;

                                    settings = _copiedPropSettings;
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
                        _selectedPropsEncloser.X + 
                        _selectedPropsEncloser.Width/2, 
                        _selectedPropsEncloser.Y + 
                        _selectedPropsEncloser.Height/2
                    );
                }
                else
                {
                    _selectedPropsEncloser.Width = 0;
                    _selectedPropsEncloser.Height = 0;

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
                    
                    SetMouseCursor(MouseCursor.ResizeNesw);
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
                else SetMouseCursor(MouseCursor.Default);

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

                        if (IsMouseButtonDown(MouseButton.Left))
                        {
                            for (var b = 0; b < foundRope.bezierHandles.Length; b++)
                            {
                                if (_bezierHandleLock == -1 && CheckCollisionPointCircle(tileMouseWorld, foundRope.bezierHandles[b], 3f))
                                    _bezierHandleLock = b;

                                if (_bezierHandleLock == b) foundRope.bezierHandles[b] = tileMouseWorld;
                            }
                        }

                        if (IsMouseButtonReleased(MouseButton.Left) && _bezierHandleLock != -1)
                            _bezierHandleLock = -1;
                    }
                }
                
                // TODO: switch on enums instead
                if (_movingProps && anySelected)
                {
                    if (IsMouseButtonPressed(MouseButton.Left)) _movingProps = false;
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

                        quads.TopLeft = Raymath.Vector2Add(quads.TopLeft, delta);
                        quads.TopRight = Raymath.Vector2Add(quads.TopRight, delta);
                        quads.BottomRight = Raymath.Vector2Add(quads.BottomRight, delta);
                        quads.BottomLeft = Raymath.Vector2Add(quads.BottomLeft, delta);

                        GLOBALS.Level.Props[s].prop.Quads = quads;

                        if (GLOBALS.Level.Props[s].type == InitPropType.Rope)
                        {
                            if (!_ropeMode)
                            {
                                for (var p = 0; p < GLOBALS.Level.Props[s].prop.Extras.RopePoints.Length; p++)
                                {
                                    GLOBALS.Level.Props[s].prop.Extras.RopePoints[p] = Raymath.Vector2Add(GLOBALS.Level.Props[s].prop.Extras.RopePoints[p], delta);
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
                    if (IsMouseButtonPressed(MouseButton.Left)) _rotatingProps = false;

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
                    if (IsMouseButtonPressed(MouseButton.Left))
                    {
                        _stretchAxes = 0;
                        _scalingProps = false;
                        SetMouseCursor(MouseCursor.Default);
                    }

                    if (IsKeyPressed(KeyboardKey.X))
                    {
                        _stretchAxes = (byte)(_stretchAxes == 1 ? 0 : 1);
                        SetMouseCursor(MouseCursor.ResizeNesw);
                    }
                    if (IsKeyPressed(KeyboardKey.Y))
                    {
                        _stretchAxes =  (byte)(_stretchAxes == 2 ? 0 : 2);
                        SetMouseCursor(MouseCursor.ResizeNs);
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

                    if (IsMouseButtonDown(MouseButton.Left))
                    {
                        if (fetchedSelected[0].prop.type == InitPropType.Rope)
                        {
                            var middleLeft = Raymath.Vector2Divide(
                                Raymath.Vector2Add(currentQuads.TopLeft, currentQuads.BottomLeft),
                                new(2f, 2f)
                            );

                            var middleRight = Raymath.Vector2Divide(
                                Raymath.Vector2Add(currentQuads.TopRight, currentQuads.BottomRight),
                                new(2f, 2f)
                            );
                            
                            var beta = Raymath.Vector2Angle(Raymath.Vector2Subtract(middleLeft, middleRight), new(1.0f, 0.0f));
                            
                            var r = Raymath.Vector2Length(Raymath.Vector2Subtract(currentQuads.TopLeft, middleLeft));
                            
                            if (
                                CheckCollisionPointCircle(
                                    posV, middleLeft,
                                    5f
                                ) || _quadLock == 1)
                            {
                                _quadLock = 1;
                                currentQuads.BottomLeft = Raymath.Vector2Add(
                                    posV, 
                                    new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                    );
                                
                                currentQuads.TopLeft = Raymath.Vector2Add(
                                    posV, 
                                    new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                    );
                                
                                currentQuads.BottomRight = Raymath.Vector2Add(
                                    middleRight, 
                                    new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                );
                                
                                currentQuads.TopRight = Raymath.Vector2Add(
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
                                
                                currentQuads.BottomLeft = Raymath.Vector2Add(
                                    middleLeft, 
                                    new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                );
                                
                                currentQuads.TopLeft = Raymath.Vector2Add(
                                    middleLeft, 
                                    new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                );
                                
                                currentQuads.BottomRight = Raymath.Vector2Add(
                                    posV, 
                                    new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                );
                                
                                currentQuads.TopRight = Raymath.Vector2Add(
                                    posV, 
                                    new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                );
                            }
                        }
                        else if (fetchedSelected[0].prop.type == InitPropType.Long)
                        {
                            var (left, top, right, bottom) = Utils.LongSides(fetchedSelected[0].prop.prop.Quads);
                            
                            var beta = Raymath.Vector2Angle(Raymath.Vector2Subtract(left, right), new(1.0f, 0.0f));
                            
                            var r = Raymath.Vector2Length(Raymath.Vector2Subtract(currentQuads.TopLeft, left));

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
                                    currentQuads.BottomLeft = Raymath.Vector2Add(
                                        posV, 
                                        new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.TopLeft = Raymath.Vector2Add(
                                        posV, 
                                        new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.BottomRight = Raymath.Vector2Add(
                                        right, 
                                        new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.TopRight = Raymath.Vector2Add(
                                        right, 
                                        new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                    );
                                    break;
                                
                                case 2: // right
                                    currentQuads.BottomLeft = Raymath.Vector2Add(
                                        left, 
                                        new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.TopLeft = Raymath.Vector2Add(
                                        left, 
                                        new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.BottomRight = Raymath.Vector2Add(
                                        posV, 
                                        new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.TopRight = Raymath.Vector2Add(
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
                    else if (IsMouseButtonReleased(MouseButton.Left) && _quadLock != 0) _quadLock = 0;
                }
                else if (_editingPropPoints && fetchedSelected.Length == 1)
                {
                    var points = fetchedSelected[0].prop.prop.Extras.RopePoints;
                    
                    if (IsMouseButtonDown(MouseButton.Left))
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
                    else if (IsMouseButtonReleased(MouseButton.Left) && _pointLock != -1) _pointLock = -1;
                }
                else if (!_ropeMode)
                {
                    if ((_shortcuts.SelectProps.Check(ctrl, shift, alt, true) || _shortcuts.SelectPropsAlt.Check(ctrl, shift, alt, true)) && !_clickTracker && canDrawTile)
                    {
                        _selection1 = GetScreenToWorld2D(GetMousePosition(), _camera);
                        _clickTracker = true;
                    }

                    if ((IsMouseButtonReleased(_shortcuts.SelectProps.Button) || IsKeyReleased(_shortcuts.SelectPropsAlt.Key)) && _clickTracker && !(_isPropsWinHovered || _isPropsWinDragged))
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

        ClearBackground(Color.Gray);

        BeginMode2D(_camera);
        {
            // DrawRectangle(0, 0, GLOBALS.Level.Width * previewScale, GLOBALS.Level.Height * previewScale, GLOBALS.Layer == 2 ? new Color(100, 100, 100, 100) : Color.White);

            #region TileEditorLayer3
            if (_showTileLayer3)
            {
                // Draw geos first

                Printers.DrawGeoLayer(
                    2, 
                    GLOBALS.PreviewScale, 
                    false, 
                    Color.Black
                );

                // then draw the tiles

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
                    if (current.prop.Depth > -20) continue;

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
                            DrawCircleV(point, 3f, Color.White);
                        }
                    }
                    
                    if (_selected[p])
                    {
                        // Side Lines
                        
                        DrawRectangleLinesEx(Utils.EncloseQuads(current.prop.Quads), 1.2f, Color.Blue);
                        
                        // Quad Points

                        if (_stretchingProp)
                        {
                            if (current.type == InitPropType.Rope)
                            {
                                DrawCircleV(
                                    Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopLeft, quads.BottomLeft), 
                                        new(2f, 2f)), 
                                    5f, 
                                    Color.Blue
                                );
                                
                                DrawCircleV(
                                    Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopRight, quads.BottomRight), 
                                        new(2f, 2f)), 
                                    5f, 
                                    Color.Blue
                                );
                                
                                DrawCircleV(quads.TopLeft, 2f, Color.Blue);
                                DrawCircleV(quads.TopRight, 2f, Color.Blue);
                                DrawCircleV(quads.BottomRight, 2f, Color.Blue);
                                DrawCircleV(quads.BottomLeft, 2f, Color.Blue);
                            }
                            else
                            {
                                DrawCircleV(quads.TopLeft, 5f, Color.Blue);
                                DrawCircleV(quads.TopRight, 5f, Color.Blue);
                                DrawCircleV(quads.BottomRight, 5f, Color.Blue);
                                DrawCircleV(quads.BottomLeft, 5f, Color.Blue);
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
                                        Color.Red
                                    );
                                    break;
                                case 2:
                                    DrawLineEx(
                                        center with { Y = -10 },
                                        center with { Y = GLOBALS.Level.Height*GLOBALS.PreviewScale + 10 },
                                        2f,
                                        Color.Green
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
                                    DrawCircleV(point, 3f, Color.Red);
                                }
                            }
                            else
                            {
                                foreach (var point in current.prop.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 2f, Color.Orange);
                                }
                            }
                            
                            if (_ropeMode)
                            {
                                // p is copied as suggested by the code editor..
                                var p1 = p;
                                var model = _models.Single(r => r.index == p1);

                                if (!model.simSwitch)
                                {
                                    foreach (var handle in model.bezierHandles) DrawCircleV(handle, 3f, Color.Green);
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
                if (GLOBALS.Layer != 2) DrawRectangle(
                    0, 
                    0, 
                    GLOBALS.Level.Width * GLOBALS.PreviewScale, 
                    GLOBALS.Level.Height * GLOBALS.PreviewScale, 
                    Color.Gray with { A = 130 });

                Printers.DrawGeoLayer(
                    1, 
                    GLOBALS.PreviewScale, 
                    false, 
                    GLOBALS.Layer < 2
                        ? Color.Black 
                        : Color.Black with { A = 80 }
                );

                // Draw layer 2 tiles

                if (_showLayer2Tiles)
                {
                    Printers.DrawTileLayer(
                        1, 
                        GLOBALS.PreviewScale, 
                        false, 
                        !GLOBALS.Settings.TileEditor.UseTextures,
                        GLOBALS.Settings.TileEditor.TintedTiles,
                        (byte)(GLOBALS.Layer < 2 ? 255 : 80)
                    );
                }
                
                // then draw the props

                for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                {
                    if (_hidden[p]) continue;
                    
                    var current = GLOBALS.Level.Props[p];
                    
                    // Filter based on depth
                    if (current.prop.Depth > -10 || current.prop.Depth < -19) continue;

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
                            DrawCircleV(point, 3f, Color.White);
                        }
                    }
                    
                    if (_selected[p])
                    {
                        // Side Lines
                        
                        DrawRectangleLinesEx(Utils.EncloseQuads(current.prop.Quads), 1.2f, Color.Blue);
                        
                        // Quad Points

                        if (_stretchingProp)
                        {
                            if (current.type == InitPropType.Rope)
                            {
                                DrawCircleV(
                                    Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopLeft, quads.BottomLeft), 
                                        new(2f, 2f)), 
                                    5f, 
                                    Color.Blue
                                );
                                
                                DrawCircleV(
                                    Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopRight, quads.BottomRight), 
                                        new(2f, 2f)), 
                                    5f, 
                                    Color.Blue
                                );
                                
                                DrawCircleV(quads.TopLeft, 2f, Color.Blue);
                                DrawCircleV(quads.TopRight, 2f, Color.Blue);
                                DrawCircleV(quads.BottomRight, 2f, Color.Blue);
                                DrawCircleV(quads.BottomLeft, 2f, Color.Blue);
                            }
                            else
                            {
                                DrawCircleV(quads.TopLeft, 5f, Color.Blue);
                                DrawCircleV(quads.TopRight, 5f, Color.Blue);
                                DrawCircleV(quads.BottomRight, 5f, Color.Blue);
                                DrawCircleV(quads.BottomLeft, 5f, Color.Blue);
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
                                        Color.Red
                                    );
                                    break;
                                case 2:
                                    DrawLineEx(
                                        center with { Y = -10 },
                                        center with { Y = GLOBALS.Level.Height*GLOBALS.PreviewScale + 10 },
                                        2f,
                                        Color.Green
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
                                    DrawCircleV(point, 3f, Color.Red);
                                }
                            }
                            else
                            {
                                foreach (var point in current.prop.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 2f, Color.Orange);
                                }
                            }
                            
                            if (_ropeMode)
                            {
                                // p is copied as suggested by the code editor..
                                var p1 = p;
                                var model = _models.Single(r => r.index == p1);

                                if (!model.simSwitch)
                                {
                                    foreach (var handle in model.bezierHandles) DrawCircleV(handle, 3f, Color.Green);
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
                if (GLOBALS.Layer != 1 && GLOBALS.Layer!= 2) 
                    DrawRectangle(
                        0, 
                        0, 
                        GLOBALS.Level.Width * GLOBALS.PreviewScale, 
                        GLOBALS.Level.Height * GLOBALS.PreviewScale, 
                        Color.Gray with { A = 130 }
                    );

                Printers.DrawGeoLayer(
                    0, 
                    GLOBALS.PreviewScale, 
                    false, 
                    GLOBALS.Layer == 0
                        ? Color.Black 
                        : Color.Black with { A = 80 }
                );

                // Draw layer 1 tiles

                if (_showLayer1Tiles)
                {
                    Printers.DrawTileLayer(
                        0, 
                        GLOBALS.PreviewScale, 
                        false, 
                        !GLOBALS.Settings.TileEditor.UseTextures,
                        GLOBALS.Settings.TileEditor.TintedTiles,
                        (byte)(GLOBALS.Layer == 0 ? 255 : 80)
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
                            DrawCircleV(point, 3f, Color.White);
                        }
                    }
                    
                    if (_selected[p])
                    {
                        // Side Lines
                        
                        DrawRectangleLinesEx(Utils.EncloseQuads(current.prop.Quads), 1.2f, Color.Blue);
                        
                        // Quad Points

                        if (_stretchingProp)
                        {
                            if (current.type == InitPropType.Rope)
                            {
                                DrawCircleV(
                                    Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopLeft, quads.BottomLeft), 
                                        new(2f, 2f)), 
                                    5f, 
                                    Color.Blue
                                );
                                
                                DrawCircleV(
                                    Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopRight, quads.BottomRight), 
                                        new(2f, 2f)), 
                                    5f, 
                                    Color.Blue
                                );
                                
                                /*DrawCircleV(quads.TopLeft, 2f, Color.Blue);
                                DrawCircleV(quads.TopRight, 2f, Color.Blue);
                                DrawCircleV(quads.BottomRight, 2f, Color.Blue);
                                DrawCircleV(quads.BottomLeft, 2f, Color.Blue);*/
                            }
                            else if (current.type == InitPropType.Long)
                            {
                                var sides = Utils.LongSides(current.prop.Quads);
                                
                                DrawCircleV(sides.left, 5f, Color.Blue);
                                DrawCircleV(sides.top, 5f, Color.Blue);
                                DrawCircleV(sides.right, 5f, Color.Blue);
                                DrawCircleV(sides.bottom, 5f, Color.Blue);
                            }
                            else
                            {
                                DrawCircleV(quads.TopLeft, 5f, Color.Blue);
                                DrawCircleV(quads.TopRight, 5f, Color.Blue);
                                DrawCircleV(quads.BottomRight, 5f, Color.Blue);
                                DrawCircleV(quads.BottomLeft, 5f, Color.Blue);
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
                                        Color.Red
                                    );
                                    break;
                                case 2:
                                    DrawLineEx(
                                        center with { Y = -10 },
                                        center with { Y = GLOBALS.Level.Height*GLOBALS.PreviewScale + 10 },
                                        2f,
                                        Color.Green
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
                                    DrawCircleV(point, 3f, Color.Red);
                                }
                            }
                            else
                            {
                                foreach (var point in current.prop.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 2f, Color.Orange);
                                }
                            }

                            if (_ropeMode)
                            {
                                // p is copied as suggested by the code editor..
                                var p1 = p;
                                var model = _models.Single(r => r.index == p1);

                                if (!model.simSwitch)
                                {
                                    foreach (var handle in model.bezierHandles) DrawCircleV(handle, 3f, Color.Green);
                                }
                            }
                        }
                    }
                }
                
            }
            #endregion
            
            // Draw the enclosing rectangle for selected props
            // DEBUG: DrawRectangleLinesEx(_selectedPropsEncloser, 3f, Color.White);

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
                                    Logger.Fatal($"failed to fetch current tile-as-prop from {nameof(_tilesAsPropsIndices)}[{_tilesAsPropsIndices.Length}]: {nameof(_propsMenuTilesCategoryIndex)} ({_propsMenuTilesCategoryIndex} was out of bounds)");
                                    throw new IndexOutOfRangeException(message: $"failed to fetch current tile-as-prop from {nameof(_tilesAsPropsIndices)}[{_tilesAsPropsIndices.Length}]: {nameof(_propsMenuTilesCategoryIndex)} ({_propsMenuTilesCategoryIndex} was out of bounds)");
                                }

                                if (_propsMenuTilesIndex >=
                                    _tilesAsPropsIndices[_propsMenuTilesCategoryIndex].Length)
                                {
                                    Logger.Fatal($"failed to fetch current tile-as-prop from {nameof(_tilesAsPropsIndices)}[{_tilesAsPropsIndices[_propsMenuTilesCategoryIndex].Length}]: {nameof(_propsMenuTilesIndex)} ({_propsMenuTilesIndex} was out of bounds)");
                                    throw new IndexOutOfRangeException(message: $"failed to fetch current tile-as-prop from {nameof(_tilesAsPropsIndices)}[{_tilesAsPropsIndices[_propsMenuTilesCategoryIndex].Length}]: {nameof(_propsMenuTilesIndex)} ({_propsMenuTilesIndex} was out of bounds)");
                                }

                                {
                                    if (currentTileAsPropCategory.index >= GLOBALS.Textures.Tiles.Length)
                                    {
                                        Logger.Fatal($"failed to fetch tile-as-prop texture from {nameof(GLOBALS.Textures.Tiles)}[{GLOBALS.Textures.Tiles.Length}]: {nameof(currentTileAsPropCategory)}.length ({currentTileAsPropCategory.index}) was outside the bounds of the array.");
                                        throw new IndexOutOfRangeException($"failed to fetch tile-as-prop texture from {nameof(GLOBALS.Textures.Tiles)}[{GLOBALS.Textures.Tiles.Length}]: {nameof(currentTileAsPropCategory)}.length ({currentTileAsPropCategory.index}) was outside the bounds of the array.");
                                    }
                                    
                                    var ind = _tilesAsPropsIndices[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];

                                    if (ind.index >= GLOBALS.Textures.Tiles[currentTileAsPropCategory.index].Length)
                                    {
                                        Logger.Fatal($"failed to fetch tile-as-prop texture from {nameof(GLOBALS.Textures.Tiles)}[{nameof(currentTileAsPropCategory)}][Length: {GLOBALS.Textures.Tiles[currentTileAsPropCategory.index].Length}]: {nameof(_tilesAsPropsIndices)}][{_propsMenuTilesCategoryIndex}][{_propsMenuTilesIndex}] ({ind.index}) was outside of the bounds of the array.");
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
                            DrawCircleV(tileMouseWorld, 3f, Color.Blue);
                            break;

                        case 2: // Current Long Prop
                        {
                            var prop = GLOBALS.LongProps[_propsMenuLongsIndex];
                            var texture = GLOBALS.Textures.LongProps[_propsMenuLongsIndex];
                            var height = texture.Height / 2f;

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
                            // consideColor.Red outdated
                            var prop = GLOBALS.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
                            var texture = GLOBALS.Textures.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
                            
                            var (width, height, settings) = prop switch
                            {
                                InitVariedStandardProp variedStandard => (variedStandard.Size.x * GLOBALS.PreviewScale / 2f, variedStandard.Size.y * GLOBALS.PreviewScale / 2f, new PropVariedSettings(variation:_defaultVariation)),
                                InitStandardProp standard => (standard.Size.x * GLOBALS.PreviewScale / 2f, standard.Size.y * GLOBALS.PreviewScale / 2f, new BasicPropSettings()),
                                InitVariedSoftProp variedSoft => (variedSoft.SizeInPixels.x  / 2f, variedSoft.SizeInPixels.y / 2f, new PropVariedSoftSettings(variation:_defaultVariation)),
                                InitSoftProp => (texture.Width  / 2f, texture.Height  / 2f, new PropSoftSettings()),
                                InitVariedDecalProp variedDecal => (variedDecal.SizeInPixels.x  / 2f, variedDecal.SizeInPixels.y / 2f, new PropVariedDecalSettings(variation:_defaultVariation)),
                                InitSimpleDecalProp => (texture.Width / 2f, texture.Height / 2f, new PropSimpleDecalSettings()), 
                                InitSoftEffectProp => (texture.Width / 2f, texture.Height / 2f, new PropSoftEffectSettings()), 
                                InitAntimatterProp => (texture.Width / 2f, texture.Height / 2f, new PropAntimatterSettings()),
                                InitLongProp => (texture.Width / 2f, texture.Height / 2f, new PropLongSettings()), 
                                InitRopeProp => (texture.Width / 2f, texture.Height / 2f, new PropRopeSettings()),
                                        
                                _ => (texture.Width / 2f, texture.Height / 2f, new BasicPropSettings())
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
                    if (IsMouseButtonDown(MouseButton.Left) && _clickTracker)
                    {
                        var mouse = GetScreenToWorld2D(GetMousePosition(), _camera);
                        var diff = Raymath.Vector2Subtract(mouse, _selection1);
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
                            Color.Blue
                        );
                    }
                    break;
            }

        }
        EndMode2D();

        #region TileEditorUI

        {
            // Selected Props
            var fetchedSelected = GLOBALS.Level.Props
                .Select((prop, index) => (prop, index))
                .Where(p => _selected[p.index])
                .Select(p => p)
                .ToArray();

            // Coordinates

            if (GLOBALS.Settings.TileEditor.HoveredTileInfo && canDrawTile)
            {
                if (inMatrixBounds)
                    DrawText(
                        $"x: {tileMatrixX}, y: {tileMatrixY}\n{GLOBALS.Level.TileMatrix[tileMatrixY, tileMatrixX, GLOBALS.Layer]}",
                        (int)tileMouse.X + previewScale,
                        (int)tileMouse.Y + previewScale,
                        15,
                        Color.White
                    );
            }
            else
            {
                if (inMatrixBounds)
                    DrawText(
                        $"x: {tileMatrixX}, y: {tileMatrixY}",
                        (int)tileMouse.X + previewScale,
                        (int)tileMouse.Y + previewScale,
                        15,
                        Color.White
                    );
            }

            // layer indicator

            var newLayer = GLOBALS.Layer;

            var layer3Hovered = GLOBALS.Layer == 2 && CheckCollisionPointRec(tileMouse, layer3Rect);

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
                var layer2Hovered = GLOBALS.Layer == 1 && CheckCollisionPointRec(tileMouse, layer2Rect);

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
                var layer1Hovered = CheckCollisionPointRec(tileMouse, layer1Rect);

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

            if (newLayer != GLOBALS.Layer)
            {
                GLOBALS.Layer = newLayer;
                UpdateDefaultDepth();
            }

            // Update prop depth render texture
            if (fetchedSelected.Length == 1)
            {
                Raylib_cs.Raylib.BeginTextureMode(GLOBALS.Textures.PropDepth);
                Raylib_cs.Raylib.ClearBackground(Raylib_cs.Color.Green);
                Printers.DrawDepthIndicator(fetchedSelected[0].prop);
                Raylib_cs.Raylib.EndTextureMode();
            }

            // Edit Mode Indicators
            if (_mode == 0) {
                 var moveTexture = GLOBALS.Textures.PropEditModes[0];
                 var rotateTexture = GLOBALS.Textures.PropEditModes[1];
                 var scaleTexture = GLOBALS.Textures.PropEditModes[2];
                 var warpTexture = GLOBALS.Textures.PropEditModes[3];
                 var editPointsTexture = GLOBALS.Textures.PropEditModes[4];

                 var moveRect = new Rectangle(135, sHeight - 50, 40, 40);
                 var rotateRect = new Rectangle(180, sHeight - 50, 40, 40);
                 var scaleRect = new Rectangle(225, sHeight - 50, 40, 40);
                 var warpRect = new Rectangle(270, sHeight - 50, 40, 40);
                 var editPointsRect = new Rectangle(315, sHeight - 50, 40, 40);

                 var rectColor = GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.Black with { A = 100 } : Color.White;

                 DrawRectangleRec(moveRect, rectColor);
                 DrawRectangleRec(rotateRect, rectColor);
                 DrawRectangleRec(scaleRect, rectColor);
                 DrawRectangleRec(warpRect, rectColor);
                 DrawRectangleRec(editPointsRect, rectColor);
                 
                 if (_movingProps) DrawRectangleRec(moveRect, Color.Blue);
                 if (_rotatingProps) DrawRectangleRec(rotateRect, Color.Blue);
                 if (_scalingProps) DrawRectangleRec(scaleRect, Color.Blue);
                 if (_stretchingProp) DrawRectangleRec(warpRect, Color.Blue);
                 if (_editingPropPoints) DrawRectangleRec(editPointsRect, Color.Blue);

                 DrawTexturePro(
                     moveTexture,
                     new Rectangle(0, 0, moveTexture.Width, moveTexture.Height),
                     moveRect,
                     new Vector2(0, 0),
                     0,
                     GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : _movingProps ? Color.White : Color.Black);

                 DrawTexturePro(
                     rotateTexture,
                     new Rectangle(0, 0, rotateTexture.Width, rotateTexture.Height),
                     rotateRect,
                     new Vector2(0, 0),
                     0,
                     GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : _rotatingProps ? Color.White : Color.Black);

                 DrawTexturePro(
                     scaleTexture,
                     new Rectangle(0, 0, scaleTexture.Width, scaleTexture.Height),
                     scaleRect,
                     new Vector2(0, 0),
                     0,
                     GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : _scalingProps ? Color.White : Color.Black);

                 DrawTexturePro(
                     warpTexture,
                     new Rectangle(0, 0, warpTexture.Width, warpTexture.Height),
                     warpRect,
                     new Vector2(0, 0),
                     0,
                     GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : _stretchingProp ? Color.White : Color.Black);

                 DrawTexturePro(
                     editPointsTexture,
                     new Rectangle(0, 0, editPointsTexture.Width, editPointsTexture.Height),
                     editPointsRect,
                     new Vector2(0, 0),
                     0,
                     GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : _editingPropPoints ? Color.White : Color.Black);
            }
            //

            rlImGui.Begin();
            
            ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);

            var menuOpened = ImGui.Begin("Props##PropsPanel");
            
            var menuPos = ImGui.GetWindowPos();
            var menuWinSpace = ImGui.GetWindowSize();

            if (CheckCollisionPointRec(tileMouse, new(menuPos.X - 5, menuPos.Y, menuWinSpace.X + 10, menuWinSpace.Y)))
            {
                _isPropsWinHovered = true;

                if (IsMouseButtonDown(MouseButton.Left)) _isPropsWinDragged = true;
            }
            else
            {
                _isPropsWinHovered = false;
            }

            if (IsMouseButtonReleased(MouseButton.Left) && _isPropsWinDragged) _isPropsWinDragged = false;
            
            if (menuOpened)
            {
                var availableSpace = ImGui.GetContentRegionAvail();

                var halfWidth = availableSpace.X / 2f;
                var halfSize = new Vector2(halfWidth, 20);
                
                ImGui.SeparatorText("Mode");
                
                if (ImGui.Selectable(
                        "Selection", 
                        _mode == 0, 
                        ImGuiSelectableFlags.None, 
                        halfSize)
                ) _mode = 0;
                
                ImGui.SameLine();
                
                if (ImGui.Selectable(
                        "Placement", 
                        _mode == 1, 
                        ImGuiSelectableFlags.None, 
                        halfSize)
                ) _mode = 1;

                var precisionSelected = ImGui.Button(
                    $"Precision: {_snapMode switch { 0 => "Free", 1 => "Grid", 2 => "Precise", _ => "?" }}",
                    availableSpace with { Y = 20 });

                if (precisionSelected) _snapMode = ++_snapMode % 3;
                
                switch (_mode)
                {
                    case 0: // Selection
                    {
                        ImGui.SeparatorText("Placed Props");
                        
                        if (ImGui.Button("Select All", availableSpace with { Y = 20 }))
                        {
                            for (var i = 0; i < _selected.Length; i++) _selected[i] = true;
                        }

                        if (ImGui.BeginListBox("Props", availableSpace with { Y = availableSpace.Y - 400 }))
                        {
                            for (var index = 0; index < GLOBALS.Level.Props.Length; index++)
                            {
                                ref var currentProp = ref GLOBALS.Level.Props[index];
                                
                                var selected = ImGui.Selectable(
                                    $"{index}. {currentProp.prop.Name}{(_hidden[index] ? " [hidden]" : "")}", 
                                    _selected[index]);
                                
                                if (selected)
                                {
                                    if (IsKeyDown(KeyboardKey.LeftControl))
                                    {
                                        _selected[index] = !_selected[index];
                                    }
                                    else if (IsKeyDown(KeyboardKey.LeftShift))
                                    {
                                        var otherSelected = Array.IndexOf(_selected, true);
                                        
                                        if (otherSelected == -1) _selected = _selected.Select((p, i) => i == index).ToArray();

                                        var first = Math.Min(otherSelected, index);
                                        var second = Math.Max(otherSelected, index);

                                        for (var i = 0; i < _selected.Length; i++)
                                        {
                                            _selected[i] = i >= first && i <= second;
                                        }
                                    }
                                    else
                                    {
                                        _selected = _selected.Select((p, i) => i == index).ToArray();
                                    }
                                }
                            }
                            
                            ImGui.EndListBox();
                        }

                        var hideSelected = ImGui.Button("Hide Selected", availableSpace with { Y = 20 });

                        if (hideSelected)
                        {
                            for (var i = 0; i < _hidden.Length; i++)
                            {
                                if (_selected[i]) _hidden[i] = !_hidden[i];
                            }
                        }

                        var deleteSelected = ImGui.Button("Delete Selected", availableSpace with { Y = 20 });

                        if (deleteSelected)
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
                        }
                        
                        ImGui.SeparatorText("Selected Prop Options");
                        
                        if (fetchedSelected.Length == 1)
                        {
                            var (selectedProp, _) = fetchedSelected[0];
                    
                            // Seed

                            var seed = selectedProp.prop.Extras.Settings.Seed;
                    
                            ImGui.SetNextItemWidth(100);
                            ImGui.InputInt("Seed", ref seed);

                            selectedProp.prop.Extras.Settings.Seed = seed;
                    
                            // Depth
                    
                            ImGui.Image(new IntPtr(GLOBALS.Textures.PropDepth.Texture.Id), new Vector2(290, 20));

                            var depth = selectedProp.prop.Depth;
                    
                            ImGui.SetNextItemWidth(100);
                            ImGui.InputInt("Depth", ref depth);
                    
                            Utils.Restrict(ref depth, -29, 0);
                    
                            selectedProp.prop.Depth = depth;
                    
                            // Variation

                            if (selectedProp.prop.Extras.Settings is IVariable v)
                            {
                                var init = GLOBALS.Props[selectedProp.position.category][selectedProp.position.index];
                                var variations = (init as IVariableInit).Variations;
                                ImGui.SetNextItemWidth(100);
                                var variation = v.Variation;
                                ImGui.InputInt("Variation", ref variation);
                                Utils.Restrict(ref variation, 0, variations -1);

                                v.Variation = variation;
                            }
                            
                            // Rope
                            
                            
                            if (fetchedSelected.Length == 1 && fetchedSelected[0].prop.type == InitPropType.Rope)
                            {
                                ImGui.SeparatorText("Rope Options");
                                
                                var modelIndex = -1;

                                for (var i = 0; i < _models.Length; i++)
                                {
                                    if (_models[i].index == fetchedSelected[0].index) modelIndex = i;
                                }

                                if (modelIndex == -1)
                                {
#if DEBUG
                                    Logger.Fatal(
                                        $"failed to fetch selected rope from {nameof(_models)}: no element with index [{fetchedSelected[0].index}] was found");
                                    throw new Exception(
                                        message:
                                        $"failed to fetch selected rope from {nameof(_models)}: no element with index [{fetchedSelected[0].index}] was found");
#else
                            goto ropeNotFound;
#endif
                                }

                                ref var currentModel = ref _models[modelIndex];

                                var oldSegmentCount = GLOBALS.Level.Props[currentModel.index].prop.Extras.RopePoints.Length;
                                var segmentCount = oldSegmentCount;
                                
                                var switchSimSelected = ImGui.Button(currentModel.simSwitch ? "Simulation" : "Bezier Path");

                                if (switchSimSelected) currentModel.simSwitch = !currentModel.simSwitch;

                                ImGui.SetNextItemWidth(100);

                                ImGui.InputInt("Segment Count", ref segmentCount);

                                // Update segment count if needed

                                if (segmentCount < 1) segmentCount = 1;

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

                                ImGui.Checkbox("Simulate Rope", ref _ropeMode);

                                if (currentModel.simSwitch) // Simulation mode
                                {
                                    var cycleFpsSelected = ImGui.Button($"{60 / _ropeSimulationFrameCut} FPS");

                                    if (cycleFpsSelected) _ropeSimulationFrameCut = ++_ropeSimulationFrameCut % 3 + 1;

                                    var release = (fetchedSelected[0].prop.prop.Extras.Settings as PropRopeSettings)
                                        .Release;

                                    var releaseClicked = ImGui.Button(release switch
                                    {
                                        PropRopeRelease.Left => "Release Left",
                                        PropRopeRelease.None => "Release None",
                                        PropRopeRelease.Right => "Release Right",
                                        _ => "Error"
                                    });

                                    if (releaseClicked)
                                    {
                                        release = (PropRopeRelease)((int)release + 1);
                                        if ((int)release > 2) release = 0;

                                        (fetchedSelected[0].prop.prop.Extras.Settings as PropRopeSettings).Release =
                                            release;
                                    }
                                }
                                else // Bezier mode
                                {
                                    var oldHandlePointNumber = currentModel.bezierHandles.Length;
                                    var handlePointNumber = oldHandlePointNumber;

                                    ImGui.SetNextItemWidth(100);
                                    ImGui.InputInt("Control Points", ref handlePointNumber);

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
                        }
                        
                    }
                        break;

                    case 1: // Placement
                    {
                        ImGui.SeparatorText("Categories");

                        var quarterSpace = availableSpace with { X = availableSpace.X / 4f, Y = 20 };

                        var tilesSelected = ImGui.Selectable("Tiles", _menuRootCategoryIndex == 0, ImGuiSelectableFlags.None, quarterSpace);
                        ImGui.SameLine();
                        var ropesSelected = ImGui.Selectable("Ropes", _menuRootCategoryIndex == 1, ImGuiSelectableFlags.None, quarterSpace);
                        ImGui.SameLine();
                        var longsSelected = ImGui.Selectable("Longs", _menuRootCategoryIndex == 2, ImGuiSelectableFlags.None, quarterSpace);
                        ImGui.SameLine();
                        var othersSelected = ImGui.Selectable("Others", _menuRootCategoryIndex == 3, ImGuiSelectableFlags.None, quarterSpace);

                        if (tilesSelected) _menuRootCategoryIndex = 0;
                        if (ropesSelected) _menuRootCategoryIndex = 1;
                        if (longsSelected) _menuRootCategoryIndex = 2;
                        if (othersSelected) _menuRootCategoryIndex = 3;

                        var listSize = new Vector2(halfWidth, availableSpace.Y - 230);
                        
                        switch (_menuRootCategoryIndex)
                        {
                            case 0: // Tiles-As-Props
                            {
                                if (ImGui.BeginListBox("##TileCategories", listSize))
                                {
                                    for (var index = 0; index < _tilesAsPropsCategoryNames.Length; index++)
                                    {
                                        var selected = ImGui.Selectable(_tilesAsPropsCategoryNames[index],
                                            index == _propsMenuTilesCategoryIndex);
                                        
                                        if (selected)
                                        {
                                            _propsMenuTilesCategoryIndex = index;
                                            Utils.Restrict(ref _propsMenuTilesIndex, 0, _tilesAsPropsNames[_propsMenuTilesCategoryIndex].Length-1);
                                        }
                                    }
                                    ImGui.EndListBox();
                                }
                                
                                ImGui.SameLine();

                                if (ImGui.BeginListBox("##Tiles", listSize))
                                {
                                    var array = _tilesAsPropsNames[_propsMenuTilesCategoryIndex];

                                    for (var index = 0; index < array.Length; index++)
                                    {
                                        var selected = ImGui.Selectable(array[index], index == _propsMenuTilesIndex);
                                        if (selected) _propsMenuTilesIndex = index;
                                    }
                                    ImGui.EndListBox();
                                }
                            }
                                break;
                            case 1: // Ropes
                            {
                                if (ImGui.BeginListBox("##Ropes", listSize))
                                {
                                    for (var index = 0; index < _ropeNames.Length; index++)
                                    {
                                        var selected = ImGui.Selectable(_ropeNames[index], index == _propsMenuRopesIndex);
                                        if (selected) _propsMenuRopesIndex = index;
                                    }
                                    ImGui.EndListBox();
                                }
                            }
                                break;
                            case 2: // Longs
                            {
                                if (ImGui.BeginListBox("##Longs", listSize))
                                {
                                    for (var index = 0; index < _longNames.Length; index++)
                                    {
                                        var selected = ImGui.Selectable(_longNames[index], index == _propsMenuLongsIndex);
                                        if (selected) _propsMenuLongsIndex = index;
                                    }
                                    ImGui.EndListBox();
                                }
                            }
                                break;
                            case 3: // Others
                            {
                                if (ImGui.BeginListBox("##OtherPropCategories", listSize))
                                {
                                    for (var index = 0; index < _otherCategoryNames.Length; index++)
                                    {
                                        var selected = ImGui.Selectable(_otherCategoryNames[index],
                                            index == _propsMenuOthersCategoryIndex);
                                        
                                        if (selected)
                                        {
                                            _propsMenuOthersCategoryIndex = index;
                                            Utils.Restrict(ref _propsMenuOthersIndex, 0, _otherNames[_propsMenuOthersCategoryIndex].Length-1);
                                        }
                                    }
                                    ImGui.EndListBox();
                                }
                                
                                ImGui.SameLine();

                                if (ImGui.BeginListBox("##OtherProps", listSize))
                                {
                                    var array = _otherNames[_propsMenuOthersCategoryIndex];

                                    for (var index = 0; index < array.Length; index++)
                                    {
                                        var selected = ImGui.Selectable(array[index], index == _propsMenuOthersIndex);
                                        if (selected) _propsMenuOthersIndex = index;
                                    }
                                    ImGui.EndListBox();
                                }
                            }
                                break;
                        }

                        ImGui.SeparatorText("Placement Options");
                        
                        // Seed

                        var seed = _defaultSeed;

                        ImGui.SetNextItemWidth(100);
                        ImGui.InputInt("Seed", ref seed);

                        _defaultSeed = seed;
                
                        // Depth
                
                        var currentTile = _tilesAsPropsIndices[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
                        var currentRope = GLOBALS.RopeProps[_propsMenuRopesIndex];
                        var currentLong = GLOBALS.LongProps[_propsMenuLongsIndex];
                        var currentOther = GLOBALS.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
                
                        var depth = _defaultDepth;
                            
                        ImGui.SetNextItemWidth(100);
                        ImGui.InputInt("Depth", ref depth);

                        Utils.Restrict(ref depth, -29, 0);

                        _defaultDepth = depth;

                        var propDepthTo = _menuRootCategoryIndex switch
                        {
                            0 => Utils.GetPropDepth(currentTile.init),
                            1 => Utils.GetPropDepth(currentRope),
                            2 => Utils.GetPropDepth(currentLong),
                            3 => Utils.GetPropDepth(currentOther),
                            _ => 0
                        };
                
                        ImGui.Text($"From {_defaultDepth} to {_defaultDepth - propDepthTo}");
                
                        // Variation

                        if (_menuRootCategoryIndex == 3 && currentOther is IVariableInit v)
                        {
                            var variations = v.Variations;
                            var variation = _defaultVariation;
                    
                            ImGui.SetNextItemWidth(100);
                            ImGui.InputInt("Variation", ref variation);
                    
                            Utils.Restrict(ref variation, 0, variations-1);

                            _defaultVariation = variation;
                        }
                    }
                        break;
                }

                ImGui.End();
            }
            
            // Navigation
                
            var navWindowRect = Printers.ImGui.NavigationWindow();

            _isNavigationWinHovered = CheckCollisionPointRec(GetMousePosition(), navWindowRect with
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
            
            // Shortcuts window
            if (GLOBALS.Settings.GeneralSettings.ShortcutWindow)
            {
                var shortcutWindowRect = Printers.ImGui.ShortcutsWindow(GLOBALS.Settings.Shortcuts.PropsEditor);

                _isShortcutsWinHovered = CheckCollisionPointRec(
                    tileMouse, 
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
        #endregion

        EndDrawing();
        #endregion
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera;
    }
}
