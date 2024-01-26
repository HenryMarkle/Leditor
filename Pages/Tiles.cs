using System.Numerics;
using System.Text;
using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class TileEditorPage(Serilog.Core.Logger logger) : IPage
{
    readonly Serilog.Core.Logger logger = logger;
    Camera2D tileCamera = new() { zoom = 1.0f };
    int tilePanelWidth = 390;
    bool materialTileSwitch = false;
    int tileCategoryIndex = 0;
    int tileIndex = 0;
    int materialCategoryIndex = 0;
    int materialIndex = 0;
    bool clickTracker = false;
    bool tileCategoryFocus = false;
    int tileCategoryScrollIndex = 0;
    int tileScrollIndex = 0;
    int materialCategoryScrollIndex = 0;
    int materialScrollIndex = 0;
    bool showTileSpecs = GLOBALS.Settings.TileEditor.VisibleSpecs;

    bool showLayer1Tiles = true;
    bool showLayer2Tiles = true;
    bool showLayer3Tiles = true;

    bool showTileLayer1 = true;
    bool showTileLayer2 = true;
    bool showTileLayer3 = true;

    readonly byte[] tilesPanelBytes = Encoding.ASCII.GetBytes("Tiles");
    readonly byte[] materialsPanelBytes = Encoding.ASCII.GetBytes("Materials");
    readonly byte[] tileSpecsPanelBytes = Encoding.ASCII.GetBytes("Tile Specs");

    public void Draw()
    {
        GLOBALS.Page = 3;

        var teWidth = GetScreenWidth();
        var teHeight = GetScreenHeight();

        var tileMouseWorld = GetScreenToWorld2D(GetMousePosition(), tileCamera);
        var tileMouse = GetMousePosition();
        var tilePanelRect = new Rectangle(teWidth - (tilePanelWidth + 10), 20, tilePanelWidth, teHeight - 40);
        var leftPanelSideStart = new Vector2(teWidth - (tilePanelWidth + 10), 50);
        var leftPanelSideEnd = new Vector2(teWidth - (tilePanelWidth + 10), teHeight - 50);
        var specsRect = showTileSpecs
            ? new Rectangle(0, GetScreenHeight() - 300, 300, 300)
            : new(-276, GetScreenHeight() - 300, 300, 300);

        //                        v this was done to avoid rounding errors
        int tileMatrixY = tileMouseWorld.Y < 0 ? -1 : (int)tileMouseWorld.Y / GLOBALS.PreviewScale;
        int tileMatrixX = tileMouseWorld.X < 0 ? -1 : (int)tileMouseWorld.X / GLOBALS.PreviewScale;

        bool canDrawTile = materialTileSwitch
            ? !CheckCollisionPointRec(tileMouse, tilePanelRect) && !CheckCollisionPointRec(tileMouse, specsRect)
            : !CheckCollisionPointRec(tileMouse, tilePanelRect);

        bool inMatrixBounds = tileMatrixX >= 0 && tileMatrixX < GLOBALS.Level.Width && tileMatrixY >= 0 && tileMatrixY < GLOBALS.Level.Height;

        var categoriesPageSize = (int)tilePanelRect.height / 30;

        // TODO: fetch init only when menu indices change
        var currentTileInit = GLOBALS.Tiles[tileCategoryIndex][tileIndex];
        var currentMaterialInit = GLOBALS.Materials[materialCategoryIndex][materialIndex];


        #region TileEditorShortcuts

        if (IsKeyPressed(KeyboardKey.KEY_ONE))
        {
            GLOBALS.Page = 1;
        }
        if (IsKeyPressed(KeyboardKey.KEY_TWO))
        {
            GLOBALS.Page = 2;
        }

        // if (Raylib.IsKeyPressed(KeyboardKey.KEY_THREE))
        // {
        //     page = 3;
        // }

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
        if (IsKeyPressed(KeyboardKey.KEY_EIGHT))
        {
            GLOBALS.Page = 8;
        }
        if (IsKeyPressed(KeyboardKey.KEY_NINE))
        {
            GLOBALS.Page = 9;
        }

        if (IsKeyPressed(KeyboardKey.KEY_Q) && canDrawTile && inMatrixBounds)
        {
            if (materialTileSwitch)
            {
                (tileCategoryIndex, tileIndex) = GLOBALS.Level.PickupTile(tileMatrixX, tileMatrixY, GLOBALS.Layer) ?? (tileCategoryIndex, tileIndex);
            }
            else
            {
                var result = GLOBALS.Level.PickupMaterial(tileMatrixX, tileMatrixY, GLOBALS.Layer);

                if (result is not null)
                {
                    materialCategoryIndex = result.Value.category;
                    materialIndex = result.Value.index;
                }
            }
        }

        var isTileLegel = GLOBALS.Level.IsTileLegal(ref currentTileInit, new(tileMatrixX, tileMatrixY));

        // handle placing tiles

        if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT) && canDrawTile && inMatrixBounds)
        {
            if (materialTileSwitch)
            {
                if (isTileLegel) GLOBALS.Level.ForcePlaceTileWithGeo(ref currentTileInit, tileCategoryIndex + 5, tileIndex + 1, (tileMatrixX, tileMatrixY, GLOBALS.Layer));
            }
            else
            {
                GLOBALS.Level.PlaceMaterial(currentMaterialInit, (tileMatrixX, tileMatrixY, GLOBALS.Layer));
            }
        }

        if (canDrawTile || clickTracker)
        {
            if (IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
            {
                // handle mouse drag
                if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
                {
                    clickTracker = true;
                    Vector2 delta = GetMouseDelta();
                    delta = RayMath.Vector2Scale(delta, -1.0f / tileCamera.zoom);
                    tileCamera.target = RayMath.Vector2Add(tileCamera.target, delta);
                }

                if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_RIGHT)) clickTracker = false;
            }
            else
            {
                // handle removing tiles
                if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT) && canDrawTile && inMatrixBounds)
                {
                    if (materialTileSwitch)
                    {
                        GLOBALS.Level.RemoveTile(tileMatrixX, tileMatrixY, GLOBALS.Layer);
                    }
                    else
                    {
                        GLOBALS.Level.RemoveMaterial(tileMatrixX, tileMatrixY, GLOBALS.Layer);
                    }
                }

            }

            // handle zoom
            var tileWheel = GetMouseWheelMove();
            if (tileWheel != 0)
            {
                Vector2 mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), tileCamera);
                tileCamera.offset = Raylib.GetMousePosition();
                tileCamera.target = mouseWorldPosition;
                tileCamera.zoom += tileWheel * GLOBALS.ZoomIncrement;
                if (tileCamera.zoom < GLOBALS.ZoomIncrement) tileCamera.zoom = GLOBALS.ZoomIncrement;
            }
        }


        if (IsKeyPressed(KeyboardKey.KEY_L))
        {
            GLOBALS.Layer++;

            if (GLOBALS.Layer > 2) GLOBALS.Layer = 0;
        }

        // handle resizing tile panel

        if (((tileMouse.X <= leftPanelSideStart.X + 5 && tileMouse.X >= leftPanelSideStart.X - 5 && tileMouse.Y >= leftPanelSideStart.Y && tileMouse.Y <= leftPanelSideEnd.Y) || clickTracker) &&
        IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
        {
            clickTracker = true;

            var delta = GetMouseDelta();

            if (tilePanelWidth - (int)delta.X >= 400 && tilePanelWidth - (int)delta.X <= 700)
            {
                tilePanelWidth -= (int)delta.X;
            }
        }

        if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
        {
            clickTracker = false;
        }

        // change focus

        if (IsKeyPressed(KeyboardKey.KEY_D))
        {
            tileCategoryFocus = false;
        }

        if (IsKeyPressed(KeyboardKey.KEY_A))
        {
            tileCategoryFocus = true;
        }

        // change tile category

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
        {
            if (materialTileSwitch)
            {
                if (tileCategoryFocus)
                {
                    tileCategoryIndex = ++tileCategoryIndex % GLOBALS.TileCategories.Length;

                    if (tileCategoryIndex % (categoriesPageSize + tileCategoryScrollIndex) == categoriesPageSize + tileCategoryScrollIndex - 1
                        && tileCategoryIndex != GLOBALS.TileCategories.Length - 1)
                        tileCategoryScrollIndex++;

                    if (tileCategoryIndex == 0)
                    {
                        tileCategoryScrollIndex = 0;
                    }

                    tileIndex = 0;
                }
                else
                {
                    tileIndex = ++tileIndex % GLOBALS.Tiles[tileCategoryIndex].Length;
                    if (
                        tileIndex % (categoriesPageSize + tileScrollIndex) == categoriesPageSize + tileScrollIndex - 1 &&
                        tileIndex != GLOBALS.Tiles[tileCategoryIndex].Length - 1) tileScrollIndex++;

                    if (tileIndex == 0) tileScrollIndex = 0;
                }
            }
            else
            {
                if (tileCategoryFocus)
                {
                    materialCategoryIndex = ++materialCategoryIndex % GLOBALS.MaterialCategories.Length;

                    if (materialCategoryIndex % (categoriesPageSize + materialCategoryScrollIndex) == categoriesPageSize + materialCategoryScrollIndex - 1
                        && materialCategoryIndex != GLOBALS.MaterialCategories.Length - 1)
                        materialCategoryScrollIndex++;

                    if (materialCategoryIndex == 0)
                    {
                        materialCategoryScrollIndex = 0;
                    }

                    materialIndex = 0;
                }
                else
                {
                    materialIndex = ++materialIndex % GLOBALS.Materials[materialCategoryIndex].Length;

                    if (
                        materialIndex % (categoriesPageSize + materialScrollIndex) == categoriesPageSize + materialScrollIndex - 1 &&
                        materialIndex != GLOBALS.Materials[materialCategoryIndex].Length - 1) materialScrollIndex++;

                    if (materialIndex == 0) materialScrollIndex = 0;
                }
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_W))
        {
            if (materialTileSwitch)
            {
                if (tileCategoryFocus)
                {

                    tileCategoryIndex--;

                    if (tileCategoryIndex < 0)
                    {
                        tileCategoryIndex = GLOBALS.Tiles.Length - 1;
                    }

                    if (tileCategoryIndex == (tileCategoryScrollIndex + 1) && tileCategoryIndex != 1) tileCategoryScrollIndex--;
                    if (tileCategoryIndex == GLOBALS.Tiles.Length - 1) tileCategoryScrollIndex += Math.Abs(GLOBALS.Tiles.Length - categoriesPageSize);
                    tileIndex = 0;
                }
                else
                {
                    if (tileIndex == (tileScrollIndex) && tileIndex != 1) tileScrollIndex--;

                    tileIndex--;
                    if (tileIndex < 0) tileIndex = GLOBALS.Tiles[tileCategoryIndex].Length - 1;

                    if (tileIndex == GLOBALS.Tiles[tileCategoryIndex].Length - 1) tileScrollIndex += GLOBALS.Tiles[tileCategoryIndex].Length - categoriesPageSize;
                }
            }
            else
            {
                if (tileCategoryFocus)
                {
                    materialCategoryIndex--;

                    if (materialCategoryIndex < 0)
                    {
                        materialCategoryIndex = GLOBALS.MaterialCategories.Length - 1;

                        if (materialCategoryIndex == (materialCategoryScrollIndex + 1) && materialCategoryIndex != 1) materialCategoryScrollIndex--;
                        if (materialCategoryScrollIndex == GLOBALS.MaterialCategories.Length - 1) materialCategoryScrollIndex = Math.Abs(GLOBALS.MaterialCategories.Length - categoriesPageSize);

                        materialIndex = 0;
                    }
                }
                else
                {
                    if (materialIndex == (materialScrollIndex) && materialIndex != 1) materialScrollIndex--;
                    materialIndex--;
                    if (materialIndex < 0) materialIndex = GLOBALS.Materials[materialCategoryIndex].Length - 1;
                    if (materialIndex == GLOBALS.Materials[materialCategoryIndex].Length - 1) materialScrollIndex += GLOBALS.Materials[materialCategoryIndex].Length - categoriesPageSize;
                }
            }
        }

        if (IsKeyPressed(KeyboardKey.KEY_T)) showTileSpecs = !showTileSpecs;
        if (IsKeyPressed(KeyboardKey.KEY_M)) materialTileSwitch = !materialTileSwitch;

        if (IsKeyPressed(KeyboardKey.KEY_P)) GLOBALS.Settings.TileEditor.HoveredTileInfo = !GLOBALS.Settings.TileEditor.HoveredTileInfo;

        if (IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
        {
            if (IsKeyPressed(KeyboardKey.KEY_Z)) showLayer1Tiles = !showLayer1Tiles;
            if (IsKeyPressed(KeyboardKey.KEY_X)) showLayer2Tiles = !showLayer2Tiles;
            if (IsKeyPressed(KeyboardKey.KEY_C)) showLayer3Tiles = !showLayer3Tiles;
        }
        else
        {
            if (IsKeyPressed(KeyboardKey.KEY_Z)) showTileLayer1 = !showTileLayer1;
            if (IsKeyPressed(KeyboardKey.KEY_X)) showTileLayer2 = !showTileLayer2;
            if (IsKeyPressed(KeyboardKey.KEY_C)) showTileLayer3 = !showTileLayer3;
        }


        var currentTilePreviewColor = GLOBALS.TileCategories[tileCategoryIndex].Item2;
        Texture currentTileTexture = GLOBALS.Textures.Tiles[tileCategoryIndex][tileIndex];

        #endregion

        BeginDrawing();

        ClearBackground(new(170, 170, 170, 255));

        BeginMode2D(tileCamera);
        {
            DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.PreviewScale, GLOBALS.Level.Height * GLOBALS.PreviewScale, new Color(255, 255, 255, 255));

            #region TileEditorGeoMatrix

            // Draw geos first
            if (showTileLayer3)
            {
                #region TileEditorLayer3

                for (int y = 0; y < GLOBALS.Level.Height; y++)
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
                                    new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
                                    new(0, 0),
                                    0,
                                    new(0, 0, 0, 255));
                            }
                            else
                            {

                                Raylib.DrawTexturePro(
                                    GLOBALS.Textures.GeoBlocks[texture],
                                    new(0, 0, 20, 20),
                                    new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
                                                new(0, 0),
                                                0,
                                                new(0, 0, 0, 170)
                                            );
                                        }
                                        break;
                                    case 3:     // bathive

                                        if (z == GLOBALS.Layer)
                                        {

                                            DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)],
                                                new(0, 0, 20, 20),
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
                                                new(0, 0),
                                                0,
                                                new(255, 255, 255, 255)
                                            ); // TODO: remove opacity from entrances
                                        }
                                        else
                                        {
                                            DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)],
                                                new(0, 0, 20, 20),
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
                                                new(0, 0),
                                                0,
                                                new(255, 255, 255, 170)
                                            );
                                        }
                                        break;

                                    case 11:    // crack
                                        if (z == GLOBALS.Layer)
                                        {
                                            DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, z))],
                                                new(0, 0, 20, 20),
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
                                                new(0, 0),
                                                0,
                                                new(255, 255, 255, 255)
                                            );
                                        }
                                        else
                                        {
                                            DrawTexturePro(
                                                GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, z))],
                                                new(0, 0, 20, 20),
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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

                // then draw the tiles

                if (showLayer3Tiles)
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

                                Vector2 center = new Vector2(
                                    initTile.Size.Item1 % 2 == 0 ? x * GLOBALS.PreviewScale + GLOBALS.PreviewScale : x * GLOBALS.PreviewScale + GLOBALS.PreviewScale/2f, 
                                    initTile.Size.Item2 % 2 == 0 ? y * GLOBALS.PreviewScale + GLOBALS.PreviewScale : y * GLOBALS.PreviewScale + GLOBALS.PreviewScale/2f);

                                float width = 0.4f * (initTile.Type == InitTileType.Box ? initTile.Size.Item1 : initTile.Size.Item1 + initTile.BufferTiles * 2) * GLOBALS.Scale;
                                float height = 0.4f * (initTile.Size.Item2 + (initTile.BufferTiles * 2)) * GLOBALS.Scale;
                                
                                if (GLOBALS.Settings.TileEditor.UseTextures)
                                {
                                    if (GLOBALS.Settings.TileEditor.TintedTiles)
                                    {
                                        Printers.DrawTileAsPropColored(
                                            ref tileTexture,
                                            ref initTile,
                                            ref center,
                                            [
                                                new(width, -height),
                                                new(-width, -height),
                                                new(-width, height),
                                                new(width, height),
                                                new(width, -height)
                                            ],
                                            color,
                                            20 - GLOBALS.Layer*10
                                        );
                                    }
                                    else
                                    {
                                        Printers.DrawTileAsProp(
                                            ref tileTexture,
                                            ref initTile,
                                            ref center,
                                            [
                                                new(width, -height),
                                                new(-width, -height),
                                                new(-width, height),
                                                new(width, height),
                                                new(width, -height)
                                            ]
                                        );
                                    }
                                }
                                else Printers.DrawTilePreview(ref initTile, ref tileTexture, ref color, (x, y));

                                EndShaderMode();
                            }
                            else if (tileCell.Type == TileType.Material)
                            {
                                // var materialName = ((TileMaterial)tileCell.Data).Name;
                                var origin = new Vector2(x * GLOBALS.PreviewScale + 5, y * GLOBALS.PreviewScale + 5);
                                var color = GLOBALS.Level.MaterialColors[y, x, z];

                                if (z != GLOBALS.Layer) color.a = 120;

                                if (color.r != 0 || color.g != 0 || color.b != 0)
                                {

                                    switch (GLOBALS.Level.GeoMatrix[y, x, z].Geo)
                                    {
                                        case 1:
                                            DrawRectangle(
                                                x * GLOBALS.PreviewScale + 5,
                                                y * GLOBALS.PreviewScale + 5,
                                                6,
                                                6,
                                                color
                                            );
                                            break;


                                        case 2:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X, origin.Y + GLOBALS.PreviewScale - 10),
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y + GLOBALS.PreviewScale - 10),
                                                color
                                            );
                                            break;


                                        case 3:
                                            DrawTriangle(
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y),
                                                new(origin.X, origin.Y + GLOBALS.PreviewScale - 10),
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y + GLOBALS.PreviewScale - 10),
                                                color
                                            );
                                            break;

                                        case 4:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X, origin.Y + GLOBALS.PreviewScale - 10),
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y),
                                                color
                                            );
                                            break;

                                        case 5:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y + GLOBALS.PreviewScale - 10),
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y),
                                                color
                                            );
                                            break;

                                        case 6:
                                            DrawRectangleV(
                                                origin,
                                                new(GLOBALS.PreviewScale - 10, (GLOBALS.PreviewScale - 10) / 2),
                                                color
                                            );
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            if (showTileLayer2)
            {
                #region TileEditorLayer2
                if (GLOBALS.Layer != 2) DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.PreviewScale, GLOBALS.Level.Height * GLOBALS.PreviewScale, new(90, 90, 90, 120));

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
                                    new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
                                    new(0, 0),
                                    0,
                                    new(0, 0, 0, 255));
                            }
                            else
                            {

                                Raylib.DrawTexturePro(
                                    GLOBALS.Textures.GeoBlocks[texture],
                                    new(0, 0, 20, 20),
                                    new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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
                                                stackableTextures[index], 
                                                new(0, 0, 20, 20),
                                                new(x*previewScale, y*previewScale, previewScale, previewScale),
                                                new(0, 0),
                                                0, 
                                                new(255, 255, 255, 255)
                                            );
                                        }
                                        else {
                                            Raylib.DrawTexturePro(
                                                stackableTextures[index], 
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
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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

                if (showLayer2Tiles)
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
                                
                                Vector2 center = new Vector2(
                                    initTile.Size.Item1 % 2 == 0 ? x * GLOBALS.PreviewScale + GLOBALS.PreviewScale : x * GLOBALS.PreviewScale + GLOBALS.PreviewScale/2f, 
                                    initTile.Size.Item2 % 2 == 0 ? y * GLOBALS.PreviewScale + GLOBALS.PreviewScale : y * GLOBALS.PreviewScale + GLOBALS.PreviewScale/2f);

                                float width = 0.4f * (initTile.Type == InitTileType.Box ? initTile.Size.Item1 : initTile.Size.Item1 + initTile.BufferTiles * 2) * GLOBALS.Scale;
                                float height = 0.4f * (initTile.Size.Item2 + (initTile.BufferTiles * 2)) * GLOBALS.Scale;
                                
                                if (GLOBALS.Settings.TileEditor.UseTextures)
                                {
                                    if (GLOBALS.Settings.TileEditor.TintedTiles)
                                    {
                                        Printers.DrawTileAsPropColored(
                                            ref tileTexture,
                                            ref initTile,
                                            ref center,
                                            [
                                                new(width, -height),
                                                new(-width, -height),
                                                new(-width, height),
                                                new(width, height),
                                                new(width, -height)
                                            ],
                                            new Color(color.r, color.g, (int)color.b, 255 - (GLOBALS.Layer - 1)*100),
                                            10 - (GLOBALS.Layer*10 % 10)
                                        );
                                    }
                                    else
                                    {
                                        Printers.DrawTileAsProp(
                                            ref tileTexture,
                                            ref initTile,
                                            ref center,
                                            [
                                                new(width, -height),
                                                new(-width, -height),
                                                new(-width, height),
                                                new(width, height),
                                                new(width, -height)
                                            ],
                                            255 - (GLOBALS.Layer - 1)*100
                                        );
                                    }
                                }
                                else Printers.DrawTilePreview(ref initTile, ref tileTexture, ref color, (x, y));
                            }
                            else if (tileCell.Type == TileType.Material)
                            {
                                // var materialName = ((TileMaterial)tileCell.Data).Name;
                                var origin = new Vector2(x * GLOBALS.PreviewScale + 5, y * GLOBALS.PreviewScale + 5);
                                var color = GLOBALS.Level.MaterialColors[y, x, z];

                                if (z != GLOBALS.Layer) color.a = 120;

                                if (color.r != 0 || color.g != 0 || color.b != 0)
                                {

                                    switch (GLOBALS.Level.GeoMatrix[y, x, 1].Geo)
                                    {
                                        case 1:
                                            DrawRectangle(
                                                x * GLOBALS.PreviewScale + 5,
                                                y * GLOBALS.PreviewScale + 5,
                                                6,
                                                6,
                                                color
                                            );
                                            break;


                                        case 2:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X, origin.Y + GLOBALS.PreviewScale - 10),
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y + GLOBALS.PreviewScale - 10),
                                                color
                                            );
                                            break;


                                        case 3:
                                            DrawTriangle(
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y),
                                                new(origin.X, origin.Y + GLOBALS.PreviewScale - 10),
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y + GLOBALS.PreviewScale - 10),
                                                color
                                            );
                                            break;

                                        case 4:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X, origin.Y + GLOBALS.PreviewScale - 10),
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y),
                                                color
                                            );
                                            break;

                                        case 5:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y + GLOBALS.PreviewScale - 10),
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y),
                                                color
                                            );
                                            break;

                                        case 6:
                                            DrawRectangleV(
                                                origin,
                                                new(GLOBALS.PreviewScale - 10, (GLOBALS.PreviewScale - 10) / 2),
                                                color
                                            );
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            if (showTileLayer1)
            {
                #region TileEditorLayer1
                if (GLOBALS.Layer != 1 && GLOBALS.Layer!= 2) DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.PreviewScale, GLOBALS.Level.Height * GLOBALS.PreviewScale, new(100, 100, 100, 100));

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
                                    new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
                                    new(0, 0),
                                    0,
                                    new(0, 0, 0, 255));
                            }
                            else
                            {

                                Raylib.DrawTexturePro(
                                    GLOBALS.Textures.GeoBlocks[texture],
                                    new(0, 0, 20, 20),
                                    new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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
                                                new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
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

                if (showLayer1Tiles)
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
                                
                                Vector2 center = new Vector2(
                                    initTile.Size.Item1 % 2 == 0 ? x * GLOBALS.PreviewScale + GLOBALS.PreviewScale : x * GLOBALS.PreviewScale + GLOBALS.PreviewScale/2f, 
                                    initTile.Size.Item2 % 2 == 0 ? y * GLOBALS.PreviewScale + GLOBALS.PreviewScale : y * GLOBALS.PreviewScale + GLOBALS.PreviewScale/2f);

                                float width = 0.4f * (initTile.Type == InitTileType.Box ? initTile.Size.Item1 : initTile.Size.Item1 + initTile.BufferTiles * 2) * GLOBALS.Scale;
                                float height = 0.4f * (initTile.Size.Item2 + initTile.BufferTiles*2) * GLOBALS.Scale;
                                
                                if (GLOBALS.Settings.TileEditor.UseTextures)
                                {
                                    if (GLOBALS.Settings.TileEditor.TintedTiles)
                                    {
                                        Printers.DrawTileAsPropColored(
                                            ref tileTexture,
                                            ref initTile,
                                            ref center,
                                            [
                                                new(width, -height),
                                                new(-width, -height),
                                                new(-width, height),
                                                new(width, height),
                                                new(width, -height)
                                            ],
                                            new Color(color.r, color.g, color.b, 255 - GLOBALS.Layer*100),
                                            0
                                        );
                                    }
                                    else
                                    {
                                        Printers.DrawTileAsProp(
                                            ref tileTexture,
                                            ref initTile,
                                            ref center,
                                            [
                                                new(width, -height),
                                                new(-width, -height),
                                                new(-width, height),
                                                new(width, height),
                                                new(width, -height)
                                            ],
                                            255 - GLOBALS.Layer*100
                                        );
                                    }
                                }
                                else Printers.DrawTilePreview(ref initTile, ref tileTexture, ref color, (x, y));
                            }
                            else if (tileCell.Type == TileType.Material)
                            {
                                // var materialName = ((TileMaterial)tileCell.Data).Name;
                                var origin = new Vector2(x * GLOBALS.PreviewScale + 5, y * GLOBALS.PreviewScale + 5);
                                var color = GLOBALS.Level.MaterialColors[y, x, 0];

                                if (z != GLOBALS.Layer) color.a = 120;

                                if (color.r != 0 || color.g != 0 || color.b != 0)
                                {

                                    switch (GLOBALS.Level.GeoMatrix[y, x, 0].Geo)
                                    {
                                        case 1:
                                            DrawRectangle(
                                                x * GLOBALS.PreviewScale + 5,
                                                y * GLOBALS.PreviewScale + 5,
                                                6,
                                                6,
                                                color
                                            );
                                            break;


                                        case 2:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X, origin.Y + GLOBALS.PreviewScale - 10),
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y + GLOBALS.PreviewScale - 10),
                                                color
                                            );
                                            break;


                                        case 3:
                                            DrawTriangle(
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y),
                                                new(origin.X, origin.Y + GLOBALS.PreviewScale - 10),
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y + GLOBALS.PreviewScale - 10),
                                                color
                                            );
                                            break;

                                        case 4:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X, origin.Y + GLOBALS.PreviewScale - 10),
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y),
                                                color
                                            );
                                            break;

                                        case 5:
                                            DrawTriangle(
                                                origin,
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y + GLOBALS.PreviewScale - 10),
                                                new(origin.X + GLOBALS.PreviewScale - 10, origin.Y),
                                                color
                                            );
                                            break;

                                        case 6:
                                            DrawRectangleV(
                                                origin,
                                                new(GLOBALS.PreviewScale - 10, (GLOBALS.PreviewScale - 10) / 2),
                                                color
                                            );
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            #endregion


            // currently held tile
            if (materialTileSwitch)
            {
                Color color = isTileLegel ? currentTilePreviewColor : new(255, 0, 0, 255);
                Printers.DrawTilePreview(ref currentTileInit, ref currentTileTexture, ref color, (tileMatrixX, tileMatrixY));

                EndShaderMode();
            }
            else
            {
                DrawRectangleLinesEx(
                    new(tileMatrixX * GLOBALS.PreviewScale, tileMatrixY * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale),
                    2f,
                    WHITE
                );
            }

            // Coordiantes

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
                fixed (byte* tpt = tilesPanelBytes)
                {
                    fixed (byte* mpt = materialsPanelBytes)
                    {
                        RayGui.GuiPanel(
                            tilePanelRect,
                            materialTileSwitch ? (sbyte*)tpt : (sbyte*)mpt
                        );
                    }
                }
            }


            // detect resize attempt

            if (tileMouse.X <= leftPanelSideStart.X + 5 && tileMouse.X >= leftPanelSideStart.X - 5 && tileMouse.Y >= leftPanelSideStart.Y && tileMouse.Y <= leftPanelSideEnd.Y)
            {
                Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_EW);

                Raylib.DrawLineEx(
                    leftPanelSideStart,
                    leftPanelSideEnd,
                    4,
                    new(0, 0, 255, 255)
                );
            }
            else
            {
                Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_ARROW);
                Raylib.DrawLineEx(
                    leftPanelSideStart,
                    leftPanelSideEnd,
                    4,
                    new(0, 0, 0, 255)
                );
            }

            // material/tile switch

            if (RayGui.GuiButton(
                new(tilePanelRect.X + (tilePanelRect.width - 200) / 2, tilePanelRect.Y + 30, 200, 30),
                materialTileSwitch ? "Tiles" : "Materials"
            )) materialTileSwitch = !materialTileSwitch;

            // tiles
            if (materialTileSwitch)
            {
                // draw categories list

                int newCategoryIndex;
                
                unsafe
                {
                    fixed (int* scrollIndex = &tileCategoryScrollIndex)
                    {
                        newCategoryIndex = RayGui.GuiListView(
                            new(teWidth - (tilePanelWidth + 10) + 10, 90, tilePanelWidth * 0.3f, teHeight - 200),
                            string.Join(";", GLOBALS.TileCategories.Select(t => t.Item1)),
                            scrollIndex,
                            tileCategoryIndex
                        );
                    }
                }


                if (newCategoryIndex != tileCategoryIndex)
                {
                    tileCategoryIndex = newCategoryIndex;
                    tileIndex = 0;
                }

                // draw category tiles list

                unsafe
                {
                    fixed (int* scrollIndex = &tileScrollIndex)
                    {
                        tileIndex = RayGui.GuiListView(
                            new(teWidth - (tilePanelWidth + 10) + 15 + (tilePanelWidth * 0.3f), 90, tilePanelWidth * 0.7f - 25, teHeight - 200),
                            string.Join(";", GLOBALS.Tiles[tileCategoryIndex].Select(i => i.Name)),
                            scrollIndex,
                            tileIndex
                        );
                    }
                }
            }
            // materials
            else
            {
                int newCategoryIndex;

                unsafe
                {
                    fixed (int* scrollIndex = &materialCategoryScrollIndex)
                    {
                        newCategoryIndex = RayGui.GuiListView(
                            new(teWidth - (tilePanelWidth + 10) + 10, 90, tilePanelWidth * 0.3f, teHeight - 200),
                            string.Join(";", GLOBALS.MaterialCategories),
                            scrollIndex,
                            materialCategoryIndex
                        );
                    }
                }

                if (newCategoryIndex != materialCategoryIndex)
                {
                    materialCategoryIndex = newCategoryIndex;
                    materialIndex = 0;
                }

                var currentMaterialsList = GLOBALS.Materials[materialCategoryIndex];

                unsafe
                {
                    fixed (int* scrollIndex = &materialScrollIndex)
                    {
                        materialIndex = RayGui.GuiListView(
                            new(teWidth - (tilePanelWidth + 10) + 15 + (tilePanelWidth * 0.3f), 90, tilePanelWidth * 0.7f - 25, teHeight - 200),
                            string.Join(";", currentMaterialsList.Select(i => i.Item1)),
                            scrollIndex,
                            materialIndex
                        );
                    }
                }


                foreach (var (index, color) in currentMaterialsList.Select(v => v.Item2).Skip(materialScrollIndex).Take(categoriesPageSize).Select((v, i) => (i, v)))
                {
                    DrawRectangleV(
                        new(teWidth - (tilePanelWidth + 10) + 23 + (tilePanelWidth * 0.3f),
                        97 + (index * 26)),
                        new(15,
                        15),
                        color
                    );
                }
            }

            // focus indictor rectangles

            if (tileCategoryFocus)
            {
                Raylib.DrawRectangleLinesEx(
                    new(teWidth - (tilePanelWidth + 10) + 10, 90, tilePanelWidth * 0.3f, teHeight - 200),
                    4f,
                    new(0, 0, 255, 255)
                );
            }
            else
            {
                Raylib.DrawRectangleLinesEx(
                    new(teWidth - (tilePanelWidth + 10) + 15 + (tilePanelWidth * 0.3f), 90, tilePanelWidth * 0.7f - 25, teHeight - 200),
                    4f,
                    new(0, 0, 255, 255)
                );
            }

            // layer indicator

            if (GLOBALS.Layer == 2)
            {
                DrawRectangleV(new(teWidth - 60, tilePanelRect.height - 30), new(40, 40), GLOBALS.Settings.GeometryEditor.LayerColors.Layer3);
                DrawText("L3", teWidth - 50, (int)tilePanelRect.height - 20, 20, new(255, 255, 255, 255));
            }
            if (GLOBALS.Layer == 1)
            {
                DrawRectangleV(new(teWidth - 60, tilePanelRect.height - 30), new(40, 40), GLOBALS.Settings.GeometryEditor.LayerColors.Layer2);
                DrawText("L2", teWidth - 50, (int)tilePanelRect.height - 20, 20, new(255, 255, 255, 255));
            }
            if (GLOBALS.Layer == 0)
            {
                DrawRectangleV(new(teWidth - 60, tilePanelRect.height - 30), new(40, 40), GLOBALS.Settings.GeometryEditor.LayerColors.Layer1);
                DrawText("L1", teWidth - 47, (int)tilePanelRect.height - 20, 20, new(255, 255, 255, 255));
            }


            // tile specs panel

            if (materialTileSwitch)
            {
                unsafe
                {
                    fixed (byte* pt = tileSpecsPanelBytes)
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

                if (RayGui.GuiButton(
                    new(specsRect.X + 276, specsRect.Y, 24, 24),
                    showTileSpecs ? "<" : ">"
                )) showTileSpecs = !showTileSpecs;

                {
                    // Console.WriteLine($"Category: {tileCategoryIndex}, Tile: {tileIndex}; ({GLOBALS.Tiles.Length}, {GLOBALS.Tiles[tileCategoryIndex].Length})");
                    var (tileWidth, tileHeight) = GLOBALS.Tiles[tileCategoryIndex][tileIndex].Size;

                    var newWholeScale = Math.Min(300 / tileWidth * 20, 200 / tileHeight * 20);
                    var newCellScale = newWholeScale / 20;

                    var specs = GLOBALS.Tiles[tileCategoryIndex][tileIndex].Specs;
                    var specs2 = GLOBALS.Tiles[tileCategoryIndex][tileIndex].Specs2;

                    var textLength = MeasureText($"{tileWidth} x {tileHeight}", 20);

                    if (showTileSpecs)
                    {
                        DrawText(
                            $"{tileWidth} x {tileHeight}",
                            (specsRect.X + specsRect.width) / 2 - textLength / 2,
                            specsRect.Y + 50, 20, new(0, 0, 0, 255)
                        );

                        for (int x = 0; x < tileWidth; x++)
                        {
                            for (int y = 0; y < tileHeight; y++)
                            {
                                var specsIndex = (x * tileHeight) + y;
                                var spec = specs[specsIndex];
                                var spec2 = specs2.Length > 0 ? specs2[specsIndex] : -1;
                                var specOrigin = new Vector2((300 - newCellScale * tileWidth) / 2 + x * newCellScale, (int)specsRect.Y + 100 + y * newCellScale);

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

                        for (int x = 0; x < tileWidth; x++)
                        {
                            for (int y = 0; y < tileHeight; y++)
                            {
                                DrawRectangleLinesEx(
                                    new(
                                        (300 - newCellScale * tileWidth) / 2 + x * newCellScale,
                                        (int)specsRect.Y + 100 + y * newCellScale,
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
            }

            // layer visibility

            {
                showTileLayer1 = RayGui.GuiCheckBox(new(tilePanelRect.X + 10, tilePanelRect.height - 30, 20, 20), "Layer 1", showTileLayer1);
                showTileLayer2 = RayGui.GuiCheckBox(new(tilePanelRect.X + 90, tilePanelRect.height - 30, 20, 20), "Layer 2", showTileLayer2);
                showTileLayer3 = RayGui.GuiCheckBox(new(tilePanelRect.X + 170, tilePanelRect.height - 30, 20, 20), "Layer 3", showTileLayer3);

                showLayer1Tiles = RayGui.GuiCheckBox(new(tilePanelRect.X + 10, tilePanelRect.height - 5, 20, 20), "Tiles", showLayer1Tiles);
                showLayer2Tiles = RayGui.GuiCheckBox(new(tilePanelRect.X + 90, tilePanelRect.height - 5, 20, 20), "Tiles", showLayer2Tiles);
                showLayer3Tiles = RayGui.GuiCheckBox(new(tilePanelRect.X + 170, tilePanelRect.height - 5, 20, 20), "Tiles", showLayer3Tiles);
            }
        }
        #endregion

        EndDrawing();

    }
}