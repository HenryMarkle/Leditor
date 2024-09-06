using System.Numerics;
using System.Runtime.CompilerServices;
using Leditor.Data;
using Leditor.Data.Geometry;
using Leditor.Data.Materials;
using Leditor.Data.Props.Legacy;
using Leditor.Data.Tiles;
using Raylib_cs;

using Color = Raylib_cs.Color;

namespace Leditor.Renderer;

public static class Utils
{
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 GetMiddleCellPos(Vector2 pos) => new Vector2(pos.X, pos.Y) * 20 - Vector2.One * 10;

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 GetMiddleCellPos(int x, int y) => new Vector2(x, y)  * 20 - Vector2.One * 10;

    public static GeoType GetGeoCellType(Geo[,,] matrix, int x, int y, int z) => (Data.Utils.InBounds(matrix, x, y) && z < 3 && z >= 0) ? matrix[y, x, z].Type : GeoType.Solid;

    public static float Diag(Vector2 p1, Vector2 p2)
    {
        var rectHeight = Math.Abs(p1.Y - p2.Y);
        var rectWidth = Math.Abs(p1.X - p2.X);
    
        return (float) Math.Sqrt(rectHeight * rectHeight + rectWidth * rectWidth);
    }

    public static Vector2 MoveToPoint(Vector2 p1, Vector2 p2, float move)
    {
        var p3 = p2 - p1;
        var diag = Diag(Vector2.Zero, p3);
    
        Vector2 dirVec;

        if (diag > 0)
        {
            dirVec = p3 / diag;
        }
        else
        {
            dirVec = new Vector2(0, 1);
        }

        return dirVec * move;
    }

    public static float LookAtPoint(Vector2 p, Vector2 target)
    {
        var dy = target.Y - p.Y;
        var dx = p.X - target.X;

        float rotation;

        if (dx != 0)
        {
            rotation = (float) Math.Atan(dy / dx);
        }
        else
        {
            rotation = (float) (1.5f * Math.PI);
        }

        float fuckedUpAngleFix;

        if (target.Y > p.Y)
        {
            fuckedUpAngleFix = 0;
        }
        else
        {
            fuckedUpAngleFix = (float) Math.PI;
        }

        rotation = fuckedUpAngleFix - rotation;

        return rotation * 180 / (float)Math.PI + 90;
    }

    public static Vector2 DegToVec(float degree)
    {
        degree += 90;
        degree *= -1;
        
        var rad = degree / 100 * (float)Math.PI * 2;

        return new Vector2((float)-Math.Cos(rad), (float)Math.Sin(rad));
    }

    public static Vector2 GiveDirFor90degrToLine(Vector2 p1, Vector2 p2)
    {
        var x1 = p1.X;
        var y1 = p1.Y;

        var x2 = p2.X;
        var y2 = p2.Y;

        var dy = y1 - y2;
        var dx = x1 - x2;

        float dir, newDir;

        if (dx != 0)
        {
            dir = dy / dx;
        }
        else
        {
            dir = 1;
        }

        if (dir != 0)
        {
            newDir = -1f / dir;
        }
        else
        {
            newDir = 1;
        }

        Vector2 newPoint = new(1, newDir);

        int fac = 1;

        if (x2 < x1)
        {
            if (y2 < y1)
            {
                fac = 1;
            }
            else
            {
                fac = -1;
            }
        }
        else
        {
            if (y2 < y1)
            {
                fac = 1;
            }
            else
            {
                fac = -1;
            }
        }

        newPoint *= fac;

        newPoint /= Diag(Vector2.Zero, newPoint);

        return newPoint;
    }

    public static Quad RotateRect(Rectangle rect, float degree)
    {
        var dir = DegToVec(degree);

        var midPoint = new Vector2((rect.X + rect.Width)/2f, (rect.Y + rect.Height)/2f);
        var topPoint = midPoint + dir * rect.Height / 2f;
        var botPoint = midPoint - dir * rect.Height / 2f;

        var crossDir = GiveDirFor90degrToLine(-dir, dir);

        var point1 = topPoint + crossDir * rect.Width / 2f;
        var point2 = topPoint - crossDir * rect.Width / 2f;
        var point3 = botPoint - crossDir * rect.Width / 2f;
        var point4 = botPoint + crossDir * rect.Width / 2f;
    
        return new(
            point2, // top left 
            point1, // top right
            point4, // bottom right
            point3  // bottom left
        );
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Restrict(int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Cycle(int value, int min, int max)
    {
        if (value < min) return max;
        if (value > max) return min;

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BoolInt(bool value) => value ? 1 : 0;

    public static MaterialDefinition[] GetEmbeddedMaterials()
    {
        return [
            new( "Standard",         new(150, 150, 150, 255), MaterialRenderType.Unified    ),
            new( "Concrete",         new(150, 255, 255, 255), MaterialRenderType.Unified    ),
            new( "RainStone",        new(  0,   0, 255, 255), MaterialRenderType.Unified    ),
            new( "Bricks",           new(200, 150, 100, 255), MaterialRenderType.Unified    ),
            new( "BigMetal",         new(255,   0,   0, 255), MaterialRenderType.Unified    ),
            new( "Tiny Signs",       new(255, 255, 255, 255), MaterialRenderType.Unified    ),
            new( "Scaffolding",      new( 60,  60,  40, 255), MaterialRenderType.Unified    ),
            new( "Dense Pipes",      new( 10,  10, 255, 255), MaterialRenderType.DensePipe  ),
            new( "SuperStructure",   new(160, 180, 255, 255), MaterialRenderType.Unified    ),
            new( "SuperStructure2",  new(190, 160,   0, 255), MaterialRenderType.Unified    ),
            new( "Tiled Stone",      new(100,   0, 255, 255), MaterialRenderType.Tiles      ),
            new( "Chaotic Stone",    new(255,   0, 255, 255), MaterialRenderType.Tiles      ),
            new( "Small Pipes",      new(255, 255,   0, 255), MaterialRenderType.Pipe       ),
            new( "Trash",            new( 90, 255,   0, 255), MaterialRenderType.Pipe       ),
            new( "Invisible",        new(200, 200, 200, 255), MaterialRenderType.Invisible  ),
            new( "LargeTrash",       new(150,  30, 255, 255), MaterialRenderType.LargeTrash ),
            new( "3DBricks",         new(255, 150,   0, 255), MaterialRenderType.Tiles      ),
            new( "Random Machines",  new( 72, 116,  80, 255), MaterialRenderType.Tiles      ),
            new( "Dirt",             new(124,  72,  52, 255), MaterialRenderType.Dirt       ),
            new( "Ceramic Tile",     new( 60,  60, 100, 255), MaterialRenderType.Ceramic    ),
            new( "Temple Stone",     new(  0, 120, 180, 255), MaterialRenderType.Tiles      ),
            new( "Circuits",         new( 15, 200,  15, 255), MaterialRenderType.DensePipe  ),
            new( "Ridge",            new(200,  15,  60, 255), MaterialRenderType.Ridge      ),

            new( "Steel",                new(220, 170, 195, 255), MaterialRenderType.Unified     ),
            new( "4Mosaic",              new(227,  76,  13, 255), MaterialRenderType.Tiles       ),
            new( "Color A Ceramic",      new(120,  0,   90, 255), MaterialRenderType.CeramicA    ),
            new( "Color B Ceramic",      new(  0, 175, 175, 255), MaterialRenderType.CeramicB    ),
            new( "Random Pipes",         new( 80,   0, 140, 255), MaterialRenderType.RandomPipes ),
            new( "Rocks",                new(185, 200,   0, 255), MaterialRenderType.Rock        ),
            new( "Rough Rock",           new(155, 170,   0, 255), MaterialRenderType.RoughRock   ),
            new( "Random Metal",         new(180,  10,  10, 255), MaterialRenderType.Tiles       ),
            new( "Cliff",                new( 75,  75,  75, 255), MaterialRenderType.Tiles       ),
            new( "Non-Slip Metal",       new(180,  80,  80, 255), MaterialRenderType.Unified     ),
            new( "Stained Glass",        new(180,  80, 180, 255), MaterialRenderType.Unified     ),
            new( "Sandy Dirt",           new(180, 180,  80, 255), MaterialRenderType.Sandy       ),
            new( "MegaTrash",            new(135,  10, 255, 255), MaterialRenderType.Unified     ),
            new( "Shallow Dense Pipes",  new( 13,  23, 110, 255), MaterialRenderType.DensePipe   ),
            new( "Sheet Metal",          new(145, 135, 125, 255), MaterialRenderType.WV          ),
            new( "Chaotic Stone 2",      new( 90,  90,  90, 255), MaterialRenderType.Tiles       ),
            new( "Asphalt",              new(115, 115, 115, 255), MaterialRenderType.Unified     ),

            new( "Shallow Circuits",     new( 15, 200, 155, 255), MaterialRenderType.DensePipe ),
            new( "Random Machines 2",    new(116, 116,  80, 255), MaterialRenderType.Tiles     ),
            new( "Small Machines",       new( 80, 116, 116, 255), MaterialRenderType.Tiles     ),
            new( "Random Metals",        new(255,   0,  80, 255), MaterialRenderType.Tiles     ),
            new( "ElectricMetal",        new(255,   0, 100, 255), MaterialRenderType.Unified   ),
            new( "Grate",                new(190,  50, 190, 255), MaterialRenderType.Unified   ),
            new( "CageGrate",            new( 50, 190, 190, 255), MaterialRenderType.Unified   ),
            new( "BulkMetal",            new( 50,  19, 190, 255), MaterialRenderType.Unified   ),
            new( "MassiveBulkMetal",     new(255,  19,  19, 255), MaterialRenderType.Unified   ),
            new( "Dune Sand",            new(255, 255, 100, 255), MaterialRenderType.Tiles     ),
        ];
    }

    public static InitLongProp[] GetEmbeddedLongProps()
    {
        return [
            new( "Cabinet Clamp",   InitPropType_Legacy.Long,  0 ),
            new( "Drill Suspender", InitPropType_Legacy.Long,  5 ),
            new( "Thick Chain",     InitPropType_Legacy.Long,  0 ),
            new( "Drill",           InitPropType_Legacy.Long, 10 ),
            new( "Piston",          InitPropType_Legacy.Long,  4 ),
             
            new( "Stretched Pipe", InitPropType_Legacy.Long, 0 ),
            new( "Twisted Thread", InitPropType_Legacy.Long, 0 ),
            new( "Stretched Wire", InitPropType_Legacy.Long, 0 ),     
        ];
    }

    public static InitRopeProp[] GetEmbeddedRopeProps()
    {
        return [
            new(name: "Wire", type: InitPropType_Legacy.Rope, depth: 0, segmentLength: 3, collisionDepth: 0, segmentRadius: 1f, gravity: 0.5f, friction: 0.5f, airFriction: 0.9f, stiff: false, previewColor: new(255, 0, 0, 255), previewEvery: 4, edgeDirection: 0, rigid: 0, selfPush: 0, sourcePush: 0),
            new("Tube", InitPropType_Legacy.Rope, 4, 10, 2, 4.5f, 0.5f, 0.5f, 0.9f, true, new(0, 0, 255, 255), 2, 5, 1.6f, 0, 0),
            new("ThickWire", InitPropType_Legacy.Rope, 3, 4, 1, 2f, 0.5f, 0.8f, 0.9f, true, new(255, 255, 0, 255), 2, 0, 0.2f, 0, 0),
            new("RidgedTube", InitPropType_Legacy.Rope, 4, 5, 2, 5, 0.5f, 0.3f, 0.7f, true, new Color(255, 0, 255, 255), 2, 0, 0.1f, 0, 0),
            new("Fuel Hose", InitPropType_Legacy.Rope, 5, 16, 1, 7, 0.5f, 0.8f, 0.9f, true, new(255, 150, 0, 255), 1, 1.4f, 0.2f, 0, 0),
            new("Broken Fuel Hose", InitPropType_Legacy.Rope, 6, 16, 1, 7, 0.5f, 0.8f, 0.9f, true, new(255, 150, 0, 255), 1, 1.4f, 0.2f, 0, 0),
            new("Large Chain", InitPropType_Legacy.Rope, 9, 28, 3, 9.5f, 0.9f, 0.8f, 0.95f, true, new(0, 255, 0, 255), 1, 0, 0, 6.5f, 0),
            new("Large Chain 2", InitPropType_Legacy.Rope, 9, 28, 3, 9.5f, 0.9f, 0.8f, 0.95f, true, new(20, 205, 0, 255), 1, 0, 0, 6.5f, 0),
            new("Bike Chain", InitPropType_Legacy.Rope, 9, 38, 3, 6.5f, 0.9f, 0.8f, 0.95f, true, new(100, 100, 100, 255), 1, 0, 0, 16.5f, 0),
            new("Zero-G Tube", InitPropType_Legacy.Rope, 4, 10, 2, 4.5f, 0, 0.5f, 0.9f, true, new(0, 255, 0, 255), 2, 0, 0.6f, 2, 0.5f),
            new("Zero-G Wire", InitPropType_Legacy.Rope, 0, 8, 0, 1, 0, 0.5f, 0.9f, true, new(255, 0, 0, 255), 2, 0.3f, 0.5f, 1.2f, 0.5f),
            new("Fat Hose", InitPropType_Legacy.Rope, 6, 40, 3, 20, 0.9f, 0.6f, 0.95f, true, new(0, 100, 255, 255), 1, 0.1f, 0.2f, 10, 0.1f),
            new("Wire Bunch", InitPropType_Legacy.Rope, 9, 50, 3, 20, 0.9f, 0.6f, 0.95f, true, new(255, 100, 150, 255), 1, 0.1f, 0.2f, 10, 0.1f),
            new("Wire Bunch 2", InitPropType_Legacy.Rope, 9, 50, 3, 20, 0.9f, 0.6f, 0.95f, true, new(255, 100, 150, 255), 1, 0.1f, 0.2f, 10, 0.1f),
            new("Big Big Pipe", InitPropType_Legacy.Rope, 6, 40, 3, 20, 0.9f, 0.6f, 0.95f, true, new(50, 150, 210, 255), 1, 0.1f, 0.2f, 10, 0.1f),
            new("Ring Chain", InitPropType_Legacy.Rope, 6, 40, 3, 20, 0.9f, 0.6f, 0.95f, true, new(100, 200, 0, 255), 1, 0.1f, 0.2f, 10, 0.1f),
            new("Christmas Wire", InitPropType_Legacy.Rope, 0, 17, 0, 8.5f, 0.5f, 0.5f, 0.9f, false, new(200, 0, 200, 255), 1, 0, 0, 0, 0),
            new("Ornate Wire", InitPropType_Legacy.Rope, 0, 17, 0, 8.5f, 0.5f, 0.5f, 0.9f, false, new(0, 200, 200, 255), 1, 0, 0, 0, 0),
        ];
    }

    public class MeteredTask
    {
        public Task? Task { get; set; }

        public int TotalProgress { get; set; }
        public int Progress { get; set; }

        public static implicit operator Task?(MeteredTask m) => m.Task;
    }

    public static Task FetchEmbeddedTextures(MaterialDefinition[] defs, Dictionary<string, CastLibrary> libs)
    {
        var tasks = defs.SelectMany((d, index) => 
            libs.Values.Select(l => 
                Task.Run(() => {
                    if (l.Members.TryGetValue(d.Name+"Texture", out var foundMember))
                    {
                        defs[index].Texture = foundMember.Texture;
                    }
                })
            )
        );

        return Task.WhenAll(tasks);
    }

    public static MeteredTask FetchEmbeddedTextures(TileDefinition[] defs, CastLibrary[] libs)
    {
        var metered = new MeteredTask()
        {
            TotalProgress = defs.Length
        };

        var tasks = defs.SelectMany((d, index) => 
            libs.Select(l => 
                Task.Run(() => {
                    if (l.Members.TryGetValue(d.Name, out var foundMember))
                    {
                        defs[index].Texture = foundMember.Texture;
                        metered.Progress++;
                    }
                })
            )
        );

        metered.Task = Task.WhenAll(tasks);
        return metered;
    }

    public static async Task<TileDefinition[]> LoadEmbeddedTiles(string initPath, Dictionary<string, CastLibrary> libs)
    {
        var embedded = await Serialization.TileImporter.ParseInitAsync_NoCategories(initPath);
    
        var tasks = embedded.SelectMany(d => 
            libs.Values.Select(l => 
                Task.Run(() => {
                    if (l.Members.TryGetValue(d.Name, out var foundMemeber))
                    {
                        d.Texture = foundMemeber.Texture;
                    }
                })
            )
        );

        await Task.WhenAll(tasks);

        return embedded;
    }
}
