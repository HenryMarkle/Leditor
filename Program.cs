global using Raylib_CsLo;
using static Raylib_CsLo.Raylib;

using System.Numerics;
using System.Text;
using Leditor.Common;
using Leditor.Lingo;
using Pidgin;
using System.Text.Json;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

#nullable enable

namespace Leditor;

class Program
{
    static readonly string[] geoNames = [
        "Solid",
        "air",
        "Slope",
        "Rectangle wall",
        "Rectangle air",
        "Platform",
        "Move level",
        "Place rock",
        "Place spear",
        "Crack terrain",
        "Vertical pole",
        "Horizontal pole",
        "Glass",
        "Copy to lower level",
        "Shortcut entrance",
        "Shortcut",
        "Drangon's den",
        "Passage",
        "Hive",
        "Waterfall",
        "Scavenger hole",
        "Wack-a-mole hole",
        "Garbage worm hole",
        "Worm grass",
        "Forbid Fly Chains",
        "Clear All"
    ];

    static string[] UIImages => [
        "assets/geo/ui/solid.png",          // 0
        "assets/geo/ui/air.png",            // 1
        // "assets/geo/ui/slopebr.png",     
        "assets/geo/ui/slopebl.png",        // 2
        "assets/geo/ui/multisolid.png",     // 3
        "assets/geo/ui/multiair.png",       // 4
        // "assets/geo/ui/slopetr.png",        
        // "assets/geo/ui/slopetl.png",        
        "assets/geo/ui/platform.png",       // 5
        "assets/geo/ui/move.png",           // 6
        "assets/geo/ui/rock.png",           // 7
        "assets/geo/ui/spear.png",          // 8
        "assets/geo/ui/crack.png",          // 9
        "assets/geo/ui/ph.png",             // 10
        "assets/geo/ui/pv.png",             // 11
        "assets/geo/ui/glass.png",          // 12
        "assets/geo/ui/backcopy.png",       // 13
        "assets/geo/ui/entry.png",          // 14
        "assets/geo/ui/shortcut.png",       // 15
        "assets/geo/ui/den.png",            // 16
        "assets/geo/ui/passage.png",        // 17
        "assets/geo/ui/bathive.png",        // 18
        "assets/geo/ui/waterfall.png",      // 19
        "assets/geo/ui/scav.png",           // 20
        "assets/geo/ui/wack.png",           // 21
        "assets/geo/ui/garbageworm.png",    // 22
        "assets/geo/ui/worm.png",           // 23
        "assets/geo/ui/forbidflychains.png",// 24
        "assets/geo/ui/clearall.png",       // 25
    ];

    static string[] GeoImages => [
        // 0: air
        "assets/geo/solid.png",
        "assets/geo/cbl.png",
        "assets/geo/cbr.png",
        "assets/geo/ctl.png",
        "assets/geo/ctr.png",
        "assets/geo/platform.png",
        "assets/geo/entryblock.png",
        // 7: NONE
        // 8: NONE
        "assets/geo/thickglass.png",
    ];

    static string[] StackableImages => [
        "assets/geo/ph.png",             // 0
        "assets/geo/pv.png",             // 1
        "assets/geo/bathive.png",        // 2
        "assets/geo/dot.png",            // 3
        "assets/geo/crackbl.png",        // 4
        "assets/geo/crackbr.png",        // 5
        "assets/geo/crackc.png",         // 6
        "assets/geo/crackh.png",         // 7
        "assets/geo/cracklbr.png",       // 8
        "assets/geo/cracktbr.png",       // 9
        "assets/geo/cracktl.png",        // 10
        "assets/geo/cracktlb.png",       // 11
        "assets/geo/cracktlr.png",       // 12
        "assets/geo/cracktr.png",        // 13
        "assets/geo/cracku.png",         // 14
        "assets/geo/crackv.png",         // 15
        "assets/geo/garbageworm.png",    // 16
        "assets/geo/scav.png",           // 17
        "assets/geo/rock.png",           // 18
        "assets/geo/waterfall.png",      // 19
        "assets/geo/wack.png",           // 20
        "assets/geo/worm.png",           // 21
        "assets/geo/entryb.png",         // 22
        "assets/geo/entryl.png",         // 23
        "assets/geo/entryr.png",         // 24
        "assets/geo/entryt.png",         // 25
        "assets/geo/looseentry.png",     // 26
        "assets/geo/passage.png",        // 27
        "assets/geo/den.png",            // 28
        "assets/geo/spear.png",          // 29
        "assets/geo/forbidflychains.png",// 30
        "assets/geo/crackb.png",         // 31
        "assets/geo/crackr.png",         // 32
        "assets/geo/crackt.png",         // 33
        "assets/geo/crackl.png",         // 34
    ];




    static Texture[] LoadUITextures() => [
        LoadTexture("assets/geo/ui/solid.png"),          // 0
        LoadTexture("assets/geo/ui/air.png"),            // 1
        // LoadTexture("assets/geo/ui/slopebr.png"),     
        LoadTexture("assets/geo/ui/slopebl.png"),        // 2
        LoadTexture("assets/geo/ui/multisolid.png"),     // 3
        LoadTexture("assets/geo/ui/multiair.png"),       // 4
        // LoadTexture("assets/geo/ui/slopetr.png"),        
        // LoadTexture("assets/geo/ui/slopetl.png"),        
        LoadTexture("assets/geo/ui/platform.png"),       // 5
        LoadTexture("assets/geo/ui/move.png"),           // 6
        LoadTexture("assets/geo/ui/rock.png"),           // 7
        LoadTexture("assets/geo/ui/spear.png"),          // 8
        LoadTexture("assets/geo/ui/crack.png"),          // 9
        LoadTexture("assets/geo/ui/ph.png"),             // 10
        LoadTexture("assets/geo/ui/pv.png"),             // 11
        LoadTexture("assets/geo/ui/glass.png"),          // 12
        LoadTexture("assets/geo/ui/backcopy.png"),       // 13
        LoadTexture("assets/geo/ui/entry.png"),          // 14
        LoadTexture("assets/geo/ui/shortcut.png"),       // 15
        LoadTexture("assets/geo/ui/den.png"),            // 16
        LoadTexture("assets/geo/ui/passage.png"),        // 17
        LoadTexture("assets/geo/ui/bathive.png"),        // 18
        LoadTexture("assets/geo/ui/waterfall.png"),      // 19
        LoadTexture("assets/geo/ui/scav.png"),           // 20
        LoadTexture("assets/geo/ui/wack.png"),           // 21
        LoadTexture("assets/geo/ui/garbageworm.png"),    // 22
        LoadTexture("assets/geo/ui/worm.png"),           // 23
        LoadTexture("assets/geo/ui/forbidflychains.png"),// 24
        LoadTexture("assets/geo/ui/clearall.png"),       // 25
    ];


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

    static Texture[] LoadLightTextures() => [
        LoadTexture("assets/light/inverted/Drought_393275_sawbladeGraf.png"),             // 0
        LoadTexture("assets/light/inverted/Drought_393400_pentagonLightEmpty.png"),       // 1
        LoadTexture("assets/light/inverted/Drought_393401_pentagonLight.png"),            // 2
        LoadTexture("assets/light/inverted/Drought_393402_roundedRectLightEmpty.png"),    // 3
        LoadTexture("assets/light/inverted/Drought_393403_squareLightEmpty.png"),         // 4
        LoadTexture("assets/light/inverted/Drought_393404_triangleLight.png"),            // 5
        LoadTexture("assets/light/inverted/Drought_393405_triangleLightEmpty.png"),       // 6
        LoadTexture("assets/light/inverted/Drought_393406_curvedTriangleLight.png"),      // 7
        LoadTexture("assets/light/inverted/Drought_393407_curvedTriangleLightEmpty.png"), // 8
        LoadTexture("assets/light/inverted/Drought_393408_discLightEmpty.png"),           // 9
        LoadTexture("assets/light/inverted/Drought_393409_hexagonLight.png"),             // 10
        LoadTexture("assets/light/inverted/Drought_393410_hexagonLightEmpty.png"),        // 11
        LoadTexture("assets/light/inverted/Drought_393411_octagonLight.png"),             // 12
        LoadTexture("assets/light/inverted/Drought_393412_octagonLightEmpty.png"),        // 13
        LoadTexture("assets/light/inverted/Internal_265_bigCircle.png"),                  // 14
        LoadTexture("assets/light/inverted/Internal_266_leaves.png"),                     // 15
        LoadTexture("assets/light/inverted/Internal_267_oilyLight.png"),                  // 16
        LoadTexture("assets/light/inverted/Internal_268_directionalLight.png"),           // 17
        LoadTexture("assets/light/inverted/Internal_269_blobLight1.png"),                 // 18
        LoadTexture("assets/light/inverted/Internal_270_blobLight2.png"),                 // 19
        LoadTexture("assets/light/inverted/Internal_271_wormsLight.png"),                 // 20
        LoadTexture("assets/light/inverted/Internal_272_crackLight.png"),                 // 21
        LoadTexture("assets/light/inverted/Internal_273_squareishLight.png"),             // 22
        LoadTexture("assets/light/inverted/Internal_274_holeLight.png"),                  // 23
        LoadTexture("assets/light/inverted/Internal_275_roundedRectLight.png"),           // 24
    ];

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
            new InitTile("AltGradeA", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeB1", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeB2", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeB3", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeB4", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeC1", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeC2", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeE1", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeE2", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeF1", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeF2", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeF3", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeF4", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeG1", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeG2", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeH", (3, 4), [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),

            new InitTile("AltGradeI", (1, 1), [0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeJ1", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeJ2", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeJ3", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeJ4", (1, 2), [0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeK1", (2, 2), [0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeK2", (2, 2), [0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeK3", (2, 2), [0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeK4", (2, 2), [0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeL", (2, 2), [0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeM", (2, 2), [0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeN", (4, 4), [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
            new InitTile("AltGradeO", (5, 5), [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], [], InitTileType.VoxelStruct, [1, 1, 1, 6, 1], 0, 1, 0, ["notTrashProp", "notProp", "INTERNAL"]),
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

            new InitTile("Tail Metal", (2, 3), [1, 1, 1, 1, 1, 1], [], InitTileType.Box, [], 1, 1, 0, ["randomMetal", "INTERNAL"]),

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

    static string[] MaterialCategories => [
        "Materials", "Drought Materials", "Community Materials"
    ];
 
    static (string, Color)[][] Materials => [
        [
            ("Standard", new(150, 150, 150, 255)),
            ("Concrete", new(150, 255, 255, 255)),
            ("RainStone", new(0, 0, 255, 255)),
            ("Bricks", new(200, 150, 100, 255)),
            ("BigMetal", new(255, 0, 0, 255)),
            ("Tiny Signs", new(255, 255, 255, 255)),
            ("Scaffolding", new(60, 60, 40, 255)),
            ("Dense Pipes", new(10, 10, 255, 255)),
            ("SuperStructure", new(160, 180, 255, 255)),
            ("SuperStructure2", new(190, 160, 0, 255)),
            ("Tiled Stone", new(100, 0, 255, 255)),
            ("Chaotic Stone", new(255, 0, 255, 255)),
            ("Small Pipes", new(255, 255, 0, 255)),
            ("Trash", new(90, 255, 0, 255)),
            ("Invisible", new(200, 200, 200, 255)),
            ("LargeTrash", new(150, 30, 255, 255)),
            ("3DBricks", new(255, 150, 0, 255)),
            ("Random Machines", new(72, 116, 80, 255)),
            ("Dirt", new(124, 72, 52, 255)),
            ("Ceramic Tile", new(60, 60, 100, 255)),
            ("Temple Stone", new(0, 120, 180, 255)),
            ("Circuits", new(15, 200, 15, 255)),
            ("Ridge", new(200, 15, 60, 255))
        ],
        [
            ("Steel", new(220, 170, 195, 255)),
            ("4Mosaic", new(227, 76, 13, 255)),
            ("Color A Ceramic", new(120, 0, 90, 255)),
            ("Color B Ceramic", new(0, 175, 175, 255)),
            ("Rocks", new(185, 200, 0, 255)),
            ("Rough Rock", new(155, 170, 0, 255)),
            ("Random Metal", new(180, 10, 10, 255)),
            ("Non-Slip Metal", new(180, 80, 80, 255)),
            ("Stained Glass", new(180, 80, 180, 255)),
            ("Sandy Dirt", new(180, 180, 80, 255)),
            ("MegaTrash", new(135, 10, 255, 255)),
            ("Shallow Dense Pipes", new(13, 23, 110, 255)),
            ("Sheet Metal", new(145, 135, 125, 255)),
            ("Chaotic Stone 2", new(90, 90, 90, 255)),
            ("Asphalt", new(115, 115, 115, 255))
        ],
        [
            ("Shallow Circuits", new(15, 200, 155, 255)),
            ("Random Machines 2", new(116, 116, 80, 255)),
            ("Small Machines", new(80, 116, 116, 255)),
            ("Random Metals", new(255, 0, 80, 255)),
            ("ElectricMetal", new(255, 0, 100, 255)),
            ("Grate", new(190, 50, 190, 255)),
            ("CageGrate", new(50, 190, 190, 255)),
            ("BulkMetal", new(50, 19, 190, 255)),
            ("MassiveBulkMetal", new(255, 19, 19, 255)),
            ("Dune Sand", new(255, 255, 100, 255))
        ]
    ];

    static Dictionary<string, Color> MaterialColors => new()
    {
        ["Standard"] = new(150, 150, 150, 255),
        ["Concrete"] = new(150, 255, 255, 255),
        ["RainStone"] = new(0, 0, 255, 255),
        ["Bricks"] = new(200, 150, 100, 255),
        ["BigMetal"] = new(255, 0, 0, 255),
        ["Tiny Signs"] = new(255, 255, 255, 255),
        ["Scaffolding"] = new(60, 60, 40, 255),
        ["Dense Pipes"] = new(10, 10, 255, 255),
        ["SuperStructure"] = new(160, 180, 255, 255),
        ["SuperStructure2"] = new(190, 160, 0, 255),
        ["Tiled Stone"] = new(100, 0, 255, 255),
        ["Chaotic Stone"] = new(255, 0, 255, 255),
        ["Small Pipes"] = new(255, 255, 0, 255),
        ["Trash"] = new(90, 255, 0, 255),
        ["Invisible"] = new(200, 200, 200, 255),
        ["LargeTrash"] = new(150, 30, 255, 255),
        ["3DBricks"] = new(255, 150, 0, 255),
        ["Random Machines"] = new(72, 116, 80, 255),
        ["Dirt"] = new(124, 72, 52, 255),
        ["Ceramic Tile"] = new(60, 60, 100, 255),
        ["Temple Stone"] = new(0, 120, 180, 255),
        ["Circuits"] = new(15, 200, 15, 255),
        ["Ridge"] = new(200, 15, 60, 255),

        ["Steel"] = new(220, 170, 195, 255),
        ["4Mosaic"] = new(227, 76, 13, 255),
        ["Color A Ceramic"] = new(120, 0, 90, 255),
        ["Color B Ceramic"] = new(0, 175, 175, 255),
        ["Rocks"] = new(185, 200, 0, 255),
        ["Rough Rock"] = new(155, 170, 0, 255),
        ["Random Metal"] = new(180, 10, 10, 255),
        ["Non-Slip Metal"] = new(180, 80, 80, 255),
        ["Stained Glass"] = new(180, 80, 180, 255),
        ["Sandy Dirt"] = new(180, 180, 80, 255),
        ["MegaTrash"] = new(135, 10, 255, 255),
        ["Shallow Dense Pipes"] = new(13, 23, 110, 255),
        ["Sheet Metal"] = new(145, 135, 125, 255),
        ["Chaotic Stone 2"] = new(90, 90, 90, 255),
        ["Asphalt"] = new(115, 115, 115, 255),

        ["Shallow Circuits"] = new(15, 200, 155, 255),
        ["Random Machines 2"] = new(116, 116, 80, 255),
        ["Small Machines"] = new(80, 116, 116, 255),
        ["Random Metals"] = new(255, 0, 80, 255),
        ["ElectricMetal"] = new(255, 0, 100, 255),
        ["Grate"] = new(190, 50, 190, 255),
        ["CageGrate"] = new(50, 190, 190, 255),
        ["BulkMetal"] = new(50, 19, 190, 255),
        ["MassiveBulkMetal"] = new(255, 19, 19, 255),
        ["Dune Sand"] = new(255, 255, 100, 255)
    };

    static ((string, Color)[], InitTile[][]) LoadTileInit()
    {
        var path = tilesInitPath;

        var text = File.ReadAllText(path).ReplaceLineEndings(Environment.NewLine);

        return Tools.GetTileInit(text);
    }

    static Image[][] LoadTileImages(in InitTile[][] init)
    {
        List<List<Image>> images = [];

        for (int category = 0; category < init.Length; category++)
        {
            images.Add([]);

            for (int tile = 0; tile < init[category].Length; tile++)
            {
                var tileInit = init[category][tile];

                images[0].Add(Raylib.LoadImage($"assets/tiles/{tileInit.Name}.png"));
            }
        }

        return images.Select(t => t.ToArray()).ToArray();
    }

    static Texture[][] LoadTileTextures(in InitTile[][] init)
    {
        List<List<Texture>> textures = [];

        for (int category = 0; category < init.Length; category++)
        {
            textures.Add([]);

            for (int tile = 0; tile < init[category].Length; tile++)
            {
                var tileInit = init[category][tile];

                textures[category].Add(Raylib.LoadTexture($"assets/tiles/{tileInit.Name}.png"));
            }
        }

        return textures.Select(t => t.ToArray()).ToArray();
    }

    static int GetTilePreviewStartingHeight(in InitTile init)
    {
        var (width, height) = init.Size;
        var bufferTiles = init.BufferTiles;
        var repeatL = init.Repeat.Length;

        return init.Type switch
        {
            InitTileType.VoxelStruct => 1 + scale * ((bufferTiles * 2) + height) * repeatL,
            InitTileType.VoxelStructRockType => 1 + scale * ((bufferTiles * 2) + height),
            InitTileType.Box => scale * height * width + (scale * (height + (2 * bufferTiles))),
            InitTileType.VoxelStructRandomDisplaceVertical => 1 + scale * ((bufferTiles * 2) + height) * repeatL,
            InitTileType.VoxelStructRandomDisplaceHorizontal => 1 + scale * ((bufferTiles * 2) + height) * repeatL,

            _ => 1 + scale * ((bufferTiles * 2) + height) * repeatL
        };
    }

    static Texture LoadTilePreviewTexture(in InitTile init)
    {
        var (width, height) = init.Size;
        Image img = Raylib.LoadImage($"assets/tiles/{init.Name}.png");

        var previewHeightStart = GetTilePreviewStartingHeight(init);

        unsafe
        {
            Raylib.ImageCrop(&img, new Rectangle(0, previewHeightStart, 16 * width, 16 * height));
        }

        Texture texture = Raylib.LoadTextureFromImage(img);
        Raylib.UnloadImage(img);
        return texture;
    }

    static Image LoadTilePreviewImageFromFile(in InitTile init, string path)
    {
        var (width, height) = init.Size;
        Image img = Raylib.LoadImage(path);

        var previewHeightStart = GetTilePreviewStartingHeight(init);

        unsafe
        {
            Raylib.ImageCrop(&img, new Rectangle(0, previewHeightStart, 16 * width, 16 * height));
        }

        return img;
    }

    static string executableDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

    static string projectsDirectory = Path.Combine(executableDirectory, "projects");
    static string tilesInitPath = Path.Combine(executableDirectory, "Init.txt");
    static string assetsDirectory = Path.Combine(executableDirectory, "assets");

    const int screenMinWidth = 1280;
    const int screenMinHeight = 800;

    const int renderCameraWidth = 1400;
    const int renderCameraHeight = 800;

    const int geoselectWidth = 200;
    const int geoselectHeight = 600;
    const int scale = 20;
    const int uiScale = 40;
    static bool camScaleMode = false;
    const int previewScale = 16;
    const float zoomIncrement = 0.125f;
    const int initialMatrixWidth = 72;
    const int initialMatrixHeight = 43;

    static Color[] layerColors;

    static readonly Color whiteStackable = new(255, 255, 255, 200);
    static readonly Color blackStackable = new(0, 0, 0, 200);

    public static int GetBlockIndex(int id) => id switch
    {
        1 => 0,
        2 => 1,
        3 => 2,
        4 => 3,
        5 => 4,
        6 => 5,
        7 => 6,
        9 => 7,

        _ => -1,
    };

    /// <summary>
    /// Maps a ui texure index to block ID
    /// </summary>
    public static int GetBlockID(uint index) => index switch
    {
        0 => 1,
        1 => 0,
        7 => 4,
        6 => 5,
        3 => 2,
        2 => 3,
        5 => 6,
        12 => 9,
        _ => -1
    };

    public static readonly int[] stackableIds = new int[29];

    /// <summary>
    /// Maps a ui texture index to a stackable ID
    /// </summary>
    public static int GetStackableID(uint index) => index switch
    {
        7 => 9,
        8 => 10,
        9 => 11,
        10 => 1,
        11 => 2,
        14 => 4,
        15 => 5,
        16 => 7,
        17 => 6,
        18 => 3,
        19 => 18,
        20 => 21,
        21 => 19,
        22 => 13,
        23 => 20,
        24 => 12,

        _ => -1
    };

    public static int GetStackableTextureIndex(int id) => id switch
    {
        1 => 0,
        2 => 1,
        3 => 2,
        5 => 3,
        6 => 27,
        7 => 28,
        9 => 18,
        10 => 29,
        12 => 30,
        13 => 16,
        18 => 19,
        19 => 20,
        20 => 21,
        21 => 17,
        _ => -1
    };

    public static int GetStackableTextureIndex(int id, RunCell[][] context)
    {
        var i = id switch
        {
            1 => 0,
            2 => 1,
            3 => 2,
            4 => -4,
            5 => 3,
            6 => 27,
            7 => 28,
            9 => 18,
            10 => 29,
            11 => -11,
            12 => 30,
            13 => 16,
            18 => 19,
            19 => 20,
            20 => 21,
            21 => 17,
            _ => -1
        };


        if (i == -4)
        {
            if (
                context[0][0].Stackables[4] || context[0][1].Stackables[4] || context[0][2].Stackables[4] ||
                context[1][0].Stackables[4] || context[1][2].Stackables[4] ||
                context[2][0].Stackables[4] || context[2][1].Stackables[4] || context[2][2].Stackables[4]
            ) return 26;

            var pattern = (
                false, context[0][1].Stackables[5] ^ context[0][1].Stackables[6] ^ context[0][1].Stackables[7] ^ context[0][1].Stackables[19], false,
                context[1][0].Stackables[5] ^ context[1][0].Stackables[6] ^ context[1][0].Stackables[7] ^ context[1][0].Stackables[19], false, context[1][2].Stackables[5] ^ context[1][2].Stackables[6] ^ context[1][2].Stackables[7] ^ context[1][2].Stackables[19],
                false, context[2][1].Stackables[5] ^ context[2][1].Stackables[6] ^ context[2][1].Stackables[7] ^ context[2][1].Stackables[19], false
            );

            var directionIndex = pattern switch
            {

                (
                    _, true, _,
                    false, _, false,
                    _, false, _
                ) => 25,

                (
                    _, false, _,
                    false, _, true,
                    _, false, _
                ) => 24,

                (
                    _, false, _,
                    false, _, false,
                    _, true, _
                ) => 22,

                (
                    _, false, _,
                    true, _, false,
                    _, false, _
                ) => 23,

                _ => 26
            };

            if (directionIndex == 26) return 26;

            var geoPattern = (
                context[0][0].Geo, context[0][1].Geo, context[0][2].Geo,
                context[1][0].Geo, 0, context[1][2].Geo,
                context[2][0].Geo, context[2][1].Geo, context[2][2].Geo
            );

            directionIndex = geoPattern switch
            {

                (
                    1, _, 1,
                    1, _, 1,
                    1, 1, 1
                ) => context[0][1].Geo is 0 or 6 ? directionIndex : 26,

                (
                    1, 1, 1,
                    1, _, _,
                    1, 1, 1
                ) => context[1][2].Geo is 0 or 6 ? directionIndex : 26,

                (
                    1, 1, 1,
                    1, _, 1,
                    1, _, 1
                ) => context[2][1].Geo is 0 or 6 ? directionIndex : 26,

                (
                    1, 1, 1,
                    _, _, 1,
                    1, 1, 1
                ) => context[1][0].Geo is 0 or 6 ? directionIndex : 26,

                _ => 26
            };

            return directionIndex;
        }
        else if (i == -11)
        {
            i = (
                false, context[0][1].Stackables[11], false,
                context[1][0].Stackables[11], false, context[1][2].Stackables[11],
                false, context[2][1].Stackables[11], false
            ) switch
            {

                (
                    _, true, _,
                    false, _, false,
                    _, false, _
                ) => 33,

                (
                    _, false, _,
                    false, _, true,
                    _, false, _
                ) => 32,

                (
                    _, false, _,
                    false, _, false,
                    _, true, _
                ) => 31,

                (
                    _, false, _,
                    true, _, false,
                    _, false, _
                ) => 34,

                //

                (
                    _, true, _,
                    false, _, true,
                    _, false, _
                ) => 13,

                (
                    _, false, _,
                    false, _, true,
                    _, true, _
                ) => 5,

                (
                    _, false, _,
                    true, _, false,
                    _, true, _
                ) => 4,

                (
                    _, true, _,
                    true, _, false,
                    _, false, _
                ) => 10,

                //

                (
                    _, true, _,
                    true, _, true,
                    _, false, _
                ) => 12,

                (
                    _, true, _,
                    false, _, true,
                    _, true, _
                ) => 9,

                (
                    _, false, _,
                    true, _, true,
                    _, true, _
                ) => 8,

                (
                    _, true, _,
                    true, _, false,
                    _, true, _
                ) => 11,

                //

                (
                    _, false, _,
                    true, _, true,
                    _, false, _
                ) => 7,

                (
                    _, true, _,
                    false, _, false,
                    _, true, _
                ) => 15,

                //

                (
                    _, true, _,
                    true, _, true,
                    _, true, _
                ) => 6,

                (
                    _, false, _,
                    false, _, false,
                    _, false, _
                ) => 14,
            };
        }

        return i;
    }

    public static int GetCorrectSlopeID(RunCell[][] context)
    {
        return (
            false, context[0][1].Geo == 1, false,
            context[1][0].Geo == 1, false, context[1][2].Geo == 1,
            false, context[2][1].Geo == 1, false
        ) switch
        {
            (
                _, false, _,
                true, _, false,
                _, true, _
            ) => 2,
            (
                _, false, _,
                false, _, true,
                _, true, _
            ) => 3,
            (
                _, true, _,
                true, _, false,
                _, false, _
            ) => 4,
            (
                _, true, _,
                false, _, true,
                _, false, _
            ) => 5,

            _ => -1

        };
    }

    static void PaintEffect(double[,] matrix, (int x, int y) matrixSize, (int x, int y) center, int brushSize, double strength)
    {
        for (int y = center.y - brushSize; y < center.y + brushSize + 1; y++)
        {
            if (y < 0 || y >= matrixSize.y) continue;

            for (int x = center.x - brushSize; x < center.x + brushSize + 1; x++)
            {
                if (x < 0 || x >= matrixSize.x) continue;

                matrix[y, x] += strength;

                if (matrix[y, x] > 100) matrix[y, x] = 100;
                if (matrix[y, x] < 0) matrix[y, x] = 0;
            }
        }
    }

    static (bool clicked, bool hovered) DrawCameraSprite(
        Vector2 origin,
        CameraQuads quads,
        Camera2D camera,
        int index = -1)
    {
        var mouse = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
        var hover = Raylib.CheckCollisionPointCircle(mouse, new(origin.X + renderCameraWidth / 2, origin.Y + renderCameraHeight / 2), 50);
        var biggerHover = Raylib.CheckCollisionPointRec(mouse, new(origin.X, origin.Y, renderCameraWidth, renderCameraHeight));

        Vector2 pointOrigin1 = new(origin.X, origin.Y),
            pointOrigin2 = new(origin.X + renderCameraWidth, origin.Y),
            pointOrigin3 = new(origin.X, origin.Y + renderCameraHeight),
            pointOrigin4 = new(origin.X + renderCameraWidth, origin.Y + renderCameraHeight);

        if (biggerHover)
        {
            Raylib.DrawRectangleV(
                origin,
                new(renderCameraWidth, renderCameraHeight),
                new(0, 255, 150, 70)
            );
        }
        else
        {
            Raylib.DrawRectangleV(
                origin,
                new(renderCameraWidth, renderCameraHeight),
                new(0, 255, 0, 70)
            );
        }

        if (index != -1)
        {
            Raylib.DrawText(
                $"{index}",
                (int)origin.X + 10,
                (int)origin.Y + 10,
                20,
                new(255, 255, 255, 255)
            );
        }

        Raylib.DrawRectangleLinesEx(
            new(origin.X, origin.Y, renderCameraWidth, renderCameraHeight),
            4f,
            new(255, 255, 255, 255)
        );

        Raylib.DrawRectangleLinesEx(
            new(origin.X, origin.Y, renderCameraWidth, renderCameraHeight),
            2f,
            new(0, 0, 0, 255)
        );

        Raylib.DrawCircleLines(
            (int)(origin.X + renderCameraWidth / 2),
            (int)(origin.Y + renderCameraHeight / 2),
            50,
            new(0, 0, 0, 255)
        );

        if (hover)
        {
            Raylib.DrawCircleV(
                new(origin.X + renderCameraWidth / 2, origin.Y + renderCameraHeight / 2),
                50,
                new Color(255, 255, 255, 100)
            );
        }

        Raylib.DrawLineEx(
            new(origin.X + 4, origin.Y + renderCameraHeight / 2),
            new(origin.X + renderCameraWidth - 4, origin.Y + renderCameraHeight / 2),
            4f,
            new(0, 0, 0, 255)
        );

        Raylib.DrawRectangleLinesEx(
            new(
                origin.X + 190,
                origin.Y + 20,
                51 * scale,
                40 * scale - 40
            ),
            4f,
            new(255, 0, 0, 255)
        );

        var quarter1 = new Rectangle(origin.X - 150, origin.Y - 150, renderCameraWidth / 2 + 150, renderCameraHeight / 2 + 150);
        var quarter2 = new Rectangle(renderCameraWidth / 2 + origin.X, origin.Y - 150, renderCameraWidth / 2 + 150, renderCameraHeight / 2 + 150);
        var quarter3 = new Rectangle(origin.X - 150, origin.Y + renderCameraHeight / 2, renderCameraWidth / 2 + 150, renderCameraHeight / 2 + 150);
        var quarter4 = new Rectangle(renderCameraWidth / 2 + origin.X, renderCameraHeight / 2 + origin.Y, renderCameraWidth / 2 + 150, renderCameraHeight / 2 + 150);

        if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)) camScaleMode = false;

        if (Raylib.CheckCollisionPointRec(mouse, quarter1))
        {

            if ((Raylib.CheckCollisionPointCircle(mouse, RayMath.Vector2Add(quads.TopLeft, pointOrigin1), 10) || camScaleMode) &&
                Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                camScaleMode = true;

                quads.TopLeft = RayMath.Vector2Subtract(mouse, pointOrigin1);
            }

            Raylib.DrawCircleV(RayMath.Vector2Add(quads.TopLeft, origin), 10, new(0, 255, 0, 255));
        }


        if (Raylib.CheckCollisionPointRec(mouse, quarter2))
        {
            if ((Raylib.CheckCollisionPointCircle(mouse, RayMath.Vector2Add(quads.TopRight, pointOrigin2), 10) || camScaleMode) &&
                Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                camScaleMode = true;
                quads.TopRight = RayMath.Vector2Subtract(mouse, pointOrigin2);
            }

            Raylib.DrawCircleV(RayMath.Vector2Add(quads.TopRight, pointOrigin2), 10, new(0, 255, 0, 255));
        }

        if (Raylib.CheckCollisionPointRec(mouse, quarter3))
        {
            if ((Raylib.CheckCollisionPointCircle(mouse, RayMath.Vector2Add(quads.BottomRight, pointOrigin3), 10) || camScaleMode) &&
                Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                camScaleMode = true;
                quads.BottomRight = RayMath.Vector2Subtract(mouse, pointOrigin3);
            }

            Raylib.DrawCircleV(RayMath.Vector2Add(quads.BottomRight, pointOrigin3), 10, new(0, 255, 0, 255));
        }

        if (Raylib.CheckCollisionPointRec(mouse, quarter4))
        {
            if ((Raylib.CheckCollisionPointCircle(mouse, RayMath.Vector2Add(quads.BottomLeft, pointOrigin4), 10) || camScaleMode) &&
                Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                camScaleMode = true;
                quads.BottomLeft = RayMath.Vector2Subtract(mouse, pointOrigin4);
            }

            Raylib.DrawCircleV(RayMath.Vector2Add(quads.BottomLeft, pointOrigin4), 10, new(0, 255, 0, 255));
        }

        return (hover && Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT), biggerHover);
    }

    static void DrawTileSpec(int id, Vector2 origin, int scale, Color color)
    {
        switch (id)
        {
            // air
            case 0: break;

            // solid
            case 1:
                DrawRectangleV(origin, RayMath.Vector2Scale(new(1, 1), scale), color);
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
            case 7: break;

            // glass
            case 9: break;
        }
    }

    static void ResizeLightMap(ref RenderTexture buffer, int newWidth, int newHeight)
    {
        var image = Raylib.LoadImageFromTexture(buffer.texture);
        Raylib.UnloadRenderTexture(buffer);
        var texture = Raylib.LoadTextureFromImage(image);
        Raylib.UnloadImage(image);

        buffer = Raylib.LoadRenderTexture((newWidth * scale) + 300, (newHeight) * scale + 300);

        Raylib.BeginTextureMode(buffer);
        Raylib.ClearBackground(new(255, 255, 255, 255));
        Raylib.DrawTexture(texture, 0, 0, new(255, 255, 255, 255));
        Raylib.EndTextureMode();

        Raylib.UnloadTexture(texture);
    }

    static LoadFileResult LoadProject(string filePath)
    {
        try
        {
            var text = File.ReadAllText(filePath).Split('\r');

            var lightMapFileName = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".png");

            if (!File.Exists(lightMapFileName)) return new();

            var lightMap = Raylib.LoadImage(lightMapFileName);

            if (text.Length < 7) return new LoadFileResult();

            var obj = Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[0]);
            var tilesObj = Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[1]);
            var obj2 = Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[5]);
            var effObj = Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[2]);
            var camsObj = Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[6]);

            var mtx = Tools.GetGeoMatrix(obj, out int givenHeight, out int givenWidth);
            var tlMtx = Tools.GetTileMatrix(tilesObj, out int tlHeight, out int tlWidth);
            var buffers = Tools.GetBufferTiles(obj2);
            var effects = Tools.GetEffects(effObj, givenWidth, givenHeight);
            var cams = Tools.GetCameras(camsObj);

            // map material colors

            Color[,,] materialColors = CommonUtils.NewMaterialColorMatrix(givenWidth, givenHeight, new(0, 0, 0, 255));

            for (int y = 0; y < givenHeight; y++)
            {
                for (int x = 0; x < givenWidth; x++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        var cell = tlMtx[y, x, z];

                        if (cell.Type != TileType.Material) continue;

                        var materialName = ((TileMaterial)cell.Data).Name;

                        if (MaterialColors.TryGetValue(materialName, out Color color)) materialColors[y, x, z] = color; 
                    }
                }
            }

            //

            return new()
            {
                Success = true,
                Width = givenWidth,
                Height = givenHeight,
                BufferTiles = buffers,
                GeoMatrix = mtx,
                TileMatrix = tlMtx,
                MaterialColorMatrix = materialColors,
                Effects = effects,
                LightMapImage = lightMap,
                Cameras = cams,
                Name = Path.GetFileNameWithoutExtension(filePath)
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new();
        }
    }

    static int GetMiddle(int number)
    {
        if (number < 3) return 0;
        if (number % 2 == 0) return number / 2 - 1;
        else return number / 2;
    }

    static Vector2 GetTileHeadOrigin(in InitTile init)
    {
        var (width, height) = init.Size;
        return new Vector2(GetMiddle(width), GetMiddle(height));
    }

    static bool IsTileLegal(ref InitTile init, Vector2 point, TileCell[,,] tileMatrix, RunCell[,,] geoMatrix, int currentLayer)
    {
        var (width, height) = init.Size;
        var specs = init.Specs;
        var specs2 = init.Specs2;

        // get the "middle" point of the tile
        var head = GetTileHeadOrigin(init);

        // the top-left of the tile
        var start = RayMath.Vector2Subtract(
            RayMath.Vector2Scale(point, 16),
            RayMath.Vector2Scale(head, 16)
        );

        for (int y = (int)start.Y; y < height; y++)
        {
            for (int x = (int)start.X; x < width; x++)
            {
                var tileCell = tileMatrix[y, x, currentLayer];
                var geoCell = geoMatrix[y, x, currentLayer];

                var spec = specs[(x - (int)start.X - height) + y - (int)start.Y];

                if (spec != -1 && (geoCell.Geo != spec || tileCell.Type != TileType.Default || tileCell.Type != TileType.Material)) return false;
            }
        }

        return true;
    }

    static unsafe void Main(string[] args)
    {
        // Initialize logging

        try
        {
            if (!Directory.Exists(Path.Combine(executableDirectory, "logs"))) Directory.CreateDirectory(Path.Combine(executableDirectory, "logs"));
        }
        catch
        {
            Console.WriteLine("Failed to create logs directory");
        }

        using var logger = new LoggerConfiguration().WriteTo.File(
            Path.Combine(executableDirectory, "logs/logs.txt"),
            fileSizeLimitBytes: 10000000,
            rollOnFileSizeLimit: true
            ).CreateLogger();

        logger.Information("program has started");

        // Import settings

        logger.Information("importing settings");

        // Default settings

        Color layer1Color = new(0, 0, 0, 255);
        Color layer2Color = new(0, 255, 0, 50);
        Color layer3Color = new(255, 0, 0, 50);

        Settings settings = new(
            new(
                new()
                ),
            new(),
            new(
                new(
                    Layer1: layer1Color,
                    Layer2: layer2Color,
                    Layer3: layer3Color
                    )
                ),
            new(true)
        );

        var serOptions = new JsonSerializerOptions { WriteIndented = true };

        try
        {
            if (File.Exists(Path.Combine(executableDirectory, "settings.json")))
            {
                string settingsText = File.ReadAllText(Path.Combine(executableDirectory, "settings.json"));
                settings = JsonSerializer.Deserialize<Settings>(settingsText);
            }
            else
            {
                logger.Debug("settings.json file not found; exporting default settings");
                var text = JsonSerializer.Serialize(settings, serOptions);
                File.WriteAllText(Path.Combine(executableDirectory, "settings.json"), text);
            }
        }
        catch (UnauthorizedAccessException)
        {
            logger.Error("access was denied to settings.json; using default settings");
        }
        catch (JsonException)
        {
            logger.Error("settings.json containes invalid information; exporting default settings");

            try
            {
                var text = JsonSerializer.Serialize(settings, serOptions);
                File.WriteAllText(Path.Combine(executableDirectory, "settings.json"), text);
            }
            catch
            {
                logger.Debug("failed to export default settings after finding settings.json to be corrupt; using default settings");
            }

        }
        catch (Exception e)
        {
            logger.Error($"failed to import settings from settings.json: {e}\nusing default settings");
        }

        layerColors = [
            settings.GeomentryEditor.LayerColors.Layer1,
            settings.GeomentryEditor.LayerColors.Layer2,
            settings.GeomentryEditor.LayerColors.Layer3
        ];

        //

        if (!Directory.Exists(projectsDirectory))
        {
            try
            {
                Directory.CreateDirectory(projectsDirectory);
            }
            catch (Exception e)
            {
                logger.Fatal($"failed to create a projects folder: {e}");
                return;
            }
        }

        //

        logger.Information("initializing data");

        string version = "0.3.21";

        int page = 0;
        int prevPage = 0;

        BufferTiles bufferTiles = new(12, 12, 3, 5);

        int tileSeed = 141;
        bool lightMode = true;
        bool defaultTerrain = true;

        int matrixWidth = initialMatrixWidth;
        int matrixHeight = initialMatrixHeight;

        bool fillLayer1 = false;
        bool fillLayer2 = false;
        bool fillLayer3 = false;

        uint geoSelectionX = 0;
        uint geoSelectionY = 0;

        int currentLayer = 0;

        bool showLayer1 = true;
        bool showLayer2 = true;
        bool showLayer3 = true;

        bool showTilesInGeo = false;

        bool showTileLayer1 = true;
        bool showTileLayer2 = true;
        bool showTileLayer3 = true;

        bool showLayer1Tiles = true;
        bool showLayer2Tiles = true;
        bool showLayer3Tiles = true;

        int prevCoordsX = -1;
        int prevCoordsY = -1;
        bool multiselect = false;

        int prevMatrixX = -1;
        int prevMatrixY = -1;

        bool clickTracker = false;
        bool clickTracker2 = false;

        int waterLevel = -1;
        bool waterInFront = true;

        bool gridContrast = false;

        RunCell[,,] geoMatrix = CommonUtils.NewGeoMatrix(matrixWidth, matrixHeight, 1);
        TileCell[,,] tileMatrix = CommonUtils.NewTileMatrix(matrixWidth, matrixHeight);
        Color[,,] materialColorMatrix = CommonUtils.NewMaterialColorMatrix(matrixWidth, matrixHeight, new(0, 0, 0, 255));
        (string, EffectOptions, double[,])[] effectList = [];

        logger.Information("indexing tile");

        var (tileCategories, initTiles) = LoadTileInit();

        // APPEND INTERNAL TILES
        tileCategories = [.. tileCategories, .. embeddedCategories];
        initTiles = [.. initTiles, .. embeddedTiles];
        //

        int flatness = 0;
        double lightAngleVariable = 0;
        double lightAngle = 90;

        const float initialGrowthFactor = 0.01f;
        float growthFactor = initialGrowthFactor;
        var lightRecSize = new Vector2(100, 100);
        bool slowGrowth = true;
        bool shading = true;

        int explorerPage = 0;

        bool addNewEffectMode = false;

        int currentAppliedEffect = 0;
        int currentAppliedEffectPage = 0;

        bool showEffectOptions = true;

        int brushRadius = 3;

        bool brushEraseMode = false;

        int lightBrushTextureIndex = 0;
        int lightBrushTexturePage = 0;

        Rectangle lightBrushSource = new();
        Rectangle lightBrushDest = new();
        Vector2 lightBrushOrigin = new();

        float lightBrushRotation = 0;
        float lightBrushWidth = 200;
        float lightBrushHeight = 200;

        bool eraseShadow = false;

        bool resizeFlag = false;
        bool newFlag = false;

        int draggedCamera = -1;

        List<RenderCamera> renderCamers = [];

        Task<LoadFileResult> loadFileTask = Task.FromResult(new LoadFileResult());
        Task<TileCheckResult>? tileCheckTask = null;

        int tilePanelWidth = 390;

        bool showTileSpecs = settings.TileEditor.VisibleSpecs;

        var missingTileWarnTitleText = "Your project seems to contain undefined tiles";
        var notFoundTileWarnTitleText = "Your project seems to have old tiles";
        var missingTileTextureWarnTitleText = "Your project contains a tile with no texture";
        var missingTileWarnSubtitleText = "If you used custom tiles on this project from a different level editor, please use its Init.txt";
        var missingTileTextureWarnSubTitleText = "If you have appended to the Init.txt file, please make sure you include the textures as well";
        var missingMaterialWarnTitleText = "Your project seems to have undefined materials";
        var missingMaterialWarnSubtitleText = "Please update the materials init.txt file before loading this project";

        // UNSAFE VARIABLES

        int matrixWidthValue = 72;  // default width
        int matrixHeightValue = 43; // default height
        var panelBytes = Encoding.ASCII.GetBytes("New Level");
        int leftPadding = 12;
        int rightPadding = 12;
        int topPadding = 3;
        int bottomPadding = 5;

        int editControl = 0;

        var previewPanelBytes = Encoding.ASCII.GetBytes("Level Options");

        var helpPanelBytes = Encoding.ASCII.GetBytes("Shortcuts");

        int helpScrollIndex = 0;
        int helpSubSection = 0;

        var explorerPanelBytes = Encoding.ASCII.GetBytes("Load Project");

        var projectNameBufferBytes = Encoding.ASCII.GetBytes("");
        var projectName = "New Project";

        var saveProjectPanelBytes = Encoding.ASCII.GetBytes("Save Project");

        var appliedEffectsPanelBytes = Encoding.ASCII.GetBytes("Applied Effects");
        var addNewEffectPanelBytes = Encoding.ASCII.GetBytes("New Effect");

        var newEffectCategoryScrollIndex = 0;
        var newEffectCategorySelectedValue = 0;

        var newEffectScrollIndex = 0;
        var newEffectSelectedValue = 0;

        var newEffectFocus = false;

        var effectOptionsPanelBytes = Encoding.ASCII.GetBytes("Options");

        var lightBrushMenuPanelBytes = Encoding.ASCII.GetBytes("Brushes");

        var geoMenuPanelBytes = Encoding.ASCII.GetBytes("Tiles");

        var cameraQuadsPanelBytes = Encoding.ASCII.GetBytes("Quads");

        var tilesPanelBytes = Encoding.ASCII.GetBytes("Tiles");

        var tileCategoryScrollIndex = 0;

        var tileCategoryIndex = 0;

        var tileScrollIndex = 0;
        var tileIndex = 0;

        bool tileCategoryFocus = false;

        var tileSpecsPanelBytes = Encoding.ASCII.GetBytes("Tile Specs");

        //

        Camera2D camera = new() { zoom = 1.0f };
        Camera2D tileCamera = new() { zoom = 1.0f };
        Camera2D mainPageCamera = new() { zoom = 0.5f };
        Camera2D effectsCamera = new() { zoom = 0.8f };
        Camera2D lightPageCamera = new() { zoom = 0.5f, target = new(-500, -200) };
        Camera2D cameraCamera = new() { zoom = 0.8f, target = new(-100, -100) };

        Rectangle border = new(
            bufferTiles.Left * scale,
            bufferTiles.Top * scale,
            (matrixWidth - (bufferTiles.Right + bufferTiles.Left)) * scale,
            (matrixHeight - (bufferTiles.Bottom + bufferTiles.Top)) * scale
        );

        logger.Information("initializing window");

        Image icon = LoadImage("icon.png");

        SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        InitWindow(screenMinWidth, screenMinHeight, "Henry's Leditor");
        SetWindowIcon(icon);
        SetWindowMinSize(screenMinWidth, screenMinHeight);

        RenderTexture lightMapBuffer = LoadRenderTexture(
            matrixWidth * scale + 300,
            matrixHeight * scale + 300
        );

        //

        logger.Information("loading textures");

        Task<Image>[][] tileImagesTasks = initTiles
                .Select((category, index) =>
                    index < initTiles.Length - 8
                        ? category.Select(tile => Task.Factory.StartNew(() => LoadImage($"assets/tiles/{tile.Name}.png"))).ToArray()
                        : category.Select(tile => Task.Factory.StartNew(() => LoadImage($"assets/tiles/embedded/{tile.Name}.png"))).ToArray()
                )
                .ToArray();

        Task<Image>[][] tilePreviewImagesTasks = initTiles
            .Select((category, index) =>
                index < initTiles.Length - 8
                    ? category.Select(tile => Task.Factory.StartNew(() => LoadTilePreviewImageFromFile(tile, $"assets/tiles/{tile.Name}.png"))).ToArray()
                    : category.Select(tile => Task.Factory.StartNew(() => LoadTilePreviewImageFromFile(tile, $"assets/tiles/embedded/{tile.Name}.png"))).ToArray())
            .ToArray();

        Task[] tileImageCategoriesDone = tileImagesTasks.Select(Task.WhenAll).ToArray();
        Task[] tilePreviewImageCategoriesDone = tilePreviewImagesTasks.Select(Task.WhenAll).ToArray();

        int totalTileImageNumber = 0;

        for (int c = 0; c < tileImagesTasks.Length; c++)
        {
            for (int t = 0; t < tileImagesTasks[c].Length; t++)
            {
                totalTileImageNumber += 2;
            }
        }

        bool imagesLoaded = false;
        bool texturesLoaded = false;

        //

        Texture[] uiTextures = LoadUITextures();
        Texture[] geoTextures = LoadGeoTextures();
        Texture[] stackableTextures = LoadStackableTextures();
        Texture[] lightTextures = LoadLightTextures();

        Texture[][] tileTextures = [];
        Texture[][] tilePreviewTextures = [];

        //

        logger.Information("loading shaders");

        var tilePreviewShader = LoadShader(
            Path.Combine(assetsDirectory, @"shaders/tile_preview.vs"),
            Path.Combine(assetsDirectory, @"shaders/tile_preview.fs"));

        //

        SetTargetFPS(60);

        logger.Information("begin main loop");

        var titleSize = MeasureText("Henry's Leditor", 60) / 2;
        var poweredBySize = MeasureText("Powered by", 30) / 2;

        float initialFrames = 0;
        //bool initialized = false;

        /*Raylib_CsLo.Texture testTexture = Raylib_CsLo.Raylib.LoadTexture("assets/test/solid.png");
        Vector2[] textureQuads = [ new(1f, 0f), new(0f, 0f), new(0f, 1f), new(1f, 1f), new(1f, 0f) ];
        Vector2[] testQuads = [new(50f, -50f), new(0f, 0f), new(-10f, 50f), new(50f, 50f), new(50f, -50f)];*/

        try
        {
            while (!WindowShouldClose())
            {
                // Splash screen
                if (initialFrames < 120 && settings.Misc.SplashScreen)
                {
                    initialFrames++;

                    var width = GetScreenWidth();
                    var height = GetScreenHeight();

                    BeginDrawing();

                    ClearBackground(new(0, 0, 0, 255));

                    DrawText("Henry's Leditor", width / 2 - titleSize, 200, 60, new(250, 250, 250, 255));

                    DrawText("Powered by", width / 2 - 128, height / 2 - 60, 30, new(250, 250, 250, 255));

                    DrawRectangle(width / 2 - 128, height / 2 - 28, 256, 256, new(250, 250, 250, 255));
                    DrawRectangle(width / 2 - 112, height / 2 - 12, 224, 224, new(0, 0, 0, 255));
                    DrawText("raylib", width / 2 - 44, height / 2 + 148, 50, new(250, 250, 250, 255));

                    DrawText(version, 0, height - 15, 15, new(255, 255, 255, 255));

                    EndDrawing();

                    continue;
                }

                if (settings.Misc.SplashScreen) logger.Debug("splash screen over");

                // Loading screen

                if (!imagesLoaded)
                {
                    int doneCount = 0;
                    int prevDoneCount = 0;
                    int completedProgress = 0;


                    for (int c = 0; c < tileImageCategoriesDone.Length; c++)
                    {
                        if (tileImageCategoriesDone[c].IsCompletedSuccessfully) doneCount++;
                        if (tilePreviewImageCategoriesDone[c].IsCompletedSuccessfully) prevDoneCount++;

                        for (int t = 0; t < tileImagesTasks[c].Length; t++)
                        {
                            if (tileImagesTasks[c][t].IsCompletedSuccessfully) completedProgress++;
                            if (tilePreviewImagesTasks[c][t].IsCompletedSuccessfully) completedProgress++;
                        }
                    }


                    if (doneCount == tileImageCategoriesDone.Length &&
                        prevDoneCount == tilePreviewImageCategoriesDone.Length) imagesLoaded = true;

                    var width = GetScreenWidth();
                    var height = GetScreenHeight();

                    BeginDrawing();
                    ClearBackground(new(0, 0, 0, 255));

                    DrawText("Henry's Leditor", width / 2 - titleSize, 200, 60, new(250, 250, 250, 255));

                    DrawText("Powered by", width / 2 - 128, height / 2 - 60, 30, new(250, 250, 250, 255));

                    DrawRectangle(width / 2 - 128, height / 2 - 28, 256, 256, new(250, 250, 250, 255));
                    DrawRectangle(width / 2 - 112, height / 2 - 12, 224, 224, new(0, 0, 0, 255));
                    DrawText("raylib", width / 2 - 44, height / 2 + 148, 50, new(250, 250, 250, 255));

                    DrawText(version, 0, height - 15, 15, new(255, 255, 255, 255));

                    RayGui.GuiProgressBar(new(100, height - 100, width - 200, 30), "", "", completedProgress, 0, totalTileImageNumber);
                    EndDrawing();

                    continue;
                }
                else if (!texturesLoaded)
                {
                    tileTextures = tileImagesTasks.Select(c => c.Select(t => LoadTextureFromImage(t.Result)).ToArray()).ToArray();
                    tilePreviewTextures = tilePreviewImagesTasks.Select(c => c.Select(t => LoadTextureFromImage(t.Result)).ToArray()).ToArray();

                    texturesLoaded = true;
                    continue;
                }

                switch (page)
                {

                    #region StartPage
                    case 0:
                        prevPage = 0;

                        Raylib.BeginDrawing();
                        {
                            Raylib.ClearBackground(new(170, 170, 170, 255));

                            if (Raylib_CsLo.RayGui.GuiButton(new(Raylib.GetScreenWidth() / 2 - 150, Raylib.GetScreenHeight() / 2 - 40, 300, 40), "Create New Project"))
                            {
                                newFlag = true;
                                page = 6;
                            }

                            if (Raylib_CsLo.RayGui.GuiButton(new(Raylib.GetScreenWidth() / 2 - 150, Raylib.GetScreenHeight() / 2, 300, 40), "Load Project"))
                            {
                                page = 11;
                            }
                        }
                        Raylib.EndDrawing();
                        break;
                    #endregion

                    #region MainPage
                    case 1:

                        prevPage = 1;

                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_TWO))
                        {
                            page = 2;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_THREE))
                        {
                            page = 3;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_FOUR))
                        {
                            page = 4;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_FIVE))
                        {
                            page = 5;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_SIX))
                        {
                            resizeFlag = true;
                            page = 6;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_SEVEN))
                        {
                            page = 7;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_EIGHT))
                        {
                            page = 8;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_NINE))
                        {
                            page = 9;
                        }

                        // handle zoom
                        var mainPageWheel = Raylib.GetMouseWheelMove();
                        if (mainPageWheel != 0)
                        {
                            Vector2 mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), mainPageCamera);
                            mainPageCamera.offset = Raylib.GetMousePosition();
                            mainPageCamera.target = mouseWorldPosition;
                            mainPageCamera.zoom += mainPageWheel * zoomIncrement;
                            if (mainPageCamera.zoom < zoomIncrement) mainPageCamera.zoom = zoomIncrement;
                        }

                        // handle mouse drag
                        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
                        {
                            Vector2 delta = Raylib.GetMouseDelta();
                            delta = RayMath.Vector2Scale(delta, -1.0f / mainPageCamera.zoom);
                            mainPageCamera.target = RayMath.Vector2Add(mainPageCamera.target, delta);
                        }

                        Raylib.BeginDrawing();
                        {
                            Raylib.ClearBackground(new(170, 170, 170, 255));

                            Raylib.BeginMode2D(mainPageCamera);
                            {
                                Raylib.DrawRectangle(0, 0, matrixWidth * scale, matrixHeight * scale, new(255, 255, 255, 255));

                                for (int y = 0; y < matrixHeight; y++)
                                {
                                    for (int x = 0; x < matrixWidth; x++)
                                    {
                                        for (int z = 1; z < 3; z++)
                                        {
                                            var cell = geoMatrix[y, x, z];

                                            var texture = GetBlockIndex(cell.Geo);

                                            if (texture >= 0)
                                            {
                                                Raylib.DrawTexture(geoTextures[texture], x * scale, y * scale, new(0, 0, 0, 170));
                                            }
                                        }
                                    }
                                }

                                if (!waterInFront && waterLevel != -1)
                                {
                                    Raylib.DrawRectangle(
                                        (-1) * scale,
                                        (matrixHeight - waterLevel) * scale,
                                        (matrixWidth + 2) * scale,
                                        waterLevel * scale,
                                        new(0, 0, 255, 250)
                                    );
                                }

                                for (int y = 0; y < matrixHeight; y++)
                                {
                                    for (int x = 0; x < matrixWidth; x++)
                                    {
                                        var cell = geoMatrix[y, x, 0];

                                        var texture = GetBlockIndex(cell.Geo);

                                        if (texture >= 0)
                                        {
                                            Raylib.DrawTexture(geoTextures[texture], x * scale, y * scale, new(0, 0, 0, 225));
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
                                                        Raylib.DrawTexture(stackableTextures[GetStackableTextureIndex(s)], x * scale, y * scale, blackStackable);
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
                                                        Raylib.DrawTexture(stackableTextures[GetStackableTextureIndex(s)], x * scale, y * scale, whiteStackable);
                                                        break;

                                                    // directional placement
                                                    case 4:     // entrance
                                                        var index = GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, 0));

                                                        if (index is 22 or 23 or 24 or 25)
                                                        {
                                                            geoMatrix[y, x, 0].Geo = 7;
                                                        }

                                                        Raylib.DrawTexture(stackableTextures[index], x * scale, y * scale, whiteStackable);
                                                        break;
                                                    case 11:    // crack
                                                        Raylib.DrawTexture(
                                                            stackableTextures[GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, 0))],
                                                            x * scale,
                                                            y * scale,
                                                            blackStackable
                                                        );
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (waterInFront && waterLevel != -1)
                                {
                                    Raylib.DrawRectangle(
                                        (-1) * scale,
                                        (matrixHeight - waterLevel) * scale,
                                        (matrixWidth + 2) * scale,
                                        waterLevel * scale,
                                        new(0, 0, 255, 110)
                                    );
                                }
                            }
                            Raylib.EndMode2D();

                            fixed (byte* pt = previewPanelBytes)
                            {
                                Raylib_CsLo.RayGui.GuiPanel(
                                    new(
                                        Raylib.GetScreenWidth() - 400,
                                        50,
                                        380,
                                        Raylib.GetScreenHeight() - 100
                                    ),
                                    (sbyte*)pt
                                );

                                Raylib.DrawText(
                                    projectName,
                                    Raylib.GetScreenWidth() - 350,
                                    100,
                                    30,
                                    new(0, 0, 0, 255)
                                );

                                var helpPressed = Raylib_CsLo.RayGui.GuiButton(new(
                                    Raylib.GetScreenWidth() - 80,
                                    100,
                                    40,
                                    40
                                ),
                                "?"
                                );

                                if (helpPressed) page = 9;

                                Raylib.DrawText("Seed", Raylib.GetScreenWidth() - 380, 205, 11, new(0, 0, 0, 255));

                                tileSeed = (int)Math.Round(Raylib_CsLo.RayGui.GuiSlider(
                                    new(
                                        Raylib.GetScreenWidth() - 290,
                                        200,
                                        200,
                                        20
                                    ),
                                    "0",
                                    "400",
                                    tileSeed,
                                    0,
                                    400
                                ));

                                lightMode = Raylib_CsLo.RayGui.GuiCheckBox(new(
                                    Raylib.GetScreenWidth() - 380,
                                    250,
                                    20,
                                    20
                                ),
                                "Light Mode",
                                lightMode);

                                defaultTerrain = Raylib_CsLo.RayGui.GuiCheckBox(new(
                                    Raylib.GetScreenWidth() - 380,
                                    290,
                                    20,
                                    20
                                ),
                                "Default Medium",
                                defaultTerrain);

                                Raylib.DrawText("Water Level",
                                    Raylib.GetScreenWidth() - 380,
                                    335,
                                    11,
                                    new(0, 0, 0, 255)
                                );

                                waterLevel = (int)Math.Round(Raylib_CsLo.RayGui.GuiSlider(
                                    new(
                                        Raylib.GetScreenWidth() - 290,
                                        330,
                                        200,
                                        20
                                    ),
                                    "-1",
                                    "999",
                                    waterLevel,
                                    -1,
                                    matrixHeight + 10
                                ));

                                waterInFront = Raylib_CsLo.RayGui.GuiCheckBox(
                                    new(
                                        Raylib.GetScreenWidth() - 380,
                                        360,
                                        20,
                                        20
                                    ),
                                    "Water In Front",
                                    waterInFront
                                );

                                //

                                var savePressed = Raylib_CsLo.RayGui.GuiButton(new(
                                        Raylib.GetScreenWidth() - 390,
                                        Raylib.GetScreenHeight() - 200,
                                        360,
                                        40
                                    ),
                                    "Save Project"
                                );

                                if (savePressed)
                                {
                                    projectNameBufferBytes = Encoding.ASCII.GetBytes(projectName);
                                    page = 12;
                                }

                                var loadPressed = Raylib_CsLo.RayGui.GuiButton(new(
                                        Raylib.GetScreenWidth() - 390,
                                        Raylib.GetScreenHeight() - 150,
                                        360,
                                        40
                                    ),
                                    "Load Project"
                                );

                                var newPressed = Raylib_CsLo.RayGui.GuiButton(new(
                                        Raylib.GetScreenWidth() - 390,
                                        Raylib.GetScreenHeight() - 100,
                                        360,
                                        40
                                    ),
                                    "New Project"
                                );

                                if (loadPressed) page = 11;
                                if (newPressed)
                                {
                                    newFlag = true;
                                    page = 6;
                                }
                            }

                        }
                        Raylib.EndDrawing();

                        break;
                    #endregion

                    #region GeoEditor
                    case 2:
                        prevPage = 2;

                        if (IsKeyPressed(KeyboardKey.KEY_ONE))
                        {
                            page = 1;
                        }
                        // if (IsKeyReleased(KeyboardKey.KEY_TWO)) page = 2;
                        if (IsKeyReleased(KeyboardKey.KEY_THREE))
                        {
                            page = 3;
                        }
                        if (IsKeyReleased(KeyboardKey.KEY_FOUR))
                        {
                            page = 4;
                        }
                        if (IsKeyReleased(KeyboardKey.KEY_FIVE))
                        {
                            page = 5;
                        }
                        if (IsKeyReleased(KeyboardKey.KEY_SIX))
                        {
                            resizeFlag = true;
                            page = 6;
                        }
                        if (IsKeyReleased(KeyboardKey.KEY_SEVEN))
                        {
                            page = 7;
                        }
                        if (IsKeyReleased(KeyboardKey.KEY_EIGHT))
                        {
                            page = 8;
                        }
                        if (IsKeyReleased(KeyboardKey.KEY_NINE))
                        {
                            page = 9;
                        }

                        Vector2 mouse = GetScreenToWorld2D(GetMousePosition(), camera);

                        //                        v this was done to avoid rounding errors
                        int matrixY = mouse.Y < 0 ? -1 : (int)mouse.Y / scale;
                        int matrixX = mouse.X < 0 ? -1 : (int)mouse.X / scale;

                        var canDrawGeo = !Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), new(Raylib.GetScreenWidth() - 210, 50, 200, Raylib.GetScreenHeight() - 100));

                        // handle geo selection

                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_D))
                        {
                            geoSelectionX = ++geoSelectionX % 4;

                            multiselect = false;
                            prevCoordsX = -1;
                            prevCoordsY = -1;
                        }
                        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_A))
                        {
                            geoSelectionX = --geoSelectionX % 4;

                            multiselect = false;
                            prevCoordsX = -1;
                            prevCoordsY = -1;
                        }
                        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_W))
                        {
                            geoSelectionY = (--geoSelectionY) % 8;

                            multiselect = false;
                            prevCoordsX = -1;
                            prevCoordsY = -1;
                        }
                        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
                        {
                            geoSelectionY = (++geoSelectionY) % 8;

                            multiselect = false;
                            prevCoordsX = -1;
                            prevCoordsY = -1;
                        }

                        // handle changing layers

                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_L))
                        {
                            currentLayer = ++currentLayer % 3;
                        }

                        uint geoIndex = (4 * geoSelectionY) + geoSelectionX;

                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_M))
                        {
                            gridContrast = !gridContrast;
                        }

                        // handle mouse drag
                        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
                        {
                            Vector2 delta = Raylib.GetMouseDelta();
                            delta = RayMath.Vector2Scale(delta, -1.0f / camera.zoom);
                            camera.target = RayMath.Vector2Add(camera.target, delta);
                        }


                        // handle zoom
                        var wheel = Raylib.GetMouseWheelMove();
                        if (wheel != 0)
                        {
                            Vector2 mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
                            camera.offset = Raylib.GetMousePosition();
                            camera.target = mouseWorldPosition;
                            camera.zoom += wheel * zoomIncrement;
                            if (camera.zoom < zoomIncrement) camera.zoom = zoomIncrement;
                        }

                        // handle placing geo

                        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                        {
                            if (canDrawGeo && matrixY >= 0 && matrixY < matrixHeight && matrixX >= 0 && matrixX < matrixWidth)
                            {
                                switch (geoIndex)
                                {
                                    case 2: // slopebl
                                        var cell = geoMatrix[matrixY, matrixX, currentLayer];

                                        var slope = GetCorrectSlopeID(CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, matrixX, matrixY, currentLayer));

                                        if (slope == -1) break;

                                        cell.Geo = slope;
                                        geoMatrix[matrixY, matrixX, currentLayer] = cell;
                                        break;

                                    case 0: // solid
                                    case 1: // air
                                    case 5: // platform
                                    case 12: // glass
                                        var cell2 = geoMatrix[matrixY, matrixX, currentLayer];

                                        cell2.Geo = GetBlockID(geoIndex);
                                        geoMatrix[matrixY, matrixX, currentLayer] = cell2;
                                        break;

                                    // multi-select: forward to next if-statement
                                    case 3:
                                    case 4:
                                    case 13:
                                    case 25:
                                        break;

                                    // stackables
                                    case 7: // rock
                                    case 8: // spear
                                    case 9: // crack
                                    case 10: // ph
                                    case 11: // pv
                                    case 14: // entry
                                    case 15: // shortcut
                                    case 16: // den
                                    case 17: // passage
                                    case 18: // bathive
                                    case 19: // waterfall
                                    case 20: // scav
                                    case 21: // wac
                                    case 22: // garbageworm
                                    case 23: // worm
                                    case 24: // forbidflychains
                                        if (geoIndex is 17 or 18 or 19 or 20 or 22 or 23 or 24 or 25 or 26 or 27 && currentLayer != 0)
                                        {
                                            break;
                                        }

                                        if (
                                            matrixX * scale < border.X ||
                                            matrixX * scale >= border.width + border.X ||
                                            matrixY * scale < border.Y ||
                                            matrixY * scale >= border.height + border.Y)
                                        {
                                            break;
                                        }

                                        var id = GetStackableID(geoIndex);
                                        var cell_ = geoMatrix[matrixY, matrixX, currentLayer];

                                        var newValue = !cell_.Stackables[id];
                                        if (matrixX != prevMatrixX || matrixY != prevMatrixY || !clickTracker)
                                        {

                                            if (cell_.Stackables[id] != newValue)
                                            {
                                                cell_.Stackables[id] = newValue;
                                                if (id == 4) { cell_.Geo = 0; }
                                                geoMatrix[matrixY, matrixX, currentLayer] = cell_;

                                            }
                                        }

                                        prevMatrixX = matrixX;
                                        prevMatrixY = matrixY;
                                        break;
                                }
                            }

                            clickTracker = true;
                        }

                        if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
                        {
                            clickTracker = false;
                        }

                        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                        {
                            if (canDrawGeo && matrixY >= 0 && matrixY < matrixHeight && matrixX >= 0 && matrixX < matrixWidth)
                            {
                                switch (geoIndex)
                                {
                                    case 25:
                                        multiselect = !multiselect;

                                        if (multiselect)
                                        {
                                            prevCoordsX = matrixX;
                                            prevCoordsY = matrixY;
                                        }
                                        else
                                        {
                                            int startX,
                                                startY,
                                                endX,
                                                endY;

                                            if (matrixX > prevCoordsX)
                                            {
                                                startX = prevCoordsX;
                                                endX = matrixX;
                                            }
                                            else
                                            {
                                                startX = matrixX;
                                                endX = prevCoordsX;
                                            }

                                            if (matrixY > prevCoordsY)
                                            {
                                                startY = prevCoordsY;
                                                endY = matrixY;
                                            }
                                            else
                                            {
                                                startY = matrixY;
                                                endY = prevCoordsY;
                                            }

                                            for (int y = startY; y <= endY; y++)
                                            {
                                                for (int x = startX; x <= endX; x++)
                                                {
                                                    geoMatrix[y, x, 0].Geo = 0;
                                                    geoMatrix[y, x, 1].Geo = 0;
                                                    geoMatrix[y, x, 2].Geo = 0;
                                                }
                                            }
                                        }
                                        break;
                                    case 13:
                                        multiselect = !multiselect;

                                        if (multiselect)
                                        {
                                            prevCoordsX = matrixX;
                                            prevCoordsY = matrixY;
                                        }
                                        else
                                        {
                                            int startX,
                                                startY,
                                                endX,
                                                endY;

                                            if (matrixX > prevCoordsX)
                                            {
                                                startX = prevCoordsX;
                                                endX = matrixX;
                                            }
                                            else
                                            {
                                                startX = matrixX;
                                                endX = prevCoordsX;
                                            }

                                            if (matrixY > prevCoordsY)
                                            {
                                                startY = prevCoordsY;
                                                endY = matrixY;
                                            }
                                            else
                                            {
                                                startY = matrixY;
                                                endY = prevCoordsY;
                                            }

                                            if (currentLayer is 0 or 1)
                                            {
                                                for (int y = startY; y <= endY; y++)
                                                {
                                                    for (int x = startX; x <= endX; x++)
                                                    {
                                                        geoMatrix[y, x, currentLayer + 1].Geo = geoMatrix[y, x, currentLayer].Geo;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case 3:
                                    case 4:
                                        multiselect = !multiselect;

                                        if (multiselect)
                                        {
                                            prevCoordsX = matrixX;
                                            prevCoordsY = matrixY;
                                        }
                                        else
                                        {
                                            int startX,
                                                startY,
                                                endX,
                                                endY;

                                            if (matrixX > prevCoordsX)
                                            {
                                                startX = prevCoordsX;
                                                endX = matrixX;
                                            }
                                            else
                                            {
                                                startX = matrixX;
                                                endX = prevCoordsX;
                                            }

                                            if (matrixY > prevCoordsY)
                                            {
                                                startY = prevCoordsY;
                                                endY = matrixY;
                                            }
                                            else
                                            {
                                                startY = matrixY;
                                                endY = prevCoordsY;
                                            }

                                            int value = geoIndex == 3 ? 1 : 0;

                                            for (int y = startY; y <= endY; y++)
                                            {
                                                for (int x = startX; x <= endX; x++)
                                                {
                                                    var cell = geoMatrix[y, x, currentLayer];
                                                    cell.Geo = value;
                                                    geoMatrix[y, x, currentLayer] = cell;
                                                }
                                            }

                                            prevCoordsX = -1;
                                            prevCoordsY = -1;
                                        }
                                        break;
                                }
                            }
                        }

                        BeginDrawing();
                        {
                            ClearBackground(new Color(120, 120, 120, 255));


                            BeginMode2D(camera);
                            {
                                // geo matrix

                                // first layer without stackables

                                if (showLayer1)
                                {
                                    for (int y = 0; y < matrixHeight; y++)
                                    {
                                        for (int x = 0; x < matrixWidth; x++)
                                        {
                                            const int z = 0;

                                            var cell = geoMatrix[y, x, z];

                                            var texture = GetBlockIndex(cell.Geo);

                                            if (texture >= 0)
                                            {
                                                DrawTexture(geoTextures[texture], x * scale, y * scale, layerColors[z]);
                                            }

                                            if (!gridContrast) DrawRectangleLinesEx(
                                                new(x * scale, y * scale, scale, scale),
                                                0.5f,
                                                new(255, 255, 255, 100)
                                            );
                                        }
                                    }
                                }

                                /*if (gridContrast)
                                {
                                    // the grid
                                    RlGl.rlPushMatrix();
                                    {
                                        RlGl.rlTranslatef(0, 0, -1);
                                        RlGl.rlRotatef(90, 1, 0, 0);
                                        DrawGrid(matrixHeight > matrixWidth ? matrixHeight : matrixWidth, scale);
                                    }
                                    RlGl.rlPopMatrix();
                                }*/

                                // the rest of the layers

                                for (int y = 0; y < matrixHeight; y++)
                                {
                                    for (int x = 0; x < matrixWidth; x++)
                                    {
                                        for (int z = 1; z < 3; z++)
                                        {
                                            if (z == 1 && !showLayer2) continue;
                                            if (z == 2 && !showLayer3) continue;

                                            var cell = geoMatrix[y, x, z];

                                            var texture = GetBlockIndex(cell.Geo);

                                            if (texture >= 0)
                                            {
                                                Raylib.DrawTexture(geoTextures[texture], x * scale, y * scale, layerColors[z]);
                                            }

                                            if (!gridContrast && !showLayer1 && z == 1) DrawRectangleLinesEx(
                                                new(x * scale, y * scale, scale, scale),
                                                0.5f,
                                                new(255, 255, 255, 100)
                                            );

                                            if (!gridContrast && !showLayer1 && z == 2) DrawRectangleLinesEx(
                                                new(x * scale, y * scale, scale, scale),
                                                0.5f,
                                                new(255, 255, 255, 100)
                                            );

                                            for (int s = 1; s < cell.Stackables.Length; s++)
                                            {
                                                if (cell.Stackables[s])
                                                {
                                                    switch (s)
                                                    {
                                                        // dump placement
                                                        case 1:     // ph
                                                        case 2:     // pv
                                                            Raylib.DrawTexture(stackableTextures[GetStackableTextureIndex(s)], x * scale, y * scale, layerColors[z]);
                                                            break;
                                                        case 3:     // bathive
                                                            /*case 5:     // entrance
                                                            case 6:     // passage
                                                            case 7:     // den
                                                            case 9:     // rock
                                                            case 10:    // spear
                                                            case 12:    // forbidflychains
                                                            case 13:    // garbagewormhole
                                                            case 18:    // waterfall
                                                            case 19:    // wac
                                                            case 20:    // worm
                                                            case 21:    // scav*/
                                                            Raylib.DrawTexture(stackableTextures[GetStackableTextureIndex(s)], x * scale, y * scale, whiteStackable); // TODO: remove opacity from entrances
                                                            break;

                                                        // directional placement
                                                        /*case 4:     // entrance
                                                            var index = GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, z));

                                                            if (index is 22 or 23 or 24 or 25)
                                                            {
                                                                geoMatrix[y, x, 0].Geo = 7;
                                                            }

                                                            Raylib.DrawTexture(stackableTextures[index], x * scale, y * scale, whiteStackable);
                                                            break;*/
                                                        case 11:    // crack
                                                            Raylib.DrawTexture(
                                                                stackableTextures[GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, z))],
                                                                x * scale,
                                                                y * scale,
                                                                whiteStackable
                                                            );
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                // draw stackables

                                if (showLayer1)
                                {
                                    for (int y = 0; y < matrixHeight; y++)
                                    {
                                        for (int x = 0; x < matrixWidth; x++)
                                        {
                                            const int z = 0;

                                            var cell = geoMatrix[y, x, z];

                                            for (int s = 1; s < cell.Stackables.Length; s++)
                                            {
                                                if (cell.Stackables[s])
                                                {
                                                    switch (s)
                                                    {
                                                        // dump placement
                                                        case 1:     // ph
                                                        case 2:     // pv
                                                            Raylib.DrawTexture(stackableTextures[GetStackableTextureIndex(s)], x * scale, y * scale, layerColors[z]);
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
                                                            Raylib.DrawTexture(stackableTextures[GetStackableTextureIndex(s)], x * scale, y * scale, new(255, 255, 255, 255)); // TODO: remove opacity from entrances
                                                            break;

                                                        // directional placement
                                                        case 4:     // entrance
                                                            var index = GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, z));

                                                            if (index is 22 or 23 or 24 or 25)
                                                            {
                                                                geoMatrix[y, x, 0].Geo = 7;
                                                            }

                                                            Raylib.DrawTexture(stackableTextures[index], x * scale, y * scale, new(255, 255, 255, 255));
                                                            break;
                                                        case 11:    // crack
                                                            Raylib.DrawTexture(
                                                                stackableTextures[GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, z))],
                                                                x * scale,
                                                                y * scale,
                                                                new(255, 255, 255, 255)
                                                            );
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }


                                // the red selection rectangle

                                for (int y = 0; y < matrixHeight; y++)
                                {
                                    for (int x = 0; x < matrixWidth; x++)
                                    {
                                        if (multiselect)
                                        {
                                            var XS = matrixX - prevCoordsX;
                                            var YS = matrixY - prevCoordsY;
                                            var width = Math.Abs(XS == 0 ? 1 : XS + (XS > 0 ? 1 : -1)) * scale;
                                            var height = Math.Abs(YS == 0 ? 1 : YS + (YS > 0 ? 1 : -1)) * scale;

                                            Rectangle rec = (XS >= 0, YS >= 0) switch
                                            {
                                                // br
                                                (true, true) => new(prevCoordsX * scale, prevCoordsY * scale, width, height),

                                                // tr
                                                (true, false) => new(prevCoordsX * scale, matrixY * scale, width, height),

                                                // bl
                                                (false, true) => new(matrixX * scale, prevCoordsY * scale, width, height),

                                                // tl
                                                (false, false) => new(matrixX * scale, matrixY * scale, width, height)
                                            };

                                            Raylib.DrawRectangleLinesEx(rec, 2, new(255, 0, 0, 255));

                                            Raylib.DrawText(
                                                $"{width / scale:0}x{height / scale:0}",
                                                (int)mouse.X + 10,
                                                (int)mouse.Y,
                                                4,
                                                new(255, 255, 255, 255)
                                                );
                                        }
                                        else
                                        {
                                            if (matrixX == x && matrixY == y)
                                            {
                                                Raylib.DrawRectangleLinesEx(new(x * scale, y * scale, scale, scale), 2, new(255, 0, 0, 255));
                                            }
                                        }
                                    }
                                }

                                // the outbound border
                                Raylib.DrawRectangleLinesEx(new(0, 0, matrixWidth * scale, matrixHeight * scale), 2, new(0, 0, 0, 255));

                                // the border
                                Raylib.DrawRectangleLinesEx(border, camera.zoom < zoomIncrement ? 5 : 2, new(255, 255, 255, 255));

                                // a lazy way to hide the rest of the grid
                                Raylib.DrawRectangle(matrixWidth * -scale, -3, matrixWidth * scale, matrixHeight * 2 * scale, new(120, 120, 120, 255));
                                Raylib.DrawRectangle(0, matrixHeight * scale, matrixWidth * scale + 2, matrixHeight * scale, new(120, 120, 120, 255));
                            }
                            Raylib.EndMode2D();

                            // geo menu

                            fixed (byte* pt = geoMenuPanelBytes)
                            {
                                Raylib_CsLo.RayGui.GuiPanel(
                                    new(Raylib.GetScreenWidth() - 210, 50, 200, Raylib.GetScreenHeight() - 100),
                                    (sbyte*)pt
                                );
                            }

                            for (int w = 0; w < 4; w++)
                            {
                                for (int h = 0; h < 8; h++)
                                {
                                    var index = (4 * h) + w;
                                    if (index < uiTextures.Length)
                                    {
                                        Raylib.DrawTexture(
                                            uiTextures[index],
                                            Raylib.GetScreenWidth() - 195 + w * uiScale + 5,
                                            h * uiScale + 100,
                                            new(0, 0, 0, 255)
                                        );
                                    }

                                    if (w == geoSelectionX && h == geoSelectionY)
                                        Raylib.DrawRectangleLinesEx(new(Raylib.GetScreenWidth() - 195 + w * uiScale + 5, h * uiScale + 100, uiScale, uiScale), 2, new(255, 0, 0, 255));
                                    else
                                        Raylib.DrawRectangleLinesEx(new(Raylib.GetScreenWidth() - 195 + w * uiScale + 5, h * uiScale + 100, uiScale, uiScale), 1, new(0, 0, 0, 255));
                                }
                            }

                            if (geoIndex < uiTextures.Length) Raylib.DrawText(geoNames[geoIndex], Raylib.GetScreenWidth() - 190, 8 * uiScale + 110, 18, new(0, 0, 0, 255));

                            switch (currentLayer)
                            {
                                case 0:
                                    Raylib.DrawRectangle(Raylib.GetScreenWidth() - 190, 8 * uiScale + 140, 40, 40, new(0, 0, 0, 255));
                                    Raylib.DrawText("L1", Raylib.GetScreenWidth() - 182, 8 * uiScale + 148, 26, new(255, 255, 255, 255));
                                    break;
                                case 1:
                                    Raylib.DrawRectangle(Raylib.GetScreenWidth() - 190, 8 * uiScale + 140, 40, 40, new(0, 255, 0, 255));
                                    Raylib.DrawText("L2", Raylib.GetScreenWidth() - 182, 8 * uiScale + 148, 26, new(255, 255, 255, 255));
                                    break;
                                case 2:
                                    Raylib.DrawRectangle(Raylib.GetScreenWidth() - 190, 8 * uiScale + 140, 40, 40, new(255, 0, 0, 255));
                                    Raylib.DrawText("L3", Raylib.GetScreenWidth() - 182, 8 * uiScale + 148, 26, new(255, 255, 255, 255));
                                    break;
                            }

                            if (matrixX >= 0 && matrixX < matrixWidth && matrixY >= 0 && matrixY < matrixHeight)
                                Raylib.DrawText(
                                    $"X = {matrixX:0}\nY = {matrixY:0}",
                                    Raylib.GetScreenWidth() - 195,
                                    Raylib.GetScreenHeight() - 100,
                                    12,
                                    new(0, 0, 0, 255));

                            else Raylib.DrawText(
                                    $"X = -\nY = -",
                                    Raylib.GetScreenWidth() - 195,
                                    Raylib.GetScreenHeight() - 100,
                                    12,
                                    new(0, 0, 0, 255));

                            showLayer1 = Raylib_CsLo.RayGui.GuiCheckBox(
                                new(Raylib.GetScreenWidth() - 190, 8 * uiScale + 190, 20, 20),
                                "Layer 1",
                                showLayer1
                            );

                            showLayer2 = Raylib_CsLo.RayGui.GuiCheckBox(
                                new(Raylib.GetScreenWidth() - 190, 8 * uiScale + 210, 20, 20),
                                "Layer 2",
                                showLayer2
                            );

                            showLayer3 = Raylib_CsLo.RayGui.GuiCheckBox(
                                new(Raylib.GetScreenWidth() - 190, 8 * uiScale + 230, 20, 20),
                                "Layer 3",
                                showLayer3
                            );
                        }
                        Raylib.EndDrawing();

                        break;
                    #endregion

                    #region TileEditor
                    case 3:
                        {
                            page = 3;

                            var teWidth = GetScreenWidth();
                            var teHeight = GetScreenHeight();

                            var tileMouseWorld = GetScreenToWorld2D(GetMousePosition(), tileCamera);
                            var tileMouse = GetMousePosition();
                            var tilePanelRect = new Rectangle(teWidth - (tilePanelWidth + 10), 50, tilePanelWidth, teHeight - 100);
                            var leftPanelSideStart = new Vector2(teWidth - (tilePanelWidth + 10), 50);
                            var leftPanelSideEnd = new Vector2(teWidth - (tilePanelWidth + 10), teHeight - 50);
                            var specsRect = showTileSpecs ? new Rectangle(0, GetScreenHeight() - 300, 300, 300) :
                                new(-276, GetScreenHeight() - 300, 300, 300);
                            bool canDrawTile = !CheckCollisionPointRec(tileMouse, tilePanelRect) && !CheckCollisionPointRec(tileMouse, specsRect);

                            var categoriesPageSize = (int)tilePanelRect.height / 30;
                            var currentTileInit = initTiles[tileCategoryIndex][tileIndex];


                            #region TileEditorShortcuts

                            if (IsKeyPressed(KeyboardKey.KEY_ONE))
                            {
                                page = 1;
                            }
                            if (IsKeyPressed(KeyboardKey.KEY_TWO))
                            {
                                page = 2;
                            }

                            // if (Raylib.IsKeyPressed(KeyboardKey.KEY_THREE))
                            // {
                            //     page = 3;
                            // }

                            if (IsKeyPressed(KeyboardKey.KEY_FOUR))
                            {
                                page = 4;
                            }
                            if (IsKeyPressed(KeyboardKey.KEY_FIVE))
                            {
                                page = 5;
                            }
                            if (IsKeyPressed(KeyboardKey.KEY_SIX))
                            {
                                resizeFlag = true;
                                page = 6;
                            }
                            if (IsKeyPressed(KeyboardKey.KEY_SEVEN))
                            {
                                page = 7;
                            }
                            if (IsKeyPressed(KeyboardKey.KEY_EIGHT))
                            {
                                page = 8;
                            }
                            if (IsKeyPressed(KeyboardKey.KEY_NINE))
                            {
                                page = 9;
                            }

                            if (canDrawTile || clickTracker)
                            {
                                // handle mouse drag
                                if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
                                {
                                    clickTracker = true;
                                    Vector2 delta = GetMouseDelta();
                                    delta = RayMath.Vector2Scale(delta, -1.0f / tileCamera.zoom);
                                    tileCamera.target = RayMath.Vector2Add(tileCamera.target, delta);
                                }

                                if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_RIGHT)) clickTracker = false;

                                // handle zoom
                                var tileWheel = Raylib.GetMouseWheelMove();
                                if (tileWheel != 0)
                                {
                                    Vector2 mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), tileCamera);
                                    tileCamera.offset = Raylib.GetMousePosition();
                                    tileCamera.target = mouseWorldPosition;
                                    tileCamera.zoom += tileWheel * zoomIncrement;
                                    if (tileCamera.zoom < zoomIncrement) tileCamera.zoom = zoomIncrement;
                                }
                            }


                            if (Raylib.IsKeyPressed(KeyboardKey.KEY_L))
                            {
                                currentLayer++;

                                if (currentLayer > 2) currentLayer = 0;
                            }

                            // handle resizing tile panel

                            if (((tileMouse.X <= leftPanelSideStart.X + 5 && tileMouse.X >= leftPanelSideStart.X - 5 && tileMouse.Y >= leftPanelSideStart.Y && tileMouse.Y <= leftPanelSideEnd.Y) || clickTracker) &&
                            Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                            {
                                clickTracker = true;

                                var delta = Raylib.GetMouseDelta();

                                if (tilePanelWidth - (int)delta.X >= 400 && tilePanelWidth - (int)delta.X <= 700)
                                {
                                    tilePanelWidth -= (int)delta.X;
                                }
                            }

                            if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
                            {
                                clickTracker = false;
                            }

                            // change focus

                            if (Raylib.IsKeyPressed(KeyboardKey.KEY_D))
                            {
                                tileCategoryFocus = false;
                            }

                            if (Raylib.IsKeyPressed(KeyboardKey.KEY_A))
                            {
                                tileCategoryFocus = true;
                            }

                            // change tile category

                            if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
                            {
                                if (tileCategoryFocus)
                                {
                                    tileCategoryIndex = ++tileCategoryIndex % tileCategories.Length;

                                    if (tileCategoryIndex % (categoriesPageSize + tileCategoryScrollIndex) == categoriesPageSize + tileCategoryScrollIndex - 1
                                        && tileCategoryIndex != tileCategories.Length - 1)
                                        tileCategoryScrollIndex++;

                                    if (tileCategoryIndex == 0)
                                    {
                                        tileCategoryScrollIndex = 0;
                                    }

                                    tileIndex = 0;
                                }
                                else
                                {
                                    tileIndex = ++tileIndex % initTiles[tileCategoryIndex].Length;

                                    if (
                                        tileIndex % (categoriesPageSize + tileScrollIndex) == categoriesPageSize + tileScrollIndex - 1 &&
                                        tileIndex != initTiles[tileCategoryIndex].Length - 1) tileScrollIndex++;

                                    if (tileIndex == 0) tileScrollIndex = 0;
                                }
                            }

                            if (Raylib.IsKeyPressed(KeyboardKey.KEY_W))
                            {
                                if (tileCategoryFocus)
                                {

                                    tileCategoryIndex--;

                                    if (tileCategoryIndex < 0)
                                    {
                                        tileCategoryIndex = tileCategories.Length - 1;
                                    }

                                    if (tileCategoryIndex == (tileCategoryScrollIndex + 1) && tileCategoryIndex != 1) tileCategoryScrollIndex--;
                                    if (tileCategoryIndex == tileCategories.Length - 1)
                                    {
                                        tileCategoryScrollIndex += Math.Abs(tileCategories.Length - categoriesPageSize);
                                    }
                                    tileIndex = 0;
                                }
                                else
                                {
                                    if (tileIndex == (tileScrollIndex) && tileIndex != 1) tileScrollIndex--;

                                    tileIndex--;
                                    if (tileIndex < 0) tileIndex = initTiles[tileCategoryIndex].Length - 1;

                                    if (tileIndex == initTiles[tileCategoryIndex].Length - 1) tileScrollIndex += initTiles[tileCategoryIndex].Length - categoriesPageSize;
                                }
                            }

                            if (IsKeyPressed(KeyboardKey.KEY_T)) showTileSpecs = !showTileSpecs;

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


                            var currentTilePreviewColor = tileCategories[tileCategoryIndex].Item2;
                            Texture currentTileTexture = tilePreviewTextures[tileCategoryIndex][tileIndex];
                            var uniformLoc = Raylib.GetShaderLocation(tilePreviewShader, "inputTexture");
                            var colorLoc = Raylib.GetShaderLocation(tilePreviewShader, "highlightColor");

                            #endregion

                            #region TileEditorDrawing
                            BeginDrawing();

                            ClearBackground(new(170, 170, 170, 255));

                            BeginMode2D(tileCamera);
                            {
                                DrawRectangle(0, 0, matrixWidth * previewScale, matrixHeight * previewScale, new Color(255, 255, 255, 255));

                                #region TileEditorGeoMatrix
                                //                        v this was done to avoid rounding errors
                                int tileMatrixY = tileMouseWorld.Y < 0 ? -1 : (int)tileMouseWorld.Y / previewScale;
                                int tileMatrixX = tileMouseWorld.X < 0 ? -1 : (int)tileMouseWorld.X / previewScale;

                                // Draw geos first
                                if (showTileLayer3)
                                {
                                    #region TileEditorLayer3

                                    for (int y = 0; y < matrixHeight; y++)
                                    {
                                        for (int x = 0; x < matrixWidth; x++)
                                        {
                                            const int z = 2;

                                            if (!showLayer3) continue;

                                            var cell = geoMatrix[y, x, z];

                                            var texture = GetBlockIndex(cell.Geo);

                                            if (texture >= 0)
                                            {
                                                if (z == currentLayer)
                                                {

                                                    Raylib.DrawTexturePro(
                                                        geoTextures[texture],
                                                        new(0, 0, 20, 20),
                                                        new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                        new(0, 0),
                                                        0,
                                                        new(0, 0, 0, 255));
                                                }
                                                else
                                                {

                                                    Raylib.DrawTexturePro(
                                                        geoTextures[texture],
                                                        new(0, 0, 20, 20),
                                                        new(x * previewScale, y * previewScale, previewScale, previewScale),
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

                                                            if (z == currentLayer)
                                                            {

                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s)],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                                    new(0, 0),
                                                                    0,
                                                                    new(0, 0, 0, 255)
                                                                );
                                                            }
                                                            else
                                                            {
                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s)],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                                    new(0, 0),
                                                                    0,
                                                                    new(0, 0, 0, 170)
                                                                );
                                                            }
                                                            break;
                                                        case 3:     // bathive
                                                            /*case 5:     // entrance
                                                            case 6:     // passage
                                                            case 7:     // den
                                                            case 9:     // rock
                                                            case 10:    // spear
                                                            case 12:    // forbidflychains
                                                            case 13:    // garbagewormhole
                                                            case 18:    // waterfall
                                                            case 19:    // wac
                                                            case 20:    // worm
                                                            case 21:*/    // scav

                                                            if (z == currentLayer)
                                                            {

                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s)],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                                    new(0, 0),
                                                                    0,
                                                                    new(255, 255, 255, 255)
                                                                ); // TODO: remove opacity from entrances
                                                            }
                                                            else
                                                            {
                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s)],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                                    new(0, 0),
                                                                    0,
                                                                    new(255, 255, 255, 170)
                                                                );
                                                            }
                                                            break;

                                                        // directional placement
                                                        /*case 4:     // entrance
                                                            var index = GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, z));

                                                            if (index is 22 or 23 or 24 or 25)
                                                            {
                                                                geoMatrix[y, x, 0].Geo = 7;
                                                            }

                                                            if (z == currentLayer) {

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
                                                            if (z == currentLayer)
                                                            {
                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, z))],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                                    new(0, 0),
                                                                    0,
                                                                    new(255, 255, 255, 255)
                                                                );
                                                            }
                                                            else
                                                            {
                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, z))],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
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
                                        for (int y = 0; y < matrixHeight; y++)
                                        {
                                            for (int x = 0; x < matrixWidth; x++)
                                            {
                                                const int z = 2;
                                                var tileCell = tileMatrix[y, x, z];

                                                if (tileCell.Type == TileType.TileHead)
                                                {
                                                    (int, int, string) tile = ((int, int, string))tileCell.Data.CategoryPostition;

                                                    var category = tilePreviewTextures[tile.Item1 - 5];
                                                    var tileTexture = category[tile.Item2 - 1];
                                                    var color = tileCategories[tile.Item1 - 5].Item2;
                                                    var initTile = initTiles[tile.Item1 - 5][tile.Item2 - 1];
                                                    var (tileWidth, tileHeight) = initTile.Size;



                                                    BeginShaderMode(tilePreviewShader);
                                                    SetShaderValueTexture(tilePreviewShader, uniformLoc, tileTexture);

                                                    SetShaderValue(
                                                        tilePreviewShader,
                                                        colorLoc,
                                                        new Vector4(color.r, color.g, color.b, 1),
                                                        ShaderUniformDataType.SHADER_UNIFORM_VEC4
                                                    );
                                                    DrawTexturePro(
                                                        tileTexture,
                                                        new(0, 0, tileTexture.width, tileTexture.height),
                                                        new(x * 16, y * 16, tileTexture.width, tileTexture.height),
                                                        // tileWidth > 2 && tileHeight > 2 ? new(16, 16) : new(0, 0),
                                                        RayMath.Vector2Scale(GetTileHeadOrigin(initTile), previewScale),
                                                        0,
                                                        z == currentLayer ? new(255, 255, 255, 255) : new(170, 170, 170, 120)
                                                    );
                                                    EndShaderMode();
                                                }
                                                else if (tileCell.Type == TileType.Material)
                                                {
                                                    var materialName = ((TileMaterial)tileCell.Data).Name;
                                                    var origin = new Vector2(x * previewScale + 5, y * previewScale + 5);
                                                    var color = materialColorMatrix[y, x, 0];

                                                    if (z != currentLayer) color.a = 120;

                                                    if (color.r != 0 || color.g != 0 || color.b != 0)
                                                    {

                                                        switch (geoMatrix[y, x, 2].Geo)
                                                        {
                                                            case 1:
                                                                DrawRectangle(
                                                                    x * previewScale + 5,
                                                                    y * previewScale + 5,
                                                                    6,
                                                                    6,
                                                                    color
                                                                );
                                                                break;


                                                            case 2:
                                                                DrawTriangle(
                                                                    origin,
                                                                    new(origin.X, origin.Y + previewScale - 10),
                                                                    new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                                    color
                                                                );
                                                                break;


                                                            case 3:
                                                                DrawTriangle(
                                                                    new(origin.X + previewScale - 10, origin.Y),
                                                                    new(origin.X, origin.Y + previewScale - 10),
                                                                    new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                                    color
                                                                );
                                                                break;

                                                            case 4:
                                                                DrawTriangle(
                                                                    origin,
                                                                    new(origin.X, origin.Y + previewScale - 10),
                                                                    new(origin.X + previewScale - 10, origin.Y),
                                                                    color
                                                                );
                                                                break;

                                                            case 5:
                                                                DrawTriangle(
                                                                    origin,
                                                                    new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                                    new(origin.X + previewScale - 10, origin.Y),
                                                                    color
                                                                );
                                                                break;

                                                            case 6:
                                                                DrawRectangleV(
                                                                    origin,
                                                                    new(previewScale - 10, (previewScale - 10) / 2),
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
                                    if (currentLayer != 2) DrawRectangle(0, 0, matrixWidth * previewScale, matrixHeight * previewScale, new(90, 90, 90, 120));

                                    for (int y = 0; y < matrixHeight; y++)
                                    {
                                        for (int x = 0; x < matrixWidth; x++)
                                        {
                                            const int z = 1;

                                            if (!showLayer2) continue;

                                            var cell = geoMatrix[y, x, z];

                                            var texture = GetBlockIndex(cell.Geo);

                                            if (texture >= 0)
                                            {
                                                if (z == currentLayer)
                                                {

                                                    Raylib.DrawTexturePro(
                                                        geoTextures[texture],
                                                        new(0, 0, 20, 20),
                                                        new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                        new(0, 0),
                                                        0,
                                                        new(0, 0, 0, 255));
                                                }
                                                else
                                                {

                                                    Raylib.DrawTexturePro(
                                                        geoTextures[texture],
                                                        new(0, 0, 20, 20),
                                                        new(x * previewScale, y * previewScale, previewScale, previewScale),
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

                                                            if (z == currentLayer)
                                                            {

                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s)],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                                    new(0, 0),
                                                                    0,
                                                                    new(0, 0, 0, 255)
                                                                );
                                                            }
                                                            else
                                                            {
                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s)],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
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

                                                            if (z == currentLayer)
                                                            {

                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s)],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                                    new(0, 0),
                                                                    0,
                                                                    new(255, 255, 255, 255)
                                                                ); // TODO: remove opacity from entrances
                                                            }
                                                            else
                                                            {
                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s)],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                                    new(0, 0),
                                                                    0,
                                                                    new(255, 255, 255, 170)
                                                                );
                                                            }
                                                            break;

                                                        // directional placement
                                                        /*case 4:     // entrance
                                                            var index = GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, z));

                                                            if (index is 22 or 23 or 24 or 25)
                                                            {
                                                                geoMatrix[y, x, 0].Geo = 7;
                                                            }

                                                            if (z == currentLayer) {

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
                                                            if (z == currentLayer)
                                                            {
                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, z))],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                                    new(0, 0),
                                                                    0,
                                                                    new(255, 255, 255, 255)
                                                                );
                                                            }
                                                            else
                                                            {
                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, z))],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
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
                                        for (int y = 0; y < matrixHeight; y++)
                                        {
                                            for (int x = 0; x < matrixWidth; x++)
                                            {
                                                const int z = 1;

                                                if (!showLayer2) continue;

                                                var tileCell = tileMatrix[y, x, z];

                                                if (tileCell.Type == TileType.TileHead)
                                                {
                                                    (int, int, string) tile = ((int, int, string))tileCell.Data.CategoryPostition;

                                                    var category = tilePreviewTextures[tile.Item1 - 5];
                                                    var tileTexture = category[tile.Item2 - 1];
                                                    var color = tileCategories[tile.Item1 - 5].Item2;
                                                    var initTile = initTiles[tile.Item1 - 5][tile.Item2 - 1];
                                                    var (tileWidth, tileHeight) = initTile.Size;



                                                    Raylib.BeginShaderMode(tilePreviewShader);
                                                    Raylib.SetShaderValueTexture(tilePreviewShader, uniformLoc, tileTexture);

                                                    Raylib.SetShaderValue(
                                                        tilePreviewShader,
                                                        colorLoc,
                                                        new Vector4(color.r, color.g, color.b, 1),
                                                        ShaderUniformDataType.SHADER_UNIFORM_VEC4
                                                    );
                                                    Raylib.DrawTexturePro(
                                                        tileTexture,
                                                        new(0, 0, tileTexture.width, tileTexture.height),
                                                        new(x * 16, y * 16, tileTexture.width, tileTexture.height),
                                                        // tileWidth > 2 && tileHeight > 2 ? new(16, 16) : new(0, 0),
                                                        RayMath.Vector2Scale(GetTileHeadOrigin(initTile), previewScale),
                                                        0,
                                                        z == currentLayer ? new(255, 255, 255, 255) : new(170, 170, 170, 120)
                                                    );
                                                    Raylib.EndShaderMode();
                                                }
                                                else if (tileCell.Type == TileType.Material)
                                                {
                                                    var materialName = ((TileMaterial)tileCell.Data).Name;
                                                    var origin = new Vector2(x * previewScale + 5, y * previewScale + 5);
                                                    var color = materialColorMatrix[y, x, 0];

                                                    if (z != currentLayer) color.a = 120;

                                                    if (color.r != 0 || color.g != 0 || color.b != 0)
                                                    {

                                                            switch (geoMatrix[y, x, 1].Geo)
                                                        {
                                                            case 1:
                                                                DrawRectangle(
                                                                    x * previewScale + 5,
                                                                    y * previewScale + 5,
                                                                    6,
                                                                    6,
                                                                    color
                                                                );
                                                                break;


                                                            case 2:
                                                                DrawTriangle(
                                                                    origin,
                                                                    new(origin.X, origin.Y + previewScale - 10),
                                                                    new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                                    color
                                                                );
                                                                break;


                                                            case 3:
                                                                DrawTriangle(
                                                                    new(origin.X + previewScale - 10, origin.Y),
                                                                    new(origin.X, origin.Y + previewScale - 10),
                                                                    new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                                    color
                                                                );
                                                                break;

                                                            case 4:
                                                                DrawTriangle(
                                                                    origin,
                                                                    new(origin.X, origin.Y + previewScale - 10),
                                                                    new(origin.X + previewScale - 10, origin.Y),
                                                                    color
                                                                );
                                                                break;

                                                            case 5:
                                                                DrawTriangle(
                                                                    origin,
                                                                    new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                                    new(origin.X + previewScale - 10, origin.Y),
                                                                    color
                                                                );
                                                                break;

                                                            case 6:
                                                                DrawRectangleV(
                                                                    origin,
                                                                    new(previewScale - 10, (previewScale - 10) / 2),
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
                                    if (currentLayer != 1 && currentLayer != 2) DrawRectangle(0, 0, matrixWidth * previewScale, matrixHeight * previewScale, new(100, 100, 100, 100));

                                    for (int y = 0; y < matrixHeight; y++)
                                    {
                                        for (int x = 0; x < matrixWidth; x++)
                                        {
                                            const int z = 0;

                                            if (!showLayer1) continue;

                                            var cell = geoMatrix[y, x, z];

                                            var texture = GetBlockIndex(cell.Geo);

                                            if (texture >= 0)
                                            {
                                                if (z == currentLayer)
                                                {

                                                    Raylib.DrawTexturePro(
                                                        geoTextures[texture],
                                                        new(0, 0, 20, 20),
                                                        new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                        new(0, 0),
                                                        0,
                                                        new(0, 0, 0, 255));
                                                }
                                                else
                                                {

                                                    Raylib.DrawTexturePro(
                                                        geoTextures[texture],
                                                        new(0, 0, 20, 20),
                                                        new(x * previewScale, y * previewScale, previewScale, previewScale),
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

                                                            if (z == currentLayer)
                                                            {

                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s)],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                                    new(0, 0),
                                                                    0,
                                                                    new(0, 0, 0, 255)
                                                                );
                                                            }
                                                            else
                                                            {
                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s)],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
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

                                                            if (z == currentLayer)
                                                            {

                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s)],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                                    new(0, 0),
                                                                    0,
                                                                    new(255, 255, 255, 255)
                                                                ); // TODO: remove opacity from entrances
                                                            }
                                                            else
                                                            {
                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s)],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                                    new(0, 0),
                                                                    0,
                                                                    new(255, 255, 255, 170)
                                                                );
                                                            }
                                                            break;

                                                        // directional placement
                                                        case 4:     // entrance
                                                            var index = GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, z));

                                                            if (index is 22 or 23 or 24 or 25)
                                                            {
                                                                geoMatrix[y, x, 0].Geo = 7;
                                                            }

                                                            if (z == currentLayer)
                                                            {

                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[index],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                                    new(0, 0),
                                                                    0,
                                                                    new(255, 255, 255, 255)
                                                                );
                                                            }
                                                            else
                                                            {
                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[index],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                                    new(0, 0),
                                                                    0,
                                                                    new(255, 255, 255, 170)
                                                                );
                                                            }

                                                            break;
                                                        case 11:    // crack
                                                            if (z == currentLayer)
                                                            {
                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, z))],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
                                                                    new(0, 0),
                                                                    0,
                                                                    new(255, 255, 255, 255)
                                                                );
                                                            }
                                                            else
                                                            {
                                                                Raylib.DrawTexturePro(
                                                                    stackableTextures[GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, z))],
                                                                    new(0, 0, 20, 20),
                                                                    new(x * previewScale, y * previewScale, previewScale, previewScale),
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
                                        for (int y = 0; y < matrixHeight; y++)
                                        {
                                            for (int x = 0; x < matrixWidth; x++)
                                            {
                                                const int z = 0;

                                                if (!showLayer1) continue;

                                                var tileCell = tileMatrix[y, x, z];

                                                if (tileCell.Type == TileType.TileHead)
                                                {
                                                    (int, int, string) tile = ((int, int, string))tileCell.Data.CategoryPostition;

                                                    /*Console.WriteLine($"Index: {tile.Item1 - 5}; Length: {tilePreviewTextures.Length}; Name = {tile.Item3}");*/
                                                    var category = tilePreviewTextures[tile.Item1 - 5];

                                                    var tileTexture = category[tile.Item2 - 1];
                                                    var color = tileCategories[tile.Item1 - 5].Item2;
                                                    var initTile = initTiles[tile.Item1 - 5][tile.Item2 - 1];
                                                    var (tileWidth, tileHeight) = initTile.Size;


                                                    BeginShaderMode(tilePreviewShader);
                                                    SetShaderValueTexture(tilePreviewShader, uniformLoc, tileTexture);

                                                    SetShaderValue(
                                                        tilePreviewShader,
                                                        colorLoc,
                                                        new Vector4(color.r, color.g, color.b, 1),
                                                        ShaderUniformDataType.SHADER_UNIFORM_VEC4
                                                    );
                                                    DrawTexturePro(
                                                        tileTexture,
                                                        new(0, 0, tileTexture.width, tileTexture.height),
                                                        new(x * 16, y * 16, tileTexture.width, tileTexture.height),
                                                        // tileWidth > 2 && tileHeight > 2 ? new(16, 16) : new(0, 0),
                                                        RayMath.Vector2Scale(GetTileHeadOrigin(initTile), previewScale),
                                                        0,
                                                        z == currentLayer ? new(255, 255, 255, 255) : new(170, 170, 170, 120)
                                                    );

                                                    EndShaderMode();
                                                }
                                                else if (tileCell.Type == TileType.Material)
                                                {
                                                    var materialName = ((TileMaterial)tileCell.Data).Name;
                                                    var origin = new Vector2(x * previewScale + 5, y * previewScale + 5);
                                                    var color = materialColorMatrix[y, x, 0];

                                                    if (z != currentLayer) color.a = 120;

                                                    if (color.r != 0 || color.g != 0 || color.b != 0)
                                                    {

                                                        switch (geoMatrix[y, x, 0].Geo)
                                                        {
                                                            case 1:
                                                                DrawRectangle(
                                                                    x * previewScale + 5,
                                                                    y * previewScale + 5,
                                                                    6,
                                                                    6,
                                                                    color
                                                                );
                                                                break;


                                                            case 2:
                                                                DrawTriangle(
                                                                    origin,
                                                                    new(origin.X, origin.Y + previewScale - 10),
                                                                    new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                                    color
                                                                );
                                                                break;


                                                            case 3:
                                                                DrawTriangle(
                                                                    new(origin.X + previewScale - 10, origin.Y),
                                                                    new(origin.X, origin.Y + previewScale - 10),
                                                                    new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                                    color
                                                                );
                                                                break;

                                                            case 4:
                                                                DrawTriangle(
                                                                    origin,
                                                                    new(origin.X, origin.Y + previewScale - 10),
                                                                    new(origin.X + previewScale - 10, origin.Y),
                                                                    color
                                                                );
                                                                break;

                                                            case 5:
                                                                DrawTriangle(
                                                                    origin,
                                                                    new(origin.X + previewScale - 10, origin.Y + previewScale - 10),
                                                                    new(origin.X + previewScale - 10, origin.Y),
                                                                    color
                                                                );
                                                                break;

                                                            case 6:
                                                                DrawRectangleV(
                                                                    origin,
                                                                    new(previewScale - 10, (previewScale - 10) / 2),
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

                                var isTileLegel = IsTileLegal(ref currentTileInit, new(tileMatrixX, tileMatrixY), tileMatrix, geoMatrix, currentLayer);

                                Console.WriteLine(isTileLegel ? "LEGAL" : "ILLEGAL");

                                BeginShaderMode(tilePreviewShader);
                                SetShaderValueTexture(tilePreviewShader, uniformLoc, currentTileTexture);

                                if (isTileLegel)
                                {

                                    SetShaderValue(
                                        tilePreviewShader,
                                        colorLoc,
                                        new Vector4(currentTilePreviewColor.r, currentTilePreviewColor.g, currentTilePreviewColor.b, 1),
                                        ShaderUniformDataType.SHADER_UNIFORM_VEC4
                                    );


                                }
                                else
                                {

                                    SetShaderValue(
                                        tilePreviewShader,
                                        colorLoc,
                                        new Vector4(255, 0, 0, 255),
                                        ShaderUniformDataType.SHADER_UNIFORM_VEC4
                                    );

                                }


                                DrawTexturePro(
                                    currentTileTexture,
                                    new(0, 0, currentTileTexture.width, currentTileTexture.height),
                                    new(tileMatrixX * previewScale, tileMatrixY * previewScale, currentTileTexture.width, currentTileTexture.height),
                                    RayMath.Vector2Scale(GetTileHeadOrigin(currentTileInit), previewScale),
                                    0,
                                    new(255, 255, 255, 255)
                                );
                                EndShaderMode();

                            }
                            EndMode2D();

                            #region TileEditorUI
                            {
                                fixed (byte* pt = tilesPanelBytes)
                                {
                                    RayGui.GuiPanel(
                                        tilePanelRect,
                                        (sbyte*)pt
                                    );
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

                                // draw categories list

                                var newCategoryIndex = RayGui.GuiListView(
                                    new(teWidth - (tilePanelWidth + 10) + 10, 90, tilePanelWidth * 0.3f, teHeight - 200),
                                    string.Join(";", tileCategories.Select(t => t.Item1)),
                                    &tileCategoryScrollIndex,
                                    tileCategoryIndex
                                );

                                if (newCategoryIndex != tileCategoryIndex)
                                {
                                    tileCategoryIndex = newCategoryIndex;
                                    tileIndex = 0;
                                }

                                // draw category tiles list


                                tileIndex = Raylib_CsLo.RayGui.GuiListView(
                                    new(teWidth - (tilePanelWidth + 10) + 15 + (tilePanelWidth * 0.3f), 90, tilePanelWidth * 0.7f - 25, teHeight - 200),
                                    string.Join(";", initTiles[tileCategoryIndex].Select(i => i.Name)),
                                    &tileScrollIndex,
                                    tileIndex
                                );

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

                                if (currentLayer == 2)
                                {
                                    DrawRectangleV(new(teWidth - 60, tilePanelRect.height), new(40, 40), new(255, 0, 0, 255));
                                    DrawText("L3", teWidth - 50, (int)tilePanelRect.height + 10, 20, new(255, 255, 255, 255));
                                }
                                if (currentLayer == 1)
                                {
                                    DrawRectangleV(new(teWidth - 60, tilePanelRect.height), new(40, 40), new(0, 255, 0, 255));
                                    DrawText("L2", teWidth - 50, (int)tilePanelRect.height + 10, 20, new(255, 255, 255, 255));
                                }
                                if (currentLayer == 0)
                                {
                                    DrawRectangleV(new(teWidth - 60, tilePanelRect.height), new(40, 40), new(0, 0, 0, 255));
                                    DrawText("L1", teWidth - 47, (int)tilePanelRect.height + 10, 20, new(255, 255, 255, 255));
                                }

                                // tile specs panel

                                fixed (byte* pt = tileSpecsPanelBytes)
                                {
                                    RayGui.GuiPanel(
                                        specsRect,
                                        (sbyte*)pt
                                    );
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
                                    // Console.WriteLine($"Category: {tileCategoryIndex}, Tile: {tileIndex}; ({initTiles.Length}, {initTiles[tileCategoryIndex].Length})");
                                    var (tileWidth, tileHeight) = initTiles[tileCategoryIndex][tileIndex].Size;

                                    var newWholeScale = Math.Min(300 / tileWidth * 20, 200 / tileHeight * 20);
                                    var newCellScale = newWholeScale / 20;

                                    var specs = initTiles[tileCategoryIndex][tileIndex].Specs;
                                    var specs2 = initTiles[tileCategoryIndex][tileIndex].Specs2;

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
                                                var spec = specs[(x * tileHeight) + y];

                                                if (spec is > 0 and < 9 and not 8)
                                                {
                                                    DrawTileSpec(
                                                        spec,
                                                        new Vector2((300 - newCellScale * tileWidth) / 2 + x * newCellScale, (int)specsRect.Y + 100 + y * newCellScale),
                                                        newCellScale,
                                                        new(0, 0, 0, 255)
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

                                // layer visibility

                                {
                                    showTileLayer1 = RayGui.GuiCheckBox(new(tilePanelRect.X + 10, tilePanelRect.height, 20, 20), "Layer 1", showTileLayer1);
                                    showTileLayer2 = RayGui.GuiCheckBox(new(tilePanelRect.X + 90, tilePanelRect.height, 20, 20), "Layer 2", showTileLayer2);
                                    showTileLayer3 = RayGui.GuiCheckBox(new(tilePanelRect.X + 170, tilePanelRect.height, 20, 20), "Layer 3", showTileLayer3);

                                    showLayer1Tiles = RayGui.GuiCheckBox(new(tilePanelRect.X + 10, tilePanelRect.height + 25, 20, 20), "Tiles", showLayer1Tiles);
                                    showLayer2Tiles = RayGui.GuiCheckBox(new(tilePanelRect.X + 90, tilePanelRect.height + 25, 20, 20), "Tiles", showLayer2Tiles);
                                    showLayer3Tiles = RayGui.GuiCheckBox(new(tilePanelRect.X + 170, tilePanelRect.height + 25, 20, 20), "Tiles", showLayer3Tiles);
                                }
                            }
                            #endregion

                            EndDrawing();
                            #endregion

                        }
                        break;
                    #endregion

                    #region CameraEditor
                    case 4:
                        prevPage = 4;

                        #region CamerasInputHandlers

                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_ONE))
                        {
                            page = 1;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_TWO))
                        {
                            page = 2;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_THREE))
                        {
                            page = 3;
                        }
                        //if (Raylib.IsKeyReleased(KeyboardKey.KEY_FOUR)) page = 4;
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_FIVE))
                        {
                            page = 5;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_SIX))
                        {
                            resizeFlag = true;
                            page = 6;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_SEVEN))
                        {
                            page = 7;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_EIGHT))
                        {
                            page = 8;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_NINE))
                        {
                            page = 9;
                        }

                        // handle mouse drag
                        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
                        {
                            Vector2 delta = Raylib.GetMouseDelta();
                            delta = RayMath.Vector2Scale(delta, -1.0f / cameraCamera.zoom);
                            cameraCamera.target = RayMath.Vector2Add(cameraCamera.target, delta);
                        }

                        // handle zoom
                        var cameraWheel = Raylib.GetMouseWheelMove();
                        if (cameraWheel != 0)
                        {
                            Vector2 mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), cameraCamera);
                            cameraCamera.offset = Raylib.GetMousePosition();
                            cameraCamera.target = mouseWorldPosition;
                            cameraCamera.zoom += cameraWheel * zoomIncrement;
                            if (cameraCamera.zoom < zoomIncrement) cameraCamera.zoom = zoomIncrement;
                        }

                        if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && clickTracker2)
                        {
                            clickTracker2 = false;
                        }

                        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT) && !clickTracker2 && draggedCamera != -1)
                        {
                            var pos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), cameraCamera);
                            renderCamers[draggedCamera].Coords = (pos.X - (72 * scale - 40) / 2, pos.Y - (43 * scale - 60) / 2);
                            draggedCamera = -1;
                            clickTracker2 = true;
                        }

                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_N) && draggedCamera == -1)
                        {
                            var pos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), cameraCamera);
                            renderCamers = [.. renderCamers, new() { Coords = (0, 0), Quads = new(new(), new(), new(), new()) }];
                            draggedCamera = renderCamers.Count - 1;
                        }

                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_D) && draggedCamera != -1)
                        {
                            renderCamers.RemoveAt(draggedCamera);
                            draggedCamera = -1;
                        }

                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
                        {
                            if (draggedCamera == -1)
                            {
                                var pos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), cameraCamera);
                                renderCamers = [.. renderCamers, new() { Coords = (0, 0), Quads = new(new(), new(), new(), new()) }];
                                draggedCamera = renderCamers.Count - 1;
                            }
                            else
                            {
                                renderCamers.RemoveAt(draggedCamera);
                                draggedCamera = -1;
                            }
                        }

                        #endregion

                        Raylib.BeginDrawing();
                        {
                            Raylib.ClearBackground(new(170, 170, 170, 255));

                            Raylib.BeginMode2D(cameraCamera);
                            {

                                Raylib.DrawRectangle(
                                    0, 0,
                                    matrixWidth * scale,
                                    matrixHeight * scale,
                                    new(255, 255, 255, 255)
                                );

                                #region CamerasLevelBackground

                                for (int y = 0; y < matrixHeight; y++)
                                {
                                    for (int x = 0; x < matrixWidth; x++)
                                    {
                                        for (int z = 1; z < 3; z++)
                                        {
                                            var cell = geoMatrix[y, x, z];

                                            var texture = GetBlockIndex(cell.Geo);

                                            if (texture >= 0)
                                            {
                                                Raylib.DrawTexture(geoTextures[texture], x * scale, y * scale, new(0, 0, 0, 170));
                                            }
                                        }
                                    }
                                }

                                if (!waterInFront && waterLevel != -1)
                                {
                                    Raylib.DrawRectangle(
                                        (-1) * scale,
                                        (matrixHeight - waterLevel) * scale,
                                        (matrixWidth + 2) * scale,
                                        waterLevel * scale,
                                        new(0, 0, 255, 255)
                                    );
                                }

                                for (int y = 0; y < matrixHeight; y++)
                                {
                                    for (int x = 0; x < matrixWidth; x++)
                                    {
                                        var cell = geoMatrix[y, x, 0];

                                        var texture = GetBlockIndex(cell.Geo);

                                        if (texture >= 0)
                                        {
                                            Raylib.DrawTexture(geoTextures[texture], x * scale, y * scale, new(0, 0, 0, 225));
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
                                                        Raylib.DrawTexture(stackableTextures[GetStackableTextureIndex(s)], x * scale, y * scale, blackStackable);
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
                                                        Raylib.DrawTexture(stackableTextures[GetStackableTextureIndex(s)], x * scale, y * scale, whiteStackable);
                                                        break;

                                                    // directional placement
                                                    case 4:     // entrance
                                                        var index = GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, 0));

                                                        if (index is 22 or 23 or 24 or 25)
                                                        {
                                                            geoMatrix[y, x, 0].Geo = 7;
                                                        }

                                                        Raylib.DrawTexture(stackableTextures[index], x * scale, y * scale, whiteStackable);
                                                        break;
                                                    case 11:    // crack
                                                        Raylib.DrawTexture(
                                                            stackableTextures[GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, 0))],
                                                            x * scale,
                                                            y * scale,
                                                            blackStackable
                                                        );
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (waterInFront && waterLevel != -1)
                                {
                                    Raylib.DrawRectangle(
                                        (-1) * scale,
                                        (matrixHeight - waterLevel) * scale,
                                        (matrixWidth + 2) * scale,
                                        waterLevel * scale,
                                        new(0, 0, 255, 110)
                                    );
                                }

                                #endregion

                                foreach (var (index, cam) in renderCamers.Select((camera, index) => (index, camera)))
                                {
                                    if (index == draggedCamera)
                                    {
                                        var pos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), cameraCamera);
                                        DrawCameraSprite(new(pos.X - (72 * scale - 40) / 2, pos.Y - (43 * scale - 60) / 2), cam.Quads, cameraCamera, index + 1);
                                        continue;
                                    }
                                    var (clicked, hovered) = DrawCameraSprite(new(cam.Coords.x, cam.Coords.y), cam.Quads, cameraCamera, index + 1);

                                    if (clicked && !clickTracker2)
                                    {
                                        draggedCamera = index;
                                        clickTracker2 = true;
                                    }
                                }

                                Raylib.DrawRectangleLinesEx(
                                    border,
                                    4f,
                                    new(200, 66, 245, 255)
                                );
                            }
                            Raylib.EndMode2D();

                            #region CameraEditorUI



                            #endregion
                        }
                        Raylib.EndDrawing();
                        break;
                    #endregion

                    #region LightEditor
                    case 5:
                        prevPage = 5;

                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_ONE))
                        {
                            page = 1;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_TWO))
                        {
                            page = 2;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_THREE))
                        {
                            page = 3;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_FOUR))
                        {
                            page = 4;
                        }
                        // if (Raylib.IsKeyReleased(KeyboardKey.KEY_FIVE)) page = 5;
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_SIX))
                        {
                            resizeFlag = true;
                            page = 6;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_SEVEN))
                        {
                            page = 7;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_EIGHT)) page = 8;
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_NINE)) page = 9;

                        if (Raylib.IsKeyDown(KeyboardKey.KEY_I) && flatness < 10) flatness++;
                        if (Raylib.IsKeyDown(KeyboardKey.KEY_K) && flatness > 0) flatness--;

                        const int textureSize = 130;

                        var panelHeight = Raylib.GetScreenHeight() - 100;

                        var pageSize = panelHeight / textureSize;

                        if (Raylib.IsKeyDown(KeyboardKey.KEY_L))
                        {
                            lightAngleVariable += 0.001f;
                            lightAngle = 180 * Math.Sin(lightAngleVariable) + 90;
                        }
                        if (Raylib.IsKeyDown(KeyboardKey.KEY_J))
                        {
                            lightAngleVariable -= 0.001f;
                            lightAngle = 180 * Math.Sin(lightAngleVariable) + 90;
                        }

                        // handle mouse drag
                        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
                        {
                            Vector2 delta = Raylib.GetMouseDelta();
                            delta = RayMath.Vector2Scale(delta, -1.0f / lightPageCamera.zoom);
                            lightPageCamera.target = RayMath.Vector2Add(lightPageCamera.target, delta);
                        }


                        // handle zoom
                        var wheel2 = Raylib.GetMouseWheelMove();
                        if (wheel2 != 0)
                        {
                            Vector2 mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), lightPageCamera);
                            lightPageCamera.offset = Raylib.GetMousePosition();
                            lightPageCamera.target = mouseWorldPosition;
                            lightPageCamera.zoom += wheel2 * zoomIncrement;
                            if (lightPageCamera.zoom < zoomIncrement) lightPageCamera.zoom = zoomIncrement;
                        }

                        // update light brush

                        {
                            var texture = lightTextures[lightBrushTextureIndex];
                            var lightMouse = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), lightPageCamera);

                            lightBrushSource = new(0, 0, texture.width, texture.height);
                            lightBrushDest = new(lightMouse.X, lightMouse.Y, lightBrushWidth, lightBrushHeight);
                            lightBrushOrigin = new(lightBrushWidth / 2, lightBrushHeight / 2);
                        }

                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_F))
                        {
                            lightBrushTextureIndex = ++lightBrushTextureIndex % lightTextures.Length;

                            lightBrushTexturePage = lightBrushTextureIndex / pageSize;
                        }
                        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_R))
                        {
                            lightBrushTextureIndex--;

                            if (lightBrushTextureIndex < 0) lightBrushTextureIndex = lightTextures.Length - 1;

                            lightBrushTexturePage = lightBrushTextureIndex / pageSize;
                        }

                        if (Raylib.IsKeyDown(KeyboardKey.KEY_Q))
                        {
                            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
                            {
                                lightBrushRotation -= 1;
                            }
                            else
                            {
                                lightBrushRotation -= 0.2f;
                            }
                        }

                        if (Raylib.IsKeyDown(KeyboardKey.KEY_E))
                        {
                            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
                            {
                                lightBrushRotation += 1;
                            }
                            else
                            {
                                lightBrushRotation += 0.2f;
                            }
                        }

                        if (Raylib.IsKeyDown(KeyboardKey.KEY_W))
                        {
                            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
                            {
                                lightBrushHeight += 5;
                            }
                            else
                            {
                                lightBrushHeight += 2;
                            }
                        }
                        else if (Raylib.IsKeyDown(KeyboardKey.KEY_S))
                        {
                            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
                            {
                                lightBrushHeight -= 5;
                            }
                            else
                            {
                                lightBrushHeight -= 2;
                            }
                        }

                        if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
                        {
                            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
                            {
                                lightBrushWidth += 5;
                            }
                            else
                            {
                                lightBrushWidth += 2;
                            }
                        }
                        else if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
                        {
                            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
                            {
                                lightBrushWidth -= 5;
                            }
                            else
                            {
                                lightBrushWidth -= 2;
                            }
                        }

                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_C))
                        {
                            eraseShadow = !eraseShadow;
                        }

                        //

                        var lightMousePos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), lightPageCamera);

                        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                        {
                            Raylib.BeginTextureMode(lightMapBuffer);
                            {
                                if (eraseShadow)
                                {
                                    Raylib.DrawTexturePro(
                                        lightTextures[lightBrushTextureIndex],
                                        lightBrushSource,
                                        lightBrushDest,
                                        lightBrushOrigin,
                                        lightBrushRotation,
                                        new(255, 255, 255, 255)
                                        );
                                }
                                else
                                {
                                    Raylib.DrawTexturePro(
                                        lightTextures[lightBrushTextureIndex],
                                        lightBrushSource,
                                        lightBrushDest,
                                        lightBrushOrigin,
                                        lightBrushRotation,
                                        new(0, 0, 0, 255)
                                        );
                                }
                            }
                            Raylib.EndTextureMode();
                        }

                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE)) slowGrowth = !slowGrowth;
                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_R)) shading = !shading;

                        if (slowGrowth)
                        {
                            if (Raylib.IsKeyDown(KeyboardKey.KEY_W))
                            {
                                lightRecSize = RayMath.Vector2Add(lightRecSize, new(0, growthFactor));
                                growthFactor += 0.03f;
                            }

                            if (Raylib.IsKeyDown(KeyboardKey.KEY_S))
                            {
                                lightRecSize = RayMath.Vector2Add(lightRecSize, new(0, -growthFactor));
                                growthFactor += 0.03f;
                            }

                            if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
                            {
                                lightRecSize = RayMath.Vector2Add(lightRecSize, new(growthFactor, 0));
                                growthFactor += 0.03f;
                            }

                            if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
                            {
                                lightRecSize = RayMath.Vector2Add(lightRecSize, new(-growthFactor, 0));
                                growthFactor += 0.03f;
                            }

                            if (Raylib.IsKeyReleased(KeyboardKey.KEY_W) ||
                                Raylib.IsKeyReleased(KeyboardKey.KEY_S) ||
                                Raylib.IsKeyReleased(KeyboardKey.KEY_D) ||
                                Raylib.IsKeyReleased(KeyboardKey.KEY_A)) growthFactor = initialGrowthFactor;
                        }
                        else
                        {
                            if (Raylib.IsKeyDown(KeyboardKey.KEY_W)) lightRecSize = RayMath.Vector2Add(lightRecSize, new(0, 3));
                            if (Raylib.IsKeyDown(KeyboardKey.KEY_S)) lightRecSize = RayMath.Vector2Add(lightRecSize, new(0, -3));
                            if (Raylib.IsKeyDown(KeyboardKey.KEY_D)) lightRecSize = RayMath.Vector2Add(lightRecSize, new(3, 0));
                            if (Raylib.IsKeyDown(KeyboardKey.KEY_A)) lightRecSize = RayMath.Vector2Add(lightRecSize, new(-3, 0));
                        }


                        Raylib.BeginDrawing();
                        {
                            Raylib.ClearBackground(new(66, 108, 245, 255));

                            Raylib.BeginMode2D(lightPageCamera);
                            {
                                Raylib.DrawRectangle(
                                    0, 0,
                                    matrixWidth * scale + 300,
                                    matrixHeight * scale + 300,
                                    new(255, 255, 255, 255)
                                );

                                for (int y = 0; y < matrixHeight; y++)
                                {
                                    for (int x = 0; x < matrixWidth; x++)
                                    {
                                        for (int z = 1; z < 3; z++)
                                        {
                                            var cell = geoMatrix[y, x, z];

                                            var texture = GetBlockIndex(cell.Geo);

                                            if (texture >= 0)
                                            {
                                                Raylib.DrawTexture(geoTextures[texture], x * scale + 300, y * scale + 300, new(0, 0, 0, 150));
                                            }
                                        }
                                    }
                                }

                                if (!waterInFront && waterLevel != -1)
                                {
                                    Raylib.DrawRectangle(
                                        (-1) * scale + 300,
                                        (matrixHeight - waterLevel) * scale + 300,
                                        (matrixWidth + 2) * scale,
                                        waterLevel * scale,
                                        new(0, 0, 255, 255)
                                    );
                                }

                                for (int y = 0; y < matrixHeight; y++)
                                {
                                    for (int x = 0; x < matrixWidth; x++)
                                    {
                                        var cell = geoMatrix[y, x, 0];

                                        var texture = GetBlockIndex(cell.Geo);

                                        if (texture >= 0)
                                        {
                                            Raylib.DrawTexture(geoTextures[texture], x * scale + 300, y * scale + 300, new(0, 0, 0, 225));
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
                                                        Raylib.DrawTexture(stackableTextures[GetStackableTextureIndex(s)], x * scale + 300, y * scale + 300, blackStackable);
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
                                                        Raylib.DrawTexture(stackableTextures[GetStackableTextureIndex(s)], x * scale + 300, y * scale + 300, whiteStackable);
                                                        break;

                                                    // directional placement
                                                    case 4:     // entrance
                                                        var index = GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, 0));

                                                        if (index is 22 or 23 or 24 or 25)
                                                        {
                                                            geoMatrix[y, x, 0].Geo = 7;
                                                        }

                                                        Raylib.DrawTexture(stackableTextures[index], x * scale + 300, y * scale + 300, whiteStackable);
                                                        break;
                                                    case 11:    // crack
                                                        Raylib.DrawTexture(
                                                            stackableTextures[GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, 0))],
                                                            x * scale + 300,
                                                            y * scale + 300,
                                                            blackStackable
                                                        );
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (waterInFront && waterLevel != -1)
                                {
                                    Raylib.DrawRectangle(
                                        (-1) * scale,
                                        (matrixHeight - waterLevel) * scale + 300,
                                        (matrixWidth + 2) * scale + 300,
                                        waterLevel * scale,
                                        new(0, 0, 255, 110)
                                    );
                                }

                                Raylib.DrawTextureRec(
                                    lightMapBuffer.texture,
                                    new Rectangle(0, 0, lightMapBuffer.texture.width, -lightMapBuffer.texture.height),
                                    new(0, 0),
                                    new(255, 255, 255, 150)
                                );

                                if (eraseShadow)
                                {
                                    Raylib.DrawTexturePro(
                                        lightTextures[lightBrushTextureIndex],
                                        lightBrushSource,
                                        lightBrushDest,
                                        lightBrushOrigin,
                                        lightBrushRotation,
                                        new(200, 66, 245, 255)
                                        );
                                }
                                else
                                {
                                    Raylib.DrawTexturePro(
                                        lightTextures[lightBrushTextureIndex],
                                        lightBrushSource,
                                        lightBrushDest,
                                        lightBrushOrigin,
                                        lightBrushRotation,
                                        new(255, 0, 0, 255)
                                        );
                                }

                            }
                            Raylib.EndMode2D();

                            // brush menu

                            {
                                fixed (byte* pt = lightBrushMenuPanelBytes)
                                {

                                    Raylib_CsLo.RayGui.GuiPanel(
                                        new(10, 50, 150, panelHeight),
                                        (sbyte*)pt
                                        );
                                }

                                var currentPage = lightTextures
                                    .Select((texture, index) => (index, texture))
                                    .Skip(lightBrushTexturePage * pageSize)
                                    .Take(pageSize)
                                    .Select((value, index) => (index, value));

                                foreach (var (pageIndex, (index, texture)) in currentPage)
                                {
                                    Raylib.DrawTexturePro(
                                        texture,
                                        new(0, 0, texture.width, texture.height),
                                        new(20, (textureSize + 1) * pageIndex + 80, textureSize, textureSize),
                                        new(0, 0),
                                        0,
                                        new(0, 0, 0, 255)
                                        );

                                    if (index == lightBrushTextureIndex) Raylib.DrawRectangleLinesEx(
                                        new(
                                            20,
                                            (textureSize + 1) * pageIndex + 80,
                                            textureSize,
                                            textureSize
                                        ),
                                        2.0f,
                                        new(0, 0, 255, 255)
                                        );
                                }
                            }


                            // angle & flatness indicator

                            Raylib.DrawCircleLines(
                                Raylib.GetScreenWidth() - 100,
                                Raylib.GetScreenHeight() - 100,
                                50.0f,
                                new(255, 0, 0, 255)
                            );

                            Raylib.DrawCircleLines(
                                Raylib.GetScreenWidth() - 100,
                                Raylib.GetScreenHeight() - 100,
                                15 + (flatness * 7),
                                new(255, 0, 0, 255)
                            );


                            Raylib.DrawCircleV(new Vector2(
                                (Raylib.GetScreenWidth() - 100) + (float)((15 + flatness * 7) * Math.Cos(lightAngle)),
                                (Raylib.GetScreenHeight() - 100) + (float)((15 + flatness * 7) * Math.Sin(lightAngle))
                                ),
                                10.0f,
                                new(255, 0, 0, 255)
                            );


                        }
                        Raylib.EndDrawing();
                        break;
                    #endregion

                    #region DimensionsEditor
                    case 6:
                        Raylib.BeginDrawing();
                        {
                            Raylib.ClearBackground(new(170, 170, 170, 255));

                            fixed (byte* pt = panelBytes)
                            {
                                Raylib_CsLo.RayGui.GuiPanel(new(30, 60, Raylib.GetScreenWidth() - 60, Raylib.GetScreenHeight() - 120), (sbyte*)pt);
                            }

                            Raylib.DrawText("Width", 50, 110, 20, new(0, 0, 0, 255));
                            if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 100, 300, 40), "", &matrixWidthValue, 72, 999, editControl == 0))
                            {
                                editControl = 0;
                            }

                            Raylib.DrawText("Height", 50, 160, 20, new(0, 0, 0, 255));
                            if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 150, 300, 40), "", &matrixHeightValue, 43, 999, editControl == 1))
                            {
                                editControl = 1;
                            }

                            Raylib_CsLo.RayGui.GuiLine(new(50, 200, Raylib.GetScreenWidth() - 100, 40), "Padding");

                            Raylib.DrawText("Left", 50, 260, 20, new(0, 0, 0, 255));
                            if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 250, 300, 40), "", &leftPadding, 0, 333, editControl == 2))
                            {
                                editControl = 2;
                            }

                            Raylib.DrawText("Right", 50, 310, 20, new(0, 0, 0, 255));
                            if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 300, 300, 40), "", &rightPadding, 0, 333, editControl == 3))
                            {
                                editControl = 3;
                            }

                            Raylib.DrawText("Top", 50, 360, 20, new(0, 0, 0, 255));
                            if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 350, 300, 40), "", &topPadding, 0, 111, editControl == 4))
                            {
                                editControl = 4;
                            }

                            Raylib.DrawText("Bottom", 50, 410, 20, new(0, 0, 0, 255));
                            if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 400, 300, 40), "", &bottomPadding, 0, 111, editControl == 5))
                            {
                                editControl = 5;
                            }

                            var isBecomingLarger = matrixWidthValue > matrixWidth || matrixHeightValue > matrixHeight;

                            if (isBecomingLarger)
                            {
                                Raylib_CsLo.RayGui.GuiLine(new(600, 100, 400, 20), "Fill extra space");
                                fillLayer1 = Raylib_CsLo.RayGui.GuiCheckBox(new(600, 130, 28, 28), "Fill Layer 1", fillLayer1);
                                fillLayer2 = Raylib_CsLo.RayGui.GuiCheckBox(new(750, 130, 28, 28), "Fill Layer 2", fillLayer2);
                                fillLayer3 = Raylib_CsLo.RayGui.GuiCheckBox(new(900, 130, 28, 28), "Fill Layer 3", fillLayer3);
                            }

                            if (Raylib_CsLo.RayGui.GuiButton(new(360, Raylib.GetScreenHeight() - 160, 300, 40), "Ok") || Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
                            {
                                logger.Debug("page 6: Ok button clicked");

                                if (
                                        bufferTiles.Left != leftPadding ||
                                        bufferTiles.Right != rightPadding ||
                                        bufferTiles.Top != topPadding ||
                                        bufferTiles.Bottom != bottomPadding
                                        )
                                {
                                    bufferTiles = new BufferTiles
                                    {
                                        Left = leftPadding,
                                        Right = rightPadding,
                                        Top = topPadding,
                                        Bottom = bottomPadding,
                                    };
                                }

                                if (resizeFlag)
                                {
                                    logger.Debug("resize flag detected");

                                    if (
                                        matrixHeight != matrixHeightValue ||
                                        matrixWidth != matrixWidthValue)
                                    {
                                        logger.Debug("dimensions don't match; resizing");

                                        logger.Debug("resizing geometry matrix");


                                        // I know this can be simplified, but I'm keeping it in case 
                                        // it becomes useful in the future
                                        if (isBecomingLarger)
                                        {
                                            var fillCell = new RunCell { Geo = 1, Stackables = [] };
                                            var emptyCell = new RunCell { Geo = 0, Stackables = [] };

                                            geoMatrix = CommonUtils.Resize(
                                                geoMatrix,
                                                matrixWidth,
                                                matrixHeight,
                                                matrixWidthValue,
                                                matrixHeightValue,
                                                [
                                                    fillLayer1 ? fillCell : emptyCell,
                                                    fillLayer2 ? fillCell : emptyCell,
                                                    fillLayer3 ? fillCell : emptyCell
                                                ]
                                            );
                                        }
                                        else
                                        {
                                            var emptyCell = new RunCell { Geo = 0, Stackables = [] };

                                            geoMatrix = CommonUtils.Resize(
                                                geoMatrix,
                                                matrixWidth,
                                                matrixHeight,
                                                matrixWidthValue,
                                                matrixHeightValue,
                                                [
                                                    emptyCell,
                                                    emptyCell,
                                                    emptyCell
                                                ]
                                            );
                                        }

                                        logger.Debug("resizing tile matrix");

                                        tileMatrix = CommonUtils.Resize(
                                            tileMatrix,
                                            matrixWidth,
                                            matrixHeight,
                                            matrixWidthValue,
                                            matrixHeightValue,
                                            [
                                                new TileCell(),
                                                new TileCell(),
                                                new TileCell()
                                            ]
                                        );

                                        materialColorMatrix = CommonUtils.Resize(
                                            materialColorMatrix, 
                                            matrixWidth, 
                                            matrixHeight, 
                                            matrixHeightValue, 
                                            matrixWidthValue,
                                            [
                                                new(0, 0, 0, 255),
                                                new(0, 0, 0, 255),
                                                new(0, 0, 0, 255)
                                            ]
                                        );


                                        logger.Debug("resizing light map");

                                        ResizeLightMap(ref lightMapBuffer, matrixWidthValue, matrixHeightValue);

                                        matrixHeight = matrixHeightValue;
                                        matrixWidth = matrixWidthValue;
                                    }

                                    logger.Debug("calculating borders");

                                    border = new(
                                        bufferTiles.Left * scale,
                                        bufferTiles.Top * scale,
                                        (matrixWidth - (bufferTiles.Right + bufferTiles.Left)) * scale,
                                        (matrixHeight - (bufferTiles.Bottom + bufferTiles.Top)) * scale
                                    );

                                    resizeFlag = false;
                                }
                                else if (newFlag)
                                {
                                    logger.Debug("new flag detected; creating a new level");

                                    geoMatrix = CommonUtils.NewGeoMatrix(matrixWidthValue, matrixHeightValue, 1);
                                    tileMatrix = CommonUtils.NewTileMatrix(matrixWidthValue, matrixHeightValue);
                                    materialColorMatrix = CommonUtils.NewMaterialColorMatrix(matrixWidthValue, matrixHeightValue, new(0, 0, 0, 255));

                                    matrixHeight = matrixHeightValue;
                                    matrixWidth = matrixWidthValue;

                                    border = new(
                                        leftPadding * scale,
                                        topPadding * scale,
                                        (matrixWidth - (rightPadding + leftPadding)) * scale,
                                        (matrixHeight - (bottomPadding + topPadding)) * scale
                                    );

                                    Raylib.UnloadRenderTexture(lightMapBuffer);
                                    lightMapBuffer = Raylib.LoadRenderTexture((matrixWidth * scale) + 300, (matrixHeight * scale) + 300);

                                    Raylib.BeginTextureMode(lightMapBuffer);
                                    Raylib.ClearBackground(new(255, 255, 255, 255));
                                    Raylib.EndTextureMode();

                                    renderCamers = [new RenderCamera() { Coords = (20f, 30f), Quads = new(new(), new(), new(), new()) }];

                                    newFlag = false;
                                }

                                page = 1;
                            }

                            if (Raylib_CsLo.RayGui.GuiButton(new(50, Raylib.GetScreenHeight() - 160, 300, 40), "Cancel"))
                            {
                                logger.Debug("page 6: Cancel button clicked");

                                leftPadding = bufferTiles.Left;
                                rightPadding = bufferTiles.Right;
                                topPadding = bufferTiles.Top;
                                bottomPadding = bufferTiles.Bottom;

                                matrixWidth = matrixWidthValue;
                                matrixHeight = matrixHeightValue;

                                page = prevPage;
                            }

                        }
                        Raylib.EndDrawing();
                        break;
                    #endregion

                    #region EffectsEditor
                    case 7:
                        prevPage = 7;

                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_ONE)) page = 1;
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_TWO)) page = 2;
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_THREE)) page = 3;
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_FOUR)) page = 4;
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_FIVE)) page = 5;
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_SIX))
                        {
                            resizeFlag = true;
                            page = 6;
                            logger.Debug("go from page 7 to page 6");
                        }
                        // if (Raylib.IsKeyReleased(KeyboardKey.KEY_SEVEN)) page = 7;
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_EIGHT)) page = 8;
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_NINE)) page = 9;

                        // Display menu

                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_N)) addNewEffectMode = !addNewEffectMode;

                        //


                        if (addNewEffectMode)
                        {
                            if (Raylib.IsKeyPressed(KeyboardKey.KEY_UP))
                            {
                                if (newEffectFocus)
                                {
                                    newEffectSelectedValue = --newEffectSelectedValue;
                                    if (newEffectSelectedValue < 0) newEffectSelectedValue = Effects.Names[newEffectCategorySelectedValue].Length - 1;
                                }
                                else
                                {
                                    newEffectSelectedValue = 0;

                                    newEffectCategorySelectedValue = --newEffectCategorySelectedValue;

                                    if (newEffectCategorySelectedValue < 0) newEffectCategorySelectedValue = Effects.Categories.Length - 1;
                                }
                            }

                            if (Raylib.IsKeyPressed(KeyboardKey.KEY_DOWN))
                            {
                                if (newEffectFocus)
                                {
                                    newEffectSelectedValue = ++newEffectSelectedValue % Effects.Names[newEffectCategorySelectedValue].Length;
                                }
                                else
                                {
                                    newEffectSelectedValue = 0;
                                    newEffectCategorySelectedValue = ++newEffectCategorySelectedValue % Effects.Categories.Length;
                                }
                            }

                            if (Raylib.IsKeyPressed(KeyboardKey.KEY_RIGHT))
                            {
                                newEffectFocus = true;
                            }

                            if (Raylib.IsKeyPressed(KeyboardKey.KEY_LEFT))
                            {
                                newEffectFocus = false;
                            }

                            if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
                            {
                                effectList = [
                                    .. effectList,
                                    (
                                        Effects.Names[newEffectCategorySelectedValue][newEffectSelectedValue],
                                        Effects.GetEffectOptions(Effects.Names[newEffectCategorySelectedValue][newEffectSelectedValue]),
                                        new double[matrixHeight, matrixWidth]
                                    )
                                ];

                                addNewEffectMode = false;
                            }

                            Raylib.BeginDrawing();
                            {
                                Raylib.DrawRectangle(
                                    0,
                                    0,
                                    Raylib.GetScreenWidth(),
                                    Raylib.GetScreenHeight(),
                                    new Color(0, 0, 0, 90)
                                );

                                fixed (byte* pt = addNewEffectPanelBytes)
                                {
                                    Raylib_CsLo.RayGui.GuiPanel(
                                        new(
                                            Raylib.GetScreenWidth() / 2 - 400,
                                            Raylib.GetScreenHeight() / 2 - 300,
                                            800,
                                            600
                                        ),
                                        (sbyte*)pt
                                    );
                                }

                                Raylib_CsLo.RayGui.GuiLine(
                                    new(
                                        Raylib.GetScreenWidth() / 2 - 390,
                                        Raylib.GetScreenHeight() / 2 - 265,
                                        150,
                                        10
                                    ),
                                    "Categories"
                                );

                                newEffectCategorySelectedValue = Raylib_CsLo.RayGui.GuiListView(
                                    new(
                                        Raylib.GetScreenWidth() / 2 - 390,
                                        Raylib.GetScreenHeight() / 2 - 250,
                                        150,
                                        540
                                    ),
                                    string.Join(";", Effects.Categories),
                                    &newEffectCategoryScrollIndex,
                                    newEffectCategorySelectedValue
                                );

                                if (!newEffectFocus) Raylib.DrawRectangleLinesEx(
                                    new(
                                        Raylib.GetScreenWidth() / 2 - 390,
                                        Raylib.GetScreenHeight() / 2 - 250,
                                        150,
                                        540
                                    ),
                                    2.0f,
                                    new(0, 0, 255, 255)
                                );

                                newEffectSelectedValue = Raylib_CsLo.RayGui.GuiListView(
                                    new(
                                        Raylib.GetScreenWidth() / 2 - 230,
                                        Raylib.GetScreenHeight() / 2 - 250,
                                        620,
                                        540
                                    ),
                                    string.Join(";", Effects.Names[newEffectCategorySelectedValue]),
                                    &newEffectScrollIndex,
                                    newEffectSelectedValue
                                );

                                if (newEffectFocus) Raylib.DrawRectangleLinesEx(
                                    new(
                                        Raylib.GetScreenWidth() / 2 - 230,
                                        Raylib.GetScreenHeight() / 2 - 250,
                                        620,
                                        540
                                    ),
                                    2.0f,
                                    new(0, 0, 255, 255)
                                );
                            }
                            Raylib.EndDrawing();
                        }
                        else
                        {

                            Vector2 effectsMouse = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), effectsCamera);

                            //                        v this was done to avoid rounding errors
                            int effectsMatrixY = effectsMouse.Y < 0 ? -1 : (int)effectsMouse.Y / scale;
                            int effectsMatrixX = effectsMouse.X < 0 ? -1 : (int)effectsMouse.X / scale;


                            var appliedEffectsPanelHeight = Raylib.GetScreenHeight() - 200;
                            const int appliedEffectRecHeight = 30;
                            var appliedEffectPageSize = appliedEffectsPanelHeight / (appliedEffectRecHeight + 20);

                            // Prevent using the brush when mouse over the effects list
                            bool canUseBrush = !Raylib.CheckCollisionPointRec(
                                effectsMouse,
                                new(
                                    Raylib.GetScreenWidth() - 300,
                                    100,
                                    280,
                                    appliedEffectsPanelHeight
                                )
                            ) && !Raylib.CheckCollisionPointRec(
                                effectsMouse,
                                new(
                                    20,
                                    Raylib.GetScreenHeight() - 220,
                                    600,
                                    200
                                )
                            );

                            // Movement

                            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
                            {
                                Vector2 delta = Raylib.GetMouseDelta();
                                delta = RayMath.Vector2Scale(delta, -1.0f / effectsCamera.zoom);
                                effectsCamera.target = RayMath.Vector2Add(effectsCamera.target, delta);
                            }

                            // Brush size

                            var effectslMouseWheel = Raylib.GetMouseWheelMove();

                            if (effectslMouseWheel != 0)
                            {
                                brushRadius += (int)effectslMouseWheel;

                                if (brushRadius < 0) brushRadius = 0;
                                if (brushRadius > 10) brushRadius = 10;
                            }

                            // Use brush

                            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                            {
                                if (
                                        effectsMatrixX >= 0 &&
                                        effectsMatrixX < matrixWidth &&
                                        effectsMatrixY >= 0 &&
                                        effectsMatrixY < matrixHeight &&
                                        (
                                            effectsMatrixX != prevMatrixX || effectsMatrixY != prevMatrixY || !clickTracker
                                        ))
                                {
                                    var mtx = effectList[currentAppliedEffect].Item3;

                                    if (brushEraseMode)
                                    {
                                        //mtx[effectsMatrixY, effectsMatrixX] -= Effects.GetBrushStrength(effectList[currentAppliedEffect].Item1);

                                        //if (mtx[effectsMatrixY, effectsMatrixX] < 0) mtx[effectsMatrixY, effectsMatrixX] = 0;

                                        PaintEffect(
                                            effectList[currentAppliedEffect].Item3,
                                            (matrixWidth, matrixHeight),
                                            (effectsMatrixX, effectsMatrixY),
                                            brushRadius,
                                            -Effects.GetBrushStrength(effectList[currentAppliedEffect].Item1)
                                        );
                                    }
                                    else
                                    {
                                        //mtx[effectsMatrixY, effectsMatrixX] += Effects.GetBrushStrength(effectList[currentAppliedEffect].Item1);

                                        PaintEffect(
                                            effectList[currentAppliedEffect].Item3,
                                            (matrixWidth, matrixHeight),
                                            (effectsMatrixX, effectsMatrixY),
                                            brushRadius,
                                            Effects.GetBrushStrength(effectList[currentAppliedEffect].Item1)
                                            );

                                        //if (mtx[effectsMatrixY, effectsMatrixX] > 100) mtx[effectsMatrixY, effectsMatrixX] = 100;
                                    }

                                    prevMatrixX = effectsMatrixX;
                                    prevMatrixY = effectsMatrixY;
                                }

                                clickTracker = true;
                            }

                            if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
                            {
                                clickTracker = false;
                            }

                            //

                            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
                            {
                                var index = currentAppliedEffect;

                                if (Raylib.IsKeyPressed(KeyboardKey.KEY_W))
                                {
                                    if (index > 0)
                                    {
                                        (effectList[index], effectList[index - 1]) = (effectList[index - 1], effectList[index]);
                                        currentAppliedEffect--;
                                    }
                                }
                                else if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
                                {
                                    if (index < effectList.Length - 1)
                                    {
                                        (effectList[index], effectList[index + 1]) = (effectList[index + 1], effectList[index]);
                                        currentAppliedEffect++;
                                    }
                                }
                            }
                            else
                            {
                                if (Raylib.IsKeyPressed(KeyboardKey.KEY_W))
                                {
                                    currentAppliedEffect--;

                                    if (currentAppliedEffect < 0) currentAppliedEffect = effectList.Length - 1;

                                    currentAppliedEffectPage = currentAppliedEffect / appliedEffectPageSize;
                                }

                                if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
                                {
                                    currentAppliedEffect = ++currentAppliedEffect % effectList.Length;

                                    currentAppliedEffectPage = currentAppliedEffect / appliedEffectPageSize;
                                }
                            }

                            if (Raylib.IsKeyPressed(KeyboardKey.KEY_O)) showEffectOptions = !showEffectOptions;
                            if (Raylib.IsKeyPressed(KeyboardKey.KEY_Q)) brushEraseMode = !brushEraseMode;


                            // Delete effect
                            if (Raylib.IsKeyPressed(KeyboardKey.KEY_X))
                            {
                                effectList = effectList.Where((e, i) => i != currentAppliedEffect).ToArray();
                                currentAppliedEffect--;
                                if (currentAppliedEffect < 0) currentAppliedEffect = effectList.Length - 1;
                            }

                            Raylib.BeginDrawing();
                            {

                                Raylib.ClearBackground(new(0, 0, 0, 255));

                                Raylib.BeginMode2D(effectsCamera);
                                {
                                    Raylib.DrawRectangleLinesEx(
                                        new Rectangle(
                                            -2, -2,
                                            (matrixWidth * scale) + 4,
                                            (matrixHeight * scale) + 4
                                        ),
                                        2f,
                                        new(255, 255, 255, 255)
                                    );
                                    Raylib.DrawRectangle(0, 0, matrixWidth * scale, matrixHeight * scale, new(255, 255, 255, 255));

                                    for (int y = 0; y < matrixHeight; y++)
                                    {
                                        for (int x = 0; x < matrixWidth; x++)
                                        {
                                            for (int z = 1; z < 3; z++)
                                            {
                                                var cell = geoMatrix[y, x, z];

                                                var texture = GetBlockIndex(cell.Geo);

                                                if (texture >= 0)
                                                {
                                                    Raylib.DrawTexture(geoTextures[texture], x * scale, y * scale, new(0, 0, 0, 170));
                                                }
                                            }
                                        }
                                    }

                                    if (!waterInFront && waterLevel != -1)
                                    {
                                        Raylib.DrawRectangle(
                                            (-1) * scale,
                                            (matrixHeight - waterLevel) * scale,
                                            (matrixWidth + 2) * scale,
                                            waterLevel * scale,
                                            new(0, 0, 255, 255)
                                        );
                                    }

                                    for (int y = 0; y < matrixHeight; y++)
                                    {
                                        for (int x = 0; x < matrixWidth; x++)
                                        {
                                            var cell = geoMatrix[y, x, 0];

                                            var texture = GetBlockIndex(cell.Geo);

                                            if (texture >= 0)
                                            {
                                                Raylib.DrawTexture(geoTextures[texture], x * scale, y * scale, new(0, 0, 0, 225));
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
                                                            Raylib.DrawTexture(stackableTextures[GetStackableTextureIndex(s)], x * scale, y * scale, blackStackable);
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
                                                            Raylib.DrawTexture(stackableTextures[GetStackableTextureIndex(s)], x * scale, y * scale, whiteStackable);
                                                            break;

                                                        // directional placement
                                                        case 4:     // entrance
                                                            var index = GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, 0));

                                                            if (index is 22 or 23 or 24 or 25)
                                                            {
                                                                geoMatrix[y, x, 0].Geo = 7;
                                                            }

                                                            Raylib.DrawTexture(stackableTextures[index], x * scale, y * scale, whiteStackable);
                                                            break;
                                                        case 11:    // crack
                                                            Raylib.DrawTexture(
                                                                stackableTextures[GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, 0))],
                                                                x * scale,
                                                                y * scale,
                                                                blackStackable
                                                            );
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (waterInFront && waterLevel != -1)
                                    {
                                        Raylib.DrawRectangle(
                                            (-1) * scale,
                                            (matrixHeight - waterLevel) * scale,
                                            (matrixWidth + 2) * scale,
                                            waterLevel * scale,
                                            new(0, 0, 255, 110)
                                        );
                                    }

                                    // Effect matrix

                                    if (effectList.Length > 0 &&
                                        currentAppliedEffect >= 0 &&
                                        currentAppliedEffect < effectList.Length)
                                    {

                                        Raylib.DrawRectangle(0, 0, matrixWidth * scale, matrixHeight * scale, new(215, 66, 245, 100));

                                        for (int y = 0; y < matrixHeight; y++)
                                        {
                                            for (int x = 0; x < matrixWidth; x++)
                                            {
                                                Raylib.DrawRectangle(x * scale, y * scale, scale, scale, new(0, 255, 0, (int)effectList[currentAppliedEffect].Item3[y, x] * 255 / 100));
                                            }
                                        }
                                    }

                                    // Brush

                                    for (int y = 0; y < matrixHeight; y++)
                                    {
                                        for (int x = 0; x < matrixWidth; x++)
                                        {
                                            if (effectsMatrixX == x && effectsMatrixY == y)
                                            {
                                                if (brushEraseMode)
                                                {
                                                    Raylib.DrawRectangleLinesEx(
                                                        new Rectangle(
                                                            x * scale,
                                                            y * scale,
                                                            scale,
                                                            scale
                                                        ),
                                                        2.0f,
                                                        new(255, 0, 0, 255)
                                                    );


                                                    Raylib.DrawRectangleLines(
                                                        (effectsMatrixX - brushRadius) * scale,
                                                        (effectsMatrixY - brushRadius) * scale,
                                                        (brushRadius * 2 + 1) * scale,
                                                        (brushRadius * 2 + 1) * scale,
                                                        new(255, 0, 0, 255));
                                                }
                                                else
                                                {
                                                    Raylib.DrawRectangleLinesEx(
                                                        new Rectangle(
                                                            x * scale,
                                                            y * scale,
                                                            scale,
                                                            scale
                                                        ),
                                                        2.0f,
                                                        new(255, 255, 255, 255)
                                                    );

                                                    Raylib.DrawRectangleLines(
                                                        (effectsMatrixX - brushRadius) * scale,
                                                        (effectsMatrixY - brushRadius) * scale,
                                                        (brushRadius * 2 + 1) * scale,
                                                        (brushRadius * 2 + 1) * scale,
                                                        new(255, 255, 255, 255));
                                                }
                                            }
                                        }
                                    }

                                }
                                Raylib.EndMode2D();

                                // UI

                                fixed (byte* pt = appliedEffectsPanelBytes)
                                {
                                    Raylib_CsLo.RayGui.GuiPanel(
                                        new(
                                            Raylib.GetScreenWidth() - 300,
                                            100,
                                            280,
                                           appliedEffectsPanelHeight
                                        ),
                                        (sbyte*)pt
                                    );
                                }

                                if (effectList.Length > appliedEffectPageSize)
                                {
                                    if (currentAppliedEffectPage < (effectList.Length / appliedEffectPageSize))
                                    {
                                        var appliedEffectsPageDownPressed = Raylib_CsLo.RayGui.GuiButton(
                                            new(
                                                Raylib.GetScreenWidth() - 290,
                                                Raylib.GetScreenHeight() - 140,
                                                130,
                                                32
                                            ),

                                            "Page Down"
                                        );

                                        if (appliedEffectsPageDownPressed)
                                        {
                                            currentAppliedEffectPage++;
                                            currentAppliedEffect = appliedEffectPageSize * currentAppliedEffectPage;
                                        }
                                    }

                                    if (currentAppliedEffectPage > 0)
                                    {
                                        var appliedEffectsPageUpPressed = Raylib_CsLo.RayGui.GuiButton(
                                            new(
                                                Raylib.GetScreenWidth() - 155,
                                                Raylib.GetScreenHeight() - 140,
                                                130,
                                                32
                                            ),

                                            "Page Up"
                                        );

                                        if (appliedEffectsPageUpPressed)
                                        {
                                            currentAppliedEffectPage--;
                                            currentAppliedEffect = appliedEffectPageSize * (currentAppliedEffectPage + 1) - 1;
                                        }
                                    }
                                }


                                // Applied effects

                                // i is index relative to the page; oi is index relative to the whole list
                                foreach (var (i, (oi, e)) in effectList.Select((value, i) => (i, value)).Skip(appliedEffectPageSize * currentAppliedEffectPage).Take(appliedEffectPageSize).Select((value, i) => (i, value)))
                                {
                                    Raylib.DrawRectangleLines(
                                        Raylib.GetScreenWidth() - 290,
                                        130 + (35 * i),
                                        260,
                                        appliedEffectRecHeight,
                                        new(0, 0, 0, 255)
                                    );

                                    if (oi == currentAppliedEffect) Raylib.DrawRectangleLinesEx(
                                        new(
                                            Raylib.GetScreenWidth() - 290,
                                            130 + (35 * i),
                                            260,
                                            appliedEffectRecHeight
                                        ),
                                        2.0f,
                                        new(0, 0, 255, 255)
                                    );

                                    Raylib.DrawText(
                                        e.Item1,
                                        Raylib.GetScreenWidth() - 280,
                                        138 + (35 * i),
                                        14,
                                        new(0, 0, 0, 255)
                                    );

                                    var deletePressed = Raylib_CsLo.RayGui.GuiButton(
                                        new(
                                            Raylib.GetScreenWidth() - 67,
                                            132 + (35 * i),
                                            37,
                                            26
                                        ),
                                        "X"
                                    );

                                    if (deletePressed)
                                    {
                                        effectList = effectList.Where((e, i) => i != oi).ToArray();
                                        currentAppliedEffect--;
                                        if (currentAppliedEffect < 0) currentAppliedEffect = effectList.Length - 1;
                                    }

                                    if (oi > 0)
                                    {
                                        var moveUpPressed = Raylib_CsLo.RayGui.GuiButton(
                                            new(
                                                Raylib.GetScreenWidth() - 105,
                                                132 + (35 * i),
                                                37,
                                                26
                                            ),
                                            "^"
                                        );

                                        if (moveUpPressed)
                                        {
                                            (effectList[oi], effectList[oi - 1]) = (effectList[oi - 1], effectList[oi]);
                                        }
                                    }

                                    if (oi < effectList.Length - 1)
                                    {
                                        var moveDownPressed = Raylib_CsLo.RayGui.GuiButton(
                                            new(
                                                Raylib.GetScreenWidth() - 143,
                                                132 + (35 * i),
                                                37,
                                                26
                                            ),
                                            "v"
                                        );

                                        if (moveDownPressed)
                                        {
                                            (effectList[oi], effectList[oi + 1]) = (effectList[oi + 1], effectList[oi]);
                                        }
                                    }

                                }

                                // Options

                                if (showEffectOptions)
                                {
                                    fixed (byte* pt = effectOptionsPanelBytes)
                                    {
                                        Raylib_CsLo.RayGui.GuiPanel(
                                            new(
                                                20,
                                                Raylib.GetScreenHeight() - 220,
                                                600,
                                                200
                                            ),
                                            (sbyte*)pt
                                        );
                                    }

                                    var options = effectList.Length > 0 ? effectList[currentAppliedEffect].Item2 : new();

                                    if (options.Layers is not null || options.Layers2 is not null)
                                    {
                                        Raylib_CsLo.RayGui.GuiLine(
                                            new(
                                                30,
                                                Raylib.GetScreenHeight() - 190,
                                                100,
                                                10
                                            ),
                                            "Layers"
                                        );

                                        if (options.Layers is not null)
                                        {
                                            var group = (int)options.Layers;

                                            if (Raylib_CsLo.RayGui.GuiCheckBox(new(30, Raylib.GetScreenHeight() - 175, 19, 19), "All", group == 0)) group = 0;
                                            if (Raylib_CsLo.RayGui.GuiCheckBox(new(30, Raylib.GetScreenHeight() - 155, 19, 19), "1", group == 1)) group = 1;
                                            if (Raylib_CsLo.RayGui.GuiCheckBox(new(30, Raylib.GetScreenHeight() - 135, 19, 19), "2", group == 2)) group = 2;
                                            if (Raylib_CsLo.RayGui.GuiCheckBox(new(30, Raylib.GetScreenHeight() - 115, 19, 19), "3", group == 3)) group = 3;
                                            if (Raylib_CsLo.RayGui.GuiCheckBox(new(30, Raylib.GetScreenHeight() - 95, 19, 19), "1st and 2nd", group == 4)) group = 4;
                                            if (Raylib_CsLo.RayGui.GuiCheckBox(new(30, Raylib.GetScreenHeight() - 75, 19, 19), "2nd and 3rd", group == 5)) group = 5;

                                            options.Layers = (EffectLayer1)group;
                                        }
                                        else
                                        {
                                            var group = (int)options.Layers2;

                                            if (Raylib_CsLo.RayGui.GuiCheckBox(new(30, Raylib.GetScreenHeight() - 175, 19, 19), "1", group == 0)) group = 0;
                                            if (Raylib_CsLo.RayGui.GuiCheckBox(new(30, Raylib.GetScreenHeight() - 155, 19, 19), "2", group == 1)) group = 1;
                                            if (Raylib_CsLo.RayGui.GuiCheckBox(new(30, Raylib.GetScreenHeight() - 135, 19, 19), "3", group == 2)) group = 2;

                                            options.Layers2 = (EffectLayer2)group;
                                        }
                                    }

                                    if (options.Color is not null)
                                    {
                                        Raylib_CsLo.RayGui.GuiLine(
                                            new(
                                                140,
                                                Raylib.GetScreenHeight() - 190,
                                                100,
                                                10
                                            ),
                                            "Color"
                                        );

                                        var group = (int)options.Color;

                                        if (Raylib_CsLo.RayGui.GuiCheckBox(new(140, Raylib.GetScreenHeight() - 175, 19, 19), "Color 1", group == 0)) group = 0;
                                        if (Raylib_CsLo.RayGui.GuiCheckBox(new(140, Raylib.GetScreenHeight() - 155, 19, 19), "Color 2", group == 1)) group = 1;
                                        if (Raylib_CsLo.RayGui.GuiCheckBox(new(140, Raylib.GetScreenHeight() - 135, 19, 19), "Dead", group == 2)) group = 2;

                                        options.Color = (EffectColor)group;
                                    }

                                    if (options.Fatness is not null)
                                    {
                                        Raylib_CsLo.RayGui.GuiLine(
                                            new(
                                                250,
                                                Raylib.GetScreenHeight() - 190,
                                                100,
                                                10
                                            ),
                                            "Fatness"
                                        );

                                        var group = (int)options.Fatness;

                                        if (Raylib_CsLo.RayGui.GuiCheckBox(new(250, Raylib.GetScreenHeight() - 175, 19, 19), "1px", group == 0)) group = 0;
                                        if (Raylib_CsLo.RayGui.GuiCheckBox(new(250, Raylib.GetScreenHeight() - 155, 19, 19), "2px", group == 1)) group = 1;
                                        if (Raylib_CsLo.RayGui.GuiCheckBox(new(250, Raylib.GetScreenHeight() - 135, 19, 19), "3px", group == 2)) group = 2;
                                        if (Raylib_CsLo.RayGui.GuiCheckBox(new(250, Raylib.GetScreenHeight() - 115, 19, 19), "Random", group == 3)) group = 3;

                                        options.Fatness = (EffectFatness)group;
                                    }

                                    if (options.Size is not null)
                                    {
                                        Raylib_CsLo.RayGui.GuiLine(
                                            new(
                                                360,
                                                Raylib.GetScreenHeight() - 190,
                                                100,
                                                10
                                            ),
                                            "Size"
                                        );

                                        var group = (int)options.Size;

                                        if (Raylib_CsLo.RayGui.GuiCheckBox(new(360, Raylib.GetScreenHeight() - 175, 19, 19), "Small", group == 0)) group = 0;
                                        if (Raylib_CsLo.RayGui.GuiCheckBox(new(360, Raylib.GetScreenHeight() - 155, 19, 19), "Fat", group == 1)) group = 1;

                                        options.Size = (EffectSize)group;
                                    }

                                    if (options.Colored is not null)
                                    {
                                        Raylib_CsLo.RayGui.GuiLine(
                                            new(
                                                470,
                                                Raylib.GetScreenHeight() - 190,
                                                100,
                                                10
                                            ),
                                            "Colored"
                                        );

                                        var group = (int)options.Colored;

                                        if (Raylib_CsLo.RayGui.GuiCheckBox(new(470, Raylib.GetScreenHeight() - 175, 19, 19), "White", group == 0)) group = 0;
                                        if (Raylib_CsLo.RayGui.GuiCheckBox(new(470, Raylib.GetScreenHeight() - 155, 19, 19), "None", group == 1)) group = 1;

                                        options.Colored = (EffectColored)group;
                                    }
                                }


                            }
                            Raylib.EndDrawing();
                        }
                        break;
                    #endregion

                    #region HelpPage
                    case 9:
                        prevPage = 9;

                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_ONE)) page = 1;
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_TWO)) page = 2;
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_THREE)) page = 3;
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_FOUR)) page = 4;
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_FIVE)) page = 5;
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_SIX))
                        {
                            resizeFlag = true;
                            page = 6;
                        }
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_SEVEN)) page = 7;
                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_EIGHT)) page = 8;
                        //if (Raylib.IsKeyReleased(KeyboardKey.KEY_NINE)) page = 9;

                        Raylib.BeginDrawing();
                        {
                            Raylib.ClearBackground(new(170, 170, 170, 255));

                            fixed (byte* pt = helpPanelBytes)
                            {
                                Raylib_CsLo.RayGui.GuiPanel(
                                    new(100, 100, Raylib.GetScreenWidth() - 200, Raylib.GetScreenHeight() - 200),
                                    (sbyte*)pt
                                );
                            }

                            helpSubSection = Raylib_CsLo.RayGui.GuiListView(
                                new Raylib_CsLo.Rectangle(120, 150, 250, Raylib.GetScreenHeight() - 270),
                                "Main Screen;Geometry Editor;Cameras Editor;Light Editor;Effects Editor;Tiles Editor; Props Editor",
                                &helpScrollIndex,
                                helpSubSection
                            );

                            Raylib.DrawRectangleLines(
                                390,
                                150,
                                Raylib.GetScreenWidth() - 510,
                                Raylib.GetScreenHeight() - 270,
                                new(170, 170, 170, 255)
                            );

                            switch (helpSubSection)
                            {
                                case 0: // main screen
                                    Raylib.DrawText(
                                        " [1] - Main screen\n[2] - Geometry editor\n[3] - Tiles editor\n[4] - Cameras editor\n" +
                                        "[5] - Light editor\n[6] - Edit dimensions\n[7] - Effects editor\n[8] - Props editor",
                                        400,
                                        160,
                                        20,
                                        new(0, 0, 0, 255)
                                    );
                                    break;

                                case 1: // geometry editor
                                    Raylib.DrawText(
                                        "[W] [A] [S] [D] - Navigate the geometry tiles menu\n" +
                                        "[L] - Change current layer\n" +
                                        "[M] - Toggle grid (contrast)",
                                        400,
                                        160,
                                        20,
                                        new(0, 0, 0, 255)
                                    );
                                    break;

                                case 2: // cameras editor
                                    Raylib.DrawText(
                                        "[N] - New Camera\n" +
                                        "[D] - Delete dragged camera\n" +
                                        "[SPACE] - Do both\n" +
                                        "[LEFT CLICK]  - Move a camera around\n" +
                                        "[RIGHT CLICK] - Move around",
                                        400,
                                        160,
                                        20,
                                        new(0, 0, 0, 255)
                                    );
                                    break;

                                case 3: // light editor
                                    Raylib.DrawText(
                                        "[Q] [E] - Rotate brush (counter-)clockwise\n" +
                                        "[SHIFT] + [Q] [R] - Rotate brush faster\n" +
                                        "[W] [S] - Resize brush vertically\n" +
                                        "[A] [D] - Resize brush horizontally\n" +
                                        "[R] [F] - Change brush\n" +
                                        "[C] - Toggle shadow eraser\n",
                                        400,
                                        160,
                                        20,
                                        new(0, 0, 0, 255)
                                    );
                                    break;

                                case 4: // effects editor
                                    Raylib.DrawText(
                                        "[Right Click] - Drag level\n" +
                                        "[Left Click] - Paint/erase effect\n" +
                                        "[Mouse Wheel] - Resize brush\n" +
                                        "[W] [S] - Move to next/previous effect\n" +
                                        "[SHIFT] + [W] [S] - Change applied effect order\n" +
                                        "[N] - Add new effect\n" +
                                        "[O] - Show/hide effect options",
                                        400,
                                        160,
                                        20,
                                        new(0, 0, 0, 255)
                                    );
                                    break;

                                case 5:
                                    break;

                                case 6:
                                    break;
                            }
                        }
                        Raylib.EndDrawing();
                        break;
                    #endregion

                    #region LoadPage
                    case 11:

                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_ZERO)) page = 0;

                        const int buttonHeight = 40;
                        var maxCount = (Raylib.GetScreenHeight() - 400) / buttonHeight;
                        var buttonOffsetX = 120;
                        var buttonWidth = Raylib.GetScreenWidth() - 240;
                        string[] projectFiles;

                        try
                        {
                            projectFiles = Directory.EnumerateFiles(projectsDirectory).ToArray();
                        }
                        catch (Exception e)
                        {
                            logger.Fatal($"failed to read project files: {e}");
                            return;
                        }

                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_W) && explorerPage > 0) explorerPage--;
                        if (Raylib.IsKeyPressed(KeyboardKey.KEY_S) && explorerPage < (projectFiles.Length / maxCount)) explorerPage++;


                        Raylib.BeginDrawing();
                        {
                            if (Raylib_CsLo.RayGui.GuiIsLocked()) // loading a project
                            {
                                Raylib.ClearBackground(new Color(0, 0, 0, 130));


                                if (!loadFileTask.IsCompleted)
                                {
                                    Raylib.DrawText("Please wait..", Raylib.GetScreenWidth() / 2 - 100, Raylib.GetScreenHeight() / 2 - 20, 30, new(255, 255, 255, 255));
                                    Raylib.EndDrawing();
                                    continue;
                                }

                                LoadFileResult res = loadFileTask.Result;

                                if (!res.Success)
                                {
                                    logger.Debug("failed to load level project");
                                    Raylib_CsLo.RayGui.GuiUnlock();
                                    Raylib.EndDrawing();
                                    continue;
                                }

                                // validate if tiles are defiend in Init.txt

                                if (tileCheckTask is null)
                                {

                                    tileCheckTask = Task.Factory.StartNew(() =>
                                    {
                                        for (int y = 0; y < res.Height; y++)
                                        {
                                            for (int x = 0; x < res.Width; x++)
                                            {
                                                for (int z = 0; z < 3; z++)
                                                {
                                                    var cell = res.TileMatrix![y, x, z];

                                                    if (cell.Type == TileType.TileHead)
                                                    {
                                                        var (category, position, name) = ((TileHead)cell.Data).CategoryPostition;

                                                        // code readibility could be optimized using System.Linq

                                                        for (var c = 0; c < initTiles.Length; c++)
                                                        {
                                                            for (var i = 0; i < initTiles[c].Length; i++)
                                                            {
                                                                if (initTiles[c][i].Name == name)
                                                                {
                                                                    res.TileMatrix![y, x, z].Data.CategoryPostition = (c + 5, i + 1, name);

                                                                    try
                                                                    {
                                                                        var texture = tileTextures[c][i];
                                                                    }
                                                                    catch
                                                                    {
                                                                        logger.Warning($"missing tile texture detected: matrix index: ({x}, {y}, {z}); category {category}, position: {position}, name: \"{name}\"");
                                                                        return TileCheckResult.MissingTexture;
                                                                    }

                                                                    goto skip;
                                                                }
                                                            }
                                                        }

                                                        // Tile not found
                                                        return TileCheckResult.Missing;
                                                    }
                                                    else if (cell.Type == TileType.Material)
                                                    {
                                                        var materialName = ((TileMaterial)cell.Data).Name;

                                                        if (!MaterialColors.ContainsKey(materialName))
                                                        {
                                                            logger.Warning($"missing material: matrix index: ({x}, {y}, {z}); Name: \"{materialName}\"");
                                                            return TileCheckResult.MissingMaterial;
                                                        }
                                                    }

                                                skip:
                                                    { }
                                                }
                                            }
                                        }

                                        logger.Debug("tile check passed");

                                        return TileCheckResult.Ok;
                                    });

                                    Raylib.EndDrawing();
                                    continue;
                                }

                                if (!tileCheckTask.IsCompleted)
                                {
                                    Raylib.DrawText("Validating..", Raylib.GetScreenWidth() / 2 - 100, Raylib.GetScreenHeight() / 2 - 20, 30, new(255, 255, 255, 255));
                                    Raylib.EndDrawing();
                                    continue;
                                }

                                if (tileCheckTask.Result != TileCheckResult.Ok)
                                {
                                    page = 13;
                                    Raylib_CsLo.RayGui.GuiUnlock();
                                    Raylib.EndDrawing();
                                    continue;
                                }


                                cameraCamera.target = new(-100, -100);
                                lightPageCamera.target = new(-500, -200);

                                renderCamers = res.Cameras;
                                geoMatrix = res.GeoMatrix!;
                                tileMatrix = res.TileMatrix!;
                                materialColorMatrix = res.MaterialColorMatrix!;
                                matrixHeight = res.Height;
                                matrixWidth = res.Width;
                                bufferTiles = res.BufferTiles;


                                effectList = res.Effects.Select(effect => (effect.Item1, Effects.GetEffectOptions(effect.Item1), effect.Item2)).ToArray();

                                matrixWidthValue = matrixWidth;
                                matrixHeightValue = matrixHeight;

                                border = new(
                                    bufferTiles.Left * scale,
                                    bufferTiles.Top * scale,
                                    (matrixWidth - (bufferTiles.Right + bufferTiles.Left)) * scale,
                                    (matrixHeight - (bufferTiles.Bottom + bufferTiles.Top)) * scale
                                );

                                var lightMapTexture = Raylib.LoadTextureFromImage(res.LightMapImage);

                                Raylib.UnloadRenderTexture(lightMapBuffer);
                                lightMapBuffer = Raylib.LoadRenderTexture(matrixWidth * scale + 300, matrixHeight * scale + 300);

                                Raylib.BeginTextureMode(lightMapBuffer);
                                Raylib.DrawTextureRec(
                                    lightMapTexture,
                                    new(0, 0, lightMapTexture.width, lightMapTexture.height),
                                    new(0, 0),
                                    new(255, 255, 255, 255)
                                );
                                Raylib.EndTextureMode();

                                Raylib.UnloadImage(res.LightMapImage);

                                projectName = res.Name;
                                page = 1;

                                tileCheckTask = null;
                                Raylib_CsLo.RayGui.GuiUnlock();
                            }
                            else // choosing a project
                            {

                                Raylib.ClearBackground(new(170, 170, 170, 255));

                                Raylib_CsLo.Rectangle panelRect = new(100, 100, Raylib.GetScreenWidth() - 200, Raylib.GetScreenHeight() - 200);

                                fixed (byte* pt = explorerPanelBytes)
                                {
                                    Raylib_CsLo.RayGui.GuiPanel(
                                        panelRect,
                                        (sbyte*)pt
                                    );
                                }

                                //no projects
                                if (projectFiles.Length == 0)
                                {
                                    Raylib.DrawText(
                                        "You have no projects yet",
                                        Raylib.GetScreenWidth() / 2 - 200,
                                        Raylib.GetScreenHeight() / 2 - 50,
                                        30,
                                        new(0, 0, 0, 255)
                                    );

                                    if (Raylib_CsLo.RayGui.GuiButton(new(Raylib.GetScreenWidth() / 2 - 100, Raylib.GetScreenHeight() / 2 + 50, 200, 50), "Create New Project"))
                                    {
                                        newFlag = true;
                                        page = 6;
                                    }
                                }
                                // there are projects
                                else
                                {
                                    Raylib.DrawText("[W] - Page Up  [S] - Page Down", Raylib.GetScreenWidth() / 2 - 220, 150, 30, new(0, 0, 0, 255));

                                    if (maxCount > projectFiles.Length)
                                    {
                                        for (int f = 0; f < projectFiles.Length; f++)
                                        {
                                            if (!projectFiles[f].EndsWith(".txt")) continue;

                                            Raylib_CsLo.RayGui.GuiButton(
                                                new Raylib_CsLo.Rectangle(buttonOffsetX, f * buttonHeight + 210, buttonWidth, buttonHeight - 1),
                                                Path.GetFileNameWithoutExtension(projectFiles[f])
                                            );
                                        }
                                    }
                                    else
                                    {
                                        var currentPage = projectFiles.Where(f => f.EndsWith(".txt")).Skip(maxCount * explorerPage).Take(maxCount);
                                        var counter = 0;

                                        foreach (var f in currentPage)
                                        {
                                            var isPressed = Raylib_CsLo.RayGui.GuiButton(
                                                new Raylib_CsLo.Rectangle(buttonOffsetX, counter * buttonHeight + 210, buttonWidth, buttonHeight - 1),
                                                Path.GetFileNameWithoutExtension(f)
                                            );

                                            if (isPressed)
                                            {
                                                Raylib_CsLo.RayGui.GuiLock();

                                                // LOAD PROJECT FILE
                                                loadFileTask = Task.Factory.StartNew(() => LoadProject(f));
                                            }

                                            counter++;
                                        }
                                    }

                                    Raylib.DrawText(
                                        $"Page {explorerPage}/{maxCount}",
                                        Raylib.GetScreenWidth() / 2 - 90,
                                        Raylib.GetScreenHeight() - 160,
                                        30,
                                        new(0, 0, 0, 255)
                                    );
                                }

                            }
                        }
                        Raylib.EndDrawing();
                        break;
                    #endregion

                    #region SavePage
                    case 12:

                        Raylib.BeginDrawing();
                        {
                            Raylib.ClearBackground(new(170, 170, 170, 255));

                            fixed (byte* pt = saveProjectPanelBytes)
                            {
                                Raylib_CsLo.RayGui.GuiPanel(
                                    new(
                                        Raylib.GetScreenWidth() / 2 - 200,
                                        Raylib.GetScreenHeight() / 2 - 150,
                                        400,
                                        300
                                    ),
                                    (sbyte*)pt
                                );
                            }

                            fixed (byte* bytes = projectNameBufferBytes)
                            {
                                Raylib_CsLo.RayGui.GuiTextBox(
                                    new(
                                        Raylib.GetScreenWidth() / 2 - 150,
                                        Raylib.GetScreenHeight() / 2 - 90,
                                        300,
                                        40
                                    ),
                                    (sbyte*)bytes,
                                    20,
                                    true
                                );
                            }


                            Raylib_CsLo.RayGui.GuiButton(
                                new(
                                    Raylib.GetScreenWidth() / 2 - 150,
                                    Raylib.GetScreenHeight() / 2,
                                    300,
                                    40
                                ),
                                "Save"
                            );

                            var cancelSavePressed = Raylib_CsLo.RayGui.GuiButton(
                                new(
                                    Raylib.GetScreenWidth() / 2 - 150,
                                    Raylib.GetScreenHeight() / 2 + 50,
                                    300,
                                    40
                                ),
                                "Cancel"
                            );

                            if (cancelSavePressed) page = 1;
                        }
                        Raylib.EndDrawing();

                        break;
                    #endregion

                    #region FailedTileCheckPage
                    case 13:
                        var sWidth = GetScreenWidth();
                        var sHeight = GetScreenHeight();

                        var okButtonRect = new Rectangle(sWidth / 2 - 100, sHeight - 200, 200, 60);

                        BeginDrawing();
                        {
                            ClearBackground(new(0, 0, 0, 255));

                            switch (tileCheckTask!.Result)
                            {
                                case TileCheckResult.Missing:
                                    DrawText(
                                        missingTileWarnTitleText,
                                        (sWidth - MeasureText(missingTileWarnTitleText, 50)) / 2,
                                        200,
                                        50,
                                        new(255, 255, 255, 255)
                                    );
                                    DrawText(missingTileWarnSubtitleText, (sWidth - MeasureText(missingTileWarnSubtitleText, 20)) / 2, 400, 20, new(255, 255, 255, 255));
                                    break;

                                case TileCheckResult.NotFound:
                                    DrawText(
                                        notFoundTileWarnTitleText,
                                        (sWidth - MeasureText(notFoundTileWarnTitleText, 50)) / 2,
                                        200,
                                        50,
                                        new(255, 255, 255, 255)
                                    );
                                    DrawText(missingTileWarnSubtitleText, (sWidth - MeasureText(missingTileWarnSubtitleText, 20)) / 2, 400, 20, new(255, 255, 255, 255));
                                    break;

                                case TileCheckResult.MissingTexture:
                                    DrawText(
                                        missingTileTextureWarnTitleText,
                                        (sWidth - MeasureText(missingTileTextureWarnTitleText, 50)) / 2,
                                        200,
                                        50,
                                        new(255, 255, 255, 255)
                                    );
                                    DrawText(missingTileTextureWarnSubTitleText, (sWidth - MeasureText(missingTileTextureWarnSubTitleText, 20)) / 2, 400, 20, new(255, 255, 255, 255));
                                    break;

                                case TileCheckResult.MissingMaterial:
                                    DrawText(
                                        missingMaterialWarnTitleText,
                                        (sWidth - MeasureText(missingMaterialWarnTitleText, 50)) / 2,
                                        200,
                                        50,
                                        new(255, 255, 255, 255)
                                    );

                                    DrawText(
                                        missingMaterialWarnSubtitleText,
                                        (sWidth - MeasureText(missingMaterialWarnSubtitleText, 20)) / 2,
                                        400,
                                        20,
                                        new(255, 255, 255, 255)
                                    );
                                    break;
                            }


                            DrawRectangleRoundedLines(okButtonRect, 3, 6, 3, new(255, 255, 255, 255));
                            DrawText("Ok", okButtonRect.X + (okButtonRect.width - MeasureText("Ok", 20)) / 2, okButtonRect.Y + 15, 20, new(255, 255, 255, 255));

                            if (CheckCollisionPointRec(GetMousePosition(), okButtonRect))
                            {
                                SetMouseCursor(MouseCursor.MOUSE_CURSOR_POINTING_HAND);

                                if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                                {
                                    SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);

                                    tileCheckTask = null;
                                    page = 0;
                                }
                            }
                            else SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
                        }
                        EndDrawing();
                        break;
                    #endregion

                    default:
                        page = prevPage;
                        break;
                }
            }
        }
        catch (Exception e)
        {
            logger.Fatal($"Bruh Moment detected: loop try-catch block has cought an expected error: {e}");
            throw new Exception(innerException: e, message: "Fucked up runtime. Figure it out.");
        }

        logger.Debug("close program detected; exiting main loop");
        logger.Information("unloading textures");

        foreach (var texture in uiTextures) Raylib.UnloadTexture(texture);
        foreach (var texture in geoTextures) Raylib.UnloadTexture(texture);
        foreach (var texture in stackableTextures) Raylib.UnloadTexture(texture);
        foreach (var texture in lightTextures) Raylib.UnloadTexture(texture);
        Raylib.UnloadRenderTexture(lightMapBuffer);

        Raylib.CloseWindow();

        logger.Information("program has terminated");
        return;
    }
}
