global using Raylib_cs;
using System.Numerics;
using System.Text;
using Leditor.Common;
using Microsoft.Win32;
using System.Windows;
using Leditor.Lingo;
using Pidgin;
using System.Collections.Specialized;
using System.Runtime.InteropServices;



namespace Leditor;

class Program
{
    static readonly string[] geoNames = [
        "Solid",
        "air",
        "Slope ES",
        "Slope SW",
        "Rectangle wall",
        "Rectangle air",
        "Slope NE",
        "Slope NW",
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

    static Texture2D[] LoadUITextures() => [
        Raylib.LoadTexture("assets/geo/ui/solid.png"),          // 0
        Raylib.LoadTexture("assets/geo/ui/air.png"),            // 1
        // Raylib.LoadTexture("assets/geo/ui/slopebr.png"),     
        Raylib.LoadTexture("assets/geo/ui/slopebl.png"),        // 2
        Raylib.LoadTexture("assets/geo/ui/multisolid.png"),     // 3
        Raylib.LoadTexture("assets/geo/ui/multiair.png"),       // 4
        // Raylib.LoadTexture("assets/geo/ui/slopetr.png"),        
        // Raylib.LoadTexture("assets/geo/ui/slopetl.png"),        
        Raylib.LoadTexture("assets/geo/ui/platform.png"),       // 5
        Raylib.LoadTexture("assets/geo/ui/move.png"),           // 6
        Raylib.LoadTexture("assets/geo/ui/rock.png"),           // 7
        Raylib.LoadTexture("assets/geo/ui/spear.png"),          // 8
        Raylib.LoadTexture("assets/geo/ui/crack.png"),          // 9
        Raylib.LoadTexture("assets/geo/ui/ph.png"),             // 10
        Raylib.LoadTexture("assets/geo/ui/pv.png"),             // 11
        Raylib.LoadTexture("assets/geo/ui/glass.png"),          // 12
        Raylib.LoadTexture("assets/geo/ui/backcopy.png"),       // 13
        Raylib.LoadTexture("assets/geo/ui/entry.png"),          // 14
        Raylib.LoadTexture("assets/geo/ui/shortcut.png"),       // 15
        Raylib.LoadTexture("assets/geo/ui/den.png"),            // 16
        Raylib.LoadTexture("assets/geo/ui/passage.png"),        // 17
        Raylib.LoadTexture("assets/geo/ui/bathive.png"),        // 18
        Raylib.LoadTexture("assets/geo/ui/waterfall.png"),      // 19
        Raylib.LoadTexture("assets/geo/ui/scav.png"),           // 20
        Raylib.LoadTexture("assets/geo/ui/wack.png"),           // 21
        Raylib.LoadTexture("assets/geo/ui/garbageworm.png"),    // 22
        Raylib.LoadTexture("assets/geo/ui/worm.png"),           // 23
        Raylib.LoadTexture("assets/geo/ui/forbidflychains.png"),// 24
        Raylib.LoadTexture("assets/geo/ui/clearall.png"),       // 25
    ];

    static Texture2D[] LoadGeoTextures() => [
        // 0: air
        Raylib.LoadTexture("assets/geo/solid.png"),
        Raylib.LoadTexture("assets/geo/cbl.png"),
        Raylib.LoadTexture("assets/geo/cbr.png"),
        Raylib.LoadTexture("assets/geo/ctl.png"),
        Raylib.LoadTexture("assets/geo/ctr.png"),
        Raylib.LoadTexture("assets/geo/platform.png"),
        Raylib.LoadTexture("assets/geo/entryblock.png"),
        // 7: NONE
        // 8: NONE
        Raylib.LoadTexture("assets/geo/thickglass.png"),
    ];

    static Texture2D[] LoadStackableTextures() => [
        Raylib.LoadTexture("assets/geo/ph.png"),             // 0
        Raylib.LoadTexture("assets/geo/pv.png"),             // 1
        Raylib.LoadTexture("assets/geo/bathive.png"),        // 2
        Raylib.LoadTexture("assets/geo/dot.png"),            // 3
        Raylib.LoadTexture("assets/geo/crackbl.png"),        // 4
        Raylib.LoadTexture("assets/geo/crackbr.png"),        // 5
        Raylib.LoadTexture("assets/geo/crackc.png"),         // 6
        Raylib.LoadTexture("assets/geo/crackh.png"),         // 7
        Raylib.LoadTexture("assets/geo/cracklbr.png"),       // 8
        Raylib.LoadTexture("assets/geo/cracktbr.png"),       // 9
        Raylib.LoadTexture("assets/geo/cracktl.png"),        // 10
        Raylib.LoadTexture("assets/geo/cracktlb.png"),       // 11
        Raylib.LoadTexture("assets/geo/cracktlr.png"),       // 12
        Raylib.LoadTexture("assets/geo/cracktr.png"),        // 13
        Raylib.LoadTexture("assets/geo/cracku.png"),         // 14
        Raylib.LoadTexture("assets/geo/crackv.png"),         // 15
        Raylib.LoadTexture("assets/geo/garbageworm.png"),    // 16
        Raylib.LoadTexture("assets/geo/scav.png"),           // 17
        Raylib.LoadTexture("assets/geo/rock.png"),           // 18
        Raylib.LoadTexture("assets/geo/waterfall.png"),      // 19
        Raylib.LoadTexture("assets/geo/wack.png"),           // 20
        Raylib.LoadTexture("assets/geo/worm.png"),           // 21
        Raylib.LoadTexture("assets/geo/entryb.png"),         // 22
        Raylib.LoadTexture("assets/geo/entryl.png"),         // 23
        Raylib.LoadTexture("assets/geo/entryr.png"),         // 24
        Raylib.LoadTexture("assets/geo/entryt.png"),         // 25
        Raylib.LoadTexture("assets/geo/looseentry.png"),     // 26
        Raylib.LoadTexture("assets/geo/passage.png"),        // 27
        Raylib.LoadTexture("assets/geo/den.png"),            // 28
        Raylib.LoadTexture("assets/geo/spear.png"),          // 29
        Raylib.LoadTexture("assets/geo/forbidflychains.png"),// 30
        Raylib.LoadTexture("assets/geo/crackb.png"),         // 31
        Raylib.LoadTexture("assets/geo/crackr.png"),         // 32
        Raylib.LoadTexture("assets/geo/crackt.png"),         // 33
        Raylib.LoadTexture("assets/geo/crackl.png"),         // 34
    ];

    static Texture2D[] LoadLightTextures() => [
        Raylib.LoadTexture("assets/light/inverted/Drought_393275_sawbladeGraf.png"),             // 0
        Raylib.LoadTexture("assets/light/inverted/Drought_393400_pentagonLightEmpty.png"),       // 1
        Raylib.LoadTexture("assets/light/inverted/Drought_393401_pentagonLight.png"),            // 2
        Raylib.LoadTexture("assets/light/inverted/Drought_393402_roundedRectLightEmpty.png"),    // 3
        Raylib.LoadTexture("assets/light/inverted/Drought_393403_squareLightEmpty.png"),         // 4
        Raylib.LoadTexture("assets/light/inverted/Drought_393404_triangleLight.png"),            // 5
        Raylib.LoadTexture("assets/light/inverted/Drought_393405_triangleLightEmpty.png"),       // 6
        Raylib.LoadTexture("assets/light/inverted/Drought_393406_curvedTriangleLight.png"),      // 7
        Raylib.LoadTexture("assets/light/inverted/Drought_393407_curvedTriangleLightEmpty.png"), // 8
        Raylib.LoadTexture("assets/light/inverted/Drought_393408_discLightEmpty.png"),           // 9
        Raylib.LoadTexture("assets/light/inverted/Drought_393409_hexagonLight.png"),             // 10
        Raylib.LoadTexture("assets/light/inverted/Drought_393410_hexagonLightEmpty.png"),        // 11
        Raylib.LoadTexture("assets/light/inverted/Drought_393411_octagonLight.png"),             // 12
        Raylib.LoadTexture("assets/light/inverted/Drought_393412_octagonLightEmpty.png"),        // 13
        Raylib.LoadTexture("assets/light/inverted/Internal_265_bigCircle.png"),                  // 14
        Raylib.LoadTexture("assets/light/inverted/Internal_266_leaves.png"),                     // 15
        Raylib.LoadTexture("assets/light/inverted/Internal_267_oilyLight.png"),                  // 16
        Raylib.LoadTexture("assets/light/inverted/Internal_268_directionalLight.png"),           // 17
        Raylib.LoadTexture("assets/light/inverted/Internal_269_blobLight1.png"),                 // 18
        Raylib.LoadTexture("assets/light/inverted/Internal_270_blobLight2.png"),                 // 19
        Raylib.LoadTexture("assets/light/inverted/Internal_271_wormsLight.png"),                 // 20
        Raylib.LoadTexture("assets/light/inverted/Internal_272_crackLight.png"),                 // 21
        Raylib.LoadTexture("assets/light/inverted/Internal_273_squareishLight.png"),             // 22
        Raylib.LoadTexture("assets/light/inverted/Internal_274_holeLight.png"),                  // 23
        Raylib.LoadTexture("assets/light/inverted/Internal_275_roundedRectLight.png"),           // 24
    ];

    static OrderedDictionary LoadTileInit()
    {
        var path = Path.Combine(executablePath, "settings", "Init.txt");

        var text = File.ReadAllText(path).ReplaceLineEndings(Environment.NewLine);

        return Tools.GetTileInit(text);
    }

    static string executablePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

    static string[] projectFiles = Directory.EnumerateFiles(Path.Combine(executablePath, "projects")).Where(f => f.EndsWith(".txt")).ToArray();

    const int screenMinWidth = 1280;
    const int screenMinHeight = 800;

    const int renderCameraWidth = 1400;
    const int renderCameraHeight = 800;

    const int geoselectWidth = 200;
    const int geoselectHeight = 600;
    const int scale = 20;
    const int uiScale = 40;
    static bool camScaleMode = false;
    const int previewScale = 10;
    const float zoomIncrement = 0.125f;
    const int initialMatrixWidth = 72;
    const int initialMatrixHeight = 43;

    static readonly Raylib_cs.Color[] layerColors = [
        new(0, 0, 0, 170),
        new(0, 250, 94, 140),
        new(159, 77, 77, 140),
    ];

    static readonly Raylib_cs.Color whiteStackable = new(255, 255, 255, 200);
    static readonly Raylib_cs.Color blackStackable = new(0, 0, 0, 200);

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

    public static int GetStackablePreviewTextureIndex(int id) => id switch
    {
        1 => 0,
        2 => 1,
    };

    public static int GetBlockPreviewTextureIndex(int id) => id switch
    {
        2 => 3,
        3 => 2,
        4 => 1,
        5 => 0,
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

    public static int GetCorrectSlopeID(RunCell[][] context) {
        return (
            false, context[0][1].Geo == 1, false,
            context[1][0].Geo == 1, false, context[1][2].Geo == 1,
            false, context[2][1].Geo == 1, false
        ) switch {
            (
                _    , false, _    ,
                true,     _,  false,
                _    , true , _
            ) => 2,
            (
                _    , false, _    ,
                false,     _,  true,
                _    , true , _
            ) => 3,
            (
                _    , true , _    ,
                true ,    _ , false,
                _    , false, _
            ) => 4,
            (
                _    , true , _    ,
                false,     _,  true,
                _    , false, _
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
        Raylib_cs.Camera2D camera, 
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
                Raylib_cs.Color.WHITE
            );
        }

        Raylib.DrawRectangleLinesEx(
            new(origin.X, origin.Y, renderCameraWidth, renderCameraHeight),
            4f,
            Raylib_cs.Color.WHITE
        );

        Raylib.DrawRectangleLinesEx(
            new(origin.X, origin.Y, renderCameraWidth, renderCameraHeight),
            2f,
            Raylib_cs.Color.BLACK
        );

        Raylib.DrawCircleLines(
            (int)(origin.X + renderCameraWidth / 2),
            (int)(origin.Y + renderCameraHeight / 2),
            50,
            Raylib_cs.Color.BLACK
        );

        if (hover)
        {
            Raylib.DrawCircleV(
                new(origin.X + renderCameraWidth / 2, origin.Y + renderCameraHeight / 2),
                50,
                new Raylib_cs.Color(255, 255, 255, 100)
            );
        }

        Raylib.DrawLineEx(
            new(origin.X + 4, origin.Y + renderCameraHeight / 2),
            new(origin.X + renderCameraWidth - 4, origin.Y + renderCameraHeight / 2),
            4f,
            Raylib_cs.Color.BLACK
        );

        Raylib.DrawRectangleLinesEx(
            new(
                origin.X + 190,
                origin.Y + 20,
                51 * scale,
                40 * scale - 40
            ),
            4f,
            Raylib_cs.Color.RED
        );

        var quarter1 = new Rectangle(origin.X - 150, origin.Y - 150, renderCameraWidth/2 + 150, renderCameraHeight/2 + 150);
        var quarter2 = new Rectangle(renderCameraWidth/2 + origin.X, origin.Y - 150, renderCameraWidth/2 + 150, renderCameraHeight/2 + 150);
        var quarter3 = new Rectangle(origin.X - 150, origin.Y + renderCameraHeight/2, renderCameraWidth/2 + 150, renderCameraHeight/2 + 150);
        var quarter4 = new Rectangle(renderCameraWidth/2 + origin.X, renderCameraHeight/2 +  origin.Y, renderCameraWidth/2 + 150, renderCameraHeight/2 + 150);
        
        if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)) camScaleMode = false;

        if (Raylib.CheckCollisionPointRec(mouse, quarter1)) {

            if ((Raylib.CheckCollisionPointCircle(mouse, Raymath.Vector2Add(quads.TopLeft, pointOrigin1), 10) || camScaleMode) && 
                Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) {
                camScaleMode = true;

                quads.TopLeft = Raymath.Vector2Subtract(mouse, pointOrigin1);
            }

            Raylib.DrawCircleV(Raymath.Vector2Add(quads.TopLeft, origin), 10, Color.GREEN);
        }


        if (Raylib.CheckCollisionPointRec(mouse, quarter2)) {
            if ((Raylib.CheckCollisionPointCircle(mouse, Raymath.Vector2Add(quads.TopRight, pointOrigin2), 10) || camScaleMode) && 
                Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) {
                camScaleMode = true;
                quads.TopRight = Raymath.Vector2Subtract(mouse, pointOrigin2);
            }

            Raylib.DrawCircleV(Raymath.Vector2Add(quads.TopRight, pointOrigin2), 10, Color.GREEN);
        }

        if (Raylib.CheckCollisionPointRec(mouse, quarter3)) {
            if ((Raylib.CheckCollisionPointCircle(mouse, Raymath.Vector2Add(quads.BottomRight, pointOrigin3), 10) || camScaleMode) && 
                Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) {
                camScaleMode = true;
                quads.BottomRight = Raymath.Vector2Subtract(mouse, pointOrigin3);
            }

            Raylib.DrawCircleV(Raymath.Vector2Add(quads.BottomRight, pointOrigin3), 10, Color.GREEN);
        }

        if (Raylib.CheckCollisionPointRec(mouse, quarter4)) {
            if ((Raylib.CheckCollisionPointCircle(mouse, Raymath.Vector2Add(quads.BottomLeft, pointOrigin4), 10) || camScaleMode) && 
                Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) {
                camScaleMode = true;
                quads.BottomLeft = Raymath.Vector2Subtract(mouse, pointOrigin4);
            }

            Raylib.DrawCircleV(Raymath.Vector2Add(quads.BottomLeft, pointOrigin4), 10, Color.GREEN);
        }

        return (hover && Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT), biggerHover);
    }

    static unsafe void Main(string[] args)
    {
        int page = 0;
        int prevPage = 0;

        BufferTiles bufferTiles = new(12, 12, 3, 3);

        int tileSeed = 141;
        bool lightMode = true;
        bool defaultTerrain = true;

        int matrixWidth = initialMatrixWidth;
        int matrixHeight = initialMatrixHeight;

        uint geoSelectionX = 0;
        uint geoSelectionY = 0;

        int currentLayer = 0;

        bool showLayer1 = true;
        bool showLayer2 = true;
        bool showLayer3 = true;

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
        (string, EffectOptions, double[,])[] effectList = [];
        bool[,] lightMatrix = CommonUtils.NewLightMatrix(matrixWidth, matrixHeight, scale);

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

        // UNSAFE VARIABLES

        int matrixWidthValue = 72;  // default width
        int matrixHeightValue = 43; // default height
        var panelBytes = Encoding.ASCII.GetBytes("New Level");
        int leftPadding = 12;
        int rightPadding = 12;
        int topPadding = 3;
        int bottomPadding = 3;

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

        //

        Raylib_cs.Camera2D camera = new() { Zoom = 1.0f };
        Raylib_cs.Camera2D mainPageCamera = new() { Zoom = 0.5f };
        Raylib_cs.Camera2D effectsCamera = new() { Zoom = 0.8f };
        Raylib_cs.Camera2D lightPageCamera = new() { Zoom = 0.5f, Target = new(-500, -200) };
        Raylib_cs.Camera2D cameraCamera = new() { Zoom = 0.8f, Target = new(-100, -100) };

        Raylib_cs.Rectangle border = new(
            bufferTiles.Left * scale,
            bufferTiles.Top * scale,
            (matrixWidth - (bufferTiles.Right * 2)) * scale,
            (matrixHeight - (bufferTiles.Bottom * 2)) * scale
            );

        Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        Raylib.InitWindow(screenMinWidth, screenMinHeight, "Henry's Leditor");
        Raylib.SetWindowMinSize(screenMinWidth, screenMinHeight);

        RenderTexture2D lightMapBuffer = Raylib.LoadRenderTexture(
            matrixWidth * scale + 300,
            matrixHeight * scale + 300
        );

        Texture2D[] uiTextures = LoadUITextures();
        Texture2D[] geoTextures = LoadGeoTextures();
        Texture2D[] stackableTextures = LoadStackableTextures();
        Texture2D[] lightTextures = LoadLightTextures();

        Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose())
        {

            switch (page)
            {

                #region StartPage
                case 0:
                    prevPage = 0;

                    Raylib.BeginDrawing();
                    {
                        Raylib.ClearBackground(Raylib_cs.Color.GRAY);

                        if (Raylib_CsLo.RayGui.GuiButton(new(Raylib.GetScreenWidth() / 2 - 150, Raylib.GetScreenHeight() / 2 - 40, 300, 40), "Create"))
                        {
                            newFlag = true;
                            page = 6;
                        }

                        if (Raylib_CsLo.RayGui.GuiButton(new(Raylib.GetScreenWidth() / 2 - 150, Raylib.GetScreenHeight() / 2, 300, 40), "Load"))
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
                        mainPageCamera.Offset = Raylib.GetMousePosition();
                        mainPageCamera.Target = mouseWorldPosition;
                        mainPageCamera.Zoom += mainPageWheel * zoomIncrement;
                        if (mainPageCamera.Zoom < zoomIncrement) mainPageCamera.Zoom = zoomIncrement;
                    }

                    // handle mouse drag
                    if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
                    {
                        Vector2 delta = Raylib.GetMouseDelta();
                        delta = Raymath.Vector2Scale(delta, -1.0f / mainPageCamera.Zoom);
                        mainPageCamera.Target = Raymath.Vector2Add(mainPageCamera.Target, delta);
                    }

                    Raylib.BeginDrawing();
                    {
                        Raylib.ClearBackground(Raylib_cs.Color.GRAY);

                        Raylib.BeginMode2D(mainPageCamera);
                        {
                            Raylib.DrawRectangle(0, 0, matrixWidth * scale, matrixHeight * scale, Color.WHITE);

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
                                    Raylib_cs.Color.DARKBLUE
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
                                Raylib_cs.Color.BLACK
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

                            Raylib.DrawText("Seed", Raylib.GetScreenWidth() - 380, 205, 11, Raylib_cs.Color.BLACK);

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
                                Raylib_cs.Color.BLACK
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
                            if (newPressed) page = 6;
                        }

                    }
                    Raylib.EndDrawing();

                    break;
                #endregion

                #region GeoEditor
                case 2:
                    prevPage = 2;

                    if (Raylib.IsKeyPressed(KeyboardKey.KEY_ONE))
                    {
                        page = 1;
                    }
                    // if (Raylib.IsKeyReleased(KeyboardKey.KEY_TWO)) page = 2;
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

                    Vector2 mouse = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);

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
                        delta = Raymath.Vector2Scale(delta, -1.0f / camera.Zoom);
                        camera.Target = Raymath.Vector2Add(camera.Target, delta);
                    }


                    // handle zoom
                    var wheel = Raylib.GetMouseWheelMove();
                    if (wheel != 0)
                    {
                        Vector2 mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
                        camera.Offset = Raylib.GetMousePosition();
                        camera.Target = mouseWorldPosition;
                        camera.Zoom += wheel * zoomIncrement;
                        if (camera.Zoom < zoomIncrement) camera.Zoom = zoomIncrement;
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
                                        matrixX * scale >= border.Width + border.X || 
                                        matrixY * scale < border.Y || 
                                        matrixY * scale >= border.Height + border.Y) {
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

                    if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
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

                                        for (int y = startY; y < endY; y++)
                                        {
                                            for (int x = startX; x < endX; x++)
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
                                            for (int y = startY; y < endY; y++)
                                            {
                                                for (int x = startX; x < endX; x++)
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

                                        for (int y = startY; y < endY; y++)
                                        {
                                            for (int x = startX; x < endX; x++)
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

                    Raylib.BeginDrawing();
                    {
                        Raylib.ClearBackground(new Raylib_cs.Color(136, 136, 136, 255));


                        Raylib.BeginMode2D(camera);
                        {
                            if (!gridContrast)
                            {
                                // the grid
                                Rlgl.PushMatrix();
                                {
                                    Rlgl.Translatef(0, 0, -1);
                                    Rlgl.Rotatef(90, 1, 0, 0);
                                    Raylib.DrawGrid(matrixHeight > matrixWidth ? matrixHeight * 2 : matrixWidth * 2, scale);
                                }
                                Rlgl.PopMatrix();
                            }

                            // geo matrix
                            for (int y = 0; y < matrixHeight; y++)
                            {
                                for (int x = 0; x < matrixWidth; x++)
                                {
                                    for (int z = 2; z > -1; z--)
                                    {
                                        if (z == 0 && !showLayer1) continue;
                                        if (z == 1 && !showLayer2) continue;
                                        if (z == 2 && !showLayer3) continue;

                                        var cell = geoMatrix[y, x, z];

                                        var texture = GetBlockIndex(cell.Geo);

                                        if (texture >= 0)
                                        {
                                            Raylib.DrawTexture(geoTextures[texture], x * scale, y * scale, layerColors[z]);
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
                                                        Raylib.DrawTexture(stackableTextures[GetStackableTextureIndex(s)], x * scale, y * scale, whiteStackable); // TODO: remove opacity from entrances
                                                        break;

                                                    // directional placement
                                                    case 4:     // entrance
                                                        var index = GetStackableTextureIndex(s, CommonUtils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, z));

                                                        if (index is 22 or 23 or 24 or 25)
                                                        {
                                                            geoMatrix[y, x, 0].Geo = 7;
                                                        }

                                                        Raylib.DrawTexture(stackableTextures[index], x * scale, y * scale, whiteStackable);
                                                        break;
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

                            if (gridContrast)
                            {
                                // the grid
                                Rlgl.PushMatrix();
                                {
                                    Rlgl.Translatef(0, 0, -1);
                                    Rlgl.Rotatef(90, 1, 0, 0);
                                    Raylib.DrawGrid(matrixHeight > matrixWidth ? matrixHeight * 2 : matrixWidth * 2, scale);
                                }
                                Rlgl.PopMatrix();
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
                                        var width = Math.Abs(XS) * scale;
                                        var height = Math.Abs(YS) * scale;

                                        Raylib_cs.Rectangle rec = (XS > 0, YS > 0) switch
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

                                        Raylib.DrawRectangleLinesEx(rec, 2, Raylib_cs.Color.RED);
                                        Raylib.DrawText(
                                            $"{width / scale:0}x{height / scale:0}",
                                            (int)mouse.X + 10,
                                            (int)mouse.Y,
                                            4,
                                            Raylib_cs.Color.WHITE
                                            );
                                    }
                                    else
                                    {
                                        if (matrixX == x && matrixY == y)
                                        {
                                            Raylib.DrawRectangleLinesEx(new(x * scale, y * scale, scale, scale), 2, Raylib_cs.Color.RED);
                                        }
                                    }
                                }
                            }

                            // the outbound border
                            Raylib.DrawRectangleLinesEx(new(0, 0, matrixWidth * scale, matrixHeight * scale), 2, Raylib_cs.Color.BLACK);

                            // the border
                            Raylib.DrawRectangleLinesEx(border, camera.Zoom < zoomIncrement ? 5 : 2, Raylib_cs.Color.WHITE);

                            // a lazy way to hide the rest of the grid
                            Raylib.DrawRectangle(matrixWidth * -scale, -3, matrixWidth * scale, matrixHeight * 2 * scale, Raylib_cs.Color.GRAY);
                            Raylib.DrawRectangle(0, matrixHeight * scale, matrixWidth * scale + 2, matrixHeight * scale, Raylib_cs.Color.GRAY);
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
                                        Raylib_cs.Color.BLACK
                                    );
                                }

                                if (w == geoSelectionX && h == geoSelectionY)
                                    Raylib.DrawRectangleLinesEx(new(Raylib.GetScreenWidth() - 195 + w * uiScale + 5, h * uiScale + 100, uiScale, uiScale), 2, Raylib_cs.Color.RED);
                                else
                                    Raylib.DrawRectangleLinesEx(new(Raylib.GetScreenWidth() - 195 + w * uiScale + 5, h * uiScale + 100, uiScale, uiScale), 1, Raylib_cs.Color.BLACK);
                            }
                        }

                        if (geoIndex < uiTextures.Length) Raylib.DrawText(geoNames[geoIndex], Raylib.GetScreenWidth() - 190, 8 * uiScale + 110, 18, Raylib_cs.Color.BLACK);

                        switch (currentLayer)
                        {
                            case 0:
                                Raylib.DrawRectangle(Raylib.GetScreenWidth() - 190, 8 * uiScale + 140, 40, 40, Raylib_cs.Color.BLACK);
                                Raylib.DrawText("L1", Raylib.GetScreenWidth() - 182, 8 * uiScale + 148, 26, Raylib_cs.Color.WHITE);
                                break;
                            case 1:
                                Raylib.DrawRectangle(Raylib.GetScreenWidth() - 190, 8 * uiScale + 140, 40, 40, Raylib_cs.Color.DARKGREEN);
                                Raylib.DrawText("L2", Raylib.GetScreenWidth() - 182, 8 * uiScale + 148, 26, Raylib_cs.Color.WHITE);
                                break;
                            case 2:
                                Raylib.DrawRectangle(Raylib.GetScreenWidth() - 190, 8 * uiScale + 140, 40, 40, Raylib_cs.Color.RED);
                                Raylib.DrawText("L3", Raylib.GetScreenWidth() - 182, 8 * uiScale + 148, 26, Raylib_cs.Color.WHITE);
                                break;
                        }

                        if (matrixX >= 0 && matrixX < matrixWidth && matrixY >= 0 && matrixY < matrixHeight)
                            Raylib.DrawText(
                                $"X = {matrixX:0}\nY = {matrixY:0}",
                                Raylib.GetScreenWidth() - 195,
                                Raylib.GetScreenHeight() - 100,
                                12,
                                Raylib_cs.Color.BLACK);

                        else Raylib.DrawText(
                                $"X = -\nY = -",
                                Raylib.GetScreenWidth() - 195,
                                Raylib.GetScreenHeight() - 100,
                                12,
                                Raylib_cs.Color.BLACK);

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
                        delta = Raymath.Vector2Scale(delta, -1.0f / cameraCamera.Zoom);
                        cameraCamera.Target = Raymath.Vector2Add(cameraCamera.Target, delta);
                    }

                    // handle zoom
                    var cameraWheel = Raylib.GetMouseWheelMove();
                    if (cameraWheel != 0)
                    {
                        Vector2 mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), cameraCamera);
                        cameraCamera.Offset = Raylib.GetMousePosition();
                        cameraCamera.Target = mouseWorldPosition;
                        cameraCamera.Zoom += cameraWheel * zoomIncrement;
                        if (cameraCamera.Zoom < zoomIncrement) cameraCamera.Zoom = zoomIncrement;
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
                        renderCamers = [ ..renderCamers, new() { Coords = (0, 0), Quads = new(new(), new(), new(), new()) } ];
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
                        Raylib.ClearBackground(Raylib_cs.Color.GRAY);

                        Raylib.BeginMode2D(cameraCamera);
                        {

                            Raylib.DrawRectangle(
                                0, 0,
                                matrixWidth * scale,
                                matrixHeight * scale,
                                Raylib_cs.Color.WHITE
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
                                    Raylib_cs.Color.DARKBLUE
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
                                Raylib_cs.Color.PINK
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
                        delta = Raymath.Vector2Scale(delta, -1.0f / lightPageCamera.Zoom);
                        lightPageCamera.Target = Raymath.Vector2Add(lightPageCamera.Target, delta);
                    }


                    // handle zoom
                    var wheel2 = Raylib.GetMouseWheelMove();
                    if (wheel2 != 0)
                    {
                        Vector2 mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), lightPageCamera);
                        lightPageCamera.Offset = Raylib.GetMousePosition();
                        lightPageCamera.Target = mouseWorldPosition;
                        lightPageCamera.Zoom += wheel2 * zoomIncrement;
                        if (lightPageCamera.Zoom < zoomIncrement) lightPageCamera.Zoom = zoomIncrement;
                    }

                    // update light brush

                    {
                        var texture = lightTextures[lightBrushTextureIndex];
                        var lightMouse = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), lightPageCamera);

                        lightBrushSource = new(0, 0, texture.Width, texture.Height);
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
                                    Raylib_cs.Color.WHITE
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
                                    Raylib_cs.Color.BLACK
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
                            lightRecSize = Raymath.Vector2Add(lightRecSize, new(0, growthFactor));
                            growthFactor += 0.03f;
                        }

                        if (Raylib.IsKeyDown(KeyboardKey.KEY_S))
                        {
                            lightRecSize = Raymath.Vector2Add(lightRecSize, new(0, -growthFactor));
                            growthFactor += 0.03f;
                        }

                        if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
                        {
                            lightRecSize = Raymath.Vector2Add(lightRecSize, new(growthFactor, 0));
                            growthFactor += 0.03f;
                        }

                        if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
                        {
                            lightRecSize = Raymath.Vector2Add(lightRecSize, new(-growthFactor, 0));
                            growthFactor += 0.03f;
                        }

                        if (Raylib.IsKeyReleased(KeyboardKey.KEY_W) ||
                            Raylib.IsKeyReleased(KeyboardKey.KEY_S) ||
                            Raylib.IsKeyReleased(KeyboardKey.KEY_D) ||
                            Raylib.IsKeyReleased(KeyboardKey.KEY_A)) growthFactor = initialGrowthFactor;
                    }
                    else
                    {
                        if (Raylib.IsKeyDown(KeyboardKey.KEY_W)) lightRecSize = Raymath.Vector2Add(lightRecSize, new(0, 3));
                        if (Raylib.IsKeyDown(KeyboardKey.KEY_S)) lightRecSize = Raymath.Vector2Add(lightRecSize, new(0, -3));
                        if (Raylib.IsKeyDown(KeyboardKey.KEY_D)) lightRecSize = Raymath.Vector2Add(lightRecSize, new(3, 0));
                        if (Raylib.IsKeyDown(KeyboardKey.KEY_A)) lightRecSize = Raymath.Vector2Add(lightRecSize, new(-3, 0));
                    }


                    Raylib.BeginDrawing();
                    {
                        Raylib.ClearBackground(Raylib_cs.Color.BLUE);

                        Raylib.BeginMode2D(lightPageCamera);
                        {
                            Raylib.DrawRectangle(
                                0, 0,
                                matrixWidth * scale + 300,
                                matrixHeight * scale + 300,
                                Raylib_cs.Color.WHITE
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
                                    Raylib_cs.Color.DARKBLUE
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
                                lightMapBuffer.Texture,
                                new Raylib_cs.Rectangle(0, 0, lightMapBuffer.Texture.Width, -lightMapBuffer.Texture.Height),
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
                                    Raylib_cs.Color.PINK
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
                                    Raylib_cs.Color.RED
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
                                    new(0, 0, texture.Width, texture.Height),
                                    new(20, (textureSize + 1) * pageIndex + 80, textureSize, textureSize),
                                    new(0, 0),
                                    0,
                                    Raylib_cs.Color.BLACK
                                    );

                                if (index == lightBrushTextureIndex) Raylib.DrawRectangleLinesEx(
                                    new(
                                        20,
                                        (textureSize + 1) * pageIndex + 80,
                                        textureSize,
                                        textureSize
                                    ),
                                    2.0f,
                                    Raylib_cs.Color.BLUE
                                    );
                            }
                        }


                        // angle & flatness indicator

                        Raylib.DrawCircleLines(
                            Raylib.GetScreenWidth() - 100,
                            Raylib.GetScreenHeight() - 100,
                            50.0f,
                            Raylib_cs.Color.RED
                        );

                        Raylib.DrawCircleLines(
                            Raylib.GetScreenWidth() - 100,
                            Raylib.GetScreenHeight() - 100,
                            15 + (flatness * 7),
                            Raylib_cs.Color.RED
                        );


                        Raylib.DrawCircleV(new Vector2(
                            (Raylib.GetScreenWidth() - 100) + (float)((15 + flatness * 7) * Math.Cos(lightAngle)),
                            (Raylib.GetScreenHeight() - 100) + (float)((15 + flatness * 7) * Math.Sin(lightAngle))
                            ),
                            10.0f,
                            Raylib_cs.Color.RED
                        );


                    }
                    Raylib.EndDrawing();
                    break;
                #endregion

                #region DimensionsEditor
                case 6:
                    Raylib.BeginDrawing();
                    {
                        Raylib.ClearBackground(Raylib_cs.Color.GRAY);

                        fixed (byte* pt = panelBytes)
                        {
                            Raylib_CsLo.RayGui.GuiPanel(new(30, 60, Raylib.GetScreenWidth() - 60, Raylib.GetScreenHeight() - 120), (sbyte*)pt);
                        }

                        Raylib.DrawText("Width", 50, 110, 20, Raylib_cs.Color.BLACK);
                        if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 100, 300, 40), "", &matrixWidthValue, 72, 999, editControl == 0))
                        {
                            editControl = 0;
                        }

                        Raylib.DrawText("Height", 50, 160, 20, Raylib_cs.Color.BLACK);
                        if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 150, 300, 40), "", &matrixHeightValue, 43, 999, editControl == 1))
                        {
                            editControl = 1;
                        }

                        Raylib_CsLo.RayGui.GuiLine(new(50, 200, Raylib.GetScreenWidth() - 100, 40), "Padding");

                        Raylib.DrawText("Left", 50, 260, 20, Raylib_cs.Color.BLACK);
                        if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 250, 300, 40), "", &leftPadding, 0, 333, editControl == 2))
                        {
                            editControl = 2;
                        }

                        Raylib.DrawText("Right", 50, 310, 20, Raylib_cs.Color.BLACK);
                        if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 300, 300, 40), "", &rightPadding, 0, 333, editControl == 3))
                        {
                            editControl = 3;
                        }

                        Raylib.DrawText("Top", 50, 360, 20, Raylib_cs.Color.BLACK);
                        if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 350, 300, 40), "", &topPadding, 0, 111, editControl == 4))
                        {
                            editControl = 4;
                        }

                        Raylib.DrawText("Bottom", 50, 410, 20, Raylib_cs.Color.BLACK);
                        if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 400, 300, 40), "", &bottomPadding, 0, 111, editControl == 5))
                        {
                            editControl = 5;
                        }

                        if (Raylib_CsLo.RayGui.GuiButton(new(360, Raylib.GetScreenHeight() - 160, 300, 40), "Ok"))
                        {
                            Raylib.EndDrawing();

                            if (resizeFlag)
                            {
                                if (
                                    matrixHeight != matrixHeightValue ||
                                    matrixWidth != matrixWidthValue)
                                {
                                    geoMatrix = CommonUtils.Resize(
                                        geoMatrix,
                                        matrixWidth,
                                        matrixHeight,
                                        matrixWidthValue,
                                        matrixHeightValue
                                        );

                                    // lightMatrix = CommonUtils.Resize(
                                    // 	lightMatrix, 
                                    // 	matrixWidth, 
                                    // 	matrixHeight,
                                    // 	matrixWidthValue,
                                    // 	matrixHeightValue, 
                                    // 	scale
                                    // 	);

                                    matrixHeight = matrixHeightValue;
                                    matrixWidth = matrixWidthValue;

                                    var img = Raylib.LoadImageFromTexture(lightMapBuffer.Texture);
                                    Raylib.ImageFlipVertical(&img);
                                    Raylib.ImageResizeCanvas(ref img, matrixWidth * scale + 300, matrixHeight * scale + 300, 0, 0, Raylib_cs.Color.WHITE);
                                    Raylib.ImageFlipVertical(&img);

                                    var texture = Raylib.LoadTextureFromImage(img);
                                    Raylib.UnloadImage(img);
                                    lightMapBuffer.Texture = texture;
                                    Raylib.UnloadTexture(texture);
                                }

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

                                border = new(
                                    bufferTiles.Left * scale,
                                    bufferTiles.Top * scale,
                                    (matrixWidth - (bufferTiles.Right * 2)) * scale,
                                    (matrixHeight - (bufferTiles.Bottom * 2)) * scale
                                );

                                resizeFlag = false;
                            }
                            else if (newFlag)
                            {
                                geoMatrix = CommonUtils.NewGeoMatrix(matrixWidthValue, matrixHeightValue, 1);

                                matrixHeight = matrixHeightValue;
                                matrixWidth = matrixWidthValue;

                                Raylib.UnloadRenderTexture(lightMapBuffer);
                                lightMapBuffer = Raylib.LoadRenderTexture((matrixWidth * scale) + 300, (matrixHeight * scale) + 300);

                                Raylib.BeginTextureMode(lightMapBuffer);
                                Raylib.ClearBackground(Raylib_cs.Color.WHITE);
                                Raylib.EndTextureMode();

                                renderCamers = [new RenderCamera() { Coords = (20f, 30f), Quads = new(new(), new(), new(), new()) }];

                                newFlag = false;
                            }

                            page = 1;
                        }

                        if (Raylib_CsLo.RayGui.GuiButton(new(50, Raylib.GetScreenHeight() - 160, 300, 40), "Cancel"))
                        {
                            Raylib.EndDrawing();

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
                                new Raylib_cs.Color(0, 0, 0, 90)
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
                                Raylib_cs.Color.BLUE
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
                                Raylib_cs.Color.BLUE
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
                            delta = Raymath.Vector2Scale(delta, -1.0f / effectsCamera.Zoom);
                            effectsCamera.Target = Raymath.Vector2Add(effectsCamera.Target, delta);
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

                            Raylib.ClearBackground(Raylib_cs.Color.BLACK);

                            Raylib.BeginMode2D(effectsCamera);
                            {
                                Raylib.DrawRectangleLinesEx(
                                    new Rectangle(
                                        -2, -2,
                                        (matrixWidth * scale) + 4,
                                        (matrixHeight * scale) + 4
                                    ),
                                    2f,
                                    Color.WHITE
                                );
                                Raylib.DrawRectangle(0, 0, matrixWidth * scale, matrixHeight * scale, Color.WHITE);

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
                                        Raylib_cs.Color.DARKBLUE
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
                                                    new Raylib_cs.Rectangle(
                                                        x * scale,
                                                        y * scale,
                                                        scale,
                                                        scale
                                                    ),
                                                    2.0f,
                                                    Raylib_cs.Color.RED
                                                );


                                                Raylib.DrawRectangleLines(
                                                    (effectsMatrixX - brushRadius) * scale,
                                                    (effectsMatrixY - brushRadius) * scale,
                                                    (brushRadius * 2 + 1) * scale,
                                                    (brushRadius * 2 + 1) * scale,
                                                    Raylib_cs.Color.RED);
                                            }
                                            else
                                            {
                                                Raylib.DrawRectangleLinesEx(
                                                    new Raylib_cs.Rectangle(
                                                        x * scale,
                                                        y * scale,
                                                        scale,
                                                        scale
                                                    ),
                                                    2.0f,
                                                    Raylib_cs.Color.WHITE
                                                );

                                                Raylib.DrawRectangleLines(
                                                    (effectsMatrixX - brushRadius) * scale,
                                                    (effectsMatrixY - brushRadius) * scale,
                                                    (brushRadius * 2 + 1) * scale,
                                                    (brushRadius * 2 + 1) * scale,
                                                    Raylib_cs.Color.WHITE);
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
                                    Color.BLACK
                                );

                                if (oi == currentAppliedEffect) Raylib.DrawRectangleLinesEx(
                                    new(
                                        Raylib.GetScreenWidth() - 290,
                                        130 + (35 * i),
                                        260,
                                        appliedEffectRecHeight
                                    ),
                                    2.0f,
                                    Raylib_cs.Color.BLUE
                                );

                                Raylib.DrawText(
                                    e.Item1,
                                    Raylib.GetScreenWidth() - 280,
                                    138 + (35 * i),
                                    14,
                                    Raylib_cs.Color.BLACK
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

                                var options = effectList[currentAppliedEffect].Item2;

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
                        Raylib.ClearBackground(Raylib_cs.Color.GRAY);

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
                            Raylib_cs.Color.GRAY
                        );

                        switch (helpSubSection)
                        {
                            case 0:
                                Raylib.DrawText(
                                    " [1] - Main screen\n[2] - Geometry editor\n[3] - Tiles editor\n[4] - Cameras editor\n" +
                                    "[5] - Light editor\n[6] - Edit dimensions\n[7] - Effects editor\n[8] - Props editor",
                                    400,
                                    160,
                                    20,
                                    Raylib_cs.Color.BLACK
                                );
                                break;

                            case 1:
                                Raylib.DrawText(
                                    "[W] [A] [S] [D] - Navigate the geometry tiles menu\n" +
                                    "[L] - Change current layer\n" +
                                    "[M] - Toggle grid (contrast)",
                                    400,
                                    160,
                                    20,
                                    Raylib_cs.Color.BLACK
                                );
                                break;

                            case 2:
                                Raylib.DrawText(
                                    "[N] - New Camera\n" +
                                    "[D] - Delete dragged camera\n"+
                                    "[SPACE] - Do both\n"+
                                    "[LEFT CLICK]  - Move a camera around\n" +
                                    "[RIGHT CLICK] - Move around",
                                    400,
                                    160,
                                    20,
                                    Raylib_cs.Color.BLACK
                                );
                                break;

                            case 3:
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
                                    Raylib_cs.Color.BLACK
                                );
                                break;

                            case 4:
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
                                    Raylib_cs.Color.BLACK
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

                    if (Raylib.IsKeyPressed(KeyboardKey.KEY_W) && explorerPage > 0) explorerPage--;
                    if (Raylib.IsKeyPressed(KeyboardKey.KEY_S) && explorerPage < (projectFiles.Length / maxCount)) explorerPage++;


                    Raylib.BeginDrawing();
                    {
                        if (Raylib_CsLo.RayGui.GuiIsLocked())
                        {
                            Raylib.ClearBackground(new Raylib_cs.Color(0, 0, 0, 130));

                            Raylib.DrawText("Please wait..", Raylib.GetScreenWidth() / 2 - 100, Raylib.GetScreenHeight() / 2 - 20, 30, Raylib_cs.Color.WHITE);

                            if (loadFileTask.IsCompleted)
                            {
                                var res = loadFileTask.Result;
                                if (res.Success)
                                {
                                    cameraCamera.Target = new(-100, -100);
                                    lightPageCamera.Target = new(-500, -200);

                                    renderCamers = res.Cameras;
                                    geoMatrix = res.GeoMatrix;
                                    matrixHeight = res.Height;
                                    matrixWidth = res.Width;
                                    bufferTiles = res.BufferTiles;

                                    effectList = res.Effects.Select(effect => (effect.Item1, Effects.GetEffectOptions(effect.Item1), effect.Item2)).ToArray();

                                    matrixWidthValue = matrixWidth;
                                    matrixHeightValue = matrixHeight;

                                    var lightMapTexture = Raylib.LoadTextureFromImage(res.LightMapImage);

                                    Raylib.UnloadRenderTexture(lightMapBuffer);
                                    lightMapBuffer = Raylib.LoadRenderTexture(matrixWidth * scale + 300, matrixHeight * scale + 300);

                                    Raylib.BeginTextureMode(lightMapBuffer);
                                    Raylib.DrawTextureRec(
                                        lightMapTexture,
                                        new(0, 0, lightMapTexture.Width, lightMapTexture.Height),
                                        new(0, 0),
                                        Raylib_cs.Color.WHITE
                                    );
                                    Raylib.EndTextureMode();

                                    Raylib.UnloadImage(res.LightMapImage);

                                    projectName = res.Name;
                                    page = 1;
                                }
                                else
                                {
                                    Console.WriteLine("FAIL");
                                }

                                Raylib_CsLo.RayGui.GuiUnlock();
                            }
                        }
                        else
                        {

                            Raylib.ClearBackground(Raylib_cs.Color.GRAY);

                            fixed (byte* pt = explorerPanelBytes)
                            {
                                Raylib_CsLo.RayGui.GuiPanel(
                                    new(100, 100, Raylib.GetScreenWidth() - 200, Raylib.GetScreenHeight() - 200),
                                    (sbyte*)pt
                                );
                            }

                            Raylib.DrawText("[W] - Page Up  [S] - Page Down", Raylib.GetScreenWidth() / 2 - 220, 150, 30, Raylib_cs.Color.BLACK);

                            if (maxCount > projectFiles.Length)
                            {
                                for (int f = 0; f < projectFiles.Length; f++)
                                {
                                    Raylib_CsLo.RayGui.GuiButton(
                                        new Raylib_CsLo.Rectangle(buttonOffsetX, f * buttonHeight + 210, buttonWidth, buttonHeight - 1),
                                        Path.GetFileNameWithoutExtension(projectFiles[f])
                                    );
                                }
                            }
                            else
                            {
                                var currentPage = projectFiles.Skip(maxCount * explorerPage).Take(maxCount);
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

                                        loadFileTask = Task.Factory.StartNew(() =>
                                        {
                                            try
                                            {
                                                var text = File.ReadAllText(f).Split('\r');

                                                var lightMapFileName = Path.Combine(Path.GetDirectoryName(f), Path.GetFileNameWithoutExtension(f) + ".png");

                                                if (!File.Exists(lightMapFileName)) return new();

                                                var lightMap = Raylib.LoadImage(lightMapFileName);

                                                Console.WriteLine($"Seems like this is fine");

                                                if (text.Length < 7) return new LoadFileResult();

                                                var obj = Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[0]);
                                                var obj2 = Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[5]);
                                                var effObj = Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[2]);
                                                var camsObj = Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[6]);

                                                var mtx = Tools.GetGeoMatrix(obj, out int givenHeight, out int givenWidth);
                                                var buffers = Tools.GetBufferTiles(obj2);
                                                var effects = Tools.GetEffects(effObj, givenWidth, givenHeight);
                                                var cams = Tools.GetCameras(camsObj);

                                                return new()
                                                {
                                                    Success = true,
                                                    Width = givenWidth,
                                                    Height = givenHeight,
                                                    BufferTiles = buffers,
                                                    GeoMatrix = mtx,
                                                    Effects = effects,
                                                    LightMapImage = lightMap,
                                                    Cameras = cams,
                                                    Name = Path.GetFileNameWithoutExtension(f)
                                                };
                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine(e);
                                                return new();
                                            }
                                        });
                                    }

                                    counter++;
                                }
                            }

                            Raylib.DrawText(
                                $"Page {explorerPage}/{maxCount}",
                                Raylib.GetScreenWidth() / 2 - 90,
                                Raylib.GetScreenHeight() - 160,
                                30,
                                Raylib_cs.Color.BLACK
                            );
                        }
                    }
                    Raylib.EndDrawing();
                    break;
                #endregion

                #region SavePage
                case 12:

                    Raylib.BeginDrawing();
                    {
                        Raylib.ClearBackground(Raylib_cs.Color.GRAY);

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

                default:
                    page = 1;
                    break;
            }
        }

        foreach (var texture in uiTextures) Raylib.UnloadTexture(texture);
        foreach (var texture in geoTextures) Raylib.UnloadTexture(texture);
        foreach (var texture in stackableTextures) Raylib.UnloadTexture(texture);
        foreach (var texture in lightTextures) Raylib.UnloadTexture(texture);
        Raylib.UnloadRenderTexture(lightMapBuffer);

        Raylib.CloseWindow();
        return;
    }
}
