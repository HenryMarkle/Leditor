global using Raylib_cs;
using System.Numerics;
using System.Security.Cryptography;

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
        "Forbid Fly Chains"
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
    ];

    static Texture2D[] LoadGeoTextures() => [
        // 0: air
        Raylib.LoadTexture("assets/geo/solid.png"),
        Raylib.LoadTexture("assets/geo/ctl.png"),
        Raylib.LoadTexture("assets/geo/ctr.png"),
        Raylib.LoadTexture("assets/geo/cbr.png"),
        Raylib.LoadTexture("assets/geo/cbl.png"),
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

    const int geoselectWidth = 200;
    const int geoselectHeight = 600;
    const int scale = 20;
    const int uiScale = 40;
    const float zoomIncrement = 0.125f;

    static readonly Color[] layerColors = [
        new(0, 0, 0, 120),
        new(0, 255, 94, 150),
        new(225, 0, 0, 150),
    ];

    static readonly Color whiteStackable = new(255, 255, 255, 200);
    static readonly Color blackStackable = new(0, 0, 0, 200);

    /// <summary>
    /// Maps a ui texure index to block ID
    /// </summary>
    public static int GetBlockID(uint index) => index switch {
        0 => 1,
        1 => 0,
        2 => 4,
        3 => 5,
        6 => 2,
        7 => 3,
        8 => 6,
        16 => 9,
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
                context[0][0].stackables[4] || context[0][1].stackables[4] || context[0][2].stackables[4] ||
                context[1][0].stackables[4] ||                                context[1][2].stackables[4] ||
                context[2][0].stackables[4] || context[2][1].stackables[4] || context[2][2].stackables[4]
            ) return 26;

            var pattern = (
                false, context[0][1].stackables[5] ^ context[0][1].stackables[6] ^ context[0][1].stackables[7] ^ context[0][1].stackables[19], false,
                context[1][0].stackables[5] ^ context[1][0].stackables[6] ^ context[1][0].stackables[7] ^ context[1][0].stackables[19], false, context[1][2].stackables[5] ^ context[1][2].stackables[6] ^ context[1][2].stackables[7] ^ context[1][2].stackables[19],
                false, context[2][1].stackables[5] ^ context[2][1].stackables[6] ^ context[2][1].stackables[7] ^ context[2][1].stackables[19], false
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
                context[0][0].geo, context[0][1].geo, context[0][2].geo,
                context[1][0].geo, 0                , context[1][2].geo,
                context[2][0].geo, context[2][1].geo, context[2][2].geo
            );

            directionIndex = geoPattern switch {

                (
                    1, _, 1,
                    1, _, 1,
                    1, 1, 1
                ) => context[0][1].geo is 0 or 6 ? directionIndex : 26,
                
                (
                    1, 1, 1,
                    1, _, _,
                    1, 1, 1
                ) => context[1][2].geo is 0 or 6 ? directionIndex : 26,
                
                (
                    1, 1, 1,
                    1, _, 1,
                    1, _, 1
                ) => context[2][1].geo is 0 or 6 ? directionIndex : 26,
                
                (
                    1, 1, 1,
                    _, _, 1,
                    1, 1, 1
                ) => context[1][0].geo is 0 or 6 ? directionIndex : 26,

                _ => 26
            };

            return directionIndex;
        } else if (i == -11) {
            i = (
                false,                        context[0][1].stackables[11], false,
                context[1][0].stackables[11], false,                        context[1][2].stackables[11],
                false,                        context[2][1].stackables[11], false
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

    static void Main(string[] args) {
        BufferTiles bufferTiles = new(12, 12, 3, 3);

        int matrixWidth = 72;
        int matrixHeight = 43;
        
        uint geoSelectionX = 0;
        uint geoSelectionY = 0;

        int currentLayer = 0;

        int prevCoordsX = -1;
        int prevCoordsY = -1;
        bool multiselect = false;

        int prevMatrixX = -1;
        int prevMatrixY = -1;
        bool clickTracker = false;

        List<List<List<RunCell>>> matrix = [];

        for (int y = 0; y < matrixHeight; y++) {
            matrix.Add([]);

            for (int x = 0; x < matrixWidth; x++) matrix[y].Add([ new(), new(), new() ]);
        }

        Camera2D camera = new() { Zoom = 1.0f, Target = new(-geoselectWidth, 0) };

        Rectangle border = new(
            bufferTiles.Left * scale, 
            bufferTiles.Top * scale, 
            (matrixWidth - (bufferTiles.Right * 2)) * scale, 
            (matrixHeight - (bufferTiles.Bottom * 2)) * scale
            );

        Raylib.InitWindow(1000, 800, "Henry's Leditor");

        Texture2D[] uiTextures = LoadUITextures();
        Texture2D[] geoTextures = LoadGeoTextures();
        Texture2D[] stackableTextures = LoadStackableTextures();

        Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose()) {

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
                geoSelectionY = (--geoSelectionY) % 7;

                multiselect = false;
                prevCoordsX = -1;
                prevCoordsY = -1;
            } else if (Raylib.IsKeyPressed(KeyboardKey.KEY_S)) {
                geoSelectionY = (++geoSelectionY) % 7;

                multiselect = false;
                prevCoordsX = -1;
                prevCoordsY = -1;
            }

            // handle changing layers

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_L)) {
                currentLayer = ++currentLayer % 3;
            }

            uint geoIndex = (4 * geoSelectionY) + geoSelectionX;

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
                        case 16: // glass
                            matrix[matrixY][matrixX][currentLayer].geo = GetBlockID(geoIndex);
                            break;

                        // multi-select: forward to next if-statement
                        case 4:
                        case 5:
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
                        case 22: // bathive
                        case 23: // scav
                        case 24: // wac
                        case 25: // garbageworm
                        case 26: // worm
                        case 27: // forbidflychains
                            var id = GetStackableID(geoIndex);
                            var stackables = matrix[matrixY][matrixX][currentLayer].stackables;
                            
                            var newValue = !stackables[id];
                            if (matrixX != prevMatrixX || matrixY != prevMatrixY || !clickTracker) {
                                
                                if (stackables[id] != newValue) {
                                    stackables[id] = newValue;
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
                                        matrix[y][x][currentLayer].geo = value;
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
                Raylib.ClearBackground(Color.GRAY);

                Raylib.BeginMode2D(camera);
                {

                    // the grid
                    Rlgl.PushMatrix();
                    {
                        Rlgl.Translatef(0, 0, -1);
                        Rlgl.Rotatef(90, 1, 0, 0);
                        Raylib.DrawGrid(matrixWidth * 2, scale);
                    }
                    Rlgl.PopMatrix();

                    // geo matrix
                    for (int y = 0; y < matrixHeight; y++) {
                        for (int x = 0; x < matrixWidth; x++) {
                            for (int z = 2; z > -1; z--) {
                                var stackables = matrix[y][x][z].stackables;
                                var value = matrix[y][x][z].geo;

                                if ((value > 0 && value < 7) || value == 9) {
                                    // a lazy solution to render glass
                                    if (value == 9) Raylib.DrawTexture(geoTextures[7], x * scale, y * scale, layerColors[z]);
                                    else Raylib.DrawTexture(geoTextures[value - 1], x * scale, y * scale, layerColors[z]);
                                }

                                for (int s = 1; s < stackables.Length; s++) {
                                    if (stackables[s]) {
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
                                                RunCell[][] context1 = [
                                                    [ 
                                                        x > 0 && y > 0 ? matrix[y - 1][x - 1][z] : new(), 
                                                        y > 0 ? matrix[y - 1][x][z] : new(), 
                                                        x < matrixWidth -1 && y > 0 ? matrix[y - 1][x + 1][z] : new()
                                                    ],
                                                    [
                                                        x > 0 ? matrix[y][x - 1][z] : new(),
                                                        new(),
                                                        x < matrixWidth -1 ? matrix[y][x + 1][z] : new(),
                                                    ],
                                                    [
                                                        x > 0 && y < matrixHeight -1 ? matrix[y + 1][x - 1][z] : new(),
                                                        y < matrixHeight -1 ? matrix[y + 1][x][z] : new(),
                                                        x < matrixWidth -1 && y < matrixHeight -1 ? matrix[y + 1][x + 1][z] : new()
                                                    ]
                                                ];
                                                Raylib.DrawTexture(stackableTextures[GetStackableTextureIndex(s, context1)], x * scale, y * scale, whiteStackable);
                                                break;
                                            case 11:    // crack
                                                RunCell[][] context2 = [
                                                    [ 
                                                        x > 0 && y > 0 ? matrix[y - 1][x - 1][z] : new(), 
                                                        y > 0 ? matrix[y - 1][x][z] : new(), 
                                                        x < matrixWidth -1 && y > 0 ? matrix[y - 1][x + 1][z] : new()
                                                    ],
                                                    [
                                                        x > 0 ? matrix[y][x - 1][z] : new(),
                                                        new(),
                                                        x < matrixWidth -1 ? matrix[y][x + 1][z] : new(),
                                                    ],
                                                    [
                                                        x > 0 && y < matrixHeight -1 ? matrix[y + 1][x - 1][z] : new(),
                                                        y < matrixHeight -1 ? matrix[y + 1][x][z] : new(),
                                                        x < matrixWidth -1 && y < matrixHeight -1 ? matrix[y + 1][x + 1][z] : new()
                                                    ]
                                                ];

                                                Raylib.DrawTexture(stackableTextures[GetStackableTextureIndex(s, context2)], x * scale, y * scale, whiteStackable);
                                                break;
                                        }
                                    }
                                }
                            }

                            // the multi-select red rectangle

                            if (multiselect) {
                                var XS = matrixX - prevCoordsX;
                                var YS = matrixY - prevCoordsY;
                                var width = Math.Abs(XS) * scale;
                                var height = Math.Abs(YS) * scale;

                                Rectangle rec = (XS > 0, YS > 0) switch {
                                    // br
                                    (true, true) => new(prevCoordsX * scale, prevCoordsY * scale, width, height),
                                    
                                    // tr
                                    (true, false) => new(prevCoordsX * scale, matrixY * scale, width, height),
                                    
                                    // bl
                                    (false, true) => new(matrixX * scale, prevCoordsY * scale, width, height),
                                    
                                    // tl
                                    (false, false) => new(matrixX * scale, matrixY * scale, width, height)
                                };

                                Raylib.DrawRectangleLinesEx(rec, 2, Color.RED);
                                Raylib.DrawText(
                                    $"{width/scale:0}x{height/scale:0}", 
                                    (int)mouse.X + 10, 
                                    (int)mouse.Y, 
                                    4, 
                                    Color.WHITE
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
                    Raylib.DrawRectangle(matrixWidth * -scale, 0, matrixWidth * scale, matrixHeight * 2 * scale, Color.GRAY);
                    Raylib.DrawRectangle(0, matrixHeight * scale, matrixWidth * scale + 2, matrixHeight * scale, Color.GRAY);
                }
                Raylib.EndMode2D();

                // geo menu

                Raylib.DrawRectangle(0, 0, geoselectWidth, geoselectHeight, Color.GRAY);
                Raylib.DrawRectangleLinesEx(new(1, 1, geoselectWidth - 1, geoselectHeight - 1), 1, Color.BLACK);

                for (int w = 0; w < 4; w++) {
                    for (int h = 0; h < 7; h++) {
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

                if (geoIndex < uiTextures.Length) Raylib.DrawText(geoNames[geoIndex], 5, 7 * uiScale + 10, 20, Color.BLACK);

                switch (currentLayer) {
                    case 0:
                        Raylib.DrawRectangle(5, 7 * uiScale + 40, 40, 40, Color.BLACK);
                        Raylib.DrawText("L1", 13, 7 * uiScale + 48, 26, Color.WHITE);
                        break;
                    case 1:
                        Raylib.DrawRectangle(5, 7 * uiScale + 40, 40, 40, Color.DARKGREEN);
                        Raylib.DrawText("L2", 13, 7 * uiScale + 48, 26, Color.WHITE);
                        break;
                    case 2:
                        Raylib.DrawRectangle(5, 7 * uiScale + 40, 40, 40, Color.RED);
                        Raylib.DrawText("L3", 13, 7 * uiScale + 48, 26, Color.WHITE);
                        break;
                }
                Raylib.DrawText($"X = {matrixX:0}\nY = {matrixY:0}", 5, 10 * uiScale + 1, 2, Color.WHITE);
            }
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();

    }
}
