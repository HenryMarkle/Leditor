using static Raylib_cs.Raylib;
using System.Numerics;
using Leditor.Types;
using Leditor.Data.Tiles;

namespace Leditor;

#nullable enable

/// <summary>
/// Functions that are called each frame; Must only be called after window initialization and in drawing mode.
/// </summary>
internal static class Printers
{
    internal static void DrawProgressBar(Rectangle rect, int progress, int total, bool outline, Color color)
    {
        if (outline) DrawRectangleLinesEx(rect, 2, color);

        DrawRectangleRec(rect with { Width = progress*rect.Width / total }, color);
    }
    
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

    internal static void DrawGeoLayer(RunCell[,,] matrix, int layer, int scale, bool grid, Color color)
    {
        var width = matrix.GetLength(1);
        var height = matrix.GetLength(0);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var cell = matrix[y, x, layer];
                
                DrawTileSpec(x*scale, y*scale, cell.Geo, scale, color);

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
                                var index = Utils.GetStackableTextureIndex(s, Utils.GetContext(matrix, width, height, x, y, layer));

                                if (index is 22 or 23 or 24 or 25)
                                {
                                    matrix[y, x, layer].Geo = 7;
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
                                        Utils.GetContext(matrix, width,height, x, y, layer))];
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
    
    
    internal static void DrawGeoLayer(int layer, int scale, bool grid, Color color)
    {
        for (var y = 0; y < GLOBALS.Level.Height; y++)
        {
            for (var x = 0; x < GLOBALS.Level.Width; x++)
            {
                var cell = GLOBALS.Level.GeoMatrix[y, x, layer];
                
                DrawTileSpec(x*scale, y*scale, cell.Geo, scale, color);

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
    
    internal static void DrawGeoLayerIntoBuffer(RenderTexture2D renderTexture, int layer, int scale)
    {
        var color = new Color(0, 255, 0, 255);

        var almostWhite = new Color(250, 250, 250, 255);

        BeginTextureMode(renderTexture);
        ClearBackground(Color.White);

        for (var y = 0; y < GLOBALS.Level.Height; y++)
        {
            for (var x = 0; x < GLOBALS.Level.Width; x++)
            {
                var cell = GLOBALS.Level.GeoMatrix[y, x, layer];
                
                DrawTileSpec(x*scale, y*scale, cell.Geo, scale, color);

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
                                        almostWhite
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
                                    almostWhite
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
                                    almostWhite
                                );
                                break;
                        }
                    }
                }
            }
        }

        EndTextureMode();
    }
    internal static void DrawGeoLayerWithMaterialsIntoBuffer(RenderTexture2D renderTexture, int layer, int scale, bool renderMaterials)
    {
        var color = new Color(0, 255, 0, 255);

        BeginTextureMode(renderTexture);
        ClearBackground(Color.White);

        for (var y = 0; y < GLOBALS.Level.Height; y++)
        {
            for (var x = 0; x < GLOBALS.Level.Width; x++)
            {
                var cell = GLOBALS.Level.GeoMatrix[y, x, layer];
                // var tileCell = GLOBALS.Level.TileMatrix[y, x, layer];
                
                DrawTileSpec(x*scale, y*scale, cell.Geo, scale, color);

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

                if (renderMaterials) {
                    switch (GLOBALS.Level.TileMatrix[y, x, layer].Data) {
                        case TileMaterial m:
                        DrawMaterialTexture(m.Name, cell.Geo, x, y, scale, 255);
                        break;

                        case TileDefault:
                        DrawMaterialTexture(GLOBALS.Level.DefaultMaterial, cell.Geo, x, y, scale, 255);
                        break;
                    }
                } 
                // else if (!renderMaterials && tileCell.Type == TileType.Material) {
                //     // var materialName = ((TileMaterial)tileCell.Data).Name;
                //     var origin = new Vector2(x * scale + 5, y * scale + 5);
                //     var color = GLOBALS.Level.MaterialColors[y, x, targetLayer];

                //     color.A = (byte)((targetLayer == currentLayer) ? 255 : opacity);

                //     if (color.R != 0 || color.G != 0 || color.B != 0)
                //     {

                //         switch (GLOBALS.Level.GeoMatrix[y, x, targetLayer].Geo)
                //         {
                //             case 1:
                //                 DrawRectangle(
                //                     x * scale + 6,
                //                     y * scale + 6,
                //                     scale - 12,
                //                     scale - 12,
                //                     color
                //                 );
                //                 break;


                //             case 2:
                //                 DrawTriangle(
                //                     origin,
                //                     new(origin.X, origin.Y + scale - 10),
                //                     new(origin.X + scale - 10, origin.Y + scale - 10),
                //                     color
                //                 );
                //                 break;


                //             case 3:
                //                 DrawTriangle(
                //                     new(origin.X + scale - 10, origin.Y),
                //                     new(origin.X, origin.Y + scale - 10),
                //                     new(origin.X + scale - 10, origin.Y + scale - 10),
                //                     color
                //                 );
                //                 break;

                //             case 4:
                //                 DrawTriangle(
                //                     origin,
                //                     new(origin.X, origin.Y + scale - 10),
                //                     new(origin.X + scale - 10, origin.Y),
                //                     color
                //                 );
                //                 break;

                //             case 5:
                //                 DrawTriangle(
                //                     origin,
                //                     new(origin.X + scale - 10, origin.Y + scale - 10),
                //                     new(origin.X + scale - 10, origin.Y),
                //                     color
                //                 );
                //                 break;

                //             case 6:
                //                 DrawRectangleV(
                //                     origin,
                //                     new(scale - 10, (scale - 10) / 2),
                //                     color
                //                 );
                //                 break;
                //         }
                //     }
                // }
            }
        }

        EndTextureMode();
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

    internal static void DrawTileLayer(int currentLayer, int targetLayer, int scale, bool grid, TileDrawMode drawMode, byte opacity = 255, bool deepTileOpacity = true)
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
                    tileCell = GLOBALS.Level.TileMatrix[y, x, targetLayer];
                }
                catch (IndexOutOfRangeException ie)
                {
                    throw new IndexOutOfRangeException(innerException: ie, message: $"Failed to fetch tile cell from {nameof(GLOBALS.Level.TileMatrix)}[{GLOBALS.Level.TileMatrix.GetLength(0)}, {GLOBALS.Level.TileMatrix.GetLength(1)}, {GLOBALS.Level.TileMatrix.GetLength(2)}]: x, y, or z ({x}, {y}, {targetLayer}) was out of bounds");
                }
                #else
                tileCell = GLOBALS.Level.TileMatrix[y, x, targetLayer];
                #endif
                
                if (tileCell.Type == TileType.TileHead)
                {
                    var data = (TileHead)tileCell.Data;

                    TileDefinition? init = data.Definition;
                    var undefined = init is null;

                    var tileTexture = undefined
                        ? GLOBALS.Textures.MissingTile 
                        : data.Definition!.Texture;

                    var color = Color.Purple;

                    if (GLOBALS.TileDex?.TryGetTileColor(data.Definition?.Name ?? "", out var foundColor) ?? false)
                    {
                        color = foundColor;
                    }

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
                        var center = new Vector2(
                        init!.Size.Item1 % 2 == 0 ? x * scale + scale : x * scale + scale/2f, 
                        init!.Size.Item2 % 2 == 0 ? y * scale + scale : y * scale + scale/2f);

                        var width = (scale / 20f)/2 * (init.Type == Data.Tiles.TileType.Box ? init.Size.Width : init.Size.Width + init.BufferTiles * 2) * 20;
                        var height = (scale / 20f)/2 * ((init.Type == Data.Tiles.TileType.Box
                            ? init.Size.Item2
                            : (init.Size.Item2 + init.BufferTiles * 2)) * 20);

                        var depth2 = Utils.SpecHasDepth(init.Specs);
                        var depth3 = Utils.SpecHasDepth(init.Specs, 2);
                        
                        var shouldBeClearlyVisible = (targetLayer == currentLayer) || 
                            (targetLayer + 1 == currentLayer && depth2) || 
                            (targetLayer + 2 == currentLayer && depth3);

                        switch (drawMode) {
                            case TileDrawMode.Preview:
                                DrawTilePreview(
                                    init, 
                                    color with { A = (byte)(shouldBeClearlyVisible ? 255 : opacity)}, 
                                    new Vector2(x, y),
                                    -Utils.GetTileHeadOrigin(init),
                                    scale
                                );
                            break;

                            case TileDrawMode.Tinted:
                                // TODO: Replace
                                DrawTileAsPropColored(
                                    init,
                                    center,
                                    [
                                        new(width, -height),
                                        new(-width, -height),
                                        new(-width, height),
                                        new(width, height),
                                        new(width, -height)
                                    ],
                                    new Color(color.R, color.G, color.B, (byte)(shouldBeClearlyVisible ? 255 : opacity)),
                                    0
                                );
                            break;

                            case TileDrawMode.Palette:
                                
                            break;
                        }
                    }
                }
                else if (tileCell.Type == TileType.TileBody)
                {
                    var missingTexture = GLOBALS.Textures.MissingTile;
                    
                    var (hx, hy, hz) = ((TileBody)tileCell.Data).HeadPosition;

                    var supposedHead = GLOBALS.Level.TileMatrix[hy - 1, hx - 1, hz - 1];

                    if (supposedHead.Data is TileHead { Definition: null } or not TileHead)
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
                    var color = GLOBALS.Level.MaterialColors[y, x, targetLayer];

                    color.A = (byte)((targetLayer == currentLayer) ? 255 : opacity);

                    if (color.R != 0 || color.G != 0 || color.B != 0)
                    {

                        switch (GLOBALS.Level.GeoMatrix[y, x, targetLayer].Geo)
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

    // TODO: drop GLOBALS.Layer references
    internal static void DrawTileLayer(int currentLayer, int targetLayer, int scale, bool grid, TileDrawMode drawMode, in Texture2D palette, byte opacity = 255, bool deepTileOpacity = true)
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
                    tileCell = GLOBALS.Level.TileMatrix[y, x, targetLayer];
                }
                catch (IndexOutOfRangeException ie)
                {
                    throw new IndexOutOfRangeException(innerException: ie, message: $"Failed to fetch tile cell from {nameof(GLOBALS.Level.TileMatrix)}[{GLOBALS.Level.TileMatrix.GetLength(0)}, {GLOBALS.Level.TileMatrix.GetLength(1)}, {GLOBALS.Level.TileMatrix.GetLength(2)}]: x, y, or z ({x}, {y}, {targetLayer}) was out of bounds");
                }
                #else
                tileCell = GLOBALS.Level.TileMatrix[y, x, targetLayer];
                #endif
                
                if (tileCell.Type == TileType.TileHead)
                {
                    var data = (TileHead)tileCell.Data;

                    TileDefinition? init = data.Definition;
                    var undefined = init is null;

                    var tileTexture = undefined
                        ? GLOBALS.Textures.MissingTile 
                        : data.Definition!.Texture;

                    var color = Color.Purple;

                    if (GLOBALS.TileDex?.TryGetTileColor(data.Definition?.Name ?? "", out var foundColor) ?? false)
                    {
                        color = foundColor;
                    }

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
                        var center = new Vector2(
                        init!.Size.Item1 % 2 == 0 ? x * scale + scale : x * scale + scale/2f, 
                        init!.Size.Item2 % 2 == 0 ? y * scale + scale : y * scale + scale/2f);

                        var width = (scale / 20f)/2 * (init.Type == Data.Tiles.TileType.Box ? init.Size.Width : init.Size.Width + init.BufferTiles * 2) * 20;
                        var height = (scale / 20f)/2 * ((init.Type == Data.Tiles.TileType.Box
                            ? init.Size.Item2
                            : (init.Size.Item2 + init.BufferTiles * 2)) * 20);

                        var depth2 = Utils.SpecHasDepth(init.Specs);
                        var depth3 = Utils.SpecHasDepth(init.Specs, 2);
                        
                        var shouldBeClearlyVisible = (targetLayer == currentLayer) || 
                            (targetLayer + 1 == currentLayer && depth2) || 
                            (targetLayer + 2 == currentLayer && depth3);

                        switch (drawMode) {
                            case TileDrawMode.Preview:
                                DrawTilePreview(
                                    init, 
                                    color with { A = (byte)(shouldBeClearlyVisible ? 255 : opacity)}, 
                                    new Vector2(x, y),
                                    -Utils.GetTileHeadOrigin(init),
                                    scale
                                );
                            break;

                            case TileDrawMode.Tinted:
                                // TODO: Replace
                                DrawTileAsPropColored(
                                    init,
                                    center,
                                    [
                                        new(width, -height),
                                        new(-width, -height),
                                        new(-width, height),
                                        new(width, height),
                                        new(width, -height)
                                    ],
                                    new Color(color.R, color.G, color.B, (byte)(shouldBeClearlyVisible ? 255 : opacity)),
                                    0
                                );
                            break;

                            case TileDrawMode.Palette:
                                DrawTileWithPalette(
                                    init,
                                    palette,
                                    center,
                                    [
                                        new(width, -height),
                                        new(-width, -height),
                                        new(-width, height),
                                        new(width, height),
                                        new(width, -height)
                                    ],
                                    targetLayer * 10,
                                    shouldBeClearlyVisible ? 1f : opacity/255f
                                );
                            break;
                        }
                    }
                }
                else if (tileCell.Type == TileType.TileBody)
                {
                    var missingTexture = GLOBALS.Textures.MissingTile;
                    
                    var (hx, hy, hz) = ((TileBody)tileCell.Data).HeadPosition;

                    var supposedHead = GLOBALS.Level.TileMatrix[hy - 1, hx - 1, hz - 1];

                    if (supposedHead.Data is TileHead { Definition: null } or not TileHead)
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
                    
                    if (drawMode == TileDrawMode.Palette) {
                        var op = (byte)((targetLayer == currentLayer) ? 255 : opacity);

                        DrawMaterialTexture(((TileMaterial)tileCell.Data).Name, GLOBALS.Level.GeoMatrix[y, x, targetLayer].Geo, x, y, scale, op);
                        continue;
                    }
                    // var materialName = ((TileMaterial)tileCell.Data).Name;
                    var origin = new Vector2(x * scale + 5, y * scale + 5);
                    var color = GLOBALS.Level.MaterialColors[y, x, targetLayer];

                    color.A = (byte)((targetLayer == currentLayer) ? 255 : opacity);

                    if (color.R != 0 || color.G != 0 || color.B != 0)
                    {

                        switch (GLOBALS.Level.GeoMatrix[y, x, targetLayer].Geo)
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
                else if (tileCell.Data is TileDefault) {
                    if (drawMode == TileDrawMode.Palette) {
                        var op = (byte)((targetLayer == currentLayer) ? 255 : opacity);
                        DrawMaterialTexture(GLOBALS.Level.DefaultMaterial, GLOBALS.Level.GeoMatrix[y, x, targetLayer].Geo, x, y, scale, op);
                        continue;
                    }
                }
            
            }
        }
    }


    internal static void DrawTileLayerIntoBuffer(RenderTexture2D renderTexture, int layer, int scale) {
        BeginTextureMode(renderTexture);
        ClearBackground(Color.White);

        for (var y = 0; y < GLOBALS.Level.Height; y++)
        {
            for (var x = 0; x < GLOBALS.Level.Width; x++)
            {
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

                switch (tileCell.Data) {
                    case TileHead h:
                    {
                        var data = h;

                        TileDefinition? init = data.Definition;
                        var undefined = init is null;

                        var tileTexture = undefined
                            ? GLOBALS.Textures.MissingTile 
                            : data.Definition!.Texture;

                        var color = Color.Purple;

                        if (GLOBALS.TileDex?.TryGetTileColor(data.Definition?.Name ?? "", out var foundColor) ?? false)
                        {
                            color = foundColor;
                        }

                        if (!undefined)
                        {
                            var center = new Vector2(
                            init!.Size.Item1 % 2 == 0 ? x * scale + scale : x * scale + scale/2f, 
                            init!.Size.Item2 % 2 == 0 ? y * scale + scale : y * scale + scale/2f);

                            var width = (scale / 20f)/2 * (init.Type == Data.Tiles.TileType.Box ? init.Size.Width : init.Size.Width + init.BufferTiles * 2) * 20;
                            var height = (scale / 20f)/2 * ((init.Type == Data.Tiles.TileType.Box
                                ? init.Size.Item2
                                : (init.Size.Item2 + init.BufferTiles * 2)) * 20);

                            var depth2 = Utils.SpecHasDepth(init.Specs);
                            var depth3 = Utils.SpecHasDepth(init.Specs, 2);
                            
                            DrawTileAsProp(
                                init,
                                center,
                                [
                                    new(width, -height),
                                    new(-width, -height),
                                    new(-width, height),
                                    new(width, height),
                                    new(width, -height)
                                ],
                                layer * 10,
                                255
                            );
                        }
                    }
                    break;

                    case TileBody b:
                    {
                        var missingTexture = GLOBALS.Textures.MissingTile;
                    
                        var (hx, hy, hz) = b.HeadPosition;

                        var supposedHead = GLOBALS.Level.TileMatrix[hy - 1, hx - 1, hz - 1];

                        if (supposedHead.Data is TileHead { Definition: null } or not TileHead)
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
                    break;

                    default:
                        DrawTileSpec(x*scale, y*scale, GLOBALS.Level.GeoMatrix[y, x, layer].Geo, scale, new Color(0, 255, 0, 255));
                        break;
                }
            }
        }
    
        EndTextureMode();
    }

    internal static void DrawTileLayerWithMaterialTexturesIntoBuffer(RenderTexture2D renderTexture, int layer, int scale) {
        BeginTextureMode(renderTexture);
        ClearBackground(Color.White);

        for (var y = 0; y < GLOBALS.Level.Height; y++)
        {
            for (var x = 0; x < GLOBALS.Level.Width; x++)
            {
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

                var geoCell = GLOBALS.Level.GeoMatrix[y, x, layer];

                if (geoCell.Stackables[1]) {
                    var stackableTexture = GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(1)];

                    DrawTexturePro(
                        stackableTexture, 
                        new(0, 0, stackableTexture.Width, stackableTexture.Height),
                        new(x * scale, y * scale, scale, scale), 
                        new(0, 0), 
                        0, 
                        new Color(0, 255, 0, 255)
                    );
                }

                if (geoCell.Stackables[2]) {
                    var stackableTexture = GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(2)];

                    DrawTexturePro(
                        stackableTexture, 
                        new(0, 0, stackableTexture.Width, stackableTexture.Height),
                        new(x * scale, y * scale, scale, scale), 
                        new(0, 0), 
                        0, 
                        new Color(0, 255, 0, 255)
                    );
                }

                switch (tileCell.Data) {
                    case TileHead h:
                    {
                        var data = h;

                        TileDefinition? init = data.Definition;
                        var undefined = init is null;

                        var tileTexture = undefined
                            ? GLOBALS.Textures.MissingTile 
                            : data.Definition!.Texture;

                        var color = Color.Purple;

                        if (GLOBALS.TileDex?.TryGetTileColor(data.Definition?.Name ?? "", out var foundColor) ?? false)
                        {
                            color = foundColor;
                        }

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
                        else
                        {
                            var center = new Vector2(
                            init!.Size.Item1 % 2 == 0 ? x * scale + scale : x * scale + scale/2f, 
                            init!.Size.Item2 % 2 == 0 ? y * scale + scale : y * scale + scale/2f);

                            var width = (scale / 20f)/2 * (init.Type == Data.Tiles.TileType.Box ? init.Size.Width : init.Size.Width + init.BufferTiles * 2) * 20;
                            var height = (scale / 20f)/2 * ((init.Type == Data.Tiles.TileType.Box
                                ? init.Size.Item2
                                : (init.Size.Item2 + init.BufferTiles * 2)) * 20);

                            var depth2 = Utils.SpecHasDepth(init.Specs);
                            var depth3 = Utils.SpecHasDepth(init.Specs, 2);
                            
                            DrawTileAsProp(
                                init,
                                center,
                                [
                                    new(width, -height),
                                    new(-width, -height),
                                    new(-width, height),
                                    new(width, height),
                                    new(width, -height)
                                ],
                                layer * 10,
                                255
                            );
                        }
                    }
                    break;

                    case TileBody b:
                    {
                        var missingTexture = GLOBALS.Textures.MissingTile;
                    
                        var (hx, hy, hz) = b.HeadPosition;

                        var supposedHead = GLOBALS.Level.TileMatrix[hy - 1, hx - 1, hz - 1];

                        if (supposedHead.Data is TileHead { Definition: null } or not TileHead)
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
                    break;

                    case TileMaterial m:
                        DrawMaterialTexture(m.Name, GLOBALS.Level.GeoMatrix[y, x, layer].Geo, x, y, scale, 255);

                    break;

                    case TileDefault:
                        DrawMaterialTexture(GLOBALS.Level.DefaultMaterial, GLOBALS.Level.GeoMatrix[y, x, layer].Geo, x, y, scale, 255);
                    break;
                }
            }
        }
    
        EndTextureMode();
    }

    internal static void DrawPropLayer(int layer, bool tinted)
    {
        var scopeNear = -layer * 10;
        var scopeFar = -(layer*10 + 9);
        
        foreach (var current in GLOBALS.Level.Props)
        {
            // Filter based on depth
            if (current.prop.Depth > scopeNear || current.prop.Depth < scopeFar) continue;

            var (category, index) = current.position;
            
            DrawProp(current.type, current.tile, category, index, current.prop, tinted);
            
            // Draw Rope Point
            if (current.type != InitPropType.Rope) continue;
            
            foreach (var point in current.prop.Extras.RopePoints)
                DrawCircleV(point, 3f, Color.White);
        }
    }
    
    internal static void DrawPropLayer(int layer, bool tinted, int scale)
    {
        var scopeNear = -layer * 10;
        var scopeFar = -(layer*10 + 9);
        
        foreach (var current in GLOBALS.Level.Props)
        {
            // Filter based on depth
            if (current.prop.Depth > scopeNear || current.prop.Depth < scopeFar) continue;

            var (category, index) = current.position;
            
             
            
            DrawProp(current.type, current.tile, category, index, current.prop, scale, tinted);
            
            // Draw Rope Point
            if (current.type != InitPropType.Rope) continue;
            
            foreach (var point in current.prop.Extras.RopePoints)
                DrawCircleV(point * (scale / 16f), 3f, Color.White);
        }
    }

    internal static void DrawPropLayer(int layer, PropDrawMode drawMode, Texture2D? palette, int scale)
    {
        var scopeNear = -layer * 10;
        var scopeFar = -(layer*10 + 9);
        
        foreach (var current in GLOBALS.Level.Props)
        {
            // Filter based on depth
            if (current.prop.Depth > scopeNear || current.prop.Depth < scopeFar) continue;

            var (category, index) = current.position;
            
            DrawProp(current.type, current.tile, category, index, current.prop, scale, drawMode, palette);
            
            // Draw Rope Point
            if (current.type != InitPropType.Rope) continue;
            
            foreach (var point in current.prop.Extras.RopePoints)
                DrawCircleV(point * (scale / 16f), 3f, Color.White);
        }
    }

    internal readonly struct DrawLevelParams()
    {
        internal int CurrentLayer { get; init; }
        internal bool GeometryLayer1 { get; init; } = true;
        internal bool GeometryLayer2 { get; init; } = true;
        internal bool GeometryLayer3 { get; init; } = true;
        internal bool TilesLayer1 { get; init; } = true;
        internal bool TilesLayer2 { get; init; } = true;
        internal bool TilesLayer3 { get; init; } = true;
        internal bool PropsLayer1 { get; init; } = true;
        internal bool PropsLayer2 { get; init; } = true;
        internal bool PropsLayer3 { get; init; } = true;
        internal TileDrawMode TileDrawMode { get; init; } = TileDrawMode.Preview;
        internal PropDrawMode PropDrawMode { get; init; } = PropDrawMode.Untinted;
        internal int Scale { get; init; } = 20;
        internal bool DarkTheme { get; init; } = false;
        internal bool Water { get; init; } = false;
        internal bool WaterAtFront { get; init; } = true;
        internal bool Grid { get; init; } = false;
        internal Texture2D? Palette { get; init; } = null;
        internal bool HighLayerContrast { get; init; } = true;
        internal bool VisiblePreceedingUnfocusedLayers { get; init; } = true;
    }
    
    internal static void DrawLevelIntoBuffer(in RenderTexture2D texture, DrawLevelParams parameters)
    {
        var geoL = LoadRenderTexture(GLOBALS.Level.Width * 20, GLOBALS.Level.Height * 20);

        BeginTextureMode(texture);
        ClearBackground(new(170, 170, 170, 255));
        EndTextureMode();

        if (parameters.GeometryLayer3)
        {
            if (parameters.TileDrawMode == TileDrawMode.Palette) {
                DrawGeoLayerWithMaterialsIntoBuffer(geoL, 2, parameters.Scale, true);
                // if (parameters.RenderMaterials) {
                // } else {
                //     BeginTextureMode(geoL);
                //     ClearBackground(Color.White with { A = 0 });
                    
                //     DrawGeoLayer(
                //         2, 
                //         parameters.Scale, 
                //         false, 
                //         Color.Green
                //     );
                //     EndTextureMode();
                // }


                BeginTextureMode(texture);

                var shader = GLOBALS.Shaders.Palette;
                BeginShaderMode(shader);

                var textureLoc = GetShaderLocation(shader, "inputTexture");
                var paletteLoc = GetShaderLocation(shader, "paletteTexture");

                var depthLoc = GetShaderLocation(shader, "depth");
                // var shadingLoc = GetShaderLocation(shader, "shading");

                SetShaderValueTexture(shader, textureLoc, geoL.Texture);
                SetShaderValueTexture(shader, paletteLoc, parameters.Palette!.Value);

                SetShaderValue(shader, depthLoc, 20, ShaderUniformDataType.Int);
                // SetShaderValue(shader, shadingLoc, 1, ShaderUniformDataType.Int);

                if (parameters.HighLayerContrast) {
                    DrawTexture(geoL.Texture, 0, 0, parameters.CurrentLayer == 2 ? Color.Black : Color.Black with { A = 120 });
                } else {
                    DrawTexture(geoL.Texture, 0, 0, Color.Black);
                }

                EndShaderMode();

                EndTextureMode();
            } else {
                BeginTextureMode(texture);

                if (parameters.HighLayerContrast) {
                    DrawGeoLayer(
                        2, 
                        parameters.Scale, 
                        false, 
                        parameters.CurrentLayer == 2 ? Color.Black : Color.Black with { A = 120 }
                    );
                } else {
                    DrawGeoLayer(
                        2, 
                        parameters.Scale, 
                        false, 
                        Color.Black
                    );
                }

                EndTextureMode();
            }

        }
        
        if (parameters.TilesLayer3)
        {
            if (parameters.TileDrawMode == TileDrawMode.Palette) {
                DrawTileLayerWithPaletteIntoBuffer(texture, parameters.CurrentLayer, 2, parameters.Scale, parameters.Palette!.Value, (byte)(parameters.HighLayerContrast ? 70 : 255), true, true);
            } else {
                BeginTextureMode(texture);
                if (parameters.HighLayerContrast) {
                    DrawTileLayer(
                        parameters.CurrentLayer,
                        2, 
                        parameters.Scale, 
                        false, 
                        parameters.TileDrawMode,
                        70
                    );
                } else {
                    DrawTileLayer(
                        parameters.CurrentLayer,
                        2, 
                        parameters.Scale, 
                        false, 
                        parameters.TileDrawMode,
                        255
                    );
                }
                EndTextureMode();
            }
        }

        if (parameters.PropsLayer3)
        {
            BeginTextureMode(texture);
            DrawPropLayer(2, parameters.PropDrawMode, parameters.Palette, parameters.Scale);
            EndTextureMode();
        }

        
        // Layer 2

        if (parameters.VisiblePreceedingUnfocusedLayers || parameters.CurrentLayer is 0 or 1) {
            if (parameters.GeometryLayer2)
            {
                if (parameters.TileDrawMode == TileDrawMode.Palette) {
                    DrawGeoLayerWithMaterialsIntoBuffer(geoL, 1, parameters.Scale, true);
                    // if (parameters.RenderMaterials) {
                    // } else {
                    //     BeginTextureMode(geoL);
                    //     ClearBackground(Color.White with { A = 0 });
                        
                    //     DrawGeoLayer(
                    //         1, 
                    //         parameters.Scale, 
                    //         false, 
                    //         Color.Green
                    //     );
                    //     EndTextureMode();
                    // }


                    BeginTextureMode(texture);

                    var shader = GLOBALS.Shaders.Palette;

                    BeginShaderMode(shader);

                    var textureLoc = GetShaderLocation(shader, "inputTexture");
                    var paletteLoc = GetShaderLocation(shader, "paletteTexture");

                    var depthLoc = GetShaderLocation(shader, "depth");
                    // var shadingLoc = GetShaderLocation(shader, "shading");

                    SetShaderValueTexture(shader, textureLoc, geoL.Texture);
                    SetShaderValueTexture(shader, paletteLoc, parameters.Palette!.Value);

                    SetShaderValue(shader, depthLoc, 10, ShaderUniformDataType.Int);
                    // SetShaderValue(shader, shadingLoc, 1, ShaderUniformDataType.Int);

                    if (parameters.HighLayerContrast) {
                        DrawTexture(geoL.Texture, 0, 0, parameters.CurrentLayer == 1 ? Color.Black : Color.Black with { A = 140 });
                    } else {
                        DrawTexture(geoL.Texture, 0, 0, Color.Black);
                    }

                    EndShaderMode();

                    EndTextureMode();
                } else {
                    BeginTextureMode(texture);

                    if (parameters.HighLayerContrast) {
                        DrawGeoLayer(
                            1, 
                            parameters.Scale, 
                            false, 
                            parameters.CurrentLayer == 1
                                ? Color.Black 
                                : Color.Black with { A = 140 }
                        );
                    } else {
                        DrawGeoLayer(
                            1, 
                            parameters.Scale, 
                            false, 
                            Color.Black
                        );
                    }


                    EndTextureMode();
                }
            }
            
            if (parameters.TilesLayer2)
            {
                if (parameters.TileDrawMode == TileDrawMode.Palette) {
                    DrawTileLayerWithPaletteIntoBuffer(texture, parameters.CurrentLayer, 1, parameters.Scale, parameters.Palette!.Value, (byte)(parameters.HighLayerContrast ? 70 : 255), true, true);

                    // if (parameters.HighLayerContrast) {
                    //     DrawTileLayer(
                    //         parameters.CurrentLayer,
                    //         1, 
                    //         parameters.Scale, 
                    //         false, 
                    //         parameters.TileDrawMode,
                    //         parameters.Palette!.Value,
                    //         70
                    //     );
                    // } else {
                    //     DrawTileLayer(
                    //         parameters.CurrentLayer,
                    //         1, 
                    //         parameters.Scale, 
                    //         false, 
                    //         parameters.TileDrawMode,
                    //         parameters.Palette!.Value,
                    //         255
                    //     );
                    // }
                } else {    
                    BeginTextureMode(texture);
                    if (parameters.HighLayerContrast) {
                        DrawTileLayer(
                            parameters.CurrentLayer,
                            1, 
                            parameters.Scale, 
                            false, 
                            parameters.TileDrawMode,
                            70
                        );
                    } else {
                        DrawTileLayer(
                            parameters.CurrentLayer,
                            1, 
                            parameters.Scale, 
                            false, 
                            parameters.TileDrawMode,
                            255
                        );
                    }
                    EndTextureMode();
                }
            }
            
            BeginTextureMode(texture);
            
            if (parameters.PropsLayer2)
            {

                DrawPropLayer(1, parameters.PropDrawMode, parameters.Palette, parameters.Scale);
            }
            
            //

            if (parameters.Water)
            {
                if (!parameters.WaterAtFront && GLOBALS.Level.WaterLevel > -1)
                {
                    DrawRectangle(
                        (-1) * parameters.Scale,
                        (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel - GLOBALS.Level.Padding.bottom) * parameters.Scale,
                        (GLOBALS.Level.Width + 2) * parameters.Scale,
                        (GLOBALS.Level.WaterLevel + GLOBALS.Level.Padding.bottom) * parameters.Scale,
                        new Color(0, 0, 255, 110)
                    );
                }
            }

            EndTextureMode();
        }
        

        // Layer 1

        if (parameters.VisiblePreceedingUnfocusedLayers || parameters.CurrentLayer == 0) {

            if (parameters.GeometryLayer1)
            {
                if (parameters.TileDrawMode == TileDrawMode.Palette) {
                    DrawGeoLayerWithMaterialsIntoBuffer(geoL, 0, parameters.Scale, true);

                    var shader = GLOBALS.Shaders.Palette;

                    BeginTextureMode(texture);

                    BeginShaderMode(shader);

                    var textureLoc = GetShaderLocation(shader, "inputTexture");
                    var paletteLoc = GetShaderLocation(shader, "paletteTexture");

                    var depthLoc = GetShaderLocation(shader, "depth");

                    SetShaderValueTexture(shader, textureLoc, geoL.Texture);
                    SetShaderValueTexture(shader, paletteLoc, parameters.Palette!.Value);

                    SetShaderValue(shader, depthLoc, 0, ShaderUniformDataType.Int);

                    if (parameters.HighLayerContrast) {
                        DrawTexture(geoL.Texture, 0, 0, parameters.CurrentLayer == 0 ? Color.Black : Color.Black with { A = 120 });
                    } else {
                        DrawTexture(geoL.Texture, 0, 0, Color.Black);
                    }

                    EndShaderMode();

                    EndTextureMode();

                    // DrawGeoLayerWithPalette(0, parameters.Scale, parameters.Palette!.Value, true);
                } else {
                    BeginTextureMode(texture);

                    if (parameters.HighLayerContrast) {
                        DrawGeoLayer(
                            0, 
                            parameters.Scale, 
                            false, 
                            parameters.CurrentLayer == 0
                                ? Color.Black 
                                : Color.Black with { A = 120 }
                        );
                    } else {
                        DrawGeoLayer(
                            0, 
                            parameters.Scale, 
                            false, 
                            Color.Black
                        );
                    }


                    EndTextureMode();
                }
            }

            
            if (parameters.TilesLayer1)
            {
                if (parameters.TileDrawMode == TileDrawMode.Palette) {
                    DrawTileLayerWithPaletteIntoBuffer(texture, parameters.CurrentLayer, 0, parameters.Scale, parameters.Palette!.Value, (byte)(parameters.HighLayerContrast ? 70 : 255), true, true);

                    // if (parameters.HighLayerContrast) {
                    //     DrawTileLayer(
                    //         parameters.CurrentLayer,
                    //         0, 
                    //         parameters.Scale, 
                    //         false, 
                    //         parameters.TileDrawMode,
                    //         parameters.Palette!.Value,
                    //         70
                    //     );
                    // } else {
                    //     DrawTileLayer(
                    //         parameters.CurrentLayer,
                    //         0, 
                    //         parameters.Scale, 
                    //         false, 
                    //         parameters.TileDrawMode,
                    //         parameters.Palette!.Value,
                    //         255
                    //     );
                    // }
                    
                } else {
                    BeginTextureMode(texture);
                    if (parameters.HighLayerContrast) {
                        DrawTileLayer(
                            parameters.CurrentLayer,
                            0, 
                            parameters.Scale, 
                            false, 
                            parameters.TileDrawMode,
                            70
                        );
                    } else {
                        DrawTileLayer(
                            parameters.CurrentLayer,
                            0, 
                            parameters.Scale, 
                            false, 
                            parameters.TileDrawMode,
                            255
                        );
                    }
                    EndTextureMode();
                }
            }
            
            BeginTextureMode(texture);

            if (parameters.PropsLayer1)
            {
                DrawPropLayer(0, parameters.PropDrawMode, parameters.Palette, parameters.Scale);
            }

            if (parameters.Water)
            {
                if (parameters.WaterAtFront && GLOBALS.Level.WaterLevel != -1)
                {
                    DrawRectangle(
                        (-1) * GLOBALS.Scale,
                        (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel - GLOBALS.Level.Padding.bottom) * GLOBALS.Scale,
                        (GLOBALS.Level.Width + 2) * GLOBALS.Scale,
                        (GLOBALS.Level.WaterLevel + GLOBALS.Level.Padding.bottom) * GLOBALS.Scale,
                        GLOBALS.Settings.GeneralSettings.DarkTheme 
                            ? GLOBALS.DarkThemeWaterColor 
                            : GLOBALS.LightThemeWaterColor
                    );
                }
            }

            if (parameters.Grid) DrawGrid(parameters.Scale);
            
            EndTextureMode();
        }
        

        UnloadRenderTexture(geoL);
    }

    internal static void DrawLevelIntoBufferV2(in RenderTexture2D texture, DrawLevelParams parameters)
    {
        var geoL = LoadRenderTexture(GLOBALS.Level.Width * 20, GLOBALS.Level.Height * 20);

        void ApplyPaletteIntoLevel(in RenderTexture2D t, int depth, byte opacity) {
            BeginTextureMode(t);

            var shader = GLOBALS.Shaders.Palette;
            BeginShaderMode(shader);

            var textureLoc = GetShaderLocation(shader, "inputTexture");
            var paletteLoc = GetShaderLocation(shader, "paletteTexture");

            var depthLoc = GetShaderLocation(shader, "depth");
            // var shadingLoc = GetShaderLocation(shader, "shading");

            SetShaderValueTexture(shader, textureLoc, geoL.Texture);
            SetShaderValueTexture(shader, paletteLoc, parameters.Palette!.Value);

            SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
            // SetShaderValue(shader, shadingLoc, 1, ShaderUniformDataType.Int);

            DrawTexture(geoL.Texture, 0, 0, Color.White with { A = opacity });

            EndShaderMode();

            EndTextureMode();
        }

        BeginTextureMode(texture);
        ClearBackground(new(170, 170, 170, 255));
        EndTextureMode();

        if (parameters.GeometryLayer3)
        {
            if (parameters.TileDrawMode == TileDrawMode.Palette) {
                if (!parameters.TilesLayer3) {
                    DrawGeoLayerWithMaterialsIntoBuffer(geoL, 2, 20, true);
                    ApplyPaletteIntoLevel(texture, 20, (byte)(!parameters.HighLayerContrast || parameters.CurrentLayer == 2 ? 255 : 120));
                }
            } else {
                BeginTextureMode(texture);

                if (parameters.HighLayerContrast) {
                    DrawGeoLayer(
                        2, 
                        parameters.Scale, 
                        false, 
                        parameters.CurrentLayer == 2
                            ? Color.Black 
                            : Color.Black with { A = 120 }
                    );
                } else {
                    DrawGeoLayer(
                        2, 
                        parameters.Scale, 
                        false, 
                        Color.Black
                    );
                }


                EndTextureMode();
            }
        }
        
        if (parameters.TilesLayer3)
        {
            if (parameters.TileDrawMode == TileDrawMode.Palette) {
                DrawTileLayerWithMaterialTexturesIntoBuffer(geoL, 2, 20);

                // Draw into the main level buffer

                ApplyPaletteIntoLevel(texture, 20, (byte)(!parameters.HighLayerContrast || parameters.CurrentLayer == 2 ? 255 : 120));
            } else {
                BeginTextureMode(texture);
                if (parameters.HighLayerContrast) {
                    DrawTileLayer(
                        parameters.CurrentLayer,
                        2, 
                        parameters.Scale, 
                        false, 
                        parameters.TileDrawMode,
                        70
                    );
                } else {
                    DrawTileLayer(
                        parameters.CurrentLayer,
                        2, 
                        parameters.Scale, 
                        false, 
                        parameters.TileDrawMode,
                        255
                    );
                }
                EndTextureMode();
            }
        }

        if (parameters.PropsLayer3)
        {
            BeginTextureMode(texture);
            DrawPropLayer(2, parameters.PropDrawMode, parameters.Palette, parameters.Scale);
            EndTextureMode();
        }

        
        // Layer 2

        if (parameters.VisiblePreceedingUnfocusedLayers || parameters.CurrentLayer is 0 or 1) {
            if (parameters.GeometryLayer2)
            {
                if (parameters.TileDrawMode == TileDrawMode.Palette) {
                    if (!parameters.TilesLayer2) {
                        DrawGeoLayerWithMaterialsIntoBuffer(geoL, 1, 20, true);
                        ApplyPaletteIntoLevel(texture, 10, (byte)(!parameters.HighLayerContrast || parameters.CurrentLayer == 1 ? 255 : 120));
                    }
                } else {
                    BeginTextureMode(texture);

                    if (parameters.HighLayerContrast) {
                        DrawGeoLayer(
                            1, 
                            parameters.Scale, 
                            false, 
                            parameters.CurrentLayer == 1
                                ? Color.Black 
                                : Color.Black with { A = 120 }
                        );
                    } else {
                        DrawGeoLayer(
                            1, 
                            parameters.Scale, 
                            false, 
                            Color.Black
                        );
                    }


                    EndTextureMode();
                }
            }
            
            if (parameters.TilesLayer2)
            {
                if (parameters.TileDrawMode == TileDrawMode.Palette) {
                    DrawTileLayerWithMaterialTexturesIntoBuffer(geoL, 1, 20);

                    // Draw into the main level buffer

                    ApplyPaletteIntoLevel(texture, 10, (byte)(!parameters.HighLayerContrast || parameters.CurrentLayer == 1 ? 255 : 120));

                } else {    
                    BeginTextureMode(texture);
                    if (parameters.HighLayerContrast) {
                        DrawTileLayer(
                            parameters.CurrentLayer,
                            1, 
                            parameters.Scale, 
                            false, 
                            parameters.TileDrawMode,
                            70
                        );
                    } else {
                        DrawTileLayer(
                            parameters.CurrentLayer,
                            1, 
                            parameters.Scale, 
                            false, 
                            parameters.TileDrawMode,
                            255
                        );
                    }
                    EndTextureMode();
                }
            }
            
            BeginTextureMode(texture);
            
            if (parameters.PropsLayer2)
            {

                DrawPropLayer(1, parameters.PropDrawMode, parameters.Palette, parameters.Scale);
            }
            
            //

            if (parameters.Water)
            {
                if (!parameters.WaterAtFront && GLOBALS.Level.WaterLevel > -1)
                {
                    DrawRectangle(
                        (-1) * parameters.Scale,
                        (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel - GLOBALS.Level.Padding.bottom) * parameters.Scale,
                        (GLOBALS.Level.Width + 2) * parameters.Scale,
                        (GLOBALS.Level.WaterLevel + GLOBALS.Level.Padding.bottom) * parameters.Scale,
                        new Color(0, 0, 255, 110)
                    );
                }
            }

            EndTextureMode();
        }
        

        // Layer 1

        if (parameters.VisiblePreceedingUnfocusedLayers || parameters.CurrentLayer == 0) {

            if (parameters.GeometryLayer1)
            {
                if (parameters.TileDrawMode == TileDrawMode.Palette) {
                    if (!parameters.TilesLayer1) {
                        DrawGeoLayerWithMaterialsIntoBuffer(geoL, 0, 20, true);
                        ApplyPaletteIntoLevel(texture, 0, (byte)(!parameters.HighLayerContrast || parameters.CurrentLayer == 0 ? 255 : 120));
                    }
                } else {
                    BeginTextureMode(texture);

                    if (parameters.HighLayerContrast) {
                        DrawGeoLayer(
                            0, 
                            parameters.Scale, 
                            false, 
                            parameters.CurrentLayer == 0
                                ? Color.Black 
                                : Color.Black with { A = 120 }
                        );
                    } else {
                        DrawGeoLayer(
                            0, 
                            parameters.Scale, 
                            false, 
                            Color.Black
                        );
                    }


                    EndTextureMode();
                }
            }

            
            if (parameters.TilesLayer1)
            {
                if (parameters.TileDrawMode == TileDrawMode.Palette) {
                    DrawTileLayerWithMaterialTexturesIntoBuffer(geoL, 0, 20);

                    // Draw into the main level buffer

                    ApplyPaletteIntoLevel(texture, 0, (byte)(!parameters.HighLayerContrast || parameters.CurrentLayer == 0 ? 255 : 120));
                    
                } else {
                    BeginTextureMode(texture);
                    if (parameters.HighLayerContrast) {
                        DrawTileLayer(
                            parameters.CurrentLayer,
                            0, 
                            parameters.Scale, 
                            false, 
                            parameters.TileDrawMode,
                            70
                        );
                    } else {
                        DrawTileLayer(
                            parameters.CurrentLayer,
                            0, 
                            parameters.Scale, 
                            false, 
                            parameters.TileDrawMode,
                            255
                        );
                    }
                    EndTextureMode();
                }
            }
            
            BeginTextureMode(texture);

            if (parameters.PropsLayer1)
            {
                DrawPropLayer(0, parameters.PropDrawMode, parameters.Palette, parameters.Scale);
            }

            if (parameters.Water)
            {
                if (parameters.WaterAtFront && GLOBALS.Level.WaterLevel != -1)
                {
                    DrawRectangle(
                        (-1) * GLOBALS.Scale,
                        (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel - GLOBALS.Level.Padding.bottom) * GLOBALS.Scale,
                        (GLOBALS.Level.Width + 2) * GLOBALS.Scale,
                        (GLOBALS.Level.WaterLevel + GLOBALS.Level.Padding.bottom) * GLOBALS.Scale,
                        GLOBALS.Settings.GeneralSettings.DarkTheme 
                            ? GLOBALS.DarkThemeWaterColor 
                            : GLOBALS.LightThemeWaterColor
                    );
                }
            }

            if (parameters.Grid) DrawGrid(parameters.Scale);
            
            EndTextureMode();
        }
        

        UnloadRenderTexture(geoL);
    }


    /// <summary>
    /// Draws four lines that intersect the cursor and extend all the way across the level
    /// </summary>
    /// <param name="matrixX">The X-coordinates of the cursor</param>
    /// <param name="matrixY">The Y-coordinates of the cursor</param>
    /// <param name="thickness">The thickness of the lines</param>
    /// <param name="radius">The distance between the single line and the cursor in matrix units</param>
    /// <param name="color">The color of the lines</param>
    internal static void DrawLevelIndexHintsHollow(
        int matrixX, 
        int matrixY, 
        float thickness, 
        int radius, 
        Color color)
    {
        DrawLineEx(new Vector2(0, (matrixY - radius)*20), new Vector2(GLOBALS.Level.Width*20, (matrixY - radius)*20), thickness, color);
        DrawLineEx(new Vector2(0, (matrixY + radius + 1)*20), new Vector2(GLOBALS.Level.Width*20, (matrixY + radius + 1)*20), thickness, color);
                    
        DrawLineEx(new Vector2((matrixX - radius)*20, 0), new Vector2((matrixX - radius)*20, GLOBALS.Level.Height*20), thickness, color);
        DrawLineEx(new Vector2((matrixX + radius + 1)*20, 0), new Vector2((matrixX + radius + 1)*20, GLOBALS.Level.Height*20), thickness, color);
    }

    internal static void DrawSplashScreen(bool version)
    {
        ClearBackground(Color.Black);

        DrawTexturePro(
            GLOBALS.Textures.SplashScreen,
            new Rectangle(0, 0, GLOBALS.Textures.SplashScreen.Width, GLOBALS.Textures.SplashScreen.Height),
            new Rectangle(0, 0, GLOBALS.MinScreenWidth, GLOBALS.MinScreenHeight),
            new Vector2(0, 0),
            0,
            Color.White
        );

        if (!version) return;
        
        if (GLOBALS.Font is null) DrawText(GLOBALS.Version, 700, 50, 15, Color.White);
        else DrawTextPro(GLOBALS.Font.Value, GLOBALS.Version, new(700, 50), new(0, 0), 0, 30, 0, Color.White);

        if (GLOBALS.Font is null) DrawText(GLOBALS.RaylibVersion, 700, 70, 15, Color.White);
        else DrawTextPro(GLOBALS.Font.Value, GLOBALS.RaylibVersion, new(700, 80), new(0, 0), 0, 30, 0, Color.White);

        if (GLOBALS.Font is null) DrawText(GLOBALS.BuildConfiguration, 700, 90, 15, Color.White);
        else DrawTextPro(GLOBALS.Font.Value, GLOBALS.BuildConfiguration, new(700, 110), new(0, 0), 0, 30, 0, Color.White);
    }

    

    internal static RL.Managed.Image GenerateLevelReviewImage() 
    {
        const int scale = 4;

        var renderTexture = LoadRenderTexture(GLOBALS.Level.Width * scale, GLOBALS.Level.Height * scale);

        BeginTextureMode(renderTexture);
        ClearBackground(Color.White);
        DrawGeoLayer(
                2, 
                scale, 
                false, 
                Color.Gray
            );
        DrawGeoLayer(
                1, 
                scale, 
                false, 
                Color.Black
            );

        if (!GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel > -1)
        {
            DrawRectangle(
                (-1) * scale,
                (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel - GLOBALS.Level.Padding.bottom) * scale,
                (GLOBALS.Level.Width + 2) * scale,
                (GLOBALS.Level.WaterLevel + GLOBALS.Level.Padding.bottom) * scale,
                new Color(0, 0, 255, 110)
            );
        }

        DrawRectangle(
            0, 
            0, 
            GLOBALS.Level.Width * scale, 
            GLOBALS.Level.Height * scale, 
            Color.Gray with { A = 130 }
        );

        DrawGeoLayer(
            0, 
            scale, 
            false, 
            Color.Black
        );

        if (GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1)
        {
            DrawRectangle(
                (-1) * scale,
                (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel - GLOBALS.Level.Padding.bottom) * scale,
                (GLOBALS.Level.Width + 2) * scale,
                (GLOBALS.Level.WaterLevel + GLOBALS.Level.Padding.bottom) * scale,
                GLOBALS.Settings.GeneralSettings.DarkTheme 
                    ? GLOBALS.DarkThemeWaterColor 
                    : GLOBALS.LightThemeWaterColor
            );
        }
        EndTextureMode();

        var img = LoadImageFromTexture(renderTexture.Texture);

        ImageFlipVertical(ref img);

        UnloadRenderTexture(renderTexture);

        return new(img);
    }

    internal static RL.Managed.Image GenerateLevelReviewImage(RunCell[,,] matrix) 
    {
        const int scale = 4;

        var width = matrix.GetLength(1);
        var height = matrix.GetLength(0);

        var renderTexture = LoadRenderTexture(width * scale, height * scale);

        BeginTextureMode(renderTexture);
        ClearBackground(Color.White);
        DrawGeoLayer(
            matrix,
            2, 
            scale, 
            false, 
            Color.Gray
            );
        DrawGeoLayer(
            matrix,
            1, 
            scale, 
            false, 
            Color.Black
            );

        DrawRectangle(
            0, 
            0, 
            width * scale, 
            height * scale, 
            Color.Gray with { A = 130 }
        );

        DrawGeoLayer(
            matrix,
            0, 
            scale, 
            false, 
            Color.Black
        );

        EndTextureMode();

        var img = LoadImageFromTexture(renderTexture.Texture);

        ImageFlipVertical(ref img);

        UnloadRenderTexture(renderTexture);

        return new(img);
    }
    
    internal static RL.Managed.Texture2D GenerateLevelReviewTexture(RunCell[,,] matrix) 
    {
        const int scale = 4;

        var width = matrix.GetLength(1);
        var height = matrix.GetLength(0);

        var renderTexture = LoadRenderTexture(width * scale, height * scale);

        BeginTextureMode(renderTexture);
        ClearBackground(Color.White);
        DrawGeoLayer(
            matrix,
            2, 
            scale, 
            false, 
            Color.Gray
            );
        DrawGeoLayer(
            matrix,
            1, 
            scale, 
            false, 
            Color.Black
            );

        DrawRectangle(
            0, 
            0, 
            width * scale, 
            height * scale, 
            Color.Gray with { A = 130 }
        );

        DrawGeoLayer(
            matrix,
            0, 
            scale, 
            false, 
            Color.Black
        );

        EndTextureMode();

        var img = LoadImageFromTexture(renderTexture.Texture);

        ImageFlipVertical(ref img);

        RL.Managed.Texture2D finalTexture = new(img);

        UnloadImage(img);

        UnloadRenderTexture(renderTexture);

        return finalTexture;
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

    internal static void DrawTextureTriangle(
        in Texture2D texture,
        Vector2 s1,
        Vector2 s2,
        Vector2 s3,
        Vector2 p1,
        Vector2 p2,
        Vector2 p3,
        Color tint
    )
    {
        var tz = new Vector2(texture.Width, texture.Height);

        Rlgl.SetTexture(texture.Id);

        Rlgl.Begin(0x0007);
        Rlgl.Color4ub(tint.R, tint.G, tint.B, tint.A);

        Rlgl.TexCoord2f(s1.X, s1.Y);
        Rlgl.Vertex2f(p1.X, p1.Y);
        
        Rlgl.TexCoord2f(s2.X, s2.Y);
        Rlgl.Vertex2f(p2.X, p2.Y);
        
        Rlgl.TexCoord2f(s3.X, s3.Y);
        Rlgl.Vertex2f(p3.X, p3.Y);
        
        Rlgl.TexCoord2f(s1.X, s1.Y);
        Rlgl.Vertex2f(p1.X, p1.Y);
        
        Rlgl.End();

        Rlgl.SetTexture(0);
    }
    
    internal static void DrawTextureQuad(
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
    
    internal static void DrawTextureQuad(
        in Texture2D texture, 
        in QuadVectors quads
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
    
    internal static void DrawTextureQuad(
        in Texture2D texture, 
        in QuadVectors quads,
        in Color tint
    )
    {
        Rlgl.SetTexture(texture.Id);
        
        Rlgl.Begin(0x0007);
        Rlgl.Color4ub(tint.R, tint.G, tint.B, tint.A);

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
    
    // TODO: Unfinished
    internal static unsafe void DrawTextureSmoothQuad(
        in Texture2D texture, 
        in QuadVectors quads,
        in Color tint
    )
    {
        Rlgl.SetTexture(texture.Id);
     
        Rlgl.TextureParameters(texture.Id, Rlgl.TEXTURE_MIN_FILTER, Rlgl.TEXTURE_FILTER_MIP_LINEAR);
        Rlgl.TextureParameters(texture.Id, Rlgl.TEXTURE_MAG_FILTER, Rlgl.TEXTURE_FILTER_LINEAR);
        
        Rlgl.Begin(0x0007);
        Rlgl.Color4ub(tint.R, tint.G, tint.B, tint.A);

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
    
    internal static void DrawTextureQuad(
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

    internal static void DrawTextureQuad(
        in Texture2D texture, 
        in PropQuads quads,
        Color tint,
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
        Rlgl.Color4ub(tint.R, tint.G, tint.B, tint.A);

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
    
    #nullable enable
    
    internal static void DrawTilePreview(
        in TileDefinition? init, 
        in Color color, 
        in (int x, int y) position,
        in int scale
    )
    {
        if (init is null) return;
        
        var texture = init.Texture;
        
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
            new Rectangle(0, 0, texture.Width, texture.Height),
            new Rectangle(position.x * scale, position.y * scale, init.Size.Item1 * scale, init.Size.Item2 * scale),
            Raymath.Vector2Scale(Utils.GetTileHeadOrigin(init), scale),
            0,
            Color.White
        );
        
        EndShaderMode();
    }
    
    /// <summary>
    /// Draws tile preview texture
    /// </summary>
    /// <param name="init"></param>
    /// <param name="texture"></param>
    /// <param name="color"></param>
    /// <param name="position"></param>
    /// <param name="scale"></param>
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
            TopLeft = position * scale,
            TopRight = (position + new Vector2(init.Size.Item1, 0)) * scale,
            BottomRight = (position + new Vector2(init.Size.Item1, init.Size.Item2)) * scale,
            BottomLeft = (position + new Vector2(0, init.Size.Item2)) * scale
        };
        
        DrawTextureQuad(
            texture, 
            quads
        );
        
        EndShaderMode();
    }
    
    #nullable enable
    
    /// <summary>
    /// Draws tile preview texture
    /// </summary>
    /// <param name="init"></param>
    /// <param name="color"></param>
    /// <param name="position"></param>
    /// <param name="scale"></param>
    internal static void DrawTilePreview(
        in TileDefinition? init, 
        in Color color, 
        in Vector2 position,
        in int scale
    )
    {
        if (init is null) return;
        
        var texture = init.Texture;
        
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
            TopLeft = position * scale,
            TopRight = (position + new Vector2(init.Size.Item1, 0)) * scale,
            BottomRight = (position + new Vector2(init.Size.Item1, init.Size.Item2)) * scale,
            BottomLeft = (position + new Vector2(0, init.Size.Item2)) * scale
        };
        
        DrawTextureQuad(
            texture, 
            quads
        );
        
        EndShaderMode();
    }
    
    internal static void DrawTilePreview(
        in InitTile init, 
        in Texture2D texture, 
        in Color color, 
        in Vector2 position,
        in Vector2 offset,
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
            TopLeft = (position + offset) * scale,
            TopRight = (position + new Vector2(init.Size.Item1, 0) + offset) * scale,
            BottomRight = (position + new Vector2(init.Size.Item1, init.Size.Item2) + offset) * scale,
            BottomLeft = (position + new Vector2(0, init.Size.Item2) + offset) * scale
        };
        
        DrawTextureQuad(
            texture, 
            quads
        );
        
        EndShaderMode();
    }
    
    internal static void DrawTilePreview(
        in TileDefinition init, 
        in Color color, 
        in Vector2 position,
        in Vector2 offset,
        in int scale
    )
    {
        var texture = init.Texture;
        
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
            TopLeft = (position + offset) * scale,
            TopRight = (position + new Vector2(init.Size.Item1, 0) + offset) * scale,
            BottomRight = (position + new Vector2(init.Size.Item1, init.Size.Item2) + offset) * scale,
            BottomLeft = (position + new Vector2(0, init.Size.Item2) + offset) * scale
        };
        
        DrawTextureQuad(
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
    [Obsolete]
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
    /// <param name="init">a reference to the tile definition</param>
    /// <param name="center">the center origin of the target position to draw on</param>
    /// <param name="quads">target placement quads</param>
    /// <param name="alpha">opacity, from 0 to 255</param>
    [Obsolete]
    internal static void DrawTileAsProp(
        in TileDefinition init,
        in Vector2 center,
        Span<Vector2> quads,
        int alpha = 255
    )
    {
        var texture = init.Texture;
        
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
    /// <param name="rotation">rotation angle</param>
    [Obsolete]
    internal static void DrawTileAsProp(
        in Texture2D texture,
        in InitTile init,
        in Vector2 center,
        int rotation
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
        SetShaderValue(GLOBALS.Shaders.Prop, alphaLoc, 1.0f, ShaderUniformDataType.Float);

        var size = new Vector2(init.Size.Item1+init.BufferTiles*2, init.Size.Item2+init.BufferTiles*2)*8;
        
        var quads = new PropQuads(
            center - size,
            new Vector2(center.X + size.X, center.Y - size.Y),
            center + size,
            new Vector2(center.X - size.X, center.Y + size.Y)
        );

        quads = Utils.RotatePropQuads(quads, rotation, center);
        
        DrawTextureQuad(texture, quads);
        
        EndShaderMode();
    }
    
    /// <summary>
    /// Draws the texture of the tile, layer by layer, from the bottom up.
    /// </summary>
    /// <param name="texture">a reference to the tile texture</param>
    /// <param name="init">a reference to the tile definition</param>
    /// <param name="center">the center origin of the target position to draw on</param>
    /// <param name="rotation">rotation angle</param>
    internal static void DrawTileAsProp(
        in TileDefinition init,
        in Vector2 center,
        int rotation
    )
    {
        var texture = init.Texture;
        
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
        SetShaderValue(GLOBALS.Shaders.Prop, alphaLoc, 1.0f, ShaderUniformDataType.Float);

        var size = new Vector2(init.Size.Item1+init.BufferTiles*2, init.Size.Item2+init.BufferTiles*2)*8;
        
        var quads = new PropQuads(
            center - size,
            new Vector2(center.X + size.X, center.Y - size.Y),
            center + size,
            new Vector2(center.X - size.X, center.Y + size.Y)
        );

        quads = Utils.RotatePropQuads(quads, rotation, center);
        
        DrawTextureQuad(texture, quads);
        
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
        
        DrawTextureQuad(texture, scaledQuads);
        
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
        
        DrawTextureQuad(texture, quads);
        
        EndShaderMode();
    }
    
    /// <summary>
    /// Draws the texture of the tile, layer by layer, from the bottom up.
    /// </summary>
    /// <param name="init">a reference to the tile definition</param>
    /// <param name="quads">target placement quads</param>
    internal static void DrawTileAsProp(
        in TileDefinition init,
        PropQuads quads,
        int depth = 0,
        int alpha = 255
    )
    {
        var scale = GLOBALS.Scale;
        var texture = init.Texture;

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
        
        DrawTextureQuad(texture, quads);
        
        EndShaderMode();
    }
    
    /// <summary>
    /// Draws the texture of the tile, layer by layer, from the bottom up.
    /// </summary>
    /// <param name="texture">a reference to the tile texture</param>
    /// <param name="init">a reference to the tile definition</param>
    /// <param name="quads">target placement quads</param>
    internal static void DrawTileAsProp(
        in Texture2D texture,
        in InitTile init,
        in Vector2 position,
        in int depth = 0,
        in int alpha = 255
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
        var depthLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "depth");
        var alphaLoc = GetShaderLocation(GLOBALS.Shaders.Prop, "alpha");

        BeginShaderMode(GLOBALS.Shaders.Prop);

        SetShaderValueTexture(GLOBALS.Shaders.Prop, textureLoc, texture);
        SetShaderValue(GLOBALS.Shaders.Prop, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.Int);
        SetShaderValue(GLOBALS.Shaders.Prop, layerHeightLoc, calLayerHeight, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.Prop, layerWidthLoc, calTextureCutWidth, ShaderUniformDataType.Float);
        SetShaderValue(GLOBALS.Shaders.Prop, depthLoc, depth, ShaderUniformDataType.Int);
        SetShaderValue(GLOBALS.Shaders.Prop, alphaLoc, alpha/255f, ShaderUniformDataType.Float);
        
        DrawTextureV(texture, position, Color.White);
        
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
            var shader = GLOBALS.Shaders.ColoredBoxTileProp;

            var (tWidth, tHeight) = init.Size;
            var bufferPixels = init.BufferTiles * 20;
            
            var height = tHeight * 20;
            var offset = new Vector2(bufferPixels,tHeight*20*tWidth + bufferPixels);
            
            var calcHeight = (float)height / (float)texture.Height;
            var calcOffset = Raymath.Vector2Divide(offset, new(texture.Width, texture.Height));
            var calcWidth = (float)tWidth*20 / texture.Width;

            var textureLoc = GetShaderLocation(shader, "inputTexture");

            var widthLoc = GetShaderLocation(shader, "width");
            var heightLoc = GetShaderLocation(shader, "height");
            var offsetLoc = GetShaderLocation(shader, "offset");
            var colorLoc = GetShaderLocation(shader, "tint");
            var depthLoc = GetShaderLocation(shader, "depth");

            BeginShaderMode(shader);

            SetShaderValueTexture(shader, textureLoc, texture);
            
            SetShaderValue(shader, widthLoc, calcWidth, ShaderUniformDataType.Float);
            
            SetShaderValue(shader, heightLoc, calcHeight,
                ShaderUniformDataType.Float);
            
            SetShaderValue(shader, offsetLoc, calcOffset,
                ShaderUniformDataType.Vec2);
            
            SetShaderValue(shader, colorLoc,
                new Vector4(tint.R / 255f, tint.G / 255f, tint.B / 255f, tint.A / 255f),
                ShaderUniformDataType.Vec4);
            
            SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);

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
    
    // TODO: Fix opacity inconsistency
    internal static void DrawTileAsProp(
        in TileDefinition init, 
        in Vector2 center, 
        Span<Vector2> quads,
        int depth,
        byte opacity
    )
    {
        var scale = GLOBALS.Scale;
        var texture = init.Texture;

        if (init.Type == Data.Tiles.TileType.Box)
        {
            var shader = GLOBALS.Shaders.BoxTile;

            var (tWidth, tHeight) = init.Size;
            var bufferPixels = init.BufferTiles * 20;
            
            var height = tHeight * 20;
            var offset = new Vector2(bufferPixels,tHeight*20*tWidth + bufferPixels);
            
            var calcHeight = (float)height / (float)texture.Height;
            var calcOffset = Raymath.Vector2Divide(offset, new(texture.Width, texture.Height));
            var calcWidth = (float)tWidth*20 / texture.Width;

            var textureLoc = GetShaderLocation(shader, "inputTexture");

            var widthLoc = GetShaderLocation(shader, "width");
            var heightLoc = GetShaderLocation(shader, "height");
            var offsetLoc = GetShaderLocation(shader, "offset");
            var depthLoc = GetShaderLocation(shader, "depth");

            BeginShaderMode(shader);

            SetShaderValueTexture(shader, textureLoc, texture);
            
            SetShaderValue(shader, widthLoc, calcWidth, ShaderUniformDataType.Float);
            
            SetShaderValue(shader, heightLoc, calcHeight,
                ShaderUniformDataType.Float);
            
            SetShaderValue(shader, offsetLoc, calcOffset,
                ShaderUniformDataType.Vec2);
            
            SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);

            DrawTexturePoly(
                texture,
                center,
                quads,
                new Span<Vector2>([new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0)]),
                5,
                Color.White with { A = opacity }
            );
            EndShaderMode();
        }
        else
        {
            var shader = GLOBALS.Shaders.Prop;

            var layerHeight = (init.Size.Item2 + (init.BufferTiles * 2)) * scale;
            float calLayerHeight = (float)layerHeight / (float)texture.Height;
            var textureCutWidth = (init.Size.Item1 + (init.BufferTiles * 2)) * scale;
            float calTextureCutWidth = (float)textureCutWidth / (float)texture.Width;

            var textureLoc = GetShaderLocation(shader, "inputTexture");
            var layerNumLoc = GetShaderLocation(shader, "layerNum");
            var layerHeightLoc = GetShaderLocation(shader, "layerHeight");
            var layerWidthLoc = GetShaderLocation(shader, "layerWidth");
            var depthLoc = GetShaderLocation(shader, "depth");
            var alphaLoc = GetShaderLocation(shader, "alpha");

            BeginShaderMode(shader);

            SetShaderValueTexture(shader, textureLoc, texture);
            SetShaderValue(shader, layerNumLoc, init.Type == Data.Tiles.TileType.VoxelStructRockType ? 1 : init.Repeat.Length,
                ShaderUniformDataType.Int);
            SetShaderValue(shader, layerHeightLoc, calLayerHeight,
                ShaderUniformDataType.Float);
            SetShaderValue(shader, layerWidthLoc, calTextureCutWidth,
                ShaderUniformDataType.Float);
            
            SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
            SetShaderValue(shader, alphaLoc, opacity/255f, ShaderUniformDataType.Float);

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
    internal static void DrawTileAsPropColored(
        in TileDefinition init, 
        in Vector2 center, 
        Span<Vector2> quads,
        Color tint,
        int depth = 0
    )
    {
        var scale = GLOBALS.Scale;
        var texture = init.Texture;

        if (init.Type == Data.Tiles.TileType.Box)
        {
            var shader = GLOBALS.Shaders.ColoredBoxTileProp;

            var (tWidth, tHeight) = init.Size;
            var bufferPixels = init.BufferTiles * 20;
            
            var height = tHeight * 20;
            var offset = new Vector2(bufferPixels,tHeight*20*tWidth + bufferPixels);
            
            var calcHeight = (float)height / (float)texture.Height;
            var calcOffset = Raymath.Vector2Divide(offset, new(texture.Width, texture.Height));
            var calcWidth = (float)tWidth*20 / texture.Width;

            var textureLoc = GetShaderLocation(shader, "inputTexture");

            var widthLoc = GetShaderLocation(shader, "width");
            var heightLoc = GetShaderLocation(shader, "height");
            var offsetLoc = GetShaderLocation(shader, "offset");
            var colorLoc = GetShaderLocation(shader, "tint");
            var depthLoc = GetShaderLocation(shader, "depth");

            BeginShaderMode(shader);

            SetShaderValueTexture(shader, textureLoc, texture);
            
            SetShaderValue(shader, widthLoc, calcWidth, ShaderUniformDataType.Float);
            
            SetShaderValue(shader, heightLoc, calcHeight,
                ShaderUniformDataType.Float);
            
            SetShaderValue(shader, offsetLoc, calcOffset,
                ShaderUniformDataType.Vec2);
            
            SetShaderValue(shader, colorLoc,
                new Vector4(tint.R / 255f, tint.G / 255f, tint.B / 255f, tint.A / 255f),
                ShaderUniformDataType.Vec4);
            
            SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);

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
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, layerNumLoc, init.Type == Data.Tiles.TileType.VoxelStructRockType ? 1 : init.Repeat.Length,
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

            DrawTextureQuad(texture, quads);
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

            DrawTextureQuad(texture, quads);
            EndShaderMode();
        }
    }
    
    /// Same as DrawTileAsProp() except it applies a tint to the base texture
    internal static void DrawTileAsPropColored(
        in TileDefinition init, 
        PropQuads quads,
        Color tint,
        int depth = 0
    )
    {
        var scale = GLOBALS.Scale;
        var texture = init.Texture;

        if (init.Type == Data.Tiles.TileType.Box)
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

            DrawTextureQuad(texture, quads);
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
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, layerNumLoc, init.Type == Data.Tiles.TileType.VoxelStructRockType ? 1 : init.Repeat.Length,
                ShaderUniformDataType.Int);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, layerHeightLoc, calLayerHeight,
                ShaderUniformDataType.Float);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, layerWidthLoc, calTextureCutWidth,
                ShaderUniformDataType.Float);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, colorLoc,
                new Vector4(tint.R / 255f, tint.G / 255f, tint.B / 255f, 1.0f),
                ShaderUniformDataType.Vec4);
            SetShaderValue(GLOBALS.Shaders.ColoredTileProp, depthLoc, depth, ShaderUniformDataType.Int);

            DrawTextureQuad(texture, quads);
            EndShaderMode();
        }
    }
    
    /// Same as DrawTileAsProp() except it applies a tint to the base texture.
    /// 
    /// <para>Only used in the preview panel.</para>
    internal static void DrawTileAsPropColored(
        in Texture2D texture, 
        in InitTile init, 
        in Vector2 position,
        in Vector2 originOffset,
        in Color tint,
        in int depth,
        in int scale
    )
    {
        var scaledQuads = new PropQuads
        {
            TopLeft = position*scale - originOffset,
            TopRight = (position + new Vector2(init.Size.Item1, 0) * scale) - originOffset,
            BottomRight = (position + new Vector2(init.Size.Item1, init.Size.Item2)) * scale - originOffset,
            BottomLeft = (position + new Vector2(0, init.Size.Item2)) * scale - originOffset
        };
        
        if (init.Type == InitTileType.Box)
        {
            var (tWidth, tHeight) = init.Size;
            var bufferPixels = init.BufferTiles * 20;
            
            var height = tHeight * 20;
            var offset = new Vector2(bufferPixels,tHeight*20*tWidth + bufferPixels);
            
            var calcHeight = height / (float)texture.Height;
            var calcOffset = Raymath.Vector2Divide(offset, new Vector2(texture.Width, texture.Height));
            var calcWidth = (float)tWidth * 20 / texture.Width;
            
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
            
            DrawTextureQuad(texture, scaledQuads);
            EndShaderMode();
        }
        else
        {
            var shader = GLOBALS.Shaders.PreviewColoredTileProp;
            
            var layerHeight = init.Size.Item2 * 20;
            var calLayerHeight = (float)layerHeight / (float)texture.Height;
            var textureCutWidth = init.Size.Item1 * 20;
            var calTextureCutWidth = (float)textureCutWidth / (float)texture.Width;
            var calcOffset = new Vector2(init.BufferTiles, init.BufferTiles) * 20;

            //if (init.Type is InitTileType.VoxelStruct) calcOffset.Y += 1;

            calcOffset /= new Vector2(texture.Width, texture.Height);
            
            var textureLoc = GetShaderLocation(shader, "inputTexture");
            var layerNumLoc = GetShaderLocation(shader, "layerNum");
            var layerHeightLoc = GetShaderLocation(shader, "layerHeight");
            var layerWidthLoc = GetShaderLocation(shader, "layerWidth");
            var colorLoc = GetShaderLocation(shader, "tint");
            var depthLoc = GetShaderLocation(shader, "depth");
            var offsetLoc = GetShaderLocation(shader, "offset");

            BeginShaderMode(shader);

            SetShaderValueTexture(shader, textureLoc, texture);
            SetShaderValue(shader, layerNumLoc, init.Type == InitTileType.VoxelStructRockType ? 1 : init.Repeat.Length,
                ShaderUniformDataType.Int);
            SetShaderValue(shader, layerHeightLoc, calLayerHeight,
                ShaderUniformDataType.Float);
            SetShaderValue(shader, layerWidthLoc, calTextureCutWidth,
                ShaderUniformDataType.Float);
            SetShaderValue(shader, colorLoc,
                new Vector4(tint.R / 255f, tint.G / 255f, tint.B / 255f, 1.0f),
                ShaderUniformDataType.Vec4);
            SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);

            SetShaderValue(shader, offsetLoc, calcOffset, ShaderUniformDataType.Vec2);
            
            DrawTextureQuad(texture, scaledQuads);
            EndShaderMode();
        }
    }
    
    /// Same as DrawTileAsProp() except it applies a tint to the base texture.
    /// 
    /// <para>Only used in the preview panel.</para>
    internal static void DrawTileAsPropColored(
        in TileDefinition init, 
        in Vector2 position,
        in Vector2 originOffset,
        in Color tint,
        in int depth,
        in int scale
    )
    {
        var texture = init.Texture;
        
        var scaledQuads = new PropQuads
        {
            TopLeft = position*scale - originOffset,
            TopRight = (position + new Vector2(init.Size.Item1, 0) * scale) - originOffset,
            BottomRight = (position + new Vector2(init.Size.Item1, init.Size.Item2)) * scale - originOffset,
            BottomLeft = (position + new Vector2(0, init.Size.Item2)) * scale - originOffset
        };
        
        if (init.Type == Data.Tiles.TileType.Box)
        {
            var (tWidth, tHeight) = init.Size;
            var bufferPixels = init.BufferTiles * 20;
            
            var height = tHeight * 20;
            var offset = new Vector2(bufferPixels,tHeight*20*tWidth + bufferPixels);
            
            var calcHeight = height / (float)texture.Height;
            var calcOffset = Raymath.Vector2Divide(offset, new Vector2(texture.Width, texture.Height));
            var calcWidth = (float)tWidth * 20 / texture.Width;
            
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
            
            DrawTextureQuad(texture, scaledQuads);
            EndShaderMode();
        }
        else
        {
            var shader = GLOBALS.Shaders.PreviewColoredTileProp;
            
            var layerHeight = init.Size.Item2 * 20;
            var calLayerHeight = (float)layerHeight / (float)texture.Height;
            var textureCutWidth = init.Size.Item1 * 20;
            var calTextureCutWidth = (float)textureCutWidth / (float)texture.Width;
            var calcOffset = new Vector2(init.BufferTiles, init.BufferTiles) * 20;

            //if (init.Type is InitTileType.VoxelStruct) calcOffset.Y += 1;

            calcOffset /= new Vector2(texture.Width, texture.Height);
            
            var textureLoc = GetShaderLocation(shader, "inputTexture");
            var layerNumLoc = GetShaderLocation(shader, "layerNum");
            var layerHeightLoc = GetShaderLocation(shader, "layerHeight");
            var layerWidthLoc = GetShaderLocation(shader, "layerWidth");
            var colorLoc = GetShaderLocation(shader, "tint");
            var depthLoc = GetShaderLocation(shader, "depth");
            var offsetLoc = GetShaderLocation(shader, "offset");

            BeginShaderMode(shader);

            SetShaderValueTexture(shader, textureLoc, texture);
            SetShaderValue(shader, layerNumLoc, init.Type == Data.Tiles.TileType.VoxelStructRockType ? 1 : init.Repeat.Length,
                ShaderUniformDataType.Int);
            SetShaderValue(shader, layerHeightLoc, calLayerHeight,
                ShaderUniformDataType.Float);
            SetShaderValue(shader, layerWidthLoc, calTextureCutWidth,
                ShaderUniformDataType.Float);
            SetShaderValue(shader, colorLoc,
                new Vector4(tint.R / 255f, tint.G / 255f, tint.B / 255f, 1.0f),
                ShaderUniformDataType.Vec4);
            SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);

            SetShaderValue(shader, offsetLoc, calcOffset, ShaderUniformDataType.Vec2);
            
            DrawTextureQuad(texture, scaledQuads);
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
    
    internal static void DrawProp(
        in InitPropBase init, 
        in Texture2D texture, 
        in PropQuads quad,
        int variation = 0,
        int depth = 0)
    {
        switch (init)
        {
            case InitVariedStandardProp variedStandard:
                DrawVariedStandardProp(variedStandard, texture, quad, variation, depth);
                break;
            
            case InitStandardProp standard:
                DrawStandardProp(standard, texture, quad, depth,  0);
                break;
            
            case InitVariedSoftProp variedSoft:
                DrawVariedSoftProp(variedSoft, texture, quad, variation, depth);
                break;
            
            case InitSoftProp soft:
                DrawSoftProp(texture, quad, depth);
                break;
            
            case InitVariedDecalProp variedDecal:
                DrawVariedDecalProp(variedDecal, texture, quad, variation, 0);
                break;
            
            case InitSimpleDecalProp:
                DrawSimpleDecalProp(texture, quad);
                break;
            
            case InitAntimatterProp:
                DrawAntimatterProp(texture, quad, depth, 0);
                break;
            
            default:
                DrawPropDefault(texture, quad, depth, 0);
                break;
        }
    }

    internal static void DrawProp(InitPropType type, TileDefinition? tile, int category, int index, Prop prop, bool tintedTiles = true)
    {
        var depth = -prop.Depth - GLOBALS.Layer*10;

        switch (type)
        {
            case InitPropType.Tile:
            {
                if (GLOBALS.TileDex is null || tile is null) return;
                
                var color = GLOBALS.TileDex.GetTileColor(tile.Name);
            
                if (tintedTiles)
                    DrawTileAsPropColored(tile, prop.Quads, color, depth);
                else
                    DrawTileAsProp(tile, prop.Quads, depth);
            }
                break;

            case InitPropType.Long:
            {
                var texture = GLOBALS.Textures.LongProps[index];
                DrawLongProp(texture, prop.Quads, depth, 0);
            }
                break;

            case InitPropType.Rope:
                break;

            default:
            {
                var texture = GLOBALS.Textures.Props[category][index];
                var init = GLOBALS.Props[category][index];

                // TODO: Could be simplified
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
                    
                    case InitAntimatterProp:
                        DrawAntimatterProp(texture, prop.Quads, depth, 0);
                        break;
                    
                    default:
                        DrawPropDefault(texture, prop.Quads, depth, 0);
                        break;
                }
            }
                break;
        }
    }
    
    internal static void DrawProp(InitPropType type, TileDefinition? tile, int category, int index, Prop prop, int scale, bool tintedTiles = true)
    {
        var depth = -prop.Depth - GLOBALS.Layer*10;

        var quads = prop.Quads;

        quads.TopLeft *= scale / 16f;
        quads.TopRight *= scale / 16f;
        quads.BottomRight *= scale / 16f;
        quads.BottomLeft *= scale / 16f;

        switch (type)
        {
            case InitPropType.Tile:
            {
                if (GLOBALS.TileDex is null || tile is null) return;
                
                var color = GLOBALS.TileDex.GetTileColor(tile.Name);
            
                if (tintedTiles)
                    DrawTileAsPropColored(tile, quads, color, depth);
                else
                    DrawTileAsProp(tile, quads, depth);
            }
                break;

            case InitPropType.Long:
            {
                var texture = GLOBALS.Textures.LongProps[index];
                DrawLongProp(texture, quads, depth, 0);
            }
                break;

            case InitPropType.Rope:
                break;

            default:
            {
                var texture = GLOBALS.Textures.Props[category][index];
                var init = GLOBALS.Props[category][index];

                // TODO: Could be simplified
                switch (init)
                {
                    case InitVariedStandardProp variedStandard:
                        DrawVariedStandardProp(variedStandard, texture, quads, ((PropVariedSettings)prop.Extras.Settings).Variation, depth);
                        break;

                    case InitStandardProp standard:
                        DrawStandardProp(standard, texture, quads, depth);
                        break;

                    case InitVariedSoftProp variedSoft:
                        DrawVariedSoftProp(variedSoft, texture, quads,  ((PropVariedSoftSettings)prop.Extras.Settings).Variation, depth);
                        break;

                    case InitSoftProp:
                        DrawSoftProp(texture, quads, depth);
                        break;

                    case InitVariedDecalProp variedDecal:
                        DrawVariedDecalProp(variedDecal, texture, quads, ((PropVariedDecalSettings)prop.Extras.Settings).Variation, depth);
                        break;

                    case InitSimpleDecalProp:
                        DrawSimpleDecalProp(texture, quads, depth);
                        break;
                    
                    case InitAntimatterProp:
                        DrawAntimatterProp(texture, quads, depth, 0);
                        break;
                    
                    default:
                        DrawPropDefault(texture, quads, depth, 0);
                        break;
                }
            }
                break;
        }
    }

    internal static void DrawProp(InitPropType type, TileDefinition? tile, int category, int index, Prop prop, int scale, PropDrawMode drawMode, Texture2D? palette)
    {
        var depth = -prop.Depth - GLOBALS.Layer*10;

        var quads = prop.Quads;

        quads.TopLeft *= scale / 16f;
        quads.TopRight *= scale / 16f;
        quads.BottomRight *= scale / 16f;
        quads.BottomLeft *= scale / 16f;

        switch (type)
        {
            case InitPropType.Tile:
            {
                if (GLOBALS.TileDex is null || tile is null) return;
                
                var color = GLOBALS.TileDex.GetTileColor(tile.Name);

                switch (drawMode) {
                    case PropDrawMode.Untinted:
                    DrawTileAsProp(tile, quads, depth);
                    break;

                    case PropDrawMode.Tinted:
                    DrawTileAsPropColored(tile, quads, color, depth);
                    break;

                    case PropDrawMode.Palette:
                    DrawTileWithPalette(tile, palette!.Value, quads, -prop.Depth);
                    break;
                }
            }
                break;

            case InitPropType.Long:
            {
                var texture = GLOBALS.Textures.LongProps[index];
                DrawLongProp(texture, quads, depth, 0);
            }
                break;

            case InitPropType.Rope:
                break;

            default:
            {
                var texture = GLOBALS.Textures.Props[category][index];
                var color = GLOBALS.PropCategories[category].Item2;
                var init = GLOBALS.Props[category][index];

                // TODO: Could be simplified
                switch (init)
                {
                    case InitVariedStandardProp variedStandard:
                        switch (drawMode) {
                            case PropDrawMode.Untinted:
                            DrawVariedStandardProp(variedStandard, texture, quads, ((PropVariedSettings)prop.Extras.Settings).Variation, depth);
                            break;

                            case PropDrawMode.Tinted:
                            DrawVariedStandardPropColored(variedStandard, texture, color, quads, ((PropVariedSettings)prop.Extras.Settings).Variation, depth);
                            break;

                            case PropDrawMode.Palette:
                            DrawVariedStandardPropWithPalette(variedStandard, texture, palette!.Value, quads, ((PropVariedSettings)prop.Extras.Settings).Variation, -prop.Depth);
                            break;
                        }
                        break;

                    case InitStandardProp standard:
                        switch (drawMode) {
                            case PropDrawMode.Untinted:
                            DrawStandardProp(standard, texture, quads, depth);
                            break;

                            case PropDrawMode.Tinted:
                            DrawStandardPropColored(standard, texture, color, quads, depth);
                            break;

                            case PropDrawMode.Palette:
                            DrawStandardPropWithPalette(standard, texture, palette!.Value, quads, -prop.Depth);
                            break;
                        }
                        break;

                    case InitVariedSoftProp variedSoft:
                        switch (drawMode) {
                            case PropDrawMode.Untinted:
                            DrawVariedSoftProp(variedSoft, texture, quads,  ((PropVariedSoftSettings)prop.Extras.Settings).Variation, depth);
                            break;

                            case PropDrawMode.Tinted:
                            DrawVariedSoftPropColored(variedSoft, texture, quads, color, ((PropVariedSoftSettings)prop.Extras.Settings).Variation, depth);
                            break;

                            case PropDrawMode.Palette:
                            DrawVariedSoftPropWithPalette(variedSoft, texture, palette!.Value, quads, ((PropVariedSoftSettings)prop.Extras.Settings).Variation, -prop.Depth);
                            break;
                        }
                        break;

                    case InitSoftProp:
                        switch (drawMode) {
                            case PropDrawMode.Untinted:
                            DrawSoftProp(texture, quads, depth);
                            break;

                            case PropDrawMode.Tinted:
                            DrawSoftPropColored(texture, quads, depth, color);
                            break;

                            case PropDrawMode.Palette:
                            DrawSoftPropWithPalette(texture, palette!.Value, quads, -prop.Depth);
                            break;
                        }
                        break;

                    case InitVariedDecalProp variedDecal:
                        DrawVariedDecalProp(variedDecal, texture, quads, ((PropVariedDecalSettings)prop.Extras.Settings).Variation, depth);
                        break;

                    case InitSimpleDecalProp:
                        DrawSimpleDecalProp(texture, quads, depth);
                        break;
                    
                    case InitAntimatterProp:
                        DrawAntimatterProp(texture, quads, depth, 0);
                        break;
                    
                    default:
                        DrawPropDefault(texture, quads, depth, 0);
                        break;
                }
            }
                break;
        }
    } 
    
    internal static void DrawProp(InitPropType type, TileDefinition? tile, int category, int index, Prop prop, int scale, Vector2 offset, bool tintedTiles = true)
    {
        var depth = -prop.Depth - GLOBALS.Layer*10;

        var quads = prop.Quads;

        quads.TopLeft *= scale / 16f;
        quads.TopRight *= scale / 16f;
        quads.BottomRight *= scale / 16f;
        quads.BottomLeft *= scale / 16f;
        
        quads.TopLeft += offset;
        quads.TopRight += offset;
        quads.BottomRight += offset;
        quads.BottomLeft += offset;

        switch (type)
        {
            case InitPropType.Tile:
            {
                if (GLOBALS.TileDex is null || tile is null) return;

                var color = GLOBALS.TileDex.GetTileColor(tile);
            
                if (tintedTiles)
                    DrawTileAsPropColored(tile, quads, color, depth);
                else
                    DrawTileAsProp(tile, quads, depth);
            }
                break;

            case InitPropType.Long:
            {
                var texture = GLOBALS.Textures.LongProps[index];
                DrawLongProp(texture, quads, depth, 0);
            }
                break;

            case InitPropType.Rope:
                break;

            default:
            {
                var texture = GLOBALS.Textures.Props[category][index];
                var init = GLOBALS.Props[category][index];

                // TODO: Could be simplified
                switch (init)
                {
                    case InitVariedStandardProp variedStandard:
                        DrawVariedStandardProp(variedStandard, texture, quads, ((PropVariedSettings)prop.Extras.Settings).Variation, depth);
                        break;

                    case InitStandardProp standard:
                        DrawStandardProp(standard, texture, quads, depth);
                        break;

                    case InitVariedSoftProp variedSoft:
                        DrawVariedSoftProp(variedSoft, texture, quads,  ((PropVariedSoftSettings)prop.Extras.Settings).Variation, depth);
                        break;

                    case InitSoftProp:
                        DrawSoftProp(texture, quads, depth);
                        break;

                    case InitVariedDecalProp variedDecal:
                        DrawVariedDecalProp(variedDecal, texture, quads, ((PropVariedDecalSettings)prop.Extras.Settings).Variation, depth);
                        break;

                    case InitSimpleDecalProp:
                        DrawSimpleDecalProp(texture, quads, depth);
                        break;
                    
                    case InitAntimatterProp:
                        DrawAntimatterProp(texture, quads, depth, 0);
                        break;
                    
                    default:
                        DrawPropDefault(texture, quads, depth, 0);
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
                DrawLongProp(texture, quads, depth, 0);
                break;
        }
    }
    
    internal static void DrawProp(
        BasicPropSettings settings, 
        InitPropBase init, 
        in Texture2D texture, 
        in PropQuads quads,
        int depth,
        int rotation)
    {
        switch (init)
        {
            case InitVariedStandardProp variedStandard:
                DrawVariedStandardProp(variedStandard, texture, quads, ((PropVariedSettings)settings).Variation, depth, rotation);
                break;

            case InitStandardProp standard:
                DrawStandardProp(standard, texture, quads, depth, rotation);
                break;

            case InitVariedSoftProp variedSoft:
                DrawVariedSoftProp(variedSoft, texture, quads,  ((PropVariedSoftSettings)settings).Variation, depth, rotation);
                break;

            case InitSoftProp:
                DrawSoftProp(texture, quads, depth, rotation);
                break;

            case InitVariedDecalProp variedDecal:
                DrawVariedDecalProp(variedDecal, texture, quads, ((PropVariedDecalSettings)settings).Variation, depth, rotation);
                break;

            case InitSimpleDecalProp:
                DrawSimpleDecalProp(texture, quads, depth, rotation);
                break;
            
            case InitLongProp:
                DrawLongProp(texture, quads, depth, rotation);
                break;
            
            case InitAntimatterProp:
                DrawAntimatterProp(texture, quads, depth, rotation);
                break;
            
            default:
                DrawPropDefault(texture, quads, depth, rotation);
                break;
        }
    }

    internal static void DrawPropDefault(in Texture2D texture, in PropQuads quads, int depth, int rotation)
    {
        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;

        var shader = GLOBALS.Shaders.DefaultProp;
        
        var textureLoc = GetShaderLocation(shader, "inputTexture");
        var depthLoc = GetShaderLocation(shader, "depth");

        BeginShaderMode(shader);

        SetShaderValueTexture(shader, textureLoc, texture);
        SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuad(texture, Utils.RotatePropQuads(quads, rotation), flippedX, flippedY);
        EndShaderMode();
    }

    internal static void DrawAntimatterProp(in Texture2D texture, in PropQuads quads, int depth, int rotation)
    {
        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;

        var shader = GLOBALS.Shaders.DefaultProp;
        
        var textureLoc = GetShaderLocation(shader, "inputTexture");
        var depthLoc = GetShaderLocation(shader, "depth");

        BeginShaderMode(shader);

        SetShaderValueTexture(shader, textureLoc, texture);
        SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuad(texture, Utils.RotatePropQuads(quads, rotation), flippedX, flippedY);
        EndShaderMode();
    }

    internal static void DrawLongProp(in Texture2D texture, in PropQuads quads, int depth, int rotation)
    {
        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.LongProp, "inputTexture");
        var depthLoc = GetShaderLocation(GLOBALS.Shaders.LongProp, "depth");

        BeginShaderMode(GLOBALS.Shaders.LongProp);

        SetShaderValueTexture(GLOBALS.Shaders.LongProp, textureLoc, texture);
        SetShaderValue(GLOBALS.Shaders.LongProp, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuad(texture, Utils.RotatePropQuads(quads, rotation), flippedX, flippedY);
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
        
        DrawTextureQuad(texture, quads, flippedX, flippedY);

        EndShaderMode();
    }

    internal static void DrawStandardPropColored(
        InitStandardProp init, 
        in Texture2D texture, 
        Color tint,
        PropQuads quads,
        int depth
    )
    {
        var shader = GLOBALS.Shaders.StandardPropColored;

        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var layerHeight = (float)texture.Height / (float)init.Repeat.Length;
        var calcLayerHeight = layerHeight / texture.Height;
        var calcWidth = (float) init.Size.x * GLOBALS.Scale / texture.Width;

        calcWidth = calcWidth > 1.00000f ? 1.0f : calcWidth;
        
        var textureLoc = GetShaderLocation(shader, "inputTexture");

        var tintLoc = GetShaderLocation(shader, "tint");
        var layerNumLoc = GetShaderLocation(shader, "layerNum");
        var layerHeightLoc = GetShaderLocation(shader, "layerHeight");
        var widthLoc = GetShaderLocation(shader, "width");
        var depthLoc = GetShaderLocation(shader, "depth");

        BeginShaderMode(shader);
        
        SetShaderValueTexture(shader, textureLoc, texture);

        var tintV = new Vector4 {
            X = tint.R / 255f,
            Y = tint.G / 255f,
            Z = tint.B / 255f,
            W = tint.A / 255f,
        };

        SetShaderValue(shader, tintLoc, tintV, ShaderUniformDataType.Vec4);
        SetShaderValue(shader, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.Int);
        SetShaderValue(shader, layerHeightLoc, calcLayerHeight, ShaderUniformDataType.Float);
        SetShaderValue(shader, widthLoc, calcWidth, ShaderUniformDataType.Float);
        SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuad(texture, quads, flippedX, flippedY);

        EndShaderMode();
    }

    internal static void DrawStandardProp(
        InitStandardProp init, 
        in Texture2D texture, 
        PropQuads quads,
        int depth,
        int rotation
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
        
        DrawTextureQuad(texture, Utils.RotatePropQuads(quads, rotation), flippedX, flippedY);

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
        
        DrawTextureQuad(texture, quads, flippedX, flippedY);
        EndShaderMode();
    }

    internal static void DrawVariedStandardPropColored(
        InitVariedStandardProp init, 
        in Texture2D texture,
        Color tint,
        PropQuads quads,
        int variation,
        int depth
    )
    {
        var shader = GLOBALS.Shaders.VariedStandardPropColored;

        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var layerHeight = (float) init.Size.y * GLOBALS.Scale;
        var variationWidth = (float) init.Size.x * GLOBALS.Scale;
        
        var calcLayerHeight = layerHeight / texture.Height;
        var calcVariationWidth = variationWidth / texture.Width;
        
        var textureLoc = GetShaderLocation(shader, "inputTexture");
        
        var tintLoc = GetShaderLocation(shader, "tint");
        var layerNumLoc = GetShaderLocation(shader, "layerNum");
        var layerHeightLoc = GetShaderLocation(shader, "layerHeight");
        var variationWidthLoc = GetShaderLocation(shader, "varWidth");
        var variationLoc = GetShaderLocation(shader, "variation");
        var depthLoc = GetShaderLocation(shader, "depth");

        BeginShaderMode(shader);

        SetShaderValueTexture(shader, textureLoc, texture);
       
        var tintV = new Vector4 { 
            X = tint.R / 255f,
            Y = tint.G / 255f,
            Z = tint.B / 255f,
            W = tint.A / 255f
        };

        SetShaderValue(shader, tintLoc, tintV, ShaderUniformDataType.Vec4);
        SetShaderValue(shader, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.Int);
        SetShaderValue(shader, layerHeightLoc, calcLayerHeight, ShaderUniformDataType.Float);
        SetShaderValue(shader, variationWidthLoc, calcVariationWidth, ShaderUniformDataType.Float);
        SetShaderValue(shader, variationLoc, variation, ShaderUniformDataType.Int);
        SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuad(texture, quads, flippedX, flippedY);
        EndShaderMode();
    }

    internal static void DrawVariedStandardProp(
        InitVariedStandardProp init, 
        in Texture2D texture, 
        PropQuads quads,
        int variation,
        int depth,
        int rotation
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
        
        DrawTextureQuad(texture, Utils.RotatePropQuads(quads, rotation), flippedX, flippedY);
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
        
        DrawTextureQuad(texture, quads, flippedX, flippedY);
        EndShaderMode();
    }

    internal static void DrawSoftPropColored(in Texture2D texture, in PropQuads quads, int depth, Color tint)
    {
        var shader = GLOBALS.Shaders.SoftPropColored;

        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var textureLoc = GetShaderLocation(shader, "inputTexture");
        var depthLoc = GetShaderLocation(shader, "depth");

        BeginShaderMode(shader);

        SetShaderValueTexture(shader, textureLoc, texture);
        SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuad(texture, quads, tint, flippedX, flippedY);
        EndShaderMode();
    }
    
    internal static void DrawSoftProp(in Texture2D texture, in PropQuads quads, int depth, int rotation)
    {
        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.SoftProp, "inputTexture");
        var depthLoc = GetShaderLocation(GLOBALS.Shaders.SoftProp, "depth");

        BeginShaderMode(GLOBALS.Shaders.SoftProp);

        SetShaderValueTexture(GLOBALS.Shaders.SoftProp, textureLoc, texture);
        SetShaderValue(GLOBALS.Shaders.SoftProp, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuad(texture, Utils.RotatePropQuads(quads, rotation), flippedX, flippedY);
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
        
        DrawTextureQuad(texture, quads, flippedX, flippedY);
        EndShaderMode();
    }

    internal static void DrawVariedSoftPropColored(
        InitVariedSoftProp init, 
        in Texture2D texture, 
        PropQuads quads,
        Color tint,
        int variation,
        int depth
    )
    {
        var shader = GLOBALS.Shaders.VariedSoftPropColored;

        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var calcHeight = (float) init.SizeInPixels.y / texture.Height;
        var calcVariationWidth = (float) init.SizeInPixels.x / texture.Width;

        var textureLoc = GetShaderLocation(shader, "inputTexture");

        var heightLoc = GetShaderLocation(shader, "height");
        var variationWidthLoc = GetShaderLocation(shader, "varWidth");
        var variationLoc = GetShaderLocation(shader, "variation");
        var depthLoc = GetShaderLocation(shader, "depth");

        BeginShaderMode(shader);

        SetShaderValueTexture(shader, textureLoc, texture);

        SetShaderValue(shader, variationWidthLoc, calcVariationWidth,
            ShaderUniformDataType.Float);
        SetShaderValue(shader, heightLoc, calcHeight, ShaderUniformDataType.Float);
        SetShaderValue(shader, variationLoc, variation, ShaderUniformDataType.Int);
        SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuad(texture, quads, tint, flippedX, flippedY);
        EndShaderMode();
    }
    
    internal static void DrawVariedSoftProp(
        InitVariedSoftProp init, 
        in Texture2D texture, 
        PropQuads quads,
        int variation,
        int depth,
        int rotation
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
        
        DrawTextureQuad(texture, Utils.RotatePropQuads(quads, rotation), flippedX, flippedY);
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
        
        DrawTextureQuad(texture, quads, flippedX, flippedY);
        EndShaderMode();
    }
    
    internal static void DrawSimpleDecalProp(in Texture2D texture, in PropQuads quads, int depth, int rotation)
    {
        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var textureLoc = GetShaderLocation(GLOBALS.Shaders.SimpleDecalProp, "inputTexture");
        var depthLoc = GetShaderLocation(GLOBALS.Shaders.SimpleDecalProp, "depth");

        BeginShaderMode(GLOBALS.Shaders.SimpleDecalProp);

        SetShaderValueTexture(GLOBALS.Shaders.SimpleDecalProp, textureLoc, texture);
        SetShaderValue(GLOBALS.Shaders.SimpleDecalProp, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuad(texture, Utils.RotatePropQuads(quads, rotation), flippedX, flippedY);
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
        
        DrawTextureQuad(texture, quads, flippedX, flippedY);
        EndShaderMode();
    }

    internal static void DrawVariedDecalProp(
        InitVariedDecalProp init, 
        in Texture2D texture, 
        in PropQuads quads,
        int variation,
        int depth,
        int rotation
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
        
        DrawTextureQuad(texture, Utils.RotatePropQuads(quads, rotation), flippedX, flippedY);
        EndShaderMode();
    }
    
    internal static void DrawCross(Vector2 origin, Vector2 size, float thickness, Color color)
    {
        DrawLineEx(origin, origin + size, thickness, color);
        DrawLineEx(origin with { Y = origin.Y + size.Y }, origin with { X = origin.X + size.X }, thickness, color);
    }

    /// <summary>
    /// Matrix coordinates required
    /// </summary>
    internal static void DrawCircularSquare(int x, int y, int radius, int scale, Color color)
    {
        var centerV = new Vector2(x + 0.5f, y + 0.5f) * scale;

        for (var lx = -radius; lx < radius + 1; lx++) {
            var mx = x + lx;

            for (var ly = -radius; ly < radius + 1; ly++) {
                var my = y + ly;

                if (CheckCollisionCircleRec(centerV, radius * scale, new(mx * scale, my * scale, scale, scale))) {
                    DrawRectangle(mx * scale, my * scale, scale, scale, color);
                }
            }
        }
    }

    /// <summary>
    /// Matrix coordinates required
    /// </summary>
    internal static void DrawCircularSquareLines(int x, int y, int radius, int scale, float thickness, Color color)
    {
        if (radius == 0) {
            DrawRectangleLinesEx(new (x * scale, y * scale, scale, scale), thickness, color);
            return;
        }

        var centerV = new Vector2(x + 0.5f, y + 0.5f) * scale;

        for (var lx = -radius; lx < radius + 1; lx++) {
            var mx = x + lx;

            for (var ly = -radius; ly < radius + 1; ly++) {
                var my = y + ly;

                var sx = mx * scale;
                var sy = my * scale;

                if (!CheckCollisionCircleRec(centerV, radius * scale, new(sx, sy, scale, scale))) 
                    continue;
            
                var left = (bool) CheckCollisionCircleRec(centerV, radius * scale, new(sx - scale, sy, scale, scale));
                var top = (bool) CheckCollisionCircleRec(centerV, radius * scale, new(sx, sy - scale, scale, scale));
                var right = (bool) CheckCollisionCircleRec(centerV, radius * scale, new(sx + scale, sy, scale, scale));
                var bottom = (bool) CheckCollisionCircleRec(centerV, radius * scale, new(sx, sy + scale, scale, scale));
            
                switch ((left, top, right, bottom)) {
                    // left wall
                    case (false, true, true, true):
                    DrawLineEx(new Vector2(sx, sy), new Vector2(sx, sy + scale), thickness, color);
                    break;

                    // top wall
                    case (true, false, true, true):
                    DrawLineEx(new Vector2(sx, sy), new Vector2(sx + scale, sy), thickness, color);
                    break;

                    // right wall
                    case (true, true, false, true):
                    DrawLineEx(new Vector2(sx + scale, sy), new Vector2(sx + scale, sy + scale), thickness, color);
                    break;

                    // bottom wall
                    case (true, true, true, false):
                    DrawLineEx(new Vector2(sx, sy + scale), new Vector2(sx + scale, sy + scale), thickness, color);
                    break;

                    // top-left
                    case (false, false, true, true):
                    DrawLineEx(new Vector2(sx, sy), new Vector2(sx + scale, sy), thickness, color);
                    DrawLineEx(new Vector2(sx, sy), new Vector2(sx, sy + scale), thickness, color);
                    break;

                    // top-right
                    case (true, false, false, true):
                    DrawLineEx(new Vector2(sx, sy), new Vector2(sx + scale, sy), thickness, color);
                    DrawLineEx(new Vector2(sx + scale, sy), new Vector2(sx + scale, sy + scale), thickness, color);
                    break;

                    // bottom-right
                    case (true, true, false, false):
                    DrawLineEx(new Vector2(sx + scale, sy), new Vector2(sx + scale, sy + scale), thickness, color);
                    DrawLineEx(new Vector2(sx, sy + scale), new Vector2(sx + scale, sy + scale), thickness, color);
                    break;

                    // bottom-left
                    case (false, true, true, false):
                    DrawLineEx(new Vector2(sx, sy + scale), new Vector2(sx + scale, sy + scale), thickness, color);
                    DrawLineEx(new Vector2(sx, sy), new Vector2(sx, sy + scale), thickness, color);
                    break;
                }
            }
        }
    }

    internal static void DrawTileSpecs(in int[,,]? specs, Vector2 origin, int scale) {
        if (specs is null) return;

        for (var x = 0; x < specs.GetLength(1); x++) {
            for (var y = 0; y < specs.GetLength(0); y++) {
                var spec = specs[y, x, 0];
                var spec2 = specs[y, x, 1];
                var spec3 = specs[y, x, 2];

                var scaledOrigin = new Vector2(origin.X + x, origin.Y + y) * scale;

                if (spec is >= 0 and < 9 and not 8)
                {
                    if (spec3 is >= 0 and < 9 and not 8) DrawTileSpec(
                        spec3,
                        scaledOrigin + new Vector2(4, 4),
                        scale - 8,
                        Color.Red,
                        spec3 is 0 ? 8 : 1
                    );
                    
                    if (spec2 is >= 0 and < 9 and not 8) DrawTileSpec(
                        spec2,
                        scaledOrigin + new Vector2(2, 2),
                        scale - 4,
                        Color.Green,
                        spec2 is 0 ? 4 : 1
                    );
                    
                    DrawTileSpec(
                        spec,
                        scaledOrigin,
                        scale,
                        GLOBALS.Settings.GeneralSettings.DarkTheme 
                            ? Color.White
                            : Color.Black
                    );
                }
            }
        }
    }

    /// <summary>
    /// Draws an individual tile geo-spec based on the ID.
    /// </summary>
    /// <param name="id">geo-tile ID</param>
    /// <param name="origin">the top-left corner to start drawing</param>
    /// <param name="scale">the scale of the drawing</param>
    /// <param name="color">the color of the sprite</param>
    internal static void DrawTileSpec2(int id, Vector2 origin, int scale, Color color)
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
                DrawRectangleRec(
                    new Rectangle(origin.X, origin.Y, scale, scale/2f),
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

    internal static void DrawTriangleLines(in Vector2 v1, in Vector2 v2, in Vector2 v3, float thickness, in Color color)
    {
        DrawLineEx(v1, v2, thickness, color);
        DrawLineEx(v2, v3, thickness, color);
        DrawLineEx(v3, v1, thickness, color);
    }

    /// <summary>
    /// Draws an individual tile geo-spec based on the ID.
    /// </summary>
    /// <param name="id">geo-tile ID</param>
    /// <param name="origin">the top-left corner to start drawing</param>
    /// <param name="scale">the scale of the drawing</param>
    /// <param name="color">the color of the sprite</param>
    /// <param name="thickness">the line thickness</param>
    internal static void DrawTileSpec(int id, Vector2 origin, int scale, Color color, float thickness = 1)
    {
        switch (id)
        {
            // air
            case 0:
                // DrawRectangleLinesEx(
                //     new(origin.X + 10, origin.Y + 10, scale - 20, scale - 20),
                //     2,
                //     color
                // );
                
                DrawCross(origin, new Vector2(scale, scale), thickness, color);
                break;

            // solid
            case 1:
                // DrawRectangleV(origin, Raymath.Vector2Scale(new(1, 1), scale), color);
                DrawRectangleLinesEx(new Rectangle(origin.X, origin.Y,  scale,  scale), thickness, color);
                break;

            // slopes
            case 2:
                DrawTriangleLines(
                    origin,
                    new(origin.X, origin.Y + scale),
                    new(origin.X + scale, origin.Y + scale),
                    thickness,
                    color
                );
                break;

            case 3:
                DrawTriangleLines(
                    new Vector2(origin.X + scale, origin.Y), 
                    new Vector2(origin.X, origin.Y + scale), 
                    new Vector2(origin.X + scale, origin.Y + scale), 
                    thickness, 
                    color
                );
                break;

            case 4:
                DrawTriangleLines(
                    origin, 
                    new Vector2(origin.X , origin.Y + scale), 
                    new Vector2(origin.X + scale, origin.Y), 
                    thickness, 
                    color
                );
                break;
            
            case 5:
                DrawTriangleLines(
                    origin,
                    new Vector2(origin.X + scale, origin.Y + scale),
                    new Vector2(origin.X + scale, origin.Y),
                    thickness,
                    color
                );
                break;

            // platform
            case 6:
                DrawRectangleLinesEx(
                    new Rectangle(origin.X, origin.Y, scale, scale/2f),
                    thickness,
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

    internal static void DrawDepthIndicator((InitPropType type, TileDefinition? tile, (int category, int index) position, Prop prop) prop)
    {
        var (c, i) = prop.position;
        
        switch (prop.type)
        {
            case InitPropType.Tile:
            {
                var init = prop.tile;

                if (init is null) return;
                
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

    // Do not use(?)
    internal static void DrawTextureTiled(Texture2D texture, Rectangle source, Rectangle dest, Vector2 origin, float rotation, float scale, Color tint)
    {
        if ((texture.Id <= 0) || (scale <= 0.0f)) return;
        if ((source.Width == 0) || (source.Height == 0)) return;

        int tileWidth = (int)(source.Width*scale), tileHeight = (int)(source.Height*scale);
        if ((dest.Width < tileWidth) && (dest.Height < tileHeight))
        {
            DrawTexturePro(texture, new Rectangle(source.X, source.Y, ((float)dest.Width/tileWidth)*source.Width, ((float)dest.Height/tileHeight)*source.Height),
                        new Rectangle(dest.X, dest.Y, dest.Width, dest.Height), origin, rotation, tint);
        }
        else if (dest.Width <= tileWidth)
        {
            var dy = 0;
            for (;dy+tileHeight < dest.Height; dy += tileHeight)
            {
                DrawTexturePro(texture, new Rectangle(source.X, source.Y, ((float)dest.Width/tileWidth)*source.Width, source.Height), new Rectangle(dest.X, dest.Y + dy, dest.Width, (float)tileHeight), origin, rotation, tint);
            }

            if (dy < dest.Height)
            {
                DrawTexturePro(texture, new Rectangle(source.X, source.Y, ((float)dest.Width/tileWidth)*source.Width, ((float)(dest.Height - dy)/tileHeight)*source.Height),
                            new Rectangle(dest.X, dest.Y + dy, dest.Width, dest.Height - dy), origin, rotation, tint);
            }
        }
        else if (dest.Height <= tileHeight)
        {
            var dx = 0;
            for (;dx+tileWidth < dest.Width; dx += tileWidth)
            {
                DrawTexturePro(texture, new Rectangle(source.X, source.Y, source.Width, ((float)dest.Height/tileHeight)*source.Height), new Rectangle(dest.X + dx, dest.Y, (float)tileWidth, dest.Height), origin, rotation, tint);
            }

            if (dx < dest.Width)
            {
                DrawTexturePro(texture, new Rectangle(source.X, source.Y, ((float)(dest.Width - dx)/tileWidth)*source.Width, ((float)dest.Height/tileHeight)*source.Height),
                            new Rectangle(dest.X + dx, dest.Y, dest.Width - dx, dest.Height), origin, rotation, tint);
            }
        }
        else
        {
            var dx = 0;
            for (;dx+tileWidth < dest.Width; dx += tileWidth)
            {
                var dy = 0;
                for (;dy+tileHeight < dest.Height; dy += tileHeight)
                {
                    DrawTexturePro(texture, source, new Rectangle (dest.X + dx, dest.Y + dy, (float)tileWidth, (float)tileHeight), origin, rotation, tint);
                }

                if (dy < dest.Height)
                {
                    DrawTexturePro(texture, new Rectangle (source.X, source.Y, source.Width, ((float)(dest.Height - dy)/tileHeight)*source.Height),
                        new Rectangle (dest.X + dx, dest.Y + dy, (float)tileWidth, dest.Height - dy), origin, rotation, tint);
                }
            }

            if (dx < dest.Width)
            {
                var dy = 0;
                for (;dy+tileHeight < dest.Height; dy += tileHeight)
                {
                    DrawTexturePro(texture, new Rectangle (source.X, source.Y, ((float)(dest.Width - dx)/tileWidth)*source.Width, source.Height),
                            new Rectangle (dest.X + dx, dest.Y + dy, dest.Width - dx, (float)tileHeight), origin, rotation, tint);
                }

                if (dy < dest.Height)
                {
                    DrawTexturePro(texture, new Rectangle (source.X, source.Y, ((float)(dest.Width - dx)/tileWidth)*source.Width, ((float)(dest.Height - dy)/tileHeight)*source.Height),
                        new Rectangle (dest.X + dx, dest.Y + dy, dest.Width - dx, dest.Height - dy), origin, rotation, tint);
                }
            }
        }
    }

    internal static void DrawMaterialTexture(string name, int geoId, int mx, int my, int scale, byte opacity) {
        var x = mx * scale;
        var y = my * scale;

        var rect = new Rectangle(x, y, scale, scale);

        var tint = Color.White with { A = opacity };
        
        switch (name) {
            case "Concrete":
            {
                var texture = GLOBALS.Textures.InternalMaterials[0];

                var tx = (float)x / texture.Width;
                var ty = (float)y / texture.Height;

                var tsx = ((float)x + scale) / texture.Width;
                var tsy = ((float)y + scale) / texture.Height;

                switch (geoId) {
                    case 1:
                    DrawTexturePro(texture, rect, rect, new(0, 0), 0, tint);
                    break;

                    case 2:// \
                        DrawTextureTriangle(
                            texture,
                            new(tx, ty),
                            new(tx, tsy),
                            new(tsx, tsy),
                            new(x, y),
                            new(x, y + scale),
                            new(x + scale, y + scale),
                            tint
                        );
                        break;


                    case 3:// /
                        DrawTextureTriangle(
                            texture,
                            new(tsx, ty),
                            new(tx, tsy),
                            new(tsx, tsy),
                            new(x + scale, y),
                            new(x, y + scale),
                            new(x + scale, y + scale),
                            tint
                        );
                        break;

                    case 5:
                        DrawTextureTriangle(
                            texture,
                            new(tx, ty),
                            new(tsx, tsy),
                            new(tsx, ty),
                            new(x, y),
                            new(x + scale, y + scale),
                            new(x + scale, y),
                            tint
                        );
                        break;

                    case 4:
                        DrawTextureTriangle(
                            texture,
                            new(tx, ty),
                            new(tx, tsy),
                            new(tsx, ty),
                            new(x, y),
                            new(x, y + scale),
                            new(x + scale, y),
                            tint
                        );
                        break;

                    case 6:
                        DrawTexturePro(texture, rect with { Height = rect.Height / 2.0f }, rect with { Height = rect.Height / 2.0f }, new(0, 0), 0, tint);
                        break;
                }
            }
            break;

            case "RainStone":
            {
                var texture = GLOBALS.Textures.InternalMaterials[1];

                var tx = (float)x / texture.Width;
                var ty = (float)y / texture.Height;

                var tsx = ((float)x + scale) / texture.Width;
                var tsy = ((float)y + scale) / texture.Height;

                switch (geoId) {
                    case 1:
                    DrawTexturePro(texture, rect, rect, new(0, 0), 0, tint);
                    break;

                    case 2:// \
                        DrawTextureTriangle(
                            texture,
                            new(tx, ty),
                            new(tx, tsy),
                            new(tsx, tsy),
                            new(x, y),
                            new(x, y + scale),
                            new(x + scale, y + scale),
                            tint
                        );
                        break;


                    case 3:// /
                        DrawTextureTriangle(
                            texture,
                            new(tsx, ty),
                            new(tx, tsy),
                            new(tsx, tsy),
                            new(x + scale, y),
                            new(x, y + scale),
                            new(x + scale, y + scale),
                            tint
                        );
                        break;

                    case 5:
                        DrawTextureTriangle(
                            texture,
                            new(tx, ty),
                            new(tsx, tsy),
                            new(tsx, ty),
                            new(x, y),
                            new(x + scale, y + scale),
                            new(x + scale, y),
                            tint
                        );
                        break;

                    case 4:
                        DrawTextureTriangle(
                            texture,
                            new(tx, ty),
                            new(tx, tsy),
                            new(tsx, ty),
                            new(x, y),
                            new(x, y + scale),
                            new(x + scale, y),
                            tint
                        );
                        break;

                    case 6:
                        DrawTexturePro(texture, rect with { Height = rect.Height / 2.0f }, rect with { Height = rect.Height / 2.0f }, new(0, 0), 0, tint);
                        break;
                }
            }
            break;

            case "Bricks":
            case "3DBricks":
            {
                var texture = GLOBALS.Textures.InternalMaterials[2];

                var tx = (float)x / texture.Width;
                var ty = (float)y / texture.Height;

                var tsx = ((float)x + scale) / texture.Width;
                var tsy = ((float)y + scale) / texture.Height;

                switch (geoId) {
                    case 1:
                    DrawTexturePro(texture, rect, rect, new(0, 0), 0, tint);
                    break;

                    case 2:// \
                        DrawTextureTriangle(
                            texture,
                            new(tx, ty),
                            new(tx, tsy),
                            new(tsx, tsy),
                            new(x, y),
                            new(x, y + scale),
                            new(x + scale, y + scale),
                            tint
                        );
                        break;


                    case 3:// /
                        DrawTextureTriangle(
                            texture,
                            new(tsx, ty),
                            new(tx, tsy),
                            new(tsx, tsy),
                            new(x + scale, y),
                            new(x, y + scale),
                            new(x + scale, y + scale),
                            tint
                        );
                        break;

                    case 5:
                        DrawTextureTriangle(
                            texture,
                            new(tx, ty),
                            new(tsx, tsy),
                            new(tsx, ty),
                            new(x, y),
                            new(x + scale, y + scale),
                            new(x + scale, y),
                            tint
                        );
                        break;

                    case 4:
                        DrawTextureTriangle(
                            texture,
                            new(tx, ty),
                            new(tx, tsy),
                            new(tsx, ty),
                            new(x, y),
                            new(x, y + scale),
                            new(x + scale, y),
                            tint
                        );
                        break;

                    case 6:
                        DrawTexturePro(texture, rect with { Height = rect.Height / 2.0f }, rect with { Height = rect.Height / 2.0f }, new(0, 0), 0, tint);
                        break;
                }
            }
            break;

            case "Non-Slip Metal":
            {
                var texture = GLOBALS.Textures.InternalMaterials[3];

                var tx = (float)x / texture.Width;
                var ty = (float)y / texture.Height;

                var tsx = ((float)x + scale) / texture.Width;
                var tsy = ((float)y + scale) / texture.Height;

                switch (geoId) {
                    case 1:
                    DrawTexturePro(texture, rect, rect, new(0, 0), 0, tint);
                    break;

                    case 2:// \
                        DrawTextureTriangle(
                            texture,
                            new(tx, ty),
                            new(tx, tsy),
                            new(tsx, tsy),
                            new(x, y),
                            new(x, y + scale),
                            new(x + scale, y + scale),
                            tint
                        );
                        break;


                    case 3:// /
                        DrawTextureTriangle(
                            texture,
                            new(tsx, ty),
                            new(tx, tsy),
                            new(tsx, tsy),
                            new(x + scale, y),
                            new(x, y + scale),
                            new(x + scale, y + scale),
                            tint
                        );
                        break;

                    case 5:
                        DrawTextureTriangle(
                            texture,
                            new(tx, ty),
                            new(tsx, tsy),
                            new(tsx, ty),
                            new(x, y),
                            new(x + scale, y + scale),
                            new(x + scale, y),
                            tint
                        );
                        break;

                    case 4:
                        DrawTextureTriangle(
                            texture,
                            new(tx, ty),
                            new(tx, tsy),
                            new(tsx, ty),
                            new(x, y),
                            new(x, y + scale),
                            new(x + scale, y),
                            tint
                        );
                        break;

                    case 6:
                        DrawTexturePro(texture, rect with { Height = rect.Height / 2.0f }, rect with { Height = rect.Height / 2.0f }, new(0, 0), 0, tint);
                        break;
                }
            }
            break;

            // case "ConcreteBricks":
            // {
            //     var texture = GLOBALS.Textures.InternalMaterials[5];

            //     var tx = (float)x / texture.Width;
            //     var ty = (float)y / texture.Height;

            //     var tsx = ((float)x + scale) / texture.Width;
            //     var tsy = ((float)y + scale) / texture.Height;

            //     switch (geoId) {
            //         case 1:
            //         DrawTexturePro(texture, rect, rect, new(0, 0), 0, tint);
            //         break;

            //         case 2:// \
            //             DrawTextureTriangle(
            //                 texture,
            //                 new(tx, ty),
            //                 new(tx, tsy),
            //                 new(tsx, tsy),
            //                 new(x, y),
            //                 new(x, y + scale),
            //                 new(x + scale, y + scale),
            //                 tint
            //             );
            //             break;


            //         case 3:// /
            //             DrawTextureTriangle(
            //                 texture,
            //                 new(tsx, ty),
            //                 new(tx, tsy),
            //                 new(tsx, tsy),
            //                 new(x + scale, y),
            //                 new(x, y + scale),
            //                 new(x + scale, y + scale),
            //                 tint
            //             );
            //             break;

            //         case 5:
            //             DrawTextureTriangle(
            //                 texture,
            //                 new(tx, ty),
            //                 new(tsx, tsy),
            //                 new(tsx, ty),
            //                 new(x, y),
            //                 new(x + scale, y + scale),
            //                 new(x + scale, y),
            //                 tint
            //             );
            //             break;

            //         case 4:
            //             DrawTextureTriangle(
            //                 texture,
            //                 new(tx, ty),
            //                 new(tx, tsy),
            //                 new(tsx, ty),
            //                 new(x, y),
            //                 new(x, y + scale),
            //                 new(x + scale, y),
            //                 tint
            //             );
            //             break;

            //         case 6:
            //             DrawTexturePro(texture, rect with { Height = rect.Height / 2.0f }, rect with { Height = rect.Height / 2.0f }, new(0, 0), 0, tint);
            //             break;
            //     }
            // }
            // break;

            case "Asphalt":
            {
                var texture = GLOBALS.Textures.InternalMaterials[4];

                var tx = (float)x / texture.Width;
                var ty = (float)y / texture.Height;

                var tsx = ((float)x + scale) / texture.Width;
                var tsy = ((float)y + scale) / texture.Height;

                switch (geoId) {
                    case 1:
                    DrawTexturePro(texture, rect, rect, new(0, 0), 0, tint);
                    break;

                    case 2:// \
                        DrawTextureTriangle(
                            texture,
                            new(tx, ty),
                            new(tx, tsy),
                            new(tsx, tsy),
                            new(x, y),
                            new(x, y + scale),
                            new(x + scale, y + scale),
                            tint
                        );
                        break;


                    case 3:// /
                        DrawTextureTriangle(
                            texture,
                            new(tsx, ty),
                            new(tx, tsy),
                            new(tsx, tsy),
                            new(x + scale, y),
                            new(x, y + scale),
                            new(x + scale, y + scale),
                            tint
                        );
                        break;

                    case 5:
                        DrawTextureTriangle(
                            texture,
                            new(tx, ty),
                            new(tsx, tsy),
                            new(tsx, ty),
                            new(x, y),
                            new(x + scale, y + scale),
                            new(x + scale, y),
                            tint
                        );
                        break;

                    case 4:
                        DrawTextureTriangle(
                            texture,
                            new(tx, ty),
                            new(tx, tsy),
                            new(tsx, ty),
                            new(x, y),
                            new(x, y + scale),
                            new(x + scale, y),
                            tint
                        );
                        break;

                    case 6:
                        DrawTexturePro(texture, rect with { Height = rect.Height / 2.0f }, rect with { Height = rect.Height / 2.0f }, new(0, 0), 0, tint);
                        break;
                }
            }
            break;
        
            default:
                DrawTileSpec(x, y, geoId, scale, new Color(0, 255, 0, (int)opacity));
                break;
        }
    }
    
    // Palette Printers

    internal static void DrawTileLayerWithPaletteIntoBuffer(RenderTexture2D renderTexture, int currentLayer, int targetLayer, int scale, in Texture2D palette, byte opacity, bool deepTileOpacity, bool renderMaterials)
    {
        // if (renderMaterials) {
        //     var buffer = LoadRenderTexture(GLOBALS.Level.Width * scale, GLOBALS.Level.Height * scale);

        //     BeginTextureMode(buffer);
        //     ClearBackground(Color.White);

        //     // Materials first
        //     for (var y = 0; y < GLOBALS.Level.Height; y++)
        //     {
        //         for (var x = 0; x < GLOBALS.Level.Width; x++)
        //         {
        //             var cell = GLOBALS.Level.TileMatrix[y, x, targetLayer];

        //             if (cell.Type != TileType.Material && cell.Type != TileType.Default) continue;

        //             var isDefault = cell.Type == TileType.Default;

        //             DrawMaterialTexture(isDefault 
        //                 ? GLOBALS.Level.DefaultMaterial 
        //                     : ((TileMaterial)cell.Data).Name, 
        //                 GLOBALS.Level.GeoMatrix[y, x, targetLayer].Geo, 
        //                 x, y, scale, 255);
        //         }
        //     }

        //     var op = (byte)((targetLayer == currentLayer) ? 255 : opacity);


        //     EndTextureMode();



        //     BeginTextureMode(renderTexture);

        //     var shader = GLOBALS.Shaders.Palette;
        //     BeginShaderMode(shader);
        //     SetShaderValueTexture(shader, GetShaderLocation(shader, "paletteTexture"), palette);
        //     SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), buffer.Texture);
        //     SetShaderValue(shader, GetShaderLocation(shader, "depth"), targetLayer*10, ShaderUniformDataType.Int);

        //     DrawTexture(buffer.Texture, 0, 0, Color.White with { A = op });

        //     EndShaderMode();

        //     EndTextureMode();

        //     UnloadRenderTexture(buffer);
        // }

        BeginTextureMode(renderTexture);

        // Then tiles

        for (var y = 0; y < GLOBALS.Level.Height; y++)
        {
            for (var x = 0; x < GLOBALS.Level.Width; x++)
            {
                TileCell tileCell;

                #if DEBUG
                try
                {
                    tileCell = GLOBALS.Level.TileMatrix[y, x, targetLayer];
                }
                catch (IndexOutOfRangeException ie)
                {
                    throw new IndexOutOfRangeException(innerException: ie, message: $"Failed to fetch tile cell from {nameof(GLOBALS.Level.TileMatrix)}[{GLOBALS.Level.TileMatrix.GetLength(0)}, {GLOBALS.Level.TileMatrix.GetLength(1)}, {GLOBALS.Level.TileMatrix.GetLength(2)}]: x, y, or z ({x}, {y}, {targetLayer}) was out of bounds");
                }
                #else
                tileCell = GLOBALS.Level.TileMatrix[y, x, targetLayer];
                #endif
                
                if (tileCell.Type == TileType.TileHead)
                {
                    var data = (TileHead)tileCell.Data;

                    TileDefinition? init = data.Definition;
                    var undefined = init is null;

                    var tileTexture = undefined
                        ? GLOBALS.Textures.MissingTile 
                        : data.Definition!.Texture;

                    var color = Color.Purple;

                    if (GLOBALS.TileDex?.TryGetTileColor(data.Definition?.Name ?? "", out var foundColor) ?? false)
                    {
                        color = foundColor;
                    }

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
                        var center = new Vector2(
                        init!.Size.Item1 % 2 == 0 ? x * scale + scale : x * scale + scale/2f, 
                        init!.Size.Item2 % 2 == 0 ? y * scale + scale : y * scale + scale/2f);

                        var width = (scale / 20f)/2 * (init.Type == Data.Tiles.TileType.Box ? init.Size.Width : init.Size.Width + init.BufferTiles * 2) * 20;
                        var height = (scale / 20f)/2 * ((init.Type == Data.Tiles.TileType.Box
                            ? init.Size.Item2
                            : (init.Size.Item2 + init.BufferTiles * 2)) * 20);

                        var depth2 = Utils.SpecHasDepth(init.Specs);
                        var depth3 = Utils.SpecHasDepth(init.Specs, 2);
                        
                        var shouldBeClearlyVisible = (targetLayer == currentLayer) || 
                            (targetLayer + 1 == currentLayer && depth2) || 
                            (targetLayer + 2 == currentLayer && depth3);

                        DrawTileWithPalette(
                            init,
                            palette,
                            center,
                            [
                                new(width, -height),
                                new(-width, -height),
                                new(-width, height),
                                new(width, height),
                                new(width, -height)
                            ],
                            targetLayer * 10,
                            shouldBeClearlyVisible ? 1f : opacity/255f
                        );
                    }
                }
                else if (tileCell.Type == TileType.TileBody)
                {
                    var missingTexture = GLOBALS.Textures.MissingTile;
                    
                    var (hx, hy, hz) = ((TileBody)tileCell.Data).HeadPosition;

                    var supposedHead = GLOBALS.Level.TileMatrix[hy - 1, hx - 1, hz - 1];

                    if (supposedHead.Data is TileHead { Definition: null } or not TileHead)
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
                else if (!renderMaterials && tileCell.Type == TileType.Material) {
                    // var materialName = ((TileMaterial)tileCell.Data).Name;
                    var origin = new Vector2(x * scale + 5, y * scale + 5);
                    var color = GLOBALS.Level.MaterialColors[y, x, targetLayer];

                    color.A = (byte)((targetLayer == currentLayer) ? 255 : opacity);

                    if (color.R != 0 || color.G != 0 || color.B != 0)
                    {

                        switch (GLOBALS.Level.GeoMatrix[y, x, targetLayer].Geo)
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

        EndTextureMode();
    }

    internal static void DrawTileWithPalette(
        in TileDefinition init,
        in Texture2D palette,
        in Vector2 center, 
        Span<Vector2> quads,
        int depth = 0,
        float alpha = 1
    )
    {
        var scale = GLOBALS.Scale;
        var texture = init.Texture;

        if (init.Type == Data.Tiles.TileType.Box)
        {
            var shader = GLOBALS.Shaders.BoxTilePalette;

            var (tWidth, tHeight) = init.Size;
            var bufferPixels = init.BufferTiles * 20;
            
            var height = tHeight * 20;
            var offset = new Vector2(bufferPixels,tHeight*20*tWidth + bufferPixels);
            
            var calcHeight = (float)height / (float)texture.Height;
            var calcOffset = Raymath.Vector2Divide(offset, new(texture.Width, texture.Height));
            var calcWidth = (float)tWidth*20 / texture.Width;

            var textureLoc = GetShaderLocation(shader, "inputTexture");
            var paletteLoc = GetShaderLocation(shader, "paletteTexture");

            var widthLoc = GetShaderLocation(shader, "width");
            var heightLoc = GetShaderLocation(shader, "height");
            var offsetLoc = GetShaderLocation(shader, "offset");
            var depthLoc = GetShaderLocation(shader, "depth");
            var alphaLoc = GetShaderLocation(shader, "alpha");

            BeginShaderMode(shader);

            SetShaderValueTexture(shader, textureLoc, texture);
            SetShaderValueTexture(shader, paletteLoc, palette);
            
            SetShaderValue(shader, widthLoc, calcWidth, ShaderUniformDataType.Float);
            
            SetShaderValue(shader, heightLoc, calcHeight,
                ShaderUniformDataType.Float);
            
            SetShaderValue(shader, offsetLoc, calcOffset,
                ShaderUniformDataType.Vec2);
            
            SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
            SetShaderValue(shader, alphaLoc, alpha, ShaderUniformDataType.Float);

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
            var shader = GLOBALS.Shaders.TilePalette;

            var layerHeight = (init.Size.Item2 + (init.BufferTiles * 2)) * scale;
            float calLayerHeight = (float)layerHeight / (float)texture.Height;
            var textureCutWidth = (init.Size.Item1 + (init.BufferTiles * 2)) * scale;
            float calTextureCutWidth = (float)textureCutWidth / (float)texture.Width;

            var textureLoc = GetShaderLocation(shader, "inputTexture");
            var paletteLoc = GetShaderLocation(shader, "paletteTexture");

            var layerNumLoc = GetShaderLocation(shader, "layerNum");
            var layerHeightLoc = GetShaderLocation(shader, "layerHeight");
            var layerWidthLoc = GetShaderLocation(shader, "layerWidth");
            var depthLoc = GetShaderLocation(shader, "depth");
            var alphaLoc = GetShaderLocation(shader, "alpha");

            BeginShaderMode(shader);

            SetShaderValueTexture(shader, textureLoc, texture);
            SetShaderValueTexture(shader, paletteLoc, palette);

            SetShaderValue(shader, layerNumLoc, init.Type == Data.Tiles.TileType.VoxelStructRockType ? 1 : init.Repeat.Length,
                ShaderUniformDataType.Int);
            SetShaderValue(shader, layerHeightLoc, calLayerHeight,
                ShaderUniformDataType.Float);
            SetShaderValue(shader, layerWidthLoc, calTextureCutWidth,
                ShaderUniformDataType.Float);
            SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
            SetShaderValue(shader, alphaLoc, alpha, ShaderUniformDataType.Float);

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

    internal static void DrawTileWithPalette(
        in TileDefinition init,
        in Texture2D palette,
        PropQuads quads,
        int depth = 0,
        float alpha = 1
    )
    {
        var scale = GLOBALS.Scale;
        var texture = init.Texture;

        if (init.Type == Data.Tiles.TileType.Box)
        {
            var shader = GLOBALS.Shaders.BoxTilePalette;

            var height = (init.Size.Item2 + init.BufferTiles*2) * scale;
            var offset = new Vector2(init.Size.Item2 > 1 ? GLOBALS.Scale : 0, scale * init.Size.Item1 * init.Size.Item2);
            
            float calcHeight = (float)height / (float)texture.Height;
            Vector2 calcOffset = Raymath.Vector2Divide(offset, new(texture.Width, texture.Height));
            float calcWidth = (float)init.Size.Item1 * GLOBALS.Scale / texture.Width;
            
            var textureLoc = GetShaderLocation(shader, "inputTexture");
            var paletteLoc = GetShaderLocation(shader, "paletteTexture");

            var widthLoc = GetShaderLocation(shader, "width");
            var heightLoc = GetShaderLocation(shader, "height");
            var offsetLoc = GetShaderLocation(shader, "offset");
            var depthLoc = GetShaderLocation(shader, "depth");
            var alphaLoc = GetShaderLocation(shader, "alpha");

            BeginShaderMode(shader);

            SetShaderValueTexture(shader, textureLoc, texture);
            SetShaderValueTexture(shader, paletteLoc, palette);
            
            SetShaderValue(shader, widthLoc, calcWidth, ShaderUniformDataType.Float);
            
            SetShaderValue(shader, heightLoc, calcHeight,
                ShaderUniformDataType.Float);
            
            SetShaderValue(shader, offsetLoc, calcOffset,
                ShaderUniformDataType.Vec2);
            
            SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
            SetShaderValue(shader, alphaLoc, alpha, ShaderUniformDataType.Float);

            DrawTextureQuad(texture, quads);
            EndShaderMode();
        }
        else
        {
            var voxelShader = GLOBALS.Shaders.TilePalette;

            var layerHeight = (init.Size.Item2 + (init.BufferTiles * 2)) * scale;
            float calLayerHeight = (float)layerHeight / (float)texture.Height;
            var textureCutWidth = (init.Size.Item1 + (init.BufferTiles * 2)) * scale;
            float calTextureCutWidth = (float)textureCutWidth / (float)texture.Width;

            var textureLoc = GetShaderLocation(voxelShader, "inputTexture");
            var paletteLoc = GetShaderLocation(voxelShader, "paletteTexture");
            var layerNumLoc = GetShaderLocation(voxelShader, "layerNum");
            var layerHeightLoc = GetShaderLocation(voxelShader, "layerHeight");
            var layerWidthLoc = GetShaderLocation(voxelShader, "layerWidth");
            var depthLoc = GetShaderLocation(voxelShader, "depth");
            var alphaLoc = GetShaderLocation(voxelShader, "alpha");

            BeginShaderMode(voxelShader);

            SetShaderValueTexture(voxelShader, textureLoc, texture);
            SetShaderValueTexture(voxelShader, paletteLoc, palette);

            SetShaderValue(voxelShader, layerNumLoc, init.Type == Data.Tiles.TileType.VoxelStructRockType ? 1 : init.Repeat.Length,
                ShaderUniformDataType.Int);
            SetShaderValue(voxelShader, layerHeightLoc, calLayerHeight,
                ShaderUniformDataType.Float);
            SetShaderValue(voxelShader, layerWidthLoc, calTextureCutWidth,
                ShaderUniformDataType.Float);

            SetShaderValue(voxelShader, depthLoc, depth, ShaderUniformDataType.Int);

            SetShaderValue(voxelShader, alphaLoc, alpha, ShaderUniformDataType.Float);

            DrawTextureQuad(texture, quads);
            EndShaderMode();
        }
    }

    internal static void DrawStandardPropWithPalette(
        InitStandardProp init,
        in Texture2D texture, 
        in Texture2D palette,
        PropQuads quads,
        int depth
    )
    {
        var shader = GLOBALS.Shaders.StandardPropPalette;

        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var layerHeight = (float)texture.Height / (float)init.Repeat.Length;
        var calcLayerHeight = layerHeight / texture.Height;
        var calcWidth = (float) init.Size.x * GLOBALS.Scale / texture.Width;

        calcWidth = calcWidth > 1.00000f ? 1.0f : calcWidth;
        
        var textureLoc = GetShaderLocation(shader, "inputTexture");
        var paletteLoc = GetShaderLocation(shader, "paletteTexture");

        var layerNumLoc = GetShaderLocation(shader, "layerNum");
        var layerHeightLoc = GetShaderLocation(shader, "layerHeight");
        var widthLoc = GetShaderLocation(shader, "width");
        var depthLoc = GetShaderLocation(shader, "depth");

        BeginShaderMode(shader);
        
        SetShaderValueTexture(shader, textureLoc, texture);
        SetShaderValueTexture(shader, paletteLoc, palette);

        SetShaderValue(shader, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.Int);
        SetShaderValue(shader, layerHeightLoc, calcLayerHeight, ShaderUniformDataType.Float);
        SetShaderValue(shader, widthLoc, calcWidth, ShaderUniformDataType.Float);
        SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuad(texture, quads, flippedX, flippedY);

        EndShaderMode();
    }

    internal static void DrawVariedStandardPropWithPalette(
        InitVariedStandardProp init, 
        in Texture2D texture,
        in Texture2D palette,
        PropQuads quads,
        int variation,
        int depth
    )
    {
        var shader = GLOBALS.Shaders.VariedStandardPropPalette;

        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var layerHeight = (float) init.Size.y * GLOBALS.Scale;
        var variationWidth = (float) init.Size.x * GLOBALS.Scale;
        
        var calcLayerHeight = layerHeight / texture.Height;
        var calcVariationWidth = variationWidth / texture.Width;
        
        var textureLoc = GetShaderLocation(shader, "inputTexture");
        var paletteLoc = GetShaderLocation(shader, "paletteTexture");
        
        var layerNumLoc = GetShaderLocation(shader, "layerNum");
        var layerHeightLoc = GetShaderLocation(shader, "layerHeight");
        var variationWidthLoc = GetShaderLocation(shader, "varWidth");
        var variationLoc = GetShaderLocation(shader, "variation");
        var depthLoc = GetShaderLocation(shader, "depth");

        BeginShaderMode(shader);

        SetShaderValueTexture(shader, textureLoc, texture);
        SetShaderValueTexture(shader, paletteLoc, palette);
       
        SetShaderValue(shader, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.Int);
        SetShaderValue(shader, layerHeightLoc, calcLayerHeight, ShaderUniformDataType.Float);
        SetShaderValue(shader, variationWidthLoc, calcVariationWidth, ShaderUniformDataType.Float);
        SetShaderValue(shader, variationLoc, variation, ShaderUniformDataType.Int);
        SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuad(texture, quads, flippedX, flippedY);
        EndShaderMode();
    }

    internal static void DrawSoftPropWithPalette(in Texture2D texture, in Texture2D palette, in PropQuads quads, int depth)
    {
        var shader = GLOBALS.Shaders.SoftPropPalette;

        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var textureLoc = GetShaderLocation(shader, "inputTexture");
        var paletteLoc = GetShaderLocation(shader, "paletteTexture");
        
        var depthLoc = GetShaderLocation(shader, "depth");

        BeginShaderMode(shader);

        SetShaderValueTexture(shader, textureLoc, texture);
        SetShaderValueTexture(shader, paletteLoc, palette);
        
        SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuad(texture, quads, flippedX, flippedY);
        EndShaderMode();
    }

    internal static void DrawVariedSoftPropWithPalette(
        InitVariedSoftProp init, 
        in Texture2D texture,
        in Texture2D palette,
        PropQuads quads,
        int variation,
        int depth
    )
    {
        var shader = GLOBALS.Shaders.VariedSoftPropPalette;
        
        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var calcHeight = (float) init.SizeInPixels.y / texture.Height;
        var calcVariationWidth = (float) init.SizeInPixels.x / texture.Width;

        var textureLoc = GetShaderLocation(shader, "inputTexture");
        var paletteLoc = GetShaderLocation(shader, "paletteTexture");

        var heightLoc = GetShaderLocation(shader, "height");
        var variationWidthLoc = GetShaderLocation(shader, "varWidth");
        var variationLoc = GetShaderLocation(shader, "variation");
        var depthLoc = GetShaderLocation(shader, "depth");

        BeginShaderMode(shader);

        SetShaderValueTexture(shader, textureLoc, texture);
        SetShaderValueTexture(shader, paletteLoc, palette);

        SetShaderValue(shader, variationWidthLoc, calcVariationWidth,
            ShaderUniformDataType.Float);
        SetShaderValue(shader, heightLoc, calcHeight, ShaderUniformDataType.Float);
        SetShaderValue(shader, variationLoc, variation, ShaderUniformDataType.Int);
        SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
        
        DrawTextureQuad(texture, quads, flippedX, flippedY);
        EndShaderMode();
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

        /// <summary>
        /// Draws the navigation bar
        /// </summary>
        /// <returns>
        /// <para>0 - none selected</para>
        /// <para>1 - save selected</para>
        /// <para>2 - save as selected</para>
        /// <para>3 - render selected</para>
        /// </returns>
        internal static int Nav(out bool hovered)
        {
            var navBar = ImGuiNET.ImGui.BeginMainMenuBar();

            hovered = ImGuiNET.ImGui.IsWindowHovered();

            if (!navBar) return 0;

            var gShortcuts = GLOBALS.Settings.Shortcuts.GlobalShortcuts;

            var isMain = GLOBALS.Page == 1;
            var isGeo = GLOBALS.Page == 2;
            var isTile = GLOBALS.Page == 3;
            var isCamera = GLOBALS.Page == 4;
            var isLight = GLOBALS.Page == 5;
            var isDimensions = GLOBALS.Page == 6;
            var isEffects = GLOBALS.Page == 7;
            var isProps = GLOBALS.Page == 8;
            var isSettings = GLOBALS.Page == 9;
            
            if (ImGuiNET.ImGui.MenuItem("Main", string.Empty, ref isMain)) {GLOBALS.Page = 1;}
            if (ImGuiNET.ImGui.MenuItem("Geometry", string.Empty, ref isGeo)) {GLOBALS.Page = 2;}
            if (ImGuiNET.ImGui.MenuItem("Tiles", string.Empty, ref isTile)) {GLOBALS.Page = 3;}
            if (ImGuiNET.ImGui.MenuItem("Cameras", string.Empty, ref isCamera)) {GLOBALS.Page = 4;}
            if (ImGuiNET.ImGui.MenuItem("Light", string.Empty, ref isLight)) {GLOBALS.Page = 5;}
            if (ImGuiNET.ImGui.MenuItem("Dimensions", string.Empty, ref isDimensions)) {GLOBALS.Page = 6;}
            if (ImGuiNET.ImGui.MenuItem("Effects", string.Empty, ref isEffects)) {GLOBALS.Page = 7;}
            if (ImGuiNET.ImGui.MenuItem("Props", string.Empty, ref isProps)) {GLOBALS.Page = 8;}
            if (ImGuiNET.ImGui.MenuItem("Settings", string.Empty, ref isSettings)) {GLOBALS.Page = 9;}

            var selected = 0;
            
            if (ImGuiNET.ImGui.BeginMenu("Options"))
            {
                if (ImGuiNET.ImGui.MenuItem("Open..", gShortcuts.Open.ToString())) 
                {
                    GLOBALS.Page = 0;
                }

                if (ImGuiNET.ImGui.MenuItem("Save", gShortcuts.QuickSave.ToString()))
                    selected = 1;

                if (ImGuiNET.ImGui.MenuItem("Save as..", gShortcuts.QuickSaveAs.ToString()))
                {
                    GLOBALS.Page = 12;
                }

                if (ImGuiNET.ImGui.MenuItem("Render", gShortcuts.Render.ToString()))
                    selected = 3;
                
                ImGuiNET.ImGui.EndMenu();
            }

            if (ImGuiNET.ImGui.MenuItem("About"))
                selected = 4;
            
            ImGuiNET.ImGui.EndMainMenuBar();

            return selected;
        }
    }
}