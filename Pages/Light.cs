using System.Numerics;
using System.Text;
using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class LightEditorPage(Serilog.Core.Logger logger) : IPage
{
    readonly Serilog.Core.Logger logger = logger;

    Camera2D camera = new() { zoom = 0.5f, target = new(-500, -200) };
    float lightAngleVariable = 90;
    int lightBrushTextureIndex = 0;
    float lightBrushWidth = 200;
    float lightBrushHeight = 200;
    int lightBrushTexturePage = 0;
    float lightBrushRotation = 0;
    bool eraseShadow = false;
    bool slowGrowth = true;
    bool shading = true;

    const float initialGrowthFactor = 0.01f;
    float growthFactor = initialGrowthFactor;
    
    Rectangle lightBrushSource = new();
    Rectangle lightBrushDest = new();
    Vector2 lightBrushOrigin = new();
    Vector2 lightRecSize = new(100, 100);

    readonly byte[] lightBrushMenuPanelBytes = Encoding.ASCII.GetBytes("Brushes");

    private readonly LightShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.LightEditor;

    public void Draw()
    {
        var ctrl = IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);
        var shift = IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
        var alt = IsKeyDown(KeyboardKey.KEY_LEFT_ALT);

        GLOBALS.PreviousPage = 5;

        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToMainPage.Check(ctrl, shift, alt))
        {
            GLOBALS.Page = 1;
        }
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToGeometryEditor.Check(ctrl, shift, alt))
        {
            GLOBALS.Page = 2;
        }
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToTileEditor.Check(ctrl, shift, alt))
        {
            GLOBALS.Page = 3;
        }
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToCameraEditor.Check(ctrl, shift, alt))
        {
            GLOBALS.Page = 4;
        }
        // if (Raylib.IsKeyReleased(KeyboardKey.KEY_FIVE)) GLOBALS.Page = 5;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToDimensionsEditor.Check(ctrl, shift, alt))
        {
            GLOBALS.ResizeFlag = true;
            GLOBALS.Page = 6;
        }
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToEffectsEditor.Check(ctrl, shift, alt))
        {
            GLOBALS.Page = 7;
        }
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToPropsEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 8;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToSettingsPage.Check(ctrl, shift, alt)) GLOBALS.Page = 9;



        if (_shortcuts.IncreaseFlatness.Check(ctrl, shift, alt, true) && GLOBALS.Level.LightFlatness < 10) GLOBALS.Level.LightFlatness++;
        if (_shortcuts.DecreaseFlatness.Check(ctrl, shift, alt, true) && GLOBALS.Level.LightFlatness > 1) GLOBALS.Level.LightFlatness--;

        const int textureSize = 130;

        var panelHeight = Raylib.GetScreenHeight() - 100;

        var pageSize = panelHeight / textureSize;

        if (_shortcuts.IncreaseAngle.Check(ctrl, shift, alt, true))
        {
            // lightAngleVariable += 0.01f;
            GLOBALS.Level.LightAngle--;/* = (int)(180 * Math.Sin(lightAngleVariable) + 90);*/
        }
        if (_shortcuts.DecreaseAngle.Check(ctrl, shift, alt, true))
        {
            // lightAngleVariable -= 0.01f;
            GLOBALS.Level.LightAngle++;/* = (int)(180 * Math.Sin(lightAngleVariable) + 90);*/
        }

        // handle mouse drag
        if (_shortcuts.DragLevel.Check(ctrl, shift, alt, true) || _shortcuts.DragLevelAlt.Check(ctrl, shift, alt, true))
        {
            var delta = GetMouseDelta();
            delta = RayMath.Vector2Scale(delta, -1.0f / camera.zoom);
            camera.target = RayMath.Vector2Add(camera.target, delta);
        }

        // handle zoom
        var wheel2 = GetMouseWheelMove();
        if (wheel2 != 0)
        {
            var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), camera);
            camera.offset = GetMousePosition();
            camera.target = mouseWorldPosition;
            camera.zoom += wheel2 * GLOBALS.ZoomIncrement;
            if (camera.zoom < GLOBALS.ZoomIncrement) camera.zoom = GLOBALS.ZoomIncrement;
        }

        // update light brush

        {
            var texture = GLOBALS.Textures.LightBrushes[lightBrushTextureIndex];
            var lightMouse = GetScreenToWorld2D(GetMousePosition(), camera);

            lightBrushSource = new(0, 0, texture.width, texture.height);
            lightBrushDest = new(lightMouse.X, lightMouse.Y, lightBrushWidth, lightBrushHeight);
            lightBrushOrigin = new(lightBrushWidth / 2, lightBrushHeight / 2);
        }

        if (_shortcuts.NextBrush.Check(ctrl, shift, alt))
        {
            lightBrushTextureIndex = ++lightBrushTextureIndex % GLOBALS.Textures.LightBrushes.Length;

            lightBrushTexturePage = lightBrushTextureIndex / pageSize;
        }
        else if (_shortcuts.PreviousBrush.Check(ctrl, shift, alt))
        {
            lightBrushTextureIndex--;

            if (lightBrushTextureIndex < 0) lightBrushTextureIndex = GLOBALS.Textures.LightBrushes.Length - 1;

            lightBrushTexturePage = lightBrushTextureIndex / pageSize;
        }

        if (_shortcuts.RotateBrushCounterClockwise.Check(ctrl, shift, alt, true))
        {
            lightBrushRotation -= 0.2f;
        } else if (_shortcuts.FastRotateBrushCounterClockwise.Check(ctrl, shift, alt, true))
        {
            lightBrushRotation -= 1;
        }

        if (_shortcuts.RotateBrushClockwise.Check(ctrl, shift, alt, true))
        {
            
            lightBrushRotation += 0.2f;
        }
        else if (_shortcuts.FastRotateBrushClockwise.Check(ctrl, shift, alt, true))
        {
            lightBrushRotation += 1;
        }

        if (_shortcuts.StretchBrushVertically.Check(ctrl, shift, alt, true))
        {
            lightBrushHeight += 2;
        }
        else if (_shortcuts.FastStretchBrushVertically.Check(ctrl, shift, alt, true))
        {
            lightBrushHeight += 5;
        }
        else if (_shortcuts.SqueezeBrushVertically.Check(ctrl, shift, alt, true))
        {
            lightBrushHeight -= 2;
        }
        else if (_shortcuts.FastSqueezeBrushVertically.Check(ctrl, shift, alt, true))
        {
            lightBrushHeight -= 5;
        }

        if (_shortcuts.StretchBrushHorizontally.Check(ctrl, shift, alt, true))
        {
            lightBrushWidth += 2;
        }
        else if (_shortcuts.FastStretchBrushHorizontally.Check(ctrl, shift, alt, true))
        {
            lightBrushWidth += 5;
        }
        else if (_shortcuts.SqueezeBrushHorizontally.Check(ctrl, shift, alt, true))
        {
            
            lightBrushWidth -= 2;
        }
        else if (_shortcuts.FastSqueezeBrushHorizontally.Check(ctrl, shift, alt, true))
        {
            lightBrushWidth -= 5;
        }

        if (_shortcuts.ToggleShadow.Check(ctrl, shift, alt))
        {
            eraseShadow = !eraseShadow;
        }

        //

        if (_shortcuts.Paint.Check(ctrl, shift, alt, true) || _shortcuts.PaintAlt.Check(ctrl, shift, alt, true))
        {
            BeginTextureMode(GLOBALS.Textures.LightMap);
            {
                if (eraseShadow)
                {
                    BeginShaderMode(GLOBALS.Shaders.ApplyLightBrush);
                    SetShaderValueTexture(GLOBALS.Shaders.ApplyLightBrush, GetShaderLocation(GLOBALS.Shaders.ApplyLightBrush, "inputTexture"), GLOBALS.Textures.LightBrushes[lightBrushTextureIndex]);
                    DrawTexturePro(
                        GLOBALS.Textures.LightBrushes[lightBrushTextureIndex],
                        lightBrushSource,
                        lightBrushDest,
                        lightBrushOrigin,
                        lightBrushRotation,
                        new Color(255, 255, 255, 255)
                        );
                    EndShaderMode();
                }
                else
                {
                    BeginShaderMode(GLOBALS.Shaders.ApplyShadowBrush);
                    SetShaderValueTexture(GLOBALS.Shaders.ApplyShadowBrush, GetShaderLocation(GLOBALS.Shaders.ApplyShadowBrush, "inputTexture"), GLOBALS.Textures.LightBrushes[lightBrushTextureIndex]);
                    DrawTexturePro(
                        GLOBALS.Textures.LightBrushes[lightBrushTextureIndex],
                        lightBrushSource,
                        lightBrushDest,
                        lightBrushOrigin,
                        lightBrushRotation,
                        new(0, 0, 0, 255)
                        );
                    EndShaderMode();
                }
            }
            Raylib.EndTextureMode();
        }

        if (_shortcuts.SlowWarpSpeed.Check(ctrl, shift, alt)) slowGrowth = !slowGrowth;
        if (IsKeyPressed(KeyboardKey.KEY_R)) shading = !shading;

        /*if (slowGrowth)
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
        }*/


        Raylib.BeginDrawing();
        {
            Raylib.ClearBackground(GLOBALS.Settings.LightEditor.Background);

            Raylib.BeginMode2D(camera);
            {
                Raylib.DrawRectangle(
                    0, 0,
                    GLOBALS.Level.Width * GLOBALS.Scale + 300,
                    GLOBALS.Level.Height * GLOBALS.Scale + 300,
                    new(255, 255, 255, 255)
                );

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
                                DrawTexture(GLOBALS.Textures.GeoBlocks[texture], x * GLOBALS.Scale + 300, y * GLOBALS.Scale + 300, new(0, 0, 0, 150));
                            }
                        }
                    }
                }

                if (!GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1)
                {
                    Raylib.DrawRectangle(
                        (-1) * GLOBALS.Scale + 300,
                        (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel) * GLOBALS.Scale + 300,
                        (GLOBALS.Level.Width + 2) * GLOBALS.Scale,
                        GLOBALS.Level.WaterLevel * GLOBALS.Scale,
                        new(0, 0, 255, 255)
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
                            Raylib.DrawTexture(GLOBALS.Textures.GeoBlocks[texture], x * GLOBALS.Scale + 300, y * GLOBALS.Scale + 300, new(0, 0, 0, 225));
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
                                        Raylib.DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], x * GLOBALS.Scale + 300, y * GLOBALS.Scale + 300, BLACK);
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
                                        DrawTexture(GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s)], x * GLOBALS.Scale + 300, y * GLOBALS.Scale + 300, WHITE);
                                        break;

                                    // directional placement
                                    case 4:     // entrance
                                        var index = Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, 0));

                                        if (index is 22 or 23 or 24 or 25)
                                        {
                                            GLOBALS.Level.GeoMatrix[y, x, 0].Geo = 7;
                                        }

                                        Raylib.DrawTexture(GLOBALS.Textures.GeoStackables[index], x * GLOBALS.Scale + 300, y * GLOBALS.Scale + 300, WHITE);
                                        break;
                                    case 11:    // crack
                                        Raylib.DrawTexture(
                                            GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(s, Utils.GetContext(GLOBALS.Level.GeoMatrix, GLOBALS.Level.Width, GLOBALS.Level.Height, x, y, 0))],
                                            x * GLOBALS.Scale + 300,
                                            y * GLOBALS.Scale + 300,
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
                        (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel) * GLOBALS.Scale + 300,
                        (GLOBALS.Level.Width + 2) * GLOBALS.Scale + 300,
                        GLOBALS.Level.WaterLevel * GLOBALS.Scale,
                        new(0, 0, 255, 110)
                    );
                }

                Raylib.DrawTextureRec(
                    GLOBALS.Textures.LightMap.texture,
                    new Rectangle(0, 0, GLOBALS.Textures.LightMap.texture.width, -GLOBALS.Textures.LightMap.texture.height),
                    new(0, 0),
                    new(255, 255, 255, 150)
                );

                // The brush

                if (eraseShadow)
                {
                    BeginShaderMode(GLOBALS.Shaders.LightBrush);
                    SetShaderValueTexture(GLOBALS.Shaders.LightBrush, GetShaderLocation(GLOBALS.Shaders.LightBrush, "inputTexture"), GLOBALS.Textures.LightBrushes[lightBrushTextureIndex]);

                    Raylib.DrawTexturePro(
                        GLOBALS.Textures.LightBrushes[lightBrushTextureIndex],
                        lightBrushSource,
                        lightBrushDest,
                        lightBrushOrigin,
                        lightBrushRotation,
                        WHITE
                        );
                    EndShaderMode();
                }
                else
                {
                    BeginShaderMode(GLOBALS.Shaders.ShadowBrush);
                    SetShaderValueTexture(GLOBALS.Shaders.ShadowBrush, GetShaderLocation(GLOBALS.Shaders.ShadowBrush, "inputTexture"), GLOBALS.Textures.LightBrushes[lightBrushTextureIndex]);

                    DrawTexturePro(
                        GLOBALS.Textures.LightBrushes[lightBrushTextureIndex],
                        lightBrushSource,
                        lightBrushDest,
                        lightBrushOrigin,
                        lightBrushRotation,
                        WHITE
                        );
                    EndShaderMode();
                }

            }
            Raylib.EndMode2D();

            // brush menu

            {
                unsafe
                {
                    fixed (byte* pt = lightBrushMenuPanelBytes)
                    {
                        RayGui.GuiPanel(new(10, 50, 150, panelHeight), (sbyte*)pt);
                    }
                }

                var currentPage = GLOBALS.Textures.LightBrushes
                    .Select((texture, index) => (index, texture))
                    .Skip(lightBrushTexturePage * pageSize)
                    .Take(pageSize)
                    .Select((value, index) => (index, value));

                // Brush menu

                foreach (var (pageIndex, (index, texture)) in currentPage)
                {
                    BeginShaderMode(GLOBALS.Shaders.ApplyShadowBrush);
                    SetShaderValueTexture(GLOBALS.Shaders.ApplyShadowBrush, GetShaderLocation(GLOBALS.Shaders.ApplyShadowBrush, "inputTexture"), texture);
                    Raylib.DrawTexturePro(
                        texture,
                        new(0, 0, texture.width, texture.height),
                        new(25, (textureSize + 1) * pageIndex + 80 + 5, textureSize - 10, textureSize - 10),
                        new(0, 0),
                        0,
                        new(0, 0, 0, 255)
                        );
                    EndShaderMode();

                    if (index == lightBrushTextureIndex) Raylib.DrawRectangleLinesEx(
                        new(
                            20,
                            (textureSize + 1) * pageIndex + 80,
                            textureSize,
                            textureSize
                        ),
                        4.0f,
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
                15 + (GLOBALS.Level.LightFlatness * 7),
                new(255, 0, 0, 255)
            );


            Raylib.DrawCircleV(new Vector2(
                (GetScreenWidth() - 100) + (float)((15 + GLOBALS.Level.LightFlatness * 7) * -Math.Cos(float.DegreesToRadians(GLOBALS.Level.LightAngle - 180))),
                (GetScreenHeight() - 100) + (float)((15 + GLOBALS.Level.LightFlatness * 7) * Math.Sin(float.DegreesToRadians(GLOBALS.Level.LightAngle - 180)))
                ),
                10.0f,
                new(255, 0, 0, 255)
            );


        }
        Raylib.EndDrawing();




    }
}
