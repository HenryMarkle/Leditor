using System.Numerics;
using System.Text;
using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class EffectsEditorPage(Serilog.Core.Logger logger) : IPage
{
    readonly Serilog.Core.Logger logger = logger;

    private readonly EffectsShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.EffectsEditor;

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
        var ctrl = IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);
        var shift = IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
        var alt = IsKeyDown(KeyboardKey.KEY_LEFT_ALT);
        
        GLOBALS.PreviousPage = 7;

        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToMainPage.Check(ctrl, shift, alt)) GLOBALS.Page = 1;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToGeometryEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 2;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToTileEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 3;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToCameraEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 4;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToLightEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 5;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToDimensionsEditor.Check(ctrl, shift, alt))
        {
            GLOBALS.ResizeFlag = true;
            GLOBALS.Page = 6;
            logger.Debug("go from GLOBALS.Page 7 to GLOBALS.Page 6");
        }
        // if (Raylib.IsKeyReleased(KeyboardKey.KEY_SEVEN)) GLOBALS.Page = 7;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToPropsEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 8;
        if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToSettingsPage.Check(ctrl, shift, alt)) GLOBALS.Page = 9;

        // Display menu

        if (_shortcuts.NewEffect.Check(ctrl, shift, alt)) addNewEffectMode = !addNewEffectMode;

        //


        if (addNewEffectMode)
        {
            if (_shortcuts.MoveUpInNewEffectMenu.Check(ctrl, shift, alt))
            {
                newEffectSelectedValue = --newEffectSelectedValue;
                if (newEffectSelectedValue < 0) newEffectSelectedValue = GLOBALS.Effects[newEffectCategorySelectedValue].Length - 1;
            }
            else if (_shortcuts.MoveDownInNewEffectMenu.Check(ctrl, shift, alt))
            {
                newEffectSelectedValue = ++newEffectSelectedValue % GLOBALS.Effects[newEffectCategorySelectedValue].Length;
            }
            else if (_shortcuts.MoveUpInNewEffectCategoryMenu.Check(ctrl, shift, alt))
            {
                newEffectSelectedValue = 0;

                newEffectCategorySelectedValue = --newEffectCategorySelectedValue;

                if (newEffectCategorySelectedValue < 0) newEffectCategorySelectedValue = GLOBALS.EffectCategories.Length - 1;
            }
            else if (_shortcuts.MoveDownInNewEffectCategoryMenu.Check(ctrl, shift, alt))
            {
                newEffectSelectedValue = 0;
                newEffectCategorySelectedValue = ++newEffectCategorySelectedValue % GLOBALS.EffectCategories.Length;
            }

            /*if (Raylib.IsKeyPressed(KeyboardKey.KEY_D))
            {
                newEffectFocus = true;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_A))
            {
                newEffectFocus = false;
            }*/

            if ((_shortcuts.AcceptNewEffect.Check(ctrl, shift, alt) || _shortcuts.AcceptNewEffectAlt.Check(ctrl, shift, alt)) && newEffectSelectedValue > 0)
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

            BeginDrawing();
            {
                DrawRectangle(
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
                        var newNewEffectCategorySelectedValue = RayGui.GuiListView(
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

                        if (newNewEffectCategorySelectedValue != newEffectCategorySelectedValue &&
                            newNewEffectCategorySelectedValue != -1)
                        {
                            #if DEBUG
                            logger.Debug($"New new effect category index: {newNewEffectCategorySelectedValue}");
                            #endif
                            newEffectCategorySelectedValue = newNewEffectCategorySelectedValue;
                        }
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

            var effectsMouse = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);

            //                        v this was done to avoid rounding errors
            var effectsMatrixY = effectsMouse.Y < 0 ? -1 : (int)effectsMouse.Y / GLOBALS.PreviewScale;
            var effectsMatrixX = effectsMouse.X < 0 ? -1 : (int)effectsMouse.X / GLOBALS.PreviewScale;


            var appliedEffectsPanelHeight = Raylib.GetScreenHeight() - 200;
            const int appliedEffectRecHeight = 30;
            var appliedEffectPageSize = appliedEffectsPanelHeight / (appliedEffectRecHeight + 20);

            // Prevent using the brush when mouse over the effects list
            bool canUseBrush = !CheckCollisionPointRec(
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
            ) && GLOBALS.Level.Effects.Length > 0;

            // Movement

            if (_shortcuts.DragLevel.Check(ctrl, shift, alt, true) || _shortcuts.DragLevelAlt.Check(ctrl, shift, alt, true))
            {
                Vector2 delta = Raylib.GetMouseDelta();
                delta = RayMath.Vector2Scale(delta, -1.0f / camera.zoom);
                camera.target = RayMath.Vector2Add(camera.target, delta);
            }

            // Brush size

            var effectslMouseWheel = GetMouseWheelMove();

            if (effectslMouseWheel != 0)
            {
                brushRadius += (int)effectslMouseWheel;

                if (brushRadius < 0) brushRadius = 0;
                if (brushRadius > 10) brushRadius = 10;
            }

            // Use brush

            if ((_shortcuts.Paint.Check(ctrl, shift, alt, true) || _shortcuts.PaintAlt.Check(ctrl, shift, alt, true)) && canUseBrush)
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
                    double[,] mtx;

                    #if DEBUG
                    try
                    {
                        mtx = GLOBALS.Level.Effects[currentAppliedEffect].Item3;
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        throw new IndexOutOfRangeException(innerException: e, message: $"Failed to fetch current applied effect from {nameof(GLOBALS.Level.Effects)} (L:{GLOBALS.Level.Effects.Length}): {nameof(currentAppliedEffect)} ({currentAppliedEffect}) was out of bounds");
                    }
                    #else
                    mtx = GLOBALS.Level.Effects[currentAppliedEffect].Item3;
                    #endif
                    
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

            if (IsMouseButtonReleased(_shortcuts.Paint.Button) || IsKeyReleased(_shortcuts.PaintAlt.Key))
            {
                clickTracker = false;
            }

            //

            if (GLOBALS.Level.Effects.Length > 0)
            {
                var index = currentAppliedEffect;
                
                if (_shortcuts.ShiftAppliedEffectUp.Check(ctrl, shift, alt))
                {
                    if (index > 0)
                    {
                        (GLOBALS.Level.Effects[index], GLOBALS.Level.Effects[index - 1]) = (GLOBALS.Level.Effects[index - 1], GLOBALS.Level.Effects[index]);
                        currentAppliedEffect--;
                    }
                }
                else if (_shortcuts.ShiftAppliedEffectDown.Check(ctrl, shift, alt))
                {
                    if (index < GLOBALS.Level.Effects.Length - 1)
                    {
                        (GLOBALS.Level.Effects[index], GLOBALS.Level.Effects[index + 1]) = (GLOBALS.Level.Effects[index + 1], GLOBALS.Level.Effects[index]);
                        currentAppliedEffect++;
                    }
                }
                
                // Cycle options

                if (_shortcuts.CycleEffectOptionsUp.Check(ctrl, shift, alt))
                {
                    _optionsIndex--;
                    if (_optionsIndex < 2) _optionsIndex = GLOBALS.Level.Effects[currentAppliedEffect].Item2.Length - 1;
                }
                else if (_shortcuts.CycleEffectOptionsDown.Check(ctrl, shift, alt))
                {
                    _optionsIndex = ++_optionsIndex % GLOBALS.Level.Effects[currentAppliedEffect].Item2.Length;
                    if (_optionsIndex == 0) _optionsIndex = 1;
                }
                else if (_shortcuts.CycleEffectOptionChoicesRight.Check(ctrl, shift, alt))
                {
                    var option = GLOBALS.Level.Effects[currentAppliedEffect].Item2[_optionsIndex];
                    var choiceIndex = Array.FindIndex(option.Options, op => op == option.Choice);
                    choiceIndex = ++choiceIndex % option.Options.Length;
                    option.Choice = option.Options[choiceIndex];
                }
                else if (_shortcuts.CycleEffectOptionChoicesLeft.Check(ctrl, shift, alt))
                {
                    var option = GLOBALS.Level.Effects[currentAppliedEffect].Item2[_optionsIndex];
                    var choiceIndex = Array.FindIndex(option.Options, op => op == option.Choice);
                    choiceIndex--;
                    if (choiceIndex < 0) choiceIndex = option.Options.Length - 1;
                    option.Choice = option.Options[choiceIndex];
                }
                
                // Cycle applied effects

                if (_shortcuts.CycleAppliedEffectUp.Check(ctrl, shift, alt))
                {
                    currentAppliedEffect--;

                    if (currentAppliedEffect < 0) currentAppliedEffect = GLOBALS.Level.Effects.Length - 1;

                    currentAppliedEffectPage = currentAppliedEffect / appliedEffectPageSize;

                    _optionsIndex = 1;
                }
                else if (_shortcuts.CycleAppliedEffectDown.Check(ctrl, shift, alt))
                {
                    currentAppliedEffect = ++currentAppliedEffect % GLOBALS.Level.Effects.Length;

                    currentAppliedEffectPage = currentAppliedEffect / appliedEffectPageSize;
                    
                    _optionsIndex = 1;
                }

                if (_shortcuts.ToggleOptionsVisibility.Check(ctrl, shift, alt)) showEffectOptions = !showEffectOptions;
                if (_shortcuts.ToggleBrushEraseMode.Check(ctrl, shift, alt)) brushEraseMode = !brushEraseMode;


                // Delete effect
                if (_shortcuts.DeleteAppliedEffect.Check(ctrl, shift, alt))
                {
                    GLOBALS.Level.Effects = GLOBALS.Level.Effects.Where((e, i) => i != currentAppliedEffect).ToArray();
                    currentAppliedEffect--;
                    if (currentAppliedEffect < 0) currentAppliedEffect = GLOBALS.Level.Effects.Length - 1;
                }
            }

            BeginDrawing();
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

                    if (options.Length > 0)
                    {
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


            }
            EndDrawing();
        }
    }
}
