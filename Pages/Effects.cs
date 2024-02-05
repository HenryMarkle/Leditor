﻿using System.Numerics;
using System.Text;
using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class EffectsEditorPage(Serilog.Core.Logger logger) : IPage
{
    readonly Serilog.Core.Logger logger = logger;

    Camera2D camera = new() { zoom = 1.0f };

    bool addNewEffectMode = false;
    bool newEffectFocus = false;

    int newEffectScrollIndex = 0;
    int newEffectSelectedValue = 0;

    int newEffectCategoryScrollIndex = 0;
    int newEffectCategorySelectedValue = 0;

    int currentAppliedEffect = 0;
    int currentAppliedEffectPage = 0;

    int prevMatrixX = -1;
    int prevMatrixY = -1;

    int brushRadius = 3;

    bool clickTracker = false;
    bool brushEraseMode = false;
    bool showEffectOptions = true;

    private int _optionsIndex = 1;

    readonly byte[] addNewEffectPanelBytes = Encoding.ASCII.GetBytes("New Effect");
    readonly byte[] appliedEffectsPanelBytes = Encoding.ASCII.GetBytes("Applied Effects");
    readonly byte[] effectOptionsPanelBytes = Encoding.ASCII.GetBytes("Options");

    // Paints an effect in the effects editor
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

    public void Draw()
    {
        GLOBALS.PreviousPage = 7;

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_ONE)) GLOBALS.Page = 1;
        if (Raylib.IsKeyReleased(KeyboardKey.KEY_TWO)) GLOBALS.Page = 2;
        if (Raylib.IsKeyReleased(KeyboardKey.KEY_THREE)) GLOBALS.Page = 3;
        if (Raylib.IsKeyReleased(KeyboardKey.KEY_FOUR)) GLOBALS.Page = 4;
        if (Raylib.IsKeyReleased(KeyboardKey.KEY_FIVE)) GLOBALS.Page = 5;
        if (Raylib.IsKeyReleased(KeyboardKey.KEY_SIX))
        {
            GLOBALS.ResizeFlag = true;
            GLOBALS.Page = 6;
            logger.Debug("go from GLOBALS.Page 7 to GLOBALS.Page 6");
        }
        // if (Raylib.IsKeyReleased(KeyboardKey.KEY_SEVEN)) GLOBALS.Page = 7;
        if (Raylib.IsKeyReleased(KeyboardKey.KEY_EIGHT)) GLOBALS.Page = 8;
        if (Raylib.IsKeyReleased(KeyboardKey.KEY_NINE)) GLOBALS.Page = 9;

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
                    if (newEffectSelectedValue < 0) newEffectSelectedValue = GLOBALS.Effects[newEffectCategorySelectedValue].Length - 1;
                }
                else
                {
                    newEffectSelectedValue = 0;

                    newEffectCategorySelectedValue = --newEffectCategorySelectedValue;

                    if (newEffectCategorySelectedValue < 0) newEffectCategorySelectedValue = GLOBALS.EffectCategories.Length - 1;
                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_DOWN))
            {
                if (newEffectFocus)
                {
                    newEffectSelectedValue = ++newEffectSelectedValue % GLOBALS.Effects[newEffectCategorySelectedValue].Length;
                }
                else
                {
                    newEffectSelectedValue = 0;
                    newEffectCategorySelectedValue = ++newEffectCategorySelectedValue % GLOBALS.EffectCategories.Length;
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

            if (IsKeyPressed(KeyboardKey.KEY_ENTER))
            {
                GLOBALS.Level.Effects = [
                    .. GLOBALS.Level.Effects,
                    (
                        GLOBALS.Effects[newEffectCategorySelectedValue][newEffectSelectedValue],
                        Utils.NewEffectOptions(GLOBALS.Effects[newEffectCategorySelectedValue][newEffectSelectedValue]),
                        new double[GLOBALS.Level.Height, GLOBALS.Level.Width]
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

                RayGui.GuiLine(
                    new(
                        Raylib.GetScreenWidth() / 2 - 390,
                        Raylib.GetScreenHeight() / 2 - 265,
                        150,
                        10
                    ),
                    "Categories"
                );

                unsafe
                {
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

                    fixed (int* scrollIndex = &newEffectCategoryScrollIndex)
                    {
                        newEffectCategorySelectedValue = RayGui.GuiListView(
                            new(
                                Raylib.GetScreenWidth() / 2 - 390,
                                Raylib.GetScreenHeight() / 2 - 250,
                                150,
                                540
                            ),
                            string.Join(";", GLOBALS.EffectCategories),
                            scrollIndex,
                            newEffectCategorySelectedValue
                        );
                    }
                }


                if (!newEffectFocus) DrawRectangleLinesEx(
                    new(
                        GetScreenWidth() / 2 - 390,
                        GetScreenHeight() / 2 - 250,
                        150,
                        540
                    ),
                    2.0f,
                    new(0, 0, 255, 255)
                );

                unsafe
                {
                    fixed (int* scrollIndex = &newEffectScrollIndex)
                    {
                        newEffectSelectedValue = RayGui.GuiListView(
                            new(
                                GetScreenWidth() / 2 - 230,
                                GetScreenHeight() / 2 - 250,
                                620,
                                540
                            ),
                            string.Join(";", GLOBALS.Effects[newEffectCategorySelectedValue]),
                            scrollIndex,
                            newEffectSelectedValue
                        );
                    }
                }


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

            Vector2 effectsMouse = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);

            //                        v this was done to avoid rounding errors
            int effectsMatrixY = effectsMouse.Y < 0 ? -1 : (int)effectsMouse.Y / GLOBALS.PreviewScale;
            int effectsMatrixX = effectsMouse.X < 0 ? -1 : (int)effectsMouse.X / GLOBALS.PreviewScale;


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
                delta = RayMath.Vector2Scale(delta, -1.0f / camera.zoom);
                camera.target = RayMath.Vector2Add(camera.target, delta);
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

            if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                if (
                        effectsMatrixX >= 0 &&
                        effectsMatrixX < GLOBALS.Level.Width &&
                        effectsMatrixY >= 0 &&
                        effectsMatrixY < GLOBALS.Level.Height &&
                        (
                            effectsMatrixX != prevMatrixX || effectsMatrixY != prevMatrixY || !clickTracker
                        ))
                {
                    var mtx = GLOBALS.Level.Effects[currentAppliedEffect].Item3;

                    if (brushEraseMode)
                    {
                        //mtx[effectsMatrixY, effectsMatrixX] -= Effects.GetBrushStrength(GLOBALS.Level.Effects[currentAppliedEffect].Item1);

                        //if (mtx[effectsMatrixY, effectsMatrixX] < 0) mtx[effectsMatrixY, effectsMatrixX] = 0;

                        PaintEffect(
                            GLOBALS.Level.Effects[currentAppliedEffect].Item3,
                            (GLOBALS.Level.Width, GLOBALS.Level.Height),
                            (effectsMatrixX, effectsMatrixY),
                            brushRadius,
                            -Utils.GetBrushStrength(GLOBALS.Level.Effects[currentAppliedEffect].Item1)
                        );
                    }
                    else
                    {
                        //mtx[effectsMatrixY, effectsMatrixX] += Effects.GetBrushStrength(GLOBALS.Level.Effects[currentAppliedEffect].Item1);

                        PaintEffect(
                            GLOBALS.Level.Effects[currentAppliedEffect].Item3,
                            (GLOBALS.Level.Width, GLOBALS.Level.Height),
                            (effectsMatrixX, effectsMatrixY),
                            brushRadius,
                            Utils.GetBrushStrength(GLOBALS.Level.Effects[currentAppliedEffect].Item1)
                            );

                        //if (mtx[effectsMatrixY, effectsMatrixX] > 100) mtx[effectsMatrixY, effectsMatrixX] = 100;
                    }

                    prevMatrixX = effectsMatrixX;
                    prevMatrixY = effectsMatrixY;
                }

                clickTracker = true;
            }

            if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
            {
                clickTracker = false;
            }

            //

            if (IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
            {
                var index = currentAppliedEffect;

                if (IsKeyPressed(KeyboardKey.KEY_W))
                {
                    if (index > 0)
                    {
                        (GLOBALS.Level.Effects[index], GLOBALS.Level.Effects[index - 1]) = (GLOBALS.Level.Effects[index - 1], GLOBALS.Level.Effects[index]);
                        currentAppliedEffect--;
                    }
                }
                else if (IsKeyPressed(KeyboardKey.KEY_S))
                {
                    if (index < GLOBALS.Level.Effects.Length - 1)
                    {
                        (GLOBALS.Level.Effects[index], GLOBALS.Level.Effects[index + 1]) = (GLOBALS.Level.Effects[index + 1], GLOBALS.Level.Effects[index]);
                        currentAppliedEffect++;
                    }
                }
            }
            // Cycle options
            else if (IsKeyDown(KeyboardKey.KEY_LEFT_ALT))
            {
                if (IsKeyPressed(KeyboardKey.KEY_W))
                {
                    _optionsIndex--;
                    if (_optionsIndex < 2) _optionsIndex = GLOBALS.Level.Effects[currentAppliedEffect].Item2.Length - 1;
                }
                else if (IsKeyPressed(KeyboardKey.KEY_S))
                {
                    _optionsIndex = ++_optionsIndex % GLOBALS.Level.Effects[currentAppliedEffect].Item2.Length;
                    if (_optionsIndex == 0) _optionsIndex = 1;
                }
                else if (IsKeyPressed(KeyboardKey.KEY_D))
                {
                    var option = GLOBALS.Level.Effects[currentAppliedEffect].Item2[_optionsIndex];
                    var choiceIndex = Array.FindIndex(option.Options, op => op == option.Choice);
                    choiceIndex = ++choiceIndex % option.Options.Length;
                    option.Choice = option.Options[choiceIndex];
                }
                else if (IsKeyPressed(KeyboardKey.KEY_A))
                {
                    var option = GLOBALS.Level.Effects[currentAppliedEffect].Item2[_optionsIndex];
                    var choiceIndex = Array.FindIndex(option.Options, op => op == option.Choice);
                    choiceIndex--;
                    if (choiceIndex < 0) choiceIndex = option.Options.Length - 1;
                    option.Choice = option.Options[choiceIndex];
                }
            }
            else
            {
                if (IsKeyPressed(KeyboardKey.KEY_W))
                {
                    currentAppliedEffect--;

                    if (currentAppliedEffect < 0) currentAppliedEffect = GLOBALS.Level.Effects.Length - 1;

                    currentAppliedEffectPage = currentAppliedEffect / appliedEffectPageSize;

                    _optionsIndex = 1;
                }

                if (IsKeyPressed(KeyboardKey.KEY_S))
                {
                    currentAppliedEffect = ++currentAppliedEffect % GLOBALS.Level.Effects.Length;

                    currentAppliedEffectPage = currentAppliedEffect / appliedEffectPageSize;
                    
                    _optionsIndex = 1;
                }
            }

            if (IsKeyPressed(KeyboardKey.KEY_O)) showEffectOptions = !showEffectOptions;
            if (IsKeyPressed(KeyboardKey.KEY_Q)) brushEraseMode = !brushEraseMode;


            // Delete effect
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_X))
            {
                GLOBALS.Level.Effects = GLOBALS.Level.Effects.Where((e, i) => i != currentAppliedEffect).ToArray();
                currentAppliedEffect--;
                if (currentAppliedEffect < 0) currentAppliedEffect = GLOBALS.Level.Effects.Length - 1;
            }

            Raylib.BeginDrawing();
            {

                Raylib.ClearBackground(new(0, 0, 0, 255));

                Raylib.BeginMode2D(camera);
                {
                    // Outer level border
                    DrawRectangleLinesEx(
                        new Rectangle(
                            -2, -2,
                            (GLOBALS.Level.Width * GLOBALS.PreviewScale) + 4,
                            (GLOBALS.Level.Height * GLOBALS.PreviewScale) + 4
                        ),
                        2f,
                        new(255, 255, 255, 255)
                    );
                    
                    DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.PreviewScale, GLOBALS.Level.Height * GLOBALS.PreviewScale, new(255, 255, 255, 255));

                    Printers.DrawGeoLayer(2, GLOBALS.PreviewScale, false, BLACK with { a = 100 }, GLOBALS.LayerStackableFilter);
                    Printers.DrawTileLayer(2, GLOBALS.PreviewScale, false, true, true);
                    
                    Printers.DrawGeoLayer(1, GLOBALS.PreviewScale, false, BLACK with { a = 200 }, GLOBALS.LayerStackableFilter);
                    Printers.DrawTileLayer(1, GLOBALS.PreviewScale, false, true, true);
                    
                    Printers.DrawGeoLayer(0, GLOBALS.PreviewScale, false, BLACK with { a = 255 });
                    Printers.DrawTileLayer(0, GLOBALS.PreviewScale, false, true, true);
                    

                    // Effect matrix

                    if (GLOBALS.Level.Effects.Length > 0 &&
                        currentAppliedEffect >= 0 &&
                        currentAppliedEffect < GLOBALS.Level.Effects.Length)
                    {

                        DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.PreviewScale, GLOBALS.Level.Height * GLOBALS.PreviewScale, new(215, 66, 245, 100));

                        for (int y = 0; y < GLOBALS.Level.Height; y++)
                        {
                            for (int x = 0; x < GLOBALS.Level.Width; x++)
                            {
                                DrawRectangle(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale, new(0, 255, 0, (int)GLOBALS.Level.Effects[currentAppliedEffect].Item3[y, x] * 255 / 100));
                            }
                        }
                    }

                    // Brush

                    for (int y = 0; y < GLOBALS.Level.Height; y++)
                    {
                        for (int x = 0; x < GLOBALS.Level.Width; x++)
                        {
                            if (effectsMatrixX == x && effectsMatrixY == y)
                            {
                                if (brushEraseMode)
                                {
                                    Raylib.DrawRectangleLinesEx(
                                        new Rectangle(
                                            x * GLOBALS.PreviewScale,
                                            y * GLOBALS.PreviewScale,
                                            GLOBALS.PreviewScale,
                                            GLOBALS.PreviewScale
                                        ),
                                        2.0f,
                                        new(255, 0, 0, 255)
                                    );


                                    Raylib.DrawRectangleLines(
                                        (effectsMatrixX - brushRadius) * GLOBALS.PreviewScale,
                                        (effectsMatrixY - brushRadius) * GLOBALS.PreviewScale,
                                        (brushRadius * 2 + 1) * GLOBALS.PreviewScale,
                                        (brushRadius * 2 + 1) * GLOBALS.PreviewScale,
                                        new(255, 0, 0, 255));
                                }
                                else
                                {
                                    Raylib.DrawRectangleLinesEx(
                                        new Rectangle(
                                            x * GLOBALS.PreviewScale,
                                            y * GLOBALS.PreviewScale,
                                            GLOBALS.PreviewScale,
                                            GLOBALS.PreviewScale
                                        ),
                                        2.0f,
                                        new(255, 255, 255, 255)
                                    );

                                    Raylib.DrawRectangleLines(
                                        (effectsMatrixX - brushRadius) * GLOBALS.PreviewScale,
                                        (effectsMatrixY - brushRadius) * GLOBALS.PreviewScale,
                                        (brushRadius * 2 + 1) * GLOBALS.PreviewScale,
                                        (brushRadius * 2 + 1) * GLOBALS.PreviewScale,
                                        new(255, 255, 255, 255));
                                }
                            }
                        }
                    }

                }
                Raylib.EndMode2D();

                // UI

                unsafe
                {
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
                }

                if (GLOBALS.Level.Effects.Length > appliedEffectPageSize)
                {
                    if (currentAppliedEffectPage < (GLOBALS.Level.Effects.Length / appliedEffectPageSize))
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

                // i is index relative to the GLOBALS.Page; oi is index relative to the whole list
                foreach (var (i, (oi, e)) in GLOBALS.Level.Effects.Select((value, i) => (i, value)).Skip(appliedEffectPageSize * currentAppliedEffectPage).Take(appliedEffectPageSize).Select((value, i) => (i, value)))
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
                        GLOBALS.Level.Effects = GLOBALS.Level.Effects.Where((e, i) => i != oi).ToArray();
                        currentAppliedEffect--;
                        if (currentAppliedEffect < 0) currentAppliedEffect = GLOBALS.Level.Effects.Length - 1;
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
                            (GLOBALS.Level.Effects[oi], GLOBALS.Level.Effects[oi - 1]) = (GLOBALS.Level.Effects[oi - 1], GLOBALS.Level.Effects[oi]);
                        }
                    }

                    if (oi < GLOBALS.Level.Effects.Length - 1)
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
                            (GLOBALS.Level.Effects[oi], GLOBALS.Level.Effects[oi + 1]) = (GLOBALS.Level.Effects[oi + 1], GLOBALS.Level.Effects[oi]);
                        }
                    }

                }

                // Options

                if (showEffectOptions)
                {
                    unsafe
                    {
                        fixed (byte* pt = effectOptionsPanelBytes)
                        {
                            RayGui.GuiPanel(
                                new(
                                    20,
                                    GetScreenHeight() - 220,
                                    600,
                                    200
                                ),
                                (sbyte*)pt
                            );
                        }
                    }

                    var options = GLOBALS.Level.Effects.Length > 0 
                        ? GLOBALS.Level.Effects[currentAppliedEffect].Item2 
                        : [];

                    ref var currentOption = ref options[_optionsIndex];
                    
                    DrawText(currentOption.Name, 30, GetScreenHeight() - 190, 20, BLACK);

                    var commulativeWidth = 30;
                    
                    foreach (var choice in currentOption.Options)
                    {
                        var length = MeasureText(choice, 20);
                        
                        var chosen = currentOption.Choice == choice;
                        
                        if (chosen)
                        {
                            DrawRectangle(
                                commulativeWidth - 10, 
                                GetScreenHeight() - 150, 
                                length + 20, 
                                20, 
                                BLUE
                            );
                        }
                        
                        DrawText(
                            choice, 
                            commulativeWidth, 
                            GetScreenHeight() - 150, 
                            20, 
                            chosen ? WHITE : BLACK
                        );

                        commulativeWidth += length + 30;
                    }
                }


            }
            Raylib.EndDrawing();
        }
    }
}
