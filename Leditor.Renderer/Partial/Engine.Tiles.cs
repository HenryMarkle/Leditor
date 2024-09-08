using Raylib_cs;
using static Raylib_cs.Raylib;

using System.Numerics;

using Leditor.Data;
using Leditor.Data.Tiles;
using Leditor.Data.Geometry;

using Color = Raylib_cs.Color;
using Leditor.Renderer.RL;

namespace Leditor.Renderer.Partial;

public partial class Engine 
{
    /// <summary>
    /// Draws a tile into the canvas
    /// </summary>
    /// <param name="tile">Tile definition</param>
    /// <param name="x">Matrix X coordinates</param>
    /// <param name="y">Matrix Y coordinates</param>
    /// <param name="layer">The current layer (0, 1, 2)</param>
    /// <param name="camera">The current render camera</param>
    /// <param name="rt">the temprary canvas to draw on</param>
    protected virtual void DrawTile_MTX(
        Tile cell,
        TileDefinition tile,
        int x, 
        int y, 
        int layer,
        in RenderCamera camera, 
        RenderTexture2D rt
    )
    {
        var (hx, hy) = Data.Utils.GetTileHeadPositionI(tile);

        int startX = x - hx;
        int startY = y - hy;

        var colored = tile.Tags.Contains("colored");
        var effectColorA = tile.Tags.Contains("effectColorA");
        var effectColorB = tile.Tags.Contains("effectColorB");

        var (width, height) = tile.Size;
        var bf = tile.BufferTiles;

        SetTextureWrap(tile.Texture, TextureWrap.MirrorClamp);

        switch (tile.Type)
        {
            case TileType.Box:
            {
                var num = tile.Size.Width * tile.Size.Height;
                var n = 0;

                for (var g = startX; g < startX + tile.Size.Width; g++)
                {
                    for (var h = startY; h < startY + tile.Size.Height; h++)
                    {
                        var rect = new Rectangle(g * 20, h * 20, 20, 20);
                    
                        BeginTextureMode(State.vertImg);
                        DrawTexturePro(tile.Texture, new(20, n * 20, 20, 20), new(g * 20, h * 20, 20, 20), Vector2.Zero, 0, Color.White);
                        EndTextureMode();

                        BeginTextureMode(State.horiImg);
                        DrawTexturePro(tile.Texture, new( 0, n * 20, 20, 20), new(g * 20, h * 20, 20, 20), Vector2.Zero, 0, Color.White);
                        EndTextureMode();

                        BeginTextureMode(rt);
                        {
                            Rectangle dest = new(
                                startX * 20 - 20 * bf,
                                startY * 20 - 20 * bf,

                                width  * 20 + 2 * 20 * bf,
                                height * 20 + 2 * 20 * bf
                            );

                            Rectangle src = new(
                                0,
                                0 + num * 20,
                                width  * 20 + 2 * 20 * bf,
                                height * 20 + 2 * 20 * bf
                            );

                            var rnd = Random.Generate(tile.Rnd);

                            src.X += src.Width * (rnd - 1);

                            var shader = Shaders.WhiteRemover;
                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);

                            DrawTexturePro(
                                tile.Texture,
                                src,
                                dest,
                                Vector2.Zero,
                                0,
                                Color.White
                            );

                            EndShaderMode();
                        }
                        EndTextureMode();

                        n++;
                    }
                }
            }
            break;
        
            case TileType.VoxelStruct:
            {
                var sublayer = layer * 10;

                Rectangle dest = new(
                    startX * 20 - 20 * bf,
                    startY * 20 - 20 * bf,
                    
                    width  * 20 + 2 * 20 * bf,
                    height * 20 + 2 * 20 * bf
                );

                Rectangle src = new(
                    0,
                    0,
                    (width  + 2 * bf) * 20,
                    (height + 2 * bf) * 20
                );

                int rnd;

                if (tile.Rnd == -1)
                {
                    rnd = 1;

                    foreach (var dir in new (int x, int y)[4] { (-1, 0), (0, -1), (1, 0), (0, 1) })
                    {
                        var cx = x + dir.x + (int)camera.Coords.X;
                        var cy = y + dir.y + (int)camera.Coords.Y;
                    
                        GeoType cellType = GeoType.Solid;

                        if (Data.Utils.InBounds(Level!.GeoMatrix, cx, cy))
                        {
                            cellType = Level!.GeoMatrix[cy, cx, 0].Type;
                        }

                        if (cellType is GeoType.Air or GeoType.Platform) break;
                        
                        rnd++;
                    }
                }
                else
                {
                    rnd = Random.Generate(tile.Rnd);
                }

                if (tile.Tags.Contains("ramp"))
                {
                    rnd = 2;

                    GeoType cellType = GeoType.Solid;

                    var cx = x + (int)camera.Coords.X;
                    var cy = y + (int)camera.Coords.Y;

                    if (Data.Utils.InBounds(Level!.GeoMatrix, cx, y + cy))
                    {
                        cellType = Level!.GeoMatrix[cy, cx, 0].Type;
                    }

                    if (cellType is GeoType.SlopeNW)
                    {
                        rnd = 1;
                    }
                }
            
                src.X += src.Width * (rnd - 1);
                src.Y += 1;

                BeginTextureMode(rt);
                {
                    var shader = Shaders.WhiteRemover;
                    BeginShaderMode(shader);
                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                    DrawTexturePro(
                        tile.Texture,
                        src,
                        dest,
                        Vector2.Zero,
                        0,
                        Color.White
                    );
                    EndShaderMode();
                }
                EndTextureMode();

                var d = -1;

                for (var l = 0; l < tile.Repeat.Length; l++)
                {
                    for (var repeat = 0; repeat < tile.Repeat[l]; repeat++)
                    {
                        d++;

                        if (d + sublayer > 29) goto out_of_repeat;

                        BeginTextureMode(_layers[d + sublayer]);
                        {
                            var shader = Shaders.WhiteRemover;

                            var currentSrc = src with { 
                                X = src.X + src.Width * (rnd - 1), 
                                Y = src.Y + src.Height * l 
                            };
                            
                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                            DrawTexturePro(
                                tile.Texture,
                                currentSrc,
                                dest,
                                Vector2.Zero,
                                0,
                                Color.White
                            );
                            EndShaderMode();

                            if (colored && !effectColorA && !effectColorB)
                            {
                                BeginTextureMode(_layersDC[d + sublayer]);
                                Draw.DrawTextureDarkest(
                                    tile.Texture, 
                                    currentSrc,
                                    dest
                                );
                                EndTextureMode();
                            }

                            if (effectColorA)
                            {
                                BeginTextureMode(_gradientA[d + sublayer]);
                                Draw.DrawTextureDarkest(
                                    tile.Texture,
                                    currentSrc,
                                    dest
                                );
                                EndTextureMode();
                            }

                            if (effectColorB)
                            {
                                BeginTextureMode(_gradientB[d + sublayer]);
                                Draw.DrawTextureDarkest(
                                    tile.Texture,
                                    currentSrc,
                                    dest
                                );
                                EndTextureMode();
                            }
                        }
                        EndTextureMode();
                    }
                }

                out_of_repeat:
                {}
            }
            break;
        
            case TileType.VoxelStructRandomDisplaceHorizontal:
            case TileType.VoxelStructRandomDisplaceVertical:
            {
                var sublayer = layer * 10;

                Rectangle dest = new(
                    (startX - bf) * 20,
                    (startY - bf) * 20,

                    (width  + 2 * bf) * 20,
                    (height + 2 * bf) * 20
                );

                Rectangle src = new(
                    0,
                    0,
                    (width  + 2 * bf) * 20,
                    (height + 2 * bf) * 20
                );

                Rectangle src1, src2, dest1, dest2;

                if (tile.Type is TileType.VoxelStructRandomDisplaceVertical)
                {
                    var rndSeed = Level!.Seed + x;

                    var dsplcPoint = Random.Generate((int)src.Height);

                    src1 = src with { Height = dsplcPoint };
                    src2 = src with { Y = src.Y + dsplcPoint, Height = src.Height - dsplcPoint };

                    dest1 = dest with { Y = dest.Y + dest.Height - dsplcPoint };
                    dest2 = dest with { Height = dest.Height - dsplcPoint };
                }
                else
                {
                    var rndSeed = Level!.Seed + y;

                    var dsplcPoint = Random.Generate((int)src.Width);

                    src1 = src with { Width = dsplcPoint };
                    src2 = src with { X = src.X + dsplcPoint };
                
                    dest1 = dest with { X = dest.X + dest.Width + dsplcPoint };
                    dest2 = dest with { Width = dest.Width - dsplcPoint };
                }

                src1.Y += 1;
                src2.Y += 1;

                BeginTextureMode(rt);
                {
                    var shader = Shaders.WhiteRemover;
                    BeginShaderMode(shader);
                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                    DrawTexturePro(
                        tile.Texture,
                        src1,
                        dest1,
                        Vector2.Zero,
                        0,
                        Color.White
                    );
                    DrawTexturePro(
                        tile.Texture,
                        src2,
                        dest2,
                        Vector2.Zero,
                        0,
                        Color.White
                    );
                    EndShaderMode();
                }
                EndTextureMode();

                var d = -1;

                for (var l = 0; l < tile.Repeat.Length; l++)
                {
                    for (var repeat = 0; repeat < tile.Repeat[l]; repeat++)
                    {
                        d++;

                        if (d + sublayer > 29) goto out_of_repeat;

                        BeginTextureMode(_layers[d + sublayer]);
                        {
                            var shader = Shaders.WhiteRemover;
                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                            DrawTexturePro(
                                tile.Texture,
                                src1 with { Y = src1.Y + src1.Height * l },
                                dest1,
                                Vector2.Zero,
                                0,
                                Color.White
                            );

                            DrawTexturePro(
                                tile.Texture,
                                src2 with { Y = src2.Y + src2.Height * l },
                                dest2,
                                Vector2.Zero,
                                0,
                                Color.White
                            );
                            EndShaderMode();
                        }
                        EndTextureMode();

                        var newSrc1 = src1 with {
                            X = src1.X + (width + 2 * bf) * 20,
                            Y = src1.Y + src1.Height * l + (height + 2 * bf) * 20 
                        };

                        var newSrc2 = src1 with {
                            X = src2.X + (width + 2 * bf) * 20,
                            Y = src2.Y + src2.Height * l + (height + 2 * bf) * 20 
                        };

                        if (colored && !effectColorA && !effectColorB)
                        {
                            BeginTextureMode(_layersDC[d + sublayer]);
                            {
                                var shader = Shaders.WhiteRemover;
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                                DrawTexturePro(
                                    tile.Texture,
                                    newSrc1,
                                    dest1,
                                    Vector2.Zero,
                                    0,
                                    Color.White
                                );

                                DrawTexturePro(
                                    tile.Texture,
                                    newSrc2,
                                    dest2,
                                    Vector2.Zero,
                                    0,
                                    Color.White
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();
                        }

                        if (effectColorA)
                        {
                            BeginTextureMode(_gradientA[d + sublayer]);
                            {
                                Draw.DrawTextureDarkest(
                                    tile.Texture,
                                    newSrc1,
                                    dest1
                                );

                                Draw.DrawTextureDarkest(
                                    tile.Texture,
                                    newSrc2,
                                    dest2
                                );
                            }
                            EndTextureMode();
                        }
                        
                        if (effectColorB)
                        {
                            BeginTextureMode(_gradientB[d + sublayer]);
                            {
                                Draw.DrawTextureDarkest(
                                    tile.Texture,
                                    newSrc1,
                                    dest1
                                );

                                Draw.DrawTextureDarkest(
                                    tile.Texture,
                                    newSrc2,
                                    dest2
                                );
                            }
                            EndTextureMode();
                        }
                    }
                }

                out_of_repeat:
                {}
            }
            break;
        
            case TileType.VoxelStructRockType:
            {
                var sublayer = layer * 10;

                Rectangle dest = new(
                    (startX - bf) * 20,
                    (startY - bf) * 20,

                    (width  + 2 * bf) * 20,
                    (height + 2 * bf) * 20
                );

                Rectangle src = new(
                    0,
                    1,
                    (width  + 2 * bf) * 20,
                    (height + 2 * bf) * 20
                );

                var rnd = Random.Generate(tile.Rnd);

                for (var d = sublayer; d < Utils.Restrict(sublayer + 9 + (10 * Utils.BoolInt(tile.HasSpecsLayer(1))), 0, 29); d++)
                {
                    if (d is 12 or 8 or 4)
                    {
                        rnd = Random.Generate(tile.Rnd);
                    }

                    BeginTextureMode(_layers[d]);
                    {
                        var shader = Shaders.WhiteRemover;
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                        DrawTexturePro(
                            tile.Texture,
                            src with { X = src.X + src.Width * (rnd - 1) },
                            dest,
                            Vector2.Zero,
                            0,
                            Color.White
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    if (colored && !effectColorA && !effectColorB)
                    {
                        BeginTextureMode(_layersDC[d]);
                        var shader = Shaders.WhiteRemover;
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                        DrawTexturePro(
                            tile.Texture, 
                            src with { X = src.X + src.Width * (rnd - 1) + (width + 2 * bf) * 20 * tile.Rnd }, 
                            dest,
                            Vector2.Zero,
                            0,
                            Color.White
                        );
                        EndShaderMode();
                        EndTextureMode();
                    }

                    if (effectColorA)
                    {
                        BeginTextureMode(_gradientA[d]);
                        Draw.DrawTextureDarkest(
                            tile.Texture, 
                            src with { X = src.X + src.Width * (rnd - 1) + (width + 2 * bf) * 20 * tile.Rnd }, 
                            dest
                        );
                        EndShaderMode();
                    }

                    if (effectColorB)
                    {
                        BeginTextureMode(_gradientB[d]);
                        Draw.DrawTextureDarkest(
                            tile.Texture, 
                            src with { X = src.X + src.Width * (rnd - 1) + (width + 2 * bf) * 20 * tile.Rnd }, 
                            dest
                        );
                        EndShaderMode();
                    }
                }
            }
            break;
        
            case TileType.VoxelStructSandType:
            {
                var sublayer = layer * 10 + 1;

                Rectangle dest = new(
                    (startX - bf) * 20,
                    (startY - bf) * 20,

                    (width  + 2 * bf) * 20,
                    (height + 2 * bf) * 20
                );

                Rectangle src = new(
                    0,
                    1,
                    (width  + 2 * bf) * 20,
                    (height + 2 * bf) * 20
                );

                for (var d = sublayer; d < Utils.Restrict(sublayer + 9 + (10 * Utils.BoolInt(tile.HasSpecsLayer(1))), 0, 29); d++)
                {
                    var rnd = Random.Generate(tile.Rnd);

                    var newSrc = src with {
                        X = src.X + src.Width * (rnd - 1)
                    };

                    BeginTextureMode(_layers[d]);
                    {
                        var shader = Shaders.WhiteRemover;
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                        DrawTexturePro(
                            tile.Texture,
                            newSrc,
                            dest,
                            Vector2.Zero,
                            0,
                            Color.White
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    newSrc.X += (width + 2 * bf) * 20 * tile.Rnd;

                    if (colored && !effectColorA && !effectColorB)
                    {
                        BeginTextureMode(_layersDC[d]);
                        var shader = Shaders.WhiteRemover;
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), tile.Texture);
                        DrawTexturePro(
                            tile.Texture,
                            newSrc,
                            dest,
                            Vector2.Zero,
                            0,
                            Color.White
                        );
                        EndShaderMode();
                        EndTextureMode();
                    }

                    if (effectColorA)
                    {
                        BeginTextureMode(_gradientA[d]);
                        Draw.DrawTextureDarkest(
                            tile.Texture,
                            newSrc,
                            dest
                        );
                        EndTextureMode();
                    }

                    if (effectColorB)
                    {
                        BeginTextureMode(_gradientB[d]);
                        Draw.DrawTextureDarkest(
                            tile.Texture,
                            newSrc,
                            dest
                        );
                        EndTextureMode();
                    }
                }
            }
            break;
        }

        // Special behavior tags

        foreach (var tag in tile.Tags)
        {
            switch (tag)
            {
                case "Chain Holder":
                {
                    var (cx, cy) = cell.SecondChainHolderPosition;

                    if (cx is -1 || cy is -1) continue;

                    Vector2 p1 = new Vector2(x, y) * 20 / 2 + new Vector2(10.1f, 10.1f);
                    Vector2 p2 = (new Vector2(cx, cy) - camera.Coords) * 20 / 2 + new Vector2(10.1f, 10.1f);

                    var sublayer = layer * 10 + 2;

                    var steps = (int)(Utils.Diag(p1, p2) / 12.0f + 0.4999f);
                    var dr = Utils.MoveToPoint(p1, p2, 1.0f);
                    var ornt = Random.Generate(2) - 1;
                    var degDir = Utils.LookAtPoint(p1, p2);
                    var stp = Random.Generate(100) * 0.01f;

                    for (var q = 1; q <= steps; q++)
                    {
                        var pos = p1 + (dr * 12 * (q - stp));

                        Rectangle dest, src;

                        if (ornt != 0)
                        {
                            dest = new(
                                pos.X - 6,
                                pos.Y - 10,
                                12,
                                20
                            );

                            src = new(
                                0,
                                0,
                                12,
                                20
                            );

                            ornt = 0;
                        }
                        else
                        {
                            dest = new(
                                pos.X - 2,
                                pos.Y - 10,
                                4,
                                10
                            );

                            src = new(
                                13,
                                0,
                                29,
                                20
                            );

                            ornt = 1;
                        }

                        BeginTextureMode(_layers[sublayer]);
                        {
                            var shader = Shaders.WhiteRemoverApplyColor;
                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.bigChainHolder);
                            Draw.DrawQuad(
                                State.bigChainHolder,
                                src,
                                Utils.RotateRect(dest, degDir),
                                Color.Red
                            );
                            EndShaderMode();
                        }
                        EndTextureMode();
                    }
                }
                break;
                case "fanBlade":
                {
                    var sublayer = (layer + 1) * 10;

                    if (sublayer > 20) sublayer -= 5;

                    var middle = new Vector2(x * 20 - 10, y * 20 - 10);

                    Quad q = new(
                        middle,                             // Top left
                        new(middle.X + 46, middle.Y),       // Top right
                        new(middle.X + 46, middle.Y + 46),  // Bottom right
                        new(middle.X, middle.Y + 46)        // bottom left
                    );

                    BeginTextureMode(_layers[sublayer - 2]);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.fanBlade);

                        Draw.DrawQuad(
                            State.fanBlade,
                            q.Rotated(Random.Generate(360)),
                            Color.Green
                        );

                        EndShaderMode();
                    }
                    EndTextureMode();

                    BeginTextureMode(_layers[sublayer]);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.fanBlade);

                        Draw.DrawQuad(
                            State.fanBlade,
                            q.Rotated(Random.Generate(360)),
                            Color.Green
                        );

                        EndShaderMode();
                    }
                    EndTextureMode();
                }
                break;
                case "Big Wheel":
                {
                    ReadOnlySpan<int> dpsL = layer switch {
                        0 => [  0,  7 ],
                        1 => [  9, 17 ],
                        _ => [ 19, 27 ]
                    };

                    var offset = new Vector2(x, y) * 20; // Needs tweaking

                    Quad q = new(
                        new Vector2(-90, -90),   // Top left
                        new Vector2( 90, -90),   // Top right
                        new Vector2( 90,  90),   // Bottom right
                        new Vector2(-90,  90)    // Bottom left
                    );

                    q += offset;

                    foreach (var l in dpsL)
                    {
                        var rnd = Random.Generate(360);

                        foreach (var dp in new int[3] { l, l + 1, l + 2 })
                        {
                            BeginTextureMode(_layers[dp]);
                            
                            var shader = Shaders.WhiteRemoverApplyColor;
                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.BigWheelGraf);
                            Draw.DrawQuad(
                                State.BigWheelGraf,
                                q.Rotated(rnd + 0.001f),
                                Color.Green
                            );
                            EndShaderMode();

                            EndTextureMode();
                        }
                    }
                }
                break;
                case "Sawblades":
                {
                    int[] dpsL = layer switch {
                        0 => [  0,  7 ],
                        1 => [  9, 17 ],
                        _ => [ 19, 27 ]
                    };

                    Vector2 offset = Utils.GetMiddleCellPos(x, y) + new Vector2(10, 10);

                    Rectangle rect = new(
                        -90 + offset.X,
                        -90 + offset.Y,
                        180,
                        180
                    );

                    foreach (var l in dpsL)
                    {
                        var rnd = Random.Generate(360);

                        foreach (var dp in new int[3] { l, l + 1, l + 2 })
                        {
                            BeginTextureMode(_layers[dp]);
                            
                            var shader = Shaders.WhiteRemoverApplyColor;
                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.sawbladeGraf);
                            Draw.DrawQuad(
                                State.sawbladeGraf,
                                Utils.RotateRect(rect, rnd + 0.001f),
                                Color.Green
                            );
                            EndShaderMode();

                            EndTextureMode();
                        }
                    }
                }
                break;
            
                case "randomCords":
                {
                    var sublayer = layer * 10 + Random.Generate(9);
                
                    var pnt = Utils.GetMiddleCellPos(new Vector2(x, y + tile.Size.Height/2f));

                    Rectangle rect = new(
                        -50 + pnt.X,
                        -50 + pnt.Y,
                        100,
                        100
                    );

                    var rnd = Random.Generate(7);

                    BeginTextureMode(_layers[sublayer]);
                    {
                        var shader = Shaders.WhiteRemover;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.randomCords);
                        Draw.DrawQuad(
                            State.randomCords,
                            new Rectangle((rnd - 1)*100 + 1, 1, 100, 100),
                            Utils.RotateRect(rect, -30+Random.Generate(60))
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();
                }
                break;
                case "Big Sign":
                case "Big SignB":
                {
                    var sublayer = layer * 10;

                    var texture = LoadRenderTexture(60, 60);

                    var rnd = Random.Generate(20);

                    Rectangle dest = new(3, 3, 26, 30);

                    BeginTextureMode(texture);
                    {
                        ClearBackground(Color.White);

                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.bigSigns1);
                        DrawTexturePro(
                            State.bigSigns1,
                            new((rnd - 1)*26, 0, 26, 30),
                            dest,
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();

                        rnd = Random.Generate(20);
                        dest = new(31, 3, 57, 30);

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.bigSigns1);
                        DrawTexturePro(
                            State.bigSigns1,
                            new((rnd - 1)*26, 0, 26, 30),
                            dest,
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();

                        rnd = Random.Generate(14);
                        dest = new(3, 35, 55, 24);

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.bigSigns2);
                        DrawTexturePro(
                            State.bigSigns2,
                            new((rnd - 1)*55, 0, 55, 24),
                            dest,
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    BeginTextureMode(rt);
                    {
                        var middle = Utils.GetMiddleCellPos(x, y);

                        var shader = Shaders.WhiteRemoverApplyColor;

                        foreach (
                            var (pos, color) 
                            in 
                            new ReadOnlySpan<(Vector2 pos, Color color)>([ 
                                (new(-4, -4), Color.Blue ), 
                                (new(-3, -3), Color.Blue ), 
                                (new( 3,  3), Color.Red  ), 
                                (new( 4,  4), Color.Red  ), 
                                (new(-2, -2), Color.Green), 
                                (new(-1, -1), Color.Green), 
                                (new( 0,  0), Color.Green), 
                                (new( 1,  1), Color.Green), 
                                (new( 2,  2), Color.Green), 
                                (new( 2,  2), Color.Green), 
                            ])
                        )
                        {

                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                            DrawTexturePro(
                                texture.Texture,
                                new(0, 0, 60, 60),
                                new(
                                    -30 + middle.X + pos.X,
                                    -30 + middle.Y + pos.Y,
                                    60,
                                    60
                                ),
                                Vector2.Zero,
                                0,
                                color
                            );
                            EndShaderMode();
                        }
                    
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                        DrawTexturePro(
                            texture.Texture,
                            new(0, 0, 60, 60),
                            new(
                                -30 + middle.X,
                                -30 + middle.Y,
                                60,
                                60
                            ),
                            Vector2.Zero,
                            0,
                            new Color(255, 0, 255, 255)
                        );
                        EndShaderMode();                     
                    }
                    EndTextureMode();


                    Draw.DrawToEffectColor(
                        State.bigSignGradient, 
                        new(0, 0, 60, 60), 
                        new(-30, -30, 60, 60), 
                        tag == "Big Sign" ? _gradientA : _gradientB, 
                        sublayer, 
                        1, 
                        1
                    );

                    UnloadRenderTexture(texture);
                }
                break;
                
                // Highly repetative code incoming.

                case "Big Western Sign":
                case "Big Western Sign Titled":
                {
                    var texture = LoadRenderTexture(36, 48);
                    var rnd = Random.Generate(20);
                    var middle = Utils.GetMiddleCellPos(x, y);
                    middle.X += 10;

                    BeginTextureMode(texture);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        DrawTexturePro(
                            State.bigWesternSigns,
                            new((rnd - 1)*36, 0, 36, 48),
                            new(0, 0, texture.Texture.Width, texture.Texture.Height),
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    ReadOnlySpan<(Vector2, Color)> list = [
                        (new(-4, -4), Color.Blue),
                        (new(-3, -3), Color.Blue),
                        (new( 3,  3), Color.Red),
                        (new( 4,  4), Color.Red),
                        (new(-2, -2), Color.Green),
                        (new(-1, -1), Color.Green),
                        (new( 0,  0), Color.Green),
                        (new( 1,  1), Color.Green),
                        (new( 2,  2), Color.Green),
                        (new( 0,  0), new(255, 0, 255, 255)),
                    ];

                    BeginTextureMode(rt);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                        if (tag == "Big Western Sign Titled")
                        {
                            var tilt = -45.1f + Random.Generate(90);


                            foreach (var (point, color) in list)
                            {
                                Draw.DrawQuad(
                                    texture.Texture,
                                    Utils.RotateRect(new Rectangle(
                                            middle.X - 18 + point.X,
                                            middle.Y - 24 + point.Y,
                                            36,
                                            48
                                        ),
                                        tilt
                                    ),
                                    color
                                );
                            }
                        }
                        else
                        {
                            foreach (var (point, color) in list)
                            {
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 36, 48),
                                    new Rectangle(
                                        middle.X - 18 + point.X,
                                        middle.Y - 24 + point.Y,
                                        36,
                                        48
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                            }
                        }

                        EndShaderMode();
                    }
                    EndTextureMode();

                    var sublayer = layer * 10;

                    Draw.DrawToEffectColor(
                        State.bigSignGradient,
                        new Rectangle(0, 0, 60, 60),
                        new Rectangle(
                            middle.X - 25,
                            middle.Y - 30,
                            50,
                            60
                        ),
                        _gradientA,
                        sublayer,
                        1,
                        1
                    );

                    UnloadRenderTexture(texture);
                }
                break;
            
                case "Big Western Sign B":
                case "Big Western Sign Titled B":
                {
                    var texture = LoadRenderTexture(36, 48);
                    var rnd = Random.Generate(20);
                    var middle = Utils.GetMiddleCellPos(x, y);
                    middle.X += 10;

                    BeginTextureMode(texture);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        DrawTexturePro(
                            State.bigWesternSigns,
                            new((rnd - 1)*36, 0, 36, 48),
                            new(0, 0, texture.Texture.Width, texture.Texture.Height),
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    ReadOnlySpan<(Vector2, Color)> list = [
                        (new(-4, -4), Color.Blue),
                        (new(-3, -3), Color.Blue),
                        (new( 3,  3), Color.Red),
                        (new( 4,  4), Color.Red),
                        (new(-2, -2), Color.Green),
                        (new(-1, -1), Color.Green),
                        (new( 0,  0), Color.Green),
                        (new( 1,  1), Color.Green),
                        (new( 2,  2), Color.Green),
                        (new( 0,  0), new(255, 0, 255, 255)),
                    ];

                    BeginTextureMode(rt);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                        if (tag == "Big Western Sign Titled B")
                        {
                            var tilt = -45.1f + Random.Generate(90);


                            foreach (var (point, color) in list)
                            {
                                Draw.DrawQuad(
                                    texture.Texture,
                                    Utils.RotateRect(new Rectangle(
                                            middle.X - 18 + point.X,
                                            middle.Y - 24 + point.Y,
                                            36,
                                            48
                                        ),
                                        tilt
                                    ),
                                    color
                                );
                            }
                        }
                        else
                        {
                            foreach (var (point, color) in list)
                            {
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 36, 48),
                                    new Rectangle(
                                        middle.X - 18 + point.X,
                                        middle.Y - 24 + point.Y,
                                        36,
                                        48
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                            }
                        }

                        EndShaderMode();
                    }
                    EndTextureMode();

                    var sublayer = layer * 10;

                    Draw.DrawToEffectColor(
                        State.bigSignGradient,
                        new Rectangle(0, 0, 60, 60),
                        new Rectangle(
                            middle.X - 25,
                            middle.Y - 30,
                            50,
                            60
                        ),
                        _gradientB,
                        sublayer,
                        1,
                        1
                    );

                    UnloadRenderTexture(texture);
                }
                break;
            
                case "Small Asian Sign": 
                case "small asian sign on wall":
                {
                    var texture = LoadRenderTexture(20, 20);
                    var rnd = Random.Generate(14);

                    Rectangle dest = new(
                        0,
                        1,
                        20,
                        17
                    );

                    BeginTextureMode(texture);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.smallAsianSigns);
                        DrawTexturePro(
                            State.smallAsianSigns,
                            dest,
                            new Rectangle(
                                (rnd - 1) * 20,
                                0,
                                20,
                                17
                            ),
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    ReadOnlySpan<(Vector2, Color)> list = [
                        (new(-4, -4), Color.Blue),
                        (new(-3, -3), Color.Blue),
                        (new( 3,  3), Color.Red),
                        (new( 4,  4), Color.Red),
                        (new(-2, -2), Color.Green),
                        (new(-1, -1), Color.Green),
                        (new( 0,  0), Color.Green),
                        (new( 1,  1), Color.Green),
                        (new( 2,  2), Color.Green),
                        (new( 0,  0), new Color(255, 0, 255, 255)),
                    ];

                    if (tag == "Small Asian Sign")
                    {
                        var middle = Utils.GetMiddleCellPos(x, y);

                        BeginTextureMode(rt);
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);

                        foreach (var (point, color) in list)
                        {
                            DrawTexturePro(
                                texture.Texture,
                                new Rectangle(0, 0, 20, 20),
                                new Rectangle(
                                    -10 + middle.X + point.X,
                                    -10 + middle.Y + point.Y,
                                    20,
                                    20
                                ),
                                Vector2.Zero,
                                0,
                                color
                            );
                        }

                        EndShaderMode();

                        EndTextureMode();

                        Draw.DrawToEffectColor(
                            State.bigSignGradient,
                            new Rectangle(0, 0, 60, 60),
                            new Rectangle(-13, -13, 26, 26),
                            _gradientA,
                            layer * 10,
                            1
                        );
                    }
                    else
                    {
                        var sublayer = layer * 10 + 8;
                        var middle = Utils.GetMiddleCellPos(x, y);

                        var shader = Shaders.WhiteRemoverApplyColor;

                        foreach (var (point, color) in list)
                        {
                            BeginTextureMode(_layers[sublayer]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 20, 20),
                                    new Rectangle(
                                        -10 + middle.X + point.X,
                                        -10 + middle.Y + point.Y,
                                        20,
                                        20
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();

                            BeginTextureMode(_layers[sublayer + 1]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 20, 20),
                                    new Rectangle(
                                        -10 + middle.X + point.X,
                                        -10 + middle.Y + point.Y,
                                        20,
                                        20
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();
                        }
                    
                        Draw.DrawToEffectColor(
                            State.bigSignGradient,
                            new Rectangle(0, 0, 60, 60),
                            new Rectangle(middle.X -13, middle.Y - 13, 26, 26),
                            _gradientA,
                            sublayer,
                            1,
                            1
                        );
                    }

                    UnloadRenderTexture(texture);
                }
                break;

                case "Small Asian Sign B": 
                case "small asian sign on wall B":
                {
                    var texture = LoadRenderTexture(20, 20);
                    var rnd = Random.Generate(14);

                    Rectangle dest = new(
                        0,
                        1,
                        20,
                        17
                    );

                    BeginTextureMode(texture);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.smallAsianSigns);
                        DrawTexturePro(
                            State.smallAsianSigns,
                            dest,
                            new Rectangle(
                                (rnd - 1) * 20,
                                0,
                                20,
                                17
                            ),
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    ReadOnlySpan<(Vector2, Color)> list = [
                        (new(-4, -4), Color.Blue),
                        (new(-3, -3), Color.Blue),
                        (new( 3,  3), Color.Red),
                        (new( 4,  4), Color.Red),
                        (new(-2, -2), Color.Green),
                        (new(-1, -1), Color.Green),
                        (new( 0,  0), Color.Green),
                        (new( 1,  1), Color.Green),
                        (new( 2,  2), Color.Green),
                        (new( 0,  0), new Color(255, 0, 255, 255)),
                    ];

                    if (tag == "Small Asian Sign B")
                    {
                        var middle = Utils.GetMiddleCellPos(x, y);

                        BeginTextureMode(rt);
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);

                        foreach (var (point, color) in list)
                        {
                            DrawTexturePro(
                                texture.Texture,
                                new Rectangle(0, 0, 20, 20),
                                new Rectangle(
                                    -10 + middle.X + point.X,
                                    -10 + middle.Y + point.Y,
                                    20,
                                    20
                                ),
                                Vector2.Zero,
                                0,
                                color
                            );
                        }

                        EndShaderMode();

                        EndTextureMode();

                        Draw.DrawToEffectColor(
                            State.bigSignGradient,
                            new Rectangle(0, 0, 60, 60),
                            new Rectangle(-13, -13, 26, 26),
                            _gradientB,
                            layer * 10,
                            1
                        );
                    }
                    else
                    {
                        var sublayer = layer * 10 + 8;
                        var middle = Utils.GetMiddleCellPos(x, y);

                        var shader = Shaders.WhiteRemoverApplyColor;

                        foreach (var (point, color) in list)
                        {
                            BeginTextureMode(_layers[sublayer]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 20, 20),
                                    new Rectangle(
                                        -10 + middle.X + point.X,
                                        -10 + middle.Y + point.Y,
                                        20,
                                        20
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();

                            BeginTextureMode(_layers[sublayer + 1]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 20, 20),
                                    new Rectangle(
                                        -10 + middle.X + point.X,
                                        -10 + middle.Y + point.Y,
                                        20,
                                        20
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();
                        }
                    
                        Draw.DrawToEffectColor(
                            State.bigSignGradient,
                            new Rectangle(0, 0, 60, 60),
                            new Rectangle(middle.X -13, middle.Y - 13, 26, 26),
                            _gradientB,
                            sublayer,
                            1,
                            1
                        );
                    }

                    UnloadRenderTexture(texture);
                }
                break;

                case "Small Asian Sign Station": 
                case "Small Asian Sign On Wall Station":
                {
                    var texture = LoadRenderTexture(20, 20);
                    var rnd = Random.Generate(14);

                    Rectangle dest = new(
                        0,
                        1,
                        20,
                        17
                    );

                    BeginTextureMode(texture);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.smallAsianSignsStation);
                        DrawTexturePro(
                            State.smallAsianSignsStation,
                            dest,
                            new Rectangle(
                                (rnd - 1) * 20,
                                0,
                                20,
                                17
                            ),
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    ReadOnlySpan<(Vector2, Color)> list = [
                        (new(-4, -4), Color.Blue),
                        (new(-3, -3), Color.Blue),
                        (new( 3,  3), Color.Red),
                        (new( 4,  4), Color.Red),
                        (new(-2, -2), Color.Green),
                        (new(-1, -1), Color.Green),
                        (new( 0,  0), Color.Green),
                        (new( 1,  1), Color.Green),
                        (new( 2,  2), Color.Green),
                        (new( 0,  0), new Color(255, 0, 255, 255)),
                    ];

                    if (tag == "Small Asian Sign Station")
                    {
                        var middle = Utils.GetMiddleCellPos(x, y);

                        BeginTextureMode(rt);
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);

                        foreach (var (point, color) in list)
                        {
                            DrawTexturePro(
                                texture.Texture,
                                new Rectangle(0, 0, 20, 20),
                                new Rectangle(
                                    -10 + middle.X + point.X,
                                    -10 + middle.Y + point.Y,
                                    20,
                                    20
                                ),
                                Vector2.Zero,
                                0,
                                color
                            );
                        }

                        EndShaderMode();

                        EndTextureMode();

                        Draw.DrawToEffectColor(
                            State.bigSignGradient,
                            new Rectangle(0, 0, 60, 60),
                            new Rectangle(-13, -13, 26, 26),
                            _gradientA,
                            layer * 10,
                            1
                        );
                    }
                    else
                    {
                        var sublayer = layer * 10 + 8;
                        var middle = Utils.GetMiddleCellPos(x, y);

                        var shader = Shaders.WhiteRemoverApplyColor;

                        foreach (var (point, color) in list)
                        {
                            BeginTextureMode(_layers[sublayer]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 20, 20),
                                    new Rectangle(
                                        -10 + middle.X + point.X,
                                        -10 + middle.Y + point.Y,
                                        20,
                                        20
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();

                            BeginTextureMode(_layers[sublayer + 1]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 20, 20),
                                    new Rectangle(
                                        -10 + middle.X + point.X,
                                        -10 + middle.Y + point.Y,
                                        20,
                                        20
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();
                        }
                    
                        Draw.DrawToEffectColor(
                            State.bigSignGradient,
                            new Rectangle(0, 0, 60, 60),
                            new Rectangle(middle.X -13, middle.Y - 13, 26, 26),
                            _gradientA,
                            sublayer,
                            1,
                            1
                        );
                    }

                    UnloadRenderTexture(texture);
                }
                break;
            
                case "Small Asian Sign Station B": 
                case "Small Asian Sign On Wall Station B":
                {
                    var texture = LoadRenderTexture(20, 20);
                    var rnd = Random.Generate(14);

                    Rectangle dest = new(
                        0,
                        1,
                        20,
                        17
                    );

                    BeginTextureMode(texture);
                    {
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.smallAsianSignsStation);
                        DrawTexturePro(
                            State.smallAsianSignsStation,
                            dest,
                            new Rectangle(
                                (rnd - 1) * 20,
                                0,
                                20,
                                17
                            ),
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    ReadOnlySpan<(Vector2, Color)> list = [
                        (new(-4, -4), Color.Blue),
                        (new(-3, -3), Color.Blue),
                        (new( 3,  3), Color.Red),
                        (new( 4,  4), Color.Red),
                        (new(-2, -2), Color.Green),
                        (new(-1, -1), Color.Green),
                        (new( 0,  0), Color.Green),
                        (new( 1,  1), Color.Green),
                        (new( 2,  2), Color.Green),
                        (new( 0,  0), new Color(255, 0, 255, 255)),
                    ];

                    if (tag == "Small Asian Sign Station")
                    {
                        var middle = Utils.GetMiddleCellPos(x, y);

                        BeginTextureMode(rt);
                        var shader = Shaders.WhiteRemoverApplyColor;

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);

                        foreach (var (point, color) in list)
                        {
                            DrawTexturePro(
                                texture.Texture,
                                new Rectangle(0, 0, 20, 20),
                                new Rectangle(
                                    -10 + middle.X + point.X,
                                    -10 + middle.Y + point.Y,
                                    20,
                                    20
                                ),
                                Vector2.Zero,
                                0,
                                color
                            );
                        }

                        EndShaderMode();

                        EndTextureMode();

                        Draw.DrawToEffectColor(
                            State.bigSignGradient,
                            new Rectangle(0, 0, 60, 60),
                            new Rectangle(-13, -13, 26, 26),
                            _gradientB,
                            layer * 10,
                            1
                        );
                    }
                    else
                    {
                        var sublayer = layer * 10 + 8;
                        var middle = Utils.GetMiddleCellPos(x, y);

                        var shader = Shaders.WhiteRemoverApplyColor;

                        foreach (var (point, color) in list)
                        {
                            BeginTextureMode(_layers[sublayer]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 20, 20),
                                    new Rectangle(
                                        -10 + middle.X + point.X,
                                        -10 + middle.Y + point.Y,
                                        20,
                                        20
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();

                            BeginTextureMode(_layers[sublayer + 1]);
                            {
                                BeginShaderMode(shader);
                                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                                DrawTexturePro(
                                    texture.Texture,
                                    new Rectangle(0, 0, 20, 20),
                                    new Rectangle(
                                        -10 + middle.X + point.X,
                                        -10 + middle.Y + point.Y,
                                        20,
                                        20
                                    ),
                                    Vector2.Zero,
                                    0,
                                    color
                                );
                                EndShaderMode();
                            }
                            EndTextureMode();
                        }
                    
                        Draw.DrawToEffectColor(
                            State.bigSignGradient,
                            new Rectangle(0, 0, 60, 60),
                            new Rectangle(middle.X -13, middle.Y - 13, 26, 26),
                            _gradientB,
                            sublayer,
                            1,
                            1
                        );
                    }

                    UnloadRenderTexture(texture);
                }
                break;
            
                // deprecated?
                case "glass":
                if (layer == 0) {
                    var middle = Utils.GetMiddleCellPos(x, y);

                    Rectangle dest = new(
                        width * -10 + middle.X,
                        height * -10 + middle.Y,
                        20,
                        20
                    );

                    // Unknown behaviour..
                    // (An image gets modified and then never used again)
                }
                break;
            
                case "harvester":
                {
                    var middle = Utils.GetMiddleCellPos(x, y);

                    var big = tile.Name == "Harvester B";

                    char letter;
                    Vector2 eye, arm, lowerPart = Vector2.Zero;

                    if (big)
                    {
                        letter = 'B';
                        middle.X += 10;
                        eye = new(75, -126);
                        arm = new(105, 108);
                    }
                    else
                    {
                        letter = 'A';
                        middle.X += 10;
                        eye = new(37, -85);
                        arm = new(58, 60);
                    }

                    var absX = x + (int)(camera.Coords.X / 20);
                    var absY = y + (int)(camera.Coords.Y / 20);
                                        
                    for (var h = absY; h < Level!.Height; h++)
                    {
                        if ((letter == 'A' && Level!.TileMatrix[h, absX, layer] is { Type: TileCellType.Head, TileDefinition.Name: "Harvester Arm A" }) ||
                            (letter == 'B' && Level!.TileMatrix[h, absX, layer] is { Type: TileCellType.Head, TileDefinition.Name: "Harvester Arm B" }))
                        {
                            if (Level!.TileMatrix[h, absX, layer] is { Type: TileCellType.Head, TileDefinition.Name: "Harvester Arm A" })
                            {
                                lowerPart = new Vector2(x, h - camera.Coords.Y/20f);
                            }
                        }
                    }

                    var lowerMiddle = Vector2.Zero;
                    if (lowerPart != Vector2.Zero)
                    {
                        lowerMiddle = Utils.GetMiddleCellPos(lowerPart);
                        if (big) lowerMiddle.X += 10;
                    }

                    for (var side = 1; side <= 2; side++)
                    {
                        var dr = side == 1 ? -1 : 1;

                        var eyePastePos = middle + new Vector2(eye.X * dr, eye.Y);
                        
                        var eyeMember = letter == 'A' ? State.HarvesterAEye : State.HarvesterBEye;
                        
                        var quad = Utils.RotateRect(new Rectangle(
                                eyePastePos.X - eyeMember.Width / 2,
                                eyePastePos.Y = eyeMember.Height / 2,
                                eyeMember.Width,
                                eyeMember.Height
                            ),
                            Random.Generate(360)
                        );


                        var shader = Shaders.WhiteRemoverApplyColor;

                        for (var depth = layer * 10 +3; depth <= layer * 10 + 6; depth++)
                        {
                            BeginTextureMode(_layers[depth]);
                            BeginShaderMode(shader);
                            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), eyeMember);
                            Draw.DrawQuad(
                                eyeMember,
                                quad,
                                Color.Green
                            );
                            EndShaderMode();
                            EndTextureMode();
                        }
                    }
                }
                break;
            
                // incomplete
                case "Temple Floor":
                if (State.TempleStoneWedge is not null &&
                    State.TempleStoneSlopeSW is not null &&
                    State.TempleStoneSlopeSE is not null) {
                    var absX = x + (int)(camera.Coords.X / 20);
                    var absY = y + (int)(camera.Coords.Y / 20);
                
                    var nextIsFloor = false;
                    
                    if (absY + 8 < Level!.Height && 
                        Level!.TileMatrix[absY + 8, absX, layer] is { Type: TileCellType.Head, TileDefinition.Name: "Temple Floor" })
                    {
                        nextIsFloor = true;
                    }

                    var prevIsFloor = false;


                    if (absY - 8 >= 0 && 
                        Level!.TileMatrix[absY + 8, absX, layer] is { Type: TileCellType.Head, TileDefinition.Name: "Temple Floor" })
                    {
                        prevIsFloor = true;
                    }

                    BeginTextureMode(rt);
                    if (prevIsFloor)
                    {
                        DrawTile_MTX(new(), State.TempleStoneWedge, x + (int)camera.Coords.X - 4, y + (int)camera.Coords.Y - 1, layer, camera, rt);
                    }
                    else
                    {
                        DrawTile_MTX(new(), State.TempleStoneSlopeSE, x + (int)camera.Coords.X - 3, y + (int)camera.Coords.Y - 1, layer, camera, rt);
                    }

                    if (!nextIsFloor)
                    {
                        DrawTile_MTX(new(), State.TempleStoneSlopeSW, x + (int)camera.Coords.X + 4, y + (int)camera.Coords.Y - 1, layer, camera, rt);
                    }
                    EndTextureMode();
                }
                break;
            
                case "Larger Sign":
                case "Larger Sign B":
                {
                    var texture = LoadRenderTexture(86, 106);
                    var rnd = Random.Generate(14);
                    var dest = new Rectangle(3, 3, 80, 100);
                    var shader = Shaders.WhiteRemoverApplyColor;

                    BeginTextureMode(texture);
                    {
                        ClearBackground(Color.White);

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.largerSigns);

                        DrawTexturePro(
                            State.largerSigns,
                            new Rectangle(
                                (rnd - 1) * 80,
                                0,
                                80,
                                100
                            ),
                            dest,
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    var sublayer = layer * 10;

                    var middle = Utils.GetMiddleCellPos(x, y);

                    middle.X += 10;

                    ReadOnlySpan<(Vector2, Color)> list = [
                        (new(-4, -4), Color.Blue),
                        (new(-3, -3), Color.Blue),
                        (new( 3,  3), Color.Red),
                        (new( 4,  4), Color.Red),
                        (new(-2, -2), Color.Green),
                        (new(-1, -1), Color.Green),
                        (new( 0,  0), Color.Green),
                        (new( 1,  1), Color.Green),
                        (new( 2,  2), Color.Green),
                        (new( 2,  2), Color.Green),
                    ];

                    foreach (var (point, color) in list)
                    {
                        BeginTextureMode(_layers[sublayer]);
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                        DrawTextureV(
                            texture.Texture,
                            middle + point - new Vector2(43, 53),
                            color
                        );
                        EndShaderMode();
                        EndTextureMode();

                        BeginTextureMode(_layers[sublayer + 1]);
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                        DrawTextureV(
                            texture.Texture,
                            middle + point - new Vector2(43, 53),
                            color
                        );
                        EndShaderMode();
                        EndTextureMode();
                    }

                    BeginTextureMode(_layers[sublayer]);
                    BeginShaderMode(shader);
                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                    DrawTextureV(
                        texture.Texture,
                        middle - new Vector2(43, 53),
                        Color.White
                    );
                    EndShaderMode();
                    EndTextureMode();

                    BeginTextureMode(_layers[sublayer + 1]);
                    BeginShaderMode(shader);
                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                    DrawTextureV(
                        texture.Texture,
                        middle - new Vector2(43, 53),
                        new Color(255, 0, 255, 255)
                    );
                    EndShaderMode();
                    EndTextureMode();

                    BeginTextureMode(State.largeSignGrad2);
                    {
                        DrawTexture(State.largeSignGrad, 0, 0, Color.White);
                    }
                    EndTextureMode();

                    BeginTextureMode(State.largeSignGrad2);
                    for (var a = 0; a <= 6; a++)
                    {
                        for (var b = 0; b <= 13; b++)
                        {
                            Rectangle rect = new(
                                a * 16 - 6,
                                b * 8 - 1,
                                16,
                                8
                            );

                            if (Random.Generate(7) == 1)
                            {
                                var blend = Random.Generate(Random.Generate(100));

                                DrawRectangleRec(
                                    rect with {
                                        Width = rect.Width + 1,
                                        Height = rect.Height + 1
                                    },
                                    Color.White with { A = (byte)(blend/2) }
                                );

                                DrawRectangleRec(
                                    rect with {
                                        X = rect.X + 1,
                                        Y = rect.Y + 1,
                                        Width = rect.Width - 1,
                                        Height = rect.Height - 1
                                    },
                                    Color.White with { A = (byte)(blend/2) }
                                );
                            }
                            else if (Random.Generate(7) == 1)
                            {
                                DrawRectangleRec(
                                    rect with {
                                        X = rect.X + 1,
                                        Y = rect.Y + 1,
                                        Width = rect.Width - 1,
                                        Height = rect.Height - 1
                                    },
                                    Color.Black with { A = (byte)Random.Generate(Random.Generate(60)) }
                                );
                            }

                            DrawRectangleRec(
                                rect with { Height = 1 },
                                Color.White with { A = 20 }
                            );

                            DrawRectangleRec(
                                rect with {
                                    Y = rect.Y + 1, 
                                    Width = 1 
                                },
                                Color.White with { A = 20 }
                            );

                        }
                    }
                    EndTextureMode();
                
                    Draw.DrawToEffectColor(
                        State.largeSignGrad2.Texture, 
                        new Rectangle(0, 0, 86, 106),
                        new Rectangle(
                            -43 + middle.X,
                            -53 + middle.Y,
                            86,
                            106
                        ),
                        tag == "Larger Sign B" ? _gradientB : _gradientA,
                        sublayer + 1,
                        1,
                        1
                    );

                    UnloadRenderTexture(texture);
                }
                break;
            
                case "Station Larger Sign":
                case "Station Larger Sign B":
                {
                    var texture = LoadRenderTexture(86, 106);
                    var rnd = Random.Generate(14);
                    var dest = new Rectangle(3, 3, 80, 100);
                    var shader = Shaders.WhiteRemoverApplyColor;

                    BeginTextureMode(texture);
                    {
                        ClearBackground(Color.White);

                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.largerSignsStation);

                        DrawTexturePro(
                            State.largerSignsStation,
                            new Rectangle(
                                (rnd - 1) * 80,
                                0,
                                80,
                                100
                            ),
                            dest,
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    var sublayer = layer * 10;

                    var middle = Utils.GetMiddleCellPos(x, y);

                    middle.X += 10;

                    ReadOnlySpan<(Vector2, Color)> list = [
                        (new(-4, -4), Color.Blue),
                        (new(-3, -3), Color.Blue),
                        (new( 3,  3), Color.Red),
                        (new( 4,  4), Color.Red),
                        (new(-2, -2), Color.Green),
                        (new(-1, -1), Color.Green),
                        (new( 0,  0), Color.Green),
                        (new( 1,  1), Color.Green),
                        (new( 2,  2), Color.Green),
                        (new( 2,  2), Color.Green),
                    ];

                    foreach (var (point, color) in list)
                    {
                        BeginTextureMode(_layers[sublayer]);
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                        DrawTextureV(
                            texture.Texture,
                            middle + point - new Vector2(43, 53),
                            color
                        );
                        EndShaderMode();
                        EndTextureMode();

                        BeginTextureMode(_layers[sublayer + 1]);
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                        DrawTextureV(
                            texture.Texture,
                            middle + point - new Vector2(43, 53),
                            color
                        );
                        EndShaderMode();
                        EndTextureMode();
                    }

                    BeginTextureMode(_layers[sublayer]);
                    BeginShaderMode(shader);
                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                    DrawTextureV(
                        texture.Texture,
                        middle - new Vector2(43, 53),
                        Color.White
                    );
                    EndShaderMode();
                    EndTextureMode();

                    BeginTextureMode(_layers[sublayer + 1]);
                    BeginShaderMode(shader);
                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                    DrawTextureV(
                        texture.Texture,
                        middle - new Vector2(43, 53),
                        new Color(255, 0, 255, 255)
                    );
                    EndShaderMode();
                    EndTextureMode();

                    BeginTextureMode(State.largeSignGrad2);
                    {
                        DrawTexture(State.largeSignGrad, 0, 0, Color.White);
                    }
                    EndTextureMode();

                    BeginTextureMode(State.largeSignGrad2);
                    for (var a = 0; a <= 6; a++)
                    {
                        for (var b = 0; b <= 13; b++)
                        {
                            Rectangle rect = new(
                                a * 16 - 6,
                                b * 8 - 1,
                                16,
                                8
                            );

                            if (Random.Generate(7) == 1)
                            {
                                var blend = Random.Generate(Random.Generate(100));

                                DrawRectangleRec(
                                    rect with {
                                        Width = rect.Width + 1,
                                        Height = rect.Height + 1
                                    },
                                    Color.White with { A = (byte)(blend/2) }
                                );

                                DrawRectangleRec(
                                    rect with {
                                        X = rect.X + 1,
                                        Y = rect.Y + 1,
                                        Width = rect.Width - 1,
                                        Height = rect.Height - 1
                                    },
                                    Color.White with { A = (byte)(blend/2) }
                                );
                            }
                            else if (Random.Generate(7) == 1)
                            {
                                DrawRectangleRec(
                                    rect with {
                                        X = rect.X + 1,
                                        Y = rect.Y + 1,
                                        Width = rect.Width - 1,
                                        Height = rect.Height - 1
                                    },
                                    Color.Black with { A = (byte)Random.Generate(Random.Generate(60)) }
                                );
                            }

                            DrawRectangleRec(
                                rect with { Height = 1 },
                                Color.White with { A = 20 }
                            );

                            DrawRectangleRec(
                                rect with {
                                    Y = rect.Y + 1, 
                                    Width = 1 
                                },
                                Color.White with { A = 20 }
                            );

                        }
                    }
                    EndTextureMode();
                
                    Draw.DrawToEffectColor(
                        State.largeSignGrad2.Texture, 
                        new Rectangle(0, 0, 86, 106),
                        new Rectangle(
                            -43 + middle.X,
                            -53 + middle.Y,
                            86,
                            106
                        ),
                        tag == "Station Larger Sign B" ? _gradientB : _gradientA,
                        sublayer + 1,
                        1,
                        1
                    );

                    UnloadRenderTexture(texture);
                }
                break;
            
                case "Station Lamp":
                {
                    var texture = LoadRenderTexture(40, 20);
                    var rnd = Random.Generate(1);
                    var rect = new Rectangle(1, 1, 38, 18);
                    var middle = Utils.GetMiddleCellPos(x, y);

                    middle.X += 11;
                    middle.Y += 1;
                    
                    var shader = Shaders.WhiteRemoverApplyColor;

                    BeginTextureMode(texture);
                    {
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.StationLamp);
                        DrawTexturePro(
                            State.StationLamp,
                            new Rectangle(
                                (rnd - 1) * 40, 
                                0, 
                                40, 
                                20
                            ),
                            new Rectangle(0, 0, texture.Texture.Width, texture.Texture.Height),
                            Vector2.Zero,
                            0,
                            Color.Black
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    BeginTextureMode(rt);
                    {
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture.Texture);
                        DrawTexturePro(
                            texture.Texture,
                            new Rectangle(),
                            new Rectangle(
                                -20 + middle.X,
                                -10 + middle.Y,
                                40,
                                20
                            ),
                            Vector2.Zero,
                            0,
                            new Color(255, 0, 255, 255)
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    var sublayer = layer * 10 + 1;

                    Draw.DrawToEffectColor(
                        State.StationLampGradient,
                        new Rectangle(0, 0, 40, 20),
                        new Rectangle(
                            middle.X - 20, 
                            middle.Y - 10, 
                            40, 
                            20
                        ),
                        _gradientA,
                        sublayer,
                        1
                    );

                    UnloadRenderTexture(texture);
                }
                break;
            
                case "LumiaireH":
                {
                    var sublayer = layer * 10 + 7;
                    var middle = Utils.GetMiddleCellPos(x, y);
                    var shader = Shaders.WhiteRemoverApplyColor;

                    var dest = new Rectangle(
                        -29 + middle.X + 10,
                        -11 + middle.Y + 10,
                        58,
                        22
                    );

                    BeginTextureMode(_layers[sublayer]);
                    {
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.LumiaireH);
                        DrawTexturePro(
                            State.LumiaireH,
                            new Rectangle(0, 0, State.LumiaireH.Width, State.LumiaireH.Height),
                            dest,
                            Vector2.Zero,
                            0,
                            new Color(255, 0, 255, 255)
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    BeginTextureMode(_gradientA[sublayer]);
                    {
                        Draw.DrawTextureDarkest(
                            State.LumHGrad,
                            new Rectangle(0, 0, State.LumHGrad.Width, State.LumHGrad.Height),
                            dest
                        );
                    }
                    EndTextureMode();
                }
                break;
            
                case "LumiaireV":
                {
                    var sublayer = layer * 10 + 7;
                    var middle = Utils.GetMiddleCellPos(x, y);
                    var shader = Shaders.WhiteRemoverApplyColor;

                    var dest = new Rectangle(
                        -11 + middle.X + 10,
                        -29 + middle.Y + 10,
                        22,
                        58
                    );

                    BeginTextureMode(_layers[sublayer]);
                    {
                        BeginShaderMode(shader);
                        SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), State.LumiaireV);
                        DrawTexturePro(
                            State.LumiaireV,
                            new Rectangle(0, 0, State.LumiaireV.Width, State.LumiaireV.Height),
                            dest,
                            Vector2.Zero,
                            0,
                            new Color(255, 0, 255, 255)
                        );
                        EndShaderMode();
                    }
                    EndTextureMode();

                    BeginTextureMode(_gradientA[sublayer]);
                    {
                        Draw.DrawTextureDarkest(
                            State.LumVGrad,
                            new Rectangle(0, 0, State.LumVGrad.Width, State.LumVGrad.Height),
                            dest
                        );
                    }
                    EndTextureMode();
                }
                break;
            }
        }
    }
}