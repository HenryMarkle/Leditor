using Raylib_cs;
using static Raylib_cs.Raylib;

using System.Numerics;
using System.Text;

using Leditor.Data;
using Leditor.Data.Tiles;
using Leditor.Data.Geometry;

using Color = Raylib_cs.Color;
using Leditor.Data.Materials;
using Leditor.Renderer.RL;
using Leditor.Data.Props.Legacy;

namespace Leditor.Renderer.Partial;

public partial class Engine
{
    /// <summary>
    /// Draws materials
    /// </summary>
    /// <param name="x">Matrix X coordinates</param>
    /// <param name="y">Matrix Y coordinates</param>
    /// <param name="layer">Current layer (0, 1, 2)</param>
    /// <param name="camera">The current rendering camera</param>
    /// <param name="mat">The material definition</param>
    /// <param name="rt">The render texture (canvas)</param>
    protected virtual void DrawMaterial_MTX(
        int x,
        int y,
        int layer,
        in RenderCamera camera,
        in MaterialDefinition mat,
        RenderTexture2D rt
    )
    {
        var sublayer = layer * 10;
        var cellRect = new Rectangle(x * 20, y * 20, 20, 20);

        var tileSetName = mat.Name;

        if (mat.Name == "Scaffolding" && Configuration.MaterialFixes)
        {
            tileSetName += "DR";
        }
        else if (mat.Name == "Invisible")
        {
            tileSetName = "SuperStructure";
        }

        var tileSet = State.tileSets.TryGetValue(tileSetName, out var foundTileSet) ? foundTileSet : mat.Texture;

        ref var cell = ref Level!.GeoMatrix[y, x, layer];

        switch (cell.Type)
        {
            case GeoType.Solid:
            {
                for (var f = 1; f <= 4; f++)
                {
                    (Vector2, Vector2) profL;
                    int gtAtV, gtAtH;
                    Rectangle pstRect;

                    switch (f)
                    {
                        case 1:
                            profL = (new(-1, 0), new(0, -1));
                            gtAtV = 2;
                            pstRect = cellRect with {
                                Width = cellRect.Width - 10,
                                Height = cellRect.Height -10
                            };
                            break;

                        case 2:
                            profL = (new(1, 0), new(0, -1));
                            gtAtV = 4;
                            pstRect = cellRect with {
                                X = cellRect.X + 10,
                                Width = cellRect.Width - 10,
                                Height = cellRect.Height -10
                            };
                            break;

                        case 3:
                            profL = (new(1, 0), new(0, 1));
                            gtAtV = 6;
                            pstRect = cellRect with {
                                X = cellRect.X + 10,
                                Y = cellRect.Y + 10,
                                Width = cellRect.Width - 10,
                                Height = cellRect.Height -10
                            };
                            break;

                        default:
                            profL = (new(-1, 0), new(0, 1));
                            gtAtV = 6;
                            pstRect = cellRect with {
                                X = cellRect.X,
                                Y = cellRect.Y + 10,
                                Width = cellRect.Width - 10,
                                Height = cellRect.Height -10
                            };
                            break;
                    }
                
                    (bool, bool) id = (
                        IsMyTileOpenToThisTile(mat, x + (int)profL.Item1.X, y + (int)profL.Item1.Y, layer), 
                        IsMyTileOpenToThisTile(mat, x + (int)profL.Item2.X, y + (int)profL.Item2.Y, layer) 
                    );

                    if (id is (true, true))
                    {
                        if (IsMyTileOpenToThisTile(mat, x + (int)(profL.Item1.X + profL.Item2.X), y + (int)(profL.Item1.Y + profL.Item2.Y), layer))
                        {
                            gtAtH = 10;
                            gtAtV = 2;
                        }
                        else
                        {
                            gtAtH = 8;
                        }
                    }
                    else
                    {
                        gtAtH = id switch {
                            (false, false) => 2,
                            (false, true) => 4,
                            (true, false) => 6,
                            _ => 0
                        };
                    }
                
                    // Don't even ask me what the fuck is this
                    if (gtAtH == 4)
                    {
                        if (gtAtV == 6)
                        {
                            gtAtV = 4;
                        }
                        else if (gtAtV == 8)
                        {
                            gtAtV = 2;
                        }
                    }
                    else if (gtAtH == 6)
                    {
                        if (gtAtV is 4 or 8)
                        {
                            gtAtV -= 2;
                        }
                    }

                    Rectangle gtRect = new(
                        (gtAtH - 1) * 10 - 5,
                        (gtAtV - 1) * 10 - 5,
                        20,
                        20
                    );

                    // pstRect = pstRect with {
                    //     X = pstRect.X - camera.Coords.X,
                    //     Y = pstRect.Y - camera.Coords.Y
                    // };

                    if (mat.Name != "Sand Block")
                    {
                        var shader = Shaders.WhiteRemover;

                        BeginTextureMode(rt);
                        {
                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tileSet);
                            DrawTexturePro(
                                tileSet,
                                gtRect,
                                pstRect with {
                                    X = pstRect.X - 5,
                                    Y = pstRect.Y - 5,
                                    Width = pstRect.Width + 10,
                                    Height = pstRect.Height + 10
                                },
                                Vector2.Zero,
                                0,
                                Color.White
                            );
                            EndShaderMode();
                        }
                        EndTextureMode();

                        for (var d = sublayer + 1; d <= sublayer + 9; d++)
                        {
                            BeginTextureMode(_layers[d]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tileSet);
                                DrawTexturePro(
                                    tileSet,
                                    gtRect with {
                                        X = gtRect.X + 120
                                    },
                                    pstRect with {
                                        X = pstRect.X - 5,
                                        Y = pstRect.Y - 5,
                                        Width = pstRect.Width + 10,
                                        Height = pstRect.Height + 10
                                    },
                                    Vector2.Zero,
                                    0,
                                    Color.White
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();
                        }
                    }
                }
            }
            break;
            
            case GeoType.SlopeNW:
            case GeoType.SlopeNE:
            case GeoType.SlopeSW:
            case GeoType.SlopeES:
            {
                var slope = cell.Type;

                (Vector2, Vector2) dir;

                if (Configuration.MaterialFixes)
                {
                    dir = slope switch {
                        GeoType.SlopeNE => (new(-1,  0), new(0,  1)),
                        GeoType.SlopeNW => (new( 0,  1), new(1,  0)),
                        GeoType.SlopeES => (new(-1,  0), new(0, -1)),
                        GeoType.SlopeSW => (new( 0, -1), new(1,  0)),
                        _ => (Vector2.Zero, Vector2.Zero)
                    };
                }
                else
                {
                    dir = slope switch {
                        GeoType.SlopeNE => (new(-1,  0), new(0,  1)),
                        GeoType.SlopeNW => (new( 1,  0), new(0,  1)),
                        GeoType.SlopeES => (new(-1,  0), new(0, -1)),
                        GeoType.SlopeSW => (new( 1,  0), new(0, -1)),
                        _ => (Vector2.Zero, Vector2.Zero)
                    };
                }

                Rectangle pstRect = new(
                    x * 20 + camera.Coords.X, 
                    y * 20 + camera.Coords.Y, 
                    20, 
                    20
                );

                // Expanded loop

                Rectangle gtRect;
                var shader = Shaders.WhiteRemover;

                // First iteration (i = dir.Item1)

                {
                    gtRect.X = 10;
                    gtRect.Y = 90 + 30 * ((int)cell.Type - 2);
                    gtRect.Width = 20;
                    gtRect.Height = 20;

                    if (IsMyTileOpenToThisTile(mat, x + (int)dir.Item1.X, y + (int)dir.Item1.Y, layer))
                    {
                        gtRect.X += 30;
                    }

                    gtRect.X -= 5;
                    gtRect.Y -= 5;
                    gtRect.Width += 10;
                    gtRect.Height += 10;

                    pstRect.X -= 5;
                    pstRect.Y -= 5;
                    pstRect.Width += 10;
                    pstRect.Height += 10;

                    if (mat.Name == "Scaffolding" && !Configuration.MaterialFixes)
                    {
                        gtRect.X += 120;
                        
                        SDraw.Draw_NoWhite_NoColor(_layers[sublayer + 5], tileSet, shader, gtRect, pstRect);
                        SDraw.Draw_NoWhite_NoColor(_layers[sublayer + 6], tileSet, shader, gtRect, pstRect);
                        SDraw.Draw_NoWhite_NoColor(_layers[sublayer + 8], tileSet, shader, gtRect, pstRect);
                        SDraw.Draw_NoWhite_NoColor(_layers[sublayer + 9], tileSet, shader, gtRect, pstRect);
                    }
                    else if (mat.Name != "Sand Block")
                    {
                        SDraw.Draw_NoWhite_NoColor(rt, tileSet, shader, gtRect, pstRect);

                        gtRect.X += 120;

                        for (var d = sublayer + 1; d <= sublayer + 9; d++)
                        {
                            SDraw.Draw_NoWhite_NoColor(_layers[d], tileSet, shader, gtRect, pstRect);
                        }
                    }
                }

                // Second iteration (i = dir.Item2)

                {
                    gtRect.X = 10;
                    gtRect.Y = 90 + 30 * ((int)cell.Type - 2);
                    gtRect.Width = 20;
                    gtRect.Height = 20;

                    if (IsMyTileOpenToThisTile(mat, x + (int)dir.Item2.X, y + (int)dir.Item2.Y, layer))
                    {
                        gtRect.X += 30;
                    }

                    gtRect.X -= 5;
                    gtRect.Y -= 5;
                    gtRect.Width += 10;
                    gtRect.Height += 10;

                    pstRect.X -= 5;
                    pstRect.Y -= 5;
                    pstRect.Width += 10;
                    pstRect.Height += 10;

                    if (mat.Name == "Scaffolding" && !Configuration.MaterialFixes)
                    {
                        gtRect.X += 120;
                        
                        SDraw.Draw_NoWhite_NoColor(_layers[sublayer + 5], tileSet, shader, gtRect, pstRect);
                        SDraw.Draw_NoWhite_NoColor(_layers[sublayer + 6], tileSet, shader, gtRect, pstRect);
                        SDraw.Draw_NoWhite_NoColor(_layers[sublayer + 8], tileSet, shader, gtRect, pstRect);
                        SDraw.Draw_NoWhite_NoColor(_layers[sublayer + 9], tileSet, shader, gtRect, pstRect);
                    }
                    else if (mat.Name != "Sand Block")
                    {
                        SDraw.Draw_NoWhite_NoColor(rt, tileSet, shader, gtRect, pstRect);

                        gtRect.X += 120;

                        for (var d = sublayer + 1; d <= sublayer + 9; d++)
                        {
                            SDraw.Draw_NoWhite_NoColor(_layers[d], tileSet, shader, gtRect, pstRect);
                        }
                    }
                }
            }
            break;

            case GeoType.Platform:
            if (mat.Name != "Invisible") {
                
                Rectangle pstRect = new(
                    x * 20 - camera.Coords.X,
                    y * 20 - camera.Coords.Y,
                    20,
                    20
                );

                if (mat.Name == "Stained Glass")
                {
                    DrawTile_MTX(Level!.TileMatrix[y, x, layer], State.SGFL!, x, y, layer, camera, rt);
                }
                else if (
                    Configuration.MaterialFixes ||
                    (mat.Name != "Sand Block" && mat.Name != "Scaffolding" && mat.Name != "Tiny Signs")
                )
                {
                    // Fine. You win.
                    DrawTile_MTX(
                        Level!.TileMatrix[y, x, layer], 
                        new TileDefinition(
                            $"tileSet{tileSetName}Floor", 
                            (1, 1), 
                            TileType.VoxelStruct, 
                            1, 
                            new int[0,0,0], 
                            [6, 1, 1, 1, 1], 
                            [], 1
                        ) { Texture = State.tileSets[$"tileSet{tileSetName}Floor"] },
                        x,
                        y,
                        layer,
                        camera,
                        rt
                    );
                }
                else
                {
                    DrawTile_MTX(
                        Level!.TileMatrix[y, x, layer],
                        State.tileSetBigMetalFloor!,
                        x,
                        y
                        ,
                        layer,
                        camera,
                        rt
                    );
                }
            }
            break;
        }
    
        var modder = mat.Name switch {
            "Concrete"          => 45,
            "RainStone"         => 6,
            "Bricks"            => 1,
            "Tiny Signs"        => 10,
            "Cliff"             => 45,
            "Non-Slip Metal"    => 5,
            "BulkMetal"         => 5,
            "MassiveBulkMetal"  => 10,
            "Asphalt"           => 45,
            
            _ => 0
        };

        if (modder is not 0)
        {
            Rectangle gtRect = new (
                (x % modder) * 20,
                (y % modder) * 20,
                20,
                20
            );

            if (mat.Name is "Bricks")
            {
                gtRect = new Rectangle(0, 0, 20, 20);
            }

            if (mat.Name is "Tiny Signs")
            {
                DrawTinySigns();
                _tinySignsDrawn = true;
            }

            if (cell[GeoType.Solid])
            {
                Rectangle pstRect = new (
                    x * 20,
                    y * 20,
                    20,
                    20
                );

                SDraw.Draw_NoWhite_NoColor(
                    _layers[sublayer],
                    mat.Texture,
                    Shaders.WhiteRemover,
                    gtRect,
                    pstRect
                );
            }
            else if (cell.IsSlope)
            {
                // This rectangle was not initialized within the scope
                // of this code block.
                // In fact, I do not know where the initialization is supposed to be.
                Rectangle pstRect = new(
                    x * 20 + camera.Coords.X, 
                    y * 20 + camera.Coords.Y, 
                    20, 
                    20
                );

                SDraw.Draw_NoWhite_NoColor(
                    _layers[sublayer],
                    mat.Texture,
                    Shaders.WhiteRemover,
                    gtRect,
                    pstRect
                );

                var pos = new Vector2(x, y) * 20;
                var offPos = new Vector2(x + 1, y + 1) * 20;

                var topLeft = pos;
                var bottomLeft = new Vector2(pos.X, offPos.Y);
                var topRight = new Vector2(offPos.X, pos.Y);
                var bottomRight = offPos;

                (Vector2, Vector2, Vector2) triangle = cell.Type switch {
                    // The triangles are in the reverse order because these are
                    // for cropping a square into a triangle.
                    GeoType.SlopeSW => (topLeft    , bottomRight, bottomLeft),
                    GeoType.SlopeES => (topRight   , bottomLeft , offPos    ),
                    GeoType.SlopeNW => (bottomLeft , topRight   , topLeft   ),
                    GeoType.SlopeNE => (bottomRight, topLeft    , topRight  ),
                    
                    _ => (Vector2.Zero, Vector2.Zero, Vector2.Zero)
                };

                // Translate to absolute position
                triangle.Item1 -= camera.Coords;
                triangle.Item2 -= camera.Coords;
                triangle.Item3 -= camera.Coords;
            
                BeginTextureMode(_layers[sublayer]);
                {
                    DrawTriangle(
                        triangle.Item1, 
                        triangle.Item2,
                        triangle.Item3,
                        Color.White
                    );
                }
                EndTextureMode();
            }
        }
    
        if (mat.Name is "Stained Glass")
        {
            // The original code was very confusing.

            modder = 1;

            var imgLoad = "SG";
            Rectangle gtRect = new (0, 0, 20, 20);

            var v = "1";
            var clr1 = "A";
            var clr2 = "B";

            var x2 = x + camera.Coords.X;
            var y2 = y + camera.Coords.Y;
        
            foreach (var effect in Level!.Effects)
            {
                if (effect.Name is not "Stained Glass Properties") continue;
                if (effect.Matrix[y, x] < 0) continue;

                var varOpt = effect.Options.FirstOrDefault(o => o.Name is "Variation");
                var clr1Opt = effect.Options.FirstOrDefault(o => o.Name is "Color 1");
                var clr2Opt = effect.Options.FirstOrDefault(o => o.Name is "Color 2");
            
                if (varOpt is { Choice: string })
                {
                    v = varOpt.Choice is "1" or "2" or "3" 
                        ? varOpt.Choice 
                        : "1";
                }

                if (clr1Opt is { Choice: string })
                {
                    clr1 = clr1Opt.Choice switch 
                    {
                        "EffectColor1" => "A",
                        "EffectColor2" => "B",
                        "None" => "C",
                        _ => "A"
                    };
                }

                if (clr2Opt is { Choice: string })
                {
                    clr2 = clr2Opt.Choice switch 
                    {
                        "EffectColor1" => "A",
                        "EffectColor2" => "B",
                        "None" => "C",
                        _ => "B"
                    };
                }

                break;
            }
        
            // Really bad, but temporary.

            var lib = Registry.CastLibraries["Drought"];

            var textureSocket = lib[$"{imgLoad}{v}Socket"].Texture;
            var textureGrad = lib[$"{imgLoad}{v}Grad"].Texture;
            var textureClr = lib[$"{imgLoad}{v}{clr1}{clr2}"].Texture;

            // The original code duplicated this section three times and then cropped
            // the cell to match the shape.

            Rectangle pstRect = new(x * 20, y * 20, 20, 20);

            pstRect.X -= camera.Coords.X;
            pstRect.Y -= camera.Coords.Y;

            SDraw.Draw_NoWhite_Color(
                _layers[sublayer],
                textureSocket,
                Shaders.WhiteRemoverApplyColor,
                gtRect,
                pstRect,
                Color.Green
            );

            SDraw.Draw_NoWhite_Color(
                _layers[sublayer + 1],
                textureSocket,
                Shaders.WhiteRemoverApplyColor,
                gtRect,
                pstRect,
                Color.Green
            );

            SDraw.Draw_NoWhite_Color(
                _layers[sublayer + 1],
                textureClr,
                Shaders.WhiteRemoverApplyColor,
                gtRect,
                pstRect,
                Color.Green
            );

            BeginTextureMode(_gradientA[sublayer + 1]);
            Draw.DrawTextureDarkest(
                textureGrad,
                gtRect,
                pstRect
            );
            EndTextureMode();

            BeginTextureMode(_gradientB[sublayer + 1]);
            Draw.DrawTextureDarkest(
                textureGrad,
                gtRect,
                pstRect
            );
            EndTextureMode();
            
            if (cell.IsSlope)
            {
                // Code copied from earlier

                var pos = new Vector2(x, y) * 20;
                var offPos = new Vector2(x + 1, y + 1) * 20;

                var topLeft = pos;
                var bottomLeft = new Vector2(pos.X, offPos.Y);
                var topRight = new Vector2(offPos.X, pos.Y);
                var bottomRight = offPos;

                (Vector2, Vector2, Vector2) triangle = cell.Type switch {
                    // The triangles are in the reverse order because these are
                    // for cropping a square into a triangle.
                    GeoType.SlopeSW => (topLeft    , bottomRight, bottomLeft),
                    GeoType.SlopeES => (topRight   , bottomLeft , offPos    ),
                    GeoType.SlopeNW => (bottomLeft , topRight   , topLeft   ),
                    GeoType.SlopeNE => (bottomRight, topLeft    , topRight  ),
                    
                    _ => (Vector2.Zero, Vector2.Zero, Vector2.Zero)
                };

                // Translate to absolute position
                triangle.Item1 -= camera.Coords;
                triangle.Item2 -= camera.Coords;
                triangle.Item3 -= camera.Coords;

                // Expanded loop

                BeginTextureMode(_layers[sublayer]);
                {
                    DrawTriangle(
                        triangle.Item1,
                        triangle.Item2,
                        triangle.Item3,
                        Color.White
                    );
                }
                EndTextureMode();

                BeginTextureMode(_layers[sublayer + 1]);
                {
                    DrawTriangle(
                        triangle.Item1,
                        triangle.Item2,
                        triangle.Item3,
                        Color.White
                    );
                }
                EndTextureMode();
            }
            else if (cell.Type is GeoType.Platform)
            {
                Rectangle halfACell = new(x * 20, y * 20 + 10, 20, 20);

                halfACell.X -= camera.Coords.X;
                halfACell.Y -= camera.Coords.Y;

                // Expanded loop

                BeginTextureMode(_layers[sublayer]);
                {
                    DrawRectangleRec(halfACell, Color.White);
                }
                EndTextureMode();

                BeginTextureMode(_layers[sublayer + 1]);
                {
                    DrawRectangleRec(halfACell, Color.White);
                }
                EndTextureMode();
            }
        }
    
        // At last. The final battle
        else if (mat.Name is "Sand Block")
        {
            modder = 28;

            Rectangle gtRect = new (
                (x % modder) * 20 - camera.Coords.X,
                (y % modder) * 20 - camera.Coords.Y,
                20,
                20
            );

            switch (cell.Type)
            {
                case GeoType.Solid:
                {
                    var rnd = Random.Generate(4);
                    Rectangle pstRect = new (
                        x * 20 - camera.Coords.X,
                        y * 20 - camera.Coords.Y,
                        20,
                        20
                    );

                    for (var d = 0; d <= 9; d++)
                    {
                        var texture = Registry.CastLibraries["Drought"][$"Sand BlockTexture{Random.Generate(4)}"].Texture;

                        SDraw.Draw_NoWhite_NoColor(
                            _layers[sublayer + d],
                            texture,
                            Shaders.WhiteRemover,
                            gtRect,
                            pstRect
                        );
                    }
                }
                break;

                case GeoType.SlopeNE:
                case GeoType.SlopeNW:
                case GeoType.SlopeES:
                case GeoType.SlopeSW:
                {
                    var rnd = Random.Generate(4);
                    Rectangle pstRect = new (
                        x * 20 - camera.Coords.X,
                        y * 20 - camera.Coords.Y,
                        20,
                        20
                    );

                    for (var d = 0; d <= 9; d++)
                    {
                        SDraw.Draw_NoWhite_NoColor(
                            _layers[d + sublayer],
                            Registry.CastLibraries["Drought"][$"Sand BlockTexture{Random.Generate(4)}"].Texture,
                            Shaders.WhiteRemover,
                            gtRect,
                            pstRect
                        );
                    }

                    var pos = new Vector2(x, y) * 20;
                    var offPos = new Vector2(x + 1, y + 1) * 20;

                    var topLeft = pos;
                    var bottomLeft = new Vector2(pos.X, offPos.Y);
                    var topRight = new Vector2(offPos.X, pos.Y);
                    var bottomRight = offPos;

                    (Vector2, Vector2, Vector2) triangle = cell.Type switch {
                        // The triangles are in the reverse order because these are
                        // for cropping a square into a triangle.
                        GeoType.SlopeSW => (topLeft    , bottomRight, bottomLeft),
                        GeoType.SlopeES => (topRight   , bottomLeft , offPos    ),
                        GeoType.SlopeNW => (bottomLeft , topRight   , topLeft   ),
                        GeoType.SlopeNE => (bottomRight, topLeft    , topRight  ),
                        
                        _ => (Vector2.Zero, Vector2.Zero, Vector2.Zero)
                    };

                    // Translate to absolute position
                    triangle.Item1 -= camera.Coords;
                    triangle.Item2 -= camera.Coords;
                    triangle.Item3 -= camera.Coords;

                    for (var d = 0; d <= 9; d++)
                    {
                        DrawTriangle(
                            triangle.Item1,
                            triangle.Item2,
                            triangle.Item3,
                            Color.White
                        );
                    }
                }
                break;
            
                case GeoType.Platform:
                {
                    Rectangle halfACell = new (
                        x * 20      - camera.Coords.X, 
                        y * 20 + 10 - camera.Coords.Y,
                        20, 
                        20
                    );

                    for (var d = 0; d <= 9; d++)
                    {
                        SDraw.Draw_NoWhite_NoColor(
                            _layers[sublayer + d],
                            Registry.CastLibraries["Drought"][$"Sand BlockTexture{Random.Generate(4)}"].Texture,
                            Shaders.WhiteRemover,
                            gtRect,
                            halfACell
                        );
                    }
                }
                break;
            }
        }
    }

    protected virtual bool CheckIfAMaterialIsSolidAndSameMaterial(
        int x, int y, int layer,
        MaterialDefinition? mat
    ) {
        x = Utils.Restrict(x, 0, Level!.Width  - 1);
        y = Utils.Restrict(y, 0, Level!.Height - 1);

        if (Level!.GeoMatrix[y, x, layer] is not { Type: GeoType.Solid }) return false;

        ref var cell = ref Level!.TileMatrix[y, x, layer];

        return cell.Type is TileCellType.Material && cell.MaterialDefinition == mat 
            || cell.Type is TileCellType.Default && Level!.DefaultMaterial == mat; 
    }

    protected virtual void AttemptDrawTempleStone_MTX(
        int x,
        int y,
        int layer,
        RenderCamera camera,
        List<(int x, int y)> list,
        List<(int x, int y)>[] corners,
        TileDefinition tile,
        RenderTexture2D rt
    )
    {
        List<(int x, int y)> occupy = [];

        switch (tile.Name)
        {
            case "Big Temple Stone No Slopes":
            occupy = [ (-1, 0), (0, -1), (0, 0), (0, 1), (1, -1), (1, 0), (1, 1), (2, 0) ];
            break;

            case "Wide Temple Stone":
            occupy = [ (0, 0), (1, 0) ];
            break;
        }

        foreach (var o in occupy)
        {
            if (!CheckIfAMaterialIsSolidAndSameMaterial(x + o.x, y + o.y, layer, State.templeStone)) return;
        }

        DrawTile_MTX(
            Level!.TileMatrix[y, x, layer],
            tile,
            x,
            y,
            layer,
            camera,
            rt
        );

        if (tile.Name is "Big Temple Stone No Slopes")
        {
            corners[0].Add((x - 1, y - 1));
            corners[1].Add((x + 2, y - 1));
            corners[2].Add((x + 2, y + 1));
            corners[3].Add((x - 1, y + 1));
        }

        foreach (var o in occupy) list.Remove((x + o.x, y + o.y));
    }

    /// <summary>
    /// Draws a material of "tile" render type.
    /// </summary>
    /// <param name="layer">The current layer</param>
    /// <param name="camera">The current rendering camera</param>
    /// <param name="mat">The material definition</param>
    /// <param name="rt">The render texture</param>
    protected virtual void RenderTileMaterial_MTX(
        int layer,
        in RenderCamera camera,
        in MaterialDefinition mat,
        in RenderTexture2D rt
    ) {
        List<(int rnd, int x, int y)> orderedTiles = new(Level!.Width * Level!.Height);

        for (var mx = 0; mx < Level!.Width; mx++)
        {
            for (var my = 0; my < Level!.Height; my++)
            {
                ref var geoCell = ref Level!.GeoMatrix[my, mx, layer];

                if (geoCell[GeoType.Air]) continue;

                ref var tileCell = ref Level!.TileMatrix[my, mx, layer];

                var validPlacement = 
                    (tileCell.Type is TileCellType.Material && tileCell.MaterialDefinition == mat) 
                    || 
                    (tileCell.Type is TileCellType.Default && Level!.DefaultMaterial == mat);

                if (!validPlacement) continue;

                if (
                    geoCell[GeoType.Solid] ||
                    Configuration.MaterialFixes 
                        && mat.Name is 
                        not "Tiled Stone" 
                        or "Chaotic Stone" 
                        or "Random Machines" 
                        or "3DBricks"
                )
                {
                    orderedTiles.Add((Random.Generate(Level!.Width + Level!.Height), mx, my));
                }
                else if (CheckCollisionPointRec(new Vector2(mx, my), new Rectangle(camera.Coords/20, new Vector2(100, 60))))
                {
                    DrawMaterial_MTX(mx, my, layer, camera, Registry.Materials!.Get("Standard"), rt);
                }
            }
        }

        orderedTiles.Sort((l1, l2) => {
            if (l1.rnd > l2.rnd) return 1;
            if (l2.rnd > l1.rnd) return -1;
            if (l1.rnd == l2.rnd) return 0;
            return 0;
        });

        switch (mat.Name)
        {
            case "Chaotic Stone":
            {
                bool[,] waiting = new bool[Level!.Height, Level!.Width];
                bool[,] deleted = new bool[Level!.Height, Level!.Width];

                // Draw Chaotic Stone corners and platoform
                if (Configuration.MaterialFixes)
                {
                    var SmallStoneSlopeNE = Registry.Tiles!.Get("Small Stone Slope NE");
                    var SmallStoneSlopeNW = Registry.Tiles!.Get("Small Stone Slope NW");
                    var SmallStoneSlopeSW = Registry.Tiles!.Get("Small Stone Slope SW");
                    var SmallStoneSlopeSE = Registry.Tiles!.Get("Small Stone Slope SE");
                    var SmallStoneFloor = Registry.Tiles!.Get("Small Stone Floor");

                    for (var q = orderedTiles.Count; q >= 0; q--)
                    {
                        var queued = orderedTiles[q];

                        switch (Level!.GeoMatrix[queued.y, queued.x, layer].Type)
                        {
                            case GeoType.SlopeNE:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeNE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeNW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeNW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeES:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeSE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeSW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeSW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Platform:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneFloor, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Solid:
                            waiting[queued.y, queued.x] = true;
                            continue;
                        }

                        orderedTiles.Remove(queued);
                        deleted[queued.y, queued.x] = true;
                    }
                }
            
                var SquareStone = Registry.Tiles!.Get("Square Stone");
                var SmallStone = Registry.Tiles!.Get("Small Stone");

                foreach (var (_, mx, my) in orderedTiles)
                {
                    // At this point, all deleted tiles should not appear in the list.
                    // if (deleted[my, mx]) continue;

                    var hts = 0;
                    
                    // Expanded loop

                    { // First iteration (dir = point(1, 0))

                        hts += Utils.BoolInt(orderedTiles.Any(v => v == (v.rnd, v.x + 1, v.y))) * Utils.BoolInt(deleted[my, mx+1]);
                    }
                    { // Second iteration (dir = point(0, 1))

                        hts += Utils.BoolInt(orderedTiles.Any(v => v == (v.rnd, v.x, v.y + 1))) * Utils.BoolInt(deleted[my+1, mx]);
                    }
                    { // Third iteration (dir = point(1, 1))

                        hts += Utils.BoolInt(orderedTiles.Any(v => v == (v.rnd, v.x + 1, v.y + 1))) * Utils.BoolInt(deleted[my+1, mx+1]);
                    }

                    // Big boy (2 x 2)
                    if (hts is not 3) continue;

                    // Check if the tile is in the camera bounds
                    if (CheckCollisionPointRec(new Vector2(mx, my), new Rectangle(camera.Coords/20, new Vector2(100, 60))))
                    {
                        DrawTile_MTX(
                            Level!.TileMatrix[my, mx, layer],
                            SquareStone,
                            mx,
                            my,
                            layer,
                            camera,
                            rt
                        );
                    }

                    // Now mark the cells as used or unavailable

                    // Expanded loop

                    { // First iteration (dir = point(1, 0))
                        deleted[mx+1, my] = true;
                        waiting[mx+1, my] = false;
                    }
                    { // Second iteration (dir = point(0, 1))
                        deleted[mx, my+1] = true;
                        waiting[mx, my+1] = false;
                    }
                    { // Third iteration (dir = point(1, 1))
                        deleted[mx+1, my+1] = true;
                        waiting[mx+1, my+1] = false;
                    }

                    deleted[my, mx] = true;
                    waiting[mx, my] = false;
                }
            
                orderedTiles = orderedTiles.Where(t => !deleted[t.y, t.x]).ToList();

                while (orderedTiles.Count > 0)
                {
                    var index = Random.Generate(orderedTiles.Count) - 1;
                    var (_, tx, ty) = orderedTiles[index];
                    
                    // Check if it's in camera bounds
                    if (CheckCollisionPointRec(new Vector2(tx, ty), new Rectangle(camera.Coords/20, new Vector2(100, 60))))
                    {
                        DrawTile_MTX(
                            Level!.TileMatrix[ty, tx, layer],
                            SmallStone,
                            tx,
                            ty,
                            layer,
                            camera,
                            rt
                        );
                    }

                    orderedTiles.RemoveAt(index);
                }
            }
            break;
        
            // Same as "Chaotic Stone", but without placing Square Stone.
            case "Tiled Stone":
            {
                bool[,] waiting = new bool[Level!.Height, Level!.Width];
                bool[,] deleted = new bool[Level!.Height, Level!.Width];

                // Draw Chaotic Stone corners and platoform
                if (Configuration.MaterialFixes)
                {
                    var SmallStoneSlopeNE = Registry.Tiles!.Get("Small Stone Slope NE");
                    var SmallStoneSlopeNW = Registry.Tiles!.Get("Small Stone Slope NW");
                    var SmallStoneSlopeSW = Registry.Tiles!.Get("Small Stone Slope SW");
                    var SmallStoneSlopeSE = Registry.Tiles!.Get("Small Stone Slope SE");
                    var SmallStoneFloor = Registry.Tiles!.Get("Small Stone Floor");

                    for (var q = orderedTiles.Count - 1; q >= 0; q--)
                    {
                        var queued = orderedTiles[q];

                        switch (Level!.GeoMatrix[queued.y, queued.x, layer].Type)
                        {
                            case GeoType.SlopeNE:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeNE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeNW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeNW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeES:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeSE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeSW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeSW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Platform:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneFloor, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Solid:
                            waiting[queued.y, queued.x] = true;
                            continue;
                        }

                        orderedTiles.Remove(queued);
                        deleted[queued.y, queued.x] = true;
                    }
                }
            
                var SmallStone = Registry.Tiles!.Get("Small Stone");

                while (orderedTiles.Count > 0)
                {
                    var index = Random.Generate(orderedTiles.Count);
                    var (_, tx, ty) = orderedTiles[index];
                    
                    // Check if it's in camera bounds
                    if (CheckCollisionPointRec(new Vector2(tx, ty), new Rectangle(camera.Coords/20, new Vector2(100, 60))))
                    {
                        DrawTile_MTX(
                            Level!.TileMatrix[ty, tx, layer],
                            SmallStone,
                            tx,
                            ty,
                            layer,
                            camera,
                            rt
                        );
                    }

                    orderedTiles.RemoveAt(index);
                }
            }
            break;
            
            // The reason I grouped all three together
            // is because all of them are pulling from the 
            // exact same tile pool.
            case "Random Machines":
            case "Random Machines 2":
            case "Small Machines":
            {
                bool[,] waiting = new bool[Level!.Height, Level!.Width];
                bool[,] deleted = new bool[Level!.Height, Level!.Width];

                // Draw Chaotic Stone corners and platoform
                if (Configuration.MaterialFixes)
                {
                    var SmallMachineSlopeNE = Registry.Tiles!.Get("Small Machine Slope NE");
                    var SmallMachineSlopeNW = Registry.Tiles!.Get("Small Machine Slope NW");
                    var SmallMachineSlopeSW = Registry.Tiles!.Get("Small Machine Slope SW");
                    var SmallMachineSlopeSE = Registry.Tiles!.Get("Small Machine Slope SE");
                    var SmallMachineFloor = Registry.Tiles!.Get("Small Machine Floor");

                    for (var q = orderedTiles.Count - 1; q >= 0; q--)
                    {
                        var queued = orderedTiles[q];

                        switch (Level!.GeoMatrix[queued.y, queued.x, layer].Type)
                        {
                            case GeoType.SlopeNE:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMachineSlopeNE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeNW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMachineSlopeNW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeES:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMachineSlopeSE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeSW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMachineSlopeSW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Platform:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMachineFloor, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Solid:
                            waiting[queued.y, queued.x] = true;
                            continue;
                        }

                        orderedTiles.RemoveAt(orderedTiles.Count - q);
                        deleted[queued.y, queued.x] = true;
                    }
                }

                // You could probably turn them into hash sets.
                Dictionary<(int x, int y), bool> delL = [];
                Dictionary<(int x, int y), bool> blocks = [];

                foreach (var (_, mx, my) in orderedTiles) blocks[(mx, my)] = true;

                foreach (var (_, mx, my) in orderedTiles)
                {
                    if (delL.ContainsKey((mx, my))) continue;

                    List<(int rnd, TileDefinition tile)> randomMachines = State.randomMachinesPool
                        .Select(t => (Random.Generate(1000), t))
                        .ToList();

                    randomMachines.Sort((r1, r2) => {
                        if (r1.rnd >  r2.rnd) return  1;
                        if (r1.rnd <  r2.rnd) return -1;
                        if (r1.rnd == r2.rnd) return  0;
                        return 0;
                    });

                    foreach (var (_, tile) in randomMachines)
                    {
                        // Testing if we can place the tile.

                        var legal = true;

                        for (var sx = 0; sx < tile.Size.Width; sx++)
                        {
                            for (var sy = 0; sy < tile.Size.Height; sy++)
                            {
                                var tx = mx + sx;
                                var ty = my + sy;

                                var spec = tile.Specs[sy, sx, 0];

                                if (!blocks.ContainsKey((tx, ty)))
                                {
                                    legal = false;
                                    goto stop_testing;
                                }

                                if (spec is -1) continue;

                                if (delL.ContainsKey((tx, ty)))
                                {
                                    legal = false;
                                    goto stop_testing;
                                }

                                if (
                                    !Data.Utils.InBounds(Level!.GeoMatrix, tx, ty) ||
                                    spec != (int)Level!.GeoMatrix[ty, tx, layer].Type
                                ) {
                                    legal = false;
                                    goto stop_testing;
                                }
                            }
                        }

                        stop_testing:

                        if (!legal) continue;

                        // Determining the tile head position, I suppose.
                        var rootPosX = mx + (int)(tile.Size.Width /2f + 0.4999f) - 1;
                        var rootPosY = my + (int)(tile.Size.Height/2f + 0.4999f) - 1;

                        // Drawing the tile if it's in camera bounds.
                        if (CheckCollisionPointRec(new Vector2(rootPosX, rootPosY), new Rectangle(camera.Coords/20f, new Vector2(100, 60))))
                        {
                            DrawTile_MTX(
                                Level!.TileMatrix[rootPosY, rootPosX, layer],
                                tile,
                                rootPosX,
                                rootPosY,
                                layer,
                                camera,
                                rt
                            );
                        }

                        for (var w = 0; w < tile.Size.Width; w++)
                        {
                            for (var h = 0; h < tile.Size.Height; h++)
                            {
                                var spec = tile.Specs[h, w, 0];

                                if (spec is not -1) delL[(mx + w, my + h)] = true;
                            }
                        }

                        break;
                    }

                }
            }
            break;

            // The code was basically copied from previous cases.
            case "Random Metal":
            {
                bool[,] waiting = new bool[Level!.Height, Level!.Width];
                bool[,] deleted = new bool[Level!.Height, Level!.Width];

                // Draw Chaotic Stone corners and platoform
                if (Configuration.MaterialFixes)
                {
                    var SmallMetalSlopeNE = Registry.Tiles!.Get("Small Metal Slope NE");
                    var SmallMetalSlopeNW = Registry.Tiles!.Get("Small Metal Slope NW");
                    var SmallMetalSlopeSW = Registry.Tiles!.Get("Small Metal Slope SW");
                    var SmallMetalSlopeSE = Registry.Tiles!.Get("Small Metal Slope SE");
                    var SmallMetalFloor = Registry.Tiles!.Get("Small Metal Floor");

                    for (var q = orderedTiles.Count - 1; q >= 0; q--)
                    {
                        var queued = orderedTiles[q];

                        switch (Level!.GeoMatrix[queued.y, queued.x, layer].Type)
                        {
                            case GeoType.SlopeNE:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalSlopeNE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeNW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalSlopeNW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeES:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalSlopeSE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeSW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalSlopeSW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Platform:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalFloor, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Solid:
                            waiting[queued.y, queued.x] = true;
                            continue;
                        }

                        orderedTiles.RemoveAt(orderedTiles.Count - q);
                        deleted[queued.y, queued.x] = true;
                    }
                }



                // You could probably turn them into hash sets.
                Dictionary<(int x, int y), bool> delL = [];
                Dictionary<(int x, int y), bool> blocks = [];

                foreach (var (_, mx, my) in orderedTiles) blocks[(mx, my)] = true;

                foreach (var (_, mx, my) in orderedTiles)
                {
                    if (delL.ContainsKey((mx, my))) continue;

                    List<(int rnd, TileDefinition tile)> randomMetals = State.randomMetalPool
                        .Select(t => (Random.Generate(1000), t))
                        .ToList();

                    randomMetals.Sort((r1, r2) => {
                        if (r1.rnd >  r2.rnd) return  1;
                        if (r1.rnd <  r2.rnd) return -1;
                        if (r1.rnd == r2.rnd) return  0;
                        return 0;
                    });

                    foreach (var (_, tile) in randomMetals)
                    {
                        // Testing if we can place the tile.

                        var legal = true;

                        for (var sx = 0; sx < tile.Size.Width; sx++)
                        {
                            for (var sy = 0; sy < tile.Size.Height; sy++)
                            {
                                var tx = mx + sx;
                                var ty = my + sy;

                                var spec = tile.Specs[sy, sx, 0];

                                if (!blocks.ContainsKey((tx, ty)))
                                {
                                    legal = false;
                                    goto stop_testing;
                                }

                                if (spec is -1) continue;

                                if (delL.ContainsKey((tx, ty)))
                                {
                                    legal = false;
                                    goto stop_testing;
                                }

                                if (
                                    !Data.Utils.InBounds(Level!.GeoMatrix, tx, ty) ||
                                    spec != (int)Level!.GeoMatrix[ty, tx, layer].Type
                                ) {
                                    legal = false;
                                    goto stop_testing;
                                }
                            }
                        }

                        stop_testing:

                        if (!legal) continue;

                        // Determining the tile head position, I suppose.
                        var rootPosX = mx + (int)(tile.Size.Width /2f + 0.4999f) - 1;
                        var rootPosY = my + (int)(tile.Size.Height/2f + 0.4999f) - 1;

                        // Drawing the tile if it's in camera bounds.
                        if (CheckCollisionPointRec(new Vector2(rootPosX, rootPosY), new Rectangle(camera.Coords/20f, new Vector2(100, 60))))
                        {
                            DrawTile_MTX(
                                Level!.TileMatrix[rootPosY, rootPosX, layer],
                                tile,
                                rootPosX,
                                rootPosY,
                                layer,
                                camera,
                                rt
                            );
                        }

                        for (var w = 0; w < tile.Size.Width; w++)
                        {
                            for (var h = 0; h < tile.Size.Height; h++)
                            {
                                var spec = tile.Specs[h, w, 0];

                                if (spec is not -1) delL[(mx + w, my + h)] = true;
                            }
                        }

                        break;
                    }

                }
            }
            break;
        
            // The code was basically copied from previous cases.
            case "Chaotic Stone 2":
            {
                bool[,] waiting = new bool[Level!.Height, Level!.Width];
                bool[,] deleted = new bool[Level!.Height, Level!.Width];

                
                // Draw Chaotic Stone corners and platoform
                if (Configuration.MaterialFixes)
                {
                    var SmallStoneSlopeNE = Registry.Tiles!.Get("Small Stone Slope NE");
                    var SmallStoneSlopeNW = Registry.Tiles!.Get("Small Stone Slope NW");
                    var SmallStoneSlopeSW = Registry.Tiles!.Get("Small Stone Slope SW");
                    var SmallStoneSlopeSE = Registry.Tiles!.Get("Small Stone Slope SE");
                    var SmallStoneFloor = Registry.Tiles!.Get("Small Stone Floor");

                    for (var q = orderedTiles.Count - 1; q >= 0; q--)
                    {
                        var queued = orderedTiles[q];

                        switch (Level!.GeoMatrix[queued.y, queued.x, layer].Type)
                        {
                            case GeoType.SlopeNE:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeNE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeNW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeNW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeES:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeSE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeSW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneSlopeSW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Platform:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallStoneFloor, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Solid:
                            waiting[queued.y, queued.x] = true;
                            continue;
                        }

                        orderedTiles.RemoveAt(orderedTiles.Count - q);
                        deleted[queued.y, queued.x] = true;
                    }
                }

                // You could probably turn them into hash sets.
                Dictionary<(int x, int y), bool> delL = [];
                Dictionary<(int x, int y), bool> blocks = [];

                foreach (var (_, mx, my) in orderedTiles) blocks[(mx, my)] = true;

                foreach (var (_, mx, my) in orderedTiles)
                {
                    if (delL.ContainsKey((mx, my))) continue;

                    List<(int rnd, TileDefinition tile)> chaoticStone2 = State.chaoticStone2Pool
                        .Select(t => (Random.Generate(1000), t))
                        .ToList();

                    chaoticStone2.Sort((r1, r2) => {
                        if (r1.rnd >  r2.rnd) return  1;
                        if (r1.rnd <  r2.rnd) return -1;
                        if (r1.rnd == r2.rnd) return  0;
                        return 0;
                    });

                    foreach (var (_, tile) in chaoticStone2)
                    {
                        // Testing if we can place the tile.

                        var legal = true;

                        for (var sx = 0; sx < tile.Size.Width; sx++)
                        {
                            for (var sy = 0; sy < tile.Size.Height; sy++)
                            {
                                var tx = mx + sx;
                                var ty = my + sy;

                                var spec = tile.Specs[sy, sx, 0];

                                if (!blocks.ContainsKey((tx, ty)))
                                {
                                    legal = false;
                                    goto stop_testing;
                                }

                                if (spec is -1) continue;

                                if (delL.ContainsKey((tx, ty)))
                                {
                                    legal = false;
                                    goto stop_testing;
                                }

                                if (
                                    !Data.Utils.InBounds(Level!.GeoMatrix, tx, ty) ||
                                    spec != (int)Level!.GeoMatrix[ty, tx, layer].Type
                                ) {
                                    legal = false;
                                    goto stop_testing;
                                }
                            }
                        }

                        stop_testing:

                        if (!legal) continue;

                        // Determining the tile head position, I suppose.
                        var rootPosX = mx + (int)(tile.Size.Width /2f + 0.4999f) - 1;
                        var rootPosY = my + (int)(tile.Size.Height/2f + 0.4999f) - 1;

                        // Drawing the tile if it's in camera bounds.
                        if (CheckCollisionPointRec(new Vector2(rootPosX, rootPosY), new Rectangle(camera.Coords/20f, new Vector2(100, 60))))
                        {
                            DrawTile_MTX(
                                Level!.TileMatrix[rootPosY, rootPosX, layer],
                                tile,
                                rootPosX,
                                rootPosY,
                                layer,
                                camera,
                                rt
                            );
                        }

                        for (var w = 0; w < tile.Size.Width; w++)
                        {
                            for (var h = 0; h < tile.Size.Height; h++)
                            {
                                var spec = tile.Specs[h, w, 0];

                                if (spec is not -1) delL[(mx + w, my + h)] = true;
                            }
                        }

                        break;
                    }

                }
            }
            break;
        
            case "Random Metals":
            {
                bool[,] waiting = new bool[Level!.Height, Level!.Width];
                bool[,] deleted = new bool[Level!.Height, Level!.Width];

                // Draw Chaotic Stone corners and platoform
                if (Configuration.MaterialFixes)
                {
                    var SmallMetalSlopeNE = Registry.Tiles!.Get("Small Metal Slope NE");
                    var SmallMetalSlopeNW = Registry.Tiles!.Get("Small Metal Slope NW");
                    var SmallMetalSlopeSW = Registry.Tiles!.Get("Small Metal Slope SW");
                    var SmallMetalSlopeSE = Registry.Tiles!.Get("Small Metal Slope SE");
                    var SmallMetalFloor = Registry.Tiles!.Get("Small Metal Floor");

                    for (var q = orderedTiles.Count - 1; q >= 0; q--)
                    {
                        var queued = orderedTiles[q];

                        switch (Level!.GeoMatrix[queued.y, queued.x, layer].Type)
                        {
                            case GeoType.SlopeNE:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalSlopeNE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeNW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalSlopeNW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeES:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalSlopeSE, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.SlopeSW:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalSlopeSW, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Platform:
                            DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], SmallMetalFloor, queued.x, queued.y, layer, camera, rt);
                            break;

                            case GeoType.Solid:
                            waiting[queued.y, queued.x] = true;
                            continue;
                        }

                        orderedTiles.RemoveAt(orderedTiles.Count - q);
                        deleted[queued.y, queued.x] = true;
                    }
                }


                // You could probably turn them into hash sets.
                Dictionary<(int x, int y), bool> delL = [];
                Dictionary<(int x, int y), bool> blocks = [];

                foreach (var (_, mx, my) in orderedTiles) blocks[(mx, my)] = true;

                foreach (var (_, mx, my) in orderedTiles)
                {
                    if (delL.ContainsKey((mx, my))) continue;

                    List<(int rnd, TileDefinition tile)> randomMetals = State.randomMetalsPool
                        .Select(t => (Random.Generate(1000), t))
                        .ToList();

                    randomMetals.Sort((r1, r2) => {
                        if (r1.rnd >  r2.rnd) return  1;
                        if (r1.rnd <  r2.rnd) return -1;
                        if (r1.rnd == r2.rnd) return  0;
                        return 0;
                    });

                    foreach (var (_, tile) in randomMetals)
                    {
                        // Testing if we can place the tile.

                        var legal = true;

                        for (var sx = 0; sx < tile.Size.Width; sx++)
                        {
                            for (var sy = 0; sy < tile.Size.Height; sy++)
                            {
                                var tx = mx + sx;
                                var ty = my + sy;

                                var spec = tile.Specs[sy, sx, 0];

                                if (!blocks.ContainsKey((tx, ty)))
                                {
                                    legal = false;
                                    goto stop_testing;
                                }

                                if (spec is -1) continue;

                                if (delL.ContainsKey((tx, ty)))
                                {
                                    legal = false;
                                    goto stop_testing;
                                }

                                if (
                                    !Data.Utils.InBounds(Level!.GeoMatrix, tx, ty) ||
                                    spec != (int)Level!.GeoMatrix[ty, tx, layer].Type
                                ) {
                                    legal = false;
                                    goto stop_testing;
                                }
                            }
                        }

                        stop_testing:

                        if (!legal) continue;

                        // Determining the tile head position, I suppose.
                        var rootPosX = mx + (int)(tile.Size.Width /2f + 0.4999f) - 1;
                        var rootPosY = my + (int)(tile.Size.Height/2f + 0.4999f) - 1;

                        // Drawing the tile if it's in camera bounds.
                        if (CheckCollisionPointRec(new Vector2(rootPosX, rootPosY), new Rectangle(camera.Coords/20f, new Vector2(100, 60))))
                        {
                            DrawTile_MTX(
                                Level!.TileMatrix[rootPosY, rootPosX, layer],
                                tile,
                                rootPosX,
                                rootPosY,
                                layer,
                                camera,
                                rt
                            );
                        }

                        for (var w = 0; w < tile.Size.Width; w++)
                        {
                            for (var h = 0; h < tile.Size.Height; h++)
                            {
                                var spec = tile.Specs[h, w, 0];

                                if (spec is not -1) delL[(mx + w, my + h)] = true;
                            }
                        }

                        break;
                    }

                }
            }
            break;
        
            case "Dune Sand":
            {
                for (var index = 1; index < orderedTiles.Count; index++)
                {
                    var (_, tx, ty) = orderedTiles[^index];
                
                    if (
                        !(!Data.Utils.InBounds(Level!.GeoMatrix, tx, ty) ||
                        Level!.GeoMatrix[ty, tx, layer].Type is not GeoType.Solid)
                    )
                    {
                        DrawTile_MTX(
                            Level!.TileMatrix[ty, tx, layer],
                            State.sandPool[Random.Generate(State.sandPool.Length - 1)],
                            tx,
                            ty,
                            layer,
                            camera,
                            rt
                        );
                    }
                    
                    orderedTiles.RemoveAt(orderedTiles.Count - index);
                }
            }
            break;

            case "Temple Stone":
            {
                bool[,] waiting = new bool[Level!.Height, Level!.Width];
                bool[,] deleted = new bool[Level!.Height, Level!.Width];

                var TempleStoneSlopeNE = Registry.Tiles!.Get("Temple Stone Slope NE");
                var TempleStoneSlopeNW = Registry.Tiles!.Get("Temple Stone Slope NW");
                var TempleStoneSlopeSW = Registry.Tiles!.Get("Temple Stone Slope SW");
                var TempleStoneSlopeSE = Registry.Tiles!.Get("Temple Stone Slope SE");
                var TempleStoneFloor = Registry.Tiles!.Get("Temple Stone Floor");

                var bigNoSlopes = Registry.Tiles!.Get("Big Temple Stone No Slopes");
                var wideTempleStone = Registry.Tiles!.Get("Wide Temple Stone");
                var smallTempleStone = Registry.Tiles!.Get("Small Temple Stone");

                for (var q = orderedTiles.Count - 1; q >= 0; q--)
                {
                    var queued = orderedTiles[q];

                    switch (Level!.GeoMatrix[queued.y, queued.x, layer].Type)
                    {
                        case GeoType.SlopeNE:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], TempleStoneSlopeNE, queued.x, queued.y, layer, camera, rt);
                        orderedTiles.Remove(queued);
                        deleted[queued.y, queued.x] = true;
                        break;

                        case GeoType.SlopeNW:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], TempleStoneSlopeNW, queued.x, queued.y, layer, camera, rt);
                        orderedTiles.Remove(queued);
                        deleted[queued.y, queued.x] = true;
                        break;

                        case GeoType.SlopeES:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], TempleStoneSlopeSE, queued.x, queued.y, layer, camera, rt);
                        orderedTiles.Remove(queued);
                        deleted[queued.y, queued.x] = true;
                        break;

                        case GeoType.SlopeSW:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], TempleStoneSlopeSW, queued.x, queued.y, layer, camera, rt);
                        orderedTiles.Remove(queued);
                        deleted[queued.y, queued.x] = true;
                        break;

                        case GeoType.Solid:
                        waiting[queued.y, queued.x] = true;
                        continue;

                        case GeoType.Glass:
                        if (Configuration.MaterialFixes) {
                            orderedTiles.Remove(queued);
                            deleted[queued.y, queued.x] = true;
                        }
                        break;
                    }
                }

                List<(int x, int y)> orderedTilesWithoutRND = orderedTiles.Select(o => (o.x, o.y)).ToList();
                List<(int x, int y)> duplicated = [..orderedTilesWithoutRND];
                List<(int x, int y)>[] corners = [ [], [], [], [] ];
            
                foreach (var t in duplicated)
                {
                    var (tx, ty) = t;

                    if ((tx % 6 == 0 && ty % 4 == 0) || (tx % 6 == 3 && ty % 4 == 2))
                    {
                        AttemptDrawTempleStone_MTX(
                            tx,
                            ty,
                            layer,
                            camera,
                            orderedTilesWithoutRND,
                            corners,
                            bigNoSlopes,
                            rt
                        );
                    }
                }

                for (var c = 1; c <= corners[0].Count; c++)
                {
                    var corner = corners[0][^c];

                    if (corners[2].Contains(corners[0][^c]))
                    {
                        orderedTilesWithoutRND.Remove(corners[0][^c]);
                    }
                }

                for (var c = 1; c <= corners[1].Count; c++)
                {
                    var corner = corners[1][^c];

                    if (corners[3].Contains(corners[1][^c]))
                    {
                        orderedTilesWithoutRND.Remove(corners[1][^c]);
                    }
                }

                while (orderedTilesWithoutRND.Count > 0)
                {
                    // Get a random item from the list
                    var t = orderedTilesWithoutRND[Random.Generate(orderedTilesWithoutRND.Count - 1)];

                    var drawn = false;

                    if (corners[0].Contains(t))
                    {
                        DrawTile_MTX(
                            Level!.TileMatrix[t.y, t.x, layer],
                            TempleStoneSlopeSE,
                            t.x,
                            t.y,
                            layer,
                            camera,
                            rt
                        );

                        drawn = true;
                    }
                    else if (corners[1].Contains(t))
                    {
                        DrawTile_MTX(
                            Level!.TileMatrix[t.y, t.x, layer],
                            TempleStoneSlopeSW,
                            t.x,
                            t.y,
                            layer,
                            camera,
                            rt
                        );

                        drawn = true;
                    }
                    else if (corners[2].Contains(t))
                    {
                        DrawTile_MTX(
                            Level!.TileMatrix[t.y, t.x, layer],
                            TempleStoneSlopeNW,
                            t.x,
                            t.y,
                            layer,
                            camera,
                            rt
                        );

                        drawn = true;
                    }
                    else if (corners[3].Contains(t))
                    {
                        DrawTile_MTX(
                            Level!.TileMatrix[t.y, t.x, layer],
                            TempleStoneSlopeNE,
                            t.x,
                            t.y,
                            layer,
                            camera,
                            rt
                        );

                        drawn = true;
                    }

                    // Merged conditions (there were two `if(!drawn)`)

                    if (!drawn)
                    {
                        ReadOnlySpan<(int x, int y)> occupy = [
                            (-1, 0), (-1, 1), (0, 0),
                            ( 0, 1), ( 1, 0), (1, 1)
                        ];

                        drawn = true;

                        foreach (var o in occupy)
                        {
                            var ox = t.x + o.x;
                            var oy = t.y + o.y;

                            if (!CheckIfAMaterialIsSolidAndSameMaterial(ox, oy, layer, State.templeStone))
                            {
                                drawn = false;
                                break;
                            }

                            for (var a = 0; a < 4; a++)
                            {
                                if (corners[a].Contains((ox, oy)))
                                {
                                    drawn = false;
                                    break;
                                }
                            }

                            if (!orderedTilesWithoutRND.Contains((ox, oy)))
                            {
                                drawn = false;
                                break;
                            }
                        }

                        if (drawn)
                        {
                            DrawTile_MTX(
                                Level!.TileMatrix[t.y, t.x, layer],
                                State.mediumTempleStone!,
                                t.x,
                                t.y,
                                layer,
                                camera,
                                rt
                            );

                            foreach (var o in occupy)
                            {
                                orderedTilesWithoutRND.Remove((t.x + o.x, t.y + o.y));
                            }
                        }
                    

                        if (
                            CheckIfAMaterialIsSolidAndSameMaterial(t.x -1, t.y, layer, State.templeStone) &&
                            orderedTilesWithoutRND.Contains((t.x -1, t.y)) &&
                            !corners[0].Contains((t.x -1, t.y)) &&
                            !corners[1].Contains((t.x -1, t.y)) &&
                            !corners[2].Contains((t.x -1, t.y)) &&
                            !corners[3].Contains((t.x -1, t.y))
                        )
                        {
                            DrawTile_MTX(
                                Level!.TileMatrix[t.y, t.x, layer],
                                wideTempleStone,
                                t.x - 1,
                                t.y,
                                layer,
                                camera,
                                rt
                            );
                        
                            orderedTilesWithoutRND.Remove((t.x -1, t.y));
                        }
                        else if (
                            CheckIfAMaterialIsSolidAndSameMaterial(t.x + 1, t.y, layer, State.templeStone) &&
                            orderedTilesWithoutRND.Contains((t.x + 1, t.y)) &&
                            !corners[0].Contains((t.x + 1, t.y)) &&
                            !corners[1].Contains((t.x + 1, t.y)) &&
                            !corners[2].Contains((t.x + 1, t.y)) &&
                            !corners[3].Contains((t.x + 1, t.y))
                        )
                        {
                            DrawTile_MTX(
                                Level!.TileMatrix[t.y, t.x, layer],
                                wideTempleStone,
                                t.x + 1,
                                t.y,
                                layer,
                                camera,
                                rt
                            );
                        
                            orderedTilesWithoutRND.Remove((t.x +1, t.y));
                        }
                        else
                        {
                            DrawTile_MTX(
                                Level!.TileMatrix[t.y, t.x, layer],
                                smallTempleStone,
                                t.x,
                                t.y,
                                layer,
                                camera,
                                rt
                            );
                        }

                        orderedTilesWithoutRND.Remove(t);
                    }
                }
            }
            break;
        
            case "4Mosaic":
            {
                var mosaicStoneSlopeNE = Registry.Tiles!.Get("4Mosaic Slope NE");
                var mosaicStoneSlopeNW = Registry.Tiles!.Get("4Mosaic Stone Slope NW");
                var mosaicStoneSlopeSW = Registry.Tiles!.Get("4Mosaic Stone Slope SW");
                var mosaicStoneSlopeSE = Registry.Tiles!.Get("4Mosaic Stone Slope SE");
                var mosaicStoneFloor = Registry.Tiles!.Get("4Mosaic Stone Floor");

                for (var q = 1; q <= orderedTiles.Count; q++)
                {
                    var queued = orderedTiles[^q];

                    switch (Level!.GeoMatrix[queued.y, queued.x, layer].Type)
                    {
                        case GeoType.SlopeNE:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], mosaicStoneSlopeNE, queued.x, queued.y, layer, camera, rt);
                        break;

                        case GeoType.SlopeNW:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], mosaicStoneSlopeNW, queued.x, queued.y, layer, camera, rt);
                        break;

                        case GeoType.SlopeES:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], mosaicStoneSlopeSE, queued.x, queued.y, layer, camera, rt);
                        break;

                        case GeoType.SlopeSW:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], mosaicStoneSlopeSW, queued.x, queued.y, layer, camera, rt);
                        break;

                        case GeoType.Platform:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], mosaicStoneFloor, queued.x, queued.y, layer, camera, rt);
                        break;

                        case GeoType.Solid:
                        continue;
                    }
                }
            }
            break;
        
            case "3DBricks":
            {
                var brick = Registry.Tiles!.Get("3DBrick Square");
                var brickStoneSlopeNE = Registry.Tiles!.Get("3DBrick Slope NE");
                var brickStoneSlopeNW = Registry.Tiles!.Get("3DBrick Slope NW");
                var brickStoneSlopeSW = Registry.Tiles!.Get("3DBrick Slope SW");
                var brickStoneSlopeSE = Registry.Tiles!.Get("3DBrick Slope SE");
                var brickStoneFloor = Registry.Tiles!.Get("3DBrick Floor");

                for (var q = 1; q <= orderedTiles.Count; q++)
                {
                    var queued = orderedTiles[^q];

                    switch (Level!.GeoMatrix[queued.y, queued.x, layer].Type)
                    {
                        case GeoType.SlopeNE:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], brickStoneSlopeNE, queued.x, queued.y, layer, camera, rt);
                        break;

                        case GeoType.SlopeNW:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], brickStoneSlopeNW, queued.x, queued.y, layer, camera, rt);
                        break;

                        case GeoType.SlopeES:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], brickStoneSlopeSE, queued.x, queued.y, layer, camera, rt);
                        break;

                        case GeoType.SlopeSW:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], brickStoneSlopeSW, queued.x, queued.y, layer, camera, rt);
                        break;

                        case GeoType.Platform:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], brickStoneFloor, queued.x, queued.y, layer, camera, rt);
                        break;

                        case GeoType.Solid:
                        DrawTile_MTX(Level!.TileMatrix[queued.y, queued.x, layer], brick, queued.x, queued.y, layer, camera, rt);
                        break;
                    }
                }
            }
            break;
        }
    }

    /// <summary>
    /// Draws materials of `pipeType` render type.
    /// </summary>
    /// <param name="mat">The material definition</param>
    /// <param name="x">Matrix X coordinates</param>
    /// <param name="y">Matrix X coordinates</param>
    /// <param name="layer">The current layer</param>
    protected virtual void DrawPipeMaterial_MTX(
        in MaterialDefinition mat, 
        int x,
        int y,
        int layer,
        in RenderCamera camera
    )
    {
        (int x, int y) gtPos = (0, 0);
    
        ref var cell = ref Level!.GeoMatrix[y, x, layer];

        switch (cell.Type)
        {
            case GeoType.Solid:
            {
                StringBuilder nbrs = new();

                ReadOnlySpan<(int x, int y)> list = [
                    (-1, 0), (0, -1), (1, 0), (0, 1)
                ];

                // Loop can be expanded
                foreach (var (ix, iy) in list)
                {
                    var nx = x + ix;
                    var ny = y + iy;

                    if (Random.Generate(2) == 1 && Data.Utils.InBounds(Level!.GeoMatrix, nx, ny) ? Level!.GeoMatrix[ny, nx, layer][GeoType.Solid] : true)
                    {
                        nbrs.Append('1');
                    }
                    else
                    {
                        var res = Utils.BoolInt(IsMyTileOpenToThisTile(mat, nx, ny, layer)).ToString();
                        nbrs.Append(res);
                    }
                }

                gtPos = nbrs.ToString() switch 
                {
                    "0101" => ( 2, 2),
                    "1010" => ( 4, 2),
                    "1111" => ( 6, 2),
                    "0111" => ( 8, 2),
                    "1101" => (10, 2),
                    "1110" => (12, 2),
                    "1011" => (14, 2),
                    "0011" => (16, 2),
                    "1001" => (18, 2),
                    "1100" => (20, 2),
                    "0110" => (22, 2),
                    "1000" => (24, 2),
                    "0010" => (26, 2),
                    "0100" => (28, 2),
                    "0001" => (30, 2),

                    "0000" => Configuration.MaterialFixes ? (40, 2) : (0, 0),
                    
                    _ => (0, 0)
                };

                if (mat.Name == "Small Pipes")
                {
                    SDraw.Draw_NoWhite_NoColor(
                        _layers[layer*10 + 5],
                        State.framework,
                        Shaders.WhiteRemover,
                        new Rectangle(0, 0, 20, 20),
                        new Rectangle(
                            x*20,
                            y*20,
                            20, 
                            20
                        )
                    );
                }
            }
            break;

            case GeoType.SlopeNW: gtPos = (32, 2); break;
            case GeoType.SlopeNE: gtPos = (34, 2); break;
            case GeoType.SlopeES: gtPos = (36, 2); break;
            case GeoType.SlopeSW: gtPos = (38, 2); break;
            case GeoType.Platform: 
            if (Configuration.MaterialFixes) gtPos = (42, 2); 
            break;
            case GeoType.Glass:
            if (Configuration.MaterialFixes) gtPos = (44, 2);
            break;
        }

        var texture = mat.Name switch {
            "Small Pipes" => State.pipeTiles,
            "Trash" => State.trashTiles,
            "largeTrash" or "megaTrash" => State.largeTrashTiles,
            "dirt" => State.dirtTiles,
            "Sandy Dirt" => State.sandyDirtTiles,
            _ => State.sandyDirtTiles
        };

        // Expanded loop

        { // First iteration (startLayer = layer * 10 + 2)
            var startLayer = layer * 10 + 2;

            ReadOnlySpan<int> numbers = [2, 4, 6, 8];
            gtPos.y = numbers[Random.Generate(4) - 1];
            
            Rectangle rect = new(
                (gtPos.x - 1) * 20 + 1 - 10,
                (gtPos.y - 1) * 20 + 1 - 10,
                40,
                40
            );

            // Expanded loop

            { // First iteration (d = startLayer)
                SDraw.Draw_NoWhite_NoColor(
                    _layers[startLayer],
                    texture,
                    Shaders.WhiteRemover,
                    rect,
                    new Rectangle(
                        x*20 - 10, 
                        y*20 - 10,
                        40,
                        40
                    )
                );
            }

            { // Second iteration (d = startLayer + 1)
                SDraw.Draw_NoWhite_NoColor(
                    _layers[startLayer + 1],
                    texture,
                    Shaders.WhiteRemover,
                    rect,
                    new Rectangle(
                        x*20 - 10, 
                        y*20 - 10,
                        40,
                        40
                    )
                );
            }
        }

        { // Second iteration (startLayer = layer * 10 + 7)
            var startLayer = layer * 10 + 7;

            ReadOnlySpan<int> numbers = [2, 4, 6, 8];
            gtPos.y = numbers[Random.Generate(4) - 1];
            
            Rectangle rect = new(
                (gtPos.x - 1) * 20 + 1 - 10,
                (gtPos.y - 1) * 20 + 1 - 10,
                40,
                40
            );

            // Expanded loop

            { // First iteration (d = startLayer)
                SDraw.Draw_NoWhite_NoColor(
                    _layers[startLayer],
                    texture,
                    Shaders.WhiteRemover,
                    rect,
                    new Rectangle(
                        x*20 - 10, 
                        y*20 - 10,
                        40,
                        40
                    )
                );
            }

            { // Second iteration (d = startLayer + 1)
                SDraw.Draw_NoWhite_NoColor(
                    _layers[startLayer + 1],
                    texture,
                    Shaders.WhiteRemover,
                    rect,
                    new Rectangle(
                        x*20 - 10, 
                        y*20 - 10,
                        40,
                        40
                    )
                );
            }
        }
    
        if (mat.Name == "Trash" && 
            (!cell[GeoType.Glass] || !Configuration.MaterialFixes)
        )
        {
            for (var q = 1; q <= 3; q++)
            {
                var d = layer * 10 + Random.Generate(9);
                var gt = Random.Generate(48) - 1;
                var middle = new Vector2(x, y)*20 + Vector2.One + new Vector2(Random.Generate(21), Random.Generate(21));
                var src = new Rectangle(gt * 50, 0, 50, 50);
                var dest = new Rectangle(middle, Vector2.One*50);
                ReadOnlySpan<Color> colors = [Color.Red, Color.Green, Color.Blue];

                SDraw.Draw_NoWhite_Color(
                    _layers[d],
                    State.assortedTrash,
                    Shaders.WhiteRemoverApplyColor,
                    src,
                    dest,
                    colors[Random.Generate(3) - 1]
                );
            }
        }
    }

    protected virtual void DrawRockMaterial_MTX(
        in MaterialDefinition mat, 
        int x,
        int y,
        int layer,
        in RenderCamera camera,
        bool tr
    ) {
        (int x, int y) gtPos = (0, 0);
    
        ref var cell = ref Level!.GeoMatrix[y, x, layer];

        switch (cell.Type)
        {
            case GeoType.Solid:
            {
                StringBuilder nbrs = new();

                ReadOnlySpan<(int x, int y)> list = [
                    (-1, 0), (0, -1), (1, 0), (0, 1)
                ];

                // Loop can be expanded
                foreach (var (ix, iy) in list)
                {
                    var nx = x + ix;
                    var ny = y + iy;

                    if (Random.Generate(2) == 1 && Data.Utils.InBounds(Level!.GeoMatrix, nx, ny) ? Level!.GeoMatrix[ny, nx, layer][GeoType.Solid] : true)
                    {
                        nbrs.Append('1');
                    }
                    else
                    {
                        var res = Utils.BoolInt(IsMyTileOpenToThisTile(mat, nx, ny, layer)).ToString();
                        nbrs.Append(res);
                    }
                }

                gtPos = nbrs.ToString() switch 
                {
                    "0101" => ( 2, 2),
                    "1010" => ( 4, 2),
                    "1111" => ( 6, 2),
                    "0111" => ( 8, 2),
                    "1101" => (10, 2),
                    "1110" => (12, 2),
                    "1011" => (14, 2),
                    "0011" => (16, 2),
                    "1001" => (18, 2),
                    "1100" => (20, 2),
                    "0110" => (22, 2),
                    "1000" => (24, 2),
                    "0010" => (26, 2),
                    "0100" => (28, 2),
                    "0001" => (30, 2),

                    "0000" => Configuration.MaterialFixes ? (40, 2) : (0, 0),
                    
                    _ => (0, 0)
                };
            }
            break;

            case GeoType.SlopeNW: gtPos = (32, 2); break;
            case GeoType.SlopeNE: gtPos = (34, 2); break;
            case GeoType.SlopeES: gtPos = (36, 2); break;
            case GeoType.SlopeSW: gtPos = (38, 2); break;
            case GeoType.Platform: 
            if (Configuration.MaterialFixes) gtPos = (42, 2); 
            break;
            case GeoType.Glass:
            if (Configuration.MaterialFixes) gtPos = (44, 2);
            break;
        }
    
        var sublayer = layer * 10;

        gtPos.y = 2 * Random.Generate(16);
        Rectangle rect = new(
            (gtPos.x - 1) * 20,
            (gtPos.y - 1) * 20,
            20,
            20
        );

        if (tr)
        {
            var r = rect;
            r.X += 1;
            r.Y += 1;

            r.X -= 10;
            r.Y -= 10;

            r.Width += 20;
            r.Height += 20;

            for (var rg = 1; rg <= 4; rg++)
            {
                SDraw.Draw_NoWhite_NoColor(
                    _layers[sublayer + rg],
                    State.rockTiles,
                    Shaders.WhiteRemover,
                    r,
                    new Rectangle(
                        (x-1) * 20 - 10,
                        (y-1) * 20 - 10,
                        40,
                        40
                    )
                );
            }
        }
        else
        {
            var r = rect;
            r.X += 1;
            r.Y += 1;

            r.X -= 10;
            r.Y -= 10;

            r.Width += 20;
            r.Height += 20;

            for (var rg = 1; rg <= 4; rg++)
            {
                SDraw.Draw_NoWhite_NoColor(
                    _layers[sublayer + rg],
                    State.rockTiles,
                    Shaders.WhiteRemover,
                    r,
                    new Rectangle(
                        (x-1) * 20 - 10,
                        (y-1) * 20 - 10,
                        40,
                        40
                    )
                );
            }
        }
    
        gtPos.y = 2 * Random.Generate(16);
        rect = new Rectangle(
            (gtPos.x - 1) * 20,
            (gtPos.y - 1) * 20,
            20,
            20
        );

        for (var rd = 5; rd <= 9; rd++)
        {
            SDraw.Draw_NoWhite_NoColor(
                _layers[sublayer + rd],
                State.rockTiles,
                Shaders.WhiteRemover,
                rect,
                new Rectangle(
                    (x-1) * 20 - 10,
                    (y-1) * 20 - 10,
                    40,
                    40
                )
            );
        }
    
    }

    protected virtual void DrawLargeTrashMaterial_MTX(
        in MaterialDefinition mat, 
        int x,
        int y,
        int layer,
        in RenderCamera camera,
        in RenderTexture2D rt
    ) {
        var distanceToAir = -1;

        for (var dist = 1; dist <= 5; dist++)
        {
            // Expanded loop

            { // First iteration (dir = (-1, 0))

                if (Utils.GetGeoCellType(Level!.GeoMatrix, x - 1, y, layer) != GeoType.Solid)
                {
                    distanceToAir = dist;
                    continue;
                }
            }

            { // First iteration (dir = (0, -1))

                if (Utils.GetGeoCellType(Level!.GeoMatrix, x, y - 1, layer) != GeoType.Solid)
                {
                    distanceToAir = dist;
                    continue;
                }
            }

            { // First iteration (dir = (1, 0))

                if (Utils.GetGeoCellType(Level!.GeoMatrix, x + 1, y, layer) != GeoType.Solid)
                {
                    distanceToAir = dist;
                    continue;
                }
            }

            { // First iteration (dir = (0, 1))

                if (Utils.GetGeoCellType(Level!.GeoMatrix, x, y + 1, layer) != GeoType.Solid)
                {
                    distanceToAir = dist;
                    continue;
                }
            }

            if (dist != -1) break;
        }
    
        if (distanceToAir != -1) distanceToAir = 5;

        if (distanceToAir < 5) DrawPipeMaterial_MTX(Registry.Materials!.Get("Trash"), x, y, layer, camera);

        if (distanceToAir < 3)
        {
            for (var q = 1; q <= distanceToAir + Random.Generate(2) - 1; q++)
            {
                var sublayer = Utils.Restrict(layer * 10 + Random.Generate(Random.Generate(10)) - 1 + Random.Generate(3), 0, 29);
                var pos = new Vector2(x, y) * 20 - camera.Coords + new Vector2(Random.Generate(21) - 11, Random.Generate(21) - 11);

                if (State.largeTrashPool.Length > 0)
                {
                    var trashTile = State.largeTrashPool[Random.Generate(State.largeTrashPool.Length) - 1];
                    
                    var (width, height) = trashTile.Size;

                    width *= 10;
                    height *= 10;

                    Quad quad = new(
                        new Vector2(pos.X - width, pos.Y - height), // Top left
                        new Vector2(pos.X + width, pos.Y - height), // top right
                        new Vector2(pos.X + width, pos.Y + height), // bottom right
                        new Vector2(pos.X - width, pos.Y + height)  // bottom left
                    );

                    State.largeTrashRenderQueue.Add(new Prop_Legacy(-sublayer, trashTile.Name, quad) {
                        Tile = trashTile,
                        Extras = new PropExtras_Legacy(new BasicPropSettings(seed: Random.Generate(1000)), [])
                    });
                }
            }
        }
    
        if (distanceToAir > 2)
        {
            var sublayer = layer * 10;
            var pos = new Vector2(x, y) * 20 - camera.Coords;

            if (Random.Generate(5) <= distanceToAir)
            {
                BeginTextureMode(_layers[sublayer]);
                DrawRectangleRec(new Rectangle(pos.X, pos.Y, 20, 20), Color.Green);
                EndTextureMode();

                var variable = Random.Generate(14);
                var quad = new Quad(
                    new Vector2(-30, -30),
                    new Vector2( 30, -30),
                    new Vector2( 30,  30),
                    new Vector2(-30,  30)
                );

                SDraw.Draw_NoWhite_NoColor(
                    _layers[sublayer],
                    State.bigJunk,
                    Shaders.WhiteRemover,
                    new Rectangle((variable - 1) * 60, 1, 60, 60),
                    quad.Rotated(Random.Generate(360))
                );
            }

            for (var q = 1; q <= distanceToAir; q++)
            {
                sublayer = layer * 10 + Random.Generate(10) - 1;
                pos = new Vector2(x, y) * 20 - camera.Coords + new Vector2(Random.Generate(21) - 11, Random.Generate(21) - 11);
                var variable = Random.Generate(14) - 1;
                var quad = new Quad(
                    new Vector2(pos.X - 30, pos.Y - 30),
                    new Vector2(pos.X + 30, pos.Y - 30),
                    new Vector2(pos.X + 30, pos.Y + 30),
                    new Vector2(pos.X - 30, pos.Y + 30)
                );

                SDraw.Draw_NoWhite_NoColor(
                    _layers[sublayer],
                    State.bigJunk,
                    Shaders.WhiteRemover,
                    new Rectangle(variable*60, 1, 60, 60),
                    quad.Rotated(Random.Generate(360))
                );
            }
        }
    }

    protected virtual void DrawRoughRockMaterial_MTX(
        in MaterialDefinition mat,
        int x,
        int y,
        int layer,
        in RenderCamera camera,
        in RenderTexture2D rt
    ) {
        Texture2D imgR = new();
        var szR = 0;
        var intOp = 1;

        switch (mat.Name) {
            case "Rough Rock":
                imgR = State.roughRock;
                szR = 60;
                intOp = 6;
            break;

            case "Sandy Dirt":
                imgR = State.sandRR;
                szR = 20;
                intOp = 2;
            break;
        }

        var distanceToAir = -1;

        for (var dist = 1; dist <= 5; dist++)
        {
            // Expanded loop

            { // First iteration (dir = (-1, 0))

                if (!Level!.GeoMatrix[y, x - 1, layer][GeoType.Solid])
                {
                    distanceToAir = dist;
                    continue;
                }
            }

            { // First iteration (dir = (0, -1))

                if (!Level!.GeoMatrix[y - 1, x, layer][GeoType.Solid])
                {
                    distanceToAir = dist;
                    continue;
                }
            }

            { // First iteration (dir = (1, 0))

                if (!Level!.GeoMatrix[y, x + 1, layer][GeoType.Solid])
                {
                    distanceToAir = dist;
                    continue;
                }
            }

            { // First iteration (dir = (0, 1))

                if (!Level!.GeoMatrix[y + 1, x, layer][GeoType.Solid])
                {
                    distanceToAir = dist;
                    continue;
                }
            }

            if (dist != -1) break;
        }
    
        if (Configuration.RoughRockSpreadsMore) {
            distanceToAir++;
        }

        if (distanceToAir < 5) {
            switch (mat.Name) {
                case "Rough Rock":
                    DrawRockMaterial_MTX(mat, x, y, layer, camera, true);
                break;

                case "Sandy Dirt":
                    DrawPipeMaterial_MTX(mat, x, y, layer, camera);
                    distanceToAir++;
                break;
            }
        }

        int variable;
        float fat, mSzR;
        Quad quad;

        if (distanceToAir > 2) {
            var sublayer = layer * 10;
            var pos = new Vector2(x - camera.Coords.X, y - camera.Coords.Y) * 20 + Vector2.One * 10;

            if (Random.Generate(5) <= distanceToAir) {
                BeginTextureMode(_layers[sublayer]);
                DrawRectangleRec(
                    new Rectangle(
                        pos.X - 10,
                        pos.Y - 10,
                        20,
                        20
                    ),
                    Color.Red
                );
                EndTextureMode();

                variable = Random.Generate(intOp);
                fat = Random.Generate(3) switch {
                    1 => 1f,
                    2 => 1.05f,
                    _ => 1.1f
                };

                mSzR = szR * fat;
                
                quad = new Quad(
                    new Vector2(pos.X - mSzR, pos.Y - mSzR),
                    new Vector2(pos.X + mSzR, pos.Y - mSzR),
                    new Vector2(pos.X + mSzR, pos.Y + mSzR),
                    new Vector2(pos.X - mSzR, pos.Y + mSzR)
                );

                SDraw.Draw_NoWhite_NoColor(
                    rt,
                    imgR,
                    Shaders.WhiteRemover,
                    new Rectangle((variable - 1) * szR * 2, 1, szR * 2, szR*2),
                    quad.Rotated(Random.Generate(360))
                );
            }

            pos = new Vector2(x - camera.Coords.X, y - camera.Coords.Y) * 20 + Vector2.One * 10 + new Vector2(Random.Generate(21) - 11, Random.Generate(21) - 11);
            variable = Random.Generate(intOp);
            fat = Random.Generate(3) switch {
                1 => 1f,
                2 => 1.05f,
                _ => 1.1f
            };

            mSzR = szR * fat;
        
            quad = new Quad(
                new Vector2(pos.X - mSzR, pos.Y - mSzR),
                new Vector2(pos.X + mSzR, pos.Y - mSzR),
                new Vector2(pos.X + mSzR, pos.Y + mSzR),
                new Vector2(pos.X - mSzR, pos.Y + mSzR)
            );

            SDraw.Draw_NoWhite_NoColor(
                rt,
                imgR,
                Shaders.WhiteRemover,
                new Rectangle((variable - 1) * szR * 2, 1, szR * 2, szR*2),
                quad.Rotated(Random.Generate(360))
            );

            for (var q = 1; q <= distanceToAir; q++)
            {
                variable = Random.Generate(intOp);
                pos = new Vector2(x - camera.Coords.X, y - camera.Coords.Y) * 20 + Vector2.One * 10 + new Vector2(Random.Generate(21) - 11, Random.Generate(21) - 11);

                fat = Random.Generate(3) switch {
                    1 => 1f,
                    2 => 1.05f,
                    _ => 1.1f
                };

                mSzR = szR * fat;

                quad = new Quad(
                    new Vector2(pos.X - mSzR, pos.Y - mSzR),
                    new Vector2(pos.X + mSzR, pos.Y - mSzR),
                    new Vector2(pos.X + mSzR, pos.Y + mSzR),
                    new Vector2(pos.X - mSzR, pos.Y + mSzR)
                );

                SDraw.Draw_NoWhite_NoColor(
                    rt,
                    imgR,
                    Shaders.WhiteRemover,
                    new Rectangle((variable - 1) * szR * 2, 1, szR * 2, szR*2),
                    quad.Rotated(Random.Generate(360))
                );
            }

            if (mat.Name == "Sandy Dirt" && Random.Generate(2) == 1) {
                pos = new Vector2(x - camera.Coords.X, y - camera.Coords.Y) * 20;

                if (Random.Generate(5) <= distanceToAir) {
                    BeginTextureMode(_layers[sublayer]);
                    DrawRectangleRec(
                        new Rectangle(
                            pos.X - 10,
                            pos.Y - 10,
                            20,
                            20
                        ),
                        Color.Red
                    );
                    EndTextureMode();

                    variable = Random.Generate(intOp);
                    fat = Random.Generate(3) switch {
                        1 => 1f,
                        2 => 1.05f,
                        _ => 1.1f
                    };

                    mSzR = szR * fat;

                    quad = new Quad(
                        new Vector2(pos.X - mSzR, pos.Y - mSzR),
                        new Vector2(pos.X + mSzR, pos.Y - mSzR),
                        new Vector2(pos.X + mSzR, pos.Y + mSzR),
                        new Vector2(pos.X - mSzR, pos.Y + mSzR)
                    );

                    SDraw.Draw_NoWhite_NoColor(
                        rt,
                        imgR,
                        Shaders.WhiteRemover,
                        new Rectangle((variable - 1) * szR * 2, 1, szR * 2, szR*2),
                        quad.Rotated(Random.Generate(360))
                    );
                }

                for (var q = 1; q <= distanceToAir; q++) {
                    sublayer = layer * 10 + Random.Generate(10) - 1;
                    pos = new Vector2(x - camera.Coords.X, y - camera.Coords.Y) * 20 + Vector2.One * 10 + new Vector2(Random.Generate(21) - 11, Random.Generate(21) - 11);
                    variable = Random.Generate(intOp);
                    fat = Random.Generate(3) switch {
                        1 => 1f,
                        2 => 1.05f,
                        _ => 1.1f
                    };

                    mSzR = szR * fat;

                    quad = new Quad(
                        new Vector2(pos.X - mSzR, pos.Y - mSzR),
                        new Vector2(pos.X + mSzR, pos.Y - mSzR),
                        new Vector2(pos.X + mSzR, pos.Y + mSzR),
                        new Vector2(pos.X - mSzR, pos.Y + mSzR)
                    );

                    SDraw.Draw_NoWhite_NoColor(
                        _layers[sublayer],
                        imgR,
                        Shaders.WhiteRemover,
                        new Rectangle((variable - 1) * szR * 2, 1, szR * 2, szR*2),
                        quad.Rotated(Random.Generate(360))
                    );
                }
            }
        }
    }

    protected virtual void DrawMegaTrashMaterial_MTX(
        in MaterialDefinition mat,
        int x,
        int y,
        int layer,
        in RenderCamera camera,
        in RenderTexture2D rt
    ) {
        var distanceToAir = -1;

        for (var dist = 1; dist <= 5; dist++)
        {
            // Expanded loop

            { // First iteration (dir = (-1, 0))

                if (!Level!.GeoMatrix[y, x - 1, layer][GeoType.Solid])
                {
                    distanceToAir = dist;
                    continue;
                }
            }

            { // First iteration (dir = (0, -1))

                if (!Level!.GeoMatrix[y - 1, x, layer][GeoType.Solid])
                {
                    distanceToAir = dist;
                    continue;
                }
            }

            { // First iteration (dir = (1, 0))

                if (!Level!.GeoMatrix[y, x + 1, layer][GeoType.Solid])
                {
                    distanceToAir = dist;
                    continue;
                }
            }

            { // First iteration (dir = (0, 1))

                if (!Level!.GeoMatrix[y + 1, x, layer][GeoType.Solid])
                {
                    distanceToAir = dist;
                    continue;
                }
            }

            if (dist != -1) break;
        }
    
        if (distanceToAir == -1) distanceToAir = 5;

        if (distanceToAir < 5) {
            DrawPipeMaterial_MTX(Registry.Materials!.Get("Trash"), x, y, layer, camera);
        }

        int sublayer;
        Vector2 pos;
        Quad quad;

        if (distanceToAir < 3 && State.megaTrashPool.Length > 0) {
            for (var q = 1; q <= distanceToAir + Random.Generate(2) - 1; q++) {
                sublayer = Utils.Restrict(layer * 10 + Random.Generate(Random.Generate(10)) - 1 + Random.Generate(3), 0, 29);
                pos = new Vector2(x, y) * 20 - camera.Coords + Vector2.One * 10 + new Vector2(Random.Generate(21) - 11, Random.Generate(21) - 11);

                var tile = State.megaTrashPool[Random.Generate(State.megaTrashPool.Length) - 1];
                var (width, height) = tile.Size;

                width  *= 10;
                height *= 10;

                quad = new Quad(
                    new Vector2(pos.X - width, pos.Y - height),
                    new Vector2(pos.X + width, pos.Y - height),
                    new Vector2(pos.X + width, pos.Y + height),
                    new Vector2(pos.X - width, pos.Y + height)
                );

                State.largeTrashRenderQueue.Add(new Prop_Legacy(-sublayer, tile.Name, quad) {
                    Tile = tile,
                    Extras = new PropExtras_Legacy(new BasicPropSettings(seed: Random.Generate(1000)), [])
                });
            }
        }
    
        // Copied from large trash code
        if (distanceToAir > 2)
        {
            sublayer = layer * 10;
            pos = new Vector2(x, y) * 20 - camera.Coords;

            if (Random.Generate(5) <= distanceToAir)
            {
                BeginTextureMode(_layers[sublayer]);
                DrawRectangleRec(new Rectangle(pos.X, pos.Y, 20, 20), Color.Green);
                EndTextureMode();

                var variable = Random.Generate(14);
                quad = new Quad(
                    new Vector2(-30, -30),
                    new Vector2( 30, -30),
                    new Vector2( 30,  30),
                    new Vector2(-30,  30)
                );

                SDraw.Draw_NoWhite_NoColor(
                    _layers[sublayer],
                    State.bigJunk,
                    Shaders.WhiteRemover,
                    new Rectangle((variable - 1) * 60, 1, 60, 60),
                    quad.Rotated(Random.Generate(360))
                );
            }

            for (var q = 1; q <= distanceToAir; q++)
            {
                sublayer = layer * 10 + Random.Generate(10) - 1;
                pos = new Vector2(x, y) * 20 - camera.Coords + new Vector2(Random.Generate(21) - 11, Random.Generate(21) - 11);
                var variable = Random.Generate(14) - 1;
                quad = new Quad(
                    new Vector2(pos.X - 30, pos.Y - 30),
                    new Vector2(pos.X + 30, pos.Y - 30),
                    new Vector2(pos.X + 30, pos.Y + 30),
                    new Vector2(pos.X - 30, pos.Y + 30)
                );

                SDraw.Draw_NoWhite_NoColor(
                    _layers[sublayer],
                    State.bigJunk,
                    Shaders.WhiteRemover,
                    new Rectangle(variable*60, 1, 60, 60),
                    quad.Rotated(Random.Generate(360))
                );
            }
        }
    }

    protected virtual void DrawDirtClot(
        in RenderCamera camera,
        Vector2 pos, 
        int layer, 
        int sublayer, 
        int variant, 
        int distanceToAir
    ) {
        var szAdd = Random.Generate(distanceToAir + 1) - 1;

        for (var d = 0; d <= 2; d++) {
            var size = 5 + szAdd + d*2;
            var pstDp = Utils.Restrict(sublayer - 1 + d, 0, 29);
            var quad = new Quad(
                new Vector2(pos.X - size, pos.Y - size),
                new Vector2(pos.X + size, pos.Y - size),
                new Vector2(pos.X + size, pos.Y + size),
                new Vector2(pos.X - size, pos.Y + size)
            );
            var texture = variant switch {
                1 => State.rubbleGraf1,
                2 => State.rubbleGraf2,
                3 => State.rubbleGraf3,
                _ => State.rubbleGraf4
            };

            SDraw.Draw_NoWhite_Color(
                _layers[pstDp],
                texture,
                Shaders.WhiteRemoverApplyColor,
                quad.Rotated(Random.Generate(360)),
                Color.Green
            );
        }

        var (gx1, gy1) = Utils.GetGridPos(pos - new Vector2(10, 10) + camera.Coords);
        var (gx2, gy2) = Utils.GetGridPos(pos + new Vector2(10, 10) + camera.Coords);

        if (
            (Random.Generate(6) > distanceToAir && Random.Generate(3) == 1) || 
            (
                (
                    Utils.GetGeoCellType(Level!.GeoMatrix, gx1, gy1, layer) != GeoType.Solid && 
                    Utils.GetGeoCellType(Level!.GeoMatrix, gx2, gy2, layer) == GeoType.Solid
                ) ||
                layer == 1
            )
        ) {
            
            for (var d = 0; d <= 2; d++) {
                var size = 5 + szAdd*0.5f + d*2;
                var pstDp = Utils.Restrict(sublayer - 1 + d, 0, 29);
                var quad = new Quad(
                    new Vector2(pos.X - size, pos.Y - size),
                    new Vector2(pos.X + size, pos.Y - size),
                    new Vector2(pos.X + size, pos.Y + size),
                    new Vector2(pos.X - size, pos.Y + size)
                );
                
                quad -= new Vector2(4 + 2*d, 4 + 2*d);

                var texture = variant switch {
                    1 => State.rubbleGraf1,
                    2 => State.rubbleGraf2,
                    3 => State.rubbleGraf3,
                    _ => State.rubbleGraf4
                };

                SDraw.Draw_NoWhite_Color(
                    _layers[pstDp],
                    texture,
                    Shaders.WhiteRemoverApplyColor,
                    quad.Rotated(Random.Generate(360)),
                    Color.Blue
                );
            }
        }

        if (
            (Random.Generate(6) > distanceToAir && Random.Generate(3) == 1) || 
            (
                (
                    Utils.GetGeoCellType(Level!.GeoMatrix, gx2, gy2, layer) != GeoType.Solid && 
                    Utils.GetGeoCellType(Level!.GeoMatrix, gx1, gy1, layer) == GeoType.Solid
                ) ||
                layer == 1
            )
        ) {
            
            for (var d = 0; d <= 2; d++) {
                var size = 5 + szAdd*0.5f + d*2;
                var pstDp = Utils.Restrict(sublayer - 1 + d, 0, 29);
                var quad = new Quad(
                    new Vector2(pos.X - size, pos.Y - size),
                    new Vector2(pos.X + size, pos.Y - size),
                    new Vector2(pos.X + size, pos.Y + size),
                    new Vector2(pos.X - size, pos.Y + size)
                );
                
                quad += new Vector2(4 + 2*d, 4 + 2*d);

                var texture = variant switch {
                    1 => State.rubbleGraf1,
                    2 => State.rubbleGraf2,
                    3 => State.rubbleGraf3,
                    _ => State.rubbleGraf4
                };

                SDraw.Draw_NoWhite_Color(
                    _layers[pstDp],
                    texture,
                    Shaders.WhiteRemoverApplyColor,
                    quad.Rotated(Random.Generate(360)),
                    Color.Blue
                );
            }
        }
    }

    protected virtual void DrawDirtMaterial_MTX(
        in MaterialDefinition mat,
        int x,
        int y,
        int layer,
        in RenderCamera camera,
        in RenderTexture2D rt
    ) {
        var sublayer  = layer * 10;
        var pos = new Vector2(x, y) * 20 - camera.Coords + Vector2.One * 10;

        var optOut = false;

        if (layer > 1) {
            optOut = Utils.GetGeoCellType(Level!.GeoMatrix, x, y, layer - 1) == GeoType.Solid;
        }

        if (optOut) {
            var variable = Random.Generate(4);
            var quad = new Quad(
                new Vector2(pos.X - 18, pos.Y - 18),
                new Vector2(pos.X + 18, pos.Y - 18),
                new Vector2(pos.X + 18, pos.Y + 18),
                new Vector2(pos.X - 18, pos.Y + 18)
            );
            var shader = Shaders.WhiteRemoverApplyColor;
            var texture = variable switch {
                1 => State.rubbleGraf1,
                2 => State.rubbleGraf2,
                3 => State.rubbleGraf3,
                _ => State.rubbleGraf4,
            };

            BeginTextureMode(_layers[sublayer]);
            {
                DrawRectangleRec(new Rectangle(pos - new Vector2(14, 14), new Vector2(28, 28)), Color.Green);
                
                BeginShaderMode(shader);
                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture);
                
                    Draw.DrawQuad(texture, quad.Rotated(Random.Generate(360)), Color.Green);
                EndShaderMode();
            }
            EndTextureMode();
        }
    
        else {

            var distanceToAir = 6;
            var ext = false;
            ReadOnlySpan<(int x, int y)> list = [ (-1,0), (-1,-1), (0,-1), (1,-1), (1,0), (1,1), (0,1), (-1,1) ];

            for (var dist = 1; dist <= distanceToAir; dist++)
            {
                foreach (var (px, py) in list)
                {
                    if (Utils.GetGeoCellType(Level!.GeoMatrix, x + px * dist, y + py * dist, layer) != GeoType.Solid) {
                        distanceToAir = dist;
                        ext = true;
                        break;
                    }
                }

                if (ext) break;
            }

            distanceToAir += Random.Generate(3) - 2;

            if (distanceToAir >= 5) {
                var variable = Random.Generate(4);
                var quad = new Quad(
                    new Vector2(pos.X - 18, pos.Y - 18),
                    new Vector2(pos.X + 18, pos.Y - 18),
                    new Vector2(pos.X + 18, pos.Y + 18),
                    new Vector2(pos.X - 18, pos.Y + 18)
                );
                var shader = Shaders.WhiteRemoverApplyColor;
                var texture = variable switch {
                    1 => State.rubbleGraf1,
                    2 => State.rubbleGraf2,
                    3 => State.rubbleGraf3,
                    _ => State.rubbleGraf4,
                };

                BeginTextureMode(_layers[sublayer]);
                {
                    DrawRectangleRec(new Rectangle(pos - new Vector2(14, 14), new Vector2(28, 28)), Color.Green);
                    
                    BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture);
                    
                        Draw.DrawQuad(texture, quad.Rotated(Random.Generate(360)), Color.Green);
                    EndShaderMode();
                }
                EndTextureMode();
            }
            else {
                var amount = Utils.Lerp(distanceToAir, 3, 0.5f) * 15;

                if (layer > 1) {
                    amount = distanceToAir * 10;
                }

                for (var q = 1; q <= amount; q++) {
                    sublayer = layer * 10 + Random.Generate(10) - 1;
                    pos += new Vector2(Random.Generate(21) - 11, Random.Generate(21) - 11);
                    var variable = Random.Generate(4); 

                    DrawDirtClot(camera, pos, layer, sublayer, variable, distanceToAir);
                }

                if (layer < 2) {
                    list = [ (-1,0), (-1,-1), (0,-1), (1,-1), (1,0), (1,1), (0,1), (-1,1) ];
                    
                    // Loop hell
                    for (var dist = 1; dist <= 3; dist++) {
                        foreach (var (px, py) in list) {
                            if (
                                Utils.GetGeoCellType(Level!.GeoMatrix, x + px * dist, y + py * dist, layer + 1) == GeoType.Solid && 
                                Utils.GetGeoCellType(Level!.GeoMatrix, x + px * dist, y + py * dist, layer) != GeoType.Solid
                            ) {
                                for (var q = 1; q <= 10; q++) {
                                    var dpAdd = 0;

                                    if (layer == 0) {
                                        dpAdd = 6 + Random.Generate(4);
                                    } else {
                                        dpAdd = 2 + Random.Generate(8);
                                    }

                                    pos = Utils.GetMiddleCellPos(new Vector2(x, y) * 20 - camera.Coords + new Vector2(Random.Generate(21) - 11, Random.Generate(21) - 11)) + new Vector2(px, py) * dist * dist * dpAdd * Random.Generate(85) * 0.01f;
                                    var variable = Random.Generate(4);
                                    DrawDirtClot(camera, pos, layer, layer * 10 + dpAdd, variable, distanceToAir);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    protected virtual void DrawSandyMaterial_MTX(

    ) {

    }

    protected virtual void DrawWVMaterial_MTX(
        in MaterialDefinition mat,
        int x,
        int y,
        int layer,
        in RenderCamera camera
    ) {
        var pos = (new Vector2(x, y) - camera.Coords/20f) * 20 - Vector2.One * 10;
        var xPos = ((int)Utils.GetGeoCellType(Level!.GeoMatrix, x, y, layer) - 1) * 20;
        var texture = State.wvTiles[mat.Name];
        var sublayer = layer * 10;

        for (var d = 0; d <= 9; d++) {
            SDraw.Draw_NoWhite_NoColor(
                _layers[sublayer + d],
                texture,
                Shaders.WhiteRemover,
                new Rectangle(xPos, d * 20 + 1, 20, 20),
                new Rectangle(pos.X - 10, pos.Y - 10, 20, 20)
            );
        }
    }

    protected virtual void DrawRidgeMaterial_MTX(
        in MaterialDefinition mat,
        int x,
        int y,
        int layer,
        in RenderCamera camera,
        in RenderTexture2D rt
    ) {
        var distanceToAir = -1;

        ReadOnlySpan<(int x, int y)> list = [ (-1, 0),  (0, -1),  (1, 0),  (0, 1) ];

        for (var dist = 1; dist <= 5; dist++) {
            foreach (var (px, py) in list) {
                if (Utils.GetGeoCellType(Level!.GeoMatrix, x + px * dist, y + py * dist, layer) != GeoType.Solid) {
                    distanceToAir = dist;
                    break;
                }
            }

            if (distanceToAir != -1) break;
        }

        if (distanceToAir == -1) distanceToAir = 5;

        if (distanceToAir >= 1) {
            var sublayer = layer * 10;
            var dp = sublayer;
            var desct = (new Vector2(x, y) - camera.Coords/20f) * 20 - Vector2.One * 10;
            var pos = desct;

            if (distanceToAir == 1) {
                SDraw.Draw_NoWhite_NoColor(
                    _layers[sublayer + 2],
                    State.ridgeBase,
                    Shaders.WhiteRemover,
                    new Rectangle(0, 0, 22, 22),
                    new Rectangle(pos.X - 10, pos.Y - 10, 20, 20)
                );
            }

            if (Random.Generate(5) <= distanceToAir) {
                var variable = Random.Generate(30);
                var quad = new Quad(
                    new Vector2(pos.X - 30, pos.Y - 30),    // Top left
                    new Vector2(pos.X + 30, pos.Y - 30),    // Top right
                    new Vector2(pos.X + 30, pos.Y + 30),    // Bottom right
                    new Vector2(pos.X - 30, pos.Y + 30)     // Bottom left
                );

                SDraw.Draw_NoWhite_NoColor(
                    rt,
                    State.ridgeRocks,
                    Shaders.WhiteRemover,
                    new Rectangle((variable - 1) * 52, 1, 52, 52),
                    quad.Rotated(Random.Generate(15))
                );
            }

            for (var q = 1; q <= distanceToAir; q++) {
                if (distanceToAir == 1) {
                    dp = sublayer + Random.Generate(2) - 1;
                } else {
                    dp = sublayer + Random.Generate(10) - 1;
                }

                pos = desct + new Vector2(Random.Generate(21) - 11, Random.Generate(21) - 11);
                var variable = Random.Generate(30);
                var quad = new Quad(
                    new Vector2(pos.X - 30, pos.Y - 30),    // Top left
                    new Vector2(pos.X + 30, pos.Y - 30),    // Top right
                    new Vector2(pos.X + 30, pos.Y + 30),    // Bottom right
                    new Vector2(pos.X - 30, pos.Y + 30)     // Bottom left
                );

                SDraw.Draw_NoWhite_NoColor(
                    _layers[dp],
                    State.ridgeRocks,
                    Shaders.WhiteRemover,
                    new Rectangle((variable - 1) * 52, 1, 52, 52),
                    quad.Rotated(Random.Generate(15))
                );
            }
        }
    }

    private int DistanceToAir(int x, int y, int layer) {
        var distanceToAir = 8;
        ReadOnlySpan<(int x, int y)> list = [ (-1,0), (-1,-1), (0,-1), (1,-1), (1,0), (1,1), (0,1), (-1,1) ];

        for (var dist = 1; dist <= 7; dist++) {
            foreach (var dir in list) {
                var dx = dir.x + x * dist;
                var dy = dir.y + y * dist;

                if (
                    Utils.GetGeoCellType(Level!.GeoMatrix, dx, dy, layer) != GeoType.Solid &&
                    Utils.GetGeoCellType(Level!.GeoMatrix, dx, dy, Utils.Restrict(layer - 1, 0, 2)) != GeoType.Solid
                ) {
                    return dist;
                }
            }
        }

        return distanceToAir;
    }

    private int DPStartLayerOfTile(int x, int y, int layer) {
        var distanceToAir = DistanceToAir(x, y, layer);

        var pushIn = 6 - distanceToAir;
        pushIn = pushIn - Utils.BoolInt(layer == 0) - 3 * Utils.BoolInt(layer == 2);
        pushIn = Utils.Restrict(pushIn, -4 * Utils.BoolInt(layer > 0) - 5 * Utils.BoolInt(layer == 2), 9 - 5 * Utils.BoolInt(layer == 0));
    
        return Utils.Restrict(pushIn, 0, 29);
    }

    private (int x, int y) DPCircuitConnection() {
        if (Random.Generate(2) == 1) return (Random.Generate(2) - 1, Random.Generate(2) - 1);

        if (Random.Generate(2) == 1) return (1, 0);

        return (0, 1);
    }

    protected virtual void DrawDensePipeMaterial_MTX(
        in MaterialDefinition mat,
        int x,
        int y,
        int layer,
        in RenderCamera camera,
        in RenderTexture2D rt
    ) {
        var pos = (new Vector2(x, y) - camera.Coords/20f) * 20 - Vector2.One * 10;
        var pstLr = DPStartLayerOfTile(x, y, layer);

        if (mat.Name is "Shallow Circuits" or "Shallow Dense Pipes") {
            pstLr = layer * 10;
        }

        if ((int)Utils.GetGeoCellType(Level!.GeoMatrix, x, y, layer) > 1) {
            var type = Utils.GetGeoCellType(Level!.GeoMatrix, x, y, layer);
            var variable = 16;

            variable = type switch {
                GeoType.SlopeNE => 20,
                GeoType.SlopeNW => 19,
                GeoType.SlopeES => 17,
                GeoType.SlopeSW => 18,
                GeoType.Platform => Configuration.MaterialFixes ? 21 : variable,
                GeoType.Glass => Configuration.MaterialFixes ? 22 : variable,
                
                _ => variable
            };

            for (var q = pstLr; q <= layer * 10; q++) {
                if (mat.Name == "Shallow Circuits") {
                    SDraw.Draw_NoWhite_NoColor(
                        _layers[q],
                        State.circuitsImage,
                        Shaders.WhiteRemover,
                        new Rectangle((variable - 1) * 40, 1, 40, 40),
                        new Rectangle(pos - Vector2.One * 20, Vector2.One * 40)
                    );
                } else if (mat.Name == "Shallow Dense Pipes") {
                    SDraw.Draw_NoWhite_NoColor(
                        _layers[q],
                        State.densePipesImage,
                        Shaders.WhiteRemover,
                        new Rectangle((variable - 1) * 40, 1, 40, 40),
                        new Rectangle(pos - Vector2.One * 20, Vector2.One * 40)
                    );
                } else {
                    SDraw.Draw_NoWhite_NoColor(
                        _layers[q],
                        State.densePipesImages[mat.Name],
                        Shaders.WhiteRemover,
                        new Rectangle((variable - 1) * 40, 1, 40, 40),
                        new Rectangle(pos - Vector2.One * 20, Vector2.One * 40)
                    );
                }
            }
        }
        else {
            ReadOnlySpan<string> list = [
                "0000", "1111", "0101", "1010", 
                "0001", "1000", "0100", "0010", 
                "1001", "1100", "0110", "0011", 
                "1011", "1101", "1110", "0111"
            ];

            var leftDp = DPStartLayerOfTile(x - 1, y, layer);
            var rightDp = DPStartLayerOfTile(x + 1, y, layer);
            var topDp = DPStartLayerOfTile(x, y - 1, layer);
            var bottomDp = DPStartLayerOfTile(x, y + 1, layer);
        
            for (var q = pstLr; q <= layer * 10; q++) {
                var left = Utils.BoolInt(Utils.IsGeoCellSolid(Level!.GeoMatrix, x - 1, y, layer)) * DPCircuitConnection().x * Utils.BoolInt(leftDp <= q);
                var right = Utils.BoolInt(Utils.IsGeoCellSolid(Level!.GeoMatrix, x + 1, y, layer)) * DPCircuitConnection().x * Utils.BoolInt(rightDp <= q);
                var top = Utils.BoolInt(Utils.IsGeoCellSolid(Level!.GeoMatrix, x, y - 1, layer)) * DPCircuitConnection().y * Utils.BoolInt(topDp <= q);
                var bottom = Utils.BoolInt(Utils.IsGeoCellSolid(Level!.GeoMatrix, x, y + 1, layer)) * DPCircuitConnection().y * Utils.BoolInt(bottomDp <= q);

                if (mat.Name is "Shallow Circuits" or "Shallow Dense Pipes") {
                    left = Utils.BoolInt(Utils.IsGeoCellSolid(Level!.GeoMatrix, x - 1, y, layer)) * DPCircuitConnection().x;
                    right = Utils.BoolInt(Utils.IsGeoCellSolid(Level!.GeoMatrix, x + 1, y, layer)) * DPCircuitConnection().x;
                    top = Utils.BoolInt(Utils.IsGeoCellSolid(Level!.GeoMatrix, x, y - 1, layer)) * DPCircuitConnection().y;
                    bottom = Utils.BoolInt(Utils.IsGeoCellSolid(Level!.GeoMatrix, x, y + 1, layer)) * DPCircuitConnection().y;
                }
            
                if (
                    Utils.GetGeoCellType(Level!.GeoMatrix, x - 1, y, layer) is not GeoType.Air &&
                    (
                        !Configuration.MaterialFixes || 
                        Utils.GetGeoCellType(Level!.GeoMatrix, x - 1, y, layer) is not GeoType.Glass
                    )
                ) {
                    left = 1;
                }

                if (
                    Utils.GetGeoCellType(Level!.GeoMatrix, x + 1, y, layer) is not GeoType.Air &&
                    (
                        !Configuration.MaterialFixes || 
                        Utils.GetGeoCellType(Level!.GeoMatrix, x + 1, y, layer) is not GeoType.Glass
                    )
                ) {
                    right = 1;
                }

                if (
                    Utils.GetGeoCellType(Level!.GeoMatrix, x, y - 1, layer) is not GeoType.Air &&
                    (
                        !Configuration.MaterialFixes || 
                        Utils.GetGeoCellType(Level!.GeoMatrix, x, y - 1, layer) is not GeoType.Glass
                    )
                ) {
                    top = 1;
                }

                if (
                    Utils.GetGeoCellType(Level!.GeoMatrix, x, y + 1, layer) is not GeoType.Air &&
                    (
                        !Configuration.MaterialFixes || 
                        Utils.GetGeoCellType(Level!.GeoMatrix, x, y + 1, layer) is not GeoType.Glass
                    )
                ) {
                    bottom = 1;
                }

                var variable = list.IndexOf($"{left}{top}{right}{bottom}");
                var rand = 1;

                if (mat.Name is "Circuits" or "Shallow Circuits") {
                    rand = Random.Generate(5);
                }

                if (mat.Name == "Shallow Circuits") {
                    SDraw.Draw_NoWhite_NoColor(
                        _layers[q],
                        State.circuitsImage,
                        Shaders.WhiteRemover,
                        new Rectangle(variable*40, 1 + (rand - 1)*40, 40, 40),
                        new Rectangle(pos - Vector2.One * 20, Vector2.One * 40)
                    );
                } else if (mat.Name == "Shallow Dense Pipes") {
                    SDraw.Draw_NoWhite_NoColor(
                        _layers[q],
                        State.densePipesImage,
                        Shaders.WhiteRemover,
                        new Rectangle(variable*40, 1 + (rand - 1)*40, 40, 40),
                        new Rectangle(pos - Vector2.One * 20, Vector2.One * 40)
                    );
                } else {
                    SDraw.Draw_NoWhite_NoColor(
                        _layers[q],
                        State.densePipesImages[mat.Name],
                        Shaders.WhiteRemover,
                        new Rectangle(variable*40, 1 + (rand - 1)*40, 40, 40),
                        new Rectangle(pos - Vector2.One * 20, Vector2.One * 40)
                    );
                }
            }
        }
    }

    protected virtual void DrawRandomPipesMaterial_MTX(
        in MaterialDefinition mat,
        int x,
        int y,
        int layer,
        in RenderCamera camera,
        in RenderTexture2D rt
    ) {
        var pos = (new Vector2(x, y) - camera.Coords/20f) * 20 - Vector2.One * 10;

        var cellType = Utils.GetGeoCellType(Level!.GeoMatrix, x, y, layer);

        if (cellType is not GeoType.Air or GeoType.Solid) {
            var variable = 16;

            variable = cellType switch {
                GeoType.SlopeNE => 20,
                GeoType.SlopeNW => 19,
                GeoType.SlopeES => 17,
                GeoType.SlopeSW => 18,
                GeoType.Platform => Random.Generate(2) - 1 == 1 ? 25 : 21,
                GeoType.Glass => 22,
                
                _ => variable
            };

            var sublayer = layer * 10;

            if (cellType is GeoType.Platform) {
                for (var d = 0; d <= 9; d++) {
                    SDraw.Draw_NoWhite_NoColor(
                        _layers[sublayer + d],
                        State.densePipesImages2[mat.Name],
                        Shaders.WhiteRemover,
                        new Rectangle((variable - 1)*20, d * 20, 20, 20),
                        new Rectangle(pos - Vector2.One * 10, Vector2.One * 20)
                    );
                }
            }
            else {
                for (var d = 0; d <= 2; d++) {
                    SDraw.Draw_NoWhite_NoColor(
                        _layers[sublayer + d],
                        State.densePipesImages2[mat.Name],
                        Shaders.WhiteRemover,
                        new Rectangle((variable - 1)*20, d * 20, 20, 20),
                        new Rectangle(pos - Vector2.One * 10, Vector2.One * 20)
                    );
                }

                for (var d = 3; d <= 6; d++) {
                    SDraw.Draw_NoWhite_NoColor(
                        _layers[sublayer + d],
                        State.densePipesImages2[mat.Name],
                        Shaders.WhiteRemover,
                        new Rectangle((variable - 1)*20, 3 * 20, 20, 20),
                        new Rectangle(pos - Vector2.One * 10, Vector2.One * 20)
                    );
                }

                var tf = 7;
                
                for (var d = 4; d <= 6; d++) {
                    SDraw.Draw_NoWhite_NoColor(
                        _layers[sublayer + tf],
                        State.densePipesImages2[mat.Name],
                        Shaders.WhiteRemover,
                        new Rectangle((variable - 1)*20, d * 20, 20, 20),
                        new Rectangle(pos - Vector2.One * 10, Vector2.One * 20)
                    );

                    tf++;
                }
            }
        }
        else {
            ReadOnlySpan<string> list = [
                "0000", "1111", "0101", "1010", 
                "0001", "1000", "0100", "0010", 
                "1001", "1100", "0110", "0011", 
                "1011", "1101", "1110", "0111"
            ];

            var sublayer = layer * 10;

            var left = Utils.BoolInt(Utils.IsGeoCellSolid(Level!.GeoMatrix, x - 1, y, layer)) * DPCircuitConnection().x;
            var right = Utils.BoolInt(Utils.IsGeoCellSolid(Level!.GeoMatrix, x + 1, y, layer)) * DPCircuitConnection().x;
            var top = Utils.BoolInt(Utils.IsGeoCellSolid(Level!.GeoMatrix, x, y - 1, layer)) * DPCircuitConnection().y;
            var bottom = Utils.BoolInt(Utils.IsGeoCellSolid(Level!.GeoMatrix, x, y + 1, layer)) * DPCircuitConnection().y;

            if (
                Utils.GetGeoCellType(Level!.GeoMatrix, x - 1, y, layer) is not GeoType.Air &&
                (
                    !Configuration.MaterialFixes || 
                    Utils.GetGeoCellType(Level!.GeoMatrix, x - 1, y, layer) is not GeoType.Glass
                )
            ) {
                left = 1;
            }

            if (
                Utils.GetGeoCellType(Level!.GeoMatrix, x + 1, y, layer) is not GeoType.Air &&
                (
                    !Configuration.MaterialFixes || 
                    Utils.GetGeoCellType(Level!.GeoMatrix, x + 1, y, layer) is not GeoType.Glass
                )
            ) {
                right = 1;
            }

            if (
                Utils.GetGeoCellType(Level!.GeoMatrix, x, y - 1, layer) is not GeoType.Air &&
                (
                    !Configuration.MaterialFixes || 
                    Utils.GetGeoCellType(Level!.GeoMatrix, x, y - 1, layer) is not GeoType.Glass
                )
            ) {
                top = 1;
            }

            if (
                Utils.GetGeoCellType(Level!.GeoMatrix, x, y + 1, layer) is not GeoType.Air &&
                (
                    !Configuration.MaterialFixes || 
                    Utils.GetGeoCellType(Level!.GeoMatrix, x, y + 1, layer) is not GeoType.Glass
                )
            ) {
                bottom = 1;
            }

            var variable = list.IndexOf($"{left}{top}{right}{bottom}") + 1;

            variable = variable switch {
                3 => Random.Generate(2) == 2 ? 23 : 3,
                4 => Random.Generate(2) == 2 ? 24 : 4,
                _ => Random.Generate(3) switch { 1 => 1, 2 => 26, _ => 27 },
            };

            for (var d = 0; d <= 2; d++) {
                SDraw.Draw_NoWhite_NoColor(
                    _layers[sublayer + d],
                    State.densePipesImages2[mat.Name],
                    Shaders.WhiteRemover,
                    new Rectangle((variable - 1)*20, d * 20, 20, 20),
                    new Rectangle(pos - Vector2.One * 10, Vector2.One * 20)
                );
            }

            for (var d = 3; d <= 6; d++) {
                SDraw.Draw_NoWhite_NoColor(
                    _layers[sublayer + d],
                    State.densePipesImages2[mat.Name],
                    Shaders.WhiteRemover,
                    new Rectangle((variable - 1)*20, 3 * 20, 20, 20),
                    new Rectangle(pos - Vector2.One * 10, Vector2.One * 20)
                );
            }

            var tf = 7;
            
            for (var d = 4; d <= 6; d++) {
                SDraw.Draw_NoWhite_NoColor(
                    _layers[sublayer + tf],
                    State.densePipesImages2[mat.Name],
                    Shaders.WhiteRemover,
                    new Rectangle((variable - 1)*20, d * 20, 20, 20),
                    new Rectangle(pos - Vector2.One * 10, Vector2.One * 20)
                );

                tf++;
            }
        }
    }
}