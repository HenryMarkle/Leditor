using Leditor.Data;
using Leditor.Data.Materials;

using static Raylib_cs.Raylib;

namespace Leditor.Renderer;

public static class Utils
{
    public static int Restrict(int value, int min, int max)
    {
        if (value < min) value = min;
        if (value > max) value = max;

        return value;
    }

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
}