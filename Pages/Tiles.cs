using System.Numerics;
using System.Text;
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
    
    private int _tilePanelWidth = 400;
    private int _materialBrushRadius;

    /*
    private readonly TileGram _gram = new(30);
    private List<TileGram.IPlaceAction> _tempPlaceGroupActions = [];
    private List<TileGram.IRemoveAction> _tempRemoveGroupActions = [];*/

    public void Draw()
    {
        GLOBALS.Page = 3;

        if (GLOBALS.Settings.GlobalCamera) _camera = GLOBALS.Camera;

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
        int tileMatrixY = tileMouseWorld.Y < 0 ? -1 : (int)tileMouseWorld.Y / GLOBALS.PreviewScale;
        int tileMatrixX = tileMouseWorld.X < 0 ? -1 : (int)tileMouseWorld.X / GLOBALS.PreviewScale;

        var inMatrixBounds = tileMatrixX >= 0 && tileMatrixX < GLOBALS.Level.Width && tileMatrixY >= 0 && tileMatrixY < GLOBALS.Level.Height;

        var canDrawTile = !CheckCollisionPointRec(tileMouse, layer3Rect) &&
                          (GLOBALS.Layer != 1 || !CheckCollisionPointRec(tileMouse, layer2Rect)) &&
                          (GLOBALS.Layer != 0 || !CheckCollisionPointRec(tileMouse, layer1Rect)) && 
                          !CheckCollisionPointRec(tileMouse, tilePanelRect);
        
        var categoriesPageSize = (int)panelMenuHeight / 30;

        // TODO: fetch init only when menu indices change
        var currentTileInit = GLOBALS.Tiles[_tileCategoryIndex][_tileIndex];
        var currentMaterialInit = GLOBALS.Materials[_materialCategoryIndex][_materialIndex];


        #region TileEditorShortcuts
        
        var ctrl = IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);
        var shift = IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
        var alt = IsKeyDown(KeyboardKey.KEY_LEFT_ALT);
        
        if (_gShortcuts.ToMainPage.Check(ctrl, shift))
        {
            #if DEBUG
            _logger.Debug($"Going to page 1");
            #endif
            GLOBALS.Page = 1;
        }
        if (_gShortcuts.ToGeometryEditor.Check(ctrl, shift))
        {
            #if DEBUG
            _logger.Debug($"Going to page 2");
            #endif
            GLOBALS.Page = 2;
        }
        // if (_gShortcuts.ToTileEditor.Check(ctrl, shift)) GLOBALS.Page = 3;
        if (_gShortcuts.ToCameraEditor.Check(ctrl, shift))
        {
            #if DEBUG
            _logger.Debug($"Going to page 4");
            #endif
            GLOBALS.Page = 4;
        }
        if (_gShortcuts.ToLightEditor.Check(ctrl, shift))
        {
            #if DEBUG
            _logger.Debug($"Going to page 5");
            #endif
            GLOBALS.Page = 5;
        }

        if (_gShortcuts.ToDimensionsEditor.Check(ctrl, shift))
        {
            #if DEBUG
            _logger.Debug($"Going to page 6");
            #endif
            GLOBALS.ResizeFlag = true; GLOBALS.Page = 6;
        }
        if (_gShortcuts.ToEffectsEditor.Check(ctrl, shift))
        {
            #if DEBUG
            _logger.Debug($"Going to page 7");
            #endif
            GLOBALS.Page = 7;
        }
        if (_gShortcuts.ToPropsEditor.Check(ctrl, shift))
        {
            #if DEBUG
            _logger.Debug($"Going to page 8");
            #endif
            GLOBALS.Page = 8;
        }
        if (_gShortcuts.ToSettingsPage.Check(ctrl, shift))
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

        if (_shortcuts.MoveDown.Check(ctrl, shift, alt))
        {
            if (_materialTileSwitch)
            {
                if (_tileCategoryFocus)
                {
                    _tileCategoryIndex = ++_tileCategoryIndex % GLOBALS.TileCategories.Length;

                    if (_tileCategoryIndex % (categoriesPageSize + _tileCategoryScrollIndex) == categoriesPageSize + _tileCategoryScrollIndex - 1
                        && _tileCategoryIndex != GLOBALS.TileCategories.Length - 1)
                        _tileCategoryScrollIndex++;

                    if (_tileCategoryIndex == 0)
                    {
                        _tileCategoryScrollIndex = 0;
                    }

                    _tileIndex = 0;
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
                    _materialCategoryIndex = ++_materialCategoryIndex % GLOBALS.MaterialCategories.Length;

                    if (_materialCategoryIndex % (categoriesPageSize + _materialCategoryScrollIndex) == categoriesPageSize + _materialCategoryScrollIndex - 1
                        && _materialCategoryIndex != GLOBALS.MaterialCategories.Length - 1)
                        _materialCategoryScrollIndex++;

                    if (_materialCategoryIndex == 0)
                    {
                        _materialCategoryScrollIndex = 0;
                    }

                    _materialIndex = 0;
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

                    _tileCategoryIndex--;

                    if (_tileCategoryIndex < 0)
                    {
                        _tileCategoryIndex = GLOBALS.Tiles.Length - 1;
                    }

                    if (_tileCategoryIndex == (_tileCategoryScrollIndex + 1) && _tileCategoryIndex != 1) _tileCategoryScrollIndex--;
                    if (_tileCategoryIndex == GLOBALS.Tiles.Length - 1) _tileCategoryScrollIndex += Math.Abs(GLOBALS.Tiles.Length - categoriesPageSize);
                    _tileIndex = 0;
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
                    _materialCategoryIndex--;

                    if (_materialCategoryIndex < 0)
                    {
                        _materialCategoryIndex = GLOBALS.MaterialCategories.Length - 1;

                        if (_materialCategoryIndex == (_materialCategoryScrollIndex + 1) && _materialCategoryIndex != 1) _materialCategoryScrollIndex--;
                        if (_materialCategoryScrollIndex == GLOBALS.MaterialCategories.Length - 1) _materialCategoryScrollIndex = Math.Abs(GLOBALS.MaterialCategories.Length - categoriesPageSize);

                        _materialIndex = 0;
                    }
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

        var currentTilePreviewColor = GLOBALS.TileCategories[_tileCategoryIndex].Item2;
        var currentTileTexture = GLOBALS.Textures.Tiles[_tileCategoryIndex][_tileIndex];

        #endregion

        BeginDrawing();

        ClearBackground(new(170, 170, 170, 255));

        BeginMode2D(_camera);
        {
            DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.PreviewScale, GLOBALS.Level.Height * GLOBALS.PreviewScale, new Color(255, 255, 255, 255));

            #region Matrix

            #region TileEditorLayer3
            // Draw geos first
            if (_showTileLayer3)
            {

                Printers.DrawGeoLayer(
                    2, 
                    GLOBALS.PreviewScale, 
                    false, 
                    GLOBALS.Layer == 2
                        ? BLACK 
                        : new(0, 0, 0, 120)
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
            }
            #endregion

            #region TileEditorLayer2
            if (_showTileLayer2)
            {
                if (GLOBALS.Layer != 2) DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.PreviewScale, GLOBALS.Level.Height * GLOBALS.PreviewScale, new(90, 90, 90, 120));

                Printers.DrawGeoLayer(
                    1, 
                    GLOBALS.PreviewScale, 
                    false, 
                    GLOBALS.Layer == 1
                        ? BLACK 
                        : new(0, 0, 0, 120)
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
            }
            #endregion

            #region TileEditorLayer1
            if (_showTileLayer1)
            {
                if (GLOBALS.Layer != 1 && GLOBALS.Layer!= 2) DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.PreviewScale, GLOBALS.Level.Height * GLOBALS.PreviewScale, new(100, 100, 100, 100));

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
            }
            #endregion

            #endregion


            // currently held tile
            if (_materialTileSwitch)
            {
                var color = isTileLegal ? currentTilePreviewColor : new(255, 0, 0, 255);
                Printers.DrawTilePreview(ref currentTileInit, ref currentTileTexture, ref color, (tileMatrixX, tileMatrixY));

                EndShaderMode();
            }
            else
            {
                DrawRectangleLinesEx(
                    new(
                        (tileMatrixX - _materialBrushRadius) * GLOBALS.PreviewScale, 
                        (tileMatrixY - _materialBrushRadius) * GLOBALS.PreviewScale, (_materialBrushRadius*2+1)*GLOBALS.PreviewScale, (_materialBrushRadius*2+1)*GLOBALS.PreviewScale),
                    2f,
                    WHITE
                );
            }

            // Coordinates

            if (GLOBALS.Settings.TileEditor.HoveredTileInfo && canDrawTile)
            {
                if (inMatrixBounds) DrawText(
                    $"x: {tileMatrixX}, y: {tileMatrixY}\n{GLOBALS.Level.TileMatrix[tileMatrixY, tileMatrixX, GLOBALS.Layer]}",
                    tileMatrixX * GLOBALS.PreviewScale + GLOBALS.PreviewScale,
                    tileMatrixY * GLOBALS.PreviewScale - GLOBALS.PreviewScale,
                    15,
                    WHITE
                );
            }
            else
            {
                if (inMatrixBounds) DrawText(
                    $"x: {tileMatrixX}, y: {tileMatrixY}",
                    tileMatrixX * GLOBALS.PreviewScale + GLOBALS.PreviewScale,
                    tileMatrixY * GLOBALS.PreviewScale - GLOBALS.PreviewScale,
                    15,
                    WHITE
                );
            }
        }
        EndMode2D();

        #region TileEditorUI
        {
            // Menu

            unsafe
            {
                fixed (byte* tpt = _tilesPanelBytes)
                {
                    fixed (byte* mpt = _materialsPanelBytes)
                    {
                        RayGui.GuiPanel(
                            tilePanelRect,
                            _materialTileSwitch ? (sbyte*)tpt : (sbyte*)mpt
                        );
                    }
                }
            }

            // detect resize attempt

            if (
                tileMouse.X <= leftPanelSideStart.X + 5 && 
                tileMouse.X >= leftPanelSideStart.X - 5 && 
                tileMouse.Y >= leftPanelSideStart.Y && 
                tileMouse.Y <= leftPanelSideEnd.Y)
            {
                SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_EW);

                DrawLineEx(
                    leftPanelSideStart,
                    leftPanelSideEnd,
                    4,
                    new(0, 0, 255, 255)
                );
            }
            else
            {
                SetMouseCursor(MouseCursor.MOUSE_CURSOR_ARROW);
                DrawLineEx(
                    leftPanelSideStart,
                    leftPanelSideEnd,
                    4,
                    new(0, 0, 0, 255)
                );
            }

            // material/tile switch

            if (RayGui.GuiButton(
                new Rectangle(tilePanelRect.X + 10, tilePanelRect.Y + 30, 200, 30),
                _materialTileSwitch ? "Tiles" : "Materials"
            )) _materialTileSwitch = !_materialTileSwitch;

            if (!_materialTileSwitch)
            {
                // Default material

                var setDefault = RayGui.GuiCheckBox(new(tilePanelRect.X + 220, tilePanelRect.Y + 30, 20, 20), "Default Material", currentMaterialInit.Item1 == GLOBALS.Level.DefaultMaterial);

                if (setDefault)
                {
                    GLOBALS.Level.DefaultMaterial = currentMaterialInit.Item1;
                }
            }

            // tiles
            if (_materialTileSwitch)
            {
                // draw categories list

                int newCategoryIndex;
                
                unsafe
                {
                    fixed (int* scrollIndex = &_tileCategoryScrollIndex)
                    {
                        fixed (int* fc = &_tileCategoryItemFocus)
                        {
                            newCategoryIndex = RayGui.GuiListViewEx(
                                new Rectangle(leftPanelSideStart.X + 10, 70, _tilePanelWidth * 0.3f, panelMenuHeight),
                                _tileCategoryNames,
                                GLOBALS.TileCategories.Length,
                                fc,
                                scrollIndex,
                                _tileCategoryIndex
                            );
                        }
                    }
                }


                if (newCategoryIndex != _tileCategoryIndex && newCategoryIndex != -1)
                {
                    #if DEBUG
                    _logger.Debug($"New tile category index: {newCategoryIndex}");
                    #endif
                    _tileCategoryIndex = newCategoryIndex;
                    _tileIndex = 0;
                }

                // draw category tiles list

                unsafe
                {
                    fixed (int* scrollIndex = &_tileScrollIndex)
                    {
                        fixed (int* fc = &_tileItemFocus)
                        {
                            
                            InitTile[] category;
                            
                            #if DEBUG
                            try
                            {
                                category = GLOBALS.Tiles[_tileCategoryIndex];
                            }
                            catch (IndexOutOfRangeException ie)
                            {
                                _logger.Fatal($"Failed to fetch tile category from {nameof(GLOBALS.Tiles)} (L:{GLOBALS.Tiles.Length}): {nameof(_tileCategoryIndex)} ({_tileCategoryIndex}) was out of bounds");
                                throw new IndexOutOfRangeException(message: $"Failed to fetch tile category from {nameof(GLOBALS.Tiles)} (L:{GLOBALS.Tiles.Length}): {nameof(_tileCategoryIndex)} ({_tileCategoryIndex}) was out of bounds", innerException: ie);
                            }
                            #else
                            category = GLOBALS.Tiles[_tileCategoryIndex];
                            #endif
                            
                            // TODO: performance issue
                            var newTileIndex = RayGui.GuiListViewEx(
                                new(leftPanelSideStart.X + 15 + (_tilePanelWidth * 0.3f), 70, _tilePanelWidth * 0.7f - 25, panelMenuHeight),
                                [..category.Select(i => i.Name)],
                                category.Length,
                                fc,
                                scrollIndex,
                                _tileIndex
                            );

                            if (newTileIndex != _tileIndex && newTileIndex != -1)
                            {
                                #if DEBUG
                                _logger.Debug($"New tile index: {newTileIndex}");
                                #endif

                                _tileIndex = newTileIndex;
                            }
                        }
                    }
                }
            }
            // materials
            else
            {
                int newCategoryIndex;

                unsafe
                {
                    fixed (int* scrollIndex = &_materialCategoryScrollIndex)
                    {
                        fixed (int* fc = &_materialCategoryFocus)
                        {
                            newCategoryIndex = RayGui.GuiListViewEx(
                                new(leftPanelSideStart.X + 10, 70, _tilePanelWidth * 0.3f, panelMenuHeight),
                                _materialCategoryNames,
                                GLOBALS.MaterialCategories.Length,
                                fc,
                                scrollIndex,
                                _materialCategoryIndex
                            );
                        }
                    }
                }

                if (newCategoryIndex != _materialCategoryIndex && newCategoryIndex != -1)
                {
                    #if DEBUG
                    _logger.Debug($"New material category index : {newCategoryIndex}");
                    #endif
                    
                    _materialCategoryIndex = newCategoryIndex;
                    _materialIndex = 0;
                }

                (string, Color)[] currentMaterialsList;

                #if DEBUG
                try
                {
                    currentMaterialsList = GLOBALS.Materials[_materialCategoryIndex];
                }
                catch (IndexOutOfRangeException ie)
                {
                    _logger.Fatal($"Failed to fetch current material from {nameof(GLOBALS.Materials)} (L:{GLOBALS.Materials.Length}): {nameof(_materialCategoryIndex)} ({_materialCategoryIndex} was out of bounds");
                    throw new IndexOutOfRangeException(innerException: ie, message: $"Failed to fetch current material from {nameof(GLOBALS.Materials)} (L:{GLOBALS.Materials.Length}): {nameof(_materialCategoryIndex)} ({_materialCategoryIndex} was out of bounds");
                }
                #else
                currentMaterialsList = GLOBALS.Materials[_materialCategoryIndex];
                #endif
                unsafe
                {
                    fixed (int* scrollIndex = &_materialScrollIndex)
                    {
                        fixed (int* fc = &_materialItemFocus)
                        {
                            // TODO: performance issue
                            var newMaterialIndex = RayGui.GuiListViewEx(
                                new(leftPanelSideStart.X + 15 + (_tilePanelWidth * 0.3f), 70, _tilePanelWidth * 0.7f - 25, panelMenuHeight),
                                [..currentMaterialsList.Select(i => i.Item1)],
                                currentMaterialsList.Length,
                                fc,
                                scrollIndex,
                                _materialIndex
                            );
                            
                            if (newMaterialIndex != _materialIndex && newMaterialIndex != -1)
                            {
                                #if DEBUG
                                _logger.Debug($"New material index: {newMaterialIndex}");
                                #endif

                                _materialIndex = newMaterialIndex;
                            } 
                        }
                    }
                }


                foreach (var (index, color) in currentMaterialsList.Select(v => v.Item2).Skip(_materialScrollIndex).Take(categoriesPageSize).Select((v, i) => (i, v)))
                {
                    DrawRectangleV(
                        new(leftPanelSideStart.X + 23 + (_tilePanelWidth * 0.3f),
                        77 + (index * 26)),
                        new(15,
                        15),
                        color
                    );
                }
            }

            // focus indicator rectangles

            if (_tileCategoryFocus)
            {
                DrawRectangleLinesEx(
                    new(leftPanelSideStart.X + 10, 70, _tilePanelWidth * 0.3f, panelMenuHeight),
                    4f,
                    new(0, 0, 255, 255)
                );
            }
            else
            {
                DrawRectangleLinesEx(
                    new(leftPanelSideStart.X + 15 + (_tilePanelWidth * 0.3f), 70, _tilePanelWidth * 0.7f - 25, panelMenuHeight),
                    4f,
                    new(0, 0, 255, 255)
                );
            }

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

            // Tile specs panel

            if (_materialTileSwitch)
            {
                unsafe
                {
                    fixed (byte* pt = _tileSpecsPanelBytes)
                    {
                        RayGui.GuiPanel(
                            specsRect,
                            (sbyte*)pt
                        );
                    }
                }

                DrawRectangleRec(
                    new(
                        specsRect.X,
                        specsRect.Y + 24,
                        specsRect.width,
                        specsRect.height
                    ),
                    new(120, 120, 120, 255)
                );

                /*if (RayGui.GuiButton(
                    new(specsRect.X + 276, specsRect.Y, 24, 24),
                    _showTileSpecs ? "<" : ">"
                )) _showTileSpecs = !_showTileSpecs;*/

                {
                    // Console.WriteLine($"Category: {tileCategoryIndex}, Tile: {tileIndex}; ({GLOBALS.Tiles.Length}, {GLOBALS.Tiles[tileCategoryIndex].Length})");
                    var (tileWidth, tileHeight) = GLOBALS.Tiles[_tileCategoryIndex][_tileIndex].Size;

                    var newWholeScale = Math.Min(specsRect.width / tileWidth * 20, (specsRect.height - 30) / tileHeight * 20);
                    var newCellScale = newWholeScale / 20;

                    int[] specs;
                    int[] specs2;
                    
                    #if DEBUG
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
                    #else
                    specs = GLOBALS.Tiles[_tileCategoryIndex][_tileIndex].Specs;
                    specs2 = GLOBALS.Tiles[_tileCategoryIndex][_tileIndex].Specs2;
                    #endif

                    /*var textLength = MeasureText($"{tileWidth} x {tileHeight}", 20);

                    DrawText(
                        $"{tileWidth} x {tileHeight}",
                        (specsRect.X + specsRect.width) / 2 - textLength / 2f,
                        specsRect.Y + 50, 20, BLACK
                    );*/

                    for (var x = 0; x < tileWidth; x++)
                    {
                        for (var y = 0; y < tileHeight; y++)
                        {
                            var specsIndex = (x * tileHeight) + y;
                            var spec = specs[specsIndex];
                            var spec2 = specs2.Length > 0 ? specs2[specsIndex] : -1;
                            var specOrigin = new Vector2(
                                specsRect.X + (specsRect.width - newCellScale * tileWidth) / 2f + x * newCellScale, 
                                (int)specsRect.Y + 30 + y * newCellScale
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
                                    specsRect.X + (specsRect.width - newCellScale * tileWidth) / 2f + x * newCellScale,
                                    (int)specsRect.Y + 30 + y * newCellScale,
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
            
            /*// Layer visibility

            {
                _showTileLayer1 = RayGui.GuiCheckBox(new(tilePanelRect.X + 10, tilePanelRect.height - 30, 20, 20), "Layer 1", _showTileLayer1);
                _showTileLayer2 = RayGui.GuiCheckBox(new(tilePanelRect.X + 90, tilePanelRect.height - 30, 20, 20), "Layer 2", _showTileLayer2);
                _showTileLayer3 = RayGui.GuiCheckBox(new(tilePanelRect.X + 170, tilePanelRect.height - 30, 20, 20), "Layer 3", _showTileLayer3);

                _showLayer1Tiles = RayGui.GuiCheckBox(new(tilePanelRect.X + 10, tilePanelRect.height - 5, 20, 20), "Tiles", _showLayer1Tiles);
                _showLayer2Tiles = RayGui.GuiCheckBox(new(tilePanelRect.X + 90, tilePanelRect.height - 5, 20, 20), "Tiles", _showLayer2Tiles);
                _showLayer3Tiles = RayGui.GuiCheckBox(new(tilePanelRect.X + 170, tilePanelRect.height - 5, 20, 20), "Tiles", _showLayer3Tiles);
            }*/
        }
        #endregion

        EndDrawing();

        if (GLOBALS.Settings.GlobalCamera) GLOBALS.Camera = _camera;
    }
}