using static Raylib_cs.Raylib;
using System.Numerics;

namespace Leditor;

/// <summary>
/// Functions that are called each frame; Must only be called after window initialization and in drawing mode.
/// </summary>
internal static class Printers
{
    internal static void DrawSlope(int id, int scale, Vector2 position, Color color)
    {
        switch (id)
        {
            case 2:
                DrawTriangle(
                    position,
                    new(position.X, position.Y + scale),
                    new(position.X + scale, position.Y + scale),
                    color
                );
                break;

            case 3:
                DrawTriangle(
                    new(position.X + scale, position.Y),
                    new(position.X, position.Y + scale),
                    new(position.X + scale, position.Y + scale),
                    color
                );
                break;

            case 4:
                DrawTriangle(
                    position,
                    new(position.X, position.Y + scale),
                    new(position.X + scale, position.Y),
                    color
                );
                break;
            case 5:
                DrawTriangle(
                    position,
                    new(position.X + scale, position.Y + scale),
                    new(position.X + scale, position.Y),
                    color
                );
                break;
        }
    }
    
    internal static void DrawGrid(int scale)
    {
        for (var x = 0; x < GLOBALS.Level.Width; x++)
        {
            for (var y = 0; y < GLOBALS.Level.Height; y++)
            {
                DrawRectangleLinesEx(
                    new(x * scale, y * scale, scale, scale),
                    0.4f,
                    new(255, 255, 255, 50)
                );
                        
                if (x % 2 == 0 && y % 2 == 0) DrawRectangleLinesEx(
                    new(x * scale, y * scale, scale*2, scale*2),
                    0.5f,
                    new(255, 255, 255, 51)
                );
            }
        }
    }
    
    internal static void DrawGrid(int scale, bool nested)
    {
        for (var x = 0; x < GLOBALS.Level.Width; x++)
        {
            for (var y = 0; y < GLOBALS.Level.Height; y++)
            {
                DrawRectangleLinesEx(
                    new(x * scale, y * scale, scale, scale),
                    0.4f,
                    new(255, 255, 255, 50)
                );
                        
                if (nested && x % 2 == 0 && y % 2 == 0) DrawRectangleLinesEx(
                    new(x * scale, y * scale, scale*2, scale*2),
                    0.5f,
                    new(255, 255, 255, 51)
                );
            }
        }
    }
    
    internal static void DrawGeoLayer(int layer, int scale, bool grid, Color color)
    {
        for (var y = 0; y < GLOBALS.Level.Height; y++)
        {
            for (var x = 0; x < GLOBALS.Level.Width; x++)
            {
                var cell = GLOBALS.Level.GeoMatrix[y, x, layer];
                
                DrawTileSpec(x*scale, y*scale, cell.Geo, scale, color);

                /*var texture = Utils.GetBlockIndex(cell.Geo);

                if (texture >= 0)
                {
                    var fetchedTexture = GLOBALS.Textures.GeoBlocks[texture];

                    DrawTexturePro(
                        GLOBALS.Textures.GeoBlocks[texture],
                        new(0, 0, fetchedTexture.Width, fetchedTexture.Height),
                        new Rectangle(x * scale, y * scale, scale, scale),
                        new(0, 0),
                        0,
                        color
                    );
                }*/

                if (grid) DrawRectangleLinesEx(
                    new(x * scale, y * scale, scale, scale),
                    0.4f,
                    new(255, 255, 255, 50)
                );
                
                if (grid && x % 2 == 0 && y % 2 == 0) DrawRectangleLinesEx(
                    new(x * scale, y * scale, scale*2, scale*2),
                    0.6f,
                    new(255, 255, 255, 55)
                );

                for (var s = 1; s < cell.Stackables.Length; s++)
                {
                    if (cell.Stackables[s])
                    {
                        switch (s)
                        {
                            // dump placement
                            case 1:     // ph
                            case 2:     // pv
                                var stackableTexture = GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)];

                                DrawTexturePro(
                                    stackableTexture, 
                                    new(0, 0, stackableTexture.Width, stackableTexture.Height),
                                    new(x * scale, y * scale, scale, scale), 
                                    new(0, 0), 
                                    0, 
                                    color
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
                                    var stackableTexture2 = GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)];

                                    DrawTexturePro(
                                        stackableTexture2,
                                        new(0, 0, stackableTexture2.Width, stackableTexture2.Height),
                                        new(x * scale, y*scale, scale, scale), 
                                        new(0, 0),
                                        0, 
                                        Color.White
                                    ); // TODO: remove opacity from entrances
                                break;

                            // directional placement
                            case 4:     // entrance
                                var index = Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, layer));

                                if (index is 22 or 23 or 24 or 25)
                                {
                                    GLOBALS.Level.GeoMatrix[y, x, layer].Geo = 7;
                                }

                                var t = GLOBALS.Textures.GeoStackables[index];
                                DrawTexturePro(
                                    t,
                                    new(0, 0, t.Width, t.Height),
                                    new(x*scale, y*scale, scale, scale), 
                                    new(0, 0),
                                    0, 
                                    Color.White
                                );
                                break;
                            case 11:    // crack
                                var crackTexture = GLOBALS.Textures.GeoStackables[
                                    Utils.GetStackableTextureIndex(s,
                                        Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width,
                                            GLOBALS.Level.Height, x, y, layer))];
                                DrawTexturePro(
                                    crackTexture,
                                    new(0, 0, crackTexture.Width, crackTexture.Height),
                                    new(
                                    x * scale,
                                    y * scale,scale, scale
                                    ),
                                    new(0, 0),
                                    0,
                                    Color.White
                                );
                                break;
                        }
                    }
                }
            }
        }
    }
    internal static void DrawGeoLayer(int layer, int scale, bool grid, Color color, Vector2 offsetPixels)
    {
        for (var y = 0; y < GLOBALS.Level.Height; y++)
        {
            for (var x = 0; x < GLOBALS.Level.Width; x++)
            {
                var cell = GLOBALS.Level.GeoMatrix[y, x, layer];
                
                DrawTileSpec(x*scale + offsetPixels.X, y*scale + offsetPixels.Y, cell.Geo, scale, color);

                if (grid) DrawRectangleLinesEx(
                    new(offsetPixels.X + x * scale, offsetPixels.Y + y * scale, scale, scale),
                    0.4f,
                    new(255, 255, 255, 50)
                );
                
                if (grid && x % 2 == 0 && y % 2 == 0) DrawRectangleLinesEx(
                    new(offsetPixels.X + x * scale, offsetPixels.Y + y * scale, scale*2, scale*2),
                    0.6f,
                    new(255, 255, 255, 55)
                );

                for (var s = 1; s < cell.Stackables.Length; s++)
                {
                    if (cell.Stackables[s])
                    {
                        switch (s)
                        {
                            // dump placement
                            case 1:     // ph
                            case 2:     // pv
                                var stackableTexture = GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)];

                                DrawTexturePro(
                                    stackableTexture, 
                                    new(0, 0, stackableTexture.Width, stackableTexture.Height),
                                    new(offsetPixels.X + x * scale, offsetPixels.Y + y * scale, scale, scale), 
                                    new(0, 0), 
                                    0, 
                                    color
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
                                    var stackableTexture2 = GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)];

                                    DrawTexturePro(
                                        stackableTexture2,
                                        new(0, 0, stackableTexture2.Width, stackableTexture2.Height),
                                        new(offsetPixels.X + x * scale,offsetPixels.Y + y*scale, scale, scale), 
                                        new(0, 0),
                                        0, 
                                        Color.White
                                    ); // TODO: remove opacity from entrances
                                break;

                            // directional placement
                            case 4:     // entrance
                                var index = Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, layer));

                                if (index is 22 or 23 or 24 or 25)
                                {
                                    GLOBALS.Level.GeoMatrix[y, x, layer].Geo = 7;
                                }

                                var t = GLOBALS.Textures.GeoStackables[index];
                                DrawTexturePro(
                                    t,
                                    new(0, 0, t.Width, t.Height),
                                    new(offsetPixels.X + x*scale, offsetPixels.Y + y*scale, scale, scale), 
                                    new(0, 0),
                                    0, 
                                    Color.White
                                );
                                break;
                            case 11:    // crack
                                var crackTexture = GLOBALS.Textures.GeoStackables[
                                    Utils.GetStackableTextureIndex(s,
                                        Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width,
                                            GLOBALS.Level.Height, x, y, layer))];
                                DrawTexturePro(
                                    crackTexture,
                                    new(0, 0, crackTexture.Width, crackTexture.Height),
                                    new(
                                        offsetPixels.X + x * scale,
                                        offsetPixels.Y + y * scale,scale, scale
                                    ),
                                    new(0, 0),
                                    0,
                                    Color.White
                                );
                                break;
                        }
                    }
                }
            }
        }
    }
    internal static void DrawGeoLayer(int layer, int scale, bool grid, Color color, bool[] stackableFilter)
    {
        for (var y = 0; y < GLOBALS.Level.Height; y++)
        {
            for (var x = 0; x < GLOBALS.Level.Width; x++)
            {
                var cell = GLOBALS.Level.GeoMatrix[y, x, layer];
                
                DrawTileSpec(x*scale, y*scale, cell.Geo, scale, color);

                /*var texture = Utils.GetBlockIndex(cell.Geo);

                if (texture >= 0)
                {
                    var fetchedTexture = GLOBALS.Textures.GeoBlocks[texture];

                    DrawTexturePro(
                        GLOBALS.Textures.GeoBlocks[texture],
                        new(0, 0, fetchedTexture.Width, fetchedTexture.Height),
                        new Rectangle(x * scale, y * scale, scale, scale),
                        new(0, 0),
                        0,
                        color
                    );
                }*/

                if (grid) DrawRectangleLinesEx(
                    new(x * scale, y * scale, scale, scale),
                    0.4f,
                    new(255, 255, 255, 50)
                );
                
                if (grid && x % 2 == 0 && y % 2 == 0) DrawRectangleLinesEx(
                    new(x * scale, y * scale, scale*2, scale*2),
                    0.6f,
                    new(255, 255, 255, 55)
                );

                for (var s = 1; s < cell.Stackables.Length; s++)
                {
                    if (stackableFilter[s] && cell.Stackables[s])
                    {
                        switch (s)
                        {
                            // dump placement
                            case 1:     // ph
                            case 2:     // pv
                                var stackableTexture = GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)];

                                DrawTexturePro(
                                    stackableTexture, 
                                    new(0, 0, stackableTexture.Width, stackableTexture.Height),
                                    new(x * scale, y * scale, scale, scale), 
                                    new(0, 0), 
                                    0, 
                                    color
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
                                    var stackableTexture2 = GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)];

                                    DrawTexturePro(
                                        stackableTexture2,
                                        new(0, 0, stackableTexture2.Width, stackableTexture2.Height),
                                        new(x * scale, y*scale, scale, scale), 
                                        new(0, 0),
                                        0, 
                                        Color.White
                                    ); // TODO: remove opacity from entrances
                                break;

                            // directional placement
                            case 4:     // entrance
                                var index = Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, layer));

                                if (index is 22 or 23 or 24 or 25)
                                {
                                    GLOBALS.Level.GeoMatrix[y, x, layer].Geo = 7;
                                }

                                var t = GLOBALS.Textures.GeoStackables[index];
                                DrawTexturePro(
                                    t,
                                    new(0, 0, t.Width, t.Height),
                                    new(x*scale, y*scale, scale, scale), 
                                    new(0, 0),
                                    0, 
                                    Color.White
                                );
                                break;
                            case 11:    // crack
                                var crackTexture = GLOBALS.Textures.GeoStackables[
                                    Utils.GetStackableTextureIndex(s,
                                        Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width,
                                            GLOBALS.Level.Height, x, y, layer))];
                                DrawTexturePro(
                                    crackTexture,
                                    new(0, 0, crackTexture.Width, crackTexture.Height),
                                    new(
                                    x * scale,
                                    y * scale,scale, scale
                                    ),
                                    new(0, 0),
                                    0,
                                    Color.White
                                );
                                break;
                        }
                    }
                }
            }
        }
    }
    internal static void DrawGeoLayer(int layer, int scale, bool grid, Color color, bool geos, bool stackables)
    {
        for (var y = 0; y < GLOBALS.Level.Height; y++)
        {
            for (var x = 0; x < GLOBALS.Level.Width; x++)
            {
                var cell = GLOBALS.Level.GeoMatrix[y, x, layer];

                if (!geos) goto skipGeos;
                
                DrawTileSpec(x*scale, y*scale, cell.Geo, scale, color);

                /*var texture = Utils.GetBlockIndex(cell.Geo);

                if (texture >= 0)
                {
                    DrawTexture(
                        GLOBALS.Textures.GeoBlocks[texture], 
                        x * scale, 
                        y * scale, 
                        color
                    );
                }*/

                if (grid) DrawRectangleLinesEx(
                    new(x * scale, y * scale, scale, scale),
                    0.4f,
                    new(255, 255, 255, 50)
                );
                
                if (grid && x % 2 == 0 && y % 2 == 0) DrawRectangleLinesEx(
                    new(x * scale, y * scale, scale*2, scale*2),
                    0.6f,
                    new(255, 255, 255, 55)
                );
                
                skipGeos:

                if (!stackables) continue;

                for (var s = 1; s < cell.Stackables.Length; s++)
                {
                    if (cell.Stackables[s])
                    {
                        switch (s)
                        {
                            // dump placement
                            case 1:     // ph
                            case 2:     // pv
                                var stackableTexture = GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)];

                                DrawTexturePro(
                                    stackableTexture, 
                                    new(0, 0, stackableTexture.Width, stackableTexture.Height),
                                    new(x * scale, y * scale, scale, scale), 
                                    new(0, 0), 
                                    0, 
                                    color
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
                                    var stackableTexture2 = GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)];

                                    DrawTexturePro(
                                        stackableTexture2,
                                        new(0, 0, stackableTexture2.Width, stackableTexture2.Height),
                                        new(x * scale, y*scale, scale, scale), 
                                        new(0, 0),
                                        0, 
                                        Color.White
                                    ); // TODO: remove opacity from entrances
                                break;

                            // directional placement
                            case 4:     // entrance
                                var index = Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, layer));

                                if (index is 22 or 23 or 24 or 25)
                                {
                                    GLOBALS.Level.GeoMatrix[y, x, layer].Geo = 7;
                                }

                                var t = GLOBALS.Textures.GeoStackables[index];
                                DrawTexturePro(
                                    t,
                                    new(0, 0, t.Width, t.Height),
                                    new(x*scale, y*scale, scale, scale), 
                                    new(0, 0),
                                    0, 
                                    Color.White
                                );
                                break;
                            case 11:    // crack
                                var crackTexture = GLOBALS.Textures.GeoStackables[
                                    Utils.GetStackableTextureIndex(s,
                                        Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width,
                                            GLOBALS.Level.Height, x, y, layer))];
                                DrawTexturePro(
                                    crackTexture,
                                    new(0, 0, crackTexture.Width, crackTexture.Height),
                                    new(
                                    x * scale,
                                    y * scale,scale, scale
                                    ),
                                    new(0, 0),
                                    0,
                                    Color.White
                                );
                                break;
                        }
                    }
                }
            }
        }
    }
    internal static void DrawGeoLayer(int layer, int scale, bool grid, Color color, bool geos, bool[] stackableFilter)
    {
        for (var y = 0; y < GLOBALS.Level.Height; y++)
        {
            for (var x = 0; x < GLOBALS.Level.Width; x++)
            {
                var cell = GLOBALS.Level.GeoMatrix[y, x, layer];

                if (!geos) goto skipGeos;
                
                DrawTileSpec(x*scale, y*scale, cell.Geo, scale, color);

                /*var texture = Utils.GetBlockIndex(cell.Geo);

                if (texture >= 0)
                {
                    DrawTexture(
                        GLOBALS.Textures.GeoBlocks[texture], 
                        x * scale, 
                        y * scale, 
                        color
                    );
                }*/

                if (grid) DrawRectangleLinesEx(
                    new(x * scale, y * scale, scale, scale),
                    0.4f,
                    new(255, 255, 255, 50)
                );
                
                if (grid && x % 2 == 0 && y % 2 == 0) DrawRectangleLinesEx(
                    new(x * scale, y * scale, scale*2, scale*2),
                    0.6f,
                    new(255, 255, 255, 55)
                );
                
                skipGeos:

                for (var s = 1; s < cell.Stackables.Length; s++)
                {
                    if (stackableFilter[s] && cell.Stackables[s])
                    {
                        switch (s)
                        {
                            // dump placement
                            case 1:     // ph
                            case 2:     // pv
                                var stackableTexture = GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)];

                                DrawTexturePro(
                                    stackableTexture, 
                                    new(0, 0, stackableTexture.Width, stackableTexture.Height),
                                    new(x * scale, y * scale, scale, scale), 
                                    new(0, 0), 
                                    0, 
                                    color
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
                                    var stackableTexture2 = GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)];

                                    DrawTexturePro(
                                        stackableTexture2,
                                        new(0, 0, stackableTexture2.Width, stackableTexture2.Height),
                                        new(x * scale, y*scale, scale, scale), 
                                        new(0, 0),
                                        0, 
                                        Color.White
                                    ); // TODO: remove opacity from entrances
                                break;

                            // directional placement
                            case 4:     // entrance
                                var index = Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, layer));

                                if (index is 22 or 23 or 24 or 25)
                                {
                                    GLOBALS.Level.GeoMatrix[y, x, layer].Geo = 7;
                                }

                                var t = GLOBALS.Textures.GeoStackables[index];
                                DrawTexturePro(
                                    t,
                                    new(0, 0, t.Width, t.Height),
                                    new(x*scale, y*scale, scale, scale), 
                                    new(0, 0),
                                    0, 
                                    Color.White
                                );
                                break;
                            case 11:    // crack
                                var crackTexture = GLOBALS.Textures.GeoStackables[
                                    Utils.GetStackableTextureIndex(s,
                                        Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width,
                                            GLOBALS.Level.Height, x, y, layer))];
                                DrawTexturePro(
                                    crackTexture,
                                    new(0, 0, crackTexture.Width, crackTexture.Height),
                                    new(
                                    x * scale,
                                    y * scale,scale, scale
                                    ),
                                    new(0, 0),
                                    0,
                                    Color.White
                                );
                                break;
                        }
                    }
                }
            }
        }
    }

    internal static void DrawTileLayer(int layer, int scale, bool grid, bool preview, bool tinted, byte opacity = 255, bool deepTileOpacity = true)
    {
        for (var y = 0; y < GLOBALS.Level.Height; y++)
        {
            for (var x = 0; x < GLOBALS.Level.Width; x++)
            {
                if (grid) DrawRectangleLinesEx(
                    new(x * scale, y * scale, scale, scale),
                    0.5f,
                    new(255, 255, 255, 100)
                );
                
                TileCell tileCell;

                #if DEBUG
                try
                {
                    tileCell = GLOBALS.Level.TileMatrix[y, x, layer];
                }
                catch (IndexOutOfRangeException ie)
                {
                    throw new IndexOutOfRangeException(innerException: ie, message: $"Failed to fetch tile cell from {nameof(GLOBALS.Level.TileMatrix)}[{GLOBALS.Level.TileMatrix.GetLength(0)}, {GLOBALS.Level.TileMatrix.GetLength(1)}, {GLOBALS.Level.TileMatrix.GetLength(2)}]: x, y, or z ({x}, {y}, {layer}) was out of bounds");
                }
                #else
                tileCell = GLOBALS.Level.TileMatrix[y, x, layer];
                #endif
                
                if (tileCell.Type == TileType.TileHead)
                {
                    var data = (TileHead)tileCell.Data;

                    var (category, index, _) = data.CategoryPostition;
                    var undefined = data.CategoryPostition is (-1, -1, _);

                    var tileTexture = undefined
                        ? GLOBALS.Textures.MissingTile 
                        : GLOBALS.Textures.Tiles[category][index];
                    
                    var color = undefined 
                        ? Color.Purple 
                        : GLOBALS.TileCategories[category].Item2;
                    
                    var initTile = undefined
                        ? GLOBALS.MissingTile 
                        : GLOBALS.Tiles[category][index];
                    
                    var center = new Vector2(
                        initTile.Size.Item1 % 2 == 0 ? x * scale + scale : x * scale + scale/2f, 
                        initTile.Size.Item2 % 2 == 0 ? y * scale + scale : y * scale + scale/2f);

                    var width = (scale / 20f)/2 * (initTile.Type == InitTileType.Box ? initTile.Size.Item1 : initTile.Size.Item1 + initTile.BufferTiles * 2) * 20;
                    var height = (scale / 20f)/2 * (initTile.Size.Item2 + initTile.BufferTiles*2) * 20;

                    if (undefined)
                    {
                        DrawTexturePro(
                            tileTexture, 
                            new Rectangle(0, 0, tileTexture.Width, tileTexture.Height),
                            new(x*scale, y*scale, scale, scale),
                            new Vector2(0, 0),
                            0,
                            Color.White with { A = opacity }
                        );
                    }
                    else
                    {
                        if (!preview)
                        {
                            if (tinted)
                            {
                                DrawTileAsPropColored(
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
                                    new Color(color.R, color.G, color.B, deepTileOpacity && layer == GLOBALS.Layer - 1 && initTile.Specs2.Length > 0 ? 255 : opacity),
                                    0
                                );
                            }
                            else
                            {
                                DrawTileAsProp(
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
                                    deepTileOpacity && layer == GLOBALS.Layer - 1 && initTile.Specs2.Length > 0 ? 255 : opacity
                                );
                            }
                        }
                        else DrawTilePreview(initTile, tileTexture, color with { A = (byte)(deepTileOpacity &&
                            layer == GLOBALS.Layer - 1 && initTile.Specs2.Length > 0
                                ? 255
                                : opacity) }, new Vector2(x, y), scale);
                    }
                }
                else if (tileCell.Type == TileType.TileBody)
                {
                    var missingTexture = GLOBALS.Textures.MissingTile;
                    
                    var (hx, hy, hz) = ((TileBody)tileCell.Data).HeadPosition;

                    var supposedHead = GLOBALS.Level.TileMatrix[hy - 1, hx - 1, hz - 1];

                    if (supposedHead.Data is TileHead { CategoryPostition: (-1, -1, _) } or not TileHead)
                    {
                        DrawTexturePro(
                            GLOBALS.Textures.MissingTile, 
                            new Rectangle(0, 0, missingTexture.Width, missingTexture.Height),
                            new Rectangle(x*scale, y*scale, scale, scale),
                            new(0, 0),
                            0,
                            Color.White
                        );
                    }
                }
                else if (tileCell.Type == TileType.Material)
                {
                    // var materialName = ((TileMaterial)tileCell.Data).Name;
                    var origin = new Vector2(x * scale + 5, y * scale + 5);
                    var color = GLOBALS.Level.MaterialColors[y, x, layer];

                    color.A = opacity;

                    if (color.R != 0 || color.G != 0 || color.B != 0)
                    {

                        switch (GLOBALS.Level.GeoMatrix[y, x, layer].Geo)
                        {
                            case 1:
                                DrawRectangle(
                                    x * scale + 6,
                                    y * scale + 6,
                                    scale - 12,
                                    scale - 12,
                                    color
                                );
                                break;


                            case 2:
                                DrawTriangle(
                                    origin,
                                    new(origin.X, origin.Y + scale - 10),
                                    new(origin.X + scale - 10, origin.Y + scale - 10),
                                    color
                                );
                                break;


                            case 3:
                                DrawTriangle(
                                    new(origin.X + scale - 10, origin.Y),
                                    new(origin.X, origin.Y + scale - 10),
                                    new(origin.X + scale - 10, origin.Y + scale - 10),
                                    color
                                );
                                break;

                            case 4:
                                DrawTriangle(
                                    origin,
                                    new(origin.X, origin.Y + scale - 10),
                                    new(origin.X + scale - 10, origin.Y),
                                    color
                                );
                                break;

                            case 5:
                                DrawTriangle(
                                    origin,
                                    new(origin.X + scale - 10, origin.Y + scale - 10),
                                    new(origin.X + scale - 10, origin.Y),
                                    color
                                );
                                break;

                            case 6:
                                DrawRectangleV(
                                    origin,
                                    new(scale - 10, (scale - 10) / 2),
                                    color
                                );
                                break;
                        }
                    }
                }
            }
        }
    }
    internal static void DrawTileLayer(int layer, int scale, bool grid, bool preview, bool tinted, Vector2 offsetPixels)
    {
        for (var y = 0; y < GLOBALS.Level.Height; y++)
        {
            for (var x = 0; x < GLOBALS.Level.Width; x++)
            {
                if (grid) DrawRectangleLinesEx(
                    new(offsetPixels.X + x * scale, offsetPixels.Y + y * scale, scale, scale),
                    0.5f,
                    new(255, 255, 255, 100)
                );
                
                TileCell tileCell;

                #if DEBUG
                try
                {
                    tileCell = GLOBALS.Level.TileMatrix[y, x, layer];
                }
                catch (IndexOutOfRangeException ie)
                {
                    throw new IndexOutOfRangeException(innerException: ie, message: $"Failed to fetch tile cell from {nameof(GLOBALS.Level.TileMatrix)}[{GLOBALS.Level.TileMatrix.GetLength(0)}, {GLOBALS.Level.TileMatrix.GetLength(1)}, {GLOBALS.Level.TileMatrix.GetLength(2)}]: x, y, or z ({x}, {y}, {layer}) was out of bounds");
                }
                #else
                tileCell = GLOBALS.Level.TileMatrix[y, x, layer];
                #endif
                
                if (tileCell.Type == TileType.TileHead)
                {
                    var data = (TileHead)tileCell.Data;

                    var (category, index, _) = data.CategoryPostition;
                    var undefined = data.CategoryPostition is (-1, -1, _);

                    var tileTexture = undefined
                        ? GLOBALS.Textures.MissingTile 
                        : GLOBALS.Textures.Tiles[category][index];
                    
                    var color = undefined 
                        ? Color.Purple 
                        : GLOBALS.TileCategories[category].Item2;
                    
                    var initTile = undefined
                        ? GLOBALS.MissingTile 
                        : GLOBALS.Tiles[category][index];
                    
                    var center = new Vector2(
                        initTile.Size.Item1 % 2 == 0 ? x * scale + scale : x * scale + scale/2f, 
                        initTile.Size.Item2 % 2 == 0 ? y * scale + scale : y * scale + scale/2f);

                    center += offsetPixels;

                    var width = (scale / 20f)/2 * (initTile.Type == InitTileType.Box ? initTile.Size.Item1 : initTile.Size.Item1 + initTile.BufferTiles * 2) * 20;
                    var height = (scale / 20f)/2 * (initTile.Size.Item2 + initTile.BufferTiles*2) * 20;
                    
                    if (undefined)
                    {
                        DrawTexturePro(
                            tileTexture, 
                            new Rectangle(0, 0, tileTexture.Width, tileTexture.Height),
                            new(x*scale, y*scale, scale, scale),
                            new Vector2(0, 0),
                            0,
                            Color.White
                        );
                    }
                    else if (tileCell.Data is TileBody body)
                    {
                        var missingTexture = GLOBALS.Textures.MissingTile;
                    
                        var (hx, hy, hz) = body.HeadPosition;

                        var supposedHead = GLOBALS.Level.TileMatrix[hy - 1, hx - 1, hz - 1];

                        if (supposedHead.Data is TileHead { CategoryPostition: (-1, -1, _) } or not TileHead)
                        {
                            DrawTexturePro(
                                GLOBALS.Textures.MissingTile, 
                                new Rectangle(0, 0, missingTexture.Width, missingTexture.Height),
                                new Rectangle(x*scale, y*scale, scale, scale),
                                new(0, 0),
                                0,
                                Color.White
                            );
                        }
                    }
                    else
                    {
                        if (!preview)
                        {
                            if (tinted)
                            {
                                DrawTileAsPropColored(
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
                                    new Color(color.R, color.G, color.B, 255 - GLOBALS.Layer*100),
                                    0
                                );
                            }
                            else
                            {
                                DrawTileAsProp(
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
                        else DrawTilePreview(initTile, tileTexture, color, new Vector2(x, y)+offsetPixels/scale, scale);
                    }
                }
                else if (tileCell.Type == TileType.Material)
                {
                    // var materialName = ((TileMaterial)tileCell.Data).Name;
                    var origin = new Vector2(x * scale + 5, y * scale + 5);
                    var color = GLOBALS.Level.MaterialColors[y, x, layer];

                    if (layer != GLOBALS.Layer) color.A = 120;

                    if (color.R != 0 || color.G != 0 || color.B != 0)
                    {

                        switch (GLOBALS.Level.GeoMatrix[y, x, layer].Geo)
                        {
                            case 1:
                                DrawRectangle(
                                    (int)offsetPixels.X + x * scale + 5,
                                    (int)offsetPixels.Y + y * scale + 5,
                                    scale - 12,
                                    scale - 12,
                                    color
                                );
                                break;


                            case 2:
                                DrawTriangle(
                                    origin + offsetPixels,
                                    new Vector2(origin.X, origin.Y + scale - 10) + offsetPixels,
                                    new Vector2(origin.X + scale - 10, origin.Y + scale - 10) + offsetPixels,
                                    color
                                );
                                break;


                            case 3:
                                DrawTriangle(
                                    new Vector2(origin.X + scale - 10, origin.Y) + offsetPixels,
                                    new Vector2(origin.X, origin.Y + scale - 10) + offsetPixels,
                                    new Vector2(origin.X + scale - 10, origin.Y + scale - 10) + offsetPixels,
                                    color
                                );
                                break;

                            case 4:
                                DrawTriangle(
                                    origin + offsetPixels,
                                    new Vector2(origin.X, origin.Y + scale - 10) + offsetPixels,
                                    new Vector2(origin.X + scale - 10, origin.Y) + offsetPixels,
                                    color
                                );
                                break;

                            case 5:
                                DrawTriangle(
                                    origin + offsetPixels,
                                    new Vector2(origin.X + scale - 10, origin.Y + scale - 10) + offsetPixels,
                                    new Vector2(origin.X + scale - 10, origin.Y) + offsetPixels,
                                    color
                                );
                                break;

                            case 6:
                                DrawRectangleV(
                                    origin + offsetPixels,
                                    new(scale - 10, (scale - 10) / 2f),
                                    color
                                );
                                break;
                        }
                    }
                }
            }
        }
    }

    internal static void DrawTexturePoly(
        Texture2D texture, 
        Vector2 center, 
        ReadOnlySpan<Vector2> points, 
        ReadOnlySpan<Vector2> texCoords, 
        int pointCount, 
        Color tint)
    {
        Rlgl.SetTexture(texture.Id);

        // Texturing is only supported on RL_QUADS
        Rlgl.Begin(0x0007);

        Rlgl.Color4ub(tint.R, tint.G, tint.B, tint.A);

        for (int i = 0; i < pointCount - 1; i++)
        {
            Rlgl.TexCoord2f(0.5f, 0.5f);
            Rlgl.Vertex2f(center.X, center.Y);

            Rlgl.TexCoord2f(texCoords[i].X, texCoords[i].Y);
            Rlgl.Vertex2f(points[i].X + center.X, points[i].Y + center.Y);

            Rlgl.TexCoord2f(texCoords[i + 1].X, texCoords[i + 1].Y);
            Rlgl.Vertex2f(points[i + 1].X + center.X, points[i + 1].Y + center.Y);

            Rlgl.TexCoord2f(texCoords[i + 1].X, texCoords[i + 1].Y);
            Rlgl.Vertex2f(points[i + 1].X + center.X, points[i + 1].Y + center.Y);
        }
        Rlgl.End();

        Rlgl.SetTexture(0);
    }
    
    internal static void DrawTexturePoly(
        Texture2D texture, 
        Vector2 center, 
        Span<Vector2> points, 
        Span<Vector2> texCoords, 
        int pointCount, 
        Color tint)
    {
        Rlgl.SetTexture(texture.Id);

        // Texturing is only supported on RL_QUADS
        Rlgl.Begin(0x0007);

        Rlgl.Color4ub(tint.R, tint.G, tint.B, tint.A);

        for (int i = 0; i < pointCount - 1; i++)
        {
            Rlgl.TexCoord2f(0.5f, 0.5f);
            Rlgl.Vertex2f(center.X, center.Y);

            Rlgl.TexCoord2f(texCoords[i].X, texCoords[i].Y);
            Rlgl.Vertex2f(points[i].X + center.X, points[i].Y + center.Y);

            Rlgl.TexCoord2f(texCoords[i + 1].X, texCoords[i + 1].Y);
            Rlgl.Vertex2f(points[i + 1].X + center.X, points[i + 1].Y + center.Y);

            Rlgl.TexCoord2f(texCoords[i + 1].X, texCoords[i + 1].Y);
            Rlgl.Vertex2f(points[i + 1].X + center.X, points[i + 1].Y + center.Y);
        }
        Rlgl.End();

        Rlgl.SetTexture(0);
    }
    
    internal static void DrawTextureQuads(
        in Texture2D texture, 
        in PropQuads quads
    )
    {
        Rlgl.SetTexture(texture.Id);

        Rlgl.Begin(0x0007);
        Rlgl.Color4ub(Color.White.R, Color.White.G, Color.White.B, Color.White.A);

        Rlgl.TexCoord2f(1.0f, 0.0f);
        Rlgl.Vertex2f(quads.TopRight.X, quads.TopRight.Y);
        
        Rlgl.TexCoord2f(0.0f, 0.0f);
        Rlgl.Vertex2f(quads.TopLeft.X, quads.TopLeft.Y);
        
        Rlgl.TexCoord2f(0.0f, 1.0f);
        Rlgl.Vertex2f(quads.BottomLeft.X, quads.BottomLeft.Y);
        
        Rlgl.TexCoord2f(1.0f, 1.0f);
        Rlgl.Vertex2f(quads.BottomRight.X, quads.BottomRight.Y);
        
        Rlgl.TexCoord2f(1.0f, 0.0f);
        Rlgl.Vertex2f(quads.TopRight.X, quads.TopRight.Y);
        Rlgl.End();

        Rlgl.SetTexture(0);
    }
    
    internal static void DrawTextureQuads(
        in Texture2D texture, 
        in PropQuads quads,
        bool flipX,
        bool flipY
    )
    {
        var (topRight, topLeft, bottomLeft, bottomRight) = (flipX, flipY) switch
        {
            (false, false) => (quads.TopRight, quads.TopLeft, quads.BottomLeft, quads.BottomRight),
            (false, true ) => (quads.BottomRight, quads.BottomLeft, quads.TopLeft, quads.TopRight),
            (true , false) => (quads.TopLeft, quads.TopRight, quads.BottomRight, quads.BottomLeft),
            (true , true ) => (quads.BottomLeft, quads.BottomRight, quads.TopRight, quads.TopLeft)
        };

        var ((trX, trY), (tlX, tlY), (blX, blY), (brX, brY)) = (flipX, flipY) switch
        {
            (false, false) => ((1.0f, 0.0f), (0.0f, 0.0f), (0.0f, 1.0f), (1.0f, 1.0f)),
            (false, true) => ((1.0f, 1.0f), (0.0f, 1.0f), (0.0f, 0.0f), (1.0f, 0.0f)),
            (true, false) => ((0.0f, 0.0f), (1.0f, 0.0f), (1.0f, 1.0f), (0.0f, 1.0f)),
            (true, true) => ((0.0f, 1.0f), (1.0f, 1.0f), (1.0f, 0.0f), (0.0f, 0.0f))
        };
        
        Rlgl.SetTexture(texture.Id);

        Rlgl.Begin(0x0007);
        Rlgl.Color4ub(Color.White.R, Color.White.G, Color.White.B, Color.White.A);

        Rlgl.TexCoord2f(trX, trY);
        Rlgl.Vertex2f(topRight.X, topRight.Y);
        
        Rlgl.TexCoord2f(tlX, tlY);
        Rlgl.Vertex2f(topLeft.X, topLeft.Y);
        
        Rlgl.TexCoord2f(blX, blY);
        Rlgl.Vertex2f(bottomLeft.X, bottomLeft.Y);
        
        Rlgl.TexCoord2f(brX, brY);
        Rlgl.Vertex2f(bottomRight.X, bottomRight.Y);
        
        Rlgl.TexCoord2f(trX, trY);
        Rlgl.Vertex2f(topRight.X, topRight.Y);
        Rlgl.End();

        Rlgl.SetTexture(0);
    }
    
    internal static void DrawTilePreview(
        ref InitTile init, 
        ref Texture2D texture, 
        ref Color color, 
        (int x, int y) position
    )
    {
        var uniformLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "inputTexture");
        var colorLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "highlightColor");
        var heightStartLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "heightStart");
        var heightLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "height");
        var widthLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "width");

        var startingTextureHeight = Utils.GetTilePreviewStartingHeight(init);
        float calcStartingTextureHeight = (float)startingTextureHeight / (float)texture.Height;
        float calcTextureHeight = (float)(init.Size.Item2 * GLOBALS.PreviewScale) / (float)texture.Height;
        float calcTextureWidth = (float)(init.Size.Item1 * GLOBALS.PreviewScale) / (float)texture.Width;

        BeginShaderMode(GLOBALS.Shaders.TilePreview);
        SetShaderValueTexture(GLOBALS.Shaders.TilePreview, uniformLoc, texture);
        SetShaderValue(GLOBALS.Shaders.TilePreview, colorLoc, new System.Numerics.Vector4(color.R, color.G, color.B, color.A), ShaderUniformDataType.Vec4);
        SetShaderValue(GLOBALS.Shaders.TilePreview, heightStartLoc, calcStartingTextureHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.TilePreview, heightLoc, calcTextureHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.TilePreview, widthLoc, calcTextureWidth, ShaderUniformDataType.Float);

        DrawTexturePro(
            texture,
            new(0, 0, texture.Width, texture.Height),
            new(position.x * GLOBALS.PreviewScale, position.y * GLOBALS.PreviewScale, init.Size.Item1 * GLOBALS.PreviewScale, init.Size.Item2 * GLOBALS.PreviewScale),
            Raymath.Vector2Scale(Utils.GetTileHeadOrigin(ref init), GLOBALS.PreviewScale),
            0,
            Color.White
        );
        EndShaderMode();
    }
    
    internal static void DrawTilePreview(
        ref InitTile init, 
        ref Texture2D texture, 
        ref Color color, 
        (int x, int y) position,
        int scale
    )
    {
        var uniformLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "inputTexture");
        var colorLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "highlightColor");
        var heightStartLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "heightStart");
        var heightLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "height");
        var widthLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "width");

        var startingTextureHeight = Utils.GetTilePreviewStartingHeight(init);
        float calcStartingTextureHeight = (float)startingTextureHeight / (float)texture.Height;
        float calcTextureHeight = (float)(init.Size.Item2 * GLOBALS.PreviewScale) / (float)texture.Height;
        float calcTextureWidth = (float)(init.Size.Item1 * GLOBALS.PreviewScale) / (float)texture.Width;

        BeginShaderMode(GLOBALS.Shaders.TilePreview);
        SetShaderValueTexture(GLOBALS.Shaders.TilePreview, uniformLoc, texture);
        SetShaderValue(GLOBALS.Shaders.TilePreview, colorLoc, new System.Numerics.Vector4(color.R, color.G, color.B, color.A), ShaderUniformDataType.Vec4);
        SetShaderValue(GLOBALS.Shaders.TilePreview, heightStartLoc, calcStartingTextureHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.TilePreview, heightLoc, calcTextureHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.TilePreview, widthLoc, calcTextureWidth, ShaderUniformDataType.Float);

        DrawTexturePro(
            texture,
            new(0, 0, texture.Width, texture.Height),
            new(position.x * scale, position.y * scale, init.Size.Item1 * scale, init.Size.Item2 * scale),
            Raymath.Vector2Scale(Utils.GetTileHeadOrigin(init), scale),
            0,
            Color.White
        );
        EndShaderMode();
    }
    
    internal static void DrawTilePreview(
        in InitTile init, 
        in Texture2D texture, 
        in Color color, 
        in (int x, int y) position,
        in int scale
    )
    {
        var uniformLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "inputTexture");
        var colorLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "highlightColor");
        var heightStartLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "heightStart");
        var heightLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "height");
        var widthLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "width");

        var startingTextureHeight = Utils.GetTilePreviewStartingHeight(init);
        var calcStartingTextureHeight = (float)startingTextureHeight / (float)texture.Height;
        var calcTextureHeight = (float)(init.Size.Item2 * 16) / (float)texture.Height;
        var calcTextureWidth = (float)(init.Size.Item1 * 16) / (float)texture.Width;

        BeginShaderMode(GLOBALS.Shaders.TilePreview);
        SetShaderValueTexture(GLOBALS.Shaders.TilePreview, uniformLoc, texture);
        SetShaderValue(GLOBALS.Shaders.TilePreview, colorLoc, new Vector4(color.R, color.G, color.B, color.A), ShaderUniformDataType.Vec4);
        SetShaderValue(GLOBALS.Shaders.TilePreview, heightStartLoc, calcStartingTextureHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.TilePreview, heightLoc, calcTextureHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.TilePreview, widthLoc, calcTextureWidth, ShaderUniformDataType.Float);

        DrawTexturePro(
            texture,
            new(0, 0, texture.Width, texture.Height),
            new(position.x * scale, position.y * scale, init.Size.Item1 * scale, init.Size.Item2 * scale),
            Raymath.Vector2Scale(Utils.GetTileHeadOrigin(init), scale),
            0,
            Color.White
        );
        
        EndShaderMode();
    }
    
    internal static void DrawTilePreview(
        in InitTile init, 
        in Texture2D texture, 
        in Color color, 
        in Vector2 position,
        in int scale
    )
    {
        var uniformLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "inputTexture");
        var colorLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "highlightColor");
        var heightStartLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "heightStart");
        var heightLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "height");
        var widthLoc = GetShaderLocation(GLOBALS.Shaders.TilePreview, "width");

        var startingTextureHeight = Utils.GetTilePreviewStartingHeight(init);
        var calcStartingTextureHeight = (float)startingTextureHeight / (float)texture.Height;
        var calcTextureHeight = (float)(init.Size.Item2 * 16) / (float)texture.Height;
        var calcTextureWidth = (float)(init.Size.Item1 * 16) / (float)texture.Width;

        BeginShaderMode(GLOBALS.Shaders.TilePreview);
        SetShaderValueTexture(GLOBALS.Shaders.TilePreview, uniformLoc, texture);
        SetShaderValue(GLOBALS.Shaders.TilePreview, colorLoc, new Vector4(color.R, color.G, color.B, color.A), ShaderUniformDataType.Vec4);
        SetShaderValue(GLOBALS.Shaders.TilePreview, heightStartLoc, calcStartingTextureHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.TilePreview, heightLoc, calcTextureHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.TilePreview, widthLoc, calcTextureWidth, ShaderUniformDataType.Float);

        var quads = new PropQuads
        {
            TopLeft = (position - Utils.GetTileHeadOrigin(init)) * scale,
            TopRight = (position + new Vector2(init.Size.Item1, 0) - Utils.GetTileHeadOrigin(init)) * scale,
            BottomRight = (position + new Vector2(init.Size.Item1, init.Size.Item2) - Utils.GetTileHeadOrigin(init)) * scale,
            BottomLeft = (position + new Vector2(0, init.Size.Item2) - Utils.GetTileHeadOrigin(init)) * scale
        };
        
        DrawTextureQuads(
            texture, 
            quads
        );
        
        EndShaderMode();
    }
    
    /// <summary>
    /// Draws a camera (used in the camera editor)
    /// </summary>
    /// <param name="origin">the top left corner of the camera</param>
    /// <param name="quad">camera quads</param>
    /// <param name="camera">the page camera</param>
    /// <param name="index">when not -1, it'll be displayed at the top-left corner of the camera to visually differentiate cameras more easily</param>
    /// <returns>two boolean values that signal whether the camera was click/dragged</returns>
    internal static (bool clicked, bool hovered) DrawCameraSprite(
        Vector2 origin,
        CameraQuad quad,
        Camera2D camera,
        int index = 0)
    {
        ref var quadLock = ref GLOBALS.CamQuadLocks[index];
        
        var mouse = GetScreenToWorld2D(Raylib.GetMousePosition(), camera);

        GLOBALS.CamLock = CheckCollisionPointRec(mouse, new(origin.X - 200, origin.Y - 200, GLOBALS.EditorCameraWidth + 200, GLOBALS.EditorCameraHeight + 200)) 
            ? index 
            : 0;
        
        var hover = CheckCollisionPointCircle(mouse, new(origin.X + GLOBALS.EditorCameraWidth / 2f, origin.Y + GLOBALS.EditorCameraHeight / 2f), 50) && quadLock == 0 && GLOBALS.CamLock == index;
        var biggerHover = CheckCollisionPointRec(mouse, new(origin.X, origin.Y, GLOBALS.EditorCameraWidth, GLOBALS.EditorCameraHeight));

        Vector2 pointOrigin1 = new(origin.X, origin.Y),
            pointOrigin2 = new(origin.X + GLOBALS.EditorCameraWidth, origin.Y),
            pointOrigin3 = new(origin.X + GLOBALS.EditorCameraWidth, origin.Y + GLOBALS.EditorCameraHeight),
            pointOrigin4 = new(origin.X, origin.Y + GLOBALS.EditorCameraHeight);

        if (biggerHover)
        {
            DrawRectangleV(
                origin,
                new(GLOBALS.EditorCameraWidth, GLOBALS.EditorCameraHeight),
                new(0, 255, 150, 70)
            );
        }
        else
        {
            DrawRectangleV(
                origin,
                new(GLOBALS.EditorCameraWidth, GLOBALS.EditorCameraHeight),
                new(0, 255, 0, 70)
            );
        }

        if (index != -1)
        {
            DrawText(
                $"{index}",
                (int)origin.X + 10,
                (int)origin.Y + 10,
                20,
                Color.White
            );
        }

        DrawRectangleLinesEx(
            new(origin.X, origin.Y, GLOBALS.EditorCameraWidth, GLOBALS.EditorCameraHeight),
            4f,
            Color.White
        );

        DrawRectangleLinesEx(
            new(origin.X, origin.Y, GLOBALS.EditorCameraWidth, GLOBALS.EditorCameraHeight),
            2f,
            Color.Black
        );

        DrawCircleLines(
            (int)(origin.X + GLOBALS.EditorCameraWidth / 2),
            (int)(origin.Y + GLOBALS.EditorCameraHeight / 2),
            50,
            Color.Black
        );

        if (hover)
        {
            DrawCircleV(
                new(origin.X + GLOBALS.EditorCameraWidth / 2, origin.Y + GLOBALS.EditorCameraHeight / 2),
                50,
                Color.White with { A = 100 }
            );
        }

        DrawLineEx(
            new(origin.X + 4, origin.Y + GLOBALS.EditorCameraHeight / 2),
            new(origin.X + GLOBALS.EditorCameraWidth - 4, origin.Y + GLOBALS.EditorCameraHeight / 2),
            4f,
            Color.Black
        );

        DrawRectangleLinesEx(
            new Rectangle(
                origin.X + 190,
                origin.Y + 20,
                51 * GLOBALS.Scale,
                40 * GLOBALS.Scale - 40
            ),
            4f,
            Color.Red
        );

        var quarter1 = new Rectangle(origin.X - 150, origin.Y - 150, GLOBALS.EditorCameraWidth / 2 + 150, GLOBALS.EditorCameraHeight / 2 + 150);
        var quarter2 = new Rectangle(GLOBALS.EditorCameraWidth / 2 + origin.X, origin.Y - 150, GLOBALS.EditorCameraWidth / 2 + 150, GLOBALS.EditorCameraHeight / 2 + 150);
        var quarter3 = new Rectangle(GLOBALS.EditorCameraWidth / 2 + origin.X, origin.Y + GLOBALS.EditorCameraHeight / 2, GLOBALS.EditorCameraWidth / 2 + 150, GLOBALS.EditorCameraHeight / 2 + 150);
        var quarter4 = new Rectangle(origin.X - 150 , GLOBALS.EditorCameraHeight / 2 + origin.Y, GLOBALS.EditorCameraWidth / 2 + 150, GLOBALS.EditorCameraHeight / 2 + 150);
        
        var topLeftV = new Vector2(pointOrigin1.X + (float)(quad.TopLeft.radius*100 * Math.Cos(float.DegreesToRadians(quad.TopLeft.angle - 90))), pointOrigin1.Y + (float)(quad.TopLeft.radius*100 * Math.Sin(float.DegreesToRadians(quad.TopLeft.angle - 90))));
        var topRightV = new Vector2(pointOrigin2.X + (float)(quad.TopRight.radius*100 * Math.Cos(float.DegreesToRadians(quad.TopRight.angle - 90))), pointOrigin2.Y + (float)(quad.TopRight.radius*100 * Math.Sin(float.DegreesToRadians(quad.TopRight.angle - 90))));
        var bottomRightV = new Vector2(pointOrigin3.X + (float)(quad.BottomRight.radius*100 * Math.Cos(float.DegreesToRadians(quad.BottomRight.angle - 90))), pointOrigin3.Y + (float)(quad.BottomRight.radius*100 * Math.Sin(float.DegreesToRadians(quad.BottomRight.angle - 90))));
        var bottomLeftV = new Vector2(pointOrigin4.X +(float)(quad.BottomLeft.radius*100 * Math.Cos(float.DegreesToRadians(quad.BottomLeft.angle - 90))), pointOrigin4.Y + (float)(quad.BottomLeft.radius*100 * Math.Sin(float.DegreesToRadians(quad.BottomLeft.angle - 90))));
        
        DrawLineV(topLeftV, topRightV, Color.Green);
        DrawLineV(topRightV, bottomRightV, Color.Green);
        DrawLineV(bottomRightV, bottomLeftV, Color.Green);
        DrawLineV(bottomLeftV, topLeftV, Color.Green);
        
        if ((IsMouseButtonReleased(GLOBALS.Settings.Shortcuts.CameraEditor.ManipulateCamera.Button) || 
             IsKeyReleased(GLOBALS.Settings.Shortcuts.CameraEditor.ManipulateCameraAlt.Key))) quadLock = 0;
        
        var ctrl = IsKeyDown(KeyboardKey.LeftControl);
        var shift = IsKeyDown(KeyboardKey.LeftShift);
        var alt = IsKeyDown(KeyboardKey.LeftAlt);

        if ((GLOBALS.Settings.Shortcuts.CameraEditor.ManipulateCamera.Check(ctrl, shift, alt, true) || 
             GLOBALS.Settings.Shortcuts.CameraEditor.ManipulateCameraAlt.Check(ctrl, shift, alt, true)))
        {
            if (GLOBALS.CamQuadLocks[index] == 0)
            {
                if (CheckCollisionPointCircle(mouse, topLeftV, 10))
                {
                    quadLock = 1;
                    GLOBALS.CamLock = index;
                }
                if (CheckCollisionPointCircle(mouse, topRightV, 10)) quadLock = 2;
                if (CheckCollisionPointCircle(mouse, bottomRightV, 10)) quadLock = 3;
                if (CheckCollisionPointCircle(mouse, bottomLeftV, 10)) quadLock = 4;
            }
            else 
            {
                switch (quadLock)
                {
                    case 1:
                    {
                        var radius = Raymath.Vector2Distance(mouse, pointOrigin1);

                        if (radius > 100)
                        {
                            radius = 100;
                        }
                        else
                        {
                            topLeftV = mouse;
                        }

                        var angle = (int)float.RadiansToDegrees(Raymath.Vector2Angle(pointOrigin1 with { Y = pointOrigin1.Y - 1 } - pointOrigin1,
                            mouse - pointOrigin1));
                
                        quad.TopLeft = (angle, radius / 100f);
                    }
                        break;

                    case 2:
                    {
                        var radius = Raymath.Vector2Distance(mouse, pointOrigin2);

                        if (radius > 100)
                        {
                            radius = 100;
                        }
                        else
                        {
                            topRightV = mouse;
                        }

                        var angle = (int)float.RadiansToDegrees(Raymath.Vector2Angle(pointOrigin2 with { Y = pointOrigin2.Y - 1 } - pointOrigin2,
                            mouse - pointOrigin2));
                
                        quad.TopRight = (angle, radius / 100f);
                    }
                        break;

                    case 3:
                    {
                        var radius = Raymath.Vector2Distance(mouse, pointOrigin3);

                        if (radius > 100)
                        {
                            radius = 100;
                        }
                        else
                        {
                            bottomRightV = mouse;
                        }

                        var angle = (int)float.RadiansToDegrees(Raymath.Vector2Angle(pointOrigin3 with { Y = pointOrigin3.Y - 1 } - pointOrigin3,
                            mouse - pointOrigin3));
                
                        quad.BottomRight = (angle, radius / 100f);
                    }
                        break;

                    case 4:
                    {
                        var radius = Raymath.Vector2Distance(mouse, pointOrigin4);

                        if (radius > 100)
                        {
                            radius = 100;
                        }
                        else
                        {
                            topLeftV = mouse;
                        }

                        var angle = (int)float.RadiansToDegrees(Raymath.Vector2Angle(pointOrigin4 with { Y = pointOrigin4.Y - 1 } - pointOrigin4,
                            mouse - pointOrigin4));
                
                        quad.BottomLeft = (angle, radius / 100f);
                    }
                        break;
                }
            } 
        }

        if (GLOBALS.CamLock == index)
        {
            if (CheckCollisionPointRec(mouse, quarter1) || quadLock != 0)
            {
                DrawCircleLines((int)pointOrigin1.X, (int)pointOrigin1.Y, quad.TopLeft.radius*100, Color.Green);
                DrawCircleV(topLeftV, 10, new(0, 255, 0, 255));
            }
            
            if (CheckCollisionPointRec(mouse, quarter2) || quadLock != 0)
            {
                DrawCircleLines((int)pointOrigin2.X, (int)pointOrigin2.Y, quad.TopRight.radius*100, Color.Green);
                DrawCircleV(topRightV, 10, new(0, 255, 0, 255));
            }
            
            if (CheckCollisionPointRec(mouse, quarter3) || quadLock != 0)
            {
                DrawCircleLines((int)pointOrigin3.X, (int)pointOrigin3.Y, quad.BottomRight.radius*100, Color.Green);
                DrawCircleV(bottomRightV, 10, new(0, 255, 0, 255));
            }
            
            if (CheckCollisionPointRec(mouse, quarter4) || quadLock != 0)
            {
                DrawCircleLines((int)pointOrigin4.X, (int)pointOrigin4.Y, quad.BottomLeft.radius*100, Color.Green);
                DrawCircleV(bottomLeftV, 10, new(0, 255, 0, 255));
            }
        }
        

        return (hover && (GLOBALS.Settings.Shortcuts.CameraEditor.GrabCamera.Check(ctrl, shift, alt, true) || GLOBALS.Settings.Shortcuts.CameraEditor.GrabCameraAlt.Check(ctrl, shift, alt, true)), biggerHover);
    }

    /// <summary>
    /// Draws the texture of the tile, layer by layer, from the bottom up.
    /// </summary>
    /// <param name="texture">a reference to the tile texture</param>
    /// <param name="init">a reference to the tile definition</param>
    /// <param name="center">the center origin of the target position to draw on</param>
    /// <param name="quads">target placement quads</param>
    /// <param name="alpha">opacity, from 0 to 255</param>
    internal static void DrawTileAsProp(
        ref Texture2D texture,
        ref InitTile init,
        ref Vector2 center,
        Span<Vector2> quads,
        int alpha = 255
    )
    {
        var layerHeight = (init.Size.Item2 + (init.BufferTiles * 2)) * 20;
        var calLayerHeight = (float)layerHeight / (float)texture.Height;
        var textureCutWidth = (init.Size.Item1 + (init.BufferTiles * 2)) * 20;
        var calTextureCutWidth = (float)textureCutWidth / (float)texture.Width;

        var textureLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "inputTexture");
        var layerNumLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerNum");
        var layerHeightLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerHeight");
        var layerWidthLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerWidth");
        var alphaLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "alpha");

        BeginShaderMode(GLOBALS.Shaders.Prop);

        SetShaderValueTexture(GLOBALS.Shaders.Prop, textureLoc, texture);
        SetShaderValue(GLOBALS.Shaders.Prop, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.Int);
        SetShaderValue(GLOBALS.Shaders.Prop, layerHeightLoc, calLayerHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.Prop, layerWidthLoc, calTextureCutWidth, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.Prop, alphaLoc, alpha / 255f, ShaderUniformDataType.Float);

        var vecs = new Span<Vector2>([
            new Vector2(1, 0), 
            new Vector2(0, 0), 
            new Vector2(0, 1), 
            new Vector2(1, 1), 
            new Vector2(1, 0)
        ]);
        
        DrawTexturePoly(
            texture,
            center,
            quads,
            vecs,
            5,
            Color.White
        );
        
        EndShaderMode();
    }
    
    /// <summary>
    /// Draws the texture of the tile, layer by layer, from the bottom up.
    /// </summary>
    /// <param name="texture">a reference to the tile texture</param>
    /// <param name="init">a reference to the tile definition</param>
    /// <param name="center">the center origin of the target position to draw on</param>
    /// <param name="quads">target placement quads</param>
    /// <param name="alpha">opacity, from 0 to 255</param>
    internal static void DrawTileAsProp(
        in Texture2D texture,
        in InitTile init,
        in Vector2 center,
        int alpha = 255,
        int scale = 20
    )
    {
        var layerHeight = (init.Size.Item2 + (init.BufferTiles * 2)) * 20;
        var calLayerHeight = (float)layerHeight / (float)texture.Height;
        var textureCutWidth = (init.Size.Item1 + (init.BufferTiles * 2)) * 20;
        var calTextureCutWidth = (float)textureCutWidth / (float)texture.Width;

        var textureLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "inputTexture");
        var layerNumLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerNum");
        var layerHeightLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerHeight");
        var layerWidthLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerWidth");
        var alphaLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "alpha");

        BeginShaderMode(GLOBALS.Shaders.Prop);

        SetShaderValueTexture(GLOBALS.Shaders.Prop, textureLoc, texture);
        SetShaderValue(GLOBALS.Shaders.Prop, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.Int);
        SetShaderValue(GLOBALS.Shaders.Prop, layerHeightLoc, calLayerHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.Prop, layerWidthLoc, calTextureCutWidth, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.Prop, alphaLoc, alpha / 255f, ShaderUniformDataType.Float);
        
        var scaledQuads = new PropQuads
        {
            TopLeft = (center - Utils.GetTileHeadOrigin(init)) * scale,
            TopRight = (center + new Vector2(init.Size.Item1, 0) - Utils.GetTileHeadOrigin(init)) * scale,
            BottomRight = (center + new Vector2(init.Size.Item1, init.Size.Item2) - Utils.GetTileHeadOrigin(init)) * scale,
            BottomLeft = (center + new Vector2(0, init.Size.Item2) - Utils.GetTileHeadOrigin(init)) * scale
        };
        
        DrawTextureQuads(texture, scaledQuads);
        
        EndShaderMode();
    }
    
    /// <summary>
    /// Draws the texture of the tile, layer by layer, from the bottom up.
    /// </summary>
    /// <param name="texture">a reference to the tile texture</param>
    /// <param name="init">a reference to the tile definition</param>
    /// <param name="quads">target placement quads</param>
    internal static void DrawTileAsProp(
        ref Texture2D texture,
        ref InitTile init,
        PropQuads quads,
        int depth = 0,
        int alpha = 255
    )
    {
        var scale = GLOBALS.Scale;

        var layerHeight = (init.Size.Item2 + (init.BufferTiles * 2)) * scale;
        float calLayerHeight = (float)layerHeight / (float)texture.Height;
        var textureCutWidth = (init.Size.Item1 + (init.BufferTiles * 2)) * scale;
        float calTextureCutWidth = (float)textureCutWidth / (float)texture.Width;

        var textureLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "inputTexture");
        var layerNumLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerNum");
        var layerHeightLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerHeight");
        var layerWidthLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerWidth");
        var depthLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "depth");
        var alphaLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "alpha");

        BeginShaderMode(GLOBALS.Shaders.Prop);

        SetShaderValueTexture(GLOBALS.Shaders.Prop, textureLoc, texture);
        SetShaderValue(GLOBALS.Shaders.Prop, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.Int);
        SetShaderValue(GLOBALS.Shaders.Prop, layerHeightLoc, calLayerHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.Prop, layerWidthLoc, calTextureCutWidth, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.Prop, depthLoc, depth, ShaderUniformDataType.Int);
        SetShaderValue(GLOBALS.Shaders.Prop, alphaLoc, alpha/255f, ShaderUniformDataType.Float);
        
        DrawTextureQuads(texture, quads);
        
        EndShaderMode();
    }
    
    /// <summary>
    /// Draws the texture of the tile, layer by layer, from the bottom up.
    /// </summary>
    /// <param name="texture">a reference to the tile texture</param>
    /// <param name="init">a reference to the tile definition</param>
    /// <param name="quads">target placement quads</param>
    internal static void DrawTileAsProp(
        ref Texture2D texture,
        ref InitTile init,
        Vector2 position,
        int depth = 0,
        int alpha = 255,
        int scale = 20
    )
    {
        var scaledQuads = new PropQuads
        {
            TopLeft = (position - Utils.GetTileHeadOrigin(init)) * scale,
            TopRight = (position + new Vector2(init.Size.Item1, 0) - Utils.GetTileHeadOrigin(init)) * scale,
            BottomRight = (position + new Vector2(init.Size.Item1, init.Size.Item2) - Utils.GetTileHeadOrigin(init)) * scale,
            BottomLeft = (position + new Vector2(0, init.Size.Item2) - Utils.GetTileHeadOrigin(init)) * scale
        };
        
        var layerHeight = (init.Size.Item2 + (init.BufferTiles * 2)) * 20;
        var calLayerHeight = (float)layerHeight / (float)texture.Height;
        var textureCutWidth = (init.Size.Item1 + (init.BufferTiles * 2)) * 20;
        var calTextureCutWidth = (float)textureCutWidth / (float)texture.Width;

        var textureLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "inputTexture");
        var layerNumLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerNum");
        var layerHeightLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerHeight");
        var layerWidthLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "layerWidth");
        var depthLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "depth");
        var alphaLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "alpha");

        BeginShaderMode(GLOBALS.Shaders.Prop);

        SetShaderValueTexture(GLOBALS.Shaders.Prop, textureLoc, texture);
        SetShaderValue(GLOBALS.Shaders.Prop, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.Int);
        SetShaderValue(GLOBALS.Shaders.Prop, layerHeightLoc, calLayerHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.Prop, layerWidthLoc, calTextureCutWidth, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.Prop, depthLoc, depth, ShaderUniformDataType.Int);
        SetShaderValue(GLOBALS.Shaders.Prop, alphaLoc, alpha/255f, ShaderUniformDataType.Float);
        
        DrawTextureQuads(texture, scaledQuads);
        
        EndShaderMode();
    }
    
    /// Same as DrawTileAsProp() except it applies a tint to the base texture
    internal static void DrawTileAsPropColored(
        ref Texture2D texture, 
        ref InitTile init, 
        ref Vector2 center, 
        Span<Vector2> quads,
        Color tint,
        int depth = 0
    )
    {
        var scale = GLOBALS.Scale;

        if (init.Type == InitTileType.Box)
        {
            var height = (init.Size.Item2 + init.BufferTiles*2) * scale;
            var offset = new Vector2(init.Size.Item2 > 1 ? GLOBALS.Scale : 0, scale * init.Size.Item1 * init.Size.Item2);
            
            float calcHeight = (float)height / (float)texture.Height;
            Vector2 calcOffset = Raymath.Vector2Divide(offset, new(texture.Width, texture.Height));
            float calcWidth = (float)init.Size.Item1 * GLOBALS.Scale / texture.Width;
            
            var textureLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "inputTexture");

            var widthLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "width");
            var heightLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "height");
            var offsetLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "offset");
            var colorLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "tint");
            var depthLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "depth");

            BeginShaderMode(GLOBALS.Shaders.ColoredBoxTileProp);

            SetShaderValueTexture(GLOBALS.Shaders.ColoredBoxTileProp, textureLoc, texture);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, widthLoc, calcWidth, ShaderUniformDataType.Float);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, heightLoc, calcHeight,
                ShaderUniformDataType.Float);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, offsetLoc, calcOffset,
                ShaderUniformDataType.Vec2);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, colorLoc,
                new Vector4(tint.R / 255f, tint.G / 255f, tint.B / 255f, tint.A / 255f),
                ShaderUniformDataType.Vec4);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, depthLoc, depth, ShaderUniformDataType.Int);

            DrawTexturePoly(
                texture,
                center,
                quads,
                new Span<Vector2>([new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0)]),
                5,
                Color.White
            );
            EndShaderMode();
        }
        else
        {
            var layerHeight = (init.Size.Item2 + (init.BufferTiles * 2)) * scale;
            float calLayerHeight = (float)layerHeight / (float)texture.Height;
            var textureCutWidth = (init.Size.Item1 + (init.BufferTiles * 2)) * scale;
            float calTextureCutWidth = (float)textureCutWidth / (float)texture.Width;

            var textureLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "inputTexture");
            var layerNumLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "layerNum");
            var layerHeightLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "layerHeight");
            var layerWidthLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "layerWidth");
            var colorLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "tint");
            var depthLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "depth");

            BeginShaderMode(GLOBALS.Shaders.ColoredTileProp);

            SetShaderValueTexture(GLOBALS.Shaders.ColoredTileProp, textureLoc, texture);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, layerNumLoc, init.Type == InitTileType.VoxelStructRockType ? 1 : init.Repeat.Length,
                ShaderUniformDataType.Int);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, layerHeightLoc, calLayerHeight,
                ShaderUniformDataType.Float);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, layerWidthLoc, calTextureCutWidth,
                ShaderUniformDataType.Float);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, colorLoc,
                new Vector4(tint.R / 255f, tint.G / 255f, tint.B / 255f, tint.A / 255f),
                ShaderUniformDataType.Vec4);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, depthLoc, depth, ShaderUniformDataType.Int);

            DrawTexturePoly(
                texture,
                center,
                quads,
                new Span<Vector2>([new(1, 0), new(0, 0), new(0, 1), new(1, 1), new(1, 0)]),
                5,
                Color.White
            );
            EndShaderMode();
        }
    }
    
    /// Same as DrawTileAsProp() except it applies a tint to the base texture
    internal static void DrawTileAsPropColored(
        ref Texture2D texture, 
        ref InitTile init, 
        PropQuads quads,
        Color tint,
        int depth = 0
    )
    {
        var scale = GLOBALS.Scale;

        if (init.Type == InitTileType.Box)
        {
            var height = (init.Size.Item2 + init.BufferTiles*2) * scale;
            var offset = new Vector2(init.Size.Item2 > 1 ? GLOBALS.Scale : 0, scale * init.Size.Item1 * init.Size.Item2);
            
            float calcHeight = (float)height / (float)texture.Height;
            Vector2 calcOffset = Raymath.Vector2Divide(offset, new(texture.Width, texture.Height));
            float calcWidth = (float)init.Size.Item1 * GLOBALS.Scale / texture.Width;
            
            var textureLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "inputTexture");

            var widthLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "width");
            var heightLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "height");
            var offsetLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "offset");
            var colorLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "tint");
            var depthLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "depth");

            BeginShaderMode(GLOBALS.Shaders.ColoredBoxTileProp);

            SetShaderValueTexture(GLOBALS.Shaders.ColoredBoxTileProp, textureLoc, texture);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, widthLoc, calcWidth, ShaderUniformDataType.Float);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, heightLoc, calcHeight,
                ShaderUniformDataType.Float);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, offsetLoc, calcOffset,
                ShaderUniformDataType.Vec2);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, colorLoc,
                new Vector4(tint.R / 255f, tint.G / 255f, tint.B / 255f, 1.0f),
                ShaderUniformDataType.Vec4);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, depthLoc, depth, ShaderUniformDataType.Int);

            DrawTextureQuads(texture, quads);
            EndShaderMode();
        }
        else
        {
            var layerHeight = (init.Size.Item2 + (init.BufferTiles * 2)) * scale;
            float calLayerHeight = (float)layerHeight / (float)texture.Height;
            var textureCutWidth = (init.Size.Item1 + (init.BufferTiles * 2)) * scale;
            float calTextureCutWidth = (float)textureCutWidth / (float)texture.Width;

            var textureLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "inputTexture");
            var layerNumLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "layerNum");
            var layerHeightLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "layerHeight");
            var layerWidthLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "layerWidth");
            var colorLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "tint");
            var depthLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "depth");

            BeginShaderMode(GLOBALS.Shaders.ColoredTileProp);

            SetShaderValueTexture(GLOBALS.Shaders.ColoredTileProp, textureLoc, texture);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, layerNumLoc, init.Type == InitTileType.VoxelStructRockType ? 1 : init.Repeat.Length,
                ShaderUniformDataType.Int);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, layerHeightLoc, calLayerHeight,
                ShaderUniformDataType.Float);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, layerWidthLoc, calTextureCutWidth,
                ShaderUniformDataType.Float);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, colorLoc,
                new Vector4(tint.R / 255f, tint.G / 255f, tint.B / 255f, 1.0f),
                ShaderUniformDataType.Vec4);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, depthLoc, depth, ShaderUniformDataType.Int);

            DrawTextureQuads(texture, quads);
            EndShaderMode();
        }
    }
    
    /// Same as DrawTileAsProp() except it applies a tint to the base texture
    internal static void DrawTileAsPropColored(
        ref Texture2D texture, 
        ref InitTile init, 
        Vector2 position,
        Color tint,
        int depth,
        int scale
    )
    {
        var scaledQuads = new PropQuads
        {
            TopLeft = (position - Utils.GetTileHeadOrigin(init)) * scale,
            TopRight = (position + new Vector2(init.Size.Item1, 0) - Utils.GetTileHeadOrigin(init)) * scale,
            BottomRight = (position + new Vector2(init.Size.Item1, init.Size.Item2) - Utils.GetTileHeadOrigin(init)) * scale,
            BottomLeft = (position + new Vector2(0, init.Size.Item2) - Utils.GetTileHeadOrigin(init)) * scale
        };
        
        if (init.Type == InitTileType.Box)
        {
            var height = (init.Size.Item2 + init.BufferTiles*2) * 20;
            var offset = new Vector2(init.Size.Item2 > 1 ? 20 : 0, 20 * init.Size.Item1 * init.Size.Item2);
            
            var calcHeight = (float)height / (float)texture.Height;
            var calcOffset = Raymath.Vector2Divide(offset, new(texture.Width, texture.Height));
            var calcWidth = (float)init.Size.Item1 * GLOBALS.Scale / texture.Width;
            
            var textureLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "inputTexture");

            var widthLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "width");
            var heightLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "height");
            var offsetLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "offset");
            var colorLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "tint");
            var depthLoc = GetShaderLocation(GLOBALS.Shaders.ColoredBoxTileProp, "depth");

            BeginShaderMode(GLOBALS.Shaders.ColoredBoxTileProp);

            SetShaderValueTexture(GLOBALS.Shaders.ColoredBoxTileProp, textureLoc, texture);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, widthLoc, calcWidth, ShaderUniformDataType.Float);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, heightLoc, calcHeight,
                ShaderUniformDataType.Float);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, offsetLoc, calcOffset,
                ShaderUniformDataType.Vec2);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, colorLoc,
                new Vector4(tint.R / 255f, tint.G / 255f, tint.B / 255f, 1.0f),
                ShaderUniformDataType.Vec4);
            
            SetShaderValue(GLOBALS.Shaders.ColoredBoxTileProp, depthLoc, depth, ShaderUniformDataType.Int);
            
            DrawTextureQuads(texture, scaledQuads);
            EndShaderMode();
        }
        else
        {
            var layerHeight = (init.Size.Item2 + (init.BufferTiles * 2)) * 20;
            var calLayerHeight = (float)layerHeight / (float)texture.Height;
            var textureCutWidth = (init.Size.Item1 + (init.BufferTiles * 2)) * 20;
            var calTextureCutWidth = (float)textureCutWidth / (float)texture.Width;

            var textureLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "inputTexture");
            var layerNumLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "layerNum");
            var layerHeightLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "layerHeight");
            var layerWidthLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "layerWidth");
            var colorLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "tint");
            var depthLoc = GetShaderLocation(GLOBALS.Shaders.ColoredTileProp, "depth");

            BeginShaderMode(GLOBALS.Shaders.ColoredTileProp);

            SetShaderValueTexture(GLOBALS.Shaders.ColoredTileProp, textureLoc, texture);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, layerNumLoc, init.Type == InitTileType.VoxelStructRockType ? 1 : init.Repeat.Length,
                ShaderUniformDataType.Int);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, layerHeightLoc, calLayerHeight,
                ShaderUniformDataType.Float);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, layerWidthLoc, calTextureCutWidth,
                ShaderUniformDataType.Float);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, colorLoc,
                new Vector4(tint.R / 255f, tint.G / 255f, tint.B / 255f, 1.0f),
                ShaderUniformDataType.Vec4);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, depthLoc, depth, ShaderUniformDataType.Int);

            DrawTextureQuads(texture, scaledQuads);
            EndShaderMode();
        }
    }

    internal static void DrawProp(
        InitPropBase init, 
        ref Texture2D texture, 
        ref Vector2 center,
        Span<Vector2> quads,
        int variation = -1)
    {
        switch (init)
        {
            case InitVariedStandardProp variedStandard:
                DrawVariedStandardProp(variedStandard, ref texture, ref center, quads, variation);
                break;
            
            case InitStandardProp standard:
                DrawStandardProp(standard, ref texture, ref center, quads);
                break;
            
            case InitVariedSoftProp variedSoft:
                DrawVariedSoftProp(variedSoft, ref texture, ref center, quads, variation);
                break;
            
            case InitSoftProp soft:
                DrawSoftProp(soft, ref texture, ref center, quads);
                break;
            
            case InitVariedDecalProp variedDecal:
                DrawVariedDecalProp(variedDecal, ref texture, ref center, quads, variation);
                break;
            
            case InitSimpleDecalProp:
                DrawSimpleDecalProp(ref texture, ref center, quads);
                break;
        }
    }

    internal static void DrawProp(InitPropType type, int category, int index, Prop prop, bool tintedTiles = true)
    {
        var depth = -prop.Depth - GLOBALS.Layer*10;

        switch (type)
        {
            case InitPropType.Tile:
            {
                var texture = GLOBALS.Textures.Tiles[category][index];
                var init = GLOBALS.Tiles[category][index];
                var color = GLOBALS.TileCategories[category].Item2;
            
                if (tintedTiles)
                    DrawTileAsPropColored(ref texture, ref init, prop.Quads, color, depth);
                else
                    DrawTileAsProp(ref texture, ref init, prop.Quads, depth);
            }
                break;

            case InitPropType.Long:
            {
                var texture = GLOBALS.Textures.LongProps[index];
                DrawLongProp(texture, prop.Quads, depth);
            }
                break;

            case InitPropType.Rope:
                break;

            default:
            {
                var texture = GLOBALS.Textures.Props[category][index];
                var init = GLOBALS.Props[category][index];

                switch (init)
                {
                    case InitVariedStandardProp variedStandard:
                        DrawVariedStandardProp(variedStandard, texture, prop.Quads, ((PropVariedSettings)prop.Extras.Settings).Variation, depth);
                        break;

                    case InitStandardProp standard:
                        DrawStandardProp(standard, texture, prop.Quads, depth);
                        break;

                    case InitVariedSoftProp variedSoft:
                        DrawVariedSoftProp(variedSoft, texture, prop.Quads,  ((PropVariedSoftSettings)prop.Extras.Settings).Variation, depth);
                        break;

                    case InitSoftProp:
                        DrawSoftProp(texture, prop.Quads, depth);
                        break;

                    case InitVariedDecalProp variedDecal:
                        DrawVariedDecalProp(variedDecal, texture, prop.Quads, ((PropVariedDecalSettings)prop.Extras.Settings).Variation, depth);
                        break;

                    case InitSimpleDecalProp:
                        DrawSimpleDecalProp(texture, prop.Quads, depth);
                        break;
                }
            }
                break;
        }
    }
    
    internal static void DrawProp(
        BasicPropSettings settings, 
        InitPropBase init, 
        in Texture2D texture, 
        in PropQuads quads,
        int depth)
    {
        switch (init)
        {
            case InitVariedStandardProp variedStandard:
                DrawVariedStandardProp(variedStandard, texture, quads, ((PropVariedSettings)settings).Variation, depth);
                break;

            case InitStandardProp standard:
                DrawStandardProp(standard, texture, quads, depth);
                break;

            case InitVariedSoftProp variedSoft:
                DrawVariedSoftProp(variedSoft, texture, quads,  ((PropVariedSoftSettings)settings).Variation, depth);
                break;

            case InitSoftProp:
                DrawSoftProp(texture, quads, depth);
                break;

            case InitVariedDecalProp variedDecal:
                DrawVariedDecalProp(variedDecal, texture, quads, ((PropVariedDecalSettings)settings).Variation, depth);
                break;

            case InitSimpleDecalProp:
                DrawSimpleDecalProp(texture, quads, depth);
                break;
            
            case InitLongProp:
                DrawLongProp(texture, quads, depth);
                break;
        }
    }

    internal static void DrawLongProp(in Texture2D texture, in PropQuads quads, int depth = 0)
    {
        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.LongProp, "inputTexture");
        var depthLoc = GetShaderLocation(GLOBALS.Shaders.LongProp, "depth");

        BeginShaderMode(GLOBALS.Shaders.LongProp);

        SetShaderValueTexture(GLOBALS.Shaders.LongProp, textureLoc, texture);
        SetShaderValue(GLOBALS.Shaders.LongProp, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuads(texture, quads, flippedX, flippedY);
        EndShaderMode();
    }
    
    internal static void DrawStandardProp(
        InitStandardProp init, 
        ref Texture2D texture, 
        ref Vector2 center,
        Span<Vector2> quads
    )
    {
        var layerHeight = (float)texture.Height / (float)init.Repeat.Length;
        var calcLayerHeight = layerHeight / texture.Height;
        var calcWidth = (float) init.Size.x * GLOBALS.Scale / texture.Width;

        calcWidth = calcWidth > 1.00000f ? 1.0f : calcWidth;
        
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.StandardProp, "inputTexture");
        var layerNumLoc = GetShaderLocation(GLOBALS.Shaders.StandardProp, "layerNum");
        var layerHeightLoc = GetShaderLocation(GLOBALS.Shaders.StandardProp, "layerHeight");
        var widthLoc = GetShaderLocation(GLOBALS.Shaders.StandardProp, "width");

        BeginShaderMode(GLOBALS.Shaders.StandardProp);

        SetShaderValueTexture(GLOBALS.Shaders.StandardProp, textureLoc, texture);
        SetShaderValue(GLOBALS.Shaders.StandardProp, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.Int);
        SetShaderValue(GLOBALS.Shaders.StandardProp, layerHeightLoc, calcLayerHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.StandardProp, widthLoc, calcWidth, ShaderUniformDataType.Float);
        
        DrawTexturePoly(
            texture, 
            center, 
            quads,
            new Span<Vector2>( [new(1, 0), new(0, 0), new(0, 1), new(1, 1), new(1, 0)]), 
            5, 
            Color.White
        );
        EndShaderMode();
    }
    
    internal static void DrawStandardProp(
        InitStandardProp init, 
        in Texture2D texture, 
        PropQuads quads,
        int depth
    )
    {
        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var layerHeight = (float)texture.Height / (float)init.Repeat.Length;
        var calcLayerHeight = layerHeight / texture.Height;
        var calcWidth = (float) init.Size.x * GLOBALS.Scale / texture.Width;

        calcWidth = calcWidth > 1.00000f ? 1.0f : calcWidth;
        
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.StandardProp, "inputTexture");
        var layerNumLoc = GetShaderLocation(GLOBALS.Shaders.StandardProp, "layerNum");
        var layerHeightLoc = GetShaderLocation(GLOBALS.Shaders.StandardProp, "layerHeight");
        var widthLoc = GetShaderLocation(GLOBALS.Shaders.StandardProp, "width");
        var depthLoc = GetShaderLocation(GLOBALS.Shaders.StandardProp, "depth");

        BeginShaderMode(GLOBALS.Shaders.StandardProp);
        
        SetShaderValueTexture(GLOBALS.Shaders.StandardProp, textureLoc, texture);
        SetShaderValue(GLOBALS.Shaders.StandardProp, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.Int);
        SetShaderValue(GLOBALS.Shaders.StandardProp, layerHeightLoc, calcLayerHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.StandardProp, widthLoc, calcWidth, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.StandardProp, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuads(texture, quads, flippedX, flippedY);

        EndShaderMode();
    }

    internal static void DrawVariedStandardProp(
        InitVariedStandardProp init, 
        ref Texture2D texture, 
        ref Vector2 center,
        Span<Vector2> quads,
        int variation
    )
    {
        var layerHeight = (float) init.Size.y * GLOBALS.Scale;
        var variationWidth = (float) init.Size.x * GLOBALS.Scale;
        
        var calcLayerHeight = layerHeight / texture.Height;
        var calcVariationWidth = variationWidth / texture.Width;
        
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.VariedStandardProp, "inputTexture");
        
        var layerNumLoc = GetShaderLocation(GLOBALS.Shaders.VariedStandardProp, "layerNum");
        var layerHeightLoc = GetShaderLocation(GLOBALS.Shaders.VariedStandardProp, "layerHeight");
        var variationWidthLoc = GetShaderLocation(GLOBALS.Shaders.VariedStandardProp, "varWidth");
        var variationLoc = GetShaderLocation(GLOBALS.Shaders.VariedStandardProp, "variation");

        BeginShaderMode(GLOBALS.Shaders.VariedStandardProp);

        SetShaderValueTexture(GLOBALS.Shaders.VariedStandardProp, textureLoc, texture);
       
        SetShaderValue(GLOBALS.Shaders.VariedStandardProp, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.Int);
        SetShaderValue(GLOBALS.Shaders.VariedStandardProp, layerHeightLoc, calcLayerHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.VariedStandardProp, variationWidthLoc, calcVariationWidth, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.VariedStandardProp, variationLoc, variation, ShaderUniformDataType.Int);
        
        DrawTexturePoly(
            texture, 
            center, 
            quads,
            new Span<Vector2>( [new(1, 0), new(0, 0), new(0, 1), new(1, 1), new(1, 0)]), 
            5, 
            Color.White
        );
        EndShaderMode();
    }
    
    internal static void DrawVariedStandardProp(
        InitVariedStandardProp init, 
        in Texture2D texture, 
        PropQuads quads,
        int variation,
        int depth
    )
    {
        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var layerHeight = (float) init.Size.y * GLOBALS.Scale;
        var variationWidth = (float) init.Size.x * GLOBALS.Scale;
        
        var calcLayerHeight = layerHeight / texture.Height;
        var calcVariationWidth = variationWidth / texture.Width;
        
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.VariedStandardProp, "inputTexture");
        
        var layerNumLoc = GetShaderLocation(GLOBALS.Shaders.VariedStandardProp, "layerNum");
        var layerHeightLoc = GetShaderLocation(GLOBALS.Shaders.VariedStandardProp, "layerHeight");
        var variationWidthLoc = GetShaderLocation(GLOBALS.Shaders.VariedStandardProp, "varWidth");
        var variationLoc = GetShaderLocation(GLOBALS.Shaders.VariedStandardProp, "variation");
        var depthLoc = GetShaderLocation(GLOBALS.Shaders.VariedStandardProp, "depth");

        BeginShaderMode(GLOBALS.Shaders.VariedStandardProp);

        SetShaderValueTexture(GLOBALS.Shaders.VariedStandardProp, textureLoc, texture);
       
        SetShaderValue(GLOBALS.Shaders.VariedStandardProp, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.Int);
        SetShaderValue(GLOBALS.Shaders.VariedStandardProp, layerHeightLoc, calcLayerHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.VariedStandardProp, variationWidthLoc, calcVariationWidth, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.VariedStandardProp, variationLoc, variation, ShaderUniformDataType.Int);
        SetShaderValue(GLOBALS.Shaders.VariedStandardProp, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuads(texture, quads, flippedX, flippedY);
        EndShaderMode();
    }

    internal static void DrawSoftProp(
        InitSoftProp init, 
        ref Texture2D texture, 
        ref Vector2 center,
        Span<Vector2> quads
    )
    {
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.SoftProp, "inputTexture");

        BeginShaderMode(GLOBALS.Shaders.SoftProp);

        SetShaderValueTexture(GLOBALS.Shaders.SoftProp, textureLoc, texture);
        
        DrawTexturePoly(
            texture, 
            center, 
            quads,
            new Span<Vector2>( [new(1, 0), new(0, 0), new(0, 1), new(1, 1), new(1, 0)]), 
            5, 
            Color.White
        );
        EndShaderMode();
    }
    
    internal static void DrawSoftProp(in Texture2D texture, in PropQuads quads, int depth)
    {
        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.SoftProp, "inputTexture");
        var depthLoc = GetShaderLocation(GLOBALS.Shaders.SoftProp, "depth");

        BeginShaderMode(GLOBALS.Shaders.SoftProp);

        SetShaderValueTexture(GLOBALS.Shaders.SoftProp, textureLoc, texture);
        SetShaderValue(GLOBALS.Shaders.SoftProp, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuads(texture, quads, flippedX, flippedY);
        EndShaderMode();
    }
    
    internal static void DrawVariedSoftProp(
        InitVariedSoftProp init, 
        ref Texture2D texture, 
        ref Vector2 center,
        Span<Vector2> quads,
        int variation
    )
    {
        var calcHeight = (float) init.SizeInPixels.y / texture.Height;
        var calcVariationWidth = (float) init.SizeInPixels.x / texture.Width;

        var offset = init.Colorize == 1 && init.Variations > 0 ? new Vector2((float) texture.Width - init.SizeInPixels.x, (float) texture.Height/2 - init.SizeInPixels.y) : new(0, 0);
        
        var calcOffset = Raymath.Vector2Divide(offset, new(texture.Width, texture.Height));
        
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "inputTexture");

        var offsetLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "offset");

        var heightLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "height");
        var variationWidthLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "varWidth");
        var variationLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "variation");

        BeginShaderMode(GLOBALS.Shaders.VariedSoftProp);

        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, offsetLoc, calcOffset, ShaderUniformDataType.Vec2);
        
        SetShaderValueTexture(GLOBALS.Shaders.VariedSoftProp, textureLoc, texture);

        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, variationWidthLoc, calcVariationWidth,
            ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, heightLoc, calcHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, variationLoc, variation, ShaderUniformDataType.Int);

        DrawTexturePoly(
            texture, 
            center, 
            quads,
            new Span<Vector2>(
            [
                new(1, 0),
                new(0, 0),
                new(0, 1),
                new(1, 1),
                new(1, 0)
            ]), 
            5, 
            Color.White
        );
        EndShaderMode();
    }
    
    internal static void DrawVariedSoftProp(
        InitVariedSoftProp init, 
        in Texture2D texture, 
        PropQuads quads,
        int variation,
        int depth
    )
    {
        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var calcHeight = (float) init.SizeInPixels.y / texture.Height;
        var calcVariationWidth = (float) init.SizeInPixels.x / texture.Width;

        var textureLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "inputTexture");

        var heightLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "height");
        var variationWidthLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "varWidth");
        var variationLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "variation");
        var depthLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "depth");

        BeginShaderMode(GLOBALS.Shaders.VariedSoftProp);

        SetShaderValueTexture(GLOBALS.Shaders.VariedSoftProp, textureLoc, texture);

        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, variationWidthLoc, calcVariationWidth,
            ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, heightLoc, calcHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, variationLoc, variation, ShaderUniformDataType.Int);
        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuads(texture, quads, flippedX, flippedY);
        EndShaderMode();
    }
    
    internal static void DrawSimpleDecalProp(
        ref Texture2D texture, 
        ref Vector2 center,
        Span<Vector2> quads
    )
    {
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.SimpleDecalProp, "inputTexture");

        BeginShaderMode(GLOBALS.Shaders.SimpleDecalProp);

        SetShaderValueTexture(GLOBALS.Shaders.SimpleDecalProp, textureLoc, texture);
        
        DrawTexturePoly(
            texture, 
            center, 
            quads,
            new Span<Vector2>( [new(1, 0), new(0, 0), new(0, 1), new(1, 1), new(1, 0)]), 
            5, 
            Color.White
        );
        EndShaderMode();
    }
    
    internal static void DrawSimpleDecalProp(in Texture2D texture, in PropQuads quads, int depth = 0)
    {
        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.SimpleDecalProp, "inputTexture");
        var depthLoc = GetShaderLocation(GLOBALS.Shaders.SimpleDecalProp, "depth");

        BeginShaderMode(GLOBALS.Shaders.SimpleDecalProp);

        SetShaderValueTexture(GLOBALS.Shaders.SimpleDecalProp, textureLoc, texture);
        SetShaderValue(GLOBALS.Shaders.SimpleDecalProp, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuads(texture, quads, flippedX, flippedY);
        EndShaderMode();
    }
    
    internal static void DrawVariedDecalProp(
        InitVariedDecalProp init, 
        ref Texture2D texture, 
        ref Vector2 center,
        Span<Vector2> quads,
        int variation
    )
    {
        var calcHeight = (float) init.SizeInPixels.y / texture.Height;
        var calcVariationWidth = (float) init.SizeInPixels.x / texture.Width;

        var textureLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "inputTexture");

        var heightLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "height");
        var variationWidthLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "varWidth");
        var variationLoc = GetShaderLocation(GLOBALS.Shaders.VariedSoftProp, "variation");

        BeginShaderMode(GLOBALS.Shaders.VariedSoftProp);

        SetShaderValueTexture(GLOBALS.Shaders.VariedSoftProp, textureLoc, texture);

        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, variationWidthLoc, calcVariationWidth,
            ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, heightLoc, calcHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.VariedSoftProp, variationLoc, variation, ShaderUniformDataType.Int);

        DrawTexturePoly(
            texture, 
            center, 
            quads,
            new Span<Vector2>(
            [
                new(1, 0),
                new(0, 0),
                new(0, 1),
                new(1, 1),
                new(1, 0)
            ]), 
            5, 
            Color.White
        );
        EndShaderMode();
    }
    
    internal static void DrawVariedDecalProp(
        InitVariedDecalProp init, 
        in Texture2D texture, 
        in PropQuads quads,
        int variation,
        int depth
    )
    {
        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var calcHeight = (float) init.SizeInPixels.y / texture.Height;
        var calcVariationWidth = (float) init.SizeInPixels.x / texture.Width;

        var textureLoc = GetShaderLocation(GLOBALS.Shaders.VariedDecalProp, "inputTexture");

        var heightLoc = GetShaderLocation(GLOBALS.Shaders.VariedDecalProp, "height");
        var variationWidthLoc = GetShaderLocation(GLOBALS.Shaders.VariedDecalProp, "varWidth");
        var variationLoc = GetShaderLocation(GLOBALS.Shaders.VariedDecalProp, "variation");
        var depthLoc = GetShaderLocation(GLOBALS.Shaders.VariedDecalProp, "depth");

        BeginShaderMode(GLOBALS.Shaders.VariedDecalProp);

        SetShaderValueTexture(GLOBALS.Shaders.VariedDecalProp, textureLoc, texture);

        SetShaderValue(GLOBALS.Shaders.VariedDecalProp, variationWidthLoc, calcVariationWidth,
            ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.VariedDecalProp, heightLoc, calcHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.VariedDecalProp, variationLoc, variation, ShaderUniformDataType.Int);
        SetShaderValue(GLOBALS.Shaders.VariedDecalProp, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuads(texture, quads, flippedX, flippedY);
        EndShaderMode();
    }

    /// <summary>
    /// Draws an individual tile geo-spec based on the ID.
    /// </summary>
    /// <param name="id">geo-tile ID</param>
    /// <param name="origin">the top-left corner to start drawing</param>
    /// <param name="scale">the scale of the drawing</param>
    /// <param name="color">the color of the sprite</param>
    internal static void DrawTileSpec(int id, Vector2 origin, int scale, Color color)
    {
        switch (id)
        {
            // air
            case 0:
                DrawRectangleLinesEx(
                    new(origin.X + 10, origin.Y + 10, scale - 20, scale - 20),
                    2,
                    color
                );
                break;

            // solid
            case 1:
                DrawRectangleV(origin, Raymath.Vector2Scale(new(1, 1), scale), color);
                break;

            // slopes
            case 2:
                DrawTriangle(
                    origin,
                    new(origin.X, origin.Y + scale),
                    new(origin.X + scale, origin.Y + scale),
                    color
                );
                break;

            case 3:
                DrawTriangle(
                    new(origin.X + scale, origin.Y),
                    new(origin.X, origin.Y + scale),
                    new(origin.X + scale, origin.Y + scale),
                    color
                );
                break;

            case 4:
                DrawTriangle(
                    origin,
                    new(origin.X, origin.Y + scale),
                    new(origin.X + scale, origin.Y),
                    color
                );
                break;
            case 5:
                DrawTriangle(
                    origin,
                    new(origin.X + scale, origin.Y + scale),
                    new(origin.X + scale, origin.Y),
                    color
                );
                break;

            // platform
            case 6:
                DrawRectangleV(
                    origin,
                    new(scale, scale / 2),
                    color
                );
                break;

            // shortcut entrance
            case 7:
                var entryTexture = GLOBALS.Textures.GeoBlocks[6];
                
                DrawTexturePro(
                    entryTexture, 
                    new(0, 0, entryTexture.Width, entryTexture.Height),
                    new (origin.X, origin.Y, scale, scale),
                    new(0, 0),
                    0,
                    color
                );
                break;

            // glass
            case 9: 
                var glassTexture = GLOBALS.Textures.GeoBlocks[7];
                
                DrawTexturePro(
                    glassTexture, 
                    new(0, 0, glassTexture.Width, glassTexture.Height),
                    new (origin.X, origin.Y, scale, scale),
                    new(0, 0),
                    0,
                    color
                );
                break;
        }
    }
    
    /// <summary>
    /// Draws an individual tile geo-spec based on the ID.
    /// </summary>
    /// <param name="id">geo-tile ID</param>
    /// <param name="origin">the top-left corner to start drawing</param>
    /// <param name="scale">the scale of the drawing</param>
    /// <param name="color">the color of the sprite</param>
    internal static void DrawTileSpec(int id, Vector2 origin, float scale, Color color)
    {
        switch (id)
        {
            // air
            case 0:
                DrawRectangleLinesEx(
                    new(origin.X + 10, origin.Y + 10, scale - 20, scale - 20),
                    2,
                    color
                );
                break;

            // solid
            case 1:
                DrawRectangleV(origin, Raymath.Vector2Scale(new(1, 1), scale), color);
                break;

            // slopes
            case 2:
                DrawTriangle(
                    origin,
                    new(origin.X, origin.Y + scale),
                    new(origin.X + scale, origin.Y + scale),
                    color
                );
                break;

            case 3:
                DrawTriangle(
                    new(origin.X + scale, origin.Y),
                    new(origin.X, origin.Y + scale),
                    new(origin.X + scale, origin.Y + scale),
                    color
                );
                break;

            case 4:
                DrawTriangle(
                    origin,
                    new(origin.X, origin.Y + scale),
                    new(origin.X + scale, origin.Y),
                    color
                );
                break;
            case 5:
                DrawTriangle(
                    origin,
                    new(origin.X + scale, origin.Y + scale),
                    new(origin.X + scale, origin.Y),
                    color
                );
                break;

            // platform
            case 6:
                DrawRectangleV(
                    origin,
                    new(scale, scale / 2),
                    color
                );
                break;

            // shortcut entrance
            case 7:
                var entryTexture = GLOBALS.Textures.GeoBlocks[6];
                
                DrawTexturePro(
                    entryTexture, 
                    new(0, 0, entryTexture.Width, entryTexture.Height),
                    new (origin.X, origin.Y, scale, scale),
                    new(0, 0),
                    0,
                    color
                );
                break;

            // glass
            case 9: 
                var glassTexture = GLOBALS.Textures.GeoBlocks[7];
                
                DrawTexturePro(
                    glassTexture, 
                    new(0, 0, glassTexture.Width, glassTexture.Height),
                    new (origin.X, origin.Y, scale, scale),
                    new(0, 0),
                    0,
                    color
                );
                break;
        }
    }
    
    /// <summary>
    /// Draws an individual tile geo-spec based on the ID.
    /// </summary>
    /// <param name="id">geo-tile ID</param>
    /// <param name="origin">the top-left corner to start drawing</param>
    /// <param name="scale">the scale of the drawing</param>
    /// <param name="color">the color of the sprite</param>
    internal static void DrawTileSpec(int x, int y, int id, float scale, Color color)
    {
        var vector = new Vector2(x, y);
        
        switch (id)
        {
            // air
            case 0:
                DrawRectangleLinesEx(
                    new(x + 10, y + 10, scale - 20, scale - 20),
                    2,
                    color
                );
                break;

            // solid
            case 1:
                DrawRectangleV(vector, Raymath.Vector2Scale(new(1, 1), scale), color);
                break;

            // slopes
            case 2:
                DrawTriangle(
                    vector,
                    new(x, y + scale),
                    new(x + scale, y + scale),
                    color
                );
                break;

            case 3:
                DrawTriangle(
                    new(x + scale, y),
                    new(x, y + scale),
                    new(x + scale, y + scale),
                    color
                );
                break;

            case 4:
                DrawTriangle(
                    vector,
                    new(x, y + scale),
                    new(x + scale, y),
                    color
                );
                break;
            case 5:
                DrawTriangle(
                    vector,
                    new(x + scale, y + scale),
                    new(x + scale, y),
                    color
                );
                break;

            // platform
            case 6:
                DrawRectangleV(
                    vector,
                    new(scale, scale / 2),
                    color
                );
                break;

            // shortcut entrance
            case 7:
                var entryTexture = GLOBALS.Textures.GeoBlocks[6];
                
                DrawTexturePro(
                    entryTexture, 
                    new(0, 0, entryTexture.Width, entryTexture.Height),
                    new (x, y, scale, scale),
                    new(0, 0),
                    0,
                    color
                );
                break;

            // glass
            case 9: 
                var glassTexture = GLOBALS.Textures.GeoBlocks[7];
                
                DrawTexturePro(
                    glassTexture, 
                    new(0, 0, glassTexture.Width, glassTexture.Height),
                    new (x, y, scale, scale),
                    new(0, 0),
                    0,
                    color
                );
                break;
        }
    }
    
    /// <summary>
    /// Draws an individual tile geo-spec based on the ID.
    /// </summary>
    /// <param name="id">geo-tile ID</param>
    /// <param name="origin">the top-left corner to start drawing</param>
    /// <param name="scale">the scale of the drawing</param>
    /// <param name="color">the color of the sprite</param>
    internal static void DrawTileSpec(float x, float y, int id, float scale, Color color)
    {
        var vector = new Vector2(x, y);
        
        switch (id)
        {
            // air
            case 0:
                DrawRectangleLinesEx(
                    new(x + 10, y + 10, scale - 20, scale - 20),
                    2,
                    color
                );
                break;

            // solid
            case 1:
                DrawRectangleV(vector, Raymath.Vector2Scale(new(1, 1), scale), color);
                break;

            // slopes
            case 2:
                DrawTriangle(
                    vector,
                    new(x, y + scale),
                    new(x + scale, y + scale),
                    color
                );
                break;

            case 3:
                DrawTriangle(
                    new(x + scale, y),
                    new(x, y + scale),
                    new(x + scale, y + scale),
                    color
                );
                break;

            case 4:
                DrawTriangle(
                    vector,
                    new(x, y + scale),
                    new(x + scale, y),
                    color
                );
                break;
            case 5:
                DrawTriangle(
                    vector,
                    new(x + scale, y + scale),
                    new(x + scale, y),
                    color
                );
                break;

            // platform
            case 6:
                DrawRectangleV(
                    vector,
                    new(scale, scale / 2),
                    color
                );
                break;

            // shortcut entrance
            case 7:
                var entryTexture = GLOBALS.Textures.GeoBlocks[6];
                
                DrawTexturePro(
                    entryTexture, 
                    new(0, 0, entryTexture.Width, entryTexture.Height),
                    new (x, y, scale, scale),
                    new(0, 0),
                    0,
                    color
                );
                break;

            // glass
            case 9: 
                var glassTexture = GLOBALS.Textures.GeoBlocks[7];
                
                DrawTexturePro(
                    glassTexture, 
                    new(0, 0, glassTexture.Width, glassTexture.Height),
                    new (x, y, scale, scale),
                    new(0, 0),
                    0,
                    color
                );
                break;
        }
    }

    internal static void DrawDepthIndicator((InitPropType type, (int category, int index) position, Prop prop) prop)
    {
        var (c, i) = prop.position;
        
        switch (prop.type)
        {
            case InitPropType.Tile:
            {
                InitTile init;
                
                init = GLOBALS.Tiles[c][i];
                
                var depth = init.Repeat.Sum() * 10;
                var offset = -prop.prop.Depth * 10;
                var overflow = offset + depth - 290;
            
                DrawRectangleRec(
                    new Rectangle(
                        offset, 
                        0, 
                        depth - (overflow > 0 ? overflow : 0), 
                        20
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
                var init = GLOBALS.Props[c][i];
                
                DrawRectangleRec(
                    new Rectangle(
                        -prop.prop.Depth * 10, 
                        0,
                        init switch
                        {
                            InitVariedStandardProp v => v.Repeat.Length,
                            InitStandardProp s => s.Repeat.Length,
                            _ => init.Depth
                        } * 10, 
                        20
                    ),
                    new Color(100, 100, 180, 255)
                );
            }
                break;
        }
        
        DrawRectangleLinesEx(
            new Rectangle(0, 0, 290, 20),
            2f,
            Color.Black
        );

        DrawLineEx(
            new Vector2(90, 0 + 0),
            new Vector2(90, 0 + 5),
            2f,
            Color.Black
        );

        DrawLineEx(
            new Vector2(180, 0),
            new Vector2(180, 5),
            2f,
            Color.Black
        );
    }

    /// Needs rlImGui mode
    internal static class ImGui
    {
        internal static Rectangle ShortcutsWindow(IEditorShortcuts editorShortcuts)
        {
            var strings = editorShortcuts.CachedStrings;

            var expanded = ImGuiNET.ImGui.Begin("Shortcuts");
            var pos = ImGuiNET.ImGui.GetWindowPos();
            var size = ImGuiNET.ImGui.GetWindowSize();
            
            if (expanded)
            {
                foreach (var (nameStr, shortcutStr) in strings)
                {
                    ImGuiNET.ImGui.Text($"{nameStr}: {shortcutStr}");
                }
            }
            ImGuiNET.ImGui.End();

            return new Rectangle(pos.X, pos.Y, size.X, size.Y);
        }

        internal static Rectangle NavigationWindow()
        {
            var expanded = ImGuiNET.ImGui.Begin("Navigation##GlobalNavigation");
            var pos = ImGuiNET.ImGui.GetWindowPos();
            var size = ImGuiNET.ImGui.GetWindowSize();
            
            if (expanded)
            {
                if (ImGuiNET.ImGui.Selectable("Main", GLOBALS.Page == 1)) GLOBALS.Page = 1;
                if (ImGuiNET.ImGui.Selectable("Geometry", GLOBALS.Page == 2)) GLOBALS.Page = 2;
                if (ImGuiNET.ImGui.Selectable("Tiles", GLOBALS.Page == 3)) GLOBALS.Page = 3;
                if (ImGuiNET.ImGui.Selectable("Cameras", GLOBALS.Page == 4)) GLOBALS.Page = 4;
                if (ImGuiNET.ImGui.Selectable("Light", GLOBALS.Page == 5)) GLOBALS.Page = 5;
                if (ImGuiNET.ImGui.Selectable("Dimensions", GLOBALS.Page == 6)) GLOBALS.Page = 6;
                if (ImGuiNET.ImGui.Selectable("Effects", GLOBALS.Page == 7)) GLOBALS.Page = 7;
                if (ImGuiNET.ImGui.Selectable("Props", GLOBALS.Page == 8)) GLOBALS.Page = 8;
                if (ImGuiNET.ImGui.Selectable("Settings", GLOBALS.Page == 9)) GLOBALS.Page = 9;
                
                ImGuiNET.ImGui.End();
            }
            
            return new Rectangle(pos.X, pos.Y, size.X, size.Y);
        }
    }
}