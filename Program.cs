global using Raylib_CsLo;
using static Raylib_CsLo.Raylib;

using System.Numerics;
using System.Text;
using Leditor.Lingo;
using Pidgin;
using System.Text.Json;
using Serilog;
using System.Security.Cryptography;

#nullable enable

namespace Leditor;

public interface IPage { void Draw(); }

class Program
{
    // Used to load geo blocks menu item textures.
    // Do not alter the indices, and do NOT call before InitWindow()
    static Texture[] LoadUITextures() => [
        LoadTexture("assets/geo/ui/solid.png"),             // 0
        LoadTexture("assets/geo/ui/air.png"),               // 1
        // LoadTexture("assets/geo/ui/slopebr.png"),     
        LoadTexture("assets/geo/ui/slopebl.png"),           // 2
        LoadTexture("assets/geo/ui/multisolid.png"),        // 3
        LoadTexture("assets/geo/ui/multiair.png"),          // 4
        // LoadTexture("assets/geo/ui/slopetr.png"),        
        // LoadTexture("assets/geo/ui/slopetl.png"),        
        LoadTexture("assets/geo/ui/platform.png"),          // 5
        LoadTexture("assets/geo/ui/move.png"),              // 6
        LoadTexture("assets/geo/ui/rock.png"),              // 7
        LoadTexture("assets/geo/ui/spear.png"),             // 8
        LoadTexture("assets/geo/ui/crack.png"),             // 9
        LoadTexture("assets/geo/ui/ph.png"),                // 10
        LoadTexture("assets/geo/ui/pv.png"),                // 11
        LoadTexture("assets/geo/ui/glass.png"),             // 12
        LoadTexture("assets/geo/ui/backcopy.png"),          // 13
        LoadTexture("assets/geo/ui/entry.png"),             // 14
        LoadTexture("assets/geo/ui/shortcut.png"),          // 15
        LoadTexture("assets/geo/ui/den.png"),               // 16
        LoadTexture("assets/geo/ui/passage.png"),           // 17
        LoadTexture("assets/geo/ui/bathive.png"),           // 18
        LoadTexture("assets/geo/ui/waterfall.png"),         // 19
        LoadTexture("assets/geo/ui/scav.png"),              // 20
        LoadTexture("assets/geo/ui/wack.png"),              // 21
        LoadTexture("assets/geo/ui/garbageworm.png"),       // 22
        LoadTexture("assets/geo/ui/worm.png"),              // 23
        LoadTexture("assets/geo/ui/forbidflychains.png"),   // 24
        LoadTexture("assets/geo/ui/clearall.png"),          // 25
        LoadTexture("assets/geo/ui/save-to-memory.png"),    // 26
        LoadTexture("assets/geo/ui/load-from-memory.png"),  // 27
    ];

    // Used to load geo block textures.
    // Do not alter the indices, and do NOT call before InitWindow()
    static Texture[] LoadGeoTextures() => [
        // 0: air
        LoadTexture("assets/geo/solid.png"),
        LoadTexture("assets/geo/cbl.png"),
        LoadTexture("assets/geo/cbr.png"),
        LoadTexture("assets/geo/ctl.png"),
        LoadTexture("assets/geo/ctr.png"),
        LoadTexture("assets/geo/platform.png"),
        LoadTexture("assets/geo/entryblock.png"),
        // 7: NONE
        // 8: NONE
        LoadTexture("assets/geo/thickglass.png"),
    ];


    // Used to load geo stackables textures.
    // And you guessed it: do not alter the indices, and do NOT call before InitWindow()
    static Texture[] LoadStackableTextures() => [
        LoadTexture("assets/geo/ph.png"),             // 0
        LoadTexture("assets/geo/pv.png"),             // 1
        LoadTexture("assets/geo/bathive.png"),        // 2
        LoadTexture("assets/geo/dot.png"),            // 3
        LoadTexture("assets/geo/crackbl.png"),        // 4
        LoadTexture("assets/geo/crackbr.png"),        // 5
        LoadTexture("assets/geo/crackc.png"),         // 6
        LoadTexture("assets/geo/crackh.png"),         // 7
        LoadTexture("assets/geo/cracklbr.png"),       // 8
        LoadTexture("assets/geo/cracktbr.png"),       // 9
        LoadTexture("assets/geo/cracktl.png"),        // 10
        LoadTexture("assets/geo/cracktlb.png"),       // 11
        LoadTexture("assets/geo/cracktlr.png"),       // 12
        LoadTexture("assets/geo/cracktr.png"),        // 13
        LoadTexture("assets/geo/cracku.png"),         // 14
        LoadTexture("assets/geo/crackv.png"),         // 15
        LoadTexture("assets/geo/garbageworm.png"),    // 16
        LoadTexture("assets/geo/scav.png"),           // 17
        LoadTexture("assets/geo/rock.png"),           // 18
        LoadTexture("assets/geo/waterfall.png"),      // 19
        LoadTexture("assets/geo/wack.png"),           // 20
        LoadTexture("assets/geo/worm.png"),           // 21
        LoadTexture("assets/geo/entryb.png"),         // 22
        LoadTexture("assets/geo/entryl.png"),         // 23
        LoadTexture("assets/geo/entryr.png"),         // 24
        LoadTexture("assets/geo/entryt.png"),         // 25
        LoadTexture("assets/geo/looseentry.png"),     // 26
        LoadTexture("assets/geo/passage.png"),        // 27
        LoadTexture("assets/geo/den.png"),            // 28
        LoadTexture("assets/geo/spear.png"),          // 29
        LoadTexture("assets/geo/forbidflychains.png"),// 30
        LoadTexture("assets/geo/crackb.png"),         // 31
        LoadTexture("assets/geo/crackr.png"),         // 32
        LoadTexture("assets/geo/crackt.png"),         // 33
        LoadTexture("assets/geo/crackl.png"),         // 34
    ];

    // Used to load light/shadow brush images as textures.
    private static Texture[] LoadLightTextures(Serilog.Core.Logger logger) => Directory
        .GetFileSystemEntries(GLOBALS.Paths.LightAssetsDirectory)
        .Where(e => e.EndsWith(".png"))
        .Select((e) =>
        {
            logger.Debug($"loading light texture \"{e}\""); 
            return LoadTexture(e); })
        .ToArray();

    // Embedded tiles and their categories.
    // They probably be should externalized and turned to normal tiles.
    // Maybe one day.

    static readonly (string, Color)[] embeddedCategories = [
        ("Drought 4Mosaic", new(227, 76, 13, 255)),
        ("Drought Missing 3DBricks", new(255, 150, 0, 255)),
        ("Drought Alt Grates", new(75, 75, 240, 255)),
        ("Drought Missing Stone", new(200, 165, 135, 255)),
        ("Drought Missing Machine", new(230, 160, 230, 255)),
        ("Drought Metal", new(100, 185, 245, 255)),
        ("Drought Missing Metal", new(180, 10, 10, 255)),
        ("Dune", new(255, 255, 180, 255)),
    ];

    static readonly InitTile[][] embeddedTiles = [
        [
            new InitTile("4Mosaic Square", (1, 1), [1], [], InitTileType.VoxelStruct, [1, 1, 8], 0, 1, 0, ["INTERNAL"]),
            new InitTile("4Mosaic Slope NE", (1, 1), [2], [], InitTileType.VoxelStruct, [1, 1, 8], 0, 1, 0, ["INTERNAL"]),
            new InitTile("4Mosaic Slope NW", (1, 1), [3], [], InitTileType.VoxelStruct, [1, 1, 8], 0, 1, 0, ["INTERNAL"]),
            new InitTile("4Mosaic Slope SW", (1, 1), [5], [], InitTileType.VoxelStruct, [1, 1, 8], 0, 1, 0, ["INTERNAL"]),
            new InitTile("4Mosaic Slope SE", (1, 1), [4], [], InitTileType.VoxelStruct, [1, 1, 8], 0, 1, 0, ["INTERNAL"]),
            new InitTile("4Mosaic Floor", (1, 1), [6], [], InitTileType.VoxelStruct, [1, 1, 8], 0, 1, 0, ["INTERNAL"]),
        ],
        [
            new InitTile("3DBrick Square", (1, 1), [1], [], InitTileType.VoxelStruct, [1, 1, 1, 7], 0, 1, 0, ["INTERNAL"]),
            new InitTile("3DBrick Slope NE", (1, 1), [2], [], InitTileType.VoxelStruct, [1, 1, 1, 7], 0, 1, 0, ["INTERNAL"]),
            new InitTile("3DBrick Slope NW", (1, 1), [3], [], InitTileType.VoxelStruct, [1, 1, 1, 7], 0, 1, 0, ["INTERNAL"]),
            new InitTile("3DBrick Slope SW", (1, 1), [5], [], InitTileType.VoxelStruct, [1, 1, 1, 7], 0, 1, 0, ["INTERNAL"]),
            new InitTile("3DBrick Slope SE", (1, 1), [4], [], InitTileType.VoxelStruct, [1, 1, 1, 7], 0, 1, 0, ["INTERNAL"]),
            new InitTile("3DBrick Floor", (1, 1), [6], [], InitTileType.VoxelStruct, [1, 1, 1, 7], 0, 1, 0, ["INTERNAL"]),
        ],
        [
            new InitTile("AltGrateA", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateB1", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateB2", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateB3", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateB4", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateC1", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateC2", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateE1", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateE2", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateF1", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateF2", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateF3", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateF4", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateG1", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateG2", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateH", (3, 4), [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),

            new InitTile("AltGrateI", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateJ1", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateJ2", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateJ3", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateJ4", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateK1", (2, 2), [0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateK2", (2, 2), [0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateK3", (2, 2), [0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateK4", (2, 2), [0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateL", (2, 2), [0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateM", (2, 2), [0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateN", (4, 4), [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGrateO", (5, 5), [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
        ],
        [
            new InitTile("Small Stone Slope NE", (1, 1), [2], [], InitTileType.VoxelStructRockType, [], 1, 4, 0, ["nonSolid", "INTERNAL"]),
            new InitTile("Small Stone Slope NW", (1, 1), [3], [], InitTileType.VoxelStructRockType, [], 1, 4, 0, ["nonSolid", "INTERNAL"]),
            new InitTile("Small Stone Slope SW", (1, 1), [5], [], InitTileType.VoxelStructRockType, [], 1, 4, 0, ["nonSolid", "INTERNAL"]),
            new InitTile("Small Stone Slope SE", (1, 1), [4], [], InitTileType.VoxelStructRockType, [], 1, 4, 0, ["nonSolid", "INTERNAL"]),
            new InitTile("Small Stone Floor", (1, 1), [6], [], InitTileType.VoxelStructRockType, [], 1, 4, 0, ["nonSolid", "INTERNAL"]),

            new InitTile("Small Stone Marked", (1, 1), [1], [], InitTileType.VoxelStructRockType, [], 1, 4, 0, ["nonSolid", "chaoticStone2 : very rare", "INTERNAL"]),
            new InitTile("Square Stone Marked", (2, 2), [1, 1, 1, 1], [], InitTileType.VoxelStructRockType, [], 1, 3, 0, ["chaoticStone2 : very rare", "INTERNAL"]),
        ],
        [
            new InitTile("Small Machine Slope NE", (1, 1), [2], [], InitTileType.VoxelStruct, [1, 1, 1, 1, 1, 1, 1, 1, 1, 1], 1, 1, 0, ["nonSolid", "INTERNAL"]),
            new InitTile("Small Machine Slope NW", (1, 1), [3], [], InitTileType.VoxelStruct, [1, 1, 1, 1, 1, 1, 1, 1, 1, 1], 1, 1, 0, ["nonSolid", "INTERNAL"]),
            new InitTile("Small Machine Slope SW", (1, 1), [5], [], InitTileType.VoxelStruct, [1, 1, 1, 1, 1, 1, 1, 1, 1, 1], 1, 1, 0, ["nonSolid", "INTERNAL"]),
            new InitTile("Small Machine Slope SE", (1, 1), [4], [], InitTileType.VoxelStruct, [1, 1, 1, 1, 1, 1, 1, 1, 1, 1], 1, 1, 0, ["nonSolid", "INTERNAL"]),
            new InitTile("Small Machine Floor", (1, 1), [6], [], InitTileType.VoxelStruct, [1, 1, 1, 1, 1, 1, 1, 1, 1, 1], 1, 1, 0, ["nonSolid", "INTERNAL"]),
        ],
        [
            new InitTile("Small Metal Alt", (1, 1), [1], [], InitTileType.Box, [], 0, 1, 0, ["randomMetal", "INTERNAL"]),
            new InitTile("Small Metal Marked", (1, 1), [1], [], InitTileType.Box, [], 0, 1, 0, ["randomMetal", "INTERNAL"]),
            new InitTile("Small Metal X", (1, 1), [1], [], InitTileType.Box, [], 0, 1, 0, ["randomMetal", "INTERNAL"]),

            new InitTile("Metal Floor Alt", (2, 1), [1, 1], [], InitTileType.Box, [], 0, 1, 0, ["randomMetal", "INTERNAL"]),

            new InitTile("Metal Wall", (1, 2), [1, 1], [], InitTileType.Box, [], 0, 1, 0, ["randomMetal", "INTERNAL"]),
            new InitTile("Metal Wall Alt", (1, 2), [1, 1], [], InitTileType.Box, [], 0, 1, 0, ["randomMetal", "INTERNAL"]),

            new InitTile("Square Metal Marked", (2, 2), [1, 1, 1, 1], [], InitTileType.Box, [], 0, 1, 0, ["randomMetal", "INTERNAL"]),
            new InitTile("Square Metal X", (2, 2), [1, 1, 1, 1], [], InitTileType.Box, [], 0, 1, 0, ["randomMetal", "INTERNAL"]),

            new InitTile("Wide Metal", (3, 2), [1, 1, 1, 1, 1, 1], [], InitTileType.Box, [], 1, 1, 0, ["randomMetal", "INTERNAL"]),

            new InitTile("Tall Metal", (2, 3), [1, 1, 1, 1, 1, 1], [], InitTileType.Box, [], 1, 1, 0, ["randomMetal", "INTERNAL"]),

            new InitTile("Big Metal X", (3, 3), [1, 1, 1, 1, 1, 1, 1, 1, 1], [], InitTileType.Box, [], 1, 1, 0, ["randomMetal", "INTERNAL"]),

            new InitTile("Large Big Metal", (4, 4), [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1], [], InitTileType.Box, [], 0, 1, 0, ["randomMetal", "INTERNAL"]),
            new InitTile("Large Big Metal Marked", (4, 4), [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1], [], InitTileType.Box, [], 0, 1, 0, ["randomMetal", "INTERNAL"]),
            new InitTile("Large Big Metal X", (4, 4), [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1], [], InitTileType.Box, [], 0, 1, 0, ["randomMetal", "INTERNAL"]),
        ],
        [
            new InitTile("Missing Metal Slope NE", (1, 1), [2], [], InitTileType.VoxelStruct, [1, 9], 0, 1, 0, ["INTERNAL"]),
            new InitTile("Missing Metal Slope NW", (1, 1), [3], [], InitTileType.VoxelStruct, [1, 9], 0, 1, 0, ["INTERNAL"]),
            new InitTile("Missing Metal Slope SW", (1, 1), [5], [], InitTileType.VoxelStruct, [1, 9], 0, 1, 0, ["INTERNAL"]),
            new InitTile("Missing Metal Slope SE", (1, 1), [4], [], InitTileType.VoxelStruct, [1, 9], 0, 1, 0, ["INTERNAL"]),
            new InitTile("Missing Metal Floor", (1, 1), [6], [], InitTileType.VoxelStruct, [5, 1, 1, 1, 1, 1], 0, 1, 0, ["INTERNAL"]),
        ],
        [
            new InitTile("Dune Sand", (1, 1), [1], [], InitTileType.VoxelStructSandtype, [], 1, 4, 0, ["nonSolid", "INTERNAL"]),
        ]
    ];

    private static Texture[][] LoadPropTextures()
    {
        return GLOBALS.Props.Select(category =>
            category.Select(prop =>
                LoadTexture(Path.Combine(GLOBALS.Paths.PropsAssetsDirectory, prop.Name + ".png"))
            ).ToArray()
        ).ToArray();
    }

    private static (string[], (string, Color)[][]) LoadMaterialInit()
    {
        var path = GLOBALS.Paths.MaterialsInitPath;

        var text = File.ReadAllText(path);

        return Tools.GetMaterialInit(text);
    }

    private static ((string, Color)[], InitTile[][]) LoadTileInit()
    {
        var path = GLOBALS.Paths.TilesInitPath;

        var text = File.ReadAllText(path).ReplaceLineEndings();

        return Tools.GetTileInit(text);
    }

    private static ((string category, Color color)[] categories, InitPropBase[][] init) LoadPropInit()
    {
        var text = File.ReadAllText(GLOBALS.Paths.PropsInitPath).ReplaceLineEndings();
        return Tools.GetPropsInit(text);
    }

    // This is used to check whether the Init.txt file has been altered
    private static string InitChecksum => "77-D1-5E-5F-D7-EF-80-6B-0B-12-30-C1-7E-39-A6-CD-C1-9A-8A-7B-E6-E4-F8-EA-15-3B-85-89-73-BE-9B-0B-AD-35-8C-9E-89-AE-34-42-57-1B-A6-A8-BE-8A-9B-CB-97-3E-AE-33-98-E1-51-92-74-24-2F-DF-81-E6-58-A2";

    private static bool CheckInit()
    {
        using var stream = File.OpenRead(GLOBALS.Paths.TilesInitPath);
        using SHA512 sha = SHA512.Create();

        var hash = sha.ComputeHash(stream);

        return BitConverter.ToString(hash) == InitChecksum;
    }
    
    // MAIN FUNCTION
    static void Main()
    {
        // Initialize logging

        try
        {
            if (!Directory.Exists(Path.Combine(GLOBALS.Paths.ExecutableDirectory, "logs"))) Directory.CreateDirectory(Path.Combine(GLOBALS.Paths.ExecutableDirectory, "logs"));
        }
        catch
        {
            Console.WriteLine("Failed to create logs directory");
            // The program should halt here..
        }

        using var logger = new LoggerConfiguration().WriteTo.File(
            Path.Combine(GLOBALS.Paths.ExecutableDirectory, "logs/logs.txt"),
            fileSizeLimitBytes: 50000000,
            rollOnFileSizeLimit: true
            ).CreateLogger();

        GLOBALS.Logger = logger;

        logger.Information("program has started");

        // check cache directory

        if (!Directory.Exists(GLOBALS.Paths.CacheDirectory))
        {
            try
            {
                Directory.CreateDirectory(GLOBALS.Paths.CacheDirectory);
            }
            catch (Exception e)
            {
                logger.Error($"failed to create cache directory: {e}");
            }
        }

        // check for the assets folder and subfolders
        var integrityFailed = false;
        foreach (var (directory, exists) in GLOBALS.Paths.DirectoryIntegrity)
        {
            if (!exists)
            {
                logger.Fatal($"critical directory not found: \"{directory}\"");
                integrityFailed = true;
            }
            
            if (integrityFailed) goto skip_file_check;
        }

        foreach (var (file, exists) in GLOBALS.Paths.FileIntegrity)
        {
            if (!exists)
            {
                logger.Fatal($"critical file not found: \"{file}]\"");
                integrityFailed = true;
            }
        }
        
        skip_file_check:

        // Import settings

        logger.Information("importing settings");

        // Default settings

        var serOptions = new JsonSerializerOptions { WriteIndented = true };

        // load the settings.json file

        try
        {
            if (File.Exists(Path.Combine(GLOBALS.Paths.ExecutableDirectory, "settings.json")))
            {
                string settingsText = File.ReadAllText(Path.Combine(GLOBALS.Paths.ExecutableDirectory, "settings.json"));
                GLOBALS.Settings = JsonSerializer.Deserialize<Settings>(settingsText) ?? throw new Exception("failed to deserialize settings.json");
            }
            else
            {
                logger.Debug("settings.json file not found; exporting default settings");
                var text = JsonSerializer.Serialize(GLOBALS.Settings, serOptions);
                File.WriteAllText(Path.Combine(GLOBALS.Paths.ExecutableDirectory, "settings.json"), text);
            }
        }
        catch (Exception e)
        {
            logger.Error($"failed to import settings from settings.json: {e}\nusing default settings");

            try
            {
                var text = JsonSerializer.Serialize(GLOBALS.Settings, serOptions);
                File.WriteAllText(Path.Combine(GLOBALS.Paths.ExecutableDirectory, "settings.json"), text);
            }
            catch
            {

            }
        }

        // check for projects folder

        if (!Directory.Exists(GLOBALS.Paths.ProjectsDirectory))
        {
            try
            {
                Directory.CreateDirectory(GLOBALS.Paths.ProjectsDirectory);
            }
            catch (Exception e)
            {
                logger.Fatal($"failed to create a projects folder: {e}");
                return;
            }
        }

        // Check for Init.txt

        bool initExists = File.Exists(GLOBALS.Paths.TilesInitPath);

        if (!initExists)
        {
            logger.Fatal("Init.txt not found");
            //throw new Exception("Init.txt not found");
        }

        // checksum files

        bool initChecksum = !initExists || CheckInit();

        if (initExists) logger.Debug(initChecksum ? "Init.txt passed checksum" : "Init.txt failed checksum");

        //

        logger.Information("initializing data");

        const string version = "Henry's Leditor v0.9.19";
        const string raylibVersion = "Raylib v4.2.0.9";

        logger.Information("indexing tiles and props");

        (GLOBALS.TileCategories, GLOBALS.Tiles) = initExists ? LoadTileInit() : ([], []);
        
        var materialsInit = LoadMaterialInit();

        GLOBALS.MaterialCategories = [..GLOBALS.MaterialCategories, ..materialsInit.Item1];
        GLOBALS.Materials = [..GLOBALS.Materials, ..materialsInit.Item2];

        try
        {
            var (categories, props) = LoadPropInit();
            
            // Shift rope props to the end of the arrays
            (GLOBALS.PropCategories, GLOBALS.Props) = ([..categories, ..GLOBALS.PropCategories], [..props, ..GLOBALS.Props]);
        }
        catch (Exception e)
        {
            logger.Fatal($"failed to load props init: {e}");
            GLOBALS.Page = 99;
        }

        int tileNumber = 0;
        int embeddedTileNumber = 0;

        for (int c = 0; c < GLOBALS.Tiles.Length; c++)
        {
            for (int t = 0; t < GLOBALS.Tiles[c].Length; t++) tileNumber++;
        }

        for (int c = 0; c < embeddedTiles.Length; c++)
        {
            for (int t = 0; t < embeddedTiles[c].Length; t++) embeddedTileNumber++;
        }

        // check for missing textures

        var missingTileImagesTask = from category in GLOBALS.Tiles from tile in category select Task.Factory.StartNew(() => { var path = Path.Combine(GLOBALS.Paths.AssetsDirectory, "tiles", $"{tile.Name}.png"); return (File.Exists(path), path); });
        var missingEmbeddedTileImagesTask = from category in embeddedTiles from tile in category select Task.Factory.StartNew(() => { var path = Path.Combine(GLOBALS.Paths.AssetsDirectory, "embedded", "tiles", $"{tile.Name}.png"); return (File.Exists(path), path); });

        using var missingTileImagesTaskEnum = missingTileImagesTask.GetEnumerator();
        using var missingEmbeddedTileImagesTaskEnum = missingEmbeddedTileImagesTask.GetEnumerator();

        var missingTextureFound = false;

        var checkMissingTextureProgress = 0;
        var checkMissingEmbeddedTextureProgress = 0;

        var checkMissingTextureDone = false;
        var checkMissingEmbeddedTextureDone = false;

        // if the enumerators are empty, there are missing textures.

        if (!missingTileImagesTaskEnum.MoveNext()) missingTextureFound = true;
        if (!missingEmbeddedTileImagesTaskEnum.MoveNext()) missingTextureFound = true;

        // APPEND INTERNAL TILES

        GLOBALS.TileCategories = [.. GLOBALS.TileCategories, .. embeddedCategories];
        GLOBALS.Tiles = [..GLOBALS.Tiles, ..embeddedTiles];

        //

        logger.Information("Initializing window");

        var icon = LoadImage("icon.png");

        SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        
        #if DEBUG
        // TODO: Change this
        SetTraceLogLevel(4);
        #else
        SetTraceLogLevel(7);
        #endif
        
        //----------------------------------------------------------------------------
        // No texture loading prior to this point
        //----------------------------------------------------------------------------
        InitWindow(GLOBALS.MinScreenWidth, GLOBALS.MinScreenHeight, "Henry's Leditor");
        //
        
        SetWindowIcon(icon);
        SetWindowMinSize(GLOBALS.MinScreenWidth, GLOBALS.MinScreenHeight);
        SetExitKey(KeyboardKey.KEY_NULL);

        // The splashscreen
        GLOBALS.Textures.SplashScreen = LoadTexture(Path.Combine(GLOBALS.Paths.ExecutableDirectory, "splashscreen.png"));

        // This is the level's light map, which will be used to draw textures on called "light/shadow brushes"
        GLOBALS.Textures.LightMap = LoadRenderTexture(
            GLOBALS.Level.Width * GLOBALS.Scale + 300,
            GLOBALS.Level.Height * GLOBALS.Scale + 300
        );

        //

        logger.Information("loading textures");

        // Load images to RAM concurrently first, then load them to VRAM in the main thread.
        // Do NOT load textures directly to VRAM on a separate thread.

        Task<Image>[][] tileImagesTasks = GLOBALS.Tiles
                .Select((category, index) =>
                    index < GLOBALS.Tiles.Length - 8
                        ? category.Select(tile => Task.Factory.StartNew(() => LoadImage(Path.Combine(GLOBALS.Paths.AssetsDirectory, "tiles", $"{tile.Name}.png")))).ToArray()
                        : category.Select(tile => Task.Factory.StartNew(() => LoadImage(Path.Combine(GLOBALS.Paths.AssetsDirectory, "embedded", "tiles", $"{tile.Name}.png")))).ToArray()
                )
                .ToArray();

        Task<Image[]>[] tileImageCategoriesDone = tileImagesTasks.Select(Task.WhenAll).ToArray();

        // Of course, don't forget to unload the images after loading the textures to VRAM.
        Task? unloadTileImages = null;

        bool imagesLoaded = false;
        bool texturesLoaded = false;

        //
        try
        {
            logger.Debug("loading UI textures");
            GLOBALS.Textures.GeoMenu = LoadUITextures();
            logger.Debug("loading geo textures");
            GLOBALS.Textures.GeoBlocks = LoadGeoTextures();
            GLOBALS.Textures.GeoStackables = LoadStackableTextures();
            logger.Debug("loading prop textures");
            GLOBALS.Textures.Props = LoadPropTextures();
            logger.Debug("loading embedded long prop textures");
            GLOBALS.Textures.LongProps = GLOBALS.LongProps
                .Select(l => LoadTexture(Path.Combine(GLOBALS.Paths.PropsAssetsDirectory, l.Name + ".png"))).ToArray();
            
            logger.Debug("loading embedded rope prop textures");
            GLOBALS.Textures.RopeProps = GLOBALS.RopeProps
                .Select(r => LoadTexture(Path.Combine(GLOBALS.Paths.PropsAssetsDirectory, r.Name + ".png"))).ToArray();

            logger.Debug("loading light brush textures");
            // Light textures need to be loaded on a separate thread, just like tile textures
            GLOBALS.Textures.LightBrushes = LoadLightTextures(logger);
        }
        catch (Exception e)
        {
            logger.Fatal($"{e}");
        }

        // These two are going to be populated with textures later
        GLOBALS.Textures.Tiles = [];

        GLOBALS.Textures.PropMenuCategories = [
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "prop category tiles.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "prop category ropes.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "prop category longs.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "prop category other.png")),
        ];

        GLOBALS.Textures.PropModes =
        [
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "props select mode.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "props place mode.png")),
        ];

        GLOBALS.Textures.ExplorerIcons =
        [
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "folder icon.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "file icon.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "up icon.png"))
        ];

        Texture[] settingsPreviewTextures =
        [
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "Bigger Head.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "Crossbox B.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "mega chimney A.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "Big Ball.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "Big Stone Marked.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "Big Fan.png"))
        ];

        Texture[] effectsPageTextures =
        [
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "plus icon.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "arrow up icon.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "arrow down icon.png")),
            LoadTexture(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "cross icon.png"))
        ];

        //

        logger.Information("loading shaders");

        GLOBALS.Shaders.TilePreview = LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "tile_preview2.frag"));

        // These two are used to display the light/shadow brush beneath the cursor
        GLOBALS.Shaders.ShadowBrush = LoadShader(null, Path.Combine(GLOBALS.Paths.AssetsDirectory, "shaders", "shadow_brush.fs"));
        GLOBALS.Shaders.LightBrush = LoadShader(null, Path.Combine(GLOBALS.Paths.AssetsDirectory, "shaders", "light_brush.fs"));

        // These two are used to actually draw/erase the shadow on the light map
        GLOBALS.Shaders.ApplyLightBrush = LoadShader(null, Path.Combine(GLOBALS.Paths.AssetsDirectory, "shaders", "apply_light_brush.fs"));
        GLOBALS.Shaders.ApplyShadowBrush = LoadShader(null, Path.Combine(GLOBALS.Paths.AssetsDirectory, "shaders", "apply_shadow_brush.fs"));

        GLOBALS.Shaders.Prop = LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop.frag"));

        GLOBALS.Shaders.StandardProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_standard.frag"));
        
        GLOBALS.Shaders.VariedStandardProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_varied_standard.frag"));
        
        GLOBALS.Shaders.SoftProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_soft.frag"));
        
        GLOBALS.Shaders.VariedSoftProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_varied_soft.frag"));
        
        GLOBALS.Shaders.SimpleDecalProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_simple_decal.frag"));
        
        GLOBALS.Shaders.VariedDecalProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_varied_decal.frag"));
        
        GLOBALS.Shaders.ColoredTileProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_colored.frag"));
        
        GLOBALS.Shaders.ColoredBoxTileProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_colored_box_type.frag"));

        GLOBALS.Shaders.LongProp =
            LoadShader(null, Path.Combine(GLOBALS.Paths.ShadersAssetsDirectory, "prop_long.frag"));
        //

        SetTargetFPS(GLOBALS.Settings.Misc.FPS);


        float initialFrames = 0;

        Texture? screenshotTexture = null;

        logger.Information("Initializing pages");
        
        // Initialize pages

        GeoEditorPage geoPage = new(logger);
        TileEditorPage tilePage = new(logger);
        CamerasEditorPage camerasPage = new(logger);
        LightEditorPage lightPage = new(logger);
        DimensionsEditorPage dimensionsPage = new(logger);
        DeathScreen deathScreen = new(logger, null, null);
        EffectsEditorPage effectsPage = new(logger, effectsPageTextures);
        PropsEditorPage propsPage = new(logger);
        MainPage mainPage = new(logger);
        StartPage startPage = new(logger);
        HelpPage helpPage = new(logger);
        LoadProjectPage loadPage = new(logger);
        SaveProjectPage savePage = new(logger);
        FailedTileCheckOnLoadPage failedTileCheckOnLoadPage = new(logger);
        AssetsNukedPage assetsNukedPage = new(logger);
        MissingAssetsPage missingAssetsPage = new(logger);
        MissingTexturesPage missingTexturesPage = new(logger);
        MissingPropTexturesPage missingPropTexturesPage = new(logger);
        MissingInitFilePage missingInitFilePage = new(logger);
        ExperimentalGeometryPage experimentalGeometryPage = new(logger);
        SettingsPage settingsPage = new(logger, settingsPreviewTextures);
        
        logger.Information("Initializing events");
        
        // Page event handlers
        loadPage.ProjectLoaded += propsPage.OnProjectLoaded;
        loadPage.ProjectLoaded += savePage.OnProjectLoaded;
        
        startPage.ProjectLoaded += propsPage.OnProjectLoaded;
        startPage.ProjectLoaded += savePage.OnProjectLoaded;
        
        dimensionsPage.ProjectCreated += propsPage.OnProjectCreated;
        //
        
        // Quick save task

        Task<(bool success, Exception? exception)>? quickSaveTask = null;
        
        logger.Information("Begin main loop");

        while (!WindowShouldClose())
        {
            try
            {
                #region Splashscreen
                if (initialFrames < 180 && GLOBALS.Settings.Misc.SplashScreen)
                {
                    initialFrames++;

                    BeginDrawing();

                    ClearBackground(new(0, 0, 0, 255));

                    DrawTexturePro(
                        GLOBALS.Textures.SplashScreen,
                        new(0, 0, GLOBALS.Textures.SplashScreen.width, GLOBALS.Textures.SplashScreen.height),
                        new(0, 0, GLOBALS.MinScreenWidth, GLOBALS.MinScreenHeight),
                        new(0, 0),
                        0,
                        new(255, 255, 255, 255)
                    );


                    if (initialFrames > 60) DrawText(version, 700, 50, 15, WHITE);

                    if (initialFrames > 70)
                    {
                        DrawText(raylibVersion, 700, 70, 15, WHITE);
                        if (GLOBALS.Settings.DeveloperMode) DrawText("Developer mode active", 50, 300, 16, YELLOW);
                    }

                    if (initialFrames > 75)
                    {
                        if (!initExists) DrawText("Init.txt not found", 700, 280, 16, new(252, 38, 38, 255));
                    }

                    if (initialFrames > 80)
                    {
                        #if DEBUG
                        if (!initChecksum) DrawText("Init.txt failed checksum", 700, 300, 16, YELLOW);
                        #else
                        if (!initChecksum) DrawText("Tiles have been modified", 700, 300, 16, YELLOW);
                        #endif
                    }

                    if (initialFrames > 90)
                    {
                        if (integrityFailed) 
                            DrawText("missing resources", 700, 320, 16, new(252, 38, 38, 255));
                    }

                    EndDrawing();

                    continue;
                }
                #endregion

                // First, check if the folders exist at all
                if (integrityFailed) GLOBALS.Page = 15;
                
                // Then check tile textures individually
                else if (!checkMissingTextureDone)
                {
                    int r = 0;

                    do
                    {
                        var currentTileTask = missingTileImagesTaskEnum.Current;

                        if (!currentTileTask.IsCompleted) goto skip;

                        var currentTile = currentTileTask.Result;

                        if (!currentTile.Item1)
                        {
                            missingTextureFound = true;
                            logger.Fatal($"missing texture: \"{currentTile.Item2}\"");
                        }

                        checkMissingTextureProgress++;

                        if (!missingTileImagesTaskEnum.MoveNext())
                        {
                            checkMissingTextureDone = true;
                            goto out_;
                        }

                        r++;

                    // Do not remove this label
                    skip:
                        { }

                        // settings.Misc.TileImageScansPerFrame is used to determine the number of scans oer frame.
                    } while (r < GLOBALS.Settings.Misc.TileImageScansPerFrame);

                // Do not remove this label
                out_:

                    var width = GetScreenWidth();
                    var height = GetScreenHeight();

                    BeginDrawing();
                    ClearBackground(new(0, 0, 0, 255));

                    DrawTexturePro(
                        GLOBALS.Textures.SplashScreen,
                        new(0, 0, GLOBALS.Textures.SplashScreen.width, GLOBALS.Textures.SplashScreen.height),
                        new(0, 0, GLOBALS.MinScreenWidth, GLOBALS.MinScreenHeight),
                        new(0, 0),
                        0,
                        new(255, 255, 255, 255)
                    );

                    if (missingTextureFound) DrawText("missing textures found", 700, 300, 16, new(252, 38, 38, 255));
                    if (GLOBALS.Settings.DeveloperMode) DrawText("Developer mode active", 50, 300, 16, YELLOW);

                    DrawText(version, 700, 50, 15, WHITE);
                    DrawText(raylibVersion, 700, 70, 15, WHITE);

                    RayGui.GuiProgressBar(new(100, height - 100, width - 200, 30), "", "", checkMissingTextureProgress, 0, tileNumber);
                    EndDrawing();

                    continue;
                }
                else if (!checkMissingEmbeddedTextureDone)
                {
                    int r = 0;

                    do
                    {
                        var currentEmbeddedTileTask = missingEmbeddedTileImagesTaskEnum.Current;

                        if (!currentEmbeddedTileTask.IsCompleted) goto skip2;

                        var currentEmbeddedTile = currentEmbeddedTileTask.Result;

                        if (!currentEmbeddedTile.Item1)
                        {
                            missingTextureFound = true;
                            logger.Fatal($"missing texture: \"{currentEmbeddedTile.Item2}\"");
                        }

                        checkMissingEmbeddedTextureProgress++;

                        if (!missingEmbeddedTileImagesTaskEnum.MoveNext())
                        {
                            checkMissingEmbeddedTextureDone = true;
                            goto out2_;
                        }

                        r++;

                    skip2:
                        { }
                    } while (r < GLOBALS.Settings.Misc.TileImageScansPerFrame);

                out2_:


                    var width = GetScreenWidth();
                    var height = GetScreenHeight();

                    BeginDrawing();
                    ClearBackground(new(0, 0, 0, 255));

                    DrawTexturePro(
                        GLOBALS.Textures.SplashScreen,
                        new(0, 0, GLOBALS.Textures.SplashScreen.width, GLOBALS.Textures.SplashScreen.height),
                        new(0, 0, GLOBALS.MinScreenWidth, GLOBALS.MinScreenHeight),
                        new(0, 0),
                        0,
                        new(255, 255, 255, 255)
                    );

                    if (missingTextureFound) DrawText("missing textures found", 700, 300, 16, new(252, 38, 38, 255));
                    if (GLOBALS.Settings.DeveloperMode) DrawText("Developer mode active", 50, 300, 16, YELLOW);

                    DrawText(version, 700, 50, 15, WHITE);
                    DrawText(raylibVersion, 700, 70, 15, WHITE);

                    RayGui.GuiProgressBar(new(100, height - 100, width - 200, 30), "", "", checkMissingEmbeddedTextureProgress, 0, embeddedTileNumber);
                    EndDrawing();

                    continue;
                }
                else if (missingTextureFound)
                {
                    GLOBALS.Page = 16;
                }
                else
                {
                    // Loading screen

                    if (!imagesLoaded)
                    {
                        int doneCount = 0;
                        int completedProgress = 0;


                        for (int c = 0; c < tileImageCategoriesDone.Length; c++)
                        {
                            if (tileImageCategoriesDone[c].IsCompletedSuccessfully) doneCount++;

                            for (int t = 0; t < tileImagesTasks[c].Length; t++)
                            {
                                if (tileImagesTasks[c][t].IsCompletedSuccessfully) completedProgress++;
                            }
                        }


                        if (doneCount == tileImageCategoriesDone.Length) imagesLoaded = true;

                        var width = GetScreenWidth();
                        var height = GetScreenHeight();

                        BeginDrawing();
                        ClearBackground(new(0, 0, 0, 255));

                        DrawTexturePro(
                            GLOBALS.Textures.SplashScreen,
                            new(0, 0, GLOBALS.Textures.SplashScreen.width, GLOBALS.Textures.SplashScreen.height),
                            new(0, 0, GLOBALS.MinScreenWidth, GLOBALS.MinScreenHeight),
                            new(0, 0),
                            0,
                            new(255, 255, 255, 255)
                        );

#if DEBUG
                        if (!initChecksum) DrawText("Init.txt failed checksum", 10, 300, 16, YELLOW);
#else
                        if (!initChecksum) DrawText("Tiles have been modified", 10, 300, 16, YELLOW);
#endif
                        if (GLOBALS.Settings.DeveloperMode) DrawText("Developer mode active", 50, 300, 16, YELLOW);

                        DrawText(version, 700, 50, 15, WHITE);
                        DrawText(raylibVersion, 700, 70, 15, WHITE);

                        RayGui.GuiProgressBar(new(100, height - 100, width - 200, 30), "", "", completedProgress, 0, (tileNumber + embeddedTileNumber));
                        EndDrawing();

                        continue;
                    }
                    else if (!texturesLoaded)
                    {
                        GLOBALS.Textures.Tiles = tileImagesTasks.Select(c => c.Select(t => LoadTextureFromImage(t.Result)).ToArray()).ToArray();

                        unloadTileImages = Task.Factory.StartNew(() =>
                        {
                            foreach (var category in tileImagesTasks)
                            {
                                foreach (var imageTask in category) UnloadImage(imageTask.Result);
                            }
                        });

                        texturesLoaded = true;
                        continue;
                    }
                }

                // page preprocessing

                if (GLOBALS.Page == 2 && GLOBALS.Settings.Experimental.NewGeometryEditor) GLOBALS.Page = 18;
                
                // Globals quick save

                {
                    var ctrl = IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);
                    var shift = IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
                    var alt = IsKeyDown(KeyboardKey.KEY_LEFT_ALT);

                    if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.QuickSave.Check(ctrl, shift, alt))
                    {
                        // TODO: quick save
                    }
                }

                // page switch

                switch (GLOBALS.Page)
                {

                    case 0: startPage.Draw(); break;
                    case 1: mainPage.Draw(); break;
                    case 2: geoPage.Draw(); break;
                    case 3: tilePage.Draw(); break;
                    case 4: camerasPage.Draw(); break;
                    case 5: lightPage.Draw(); break;
                    case 6: dimensionsPage.Draw(); break;
                    case 7: effectsPage.Draw(); break;
                    case 8: propsPage.Draw(); break;
                    case 9: settingsPage.Draw(); break;
                    case 11: loadPage.Draw(); break;
                    case 12: savePage.Draw(); break;
                    case 13: failedTileCheckOnLoadPage.Draw(); break;
                    case 14: assetsNukedPage.Draw(); break;
                    case 15: missingAssetsPage.Draw(); break;
                    case 16: missingTexturesPage.Draw(); break;
                    case 17: missingInitFilePage.Draw(); break;
                    case 18: experimentalGeometryPage.Draw(); break;
                    case 19: missingPropTexturesPage.Draw(); break;
                    case 99: deathScreen.Draw(); break;
                    
                    default:
                        GLOBALS.Page = GLOBALS.PreviousPage;
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Fatal($"Bruh Moment detected: loop try-catch block has caught an unexpected error: {e}");

                if (GLOBALS.Settings.Misc.FunnyDeathScreen)
                {
                    var screenshot = LoadImageFromScreen();
                    screenshotTexture = LoadTextureFromImage(screenshot);
                    UnloadImage(screenshot);
                }

                deathScreen = new(logger, screenshotTexture, e);

                GLOBALS.Page = 99; // game over
            }
        }

        logger.Debug("Exiting main loop");
        logger.Information("unloading textures");
        
        UnloadImage(icon);

        foreach (var texture in GLOBALS.Textures.GeoMenu) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.GeoBlocks) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.GeoStackables) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.LightBrushes) UnloadTexture(texture);
        foreach (var category in GLOBALS.Textures.Tiles) { foreach (var texture in category) UnloadTexture(texture); }
        foreach (var texture in GLOBALS.Textures.PropMenuCategories) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.PropModes) UnloadTexture(texture);
        foreach (var category in GLOBALS.Textures.Props) { foreach (var texture in category) UnloadTexture(texture); }
        foreach (var texture in GLOBALS.Textures.LongProps) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.RopeProps) UnloadTexture(texture);
        foreach (var texture in settingsPreviewTextures) UnloadTexture(texture);
        foreach (var texture in GLOBALS.Textures.ExplorerIcons) UnloadTexture(texture);
        foreach (var texture in effectsPageTextures) UnloadTexture(texture);
        
        logger.Debug("Unloading light map");

        UnloadRenderTexture(GLOBALS.Textures.LightMap);
        
        logger.Debug("Unloading shaders");

        UnloadShader(GLOBALS.Shaders.TilePreview);
        UnloadShader(GLOBALS.Shaders.ShadowBrush);
        UnloadShader(GLOBALS.Shaders.LightBrush);
        UnloadShader(GLOBALS.Shaders.ApplyLightBrush);
        UnloadShader(GLOBALS.Shaders.ApplyShadowBrush);
        UnloadShader(GLOBALS.Shaders.Prop);
        UnloadShader(GLOBALS.Shaders.StandardProp);
        UnloadShader(GLOBALS.Shaders.VariedStandardProp);
        UnloadShader(GLOBALS.Shaders.SoftProp);
        UnloadShader(GLOBALS.Shaders.VariedSoftProp);
        UnloadShader(GLOBALS.Shaders.SimpleDecalProp);
        UnloadShader(GLOBALS.Shaders.VariedDecalProp);
        UnloadShader(GLOBALS.Shaders.ColoredTileProp);
        UnloadShader(GLOBALS.Shaders.ColoredBoxTileProp);
        UnloadShader(GLOBALS.Shaders.LongProp);

        unloadTileImages?.Wait();

        CloseWindow();

        logger.Information("program has terminated");
    }
}
