using System.Numerics;
using System.Text;
using ImGuiNET;
using rlImGui_cs;
using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class EffectsEditorPage(Serilog.Core.Logger logger, Texture[] textures, Camera2D? camera = null) : IPage
{
    private readonly Serilog.Core.Logger _logger = logger;
    private readonly Texture[] _textures = textures;

    private readonly EffectsShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.EffectsEditor;

    private Camera2D _camera = camera ?? new() { zoom = 1.0f };

    private bool _addNewEffectMode;

    private int _newEffectScrollIndex;
    private int _newEffectSelectedValue;

    private int _newEffectCategoryScrollIndex;
    private int _newEffectCategorySelectedValue;

    private int _currentAppliedEffect;
    private int _currentAppliedEffectPage;

    private int _newEffectCategoryItemFocus;
    private int _newEffectItemFocus;
    
    private readonly string[] _newEffectCategoryNames = [..GLOBALS.EffectCategories];
    private readonly string[][] _newEffectNames = GLOBALS.Effects.Select(c => c.Select(e => e).ToArray()).ToArray();

    private int _prevMatrixX = -1;
    private int _prevMatrixY = -1;

    private int _brushRadius = 3;

    private bool _clickTracker = false;
    private bool _brushEraseMode = false;

    private int _optionsIndex = 1;

    private readonly byte[] _addNewEffectPanelBytes = "New Effect"u8.ToArray();
    private readonly byte[] _appliedEffectsPanelBytes = "Applied Effects"u8.ToArray();
    private readonly byte[] _effectOptionsPanelBytes = "Options"u8.ToArray();

    // Paints an effect in the effects editor
    /*static void PaintEffect(double[,] matrix, (int x, int y) matrixSize, (int x, int y) center, int brushSize, double strength)
    {
        for (var y = center.y - brushSize; y < center.y + brushSize + 1; y++)
        {
            if (y < 0 || y >= matrixSize.y) continue;

            for (var x = center.x - brushSize; x < center.x + brushSize + 1; x++)
            {
                if (x < 0 || x >= matrixSize.x) continue;

                matrix[y, x] += strength;

                if (matrix[y, x] > 100) matrix[y, x] = 100;
                if (matrix[y, x] < 0) matrix[y, x] = 0;
            }
        }
    }*/
    
    // Paints an effect in the effects editor
    static void PaintEffectCircular(double[,] matrix, (int x, int y) center, int radius, double strength)
    {
        var centerV = new Vector2(center.x + 0.5f, center.y + 0.5f) * GLOBALS.PreviewScale;
        
        if (radius == 0 && center.y >= 0 && center.y < matrix.GetLength(0) && center.x >= 0 && center.x < matrix.GetLength(1))
        {
            matrix[center.y, center.x] += strength;

            if (matrix[center.y, center.x] > 100) matrix[center.y, center.x] = 100;
            if (matrix[center.y, center.x] < 0) matrix[center.y, center.x] = 0;
                    
            return;
        }
        
        for (var y = 0; y < matrix.GetLength(0); y++)
        {
            for (var x = 0; x < matrix.GetLength(1); x++)
            {
                ref var cell = ref matrix[y, x];
                
                var squareV = new Vector2(x + 0.5f, y + 0.5f) * GLOBALS.PreviewScale;

                
                if (CheckCollisionCircleRec(centerV, radius * GLOBALS.PreviewScale,
                        new(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale,
                            GLOBALS.PreviewScale)))
                {
                    var painted = cell + strength;
                    
                    if (strength > 90 || painted >= 99f)
                    {
                        cell = 100;
                        continue;
                    }
                    if (strength < -90 || painted < -90f)
                    {
                        cell = 0;
                        continue;
                    }
                    
                    var distance = RayMath.Vector2Distance(squareV, centerV) / GLOBALS.PreviewScale;

                    if (distance < 1) distance = 1;
                    
                    cell += strength / (distance);

                    if (cell > 100) cell = 100;
                    if (cell < 0) cell = 0;
                }
                
            }
        }
    }
    
    private bool _isShortcutsWinHovered;
    private bool _isShortcutsWinDragged;
    
    private bool _isOptionsWinHovered;
    private bool _isOptionsWinDragged;

    private bool _isOptionsInputActive;

    public void Draw()
    {
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera;
        
        var ctrl = IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);
        var shift = IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
        var alt = IsKeyDown(KeyboardKey.KEY_LEFT_ALT);
        
        GLOBALS.PreviousPage = 7;
        
        if (!_isOptionsInputActive)
        {
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToMainPage.Check(ctrl, shift, alt)) GLOBALS.Page = 1;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToGeometryEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 2;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToTileEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 3;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToCameraEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 4;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToLightEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 5;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToDimensionsEditor.Check(ctrl, shift, alt))
            {
                GLOBALS.ResizeFlag = true;
                GLOBALS.NewFlag = false;
                GLOBALS.Page = 6;
                _logger.Debug("go from GLOBALS.Page 7 to GLOBALS.Page 6");
            }
            // if (IsKeyReleased(KeyboardKey.KEY_SEVEN)) GLOBALS.Page = 7;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToPropsEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 8;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToSettingsPage.Check(ctrl, shift, alt)) GLOBALS.Page = 9;
        }


        // Display menu

        if (_shortcuts.NewEffect.Check(ctrl, shift, alt))
        {
            _addNewEffectMode = !_addNewEffectMode;
        }

        //


        if (_addNewEffectMode)
        {
            if (_shortcuts.NewEffectMenuCategoryNavigation.Check(ctrl, shift, alt, true))
            {
                if (IsKeyPressed(_shortcuts.MoveUpInNewEffectMenu.Key))
                {
                    _newEffectSelectedValue = 0;

                    _newEffectCategorySelectedValue = --_newEffectCategorySelectedValue;

                    if (_newEffectCategorySelectedValue < 0) _newEffectCategorySelectedValue = GLOBALS.EffectCategories.Length - 1;
                }
                else if (IsKeyPressed(_shortcuts.MoveDownInNewEffectMenu.Key))
                {
                    _newEffectSelectedValue = 0;
                    _newEffectCategorySelectedValue = ++_newEffectCategorySelectedValue % GLOBALS.EffectCategories.Length;
                }
            }
            else
            {
                if (_shortcuts.MoveUpInNewEffectMenu.Check(ctrl, shift, alt))
                {
                    _newEffectSelectedValue = --_newEffectSelectedValue;
                    if (_newEffectSelectedValue < 0) _newEffectSelectedValue = GLOBALS.Effects[_newEffectCategorySelectedValue].Length - 1;
                }
                else if (_shortcuts.MoveDownInNewEffectMenu.Check(ctrl, shift, alt))
                {
                    _newEffectSelectedValue = ++_newEffectSelectedValue % GLOBALS.Effects[_newEffectCategorySelectedValue].Length;
                }
                
            }
            
            

            if ((_shortcuts.AcceptNewEffect.Check(ctrl, shift, alt) || _shortcuts.AcceptNewEffectAlt.Check(ctrl, shift, alt)) && 
                _newEffectSelectedValue > -1 && _newEffectSelectedValue < GLOBALS.Effects[_newEffectCategorySelectedValue].Length)
            {
                GLOBALS.Level.Effects = [
                    .. GLOBALS.Level.Effects,
                    (
                        GLOBALS.Effects[_newEffectCategorySelectedValue][_newEffectSelectedValue],
                        Utils.NewEffectOptions(GLOBALS.Effects[_newEffectCategorySelectedValue][_newEffectSelectedValue]),
                        new double[GLOBALS.Level.Height, GLOBALS.Level.Width]
                    )
                ];

                _addNewEffectMode = false;
                if (_currentAppliedEffect == -1) _currentAppliedEffect = 0;
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
                        GetScreenWidth() / 2f - 390,
                        GetScreenHeight() / 2f - 265,
                        150,
                        10
                    ),
                    "Categories"
                );

                unsafe
                {
                    var panelRect = new Rectangle(
                        GetScreenWidth() / 2f - 400,
                        GetScreenHeight() / 2f - 300,
                        800,
                        600
                    );

                    fixed (byte* pt = _addNewEffectPanelBytes)
                    {
                        RayGui.GuiPanel(
                            panelRect,
                            (sbyte*)pt
                        );
                    }

                    // Close Button

                    var closeClicked = RayGui.GuiButton(new Rectangle(panelRect.X + panelRect.width - 24, panelRect.Y, 24, 24), "X");

                    if (closeClicked) _addNewEffectMode = false;

                    fixed (int* scrollIndex = &_newEffectCategoryScrollIndex)
                    {
                        fixed (int* fc = &_newEffectCategoryItemFocus)
                        {
                            var newNewEffectCategorySelectedValue = RayGui.GuiListViewEx(
                                new(
                                    GetScreenWidth() / 2f - 390,
                                    GetScreenHeight() / 2f - 250,
                                    150,
                                    540
                                ),
                                _newEffectCategoryNames,
                                _newEffectCategoryNames.Length,
                                fc,
                                scrollIndex,
                                _newEffectCategorySelectedValue
                            );

                            if (newNewEffectCategorySelectedValue != _newEffectCategorySelectedValue &&
                                newNewEffectCategorySelectedValue != -1)
                            {
                                #if DEBUG
                                _logger.Debug($"New new effect category index: {newNewEffectCategorySelectedValue}");
                                #endif
                                _newEffectCategorySelectedValue = newNewEffectCategorySelectedValue;
                            }
                        }
                    }
                }

                /*if (!_newEffectFocus) DrawRectangleLinesEx(
                    new(
                        GetScreenWidth() / 2f - 390,
                        GetScreenHeight() / 2f - 250,
                        150,
                        540
                    ),
                    2.0f,
                    new(0, 0, 255, 255)
                );*/

                unsafe
                {
                    fixed (int* scrollIndex = &_newEffectScrollIndex)
                    {
                        fixed (int* fc = &_newEffectItemFocus)
                        {
                            var newNewEffectSelectedValue = RayGui.GuiListViewEx(
                                new(
                                    GetScreenWidth() / 2f - 230,
                                    GetScreenHeight() / 2f - 250,
                                    620,
                                    540
                                ),
                                _newEffectNames[_newEffectCategorySelectedValue],
                                _newEffectNames[_newEffectCategorySelectedValue].Length,
                                fc,
                                scrollIndex,
                                _newEffectSelectedValue
                            );

                            if (newNewEffectSelectedValue == -1) {
                                GLOBALS.Level.Effects = [
                                    .. GLOBALS.Level.Effects,
                                    (
                                        GLOBALS.Effects[_newEffectCategorySelectedValue][_newEffectSelectedValue],
                                        Utils.NewEffectOptions(GLOBALS.Effects[_newEffectCategorySelectedValue][_newEffectSelectedValue]),
                                        new double[GLOBALS.Level.Height, GLOBALS.Level.Width]
                                    )
                                ];

                                _addNewEffectMode = false;
                                if (_currentAppliedEffect == -1) _currentAppliedEffect = 0;
                            }
                            else
                            {
                                _newEffectSelectedValue = newNewEffectSelectedValue;
                            }
                        }
                    }
                }


                /*if (_newEffectFocus) DrawRectangleLinesEx(
                    new(
                        GetScreenWidth() / 2f - 230,
                        GetScreenHeight() / 2f - 250,
                        620,
                        540
                    ),
                    2.0f,
                    new(0, 0, 255, 255)
                );*/
            }
            EndDrawing();
        }
        else
        {

            var effectsMouse = GetScreenToWorld2D(GetMousePosition(), _camera);

            //                        v this was done to avoid rounding errors
            var effectsMatrixY = effectsMouse.Y < 0 ? -1 : (int)effectsMouse.Y / GLOBALS.PreviewScale;
            var effectsMatrixX = effectsMouse.X < 0 ? -1 : (int)effectsMouse.X / GLOBALS.PreviewScale;

            var appliedEffectsPanelHeight = GetScreenHeight() - 200;
            const int appliedEffectRecHeight = 30;
            var appliedEffectPageSize = (appliedEffectsPanelHeight / (appliedEffectRecHeight + 30));

            // Prevent using the brush when mouse over the effects list
            var canUseBrush = !_isOptionsWinHovered && 
                              !_isOptionsWinDragged && 
                              !_isShortcutsWinHovered && 
                              !_isShortcutsWinDragged && 
                              !_addNewEffectMode && 
                              !CheckCollisionPointRec(
                GetMousePosition(),
                new(
                    GetScreenWidth() - 300,
                    100,
                    280,
                    appliedEffectsPanelHeight
                )
            ) && GLOBALS.Level.Effects.Length > 0;

            // Movement

            if (_shortcuts.DragLevel.Check(ctrl, shift, alt, true) || _shortcuts.DragLevelAlt.Check(ctrl, shift, alt, true))
            {
                var delta = GetMouseDelta();
                delta = RayMath.Vector2Scale(delta, -1.0f / _camera.zoom);
                _camera.target = RayMath.Vector2Add(_camera.target, delta);
            }

            // Brush size

            var effectsMouseWheel = GetMouseWheelMove();
            var isBrushSizeConstrained = _currentAppliedEffect >= 0 && 
                                         _currentAppliedEffect < GLOBALS.Level.Effects.Length && 
                                         Utils.IsEffectBruhConstrained(GLOBALS.Level.Effects[_currentAppliedEffect].Item1);

            if (IsKeyDown(_shortcuts.ResizeBrush.Key))
            {
                if (isBrushSizeConstrained)
                {
                    _brushRadius = 0;
                }
                else if (effectsMouseWheel != 0)
                {
                    _brushRadius += (int)effectsMouseWheel;

                    if (_brushRadius < 0) _brushRadius = 0;
                    if (_brushRadius > 10) _brushRadius = 10;
                }
            }
            else
            {
                if (effectsMouseWheel != 0)
                {
                    var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
                    _camera.offset = GetMousePosition();
                    _camera.target = mouseWorldPosition;
                    _camera.zoom += effectsMouseWheel * GLOBALS.ZoomIncrement;
                    if (_camera.zoom < GLOBALS.ZoomIncrement) _camera.zoom = GLOBALS.ZoomIncrement;
                }
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
                            effectsMatrixX != _prevMatrixX || effectsMatrixY != _prevMatrixY || !_clickTracker
                        ))
                {
                    (string, EffectOptions[], double[,]) mtx;

                    #if DEBUG
                    try
                    {
                        mtx = GLOBALS.Level.Effects[_currentAppliedEffect];
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        throw new IndexOutOfRangeException(innerException: e, message: $"Failed to fetch current applied effect from {nameof(GLOBALS.Level.Effects)} (L:{GLOBALS.Level.Effects.Length}): {nameof(_currentAppliedEffect)} ({_currentAppliedEffect}) was out of bounds");
                    }
                    #else
                    mtx = GLOBALS.Level.Effects[currentAppliedEffect];
                    #endif

                    var strength = Utils.GetEffectBrushStrength(mtx.Item1);
                    var useStrong = _shortcuts.StrongBrush.Check(ctrl, shift, alt, true);
                    
                    PaintEffectCircular(
                        mtx.Item3,
                        (effectsMatrixX, effectsMatrixY),
                        _brushRadius,
                        useStrong ? strength + 10  : strength
                    );

                    _prevMatrixX = effectsMatrixX;
                    _prevMatrixY = effectsMatrixY;
                }

                _clickTracker = true;
            }
            if ((_shortcuts.Erase.Check(ctrl, shift, alt, true) || _shortcuts.EraseAlt.Check(ctrl, shift, alt, true)) && canUseBrush)
            {
                _brushEraseMode = true;
                
                if (
                        effectsMatrixX >= 0 &&
                        effectsMatrixX < GLOBALS.Level.Width &&
                        effectsMatrixY >= 0 &&
                        effectsMatrixY < GLOBALS.Level.Height &&
                        (
                            effectsMatrixX != _prevMatrixX || effectsMatrixY != _prevMatrixY || !_clickTracker
                        ))
                {
                    (string, EffectOptions[], double[,]) mtx;

                    #if DEBUG
                    try
                    {
                        mtx = GLOBALS.Level.Effects[_currentAppliedEffect];
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        throw new IndexOutOfRangeException(innerException: e, message: $"Failed to fetch current applied effect from {nameof(GLOBALS.Level.Effects)} (L:{GLOBALS.Level.Effects.Length}): {nameof(_currentAppliedEffect)} ({_currentAppliedEffect}) was out of bounds");
                    }
                    #else
                    mtx = GLOBALS.Level.Effects[currentAppliedEffect];
                    #endif
                    
                    var strength = -Utils.GetEffectBrushStrength(mtx.Item1);
                    var useStrong = _shortcuts.StrongBrush.Check(ctrl, shift, alt, true);
                    
                    PaintEffectCircular(
                        mtx.Item3,
                        (effectsMatrixX, effectsMatrixY),
                        _brushRadius,
                        useStrong ? strength - 10 : strength
                    );

                    _prevMatrixX = effectsMatrixX;
                    _prevMatrixY = effectsMatrixY;
                }

                _clickTracker = true;
            }
            else _brushEraseMode = false;
            
            if (IsMouseButtonReleased(_shortcuts.Paint.Button) || IsKeyReleased(_shortcuts.PaintAlt.Key))
            {
                _clickTracker = false;
            }

            //

            if (GLOBALS.Level.Effects.Length > 0)
            {
                var index = _currentAppliedEffect;
                
                if (_shortcuts.ShiftAppliedEffectUp.Check(ctrl, shift, alt))
                {
                    if (index > 0)
                    {
                        (GLOBALS.Level.Effects[index], GLOBALS.Level.Effects[index - 1]) = (GLOBALS.Level.Effects[index - 1], GLOBALS.Level.Effects[index]);
                        _currentAppliedEffect--;
                    }
                }
                else if (_shortcuts.ShiftAppliedEffectDown.Check(ctrl, shift, alt))
                {
                    if (index < GLOBALS.Level.Effects.Length - 1)
                    {
                        (GLOBALS.Level.Effects[index], GLOBALS.Level.Effects[index + 1]) = (GLOBALS.Level.Effects[index + 1], GLOBALS.Level.Effects[index]);
                        _currentAppliedEffect++;
                    }
                }
                
                // Cycle options

                if (_shortcuts.CycleEffectOptionsUp.Check(ctrl, shift, alt))
                {
                    _optionsIndex--;
                    if (_optionsIndex < 2) _optionsIndex = GLOBALS.Level.Effects[_currentAppliedEffect].Item2.Length - 1;
                }
                else if (_shortcuts.CycleEffectOptionsDown.Check(ctrl, shift, alt))
                {
                    if (GLOBALS.Level.Effects[_currentAppliedEffect].Item2.Length > 0) {

                        _optionsIndex = ++_optionsIndex % GLOBALS.Level.Effects[_currentAppliedEffect].Item2.Length;
                        if (_optionsIndex == 0) _optionsIndex = 1;
                    }
                }
                else if (_shortcuts.CycleEffectOptionChoicesRight.Check(ctrl, shift, alt))
                {
                    var option = GLOBALS.Level.Effects[_currentAppliedEffect].Item2[_optionsIndex];
                    if (option.Options.Length > 0) {
                        var choiceIndex = Array.FindIndex(option.Options, op => op == option.Choice);
                        choiceIndex = ++choiceIndex % option.Options.Length;
                        option.Choice = option.Options[choiceIndex];
                    }
                }
                else if (_shortcuts.CycleEffectOptionChoicesLeft.Check(ctrl, shift, alt))
                {
                    var option = GLOBALS.Level.Effects[_currentAppliedEffect].Item2[_optionsIndex];
                    var choiceIndex = Array.FindIndex(option.Options, op => op == option.Choice);
                    choiceIndex--;
                    if (choiceIndex < 0) choiceIndex = option.Options.Length - 1;

                    if (choiceIndex >= 0 && choiceIndex < option.Options.Length) option.Choice = option.Options[choiceIndex];
                }
                
                // Cycle applied effects

                if (_shortcuts.CycleAppliedEffectUp.Check(ctrl, shift, alt))
                {
                    _currentAppliedEffect--;

                    if (_currentAppliedEffect < 0) _currentAppliedEffect = GLOBALS.Level.Effects.Length - 1;

                    _currentAppliedEffectPage = _currentAppliedEffect / appliedEffectPageSize;

                    _optionsIndex = 1;
                }
                else if (_shortcuts.CycleAppliedEffectDown.Check(ctrl, shift, alt))
                {
                    _currentAppliedEffect = ++_currentAppliedEffect % GLOBALS.Level.Effects.Length;

                    _currentAppliedEffectPage = _currentAppliedEffect / appliedEffectPageSize;
                    
                    _optionsIndex = 1;
                }

                // Delete effect
                if (_shortcuts.DeleteAppliedEffect.Check(ctrl, shift, alt))
                {
                    GLOBALS.Level.Effects = GLOBALS.Level.Effects.Where((e, i) => i != _currentAppliedEffect).ToArray();
                    _currentAppliedEffect--;
                    if (_currentAppliedEffect < 0) _currentAppliedEffect = GLOBALS.Level.Effects.Length - 1;
                }
            }

            BeginDrawing();
            {

                ClearBackground(new Color(0, 0, 0, 255));

                BeginMode2D(_camera);
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
                        _currentAppliedEffect >= 0 &&
                        _currentAppliedEffect < GLOBALS.Level.Effects.Length)
                    {

                        DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.PreviewScale, GLOBALS.Level.Height * GLOBALS.PreviewScale, new(215, 66, 245, 100));

                        for (int y = 0; y < GLOBALS.Level.Height; y++)
                        {
                            for (int x = 0; x < GLOBALS.Level.Width; x++)
                            {
                                DrawRectangle(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale, GLOBALS.PreviewScale, GLOBALS.PreviewScale, new(0, 255, 0, (int)GLOBALS.Level.Effects[_currentAppliedEffect].Item3[y, x] * 255 / 100));
                                
                                // Brush
                                
                                var effAreaRect = new Rectangle(x * GLOBALS.PreviewScale, y * GLOBALS.PreviewScale,
                                    GLOBALS.PreviewScale, GLOBALS.PreviewScale);
                                if (_brushRadius > 0 && CheckCollisionCircleRec(
                                        new Vector2(effectsMatrixX+0.5f, effectsMatrixY+0.5f) * GLOBALS.PreviewScale,
                                        _brushRadius*GLOBALS.PreviewScale, effAreaRect))
                                {
                                    DrawRectangleRec(effAreaRect, WHITE with { a = 50 });
                                }
                                
                                if (effectsMatrixX == x && effectsMatrixY == y)
                                {
                                    DrawCircleLines(
                                        (int) ((x + 0.5f) * GLOBALS.PreviewScale), 
                                        (int) ((y + 0.5f) * GLOBALS.PreviewScale),
                                        (_brushRadius+1)*GLOBALS.PreviewScale,
                                        _brushEraseMode ? RED : WHITE);
                                        
                                    DrawRectangleLinesEx(
                                        new Rectangle(
                                            x * GLOBALS.PreviewScale,
                                            y * GLOBALS.PreviewScale,
                                            GLOBALS.PreviewScale,
                                            GLOBALS.PreviewScale
                                        ),
                                        2.0f,
                                        _brushEraseMode ? RED : WHITE
                                    );
                                }
                            }
                        }
                    }
                }
                EndMode2D();

                // UI

                unsafe
                {
                    fixed (byte* pt = _appliedEffectsPanelBytes)
                    {
                        RayGui.GuiPanel(
                            new Rectangle(
                                GetScreenWidth() - 300,
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
                    if (_currentAppliedEffectPage < (GLOBALS.Level.Effects.Length / appliedEffectPageSize))
                    {
                        var appliedEffectsPageDownPressed = RayGui.GuiButton(
                            new Rectangle(
                                GetScreenWidth() - 290,
                                GetScreenHeight() - 140,
                                130,
                                32
                            ),

                            "Page Down"
                        );

                        if (appliedEffectsPageDownPressed)
                        {
                            _currentAppliedEffectPage++;
                            _currentAppliedEffect = appliedEffectPageSize * _currentAppliedEffectPage;
                        }
                    }

                    if (_currentAppliedEffectPage > 0)
                    {
                        var appliedEffectsPageUpPressed = RayGui.GuiButton(
                            new Rectangle(
                                GetScreenWidth() - 155,
                                GetScreenHeight() - 140,
                                130,
                                32
                            ),

                            "Page Up"
                        );

                        if (appliedEffectsPageUpPressed)
                        {
                            _currentAppliedEffectPage--;
                            _currentAppliedEffect = appliedEffectPageSize * (_currentAppliedEffectPage + 1) - 1;
                        }
                    }
                }


                // Applied effects

                var addEffectRect = new Rectangle(GetScreenWidth() - 290, 130, 35, 35);
                var newEffectHovered = CheckCollisionPointRec(GetMousePosition(), addEffectRect);

                if (newEffectHovered) {
                    DrawRectangleRec(addEffectRect, BLUE with { a = 150 });

                    if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) {
                        _addNewEffectMode = true;
                    }
                }
                
                DrawTexturePro(
                    _textures[0], 
                    new Rectangle(0, 0, _textures[0].width, _textures[0].height), 
                    addEffectRect,
                    new Vector2(0, 0),
                    0,
                    BLACK
                );

                // i is index relative to the GLOBALS.Page; oi is index relative to the whole list
                foreach (var (i, (oi, e)) in GLOBALS.Level.Effects
                             .Select((value, i) => (i, value))
                             .Skip(appliedEffectPageSize * _currentAppliedEffectPage)
                             .Take(appliedEffectPageSize)
                             .Select((value, i) => (i, value)))
                {
                    var appliedEffectRect = new Rectangle(GetScreenWidth() - 290, 130 + (35 * i) + 50, 260, appliedEffectRecHeight);
                    var appliedEffectHovered = CheckCollisionPointRec(GetMousePosition(), appliedEffectRect);
                    if (appliedEffectHovered && IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                    {
                        _currentAppliedEffect = oi;
                        _currentAppliedEffectPage = _currentAppliedEffect / appliedEffectPageSize;
                    }
                    
                    DrawRectangleLines(
                        GetScreenWidth() - 290,
                        130 + (35 * i) + 50,
                        260,
                        appliedEffectRecHeight,
                        appliedEffectHovered ? BLUE with { a = 100 } : GRAY
                    );
                    
                    if (oi == _currentAppliedEffect) DrawRectangleLinesEx(
                        new Rectangle(
                            GetScreenWidth() - 290,
                            130 + (35 * i) + 50,
                            260,
                            appliedEffectRecHeight
                        ),
                        2.0f,
                        BLUE
                    );
                    
                    if (GLOBALS.Font is null) {
                        DrawText(
                            e.Item1,
                            GetScreenWidth() - 280,
                            138 + (35 * i) + 50,
                            14,
                            new(0, 0, 0, 255)
                        );
                    } else {
                        DrawTextEx(
                            GLOBALS.Font.Value,
                            e.Item1,
                            new(GetScreenWidth() - 280, 133 + (35 * i) + 50),
                            25,
                            1,
                            BLACK
                        );
                    }

                    
                    // Delete Button

                    var deleteRect = new Rectangle(
                        GetScreenWidth() - 67, 132 + (35 * i) + 50, 26, 26);
                    var deleteHovered = CheckCollisionPointRec(GetMousePosition(), deleteRect);
                    
                    if (deleteHovered)
                    {
                        DrawRectangleRec(deleteRect, BLUE);
                        
                        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                        {
                            GLOBALS.Level.Effects = GLOBALS.Level.Effects.Where((_, ei) => ei != oi).ToArray();
                            if (_currentAppliedEffect < 0) _currentAppliedEffect = 0;
                            else if (_currentAppliedEffect >= GLOBALS.Level.Effects.Length - 1)
                                _currentAppliedEffect = GLOBALS.Level.Effects.Length - 1;
                        }
                    }
                    
                    DrawTexturePro(
                        _textures[3],
                        new Rectangle(0, 0, _textures[3].width, _textures[3].height),
                        deleteRect,
                        new(0, 0),
                        0,
                        deleteHovered ? WHITE : BLACK
                    );
                    
                    // Shift Up
                    if (oi > 0)
                    {
                        var shiftUpRect = new Rectangle(
                            GetScreenWidth() - 105,
                            132 + (35 * i) + 50,
                            26,
                            26
                        );
                        
                        var shiftUpHovered = CheckCollisionPointRec(GetMousePosition(), shiftUpRect);
                        
                        DrawTexturePro(
                            _textures[1],
                            new(0, 0, _textures[1].width, _textures[1].height),
                            shiftUpRect,
                            new(0, 0),
                            0,
                            shiftUpHovered ? WHITE : BLACK
                        );

                        if (shiftUpHovered)
                        {
                            DrawRectangleRec(shiftUpRect, BLUE);
                            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                            {
                                (GLOBALS.Level.Effects[oi], GLOBALS.Level.Effects[oi - 1]) = (GLOBALS.Level.Effects[oi - 1], GLOBALS.Level.Effects[oi]);
                            }
                        }
                    }

                    // Shift Down
                    if (oi < GLOBALS.Level.Effects.Length - 1)
                    {
                        var shiftDownRect = new Rectangle(
                            GetScreenWidth() - 143,
                            132 + (35 * i) + 50,
                            37,
                            26
                        );

                        var shiftDownHovered = CheckCollisionPointRec(GetMousePosition(), shiftDownRect);
                        
                        DrawTexturePro(
                            _textures[2], 
                            new Rectangle(0, 0, _textures[2].width, _textures[2].height),
                            shiftDownRect,
                            new(0, 0),
                            0,
                            shiftDownHovered ? WHITE : BLACK
                        );

                        if (shiftDownHovered)
                        {
                            DrawRectangleRec(shiftDownRect, BLUE);
                            
                            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                            {
                                (GLOBALS.Level.Effects[oi], GLOBALS.Level.Effects[oi + 1]) = (GLOBALS.Level.Effects[oi + 1], GLOBALS.Level.Effects[oi]);
                            }
                        }
                    }
                }

                // Options
                
                var options = GLOBALS.Level.Effects.Length > 0 && _currentAppliedEffect != -1
                    ? GLOBALS.Level.Effects[_currentAppliedEffect].Item2 
                    : [];
                
                rlImGui.Begin();

                if (ImGui.Begin("Options"))
                {
                    var pos = ImGui.GetWindowPos();
                    var winSpace = ImGui.GetWindowSize();
                    
                    if (CheckCollisionPointRec(GetMousePosition(), new(pos.X - 5, pos.Y, winSpace.X + 10, winSpace.Y)))
                    {
                        _isOptionsWinHovered = true;

                        if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) _isOptionsWinDragged = true;
                    }
                    else
                    {
                        _isOptionsWinHovered = false;
                    }
                    
                    if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && _isOptionsWinDragged) _isOptionsWinDragged = false;
                    
                    var halfWidth = ImGui.GetContentRegionAvail().X / 2f - ImGui.GetStyle().ItemSpacing.X / 2f;
                    var boxHeight = ImGui.GetContentRegionAvail().Y;
                    
                    if (options is not [])
                    {
                        if (ImGui.BeginListBox("##EffectOptionTitles", new Vector2(halfWidth, boxHeight)))
                        {
                            for (var optionIndex = 0; optionIndex < options.Length; optionIndex++)
                            {
                                if (options[optionIndex].Name == "Delete/Move") continue;
                                
                                var selected = ImGui.Selectable(options[optionIndex].Name, optionIndex == _optionsIndex);
                                if (selected) _optionsIndex = optionIndex;
                            }
                            ImGui.EndListBox();
                        }
                        
                        ImGui.SameLine();
                        
                        ref var currentOption = ref options[_optionsIndex];
                        
                        if (currentOption.Options is [] && currentOption.Choice is int number)
                        {
                            ImGui.SetNextItemWidth(halfWidth);
                            var active = ImGui.InputInt(string.Empty, ref number);
                            
                            _isOptionsInputActive = active;

                            if (number != (int)currentOption.Choice) currentOption.Choice = number;
                        }
                        else if (ImGui.BeginListBox("##EffectOptionList", new Vector2(halfWidth, boxHeight)))
                        {
                            foreach (var t in currentOption.Options)
                            {
                                var selected = ImGui.Selectable(t, t == currentOption.Choice);

                                if (selected) currentOption.Choice = t;
                            }

                            ImGui.EndListBox();
                        }
                    }
                    else ImGui.Text("No Options");
                    
                    ImGui.End();
                }
                
                rlImGui.End();

                /*if (_showEffectOptions)
                {
                    
                    /*unsafe
                    {
                        fixed (byte* pt = _effectOptionsPanelBytes)
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
                    #1#
                    
                    
                }*/

                // Shortcuts window
                if (GLOBALS.Settings.GeneralSettings.ShortcutWindow)
                {
                    rlImGui.Begin();
                    var shortcutWindowRect = Printers.ImGui.ShortcutsWindow(GLOBALS.Settings.Shortcuts.EffectsEditor);

                    _isShortcutsWinHovered = CheckCollisionPointRec(
                        GetMousePosition(), 
                        shortcutWindowRect with
                        {
                            X = shortcutWindowRect.X - 5, width = shortcutWindowRect.width + 10
                        }
                    );

                    if (_isShortcutsWinHovered && IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                    {
                        _isShortcutsWinDragged = true;
                    }
                    else if (_isShortcutsWinDragged && IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
                    {
                        _isShortcutsWinDragged = false;
                    }


                    rlImGui.End();
                }

            }
            EndDrawing();
            
            if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera;
        }
    }
}
