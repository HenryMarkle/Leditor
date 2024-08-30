using System.Runtime.CompilerServices;
using Leditor.Data;
using Leditor.Data.Materials;
using Leditor.Data.Props.Legacy;
using static Raylib_cs.Raylib;

namespace Leditor.Renderer;

public static class Utils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Restrict(int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;

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
}