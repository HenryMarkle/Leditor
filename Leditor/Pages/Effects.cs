using System.Numerics;
using ImGuiNET;
using rlImGui_cs;
using static Raylib_cs.Raylib;
using Leditor.Types;


namespace Leditor.Pages;

internal class EffectsEditorPage : EditorPage
{
    public override void Dispose()
    {
        Disposed = true;
    }
    
    private readonly EffectsShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.EffectsEditor;

    private Camera2D _camera = new() { Zoom = 1.0f };

    private bool _showTiles = true;
    private bool _showProps;
    private bool _tintedProps;
    
    private bool _addNewEffectMode;

    private int _newEffectSelectedValue;

    private int _newEffectCategorySelectedValue;

    private int _currentAppliedEffect;
    private int _currentAppliedEffectPage;

    private readonly string[] _newEffectCategoryNames = [..GLOBALS.EffectCategories];
    private readonly string[][] _newEffectNames = GLOBALS.Effects.Select(c => c.Select(e => e).ToArray()).ToArray();

    private int _prevMatrixX = -1;
    private int _prevMatrixY = -1;

    private int _brushRadius = 3;

    private bool _clickTracker;
    private bool _brushEraseMode;

    private bool _shouldRedrawLevel = true;

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
                    
                    var distance = Raymath.Vector2Distance(squareV, centerV) / GLOBALS.Scale;

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

    private bool _isNavbarHovered;

    private bool _isOptionsInputActive;

    private void RedrawLevel()
    {
        BeginTextureMode(GLOBALS.Textures.GeneralLevel);
        ClearBackground(
            GLOBALS.Settings.GeneralSettings.DarkTheme
                ? new Color(50, 50, 50, 255)
                : Color.White);

        Printers.DrawGeoLayer(2, GLOBALS.Scale, false, GLOBALS.Settings.GeneralSettings.DarkTheme ? new Color(150, 150, 150, 255) : Color.Black with { A = 150 }, GLOBALS.LayerStackableFilter);
        if (_showTiles) Printers.DrawTileLayer(GLOBALS.Layer, 2, GLOBALS.Scale, false, TileDrawMode.Preview);
        if(_showProps) Printers.DrawPropLayer(2, _tintedProps, GLOBALS.Scale);
        
        Printers.DrawGeoLayer(1, GLOBALS.Scale, false, GLOBALS.Settings.GeneralSettings.DarkTheme ? new Color(100, 100, 100, 255) : Color.Black with { A = 150 }, GLOBALS.LayerStackableFilter);
        if (_showTiles) Printers.DrawTileLayer(GLOBALS.Layer, 1, GLOBALS.Scale, false, TileDrawMode.Preview);
        if(_showProps) Printers.DrawPropLayer(1, _tintedProps, GLOBALS.Scale);
        
        if (!GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel > -1)
        {
            DrawRectangle(
                (-1) * GLOBALS.Scale,
                (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel - GLOBALS.Level.Padding.bottom) * GLOBALS.Scale,
                (GLOBALS.Level.Width + 2) * GLOBALS.Scale,
                (GLOBALS.Level.WaterLevel + GLOBALS.Level.Padding.bottom) * GLOBALS.Scale,
                new Color(0, 0, 255, 110)
            );
        }
        
        Printers.DrawGeoLayer(0, GLOBALS.Scale, false, Color.Black with { A = 255 });
        if (_showTiles) Printers.DrawTileLayer(GLOBALS.Layer, 0, GLOBALS.Scale, false, TileDrawMode.Preview);
        if(_showProps) Printers.DrawPropLayer(0, _tintedProps, GLOBALS.Scale);
        
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
        
        EndTextureMode();
    }

    public void OnPageUpdated(int previous, int @next) {
        _shouldRedrawLevel = true;
    }

    public override void Draw()
    {
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera;
        
        var ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
        var shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
        var alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);
        
        if (!_isOptionsInputActive)
        {
            // if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToMainPage.Check(ctrl, shift, alt)) GLOBALS.Page = 1;
            // if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToGeometryEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 2;
            // if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToTileEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 3;
            // if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToCameraEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 4;
            // if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToLightEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 5;
            // if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToDimensionsEditor.Check(ctrl, shift, alt))
            // {
            //     GLOBALS.Page = 6;
            //     Logger.Debug("go from GLOBALS.Page 7 to GLOBALS.Page 6");
            // }
            // // if (IsKeyReleased(KeyboardKey.KEY_SEVEN)) GLOBALS.Page = 7;
            // if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToPropsEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 8;
            // if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToSettingsPage.Check(ctrl, shift, alt)) GLOBALS.Page = 9;
        }


        // Display menu

        if (_shortcuts.NewEffect.Check(ctrl, shift, alt))
        {
            _addNewEffectMode = !_addNewEffectMode;
        }

        //

        if (_addNewEffectMode)
        {
            if (_shortcuts.MoveCategoryUpInNewEffectsMenu.Check(ctrl, shift, alt))
            {
                _newEffectSelectedValue = 0;

                _newEffectCategorySelectedValue = --_newEffectCategorySelectedValue;

                if (_newEffectCategorySelectedValue < 0) _newEffectCategorySelectedValue = GLOBALS.EffectCategories.Length - 1;
            }
            else if (_shortcuts.MoveCategoryDownInNewEffectsMenu.Check(ctrl, shift, alt))
            {
                _newEffectSelectedValue = 0;
                _newEffectCategorySelectedValue = ++_newEffectCategorySelectedValue % GLOBALS.EffectCategories.Length;
            }

            if (_shortcuts.MoveUpInNewEffectMenu.Check(ctrl, shift, alt))
            {
                _newEffectSelectedValue = --_newEffectSelectedValue;
                if (_newEffectSelectedValue < 0) _newEffectSelectedValue = GLOBALS.Effects[_newEffectCategorySelectedValue].Length - 1;
            }
            else if (_shortcuts.MoveDownInNewEffectMenu.Check(ctrl, shift, alt))
            {
                _newEffectSelectedValue = ++_newEffectSelectedValue % GLOBALS.Effects[_newEffectCategorySelectedValue].Length;
            }
            
            

            if ((_shortcuts.AcceptNewEffect.Check(ctrl, shift, alt) || _shortcuts.AcceptNewEffectAlt.Check(ctrl, shift, alt)) && 
                _newEffectSelectedValue > -1 && _newEffectSelectedValue < GLOBALS.Effects[_newEffectCategorySelectedValue].Length)
            {
                var effectToAdd = GLOBALS.Effects[_newEffectCategorySelectedValue][_newEffectSelectedValue];
                var matrixToAdd = new double[GLOBALS.Level.Height, GLOBALS.Level.Width];

                if (Utils.EffectCoversScreen(effectToAdd)) {
                    for (var y = 0; y < GLOBALS.Level.Height; y++) {
                        for (var x = 0; x < GLOBALS.Level.Width; x++) {
                            matrixToAdd[y, x] = 100;
                        }
                    }
                }

                GLOBALS.Level.Effects = [
                    .. GLOBALS.Level.Effects,
                    (
                        effectToAdd,
                        Utils.NewEffectOptions(GLOBALS.Effects[_newEffectCategorySelectedValue][_newEffectSelectedValue]),
                        matrixToAdd
                    )
                ];

                _addNewEffectMode = false;
                if (GLOBALS.Level.Effects.Length > 0) _currentAppliedEffect = GLOBALS.Level.Effects.Length -1;
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

                // ImGui
                
                rlImGui.Begin();
                
                ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
                
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

                            if (selected) {
                                if (_newEffectSelectedValue != index) _newEffectSelectedValue = index;
                                else {
                                    var effectToAdd = GLOBALS.Effects[_newEffectCategorySelectedValue][_newEffectSelectedValue];
                                    var matrixToAdd = new double[GLOBALS.Level.Height, GLOBALS.Level.Width];

                                    if (Utils.EffectCoversScreen(effectToAdd)) {
                                        for (var y = 0; y < GLOBALS.Level.Height; y++) {
                                            for (var x = 0; x < GLOBALS.Level.Width; x++) {
                                                matrixToAdd[y, x] = 100;
                                            }
                                        }
                                    }

                                    GLOBALS.Level.Effects = [
                                        .. GLOBALS.Level.Effects,
                                        (
                                            effectToAdd,
                                            Utils.NewEffectOptions(GLOBALS.Effects[_newEffectCategorySelectedValue][_newEffectSelectedValue]),
                                            matrixToAdd
                                        )
                                    ];

                                    _addNewEffectMode = false;
                                    _currentAppliedEffect++;

                                    if (GLOBALS.Level.Effects.Length > 0) _currentAppliedEffect = GLOBALS.Level.Effects.Length - 1;
                                }
                            }
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
                        _currentAppliedEffect++;

                        if (GLOBALS.Level.Effects.Length > 0) _currentAppliedEffect = GLOBALS.Level.Effects.Length - 1;
                    }
                    
                    ImGui.End();
                }
                rlImGui.End();
            }
            EndDrawing();
        }
        else
        {
            Utils.Restrict(ref _currentAppliedEffect, 0, GLOBALS.Level.Effects.Length-1);
            
            var effectsMouse = GetScreenToWorld2D(GetMousePosition(), _camera);

            //                        v this was done to avoid rounding errors
            var effectsMatrixY = effectsMouse.Y < 0 ? -1 : (int)effectsMouse.Y / GLOBALS.Scale;
            var effectsMatrixX = effectsMouse.X < 0 ? -1 : (int)effectsMouse.X / GLOBALS.Scale;

            var appliedEffectsPanelHeight = GetScreenHeight() - 200;
            const int appliedEffectRecHeight = 30;
            var appliedEffectPageSize = (appliedEffectsPanelHeight / (appliedEffectRecHeight + 30));

            // Prevent using the brush when mouse over the effects list
            var canUseBrush = !_isNavbarHovered && 
                                !_isSettingsWinHovered && 
                              !_isSettingsWinDragged && 
                              !_isOptionsWinHovered && 
                              !_isOptionsWinDragged && 
                              !_isShortcutsWinHovered && 
                              !_isShortcutsWinDragged && 
                              !_isNavigationWinHovered &&
                              !_isNavigationWinDragged &&
                              !_isEffectsWinHovered &&
                              !_isEffectsWinDragged &&
                              !_addNewEffectMode && GLOBALS.Level.Effects.Length > 0;

            // Movement

            if (_shortcuts.DragLevel.Check(ctrl, shift, alt, true) || _shortcuts.DragLevelAlt.Check(ctrl, shift, alt, true))
            {
                var delta = GetMouseDelta();
                delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
                _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
            }

            // Brush size

            var effectsMouseWheel = GetMouseWheelMove();
            var isBrushSizeConstrained = _currentAppliedEffect >= 0 && 
                                         _currentAppliedEffect < GLOBALS.Level.Effects.Length && 
                                         Utils.IsEffectBruhConstrained(GLOBALS.Level.Effects[_currentAppliedEffect].Item1);

            if (isBrushSizeConstrained)
            {
                _brushRadius = 0;
            }

            if (_shortcuts.EnlargeBrush.Check(ctrl, shift, alt)) {
                if (isBrushSizeConstrained)
                {
                    _brushRadius = 0;
                }
                else 
                {
                    _brushRadius += 1;

                    Utils.Restrict(ref _brushRadius, 0, 10);
                }
            } else if (_shortcuts.ShrinkBrush.Check(ctrl, shift, alt)) {
                if (isBrushSizeConstrained)
                {
                    _brushRadius = 0;
                }
                else 
                {
                    _brushRadius -= 1;

                    Utils.Restrict(ref _brushRadius, 0, 10);
                }
            }

            if (IsKeyDown(_shortcuts.ResizeBrush.Key))
            {
                if (isBrushSizeConstrained)
                {
                    _brushRadius = 0;
                }
                else if (effectsMouseWheel != 0)
                {
                    _brushRadius += (int)effectsMouseWheel;

                    Utils.Restrict(ref _brushRadius, 0, 10);
                }
            }
            else if (canUseBrush)
            {
                if (effectsMouseWheel != 0)
                {
                    var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
                    _camera.Offset = GetMousePosition();
                    _camera.Target = mouseWorldPosition;
                    _camera.Zoom += effectsMouseWheel * GLOBALS.ZoomIncrement;
                    if (_camera.Zoom < GLOBALS.ZoomIncrement) _camera.Zoom = GLOBALS.ZoomIncrement;
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
                    mtx = GLOBALS.Level.Effects[_currentAppliedEffect];
                    #endif

                    var strength = Utils.GetEffectBrushStrength(mtx.Item1);
                    Utils.Restrict (ref strength, 0, 100);
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
                    mtx = GLOBALS.Level.Effects[_currentAppliedEffect];
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
                    if (_optionsIndex < 1) _optionsIndex = GLOBALS.Level.Effects[_currentAppliedEffect].Item2.Length - 1;
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
                if (_shouldRedrawLevel)
                {
                    Printers.DrawLevelIntoBuffer(GLOBALS.Textures.GeneralLevel, new Printers.DrawLevelParams
                    {
                        Water = true,
                        WaterAtFront = GLOBALS.Level.WaterAtFront,
                        DarkTheme = GLOBALS.Settings.GeneralSettings.DarkTheme,
                        TilesLayer1 = _showTiles,
                        TilesLayer2 = _showTiles,
                        TilesLayer3 = _showTiles,
                        PropsLayer1 = _showProps,
                        PropsLayer2 = _showProps,
                        PropsLayer3 = _showProps,
                        CurrentLayer = 0,
                        PropDrawMode = GLOBALS.Settings.GeneralSettings.DrawPropMode,
                        TileDrawMode = GLOBALS.Settings.GeneralSettings.DrawTileMode,
                        Palette = GLOBALS.SelectedPalette,
                        VisibleStrayTileFragments = false
                    });
                    _shouldRedrawLevel = false;
                }

                ClearBackground(new Color(0, 0, 0, 255));

                BeginMode2D(_camera);
                {
                    // Outer level border
                    DrawRectangleLinesEx(
                        new Rectangle(
                            -3, -3,
                            (GLOBALS.Level.Width * GLOBALS.Scale) + 6,
                            (GLOBALS.Level.Height * GLOBALS.Scale) + 6
                        ),
                        3f,
                        Color.White
                    );
                    
                    BeginShaderMode(GLOBALS.Shaders.VFlip);
                    SetShaderValueTexture(GLOBALS.Shaders.VFlip, GetShaderLocation(GLOBALS.Shaders.VFlip, "inputTexture"), GLOBALS.Textures.GeneralLevel.Texture);
                    DrawTexture(GLOBALS.Textures.GeneralLevel.Texture, 0, 0, Color.White);
                    EndShaderMode();

                    // Effect matrix

                    if (GLOBALS.Level.Effects.Length > 0 &&
                        _currentAppliedEffect >= 0 &&
                        _currentAppliedEffect < GLOBALS.Level.Effects.Length)
                    {

                        if (GLOBALS.Settings.GeneralSettings.DarkTheme) 
                            DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.Scale, GLOBALS.Level.Height * GLOBALS.Scale, GLOBALS.Settings.EffectsSettings.EffectsCanvasColorDark);
                        else DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.Scale, GLOBALS.Level.Height * GLOBALS.Scale, GLOBALS.Settings.EffectsSettings.EffectsCanvasColorLight);

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
                                    brushColor with { A = (byte)((GLOBALS.Level.Effects[_currentAppliedEffect].Item3[y, x] * 255 / 100) * 0.9f) }
                                );
                            }
                        }
                    }

                    // Brush

                    if (GLOBALS.Settings.EffectsSettings.BlockyBrush) {
                        Printers.DrawCircularSquareLines(effectsMatrixX, effectsMatrixY, _brushRadius, 20, 2, Color.White);
                    } else {
                        DrawCircleLines(effectsMatrixX * 20, effectsMatrixY * 20, _brushRadius * 20, Color.White);
                    }

                }
                EndMode2D();

                // UI

                rlImGui.Begin();
                
                ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
                
                // Navigation bar
                
                if (GLOBALS.Settings.GeneralSettings.Navbar) GLOBALS.NavSignal = Printers.ImGui.Nav(out _isNavbarHovered);
                
                // Applied Effects

                var effectsOpened = ImGui.Begin("Effects##AppliedEffectsList");

                var effectsPos = ImGui.GetWindowPos();
                var effectsWinSpace = ImGui.GetWindowSize();

                if (CheckCollisionPointRec(GetMousePosition(), new(effectsPos.X - 5, effectsPos.Y, effectsWinSpace.X + 10, effectsWinSpace.Y)))
                {
                    _isEffectsWinHovered = true;

                    if (IsMouseButtonDown(MouseButton.Left)) _isEffectsWinDragged = true;
                }
                else
                {
                    _isEffectsWinHovered = false;
                }

                if (IsMouseButtonReleased(MouseButton.Left) && _isEffectsWinDragged) _isEffectsWinDragged = false;
                
                if (effectsOpened)
                {
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
                        var drawList = ImGui.GetWindowDrawList();
                        var textHeight = ImGui.GetTextLineHeight();

                        // i is index relative to the GLOBALS.Page; oi is index relative to the whole list
                        foreach (var (i, (name, _, _)) in GLOBALS.Level.Effects.Select((value, i) => (i, value)))
                        {
                            var options = GLOBALS.Level.Effects.Length > 0
                                ? GLOBALS.Level.Effects[i].Item2
                                : [];

                            var option = options.Length > 0 
                                ? options.SingleOrDefault(o => o.Name == "Layers") 
                                : null;

                            if (option is null) {
                                if (ImGui.Selectable($"      {i}. {name}", i == _currentAppliedEffect)) _currentAppliedEffect = i;
                            } else {
                                var cursor = ImGui.GetCursorScreenPos();

                                var active = GLOBALS.Settings.GeneralSettings.DarkTheme ? Vector4.One : Vector4.Zero + new Vector4(0, 0, 0, 1);
                                var deactive = GLOBALS.Settings.GeneralSettings.DarkTheme ? Vector4.One - new Vector4(0.6f, 0.6f, 0.6f, 0) : Vector4.One - new Vector4(0.2f, 0.2f, 0.2f, 0);

                                switch (option.Choice as string) {
                                    case "All":
                                    {
                                        drawList.AddRectFilled(
                                            p_min: cursor,
                                            p_max: cursor + new Vector2(10f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(active)
                                        );

                                        drawList.AddRectFilled(
                                            p_min: cursor + new Vector2(12, 0),
                                            p_max: cursor + new Vector2(22f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(active)
                                        );

                                        drawList.AddRectFilled(
                                            p_min: cursor + new Vector2(24, 0),
                                            p_max: cursor + new Vector2(34f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(active)
                                        );
                                    }
                                    break;

                                    case "1":
                                    {
                                        drawList.AddRectFilled(
                                            p_min: cursor,
                                            p_max: cursor + new Vector2(10f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(active)
                                        );
                                        drawList.AddRectFilled(
                                            p_min: cursor + new Vector2(12, 0),
                                            p_max: cursor + new Vector2(22f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(deactive)
                                        );
                                        drawList.AddRectFilled(
                                            p_min: cursor + new Vector2(24, 0),
                                            p_max: cursor + new Vector2(34f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(deactive)
                                        );
                                    }
                                    break;

                                    case "2":
                                    {
                                        drawList.AddRectFilled(
                                            p_min: cursor,
                                            p_max: cursor + new Vector2(10f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(deactive)
                                        );
                                        drawList.AddRectFilled(
                                            p_min: cursor + new Vector2(12, 0),
                                            p_max: cursor + new Vector2(22f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(active)
                                        );
                                        drawList.AddRectFilled(
                                            p_min: cursor + new Vector2(24, 0),
                                            p_max: cursor + new Vector2(34f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(deactive)
                                        );
                                    }
                                    break;

                                    case "3":
                                    {
                                        drawList.AddRectFilled(
                                            p_min: cursor,
                                            p_max: cursor + new Vector2(10f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(deactive)
                                        );
                                        drawList.AddRectFilled(
                                            p_min: cursor + new Vector2(12, 0),
                                            p_max: cursor + new Vector2(24f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(deactive)
                                        );
                                        drawList.AddRectFilled(
                                            p_min: cursor + new Vector2(22, 0),
                                            p_max: cursor + new Vector2(34f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(active)
                                        );
                                    }
                                    break;

                                    case "1:st and 2:nd":
                                    {
                                        drawList.AddRectFilled(
                                            p_min: cursor,
                                            p_max: cursor + new Vector2(10f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(active)
                                        );
                                        drawList.AddRectFilled(
                                            p_min: cursor + new Vector2(12, 0),
                                            p_max: cursor + new Vector2(22f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(active)
                                        );
                                        drawList.AddRectFilled(
                                            p_min: cursor + new Vector2(24, 0),
                                            p_max: cursor + new Vector2(34f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(deactive)
                                        );
                                    }
                                    break;

                                    case "2:nd and 3:rd":
                                    {
                                        drawList.AddRectFilled(
                                            p_min: cursor,
                                            p_max: cursor + new Vector2(10f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(deactive)
                                        );
                                        drawList.AddRectFilled(
                                            p_min: cursor + new Vector2(12, 0),
                                            p_max: cursor + new Vector2(22f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(active)
                                        );
                                        drawList.AddRectFilled(
                                            p_min: cursor + new Vector2(24, 0),
                                            p_max: cursor + new Vector2(34f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(active)
                                        );
                                    }
                                    break;
                                }

                                if (ImGui.Selectable($"      {i}. {name}", i == _currentAppliedEffect)) _currentAppliedEffect = i;
                            }
                            
                        }

                        ImGui.EndListBox();
                    }
                    
                    ImGui.End();
                }
                
                // Options
                {
                    var options = GLOBALS.Level.Effects.Length > 0
                        ? GLOBALS.Level.Effects[_currentAppliedEffect].Item2
                        : [];

                    var optionsOpened = ImGui.Begin("Options");

                    var optionsPos = ImGui.GetWindowPos();
                    var optionsWinSpace = ImGui.GetWindowSize();

                    if (CheckCollisionPointRec(GetMousePosition(),
                            new(optionsPos.X - 5, optionsPos.Y, optionsWinSpace.X + 10, optionsWinSpace.Y)))
                    {
                        _isOptionsWinHovered = true;

                        if (IsMouseButtonDown(MouseButton.Left)) _isOptionsWinDragged = true;
                    }
                    else
                    {
                        _isOptionsWinHovered = false;
                    }

                    if (IsMouseButtonReleased(MouseButton.Left) && _isOptionsWinDragged)
                        _isOptionsWinDragged = false;
                    
                    if (optionsOpened)
                    {
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
                            
                            Utils.Restrict(ref _optionsIndex, 0, options.Length-1);

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
                            X = shortcutWindowRect.X - 5, Width = shortcutWindowRect.Width + 10
                        }
                    );

                    if (_isShortcutsWinHovered && IsMouseButtonDown(MouseButton.Left))
                    {
                        _isShortcutsWinDragged = true;
                    }
                    else if (_isShortcutsWinDragged && IsMouseButtonReleased(MouseButton.Left))
                    {
                        _isShortcutsWinDragged = false;
                    }


                }
                
                // Settings window

                var settingsOpened = ImGui.Begin("Settings##EffectsSettings");
                
                var settingsPos = ImGui.GetWindowPos();
                var settingsWinSpace = ImGui.GetWindowSize();

                if (CheckCollisionPointRec(GetMousePosition(), new(settingsPos.X - 5, settingsPos.Y, settingsWinSpace.X + 10, settingsWinSpace.Y)))
                {
                    _isSettingsWinHovered = true;

                    if (IsMouseButtonDown(MouseButton.Left)) _isSettingsWinDragged = true;
                }
                else
                {
                    _isSettingsWinHovered = false;
                }

                if (IsMouseButtonReleased(MouseButton.Left) && _isSettingsWinDragged) _isSettingsWinDragged = false;
                
                if (settingsOpened)
                {
                    ImGui.SeparatorText("Effect Colors");
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
                    
                    ImGui.SeparatorText("Effects Canvas Colors");

                    Vector4 lightCanvasColor = GLOBALS.Settings.EffectsSettings.EffectsCanvasColorLight;
                    Vector4 darkCanvasColor = GLOBALS.Settings.EffectsSettings.EffectsCanvasColorDark;

                    lightCanvasColor /= 255f;
                    darkCanvasColor /= 255f;
                    
                    ImGui.SetNextItemWidth(250);
                    ImGui.ColorEdit4("Light-mode canvas", ref lightCanvasColor);
                    
                    ImGui.SetNextItemWidth(250);
                    ImGui.ColorEdit4("Dark-mode canvas", ref darkCanvasColor);

                    GLOBALS.Settings.EffectsSettings.EffectsCanvasColorLight = lightCanvasColor*255;
                    GLOBALS.Settings.EffectsSettings.EffectsCanvasColorDark = darkCanvasColor*255;

                    ImGui.Spacing();

                    if (ImGui.Checkbox("Tiles", ref _showTiles)) _shouldRedrawLevel = true;
                    if (ImGui.Checkbox("Props", ref _showProps)) _shouldRedrawLevel = true;

                    if (_showProps)
                    {
                        if (ImGui.Checkbox("Tinted Props", ref _tintedProps)) _shouldRedrawLevel = true;
                    };

                    var blockyBrush = GLOBALS.Settings.EffectsSettings.BlockyBrush;

                    if (ImGui.Checkbox("Blocky Brush Style", ref blockyBrush))
                        GLOBALS.Settings.EffectsSettings.BlockyBrush = blockyBrush;
                    
                    ImGui.End();
                }
                
                rlImGui.End();
            }
            EndDrawing();
            
        }
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera;
    }
}
