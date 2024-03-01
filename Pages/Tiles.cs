using System.Numerics;
using System.Text;
using ImGuiNET;
using rlImGui_cs;
using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class TileEditorPage(Serilog.Core.Logger logger, Camera2D? camera = null) : IPage
{
    readonly Serilog.Core.Logger _logger = logger;
    Camera2D _camera = camera ?? new() { zoom = 1.0f };

    private readonly GlobalShortcuts _gShortcuts = GLOBALS.Settings.Shortcuts.GlobalShortcuts;
    private readonly TileShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.TileEditor;
    
    private bool _materialTileSwitch;
    private int _tileCategoryIndex;
    private int _tileIndex;
    private int _materialCategoryIndex;
    private int _materialIndex;
    private bool _clickTracker;
    private bool _tileCategoryFocus;
    private int _tileCategoryScrollIndex;
    private int _tileScrollIndex;
    private int _materialCategoryScrollIndex;
    private int _materialScrollIndex;

    private bool _highlightPaths;

    private int _tileItemFocus;
    private int _tileCategoryItemFocus;
    
    private int _materialItemFocus;
    private int _materialCategoryFocus;
    
    private bool _showLayer1Tiles = true;
    private bool _showLayer2Tiles = true;
    private bool _showLayer3Tiles = true;

    private bool _showTileLayer1 = true;
    private bool _showTileLayer2 = true;
    private bool _showTileLayer3 = true;

    private readonly byte[] _tilesPanelBytes = "Tiles"u8.ToArray();
    private readonly byte[] _materialsPanelBytes = "Materials"u8.ToArray();
    private readonly byte[] _tileSpecsPanelBytes = "Tile Specs"u8.ToArray();

    private readonly string[] _tileCategoryNames = [..GLOBALS.TileCategories.Select(t => t.Item1)];
    private readonly string[] _materialCategoryNames = [..GLOBALS.MaterialCategories];

    private bool _tileSpecDisplayMode;
    
    private int _tilePanelWidth = 400;
    private int _materialBrushRadius;

    private bool _isShortcutsWinHovered;
    private bool _isShortcutsWinDragged;
    
    private bool _isTilesWinHovered;
    private bool _isTilesWinDragged;
    
    private bool _isSpecsWinHovered;
    private bool _isSpecsWinDragged;

    /*
    private readonly TileGram _gram = new(30);
    private List<TileGram.IPlaceAction> _tempPlaceGroupActions = [];
    private List<TileGram.IRemoveAction> _tempRemoveGroupActions = [];*/

    private void ToNextTileCategory(int pageSize)
    {
        _tileCategoryIndex = ++_tileCategoryIndex % GLOBALS.TileCategories.Length;

        if (_tileCategoryIndex % (pageSize + _tileCategoryScrollIndex) == pageSize + _tileCategoryScrollIndex - 1
            && _tileCategoryIndex != GLOBALS.TileCategories.Length - 1)
            _tileCategoryScrollIndex++;

        if (_tileCategoryIndex == 0)
        {
            _tileCategoryScrollIndex = 0;
        }

        _tileIndex = 0;
    }

    private void ToPreviousCategory(int pageSize)
    {
        _tileCategoryIndex--;

        if (_tileCategoryIndex < 0)
        {
            _tileCategoryIndex = GLOBALS.Tiles.Length - 1;
        }

        if (_tileCategoryIndex == (_tileCategoryScrollIndex + 1) && _tileCategoryIndex != 1) _tileCategoryScrollIndex--;
        if (_tileCategoryIndex == GLOBALS.Tiles.Length - 1) _tileCategoryScrollIndex += Math.Abs(GLOBALS.Tiles.Length - pageSize);
        _tileIndex = 0;
    }

    private void ToNextMaterialCategory(int pageSize)
    {
        _materialCategoryIndex = ++_materialCategoryIndex % GLOBALS.MaterialCategories.Length;

        if (_materialCategoryIndex % (pageSize + _materialCategoryScrollIndex) == pageSize + _materialCategoryScrollIndex - 1
            && _materialCategoryIndex != GLOBALS.MaterialCategories.Length - 1)
            _materialCategoryScrollIndex++;

        if (_materialCategoryIndex == 0)
        {
            _materialCategoryScrollIndex = 0;
        }

        _materialIndex = 0;
    }
    
    private void ToPreviousMaterialCategory(int pageSize)
    {
        _materialCategoryIndex--;

        if (_materialCategoryIndex < 0)
        {
            _materialCategoryIndex = GLOBALS.MaterialCategories.Length - 1;

            if (_materialCategoryIndex == (_materialCategoryScrollIndex + 1) && _materialCategoryIndex != 1) _materialCategoryScrollIndex--;
            if (_materialCategoryScrollIndex == GLOBALS.MaterialCategories.Length - 1) _materialCategoryScrollIndex = Math.Abs(GLOBALS.MaterialCategories.Length - pageSize);

            _materialIndex = 0;
        }
    }

    public void Draw()
    {
        GLOBALS.Page = 3;

        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera;

        var teWidth = GetScreenWidth();
        var teHeight = GetScreenHeight();

        var tileMouseWorld = GetScreenToWorld2D(GetMousePosition(), _camera);
        var tileMouse = GetMousePosition();
        
        var tilePanelRect = new Rectangle(teWidth - _tilePanelWidth, 0, _tilePanelWidth, teHeight);
        var panelMenuHeight = tilePanelRect.height - 270;
        var leftPanelSideStart = new Vector2(teWidth - _tilePanelWidth, 0);
        var leftPanelSideEnd = new Vector2(teWidth - _tilePanelWidth, teHeight);
        var specsRect = new Rectangle(teWidth - 200, teHeight - 200, 200, 200);
        
        var layer3Rect = new Rectangle(10, teHeight - 80, 40, 40);
        var layer2Rect = new Rectangle(20, teHeight - 90, 40, 40);
        var layer1Rect = new Rectangle(30, teHeight - 100, 40, 40);

        //                        v this was done to avoid rounding errors
        int tileMatrixY = tileMouseWorld.Y < 0 ? -1 : (int)tileMouseWorld.Y / GLOBALS.Scale;
        int tileMatrixX = tileMouseWorld.X < 0 ? -1 : (int)tileMouseWorld.X / GLOBALS.Scale;

        var inMatrixBounds = tileMatrixX >= 0 && tileMatrixX < GLOBALS.Level.Width && tileMatrixY >= 0 && tileMatrixY < GLOBALS.Level.Height;

        var canDrawTile = !_isSpecsWinHovered &&
                          !_isSpecsWinDragged &&
                          !_isTilesWinHovered &&
                          !_isTilesWinDragged &&
                          !_isShortcutsWinHovered &&
                          !_isShortcutsWinDragged &&
                          !CheckCollisionPointRec(tileMouse, layer3Rect) &&
                          (GLOBALS.Layer != 1 || !CheckCollisionPointRec(tileMouse, layer2Rect)) &&
                          (GLOBALS.Layer != 0 || !CheckCollisionPointRec(tileMouse, layer1Rect));
        
        var categoriesPageSize = (int)panelMenuHeight / 26;

        // TODO: fetch init only when menu indices change
        var currentTileInit = GLOBALS.Tiles[_tileCategoryIndex][_tileIndex];
        var currentMaterialInit = GLOBALS.Materials[_materialCategoryIndex][_materialIndex];


        #region TileEditorShortcuts
        
        var ctrl = IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);
        var shift = IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
        var alt = IsKeyDown(KeyboardKey.KEY_LEFT_ALT);
        
        if (_gShortcuts.ToMainPage.Check(ctrl, shift, alt))
        {
            #if DEBUG
            _logger.Debug($"Going to page 1");
            #endif
            GLOBALS.Page = 1;
        }
        if (_gShortcuts.ToGeometryEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            _logger.Debug($"Going to page 2");
            #endif
            GLOBALS.Page = 2;
        }
        // if (_gShortcuts.ToTileEditor.Check(ctrl, shift)) GLOBALS.Page = 3;
        if (_gShortcuts.ToCameraEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            _logger.Debug($"Going to page 4");
            #endif
            GLOBALS.Page = 4;
        }
        if (_gShortcuts.ToLightEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            _logger.Debug($"Going to page 5");
            #endif
            GLOBALS.Page = 5;
        }

        if (_gShortcuts.ToDimensionsEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            _logger.Debug($"Going to page 6");
            #endif
            GLOBALS.ResizeFlag = true; 
            GLOBALS.NewFlag = false; 
            GLOBALS.Page = 6;
        }
        if (_gShortcuts.ToEffectsEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            _logger.Debug($"Going to page 7");
            #endif
            GLOBALS.Page = 7;
        }
        if (_gShortcuts.ToPropsEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            _logger.Debug($"Going to page 8");
            #endif
            GLOBALS.Page = 8;
        }
        if (_gShortcuts.ToSettingsPage.Check(ctrl, shift, alt))
        {
            #if DEBUG
            _logger.Debug($"Going to page 9");
            #endif
            GLOBALS.Page = 9;
        }
        
        if (_shortcuts.PickupItem.Check(ctrl, shift, alt) && canDrawTile && inMatrixBounds)
        {
            switch (GLOBALS.Level.TileMatrix[tileMatrixY, tileMatrixX, GLOBALS.Layer].Data)
            {
                case TileMaterial:
                    var result = Utils.PickupMaterial(tileMatrixX, tileMatrixY, GLOBALS.Layer);

                    if (result is not null)
                    {
                        _materialCategoryIndex = result!.Value.category;
                        _materialIndex = result!.Value.index;
                    }

                    _materialTileSwitch = false;
                    break;
                
                case TileHead:
                case TileBody:
                    var pickedTile = Utils.PickupTile(tileMatrixX, tileMatrixY, GLOBALS.Layer);
                    (_tileCategoryIndex, _tileIndex) = pickedTile ?? (_tileCategoryIndex, _tileIndex);
                    _materialTileSwitch = pickedTile is not null;
                    break;
            }
        }

        var isTileLegal = Utils.IsTileLegal(ref currentTileInit, new(tileMatrixX, tileMatrixY));

        // handle placing tiles

        if (_shortcuts.Draw.Check(ctrl, shift, alt, true) && canDrawTile && inMatrixBounds)
        {
            // _clickTracker = true;
            if (_materialTileSwitch)
            {
                if (_shortcuts.ForcePlaceTileWithGeo.Check(ctrl, shift, alt, true))
                {
                    Utils.ForcePlaceTileWithGeo(
                        currentTileInit, 
                        _tileCategoryIndex, 
                        _tileIndex, 
                        (tileMatrixX, tileMatrixY, 
                            GLOBALS.Layer
                        )
                    );
                    
                    /*_tempPlaceGroupActions.Add(
                        new TileGram.PlaceTileAction(
                            (tileMatrixX, tileMatrixY, GLOBALS.Layer), 
                            (_tileCategoryIndex, _tileIndex), 
                            true
                        )
                    );*/
                } 
                else if (_shortcuts.ForcePlaceTileWithoutGeo.Check(ctrl, shift, alt, true))
                {
                    Utils.ForcePlaceTileWithoutGeo(
                        currentTileInit, 
                        _tileCategoryIndex, 
                        _tileIndex, 
                        (tileMatrixX, tileMatrixY, 
                            GLOBALS.Layer
                        )
                    );
                    
                    /*_tempPlaceGroupActions.Add(
                        new TileGram.PlaceTileAction(
                            (tileMatrixX, tileMatrixY, GLOBALS.Layer), 
                            (_tileCategoryIndex, _tileIndex), 
                            false
                        )
                    );*/
                }
                else
                {
                    if (isTileLegal)
                    {
                        Utils.ForcePlaceTileWithGeo(
                            currentTileInit,
                            _tileCategoryIndex,
                            _tileIndex,
                            (tileMatrixX, tileMatrixY,
                                GLOBALS.Layer
                            )
                        );
                        
                        /*_tempPlaceGroupActions.Add(
                            new TileGram.PlaceTileAction(
                                (tileMatrixX, tileMatrixY, GLOBALS.Layer), 
                                (_tileCategoryIndex, _tileIndex), 
                                false
                            )
                        );*/
                    }
                }
            }
            else
            {
                Utils.PlaceMaterial(currentMaterialInit, (tileMatrixX, tileMatrixY, GLOBALS.Layer), _materialBrushRadius);
                
                /*_tempPlaceGroupActions.Add(
                    new TileGram.PlaceMaterialAction(
                        (tileMatrixX, tileMatrixY, GLOBALS.Layer), 
                        currentMaterialInit.Item1
                    )
                );*/
            }
        }
        if ((IsMouseButtonReleased(_shortcuts.Draw.Button) || IsKeyReleased(_shortcuts.AltDraw.Key)) && _clickTracker)
        {
            /*_clickTracker = false;
            _gram.Proceed(new TileGram.PlaceGroupAction([.._tempPlaceGroupActions]));
            _tempPlaceGroupActions.Clear();*/
        }

        if (canDrawTile || _clickTracker)
        {
            // handle mouse drag
            if (_shortcuts.DragLevel.Check(ctrl, shift, alt, true))
            {
                _clickTracker = true;
                var delta = GetMouseDelta();
                delta = RayMath.Vector2Scale(delta, -1.0f / _camera.zoom);
                _camera.target = RayMath.Vector2Add(_camera.target, delta);
            }

            if (IsMouseButtonReleased(_shortcuts.DragLevel.Button)) _clickTracker = false;
            
            // handle removing tiles
            if (_shortcuts.Erase.Check(ctrl, shift, alt, true) && canDrawTile && inMatrixBounds)
            {
                var cell = GLOBALS.Level.TileMatrix[tileMatrixY, tileMatrixX, GLOBALS.Layer];

                switch (cell.Data)
                {
                    case TileHead:
                    case TileBody:
                        Utils.RemoveTile(tileMatrixX, tileMatrixY, GLOBALS.Layer);
                        break;
                    
                    case TileMaterial:
                        Utils.RemoveMaterial(tileMatrixX, tileMatrixY, GLOBALS.Layer, _materialBrushRadius);
                        break;
                }
            }

            // handle zoom
            var tileWheel = GetMouseWheelMove();
            if (tileWheel != 0)
            {
                if (IsKeyDown(_shortcuts.ResizeMaterialBrush.Key))
                {
                    _materialBrushRadius += tileWheel > 0 ? 1 : -1;
                }
                else
                {
                    var mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), _camera);
                    _camera.offset = Raylib.GetMousePosition();
                    _camera.target = mouseWorldPosition;
                    _camera.zoom += tileWheel * GLOBALS.ZoomIncrement;
                    if (_camera.zoom < GLOBALS.ZoomIncrement) _camera.zoom = GLOBALS.ZoomIncrement;
                }
            }
        }


        if (_shortcuts.CycleLayers.Check(ctrl, shift, alt))
        {
            GLOBALS.Layer++;

            if (GLOBALS.Layer > 2) GLOBALS.Layer = 0;
        }

        // handle resizing tile panel
        // TODO: remove this feature
        if (((tileMouse.X <= leftPanelSideStart.X + 5 && tileMouse.X >= leftPanelSideStart.X - 5 && tileMouse.Y >= leftPanelSideStart.Y && tileMouse.Y <= leftPanelSideEnd.Y) || _clickTracker) &&
        IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
        {
            _clickTracker = true;

            var delta = GetMouseDelta();

            if (_tilePanelWidth - (int)delta.X >= 400 && _tilePanelWidth - (int)delta.X <= 700)
            {
                _tilePanelWidth -= (int)delta.X;
            }
        }

        if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
        {
            _clickTracker = false;
        }

        // change focus

        if (_shortcuts.FocusOnTileMenu.Check(ctrl, shift, alt))
        {
            _tileCategoryFocus = false;
        }

        if (_shortcuts.FocusOnTileCategoryMenu.Check(ctrl, shift, alt))
        {
            _tileCategoryFocus = true;
        }

        // change tile category

        if (_shortcuts.MoveToNextCategory.Check(ctrl, shift, alt))
        {
            if (_materialTileSwitch) ToNextTileCategory(categoriesPageSize);
            else ToNextMaterialCategory(categoriesPageSize);
        }
        else if (_shortcuts.MoveToPreviousCategory.Check(ctrl, shift, alt))
        {
            if (_materialTileSwitch) ToPreviousCategory(categoriesPageSize);
            else ToPreviousMaterialCategory(categoriesPageSize);
        }

        if (_shortcuts.MoveDown.Check(ctrl, shift, alt))
        {
            if (_materialTileSwitch)
            {
                if (_tileCategoryFocus)
                {
                    ToNextTileCategory(categoriesPageSize);
                }
                else
                {
                    _tileIndex = ++_tileIndex % GLOBALS.Tiles[_tileCategoryIndex].Length;
                    if (
                        _tileIndex % (categoriesPageSize + _tileScrollIndex) == categoriesPageSize + _tileScrollIndex - 1 &&
                        _tileIndex != GLOBALS.Tiles[_tileCategoryIndex].Length - 1) _tileScrollIndex++;

                    if (_tileIndex == 0) _tileScrollIndex = 0;
                }
            }
            else
            {
                if (_tileCategoryFocus)
                {
                    ToNextMaterialCategory(categoriesPageSize);
                }
                else
                {
                    _materialIndex = ++_materialIndex % GLOBALS.Materials[_materialCategoryIndex].Length;

                    if (
                        _materialIndex % (categoriesPageSize + _materialScrollIndex) == categoriesPageSize + _materialScrollIndex - 1 &&
                        _materialIndex != GLOBALS.Materials[_materialCategoryIndex].Length - 1) _materialScrollIndex++;

                    if (_materialIndex == 0) _materialScrollIndex = 0;
                }
            }
        }

        if (_shortcuts.MoveUp.Check(ctrl, shift, alt))
        {
            if (_materialTileSwitch)
            {
                if (_tileCategoryFocus)
                {
                    ToPreviousCategory(categoriesPageSize);
                }
                else
                {
                    if (_tileIndex == (_tileScrollIndex) && _tileIndex != 1) _tileScrollIndex--;

                    _tileIndex--;
                    if (_tileIndex < 0) _tileIndex = GLOBALS.Tiles[_tileCategoryIndex].Length - 1;

                    if (_tileIndex == GLOBALS.Tiles[_tileCategoryIndex].Length - 1) _tileScrollIndex += GLOBALS.Tiles[_tileCategoryIndex].Length - categoriesPageSize;
                }
            }
            else
            {
                if (_tileCategoryFocus)
                {
                    ToPreviousMaterialCategory(categoriesPageSize);
                }
                else
                {
                    if (_materialIndex == (_materialScrollIndex) && _materialIndex != 1) _materialScrollIndex--;
                    _materialIndex--;
                    if (_materialIndex < 0) _materialIndex = GLOBALS.Materials[_materialCategoryIndex].Length - 1;
                    if (_materialIndex == GLOBALS.Materials[_materialCategoryIndex].Length - 1) _materialScrollIndex += GLOBALS.Materials[_materialCategoryIndex].Length - categoriesPageSize;
                }
            }
        }

        if (_shortcuts.TileMaterialSwitch.Check(ctrl, shift, alt)) _materialTileSwitch = !_materialTileSwitch;

        if (_shortcuts.HoveredItemInfo.Check(ctrl, shift, alt)) GLOBALS.Settings.TileEditor.HoveredTileInfo = !GLOBALS.Settings.TileEditor.HoveredTileInfo;

        if (_shortcuts.ToggleLayer1Tiles.Check(ctrl, shift, alt)) _showLayer1Tiles = !_showLayer1Tiles;
        if (_shortcuts.ToggleLayer2Tiles.Check(ctrl, shift, alt)) _showLayer2Tiles = !_showLayer2Tiles;
        if (_shortcuts.ToggleLayer3Tiles.Check(ctrl, shift, alt)) _showLayer3Tiles = !_showLayer3Tiles;
        if (_shortcuts.ToggleLayer1.Check(ctrl, shift, alt)) _showTileLayer1 = !_showTileLayer1;
        if (_shortcuts.ToggleLayer2.Check(ctrl, shift, alt)) _showTileLayer2 = !_showTileLayer2;
        if (_shortcuts.ToggleLayer3.Check(ctrl, shift, alt)) _showTileLayer3 = !_showTileLayer3;

        if (_shortcuts.TogglePathsView.Check(ctrl, shift, alt)) _highlightPaths = !_highlightPaths;

        var currentTilePreviewColor = GLOBALS.TileCategories[_tileCategoryIndex].Item2;
        var currentTileTexture = GLOBALS.Textures.Tiles[_tileCategoryIndex][_tileIndex];

        #endregion

        BeginDrawing();

        ClearBackground(new(170, 170, 170, 255));

        BeginMode2D(_camera);
        {
            // DrawRectangle(
            //     0, 
            //     0, 
            //     GLOBALS.Level.Width * GLOBALS.Scale, 
            //     GLOBALS.Level.Height * GLOBALS.Scale, 
            //     GLOBALS.Layer == 2 ? GRAY with { a = 100 } : WHITE
            // );

            #region Matrix

            #region TileEditorLayer3
            // Draw geos first
            if (_showTileLayer3)
            {

                Printers.DrawGeoLayer(
                    2, 
                    GLOBALS.Scale, 
                    false, 
                    BLACK
                );

                // then draw the tiles

                if (_showLayer3Tiles)
                {
                    Printers.DrawTileLayer(
                        2, 
                        GLOBALS.Scale, 
                        false, 
                        !GLOBALS.Settings.TileEditor.UseTextures,
                        GLOBALS.Settings.TileEditor.TintedTiles
                    );
                }
            }
            #endregion

            #region TileEditorLayer2
            if (_showTileLayer2)
            {
                if (GLOBALS.Layer != 2) DrawRectangle(
                    0, 
                    0, 
                    GLOBALS.Level.Width * GLOBALS.Scale, 
                    GLOBALS.Level.Height * GLOBALS.Scale, 
                    GRAY with { a = 130 });

                Printers.DrawGeoLayer(
                    1, 
                    GLOBALS.Scale, 
                    false, 
                    GLOBALS.Layer < 2
                        ? BLACK 
                        : BLACK with { a = 80 }
                );

                // Draw layer 2 tiles

                if (_showLayer2Tiles)
                {
                    Printers.DrawTileLayer(
                        1, 
                        GLOBALS.Scale, 
                        false, 
                        !GLOBALS.Settings.TileEditor.UseTextures,
                        GLOBALS.Settings.TileEditor.TintedTiles,
                        (byte)(GLOBALS.Layer < 2 ? 255 : 80)
                    );
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
                        GLOBALS.Level.Width * GLOBALS.Scale, 
                        GLOBALS.Level.Height * GLOBALS.Scale, 
                        GRAY with { a = 130 }
                    );

                Printers.DrawGeoLayer(
                    0, 
                    GLOBALS.Scale, 
                    false, 
                    GLOBALS.Layer == 0
                        ? BLACK 
                        : BLACK with { a = 80 }
                );

                // Draw layer 1 tiles

                if (_showLayer1Tiles)
                {
                    Printers.DrawTileLayer(
                        0, 
                        GLOBALS.Scale, 
                        false, 
                        !GLOBALS.Settings.TileEditor.UseTextures,
                        GLOBALS.Settings.TileEditor.TintedTiles,
                        (byte)(GLOBALS.Layer == 0 ? 255 : 80)
                    );
                }
            }
            #endregion

            if (_highlightPaths)
            {
                DrawRectangle(
                    0,
                    0,
                    GLOBALS.Level.Width * GLOBALS.Scale,
                    GLOBALS.Level.Height * GLOBALS.Scale,
                    BLACK with { a = 190 });
            }

            Printers.DrawGeoLayer(0, GLOBALS.Scale, false, WHITE, false, GLOBALS.GeoPathsFilter);
            
            #endregion


            // currently held tile
            if (_materialTileSwitch)
            {
                var color = isTileLegal ? currentTilePreviewColor : RED;
                Printers.DrawTilePreview(ref currentTileInit, ref currentTileTexture, ref color, (tileMatrixX, tileMatrixY));

                EndShaderMode();
            }
            else
            {
                DrawRectangleLinesEx(
                    new(
                        (tileMatrixX - _materialBrushRadius) * GLOBALS.Scale, 
                        (tileMatrixY - _materialBrushRadius) * GLOBALS.Scale, (_materialBrushRadius*2+1)*GLOBALS.Scale, (_materialBrushRadius*2+1)*GLOBALS.Scale),
                    2f,
                    WHITE
                );
            }

            // Coordinates

            if (GLOBALS.Settings.TileEditor.HoveredTileInfo && canDrawTile)
            {
                if (inMatrixBounds) DrawText(
                    $"x: {tileMatrixX}, y: {tileMatrixY}\n{GLOBALS.Level.TileMatrix[tileMatrixY, tileMatrixX, GLOBALS.Layer]}",
                    tileMatrixX * GLOBALS.Scale + GLOBALS.Scale,
                    tileMatrixY * GLOBALS.Scale - GLOBALS.Scale,
                    15,
                    WHITE
                );
            }
            else
            {
                /*rlImGui.Begin();

                ImGui.Begin("Window");
                ImGui.Text("TEST");
                
                
                ImGui.End();
                rlImGui.End();*/
                
                if (inMatrixBounds) DrawText(
                    $"x: {tileMatrixX}, y: {tileMatrixY}",
                    tileMatrixX * GLOBALS.Scale + GLOBALS.Scale,
                    tileMatrixY * GLOBALS.Scale - GLOBALS.Scale,
                    15,
                    WHITE
                );
            }
        }
        EndMode2D();

        #region TileEditorUI
        {
            
            // ImGui
            
            Raylib_cs.Raylib.BeginTextureMode(GLOBALS.Textures.TileSpecs);
            {
                ClearBackground(GRAY);

                if (_tileSpecDisplayMode)
                {
                    ref var texture = ref GLOBALS.Textures.Tiles[_tileCategoryIndex][_tileIndex];
                    ref var color = ref GLOBALS.TileCategories[_tileCategoryIndex].Item2;
                    
                    var newWholeScale = Math.Min(200 / currentTileInit.Size.Item1, 200 /
                        currentTileInit.Size.Item2);
                    
                    Printers.DrawTileAsPropColored(
                        ref texture, 
                        ref currentTileInit, 
                        new PropQuads(
                            new Vector2(0,0), 
                            new Vector2(currentTileInit.Size.Item1*newWholeScale, 0), 
                            new Vector2(currentTileInit.Size.Item1*newWholeScale, currentTileInit.Size.Item2*newWholeScale), 
                            new Vector2(0, currentTileInit.Size.Item2*newWholeScale)),
                        color);
                }
                else
                {
                    var (tileWidth, tileHeight) = GLOBALS.Tiles[_tileCategoryIndex][_tileIndex].Size;

                    var newWholeScale = Math.Min(200 / tileWidth * 20, 200 / tileHeight * 20);
                    var newCellScale = newWholeScale / 20;

                    int[] specs;
                    int[] specs2;
                    
                    try
                    {
                        specs = GLOBALS.Tiles[_tileCategoryIndex][_tileIndex].Specs;
                        specs2 = GLOBALS.Tiles[_tileCategoryIndex][_tileIndex].Specs2;
                    }
                    catch (IndexOutOfRangeException ie)
                    {
                        _logger.Fatal($"Failed to fetch tile specs(2) from {nameof(GLOBALS.Tiles)} (L:{GLOBALS.Tiles.Length}): {nameof(_tileCategoryIndex)} or {_tileIndex} were out of bounds");
                        throw new IndexOutOfRangeException(innerException: ie,
                            message:
                            $"Failed to fetch tile specs(2) from {nameof(GLOBALS.Tiles)} (L:{GLOBALS.Tiles.Length}): {nameof(_tileCategoryIndex)} or {_tileIndex} were out of bounds");
                    }
                    
                    for (var x = 0; x < tileWidth; x++)
                    {
                        for (var y = 0; y < tileHeight; y++)
                        {
                            var specsIndex = (x * tileHeight) + y;
                            var spec = specs[specsIndex];
                            var spec2 = specs2.Length > 0 ? specs2[specsIndex] : -1;
                            var specOrigin = new Vector2(
                                (specsRect.width - newCellScale * tileWidth) / 2f + x * newCellScale, 
                                y * newCellScale
                            );

                            if (spec is >= 0 and < 9 and not 8)
                            {
                                Printers.DrawTileSpec(
                                    spec,
                                    specOrigin,
                                    newCellScale,
                                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer1
                                );
                                
                                Printers.DrawTileSpec(
                                    spec2,
                                    specOrigin,
                                    newCellScale,
                                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer2 with { A = 100 } // this can be optimized
                                );
                            }
                        }
                    }

                    for (var x = 0; x < tileWidth; x++)
                    {
                        for (var y = 0; y < tileHeight; y++)
                        {
                            DrawRectangleLinesEx(
                                new(
                                    (specsRect.width - newCellScale * tileWidth) / 2f + x * newCellScale,
                                    y * newCellScale,
                                    newCellScale,
                                    newCellScale
                                ),
                                Math.Max(tileWidth, tileHeight) switch
                                {
                                    > 25 => 0.3f,
                                    > 10 => 0.5f,
                                    _ => 1f
                                },
                                new(255, 255, 255, 255)
                            );
                        }
                    }
                }
                
            }
            Raylib_cs.Raylib.EndTextureMode();

            rlImGui.Begin();

            if (ImGui.Begin("Tiles & Materials", ImGuiWindowFlags.NoFocusOnAppearing))
            {
                var pos = ImGui.GetWindowPos();
                var winSpace = ImGui.GetWindowSize();

                if (CheckCollisionPointRec(tileMouse, new(pos.X - 5, pos.Y, winSpace.X + 10, winSpace.Y)))
                {
                    _isTilesWinHovered = true;

                    if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) _isTilesWinDragged = true;
                }
                else
                {
                    _isTilesWinHovered = false;
                }

                if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && _isTilesWinDragged) _isTilesWinDragged = false;

                if (ImGui.Button(_materialTileSwitch ? "Switch to materials" : "Switch to tiles"))
                    _materialTileSwitch = !_materialTileSwitch;

                if (!_materialTileSwitch)
                {
                    if (currentMaterialInit.Item1 == GLOBALS.Level.DefaultMaterial)
                    {
                        ImGui.Text("Current material is the default");
                    }
                    else
                    {
                        if (ImGui.Button("Set to default material"))
                        {
                            GLOBALS.Level.DefaultMaterial = currentMaterialInit.Item1;
                        }
                    }
                }
                
                var halfWidth = ImGui.GetContentRegionAvail().X / 2f - ImGui.GetStyle().ItemSpacing.X / 2f;
                var boxHeight = ImGui.GetContentRegionAvail().Y;
                
                if (ImGui.BeginListBox("##Groups", new Vector2(halfWidth, boxHeight)))
                {
                    if (_materialTileSwitch)
                    {
                        for (var categoryIndex = 0; categoryIndex < _tileCategoryNames.Length; categoryIndex++)
                        {
                            ref var category = ref GLOBALS.TileCategories[categoryIndex];
                            
                            
                            var selected = ImGui.Selectable(
                                category.Item1, 
                                _tileCategoryIndex == categoryIndex);
                            
                            if (selected)
                            {
                                _tileCategoryIndex = categoryIndex;
                                _tileIndex = 0;
                            }
                        }
                    }
                    else
                    {
                        for (var category = 0; category < _materialCategoryNames.Length; category++)
                        {
                            var selected = ImGui.Selectable(_materialCategoryNames[category], _materialCategoryIndex == category);
                            if (selected)
                            {
                                _materialCategoryIndex = category;
                                _materialIndex = 0;
                            }
                        }
                    }
                
                    ImGui.EndListBox();
                }
                
                ImGui.SameLine();
                if (ImGui.BeginListBox("##Tiles", new Vector2(halfWidth, boxHeight)))
                {
                    if (_materialTileSwitch)
                    {
                        for (var tile = 0; tile < GLOBALS.Tiles[_tileCategoryIndex].Length; tile++)
                        {
                            var selected = ImGui.Selectable(
                                GLOBALS.Tiles[_tileCategoryIndex][tile].Name, 
                                _tileIndex == tile
                            );

                            if (selected) _tileIndex = tile;
                        }
                    }
                    else
                    {
                        for (var materialIndex = 0; materialIndex < GLOBALS.Materials[_materialCategoryIndex].Length; materialIndex++)
                        {
                            ref var material = ref GLOBALS.Materials[_materialCategoryIndex][materialIndex];
                            
                            var selected = ImGui.Selectable(
                                material.Item1,
                                _materialIndex == materialIndex
                            );

                            if (selected) _materialIndex = materialIndex;
                        }
                    }
                    ImGui.EndListBox();
                }
            }
            ImGui.End();
            
            // Tile Specs with ImGui

            if (ImGui.Begin("Specs"))
            {
                var pos = ImGui.GetWindowPos();
                var winSpace = ImGui.GetWindowSize();
                
                if (CheckCollisionPointRec(GetMousePosition(), new(pos.X - 5, pos.Y, winSpace.X + 10, winSpace.Y)))
                {
                    _isSpecsWinHovered = true;

                    if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) _isSpecsWinDragged = true;
                }
                else
                {
                    _isSpecsWinHovered = false;
                }
                
                if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && _isSpecsWinDragged) _isSpecsWinDragged = false;

                var displayClicked = ImGui.Button(_tileSpecDisplayMode ? "Texture" : "Geometry");

                if (displayClicked) _tileSpecDisplayMode = !_tileSpecDisplayMode;
                
                // rlImGui.ImageRenderTexture(GLOBALS.Textures.TileSpecs);
                rlImGui.ImageRenderTextureFit(GLOBALS.Textures.TileSpecs);
                
                // Idk where to put this
                ImGui.End();
            }
            
            rlImGui.End();

            #region LayerIndicator
            
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

            DrawRectangleLines(10, (int)layer3Rect.Y, 40, 40, GRAY);

            if (GLOBALS.Layer == 2) DrawText("3", 26, (int)layer3Rect.Y+10, 22, BLACK);
            
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

                DrawRectangleLines(20, (int)layer2Rect.Y, 40, 40, GRAY);

                if (GLOBALS.Layer == 1) DrawText("2", 35, (int)layer2Rect.Y + 10, 22, BLACK);
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
                    30, (int)layer1Rect.Y, 40, 40, GRAY);

                DrawText("1", 48, (int)layer1Rect.Y + 10, 22, BLACK);
            }

            if (newLayer != GLOBALS.Layer) GLOBALS.Layer = newLayer;
            
            #endregion
            
            // Hovered tile info text

            if (inMatrixBounds && canDrawTile)
            {

                TileCell hoveredTile;

                #if DEBUG
                try
                {
                    hoveredTile = GLOBALS.Level.TileMatrix[tileMatrixY, tileMatrixX, GLOBALS.Layer];
                }
                catch (IndexOutOfRangeException ie)
                {
                    throw new IndexOutOfRangeException(innerException: ie,
                        message:
                        $"Failed to fetch hovered tile from {nameof(GLOBALS.Level.TileMatrix)} (LX: {GLOBALS.Level.TileMatrix.GetLength(1)}, LY: {GLOBALS.Level.TileMatrix.GetLength(0)}): x, y, or z ({tileMatrixX}, {tileMatrixY}, {GLOBALS.Layer}) were out of bounds");
                }
                #else
                hoveredTile = GLOBALS.Level.TileMatrix[tileMatrixY, tileMatrixX, GLOBALS.Layer];
                #endif

                switch (hoveredTile.Data)
                {
                    case TileDefault:
                    {
                        if (GLOBALS.Font is null)
                        {
                            DrawText(
                                GLOBALS.Level.DefaultMaterial,
                                0,
                                specsRect.Y + specsRect.height - 20,
                                20,
                                WHITE
                            );
                        }
                        else
                        {
                            DrawTextEx(
                                GLOBALS.Font.Value,
                                GLOBALS.Level.DefaultMaterial,
                                new Vector2(10, specsRect.Y + specsRect.height - 30),
                                30,
                                1,
                                WHITE
                            );
                        }
                    }
                        break;

                    case TileHead h:
                    {
                        if (GLOBALS.Font is null) {
                            DrawText(
                                h.CategoryPostition is (-1, -1, _) ? $"Undefined Tile \"{h.CategoryPostition.Name}\"" : h.CategoryPostition.Name,
                                0,
                                specsRect.Y + specsRect.height - 20,
                                20,
                                WHITE
                            );
                        }
                        else
                        {
                            DrawTextEx(
                                GLOBALS.Font.Value,
                                h.CategoryPostition is (-1, -1, _) ? $"Undefined Tile \"{h.CategoryPostition.Name}\"" : h.CategoryPostition.Name,
                                new Vector2(10, specsRect.Y + specsRect.height - 30),
                                30,
                                1,
                                WHITE
                            );
                        }
                    }
                        break;

                    case TileBody b:
                    {
                        var (hx, hy, hz) = b.HeadPosition;
                        
                        try
                        {
                            var supposedHead = GLOBALS.Level.TileMatrix[hy-1, hx-1, hz-1];

                            if (GLOBALS.Font is null)
                            {
                                DrawText(
                                    supposedHead.Data is TileHead h
                                        ? (h.CategoryPostition is (-1, -1, _) ? $"Undefined Tile \"{h.CategoryPostition.Name}\"" : h.CategoryPostition.Name)
                                        : "Stray Tile Fragment",
                                    0,
                                    specsRect.Y + specsRect.height - 20,
                                    20,
                                    WHITE
                                );
                                
                            }
                            else
                            {
                                DrawTextEx(
                                    GLOBALS.Font.Value,
                                    supposedHead.Data is TileHead h
                                        ? (h.CategoryPostition is (-1, -1, _) ? $"Undefined Tile \"{h.CategoryPostition.Name}\"" : h.CategoryPostition.Name)
                                        : "Stray Tile Fragment",
                                    new Vector2(10,
                                        specsRect.Y + specsRect.height - 30),
                                    30,
                                    1,
                                    WHITE
                                );
                            }
                        
                        }
                        catch (IndexOutOfRangeException)
                        {
                            if (GLOBALS.Font is null) {
                                DrawText("Stray Tile Fragment",
                                    0,
                                    specsRect.Y + specsRect.height - 20,
                                    20,
                                    WHITE
                                );
                            }
                            else
                            {
                                DrawTextEx(
                                    GLOBALS.Font.Value,
                                    "Stray Tile Fragment",
                                    new Vector2(10,
                                        specsRect.Y + specsRect.height - 30),
                                    30,
                                    1,
                                    WHITE
                                );
                            }
                        }
                    }
                        break;
                    
                    case TileMaterial m:
                    {
                        if (GLOBALS.Font is null) {
                            DrawText(m.Name,
                                0,
                                specsRect.Y + specsRect.height - 20,
                                20,
                                WHITE
                            );
                        }
                        else
                        {
                            DrawTextEx(
                                GLOBALS.Font.Value,
                                m.Name,
                                new Vector2(10, specsRect.Y + specsRect.height - 30),
                                30,
                                1,
                                WHITE
                            );
                        }
                    }
                        break;
                }
            }
        }
        
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
        #endregion

        EndDrawing();

        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera;
    }
}