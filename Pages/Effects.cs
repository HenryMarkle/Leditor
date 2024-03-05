﻿using System.Numerics;
using System.Text;
using ImGuiNET;
using rlImGui_cs;
using static Raylib_CsLo.Raylib;

namespace Leditor;

internal class EffectsEditorPage(Serilog.Core.Logger logger, Camera2D? camera = null) : IPage
{
    private readonly Serilog.Core.Logger _logger = logger;

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

    private bool _clickTracker;
    private bool _brushEraseMode;

    private int _optionsIndex = 1;

    private readonly byte[] _addNewEffectPanelBytes = "New Effect"u8.ToArray();
    private readonly byte[] _appliedEffectsPanelBytes = "Applied Effects"u8.ToArray();
    
    // Paints an effect in the effects editor
    static void PaintEffectCircular(double[,] matrix, (int x, int y) center, int radius, double strength)
    {
        var centerV = new Vector2(center.x + 0.5f, center.y + 0.5f) * GLOBALS.Scale;
        
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
                
                var squareV = new Vector2(x + 0.5f, y + 0.5f) * GLOBALS.Scale;

                
                if (CheckCollisionCircleRec(centerV, radius * GLOBALS.Scale,
                        new(x * GLOBALS.Scale, y * GLOBALS.Scale, GLOBALS.Scale,
                            GLOBALS.Scale)))
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
                    
                    var distance = RayMath.Vector2Distance(squareV, centerV) / GLOBALS.Scale;

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
    
    private bool _isEffectsWinHovered;
    private bool _isEffectsWinDragged;
    
    private bool _isNavigationWinHovered;
    private bool _isNavigationWinDragged;
    
    private bool _isSettingsWinHovered;
    private bool _isSettingsWinDragged;

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
                    GetScreenWidth(),
                    GetScreenHeight(),
                    new Color(0, 0, 0, 90)
                );

                // RayGui.GuiLine(
                //     new Rectangle(
                //         GetScreenWidth() / 2f - 390,
                //         GetScreenHeight() / 2f - 265,
                //         150,
                //         10
                //     ),
                //     "Categories"
                // );

                // unsafe
                // {
                //     var panelRect = new Rectangle(
                //         GetScreenWidth() / 2f - 400,
                //         GetScreenHeight() / 2f - 300,
                //         800,
                //         600
                //     );
                //
                //     fixed (byte* pt = _addNewEffectPanelBytes)
                //     {
                //         RayGui.GuiPanel(
                //             panelRect,
                //             (sbyte*)pt
                //         );
                //     }
                //
                //     // Close Button
                //
                //     var closeClicked = RayGui.GuiButton(new Rectangle(panelRect.X + panelRect.width - 24, panelRect.Y, 24, 24), "X");
                //
                //     if (closeClicked) _addNewEffectMode = false;
                //
                //     fixed (int* scrollIndex = &_newEffectCategoryScrollIndex)
                //     {
                //         fixed (int* fc = &_newEffectCategoryItemFocus)
                //         {
                //             var newNewEffectCategorySelectedValue = RayGui.GuiListViewEx(
                //                 new(
                //                     GetScreenWidth() / 2f - 390,
                //                     GetScreenHeight() / 2f - 250,
                //                     150,
                //                     540
                //                 ),
                //                 _newEffectCategoryNames,
                //                 _newEffectCategoryNames.Length,
                //                 fc,
                //                 scrollIndex,
                //                 _newEffectCategorySelectedValue
                //             );
                //
                //             if (newNewEffectCategorySelectedValue != _newEffectCategorySelectedValue &&
                //                 newNewEffectCategorySelectedValue != -1)
                //             {
                //                 #if DEBUG
                //                 _logger.Debug($"New new effect category index: {newNewEffectCategorySelectedValue}");
                //                 #endif
                //                 _newEffectCategorySelectedValue = newNewEffectCategorySelectedValue;
                //             }
                //         }
                //     }
                // }
                //
                // unsafe
                // {
                //     fixed (int* scrollIndex = &_newEffectScrollIndex)
                //     {
                //         fixed (int* fc = &_newEffectItemFocus)
                //         {
                //             var newNewEffectSelectedValue = RayGui.GuiListViewEx(
                //                 new(
                //                     GetScreenWidth() / 2f - 230,
                //                     GetScreenHeight() / 2f - 250,
                //                     620,
                //                     540
                //                 ),
                //                 _newEffectNames[_newEffectCategorySelectedValue],
                //                 _newEffectNames[_newEffectCategorySelectedValue].Length,
                //                 fc,
                //                 scrollIndex,
                //                 _newEffectSelectedValue
                //             );
                //
                //             if (newNewEffectSelectedValue == -1) {
                //                 GLOBALS.Level.Effects = [
                //                     .. GLOBALS.Level.Effects,
                //                     (
                //                         GLOBALS.Effects[_newEffectCategorySelectedValue][_newEffectSelectedValue],
                //                         Utils.NewEffectOptions(GLOBALS.Effects[_newEffectCategorySelectedValue][_newEffectSelectedValue]),
                //                         new double[GLOBALS.Level.Height, GLOBALS.Level.Width]
                //                     )
                //                 ];
                //
                //                 _addNewEffectMode = false;
                //                 if (_currentAppliedEffect == -1) _currentAppliedEffect = 0;
                //             }
                //             else
                //             {
                //                 _newEffectSelectedValue = newNewEffectSelectedValue;
                //             }
                //         }
                //     }
                // }

                // ImGui
                
                rlImGui.Begin();
                if (ImGui.Begin("New Effect##NewEffectMenu", ImGuiWindowFlags.NoCollapse))
                {
                    var availableSpace = ImGui.GetContentRegionAvail();
                    
                    ImGui.LabelText("##NewEffectCategories", "Categories");
                    
                    ImGui.SameLine();
                    
                    ImGui.LabelText("##NewEffects", "Effects");
                    
                    if (ImGui.BeginListBox("##NewEffectCategories", availableSpace with { X = 250, Y = availableSpace.Y - 60 }))
                    {
                        for (var index = 0; index < _newEffectCategoryNames.Length; index++)
                        {
                            var selected = ImGui.Selectable(_newEffectCategoryNames[index], index == _newEffectCategorySelectedValue);
                            if (selected)
                            {
                                _newEffectCategorySelectedValue = index;
                                Utils.Restrict(ref _newEffectSelectedValue, 0, _newEffectNames[_newEffectCategorySelectedValue].Length-1);
                            }
                        }
                        
                        ImGui.EndListBox();
                    }
                    
                    ImGui.SameLine();
                    
                    if (ImGui.BeginListBox("##NewEffects", availableSpace with { Y = availableSpace.Y - 60 }))
                    {
                        for (var index = 0; index < _newEffectNames[_newEffectCategorySelectedValue].Length; index++)
                        {
                            var selected =
                                ImGui.Selectable(
                                    _newEffectNames[_newEffectCategorySelectedValue][index],index == _newEffectSelectedValue);

                            if (selected) _newEffectSelectedValue = index;
                        }
                        
                        ImGui.EndListBox();
                    }
                    
                    ImGui.Dummy(new (availableSpace.X - 120, 20));
                    ImGui.SameLine();
                    var cancelSelected = ImGui.Button("Cancel");
                    ImGui.SameLine();
                    var selectSelected = ImGui.Button("Select");

                    if (cancelSelected)
                    {
                        _addNewEffectMode = false;
                    }

                    if (selectSelected)
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
                    
                    ImGui.End();
                }
                rlImGui.End();
            }
            EndDrawing();
        }
        else
        {

            var effectsMouse = GetScreenToWorld2D(GetMousePosition(), _camera);

            //                        v this was done to avoid rounding errors
            var effectsMatrixY = effectsMouse.Y < 0 ? -1 : (int)effectsMouse.Y / GLOBALS.Scale;
            var effectsMatrixX = effectsMouse.X < 0 ? -1 : (int)effectsMouse.X / GLOBALS.Scale;

            var appliedEffectsPanelHeight = GetScreenHeight() - 200;
            const int appliedEffectRecHeight = 30;
            var appliedEffectPageSize = (appliedEffectsPanelHeight / (appliedEffectRecHeight + 30));

            // Prevent using the brush when mouse over the effects list
            var canUseBrush = !_isSettingsWinHovered && 
                              !_isSettingsWinDragged && 
                              !_isOptionsWinHovered && 
                              !_isOptionsWinDragged && 
                              !_isShortcutsWinHovered && 
                              !_isShortcutsWinDragged && 
                              !_isNavigationWinHovered &&
                              !_isNavigationWinDragged &&
                              !_isEffectsWinHovered &&
                              !_isEffectsWinDragged &&
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
                if (effectsMouseWheel != 0 && canUseBrush)
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
                            (GLOBALS.Level.Width * GLOBALS.Scale) + 4,
                            (GLOBALS.Level.Height * GLOBALS.Scale) + 4
                        ),
                        2f,
                        WHITE
                    );
                    
                    DrawRectangle(
                        0, 
                        0, 
                        GLOBALS.Level.Width * GLOBALS.Scale, 
                        GLOBALS.Level.Height * GLOBALS.Scale, 
                        GLOBALS.Settings.GeneralSettings.DarkTheme
                            ? new Color(50, 50, 50, 255)
                            : WHITE);

                    Printers.DrawGeoLayer(2, GLOBALS.Scale, false, GLOBALS.Settings.GeneralSettings.DarkTheme ? new Color(150, 150, 150, 255) : BLACK with { a = 150 }, GLOBALS.LayerStackableFilter);
                    Printers.DrawTileLayer(2, GLOBALS.Scale, false, true, true);
                    
                    Printers.DrawGeoLayer(1, GLOBALS.Scale, false, GLOBALS.Settings.GeneralSettings.DarkTheme ? new Color(100, 100, 100, 255) : BLACK with { a = 150 }, GLOBALS.LayerStackableFilter);
                    Printers.DrawTileLayer(1, GLOBALS.Scale, false, true, true);
                    
                    if (!GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1)
                    {
                        DrawRectangle(
                            (-1) * GLOBALS.Scale,
                            (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel) * GLOBALS.Scale,
                            (GLOBALS.Level.Width + 2) * GLOBALS.Scale,
                            GLOBALS.Level.WaterLevel * GLOBALS.Scale,
                            GLOBALS.Settings.GeneralSettings.DarkTheme 
                                ? GLOBALS.DarkThemeWaterColor 
                                : GLOBALS.LightThemeWaterColor
                        );
                    }
                    
                    Printers.DrawGeoLayer(0, GLOBALS.Scale, false, BLACK with { a = 255 });
                    Printers.DrawTileLayer(0, GLOBALS.Scale, false, true, true);
                    
                    if (GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1)
                    {
                        DrawRectangle(
                            (-1) * GLOBALS.Scale,
                            (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel) * GLOBALS.Scale,
                            (GLOBALS.Level.Width + 2) * GLOBALS.Scale,
                            GLOBALS.Level.WaterLevel * GLOBALS.Scale,
                            GLOBALS.Settings.GeneralSettings.DarkTheme 
                                ? GLOBALS.DarkThemeWaterColor 
                                : GLOBALS.LightThemeWaterColor
                        );
                    }

                    // Effect matrix

                    if (GLOBALS.Level.Effects.Length > 0 &&
                        _currentAppliedEffect >= 0 &&
                        _currentAppliedEffect < GLOBALS.Level.Effects.Length)
                    {

                        if (!GLOBALS.Settings.GeneralSettings.DarkTheme) DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.Scale, GLOBALS.Level.Height * GLOBALS.Scale, new(215, 66, 245, 100));

                        var brushColor = GLOBALS.Settings.GeneralSettings.DarkTheme
                            ? GLOBALS.Settings.EffectsSettings.EffectColorDark
                            : GLOBALS.Settings.EffectsSettings.EffectColorLight;
                        
                        for (var y = 0; y < GLOBALS.Level.Height; y++)
                        {
                            for (var x = 0; x < GLOBALS.Level.Width; x++)
                            {
                                DrawRectangle(
                                    x * GLOBALS.Scale, 
                                    y * GLOBALS.Scale, 
                                    GLOBALS.Scale, 
                                    GLOBALS.Scale, 
                                    brushColor with { A = (byte)(GLOBALS.Level.Effects[_currentAppliedEffect].Item3[y, x] * 255 / 100) }
                                );
                                
                                // Brush
                                
                                var effAreaRect = new Rectangle(x * GLOBALS.Scale, y * GLOBALS.Scale,
                                    GLOBALS.Scale, GLOBALS.Scale);
                                if (_brushRadius > 0 && CheckCollisionCircleRec(
                                        new Vector2(effectsMatrixX+0.5f, effectsMatrixY+0.5f) * GLOBALS.Scale,
                                        _brushRadius*GLOBALS.Scale, effAreaRect))
                                {
                                    DrawRectangleRec(effAreaRect, WHITE with { a = 50 });
                                }
                                
                                if (effectsMatrixX == x && effectsMatrixY == y)
                                {
                                    DrawCircleLines(
                                        (int) ((x + 0.5f) * GLOBALS.Scale), 
                                        (int) ((y + 0.5f) * GLOBALS.Scale),
                                        (_brushRadius+1)*GLOBALS.Scale,
                                        _brushEraseMode ? RED : WHITE);
                                        
                                    DrawRectangleLinesEx(
                                        new Rectangle(
                                            x * GLOBALS.Scale,
                                            y * GLOBALS.Scale,
                                            GLOBALS.Scale,
                                            GLOBALS.Scale
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

                rlImGui.Begin();
                
                // Applied Effects

                if (ImGui.Begin("Effects##AppliedEffectsList"))
                {
                    var pos = ImGui.GetWindowPos();
                    var winSpace = ImGui.GetWindowSize();

                    if (CheckCollisionPointRec(GetMousePosition(), new(pos.X - 5, pos.Y, winSpace.X + 10, winSpace.Y)))
                    {
                        _isEffectsWinHovered = true;

                        if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) _isEffectsWinDragged = true;
                    }
                    else
                    {
                        _isEffectsWinHovered = false;
                    }

                    if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && _isEffectsWinDragged) _isEffectsWinDragged = false;
                    
                    //
                    
                    if (ImGui.Button("+")) _addNewEffectMode = true;
                    
                    if (GLOBALS.Level.Effects.Length > 0 && ImGui.Button("X"))
                    {
                        ImGui.SameLine();
                        GLOBALS.Level.Effects = GLOBALS.Level.Effects.Where((_, ei) => ei != _currentAppliedEffect).ToArray();

                        if (_currentAppliedEffect < 0) _currentAppliedEffect = 0;
                        else if (_currentAppliedEffect >= GLOBALS.Level.Effects.Length - 1)
                            _currentAppliedEffect = GLOBALS.Level.Effects.Length - 1;
                    }

                    if (_currentAppliedEffect > 0)
                    {
                        ImGui.SameLine();
                        if (ImGui.Button("Shift Up"))
                        {
                            (GLOBALS.Level.Effects[_currentAppliedEffect],
                                GLOBALS.Level.Effects[_currentAppliedEffect - 1]) = (
                                GLOBALS.Level.Effects[_currentAppliedEffect - 1],
                                GLOBALS.Level.Effects[_currentAppliedEffect]);
                            
                            _currentAppliedEffect--;
                        }
                    }

                    if (_currentAppliedEffect < GLOBALS.Level.Effects.Length - 1)
                    {
                        ImGui.SameLine();
                        if (ImGui.Button("Shift Down"))
                        {
                            (GLOBALS.Level.Effects[_currentAppliedEffect],
                                GLOBALS.Level.Effects[_currentAppliedEffect + 1]) = (
                                GLOBALS.Level.Effects[_currentAppliedEffect + 1],
                                GLOBALS.Level.Effects[_currentAppliedEffect]);
                            
                            _currentAppliedEffect++;
                        }
                    }

                    if (ImGui.BeginListBox("##AppliedEffects", ImGui.GetContentRegionAvail()))
                    {
                        // i is index relative to the GLOBALS.Page; oi is index relative to the whole list
                        foreach (var (i, (name, _, _)) in GLOBALS.Level.Effects.Select((value, i) => (i, value)))
                        {
                            if (ImGui.Selectable(name, i == _currentAppliedEffect)) _currentAppliedEffect = i;
                        }

                        ImGui.EndListBox();
                    }
                    
                    ImGui.End();
                }
                
                // Navigation
            
                var navWindowRect = Printers.ImGui.NavigationWindow();

                _isNavigationWinHovered = CheckCollisionPointRec(GetMousePosition(), navWindowRect with
                {
                    X = navWindowRect.X - 5, width = navWindowRect.width + 10
                });
                
                if (_isNavigationWinHovered && IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    _isNavigationWinDragged = true;
                }
                else if (_isNavigationWinDragged && IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    _isNavigationWinDragged = false;
                }
                
                // Options
                {
                    var options = GLOBALS.Level.Effects.Length > 0 && _currentAppliedEffect != -1
                        ? GLOBALS.Level.Effects[_currentAppliedEffect].Item2
                        : [];


                    if (ImGui.Begin("Options"))
                    {
                        var pos = ImGui.GetWindowPos();
                        var winSpace = ImGui.GetWindowSize();

                        if (CheckCollisionPointRec(GetMousePosition(),
                                new(pos.X - 5, pos.Y, winSpace.X + 10, winSpace.Y)))
                        {
                            _isOptionsWinHovered = true;

                            if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) _isOptionsWinDragged = true;
                        }
                        else
                        {
                            _isOptionsWinHovered = false;
                        }

                        if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && _isOptionsWinDragged)
                            _isOptionsWinDragged = false;

                        var halfWidth = ImGui.GetContentRegionAvail().X / 2f - ImGui.GetStyle().ItemSpacing.X / 2f;
                        var boxHeight = ImGui.GetContentRegionAvail().Y;

                        if (options is not [])
                        {
                            if (ImGui.BeginListBox("##EffectOptionTitles", new Vector2(halfWidth, boxHeight)))
                            {
                                for (var optionIndex = 0; optionIndex < options.Length; optionIndex++)
                                {
                                    if (options[optionIndex].Name == "Delete/Move") continue;

                                    var selected = ImGui.Selectable(options[optionIndex].Name,
                                        optionIndex == _optionsIndex);
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
                }
                
                // Shortcuts window
                if (GLOBALS.Settings.GeneralSettings.ShortcutWindow)
                {
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


                }
                
                // Settings window
                if (ImGui.Begin("Settings##EffectsSettings"))
                {
                    var pos = ImGui.GetWindowPos();
                    var winSpace = ImGui.GetWindowSize();

                    if (CheckCollisionPointRec(GetMousePosition(), new(pos.X - 5, pos.Y, winSpace.X + 10, winSpace.Y)))
                    {
                        _isSettingsWinHovered = true;

                        if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)) _isSettingsWinDragged = true;
                    }
                    else
                    {
                        _isSettingsWinHovered = false;
                    }

                    if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && _isSettingsWinDragged) _isSettingsWinDragged = false;
                    
                    //
                    
                    ImGui.SeparatorText("Colors");
                    var lightEffectColor = GLOBALS.Settings.EffectsSettings.EffectColorLight;
                    var darkEffectColor = GLOBALS.Settings.EffectsSettings.EffectColorDark;

                    var lightColor = new Vector3(lightEffectColor.R/255f, lightEffectColor.G/255f, lightEffectColor.B/255f);
                    ImGui.SetNextItemWidth(250);
                    ImGui.ColorEdit3("Light-mode effect", ref lightColor);
                    
                    var darkColor = new Vector3(darkEffectColor.R/255f, darkEffectColor.G/255f, darkEffectColor.B/255f);
                    ImGui.SetNextItemWidth(250);
                    ImGui.ColorEdit3("Dark-mode effect", ref darkColor);

                    GLOBALS.Settings.EffectsSettings.EffectColorLight = new ConColor((byte)(lightColor.X * 255), (byte)
                        (lightColor.Y * 255), (byte)(lightColor.Z * 255), 255);
                    
                    GLOBALS.Settings.EffectsSettings.EffectColorDark = new ConColor((byte)(darkColor.X * 255), (byte)
                        (darkColor.Y * 255), (byte)(darkColor.Z * 255), 255);
                    
                    ImGui.End();
                }
                
                rlImGui.End();
            }
            EndDrawing();
            
        }
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera;
    }
}
