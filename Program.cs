global using Raylib_cs;
using System.Numerics;
using System.Text;
namespace Leditor;

class Program {
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
        Raylib.LoadTexture("assets/geo/ui/slopebr.png"),        // 2
        Raylib.LoadTexture("assets/geo/ui/slopebl.png"),        // 3
        Raylib.LoadTexture("assets/geo/ui/multisolid.png"),     // 4
        Raylib.LoadTexture("assets/geo/ui/multiair.png"),       // 5
        Raylib.LoadTexture("assets/geo/ui/slopetr.png"),        // 6
        Raylib.LoadTexture("assets/geo/ui/slopetl.png"),        // 7
        Raylib.LoadTexture("assets/geo/ui/platform.png"),       // 8
        Raylib.LoadTexture("assets/geo/ui/move.png"),           // 9
        Raylib.LoadTexture("assets/geo/ui/rock.png"),           // 10
        Raylib.LoadTexture("assets/geo/ui/spear.png"),          // 11
        Raylib.LoadTexture("assets/geo/ui/crack.png"),          // 12
        Raylib.LoadTexture("assets/geo/ui/ph.png"),             // 13
        Raylib.LoadTexture("assets/geo/ui/pv.png"),             // 14
        Raylib.LoadTexture("assets/geo/ui/glass.png"),          // 15
        Raylib.LoadTexture("assets/geo/ui/backcopy.png"),       // 16
        Raylib.LoadTexture("assets/geo/ui/entry.png"),          // 17
        Raylib.LoadTexture("assets/geo/ui/shortcut.png"),       // 18
        Raylib.LoadTexture("assets/geo/ui/den.png"),            // 19
        Raylib.LoadTexture("assets/geo/ui/passage.png"),        // 20
        Raylib.LoadTexture("assets/geo/ui/bathive.png"),        // 21
        Raylib.LoadTexture("assets/geo/ui/waterfall.png"),      // 22
        Raylib.LoadTexture("assets/geo/ui/scav.png"),           // 23
        Raylib.LoadTexture("assets/geo/ui/wack.png"),           // 24
        Raylib.LoadTexture("assets/geo/ui/garbageworm.png"),    // 25
        Raylib.LoadTexture("assets/geo/ui/worm.png"),           // 26
        Raylib.LoadTexture("assets/geo/ui/forbidflychains.png"),// 27
        Raylib.LoadTexture("assets/geo/ui/clearall.png"),       // 28
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

    // readonly Dictionary<string, string[]> _effects = new() {
    //     [ "Natural" ] = [ "Slime", "Melt", "Rust", "Barnacles", "Rubble", "DecalsOnlySlime" ],
    //     [ "Erosion" ] = [ "Roughen", "SlimeX3", "Super Melt", "Destructive Melt", "Erode", "Super Erode", "DaddyCorruption" ],
    //     [ "Artificial" ] = [ "Wires", "Chains" ],
    //     [ "Plants" ] = [ "Root Grass", "Seed Pods", "Growers", "Cacti", "Rain Moss", "Hang Roots", "Grass" ],
    //     [ "Plants2" ] = [ "Arm Growers", "Horse Tails", "Circuit Plants", "Feather Plants", "Thorn Growers", "Rollers", "Garbage Spirals" ],
    //     [ "Plants3" ] = [ "Thick Roots", "Shadow Plants" ],
    //     [ "Plants (Individual)" ] = [ "Fungi Flowers", "Lighthouse Flowers", "Fern", "Giant Mushroom", "Sprawlbush", "featherFern", "Fungus Tree" ],
    //     [ "Paint Effects" ] = [ "BlackGoo", "DarkSlime" ],
    //     [ "Restoration" ] = [ "Restore As Scaffolding", "Ceramic Chaos" ],
    //     [ "Drought Plants" ] = [ "Colored Hang Roots", "Colored Thick Roots", "Colored Shadow Plants", "Colored Lighthouse Flowers", "Colored Fungi Flowers", "Root Plants" ],
    //     [ "Drought Plants 2" ] = [ "Foliage", "Mistletoe", "High Fern", "High Grass", "Little Flowers", "Wastewater Mold" ],
    //     [ "Drought Plants 3" ] = [ "Spinets", "Small Springs", "Mini Growers", "Clovers", "Reeds", "Lavenders", "Dense Mold" ],
    //     [ "Drought Erosion" ] = [ "Ultra Super Erode", "Impacts" ],
    //     [ "Drought Paint Effects" ] = [ "Super BlackGoo", "Stained Glass Properties" ],
    //     [ "Drought Natural" ] = [ "Colored Barnacles", "Colored Rubble", "Fat Slime" ],
    //     [ "Drought Artificial" ] = [ "Assorted Trash", "Colored Wires", "Colored Chains", "Ring Chains" ],
    //     [ "Dakras Plants" ] = [ "Left Facing Kelp", "Right Facing Kelp", "Mixed Facing Kelp", "Bubble Grower", "Moss Wall", "Club Moss" ],
    //     [ "Leo Plants" ] = [ "Ivy" ],
    //     [ "Nautillo Plants" ] = [ "Fuzzy Growers" ]
    // };

    readonly string[][] effects = [
        [ "Slime", "Melt", "Rust", "Barnacles", "Rubble", "DecalsOnlySlime" ],
        [ "Roughen", "SlimeX3", "Super Melt", "Destructive Melt", "Erode", "Super Erode", "DaddyCorruption" ],
        [ "Wires", "Chains" ],
        [ "Root Grass", "Seed Pods", "Growers", "Cacti", "Rain Moss", "Hang Roots", "Grass" ],
        [ "Arm Growers", "Horse Tails", "Circuit Plants", "Feather Plants", "Thorn Growers", "Rollers", "Garbage Spirals" ],
        [ "Thick Roots", "Shadow Plants" ],
        [ "Fungi Flowers", "Lighthouse Flowers", "Fern", "Giant Mushroom", "Sprawlbush", "featherFern", "Fungus Tree" ],
        [ "BlackGoo", "DarkSlime" ],
        [ "Restore As Scaffolding", "Ceramic Chaos" ],
        [ "Colored Hang Roots", "Colored Thick Roots", "Colored Shadow Plants", "Colored Lighthouse Flowers", "Colored Fungi Flowers", "Root Plants" ],
        [ "Foliage", "Mistletoe", "High Fern", "High Grass", "Little Flowers", "Wastewater Mold" ],
        [ "Spinets", "Small Springs", "Mini Growers", "Clovers", "Reeds", "Lavenders", "Dense Mold" ],
        [ "Ultra Super Erode", "Impacts" ],
        [ "Super BlackGoo", "Stained Glass Properties" ],
        [ "Colored Barnacles", "Colored Rubble", "Fat Slime" ],
        [ "Assorted Trash", "Colored Wires", "Colored Chains", "Ring Chains" ],
        [ "Left Facing Kelp", "Right Facing Kelp", "Mixed Facing Kelp", "Bubble Grower", "Moss Wall", "Club Moss" ],
        [ "Ivy" ],
        [ "Fuzzy Growers" ]
    ];

    readonly string[] effectNames = [
        "Natural",
        "Erosion",
        "Artificial",
        "Plants",
        "Plants2",
        "Plants3",
        "Plants (Individual)",
        "Paint Effects",
        "Restoration",
        "Drought Plants",
        "Drought Plants 2",
        "Drought Plants 3",
        "Drought Erosion",
        "Drought Paint Effects",
        "Drought Natural",
        "Drought Artificial",
        "Dakras Plants",
        "Leo Plants",
        "Nautillo Plants"
    ];

    const int screenMinWidth = 1080;
    const int screenMinHeight = 800;

    const int geoselectWidth = 200;
    const int geoselectHeight = 600;
    const int scale = 20;
    const int uiScale = 40;
    const int previewScale = 10;
    const float zoomIncrement = 0.125f;
    const int initialMatrixWidth = 72;
    const int initialMatrixHeight = 43;

    static readonly Raylib_cs.Color[] layerColors = [
        new(0, 0, 0, 170),
        new(0, 255, 94, 170),
        new(225, 0, 0, 180),
    ];

    static readonly Raylib_cs.Color whiteStackable = new(255, 255, 255, 200);
    static readonly Raylib_cs.Color blackStackable = new(0, 0, 0, 200);

    public static int GetBlockIndex(int id) => id switch {
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
    public static int GetBlockID(uint index) => index switch {
        0 => 1,
        1 => 0,
        7 => 4,
        6 => 5,
        3 => 2,
        2 => 3,
        8 => 6,
        15 => 9,
        _ => -1
    };

    public static readonly int[] stackableIds = new int[29];

    /// <summary>
    /// Maps a ui texture index to a stackable ID
    /// </summary>
    public static int GetStackableID(uint index) => index switch {
        13 => 1,
        14 => 2,
        21 => 3,
        17 => 4,
        18 => 5,
        20 => 6,
        19 => 7,
        10 => 9,
        11 => 10,
        12 => 11,
        27 => 12,
        25 => 13,
        22 => 18,
        24 => 19,
        26 => 20,
        23 => 21,

        _ => -1
    };

    public static int GetStackableTextureIndex(int id) => id switch {
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


        if (i == -4) {
            if (
                context[0][0].Stackables[4] || context[0][1].Stackables[4] || context[0][2].Stackables[4] ||
                context[1][0].Stackables[4] ||                                context[1][2].Stackables[4] ||
                context[2][0].Stackables[4] || context[2][1].Stackables[4] || context[2][2].Stackables[4]
            ) return 26;

            var pattern = (
                false, context[0][1].Stackables[5] ^ context[0][1].Stackables[6] ^ context[0][1].Stackables[7] ^ context[0][1].Stackables[19], false,
                context[1][0].Stackables[5] ^ context[1][0].Stackables[6] ^ context[1][0].Stackables[7] ^ context[1][0].Stackables[19], false, context[1][2].Stackables[5] ^ context[1][2].Stackables[6] ^ context[1][2].Stackables[7] ^ context[1][2].Stackables[19],
                false, context[2][1].Stackables[5] ^ context[2][1].Stackables[6] ^ context[2][1].Stackables[7] ^ context[2][1].Stackables[19], false
            );

            var directionIndex = pattern switch {

                (
                    _    , true , _    ,
                    false, _    , false,
                    _    , false, _
                ) => 25,
                
                (
                    _    , false, _    ,
                    false, _    , true,
                    _    , false, _
                ) => 24,
                
                (
                    _    , false, _    ,
                    false, _    , false,
                    _    , true , _
                ) => 22,
                
                (
                    _    , false, _    ,
                    true , _    , false,
                    _    , false, _
                ) => 23,

                _ => 26
            };

            if (directionIndex == 26) return 26;

            var geoPattern = (
                context[0][0].Geo, context[0][1].Geo, context[0][2].Geo,
                context[1][0].Geo, 0                , context[1][2].Geo,
                context[2][0].Geo, context[2][1].Geo, context[2][2].Geo
            );

            directionIndex = geoPattern switch {

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
        } else if (i == -11) {
            i = (
                false,                        context[0][1].Stackables[11], false,
                context[1][0].Stackables[11], false,                        context[1][2].Stackables[11],
                false,                        context[2][1].Stackables[11], false
            ) switch {

                (
                    _    , true , _    ,
                    false, _    , false,
                    _    , false, _
                ) => 33,
                
                (
                    _    , false , _    ,
                    false, _     , true ,
                    _    , false , _
                ) => 32,
                
                (
                    _    , false , _    ,
                    false, _     , false,
                    _    , true  , _
                ) => 31,
                
                (
                    _    , false , _    ,
                    true , _     , false,
                    _    , false  , _
                ) => 34,

                //
                
                (
                    _    , true  , _    ,
                    false, _     , true ,
                    _    , false , _
                ) => 13,
                
                (
                    _    , false , _    ,
                    false, _     , true ,
                    _    , true  , _
                ) => 5,
                
                (
                    _    , false , _    ,
                    true , _     , false,
                    _    , true  , _
                ) => 4,
                
                (
                    _    , true  , _    ,
                    true , _     , false,
                    _    , false , _
                ) => 10,

                //

                (
                    _    , true  , _    ,
                    true , _     , true ,
                    _    , false , _
                ) => 12,

                (
                    _    , true  , _    ,
                    false, _     , true ,
                    _    , true  , _
                ) => 9,

                (
                    _    , false , _    ,
                    true , _     , true ,
                    _    , true  , _
                ) => 8,

                (
                    _    , true  , _    ,
                    true , _     , false,
                    _    , true  , _
                ) => 11,

                //

                (
                    _    , false , _   ,
                    true , _     , true,
                    _    , false , _
                ) => 7,

                (
                    _    , true , _    ,
                    false, _    , false,
                    _    , true , _
                ) => 15,

                //

                (
                    _    , true , _   ,
                    true , _    , true,
                    _    , true , _
                ) => 6,

                (
                    _    , false , _    ,
                    false , _     , false,
                    _    , false  , _
                ) => 14,
            };
        }

        return i;
    }

    static unsafe void Main(string[] args) {
        int page = 0;

        BufferTiles bufferTiles = new(12, 12, 3, 3);

        int tileSeed = 141;
        bool lightMode = true;
        bool defaultTerrain = true;

        int matrixWidth = initialMatrixWidth;
        int matrixHeight = initialMatrixHeight;
        
        uint geoSelectionX = 0;
        uint geoSelectionY = 0;

        int currentLayer = 0;

        int prevCoordsX = -1;
        int prevCoordsY = -1;
        bool multiselect = false;

        int prevMatrixX = -1;
        int prevMatrixY = -1;
        bool clickTracker = false;

        int waterLevel = -1;
        bool waterInFront = true;

        bool gridContrast = false;

        RunCell[,,] geoMatrix = Utils.NewGeoMatrix(matrixWidth, matrixHeight, 1);
        bool[,] lightMatrix = Utils.NewLightMatrix(matrixWidth, matrixHeight, scale);

        bool initial = true;

        int flatness = 0;
        double lightAngleVariable = 0;
        double lightAngle = 90;

        const float initialGrowthFactor = 0.01f;
        float growthFactor = initialGrowthFactor;
        var lightRecSize = new Vector2(100, 100);
        bool slowGrowth = true;
        bool shading = true;

        // UNSAFE VARIABLES

        int matrixWidthValue = 72;
        int matrixHeightValue = 43;
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
        //

        Camera2D camera = new() { Zoom = 1.0f, Target = new(-geoselectWidth, 0) };
        Camera2D mainPageCamera = new() { Zoom = 1.0f };
        Camera2D lightPageCamera = new() { Zoom = 0.5f };

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



        Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose()) {

            switch (page) {

            case 0:
                Raylib.BeginDrawing();
                {
                    Raylib.ClearBackground(Raylib_cs.Color.GRAY);

                    if (Raylib_CsLo.RayGui.GuiButton(new (Raylib.GetScreenWidth() / 2 - 150, Raylib.GetScreenHeight() / 2 - 40, 300, 40), "Create")) {
                        page = 6;
                    }
                    Raylib_CsLo.RayGui.GuiButton(new (Raylib.GetScreenWidth() / 2 - 150, Raylib.GetScreenHeight() / 2, 300, 40), "Load");
                }
                Raylib.EndDrawing();
                break;

            case 1:
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_ONE)) page = 1;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_TWO)) page = 2;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_THREE)) page = 3;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_FOUR)) page = 4;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_FIVE)) page = 5;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_SIX)) page = 6;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_SEVEN)) page = 7;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_EIGHT)) page = 8;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_NINE)) page = 9;

                if (
                matrixHeight != matrixHeightValue || 
                matrixWidth != matrixWidthValue
                ) {
                    geoMatrix = Utils.Resize(
                        geoMatrix, 
                        matrixWidth, 
                        matrixHeight, 
                        matrixWidthValue, 
                        matrixHeightValue
                        );

                    lightMatrix = Utils.Resize(
                        lightMatrix, 
                        matrixWidth, 
                        matrixHeight,
                        matrixWidthValue,
                        matrixHeightValue, 
                        scale
                        );

                    matrixHeight = matrixHeightValue;
                    matrixWidth = matrixWidthValue;

                    var img = Raylib.LoadImageFromTexture(lightMapBuffer.Texture);
                    Raylib.ImageFlipVertical(&img);
                    Raylib.ImageResizeCanvas(ref img, matrixWidth * scale + 300, matrixHeight * scale + 300, 0, 0, Color.WHITE);
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
                    ) {
                        bufferTiles = new BufferTiles {
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

                // handle mouse drag
                if(Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT)) {
                    Vector2 delta = Raylib.GetMouseDelta();
                    delta = Raymath.Vector2Scale(delta, -1.0f/mainPageCamera.Zoom);
                    mainPageCamera.Target = Raymath.Vector2Add(mainPageCamera.Target, delta);
                }

                Raylib.BeginDrawing();
                {
                    Raylib.ClearBackground(Raylib_cs.Color.GRAY);
                    
                    Raylib.BeginMode2D(mainPageCamera);
                    {
                        for (int y = 0; y < matrixHeight; y++) {
                            for (int x = 0; x < matrixWidth; x++) {
                                Raylib.DrawRectangle(
                                    x * previewScale,
                                    y * previewScale,
                                    previewScale,
                                    previewScale,
                                    new(244, 244, 244, 255)
                                );

                                if (geoMatrix[y,x,2].Geo == 1) Raylib.DrawRectangle(
                                    x * previewScale, 
                                    y * previewScale, 
                                    previewScale, 
                                    previewScale,
                                    new(0, 0, 0, 160));

                                if (geoMatrix[y,x,1].Geo == 1) Raylib.DrawRectangle(
                                    x * previewScale,
                                    y * previewScale,
                                    previewScale,
                                    previewScale,
                                    new(0,0,0, 160)
                                );
                            }
                        }

                        if (!waterInFront && waterLevel != -1) {
                            Raylib.DrawRectangle(
                                (-1) * previewScale,
                                (matrixHeight - waterLevel) * previewScale,
                                (matrixWidth + 2) * previewScale,
                                waterLevel * previewScale,
                                Raylib_cs.Color.DARKBLUE
                            );
                        }

                        for (int y = 0; y < matrixHeight; y++) {
                            for (int x = 0; x < matrixWidth; x++) {
                                if (geoMatrix[y,x,0].Geo == 1) Raylib.DrawRectangle(
                                    x * previewScale,
                                    y * previewScale,
                                    previewScale,
                                    previewScale,
                                    new(0,0,0, 225)
                                );
                            }
                        }

                        if (waterInFront && waterLevel != -1) {
                            Raylib.DrawRectangle(
                                (-1) * previewScale,
                                (matrixHeight - waterLevel) * previewScale,
                                (matrixWidth + 2) * previewScale,
                                waterLevel * previewScale,
                                new(0, 0, 255, 110)
                            );
                        }
                    }
                    Raylib.EndMode2D();

                    fixed (byte* pt = previewPanelBytes) {
                        Raylib_CsLo.RayGui.GuiPanel(
                            new(
                                Raylib.GetScreenWidth() - 400,
                                50,
                                380,
                                Raylib.GetScreenHeight() - 100
                            ), 
                            (sbyte*) pt
                        );

                        Raylib.DrawText("Seed", Raylib.GetScreenWidth() - 380, 105, 11, Raylib_cs.Color.BLACK);

                        tileSeed = (int)Math.Round(Raylib_CsLo.RayGui.GuiSlider(
                            new(
                                Raylib.GetScreenWidth() - 290,
                                100,
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
                            150,
                            20,
                            20
                        ),
                        "Light Mode",
                        lightMode);

                        defaultTerrain = Raylib_CsLo.RayGui.GuiCheckBox(new(
                            Raylib.GetScreenWidth() - 380,
                            190,
                            20,
                            20
                        ),
                        "Default Medium",
                        defaultTerrain);

                        Raylib.DrawText("Water Level",
                            Raylib.GetScreenWidth() - 380,
                            235, 
                            11,
                            Raylib_cs.Color.BLACK
                        );
                        waterLevel = (int)Math.Round(Raylib_CsLo.RayGui.GuiSlider(
                            new(
                                Raylib.GetScreenWidth() - 290,
                                230,
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
                                260,
                                20,
                                20
                            ),
                            "Water In Front",
                            waterInFront
                        );
                    }
                    
                }
                Raylib.EndDrawing();

                break;

            case 2:
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_ONE)) page = 1;
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_TWO)) page = 2;
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_THREE)) page = 3;
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_FOUR)) page = 4;
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_FIVE)) page = 5;
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_SIX)) page = 6;
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_SEVEN)) page = 7;
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_EIGHT)) page = 8;
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_NINE)) page = 9;

            Vector2 mouse = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
            
            //                        v this was done to avoid rounding errors
            int matrixY = mouse.Y < 0 ? -1 : (int)mouse.Y / scale;
            int matrixX = mouse.X < 0 ? -1 : (int)mouse.X / scale;
            

            // handle geo selection

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_D)) {
                geoSelectionX = ++geoSelectionX % 4;

                multiselect = false;
                prevCoordsX = -1;
                prevCoordsY = -1;
            } else if (Raylib.IsKeyPressed(KeyboardKey.KEY_A)) {
                geoSelectionX = --geoSelectionX % 4;

                multiselect = false;
                prevCoordsX = -1;
                prevCoordsY = -1;
            } else if (Raylib.IsKeyPressed(KeyboardKey.KEY_W)) {
                geoSelectionY = (--geoSelectionY) % 8;

                multiselect = false;
                prevCoordsX = -1;
                prevCoordsY = -1;
            } else if (Raylib.IsKeyPressed(KeyboardKey.KEY_S)) {
                geoSelectionY = (++geoSelectionY) % 8;

                multiselect = false;
                prevCoordsX = -1;
                prevCoordsY = -1;
            }

            // handle changing layers

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_L)) {
                currentLayer = ++currentLayer % 3;
            }

            uint geoIndex = (4 * geoSelectionY) + geoSelectionX;

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_M)) {
                gridContrast = !gridContrast;
            }

            // handle mouse drag
            if(Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT)) {
                Vector2 delta = Raylib.GetMouseDelta();
                delta = Raymath.Vector2Scale(delta, -1.0f/camera.Zoom);
                camera.Target = Raymath.Vector2Add(camera.Target, delta);
            }


            // handle zoom
            var wheel = Raylib.GetMouseWheelMove();
            if (wheel != 0) {
                Vector2 mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
                camera.Offset = Raylib.GetMousePosition();
                camera.Target = mouseWorldPosition;
                camera.Zoom += wheel * zoomIncrement;
                if (camera.Zoom < zoomIncrement) camera.Zoom = zoomIncrement;
            }

            // handle placing geo

            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) {
                if (matrixY >= 0 && matrixY < matrixHeight && matrixX >= 0 && matrixX < matrixWidth) {
                    switch (geoIndex) {
                        // blocks
                        case 0: // solid
                        case 1: // air
                        case 2: // slopebr
                        case 3: // slopebl
                        case 6: // slopetr
                        case 7: // slopetl
                        case 8: // platform
                        case 15: // glass
                            var cell = geoMatrix[matrixY, matrixX, currentLayer];

                            cell.Geo = GetBlockID(geoIndex);
                            geoMatrix[matrixY, matrixX, currentLayer] = cell;
                            break;

                        // multi-select: forward to next if-statement
                        case 4:
                        case 5:
                        case 16:
                        case 28:
                            break;

                        // stackables
                        case 10: // rock
                        case 11: // spear
                        case 12: // crack
                        case 13: // ph
                        case 14: // pv
                        case 17: // entry
                        case 18: // shortcut
                        case 19: // den
                        case 20: // passage
                        case 21: // bathive
                        case 22: // waterfall
                        case 23: // scav
                        case 24: // wac
                        case 25: // garbageworm
                        case 26: // worm
                        case 27: // forbidflychains
                            if (geoIndex is 17 or 18 or 19 or 20 or 22 or 23 or 24 or 25 or 26 or 27 && currentLayer != 0) {
                                break;
                            }

                            var id = GetStackableID(geoIndex);
                            var cell_ = geoMatrix[matrixY, matrixX, currentLayer];
                            
                            var newValue = !cell_.Stackables[id];
                            if (matrixX != prevMatrixX || matrixY != prevMatrixY || !clickTracker) {
                                
                                if (cell_.Stackables[id] != newValue) {
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

            if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)) {
                clickTracker = false;
            }

            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON)) {
                if (matrixY >= 0 && matrixY < matrixHeight && matrixX >= 0 && matrixX < matrixWidth) {
                    switch (geoIndex) {
                        case 28:
                            multiselect = !multiselect;
                            
                            if (multiselect) {
                                prevCoordsX = matrixX;
                                prevCoordsY = matrixY;
                            } else {
                                int startX, 
                                    startY,
                                    endX,
                                    endY;

                                if (matrixX > prevCoordsX) {
                                    startX = prevCoordsX;
                                    endX = matrixX;
                                } else {
                                    startX = matrixX;
                                    endX = prevCoordsX;
                                }

                                if (matrixY > prevCoordsY) {
                                    startY = prevCoordsY;
                                    endY = matrixY;
                                } else {
                                    startY = matrixY;
                                    endY = prevCoordsY;
                                }

                                for (int y = startY; y < endY; y++) {
                                    for (int x = startX; x < endX; x++) {
                                        geoMatrix[y, x, 0].Geo = 0;
                                        geoMatrix[y, x, 1].Geo = 0;
                                        geoMatrix[y, x, 2].Geo = 0;
                                    }
                                }
                            }
                            break;
                        case 16:
                            multiselect = !multiselect;
                            
                            if (multiselect) {
                                prevCoordsX = matrixX;
                                prevCoordsY = matrixY;
                            } else {
                                int startX, 
                                    startY,
                                    endX,
                                    endY;

                                if (matrixX > prevCoordsX) {
                                    startX = prevCoordsX;
                                    endX = matrixX;
                                } else {
                                    startX = matrixX;
                                    endX = prevCoordsX;
                                }

                                if (matrixY > prevCoordsY) {
                                    startY = prevCoordsY;
                                    endY = matrixY;
                                } else {
                                    startY = matrixY;
                                    endY = prevCoordsY;
                                }

                                if (currentLayer is 0 or 1) {
                                    for (int y = startY; y < endY; y++) {
                                        for (int x = startX; x < endX; x++) {
                                            geoMatrix[y, x, currentLayer + 1].Geo = geoMatrix[y, x, currentLayer].Geo;
                                        }
                                    }
                                }
                            }
                            break;
                        case 4:
                        case 5:
                            multiselect = !multiselect;

                            if (multiselect) {
                                prevCoordsX = matrixX;
                                prevCoordsY = matrixY;
                            } else {
                                int startX, 
                                    startY,
                                    endX,
                                    endY;

                                if (matrixX > prevCoordsX) {
                                    startX = prevCoordsX;
                                    endX = matrixX;
                                } else {
                                    startX = matrixX;
                                    endX = prevCoordsX;
                                }

                                if (matrixY > prevCoordsY) {
                                    startY = prevCoordsY;
                                    endY = matrixY;
                                } else {
                                    startY = matrixY;
                                    endY = prevCoordsY;
                                }

                                int value = geoIndex == 4 ? 1 : 0;

                                for (int y = startY; y < endY; y++) {
                                    for (int x = startX; x < endX; x++) {
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
                Raylib.ClearBackground(Raylib_cs.Color.GRAY);


                Raylib.BeginMode2D(camera);
                {
                    if (!gridContrast) {
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
                    for (int y = 0; y < matrixHeight; y++) {
                        for (int x = 0; x < matrixWidth; x++) {
                            for (int z = 2; z > -1; z--) {
                                var cell = geoMatrix[y, x, z];

                                var texture = GetBlockIndex(cell.Geo);

                                if (texture >= 0) {
                                    Raylib.DrawTexture(geoTextures[texture], x * scale, y * scale, layerColors[z]);
                                }

                                for (int s = 1; s < cell.Stackables.Length; s++) {
                                    if (cell.Stackables[s]) {
                                        switch (s) {
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
                                                // RunCell[][] context1 = [
                                                //     [ 
                                                //         x > 0 && y > 0 ? geoMatrix[y - 1, x - 1, z] : new(), 
                                                //         y > 0 ? geoMatrix[y - 1, x, z] : new(), 
                                                //         x < matrixWidth -1 && y > 0 ? geoMatrix[y - 1, x + 1, z] : new()
                                                //     ],
                                                //     [
                                                //         x > 0 ? geoMatrix[y, x - 1, z] : new(),
                                                //         new(),
                                                //         x < matrixWidth -1 ? geoMatrix[y, x + 1, z] : new(),
                                                //     ],
                                                //     [
                                                //         x > 0 && y < matrixHeight -1 ? geoMatrix[y + 1, x - 1, z] : new(),
                                                //         y < matrixHeight -1 ? geoMatrix[y + 1,x, z] : new(),
                                                //         x < matrixWidth -1 && y < matrixHeight -1 ? geoMatrix[y + 1, x + 1, z] : new()
                                                //     ]
                                                // ];
                                                var index = GetStackableTextureIndex(s, Utils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, z));

                                                if (index is 22 or 23 or 24 or 25) {
                                                    geoMatrix[y, x, 0].Geo = 7;
                                                }

                                                Raylib.DrawTexture(stackableTextures[index], x * scale, y * scale, whiteStackable);
                                                break;
                                            case 11:    // crack
                                                // RunCell[][] context2 = [
                                                //     [ 
                                                //         x > 0 && y > 0 ? geoMatrix[y - 1, x - 1, z] : new(), 
                                                //         y > 0 ? geoMatrix[y - 1, x, z] : new(), 
                                                //         x < matrixWidth -1 && y > 0 ? geoMatrix[y - 1, x + 1, z] : new()
                                                //     ],
                                                //     [
                                                //         x > 0 ? geoMatrix[y, x - 1, z] : new(),
                                                //         new(),
                                                //         x < matrixWidth -1 ? geoMatrix[y, x + 1, z] : new(),
                                                //     ],
                                                //     [
                                                //         x > 0 && y < matrixHeight -1 ? geoMatrix[y + 1, x - 1, z] : new(),
                                                //         y < matrixHeight -1 ? geoMatrix[y + 1, x, z] : new(),
                                                //         x < matrixWidth -1 && y < matrixHeight -1 ? geoMatrix[y + 1, x + 1, z] : new()
                                                //     ]
                                                // ];

                                                Raylib.DrawTexture(
                                                    stackableTextures[GetStackableTextureIndex(s, Utils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, z))], 
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

                    if (gridContrast) {
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

                    for (int y = 0; y < matrixHeight; y++) {
                        for (int x = 0; x < matrixWidth; x++) {
                            if (multiselect) {
                                var XS = matrixX - prevCoordsX;
                                var YS = matrixY - prevCoordsY;
                                var width = Math.Abs(XS) * scale;
                                var height = Math.Abs(YS) * scale;

                                Raylib_cs.Rectangle rec = (XS > 0, YS > 0) switch {
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
                                    $"{width/scale:0}x{height/scale:0}", 
                                    (int)mouse.X + 10, 
                                    (int)mouse.Y, 
                                    4, 
                                    Raylib_cs.Color.WHITE
                                    );
                            } else {
                                if (matrixX == x && matrixY == y) {
                                    Raylib.DrawRectangleLinesEx(new(x * scale, y * scale, scale, scale), 2, Color.RED);
                                }
                            }
                        }
                    }

                    // the outbound border
                    Raylib.DrawRectangleLinesEx(new(0, 0, matrixWidth * scale, matrixHeight * scale), 2, Color.BLACK);

                    // the border
                    Raylib.DrawRectangleLinesEx(border, camera.Zoom < zoomIncrement ? 5 : 2, Color.WHITE);

                    // a lazy way to hide the rest of the grid
                    Raylib.DrawRectangle(matrixWidth * -scale, -3, matrixWidth * scale, matrixHeight * 2 * scale, Color.GRAY);
                    Raylib.DrawRectangle(0, matrixHeight * scale, matrixWidth * scale + 2, matrixHeight * scale, Color.GRAY);
                }
                Raylib.EndMode2D();

                // geo menu

                Raylib.DrawRectangle(0, 0, geoselectWidth, geoselectHeight, Color.GRAY);
                Raylib.DrawRectangleLinesEx(new(1, 1, geoselectWidth - 1, geoselectHeight - 1), 1, Color.BLACK);

                for (int w = 0; w < 4; w++) {
                    for (int h = 0; h < 8; h++) {
                        var index = (4 * h) + w;
                        if (index < uiTextures.Length) {
                            Raylib.DrawTexture(uiTextures[index],  w * uiScale + 5, h * uiScale + 5, Color.BLACK);   
                        }

                    if (w == geoSelectionX && h == geoSelectionY) 
                        Raylib.DrawRectangleLinesEx(new(w * uiScale + 5, h * uiScale + 5, uiScale, uiScale), 2, Color.RED); 
                    else
                        Raylib.DrawRectangleLinesEx(new(w * uiScale + 5, h * uiScale + 5, uiScale, uiScale), 1, Color.BLACK);
                    }
                }

                if (geoIndex < uiTextures.Length) Raylib.DrawText(geoNames[geoIndex], 5, 8 * uiScale + 10, 20, Color.BLACK);

                switch (currentLayer) {
                    case 0:
                        Raylib.DrawRectangle(5, 8 * uiScale + 40, 40, 40, Color.BLACK);
                        Raylib.DrawText("L1", 13, 8 * uiScale + 48, 26, Color.WHITE);
                        break;
                    case 1:
                        Raylib.DrawRectangle(5, 8 * uiScale + 40, 40, 40, Color.DARKGREEN);
                        Raylib.DrawText("L2", 13, 8 * uiScale + 48, 26, Color.WHITE);
                        break;
                    case 2:
                        Raylib.DrawRectangle(5, 8 * uiScale + 40, 40, 40, Color.RED);
                        Raylib.DrawText("L3", 13, 8 * uiScale + 48, 26, Color.WHITE);
                        break;
                }
                Raylib.DrawText($"X = {matrixX:0}\nY = {matrixY:0}", 5, 10 * uiScale + 1, 2, Color.WHITE);
            }
            Raylib.EndDrawing();

            break;

            case 4:
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_ONE)) page = 1;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_TWO)) page = 2;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_THREE)) page = 3;
                //if (Raylib.IsKeyReleased(KeyboardKey.KEY_FOUR)) page = 4;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_FIVE)) page = 5;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_SIX)) page = 6;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_SEVEN)) page = 7;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_EIGHT)) page = 8;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_NINE)) page = 9;

                Raylib.BeginDrawing();
                {
                    Raylib.ClearBackground(Color.GRAY);
                    Raylib.DrawText("Camera editor", 100, 100, 30, Color.BLACK);
                }
                Raylib.EndDrawing();
                break;

            case 5:
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_ONE)) page = 1;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_TWO)) page = 2;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_THREE)) page = 3;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_FOUR)) page = 4;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_FIVE)) page = 5;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_SIX)) page = 6;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_SEVEN)) page = 7;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_EIGHT)) page = 8;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_NINE)) page = 9;

                if (Raylib.IsKeyDown(KeyboardKey.KEY_I) && flatness < 10) flatness++;
                if (Raylib.IsKeyDown(KeyboardKey.KEY_K) && flatness > 0) flatness--;

                if (Raylib.IsKeyDown(KeyboardKey.KEY_L)) {
                    lightAngleVariable += 0.001f;
                    lightAngle = 180 * Math.Sin(lightAngleVariable) + 90;
                }
                if (Raylib.IsKeyDown(KeyboardKey.KEY_J)) {
                    lightAngleVariable -= 0.001f;
                    lightAngle = 180 * Math.Sin(lightAngleVariable) + 90;
                }

                // handle mouse drag
                if(Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT)) {
                    Vector2 delta = Raylib.GetMouseDelta();
                    delta = Raymath.Vector2Scale(delta, -1.0f/lightPageCamera.Zoom);
                    lightPageCamera.Target = Raymath.Vector2Add(lightPageCamera.Target, delta);
                }


                // handle zoom
                var wheel2 = Raylib.GetMouseWheelMove();
                if (wheel2 != 0) {
                    Vector2 mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), lightPageCamera);
                    lightPageCamera.Offset = Raylib.GetMousePosition();
                    lightPageCamera.Target = mouseWorldPosition;
                    lightPageCamera.Zoom += wheel2 * zoomIncrement;
                    if (lightPageCamera.Zoom < zoomIncrement) lightPageCamera.Zoom = zoomIncrement;
                }

                var lightMousePos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), lightPageCamera);

                if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) {
                    Raylib.BeginTextureMode(lightMapBuffer);
                    {
                        // paint shadows here
                    }
                    Raylib.EndTextureMode();
                }

                if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE)) slowGrowth = !slowGrowth;
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_R)) shading = !shading;

                if (slowGrowth) {
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_W)) {
                        lightRecSize = Raymath.Vector2Add(lightRecSize, new(0, growthFactor));
                        growthFactor += 0.03f;
                    }

                    if (Raylib.IsKeyDown(KeyboardKey.KEY_S)) {
                        lightRecSize = Raymath.Vector2Add(lightRecSize, new(0, -growthFactor));
                        growthFactor += 0.03f;
                    }

                    if (Raylib.IsKeyDown(KeyboardKey.KEY_D)) {
                        lightRecSize = Raymath.Vector2Add(lightRecSize, new(growthFactor, 0));
                        growthFactor += 0.03f;
                    }

                    if (Raylib.IsKeyDown(KeyboardKey.KEY_A)) {
                        lightRecSize = Raymath.Vector2Add(lightRecSize, new(-growthFactor, 0));
                        growthFactor += 0.03f;
                    }

                    // clockwise rotation
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_E)) {
                        
                    }

                    // counter-clockwise rotation
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_Q)) {
                        
                    }

                    if (Raylib.IsKeyReleased(KeyboardKey.KEY_W) || 
                        Raylib.IsKeyReleased(KeyboardKey.KEY_S) ||
                        Raylib.IsKeyReleased(KeyboardKey.KEY_D) ||
                        Raylib.IsKeyReleased(KeyboardKey.KEY_A)) growthFactor = initialGrowthFactor;
                } else {
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_W)) lightRecSize = Raymath.Vector2Add(lightRecSize, new( 0,  3));
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_S)) lightRecSize = Raymath.Vector2Add(lightRecSize, new( 0, -3));
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_D)) lightRecSize = Raymath.Vector2Add(lightRecSize, new( 3,  0));
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_A)) lightRecSize = Raymath.Vector2Add(lightRecSize, new(-3,  0));
                }


                Raylib.BeginDrawing();
                {
                    Raylib.ClearBackground(Raylib_cs.Color.GRAY);

                    Raylib.BeginMode2D(lightPageCamera);
                    {

                        for (int y = 0; y < matrixHeight; y++) {
                            for (int x = 0; x < matrixWidth; x++) {
                                var cell = geoMatrix[y, x, currentLayer];
                                var lightGeoIndex = GetBlockIndex(cell.Geo);
                                if (lightGeoIndex >= 0 && lightGeoIndex < geoTextures.Length)
                                    Raylib.DrawTexture(
                                        geoTextures[lightGeoIndex], 
                                        300 + x * scale, 
                                        300 + y * scale, 
                                        Color.BLACK
                                    );

                                if (cell.Stackables[4]) {
                                    var lightStackableIndex = GetStackableTextureIndex(4, Utils.GetContext(geoMatrix, matrixWidth, matrixHeight, x, y, 0));
                                    if (lightStackableIndex >= 0 && lightStackableIndex < stackableTextures.Length) {

                                        Raylib.DrawTexture(
                                            stackableTextures[lightStackableIndex], 
                                            300 + x * scale, 
                                            300 + y * scale, 
                                            Color.WHITE
                                        );
                                    }
                                }
                            }
                        }

                        Raylib.DrawTextureRec(
                            lightMapBuffer.Texture, 
                            new Rectangle(0, 0, lightMapBuffer.Texture.Width, lightMapBuffer.Texture.Height), 
                            new(0, 0), 
                            Color.WHITE
                        );

                        Raylib.DrawRectangleV(
                            Raymath.Vector2Add(lightMousePos, Raymath.Vector2Divide(lightRecSize, new(-2, -2))), 
                            lightRecSize, 
                            new(255, 0, 0, 190)
                        );
                    }
                    Raylib.EndMode2D();

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

            case 6:
                Raylib.BeginDrawing();
                {
                    Raylib.ClearBackground(Raylib_cs.Color.GRAY);

                    fixed (byte* pt = panelBytes) {
                        Raylib_CsLo.RayGui.GuiPanel(new(30, 60, Raylib.GetScreenWidth() - 60, Raylib.GetScreenHeight() - 120), (sbyte*)pt);
                    }

                    Raylib.DrawText("Width", 50, 110, 20, Raylib_cs.Color.BLACK);
                    if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 100, 300, 40), "", &matrixWidthValue, 72, 999, editControl == 0)) {
                        editControl = 0;
                    }

                    Raylib.DrawText("Height", 50, 160, 20, Raylib_cs.Color.BLACK);
                    if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 150, 300, 40), "", &matrixHeightValue, 43, 999, editControl == 1)) {
                        editControl = 1;
                    }

                    Raylib_CsLo.RayGui.GuiLine(new(50, 200, Raylib.GetScreenWidth() - 100, 40), "Padding");

                    Raylib.DrawText("Left", 50, 260, 20, Raylib_cs.Color.BLACK);
                    if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 250, 300, 40), "", &leftPadding, 0, 333, editControl == 2)) {
                        editControl = 2;
                    }

                    Raylib.DrawText("Right", 50, 310, 20, Raylib_cs.Color.BLACK);
                    if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 300, 300, 40), "", &rightPadding, 0, 333, editControl == 3)) {
                        editControl = 3;
                    }
                    
                    Raylib.DrawText("Top", 50, 360, 20, Raylib_cs.Color.BLACK);
                    if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 350, 300, 40), "", &topPadding, 0, 111, editControl == 4)) {
                        editControl = 4;
                    }

                    Raylib.DrawText("Bottom", 50, 410, 20, Raylib_cs.Color.BLACK);
                    if (Raylib_CsLo.RayGui.GuiSpinner(new(130, 400, 300, 40), "", &bottomPadding, 0, 111, editControl == 5)) {
                        editControl = 5;
                    }

                    if (Raylib_CsLo.RayGui.GuiButton(new(360, Raylib.GetScreenHeight() - 160, 300, 40), "Ok")) {
                        Raylib.EndDrawing();

                        initial = false;
                        page = 1;
                    }

                    if (Raylib_CsLo.RayGui.GuiButton(new(50, Raylib.GetScreenHeight() - 160, 300, 40), "Cancel")) {
                        Raylib.EndDrawing();

                        leftPadding = bufferTiles.Left;
                        rightPadding = bufferTiles.Right;
                        topPadding = bufferTiles.Top;
                        bottomPadding = bufferTiles.Bottom;

                        matrixWidth = matrixWidthValue;
                        matrixHeight = matrixHeightValue;

                        page = initial ? 0 : 1;
                        
                    }
                }
                Raylib.EndDrawing();
                break;
            
            case 9:
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_ONE)) page = 1;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_TWO)) page = 2;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_THREE)) page = 3;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_FOUR)) page = 4;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_FIVE)) page = 5;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_SIX)) page = 6;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_SEVEN)) page = 7;
                if (Raylib.IsKeyReleased(KeyboardKey.KEY_EIGHT)) page = 8;
                //if (Raylib.IsKeyReleased(KeyboardKey.KEY_NINE)) page = 9;

                Raylib.BeginDrawing();
                {
                    Raylib.ClearBackground(Color.GRAY);

                    fixed (byte* pt = helpPanelBytes) {
                        Raylib_CsLo.RayGui.GuiPanel(
                            new(100, 100, Raylib.GetScreenWidth() - 200, Raylib.GetScreenHeight() - 200), 
                            (sbyte*) pt
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
                        Color.GRAY
                    );

                    switch (helpSubSection) {
                        case 0:
                            Raylib.DrawText(
                                " [1] - Main screen\n[2] - Geometry editor\n[3] - Tiles editor\n[4] - Cameras editor\n" + 
                                "[5] - Light editor\n[6] - Edit dimensions\n[7] - Effects editor\n[8] - Props editor", 
                                400, 
                                160, 
                                20, 
                                Color.BLACK
                            );
                            break;

                        case 1:
                            Raylib.DrawText(
                                "[W] [A] [S] [D] - Navigate the geometry tiles menu\n" +
                                "[L] - Change current layer\n"+ 
                                "[M] - Toggle grid (contrast)", 
                                400, 
                                160, 
                                20, 
                                Color.BLACK
                            );
                            break;

                        case 2:
                            break;

                        case 3:
                            break;

                        case 4:
                            break;

                        case 5:
                            break;

                        case 6:
                            break;
                    }
                }
                Raylib.EndDrawing();
                break;         
            default:
                page = 1;
                break;
            }
        }

        foreach (var texture in uiTextures) Raylib.UnloadTexture(texture);
        foreach (var texture in geoTextures) Raylib.UnloadTexture(texture);
        foreach (var texture in stackableTextures) Raylib.UnloadTexture(texture);

        Raylib.CloseWindow();
        return;
    }
}
