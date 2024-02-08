using System.Numerics;
using System.Text;
using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class MainPage(Serilog.Core.Logger logger) : IPage
{
    readonly Serilog.Core.Logger logger = logger;

    Camera2D camera = new() { zoom = 0.5f };

    readonly byte[] previewPanelBytes = Encoding.ASCII.GetBytes("Level Options");

    public void Draw()
    {
        GLOBALS.PreviousPage = 1;

        if (IsKeyReleased(KeyboardKey.KEY_TWO))
        {
            GLOBALS.Page = 2;
        }
        if (IsKeyReleased(KeyboardKey.KEY_THREE))
        {
            GLOBALS.Page = 3;
        }
        if (IsKeyReleased(KeyboardKey.KEY_FOUR))
        {
            GLOBALS.Page = 4;
        }
        if (IsKeyReleased(KeyboardKey.KEY_FIVE))
        {
            GLOBALS.Page = 5;
        }
        if (IsKeyReleased(KeyboardKey.KEY_SIX))
        {
            GLOBALS.ResizeFlag = true;
            GLOBALS.Page = 6;
        }
        if (IsKeyReleased(KeyboardKey.KEY_SEVEN))
        {
            GLOBALS.Page = 7;
        }
        if (IsKeyReleased(KeyboardKey.KEY_EIGHT))
        {
            GLOBALS.Page = 8;
        }
        if (IsKeyReleased(KeyboardKey.KEY_NINE))
        {
            GLOBALS.Page = 9;
        }

        // handle zoom
        var mainPageWheel = Raylib.GetMouseWheelMove();
        if (mainPageWheel != 0)
        {
            Vector2 mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
            camera.offset = Raylib.GetMousePosition();
            camera.target = mouseWorldPosition;
            camera.zoom += mainPageWheel * GLOBALS.ZoomIncrement;
            if (camera.zoom < GLOBALS.ZoomIncrement) camera.zoom = GLOBALS.ZoomIncrement;
        }

        // handle mouse drag
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
        {
            Vector2 delta = Raylib.GetMouseDelta();
            delta = RayMath.Vector2Scale(delta, -1.0f / camera.zoom);
            camera.target = RayMath.Vector2Add(camera.target, delta);
        }

        Raylib.BeginDrawing();
        {
            Raylib.ClearBackground(new(170, 170, 170, 255));

            Raylib.BeginMode2D(camera);
            {
                Raylib.DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.Scale, GLOBALS.Level.Height * GLOBALS.Scale, new(255, 255, 255, 255));

                for (int y = 0; y < GLOBALS.Level.Height; y++)
                {
                    for (int x = 0; x < GLOBALS.Level.Width; x++)
                    {
                        for (int z = 1; z < 3; z++)
                        {
                            var cell = GLOBALS.Level.GeoMatrix[y, x, z];

                            var texture = Utils.GetBlockIndex(cell.Geo);

                            if (texture >= 0)
                            {
                                Raylib.DrawTexture(GLOBALS.Textures.GeoBlocks[texture], x * GLOBALS.Scale, y * GLOBALS.Scale, new(0, 0, 0, 170));
                            }
                        }
                    }
                }

                if (!GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1)
                {
                    Raylib.DrawRectangle(
                        (-1) * GLOBALS.Scale,
                        (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel) * GLOBALS.Scale,
                        (GLOBALS.Level.Width + 2) * GLOBALS.Scale,
                        GLOBALS.Level.WaterLevel * GLOBALS.Scale,
                        new(0, 0, 255, 250)
                    );
                }

                for (int y = 0; y < GLOBALS.Level.Height; y++)
                {
                    for (int x = 0; x < GLOBALS.Level.Width; x++)
                    {
                        var cell = GLOBALS.Level.GeoMatrix[y, x, 0];

                        var texture = Utils.GetBlockIndex(cell.Geo);

                        if (texture >= 0)
                        {
                            Raylib.DrawTexture(GLOBALS.Textures.GeoBlocks[texture], x * GLOBALS.Scale, y * GLOBALS.Scale, new(0, 0, 0, 225));
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
                                        Raylib.DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], x * GLOBALS.Scale, y * GLOBALS.Scale, BLACK);
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
                                        Raylib.DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], x * GLOBALS.Scale, y * GLOBALS.Scale, WHITE);
                                        break;

                                    // directional placement
                                    case 4:     // entrance
                                        var index = Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, 0));

                                        if (index is 22 or 23 or 24 or 25)
                                        {
                                            GLOBALS.Level.GeoMatrix[y, x, 0].Geo = 7;
                                        }

                                        Raylib.DrawTexture(GLOBALS.Textures.GeoStackables[index], x * GLOBALS.Scale, y * GLOBALS.Scale, WHITE);
                                        break;
                                    case 11:    // crack
                                        Raylib.DrawTexture(
                                            GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, 0))],
                                            x * GLOBALS.Scale,
                                            y * GLOBALS.Scale,
                                            BLACK
                                        );
                                        break;
                                }
                            }
                        }
                    }
                }

                if (GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1)
                {
                    Raylib.DrawRectangle(
                        (-1) * GLOBALS.Scale,
                        (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel) * GLOBALS.Scale,
                        (GLOBALS.Level.Width + 2) * GLOBALS.Scale,
                        GLOBALS.Level.WaterLevel * GLOBALS.Scale,
                        new(0, 0, 255, 110)
                    );
                }
            }
            Raylib.EndMode2D();

            unsafe
            {
                fixed (byte* pt = previewPanelBytes)
                {
                    Raylib_CsLo.RayGui.GuiPanel(
                        new(
                            Raylib.GetScreenWidth() - 400,
                            50,
                            380,
                            GetScreenHeight() - 100
                        ),
                        (sbyte*)pt
                    );

                    DrawText(
                        GLOBALS.Level.ProjectName,
                        GetScreenWidth() - 350,
                        100,
                        30,
                        new(0, 0, 0, 255)
                    );

                    var helpPressed = RayGui.GuiButton(new(
                        GetScreenWidth() - 80,
                        100,
                        40,
                        40
                    ),
                    "?"
                    );

                    if (helpPressed) GLOBALS.Page = 9;

                    Raylib.DrawText("Seed", Raylib.GetScreenWidth() - 380, 205, 11, new(0, 0, 0, 255));

                    GLOBALS.Level.Seed = (int)Math.Round(Raylib_CsLo.RayGui.GuiSlider(
                        new(
                            Raylib.GetScreenWidth() - 290,
                            200,
                            200,
                            20
                        ),
                        "0",
                        "400",
                        GLOBALS.Level.Seed,
                        0,
                        400
                    ));

                    GLOBALS.Level.LightMode = RayGui.GuiCheckBox(new(
                        Raylib.GetScreenWidth() - 380,
                        250,
                        20,
                        20
                    ),
                    "Light Mode",
                    GLOBALS.Level.LightMode);

                    GLOBALS.Level.DefaultTerrain = RayGui.GuiCheckBox(new(
                        GetScreenWidth() - 380,
                        290,
                        20,
                        20
                    ),
                    "Default Medium",
                    GLOBALS.Level.DefaultTerrain);

                    Raylib.DrawText("Water Level",
                        Raylib.GetScreenWidth() - 380,
                        335,
                        11,
                        new(0, 0, 0, 255)
                    );

                    GLOBALS.Level.WaterLevel = (int)Math.Round(Raylib_CsLo.RayGui.GuiSlider(
                        new(
                            Raylib.GetScreenWidth() - 290,
                            330,
                            200,
                            20
                        ),
                        "-1",
                        "999",
                        GLOBALS.Level.WaterLevel,
                        -1,
                        GLOBALS.Level.Height + 10
                    ));

                    GLOBALS.Level.WaterAtFront = Raylib_CsLo.RayGui.GuiCheckBox(
                        new(
                            Raylib.GetScreenWidth() - 380,
                            360,
                            20,
                            20
                        ),
                        "Water In Front",
                        GLOBALS.Level.WaterAtFront
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
                        GLOBALS.Page = 12;
                    }

                    var loadPressed = RayGui.GuiButton(new(
                            GetScreenWidth() - 390,
                            GetScreenHeight() - 150,
                            360,
                            40
                        ),
                        "Load Project"
                    );

                    var newPressed = RayGui.GuiButton(new(
                            GetScreenWidth() - 390,
                            GetScreenHeight() - 100,
                            360,
                            40
                        ),
                        "New Project"
                    );

                    if (loadPressed) GLOBALS.Page = 0;
                    if (newPressed)
                    {
                        GLOBALS.NewFlag = true;
                        GLOBALS.Page = 6;
                    }
                }
            }

        }
        EndDrawing();
    }
}
