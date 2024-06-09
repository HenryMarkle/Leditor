using System.Numerics;
using System.Text.Json;
using ImGuiNET;
using rlImGui_cs;
using static Raylib_cs.Raylib;
using Leditor.Types;

namespace Leditor.Pages;

internal class LightEditorPage : EditorPage, IContextListener
{
    public override void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
        _stretchTexture?.Dispose();

        _layer1.Dispose();
        _layer2.Dispose();
        _layer3.Dispose();

        UnloadShader(_mask);
    }

    private void ResetRTs() {
        _layer1.Dispose();
        _layer2.Dispose();
        _layer3.Dispose();

        var width = GLOBALS.Level.Width * 20 + 300;
        var height = GLOBALS.Level.Height * 20 + 300;

        _layer1 = new(width, height);
        _layer2 = new(width, height);
        _layer3 = new(width, height);

        BeginTextureMode(_layer1);
        ClearBackground(Color.White);
        EndTextureMode();

        BeginTextureMode(_layer2);
        ClearBackground(Color.White);
        EndTextureMode();

        BeginTextureMode(_layer3);
        ClearBackground(Color.White);
        EndTextureMode();
    }

    public void OnProjectCreated(object sender, EventArgs e)
    {
        ResetRTs();
    }

    public void OnProjectLoaded(object sender, EventArgs e)
    {
        ResetRTs();
    }

    public LightEditorPage()
    {
        _layer1 = new(0, 0);
        _layer2 = new(0, 0);
        _layer3 = new(0, 0);

        _mask = LoadShaderFromMemory(null, @"#version 330
        
        in vec2 fragTexCoord;
        in vec4 fragColor;
        
        out vec4 FragColor;
        
        uniform sampler2D inputTexture;
        uniform int vflip = 0;
        
        void main() {
            vec4 color = vec4(0, 0, 0, 0);

            if (vflip == 0) {
                color = texture(inputTexture, fragTexCoord);
            } else {
                color = texture(inputTexture, vec2(fragTexCoord.x, 1.0 - fragTexCoord.y));
            }

            if (color.r == 1.0 && color.g == 1.0 && color.b == 1.0) { discard; }

            FragColor = fragColor;
        }");
    }
    
    private Camera2D _camera = new() { Zoom = 0.5f, Target = new(-500, -200) };
    
    private RL.Managed.RenderTexture2D _layer1;
    private RL.Managed.RenderTexture2D _layer2;
    private RL.Managed.RenderTexture2D _layer3;

    private Shader _mask;

    private int _lightBrushTextureIndex;
    private float _lightBrushWidth = 200;
    private float _lightBrushHeight = 200;
    private float _lightBrushRotation;
    private bool _eraseShadow;
    private bool _showTiles;
    private bool _showProps;
    private bool _tilePreview = true;
    private bool _tintedTileTextures = true;

    private bool _shouldRedrawLevel = true;

    private int _quadLock;
    private readonly QuadVectors _quadPoints = new();

    private bool _stretchMode;

    private bool _isDraggingIndicator;

    private const float InitialGrowthFactor = 0.01f;
    private float _growthFactor = InitialGrowthFactor;

    private RL.Managed.Texture2D _stretchTexture;
    

    private void DrawLayers() {
        var shader = _mask;

        BeginTextureMode(_layer3);
        ClearBackground(Color.White);

        DrawTextureV(GLOBALS.Textures.LightMap.Texture, GLOBALS.Level.LightFlatness * 30 * DegreeToVector(GLOBALS.Level.LightAngle), Color.White);

        EndTextureMode();


        BeginTextureMode(_layer2);
        {
            ClearBackground(Color.White with { A = 0 });

            BeginShaderMode(shader);
            {
                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), GLOBALS.Textures.LightMap.Texture);

                DrawTextureRec(
                    GLOBALS.Textures.LightMap.Texture,
                    new Rectangle(GLOBALS.Level.LightFlatness * 20 * DegreeToVector(GLOBALS.Level.LightAngle), new Vector2(GLOBALS.Textures.LightMap.Texture.Width, GLOBALS.Textures.LightMap.Texture.Height)),
                    new Vector2(0, 0),
                    Color.Gray
                );
            }
            EndShaderMode();
        }
        EndTextureMode();
        
        BeginTextureMode(_layer1);
        {
            ClearBackground(Color.White with { A = 0 });

            BeginShaderMode(shader);
            {
                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), GLOBALS.Textures.LightMap.Texture);

                DrawTextureRec(
                    GLOBALS.Textures.LightMap.Texture,
                    new Rectangle(GLOBALS.Level.LightFlatness * 10 * DegreeToVector(GLOBALS.Level.LightAngle), new Vector2(GLOBALS.Textures.LightMap.Texture.Width, GLOBALS.Textures.LightMap.Texture.Height)),
                    new Vector2(0, 0),
                    Color.Black with { A = 170 }
                );
            }
            EndShaderMode();
        }
        EndTextureMode();
    }
    
    private void ResetQuadPoints()
    {
        var width = GLOBALS.Level.Width * GLOBALS.Scale + 300;
        var height = GLOBALS.Level.Height * GLOBALS.Scale + 300;
        
        _quadPoints.TopLeft = new Vector2(0, 0);
        _quadPoints.TopRight = new Vector2(width, 0);
        _quadPoints.BottomRight = new Vector2(width, height);;
        _quadPoints.BottomLeft = new Vector2(0, height);
        
        _stretchTexture?.Dispose();

        var img = LoadImageFromTexture(GLOBALS.Textures.LightMap.Texture);
        ImageFlipVertical(ref img);
        
        _stretchTexture = new RL.Managed.Texture2D(img);
        
        UnloadImage(img);
    }

    private void SwitchStretchMode()
    {
        _stretchMode = !_stretchMode;
        
        ResetQuadPoints();
    }

    private void ResetGrowthFactor()
    {
        _growthFactor = InitialGrowthFactor;
    }
    private void IncreaseGrowthFactor()
    {
        _growthFactor += 0.05f;
    }
    
    private Rectangle _lightBrushSource;
    private Rectangle _lightBrushDest;
    private Vector2 _lightBrushOrigin;

    private readonly LightShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.LightEditor;
    
    private bool _isShortcutsWinHovered;
    private bool _isShortcutsWinDragged;

    private bool _isStretchWinHovered;
    private bool _isStretchWinDragged;
    
    private bool _isBrushesWinHovered;
    private bool _isBrushesWinDragged;
    
    private bool _isSettingsWinHovered;
    private bool _isSettingsWinDragged;

    private bool _isNavbarHovered;


    public void OnPageUpdated(int previous, int @next) {
        _shouldRedrawLevel = true;
    }

    private Vector2 GetIndicatorPosition() => GLOBALS.Settings.LightEditor.LightIndicatorPosition switch
    {
        LightEditorSettings.ScreenRelativePosition.TopLeft => new Vector2(100, 130),
        LightEditorSettings.ScreenRelativePosition.TopRight => new Vector2(GetScreenWidth() - 100, 130),
        LightEditorSettings.ScreenRelativePosition.BottomLeft => new Vector2(100, GetScreenHeight() - 100),
        LightEditorSettings.ScreenRelativePosition.BottomRight => new Vector2(GetScreenWidth() - 100, GetScreenHeight() - 100),
        LightEditorSettings.ScreenRelativePosition.MiddleBottom => new Vector2((GetScreenWidth() - 100)/2f, GetScreenHeight() - 100),
        LightEditorSettings.ScreenRelativePosition.MiddleTop => new Vector2((GetScreenWidth() - 100)/2f, 130),

        _ => new Vector2(100, 100)
    };

    private static Vector2 DegreeToVector(int degree) {
        degree += 270;
        // degree *= -1;
        
        var rad = degree/360f * Math.PI*2;

        return new Vector2((float)Math.Cos(rad) * -1, (float)Math.Sin(rad));
    }

    private static Vector2 DegreeToVector2(int degree) {
        degree += 90;
        degree *= -1;
        
        var rad = degree/360f * Math.PI*2;

        return new Vector2((float)Math.Cos(rad) * -1, (float)Math.Sin(rad));
    }

    private bool _clickLock = false;

    public override void Draw()
    {
        
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera with { Target = GLOBALS.Camera.Target + new Vector2(300, 300)};
        var mouse = GetMousePosition();
        var worldMouse = GetScreenToWorld2D(mouse, _camera);
        
        var indicatorOrigin = GetIndicatorPosition();

        var indicatorPoint = new Vector2(
            indicatorOrigin.X + (float)((15 + GLOBALS.Level.LightFlatness * 7) * Math.Cos(float.DegreesToRadians(GLOBALS.Level.LightAngle + 90))),
            indicatorOrigin.Y + (float)((15 + GLOBALS.Level.LightFlatness * 7) * Math.Sin(float.DegreesToRadians(GLOBALS.Level.LightAngle + 90)))
        );
        
        var indHovered = CheckCollisionPointCircle(mouse, indicatorPoint, 10f); 

        if (IsMouseButtonReleased(MouseButton.Left) && _isDraggingIndicator)
            {
                _isDraggingIndicator = false;

                if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) {
                    _shouldRedrawLevel = true;
                }              

                _clickLock = false;  
            }

        if ((indHovered && IsMouseButtonDown(MouseButton.Left)) || _clickLock) {
            _isDraggingIndicator = true;
            _clickLock = true;

            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) {
                _shouldRedrawLevel = true;
            }
        }

        var panelHeight = GetScreenHeight() - 100;
        var brushPanel = new Rectangle(10, 50, 120, panelHeight);

        var canPaint = !_isNavbarHovered && 
                        !_isSettingsWinHovered && 
                       !_isSettingsWinDragged && 
                       !_isBrushesWinHovered && 
                       !_isBrushesWinDragged && 
                       !_isShortcutsWinHovered && 
                       !_isShortcutsWinDragged && 
                       !_isStretchWinHovered &&
                       !_isStretchWinDragged &&
                        !indHovered && 
                       !_isDraggingIndicator;
        
        var ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
        var shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
        var alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);


        if (_shortcuts.IncreaseFlatness.Check(ctrl, shift, alt, true) && GLOBALS.Level.LightFlatness < 10) GLOBALS.Level.LightFlatness++;
        if (_shortcuts.DecreaseFlatness.Check(ctrl, shift, alt, true) && GLOBALS.Level.LightFlatness > 1) GLOBALS.Level.LightFlatness--;

        if (_shortcuts.IncreaseAngle.Check(ctrl, shift, alt, true))
        {
            GLOBALS.Level.LightAngle--;

            if (GLOBALS.Level.LightAngle == 0) GLOBALS.Level.LightAngle = 360;
            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) {
                _shouldRedrawLevel = true;
            }
        }
        if (_shortcuts.DecreaseAngle.Check(ctrl, shift, alt, true))
        {
            GLOBALS.Level.LightAngle = ++GLOBALS.Level.LightAngle % 360;
            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) {
                _shouldRedrawLevel = true;
            }
        }

        // handle mouse drag
        if (_shortcuts.DragLevel.Check(ctrl, shift, alt, true) || _shortcuts.DragLevelAlt.Check(ctrl, shift, alt, true))
        {
            var delta = GetMouseDelta();
            delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
            _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
        }

        // handle zoom
        var wheel2 = GetMouseWheelMove();
        if (wheel2 != 0 && canPaint)
        {
            var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
            _camera.Offset = GetMousePosition();
            _camera.Target = mouseWorldPosition;
            _camera.Zoom += wheel2 * GLOBALS.ZoomIncrement;
            if (_camera.Zoom < GLOBALS.ZoomIncrement) _camera.Zoom = GLOBALS.ZoomIncrement;
        }

        // update light brush

        {
            var texture = GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex];
            var lightMouse = GetScreenToWorld2D(GetMousePosition(), _camera);

            _lightBrushSource = new(0, 0, texture.Width, texture.Height);
            _lightBrushDest = new(lightMouse.X, lightMouse.Y, _lightBrushWidth, _lightBrushHeight);
            _lightBrushOrigin = new(_lightBrushWidth / 2, _lightBrushHeight / 2);
        }

        if (_shortcuts.NextBrush.Check(ctrl, shift, alt))
        {
            _lightBrushTextureIndex = ++_lightBrushTextureIndex % GLOBALS.Textures.LightBrushes.Length;
        }
        else if (_shortcuts.PreviousBrush.Check(ctrl, shift, alt))
        {
            _lightBrushTextureIndex--;

            if (_lightBrushTextureIndex < 0) _lightBrushTextureIndex = GLOBALS.Textures.LightBrushes.Length - 1;
        }
        
        if (_shortcuts.RotateBrushCounterClockwise.Check(ctrl, shift, alt, true))
        {
            _lightBrushRotation -= 0.2f;
            
        }
        if (_shortcuts.RotateBrushClockwise.Check(ctrl, shift, alt, true))
        {
            
            _lightBrushRotation += 0.2f;
            
        }
        if (_shortcuts.StretchBrushVertically.Check(ctrl, shift, alt, true))
        {
            _lightBrushHeight += 2;
            
        }
        if (_shortcuts.SqueezeBrushVertically.Check(ctrl, shift, alt, true))
        {
            _lightBrushHeight -= 2;
            
        }
        if (_shortcuts.StretchBrushHorizontally.Check(ctrl, shift, alt, true))
        {
            _lightBrushWidth += 2;
            
        }
        if (_shortcuts.SqueezeBrushHorizontally.Check(ctrl, shift, alt, true))
        {
            
            _lightBrushWidth -= 2;
            
        }
        
        if (_shortcuts.FastRotateBrushCounterClockwise.Check(ctrl, shift, alt, true))
        {
            _lightBrushRotation -= 1 + _growthFactor;
            IncreaseGrowthFactor();
            
        }
        else if (_shortcuts.FastRotateBrushClockwise.Check(ctrl, shift, alt, true))
        {
            _lightBrushRotation += 1+_growthFactor;
            IncreaseGrowthFactor();
            
        }
        else if (_shortcuts.FastStretchBrushVertically.Check(ctrl, shift, alt, true))
        {
            _lightBrushHeight += 5+_growthFactor;
            IncreaseGrowthFactor();
            
        }
        else if (_shortcuts.FastSqueezeBrushVertically.Check(ctrl, shift, alt, true))
        {
            _lightBrushHeight -= 5+_growthFactor;
            IncreaseGrowthFactor();
            
        }
        else if (_shortcuts.FastStretchBrushHorizontally.Check(ctrl, shift, alt, true))
        {
            _lightBrushWidth += 5+_growthFactor;
            IncreaseGrowthFactor();
            
        }
        else if (_shortcuts.FastSqueezeBrushHorizontally.Check(ctrl, shift, alt, true))
        {
            _lightBrushWidth -= 5+_growthFactor;
            IncreaseGrowthFactor();
            
        }
        else ResetGrowthFactor();


        if (_shortcuts.ToggleTileVisibility.Check(ctrl, shift, alt)) _showTiles = !_showTiles;
        if (_shortcuts.ToggleTilePreview.Check(ctrl, shift, alt)) _tilePreview = !_tilePreview;
        if (_shortcuts.ToggleTintedTileTextures.Check(ctrl, shift, alt)) _tintedTileTextures = !_tintedTileTextures;

        //

        if (canPaint && !_stretchMode && (_shortcuts.Paint.Check(ctrl, shift, alt, true) || _shortcuts.PaintAlt.Check(ctrl, shift, alt, true)))
        {
            BeginTextureMode(GLOBALS.Textures.LightMap);
            {
                BeginShaderMode(GLOBALS.Shaders.ApplyShadowBrush);
                SetShaderValueTexture(GLOBALS.Shaders.ApplyShadowBrush, GetShaderLocation(GLOBALS.Shaders.ApplyShadowBrush, "inputTexture"), GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex]);
                DrawTexturePro(
                    GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex],
                    _lightBrushSource,
                    _lightBrushDest,
                    _lightBrushOrigin,
                    _lightBrushRotation,
                    Color.Black
                );
                EndShaderMode();
            }
            EndTextureMode();

            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) {
                _shouldRedrawLevel = true;
            }
        }

        if (canPaint && !_stretchMode && (_shortcuts.Erase.Check(ctrl, shift, alt, true) ||
                         _shortcuts.EraseAlt.Check(ctrl, shift, alt, true)))
        {
            _eraseShadow = true;

            BeginTextureMode(GLOBALS.Textures.LightMap);
            {
                BeginShaderMode(GLOBALS.Shaders.ApplyLightBrush);
                SetShaderValueTexture(GLOBALS.Shaders.ApplyLightBrush,
                    GetShaderLocation(GLOBALS.Shaders.ApplyLightBrush, "inputTexture"),
                    GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex]);
                DrawTexturePro(
                    GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex],
                    _lightBrushSource,
                    _lightBrushDest,
                    _lightBrushOrigin,
                    _lightBrushRotation,
                    Color.White
                );
                EndShaderMode();
            }
            EndTextureMode();

            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) {
                _shouldRedrawLevel = true;
            }
        }
        else _eraseShadow = false;

        // if (IsKeyPressed(KeyboardKey.R)) _shading = !_shading;

        if (_stretchMode)
        {
            if (_quadLock == 0)
            {
                if (CheckCollisionPointCircle(worldMouse, _quadPoints.TopLeft, 10) &&
                    IsMouseButtonDown(MouseButton.Left))
                {
                    _quadLock = 1;
                }
                
                if (CheckCollisionPointCircle(worldMouse, _quadPoints.TopRight, 10) &&
                    IsMouseButtonDown(MouseButton.Left))
                {
                    _quadLock = 2;
                }
                
                if (CheckCollisionPointCircle(worldMouse, _quadPoints.BottomRight, 10) &&
                    IsMouseButtonDown(MouseButton.Left))
                {
                    _quadLock = 3;
                }
                
                if (CheckCollisionPointCircle(worldMouse, _quadPoints.BottomLeft, 10) &&
                    IsMouseButtonDown(MouseButton.Left))
                {
                    _quadLock = 4;
                }
            }

            if (IsMouseButtonReleased(MouseButton.Left)) _quadLock = 0;

            switch (_quadLock)
            {
                case 1: _quadPoints.TopLeft = worldMouse; break;
                case 2: _quadPoints.TopRight = worldMouse; break;
                case 3: _quadPoints.BottomRight = worldMouse; break;
                case 4: _quadPoints.BottomLeft = worldMouse; break;
            }
        }

        DrawLayers();
        
        BeginDrawing();
        {
            ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme 
                ? Color.Black 
                : GLOBALS.Settings.LightEditor.Background);


            if (_shouldRedrawLevel)
            {
                Printers.DrawLevelIntoBuffer(GLOBALS.Textures.GeneralLevel, new Printers.DrawLevelParams
                {
                    CurrentLayer = 0,
                    Water = true,
                    WaterAtFront = GLOBALS.Level.WaterAtFront,
                    
                    TilesLayer1 = _showTiles,
                    TilesLayer2 = _showTiles,
                    TilesLayer3 = _showTiles,
                    
                    PropsLayer1 = _showProps,
                    PropsLayer2 = _showProps,
                    PropsLayer3 = _showProps,
                    
                    TileDrawMode = GLOBALS.Settings.GeneralSettings.DrawTileMode,
                    PropDrawMode = GLOBALS.Settings.GeneralSettings.DrawPropMode,
                    HighLayerContrast = GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette,
                    Palette = GLOBALS.SelectedPalette,
                    Shadows = GLOBALS.Settings.LightEditor.Projection == LightEditorSettings.LightProjection.ThreeLayers
                });
                _shouldRedrawLevel = false;
            }

            BeginMode2D(_camera);
            {
                #region Level
                DrawRectangle(
                    0, 0,
                    GLOBALS.Level.Width * GLOBALS.Scale + 300,
                    GLOBALS.Level.Height * GLOBALS.Scale + 300,
                    GLOBALS.Settings.GeneralSettings.DarkTheme 
                        ? GLOBALS.Settings.LightEditor.LevelBackgroundDark 
                        : GLOBALS.Settings.LightEditor.LevelBackgroundLight
                );

                if (GLOBALS.Settings.GeneralSettings.DarkTheme)
                {
                    DrawRectangleLinesEx(
                        new Rectangle(-2, -2, GLOBALS.Level.Width*GLOBALS.Scale+304, GLOBALS.Level.Height*GLOBALS.Scale+304),
                        2f,
                        Color.White);
                }

                BeginShaderMode(GLOBALS.Shaders.VFlip);
                SetShaderValueTexture(GLOBALS.Shaders.VFlip, GetShaderLocation(GLOBALS.Shaders.VFlip, "inputTexture"), GLOBALS.Textures.GeneralLevel.Texture);
                DrawTexture(GLOBALS.Textures.GeneralLevel.Texture, 300, 300, Color.White);
                EndShaderMode();
                #endregion
                
                // Lightmap

                if (_stretchMode)
                {
                    BeginShaderMode(GLOBALS.Shaders.LightMapStretch);
                    SetShaderValueTexture(GLOBALS.Shaders.LightMapStretch, GetShaderLocation(GLOBALS.Shaders.LightMapStretch, "textureSampler"), _stretchTexture);
                    Printers.DrawTextureQuad(_stretchTexture, _quadPoints, Color.White with { A = 150 });
                    EndShaderMode();
                }
                else if (GLOBALS.Settings.LightEditor.Projection == LightEditorSettings.LightProjection.None) {
                    DrawTextureRec(
                        GLOBALS.Textures.LightMap.Texture,
                        new Rectangle(0, 0, GLOBALS.Textures.LightMap.Texture.Width, -GLOBALS.Textures.LightMap.Texture.Height),
                        new Vector2(0, 0),
                        Color.White with { A = 150 }
                    );
                }
                else if (GLOBALS.Settings.LightEditor.Projection == LightEditorSettings.LightProjection.Basic)
                {
                    BeginShaderMode(_mask);
                    {
                        SetShaderValueTexture(_mask, GetShaderLocation(_mask, "inputTexture"), GLOBALS.Textures.LightMap.Texture);
                        SetShaderValue(_mask, GetShaderLocation(_mask, "vflip"), 1, ShaderUniformDataType.Int);

                        DrawTextureV(
                            GLOBALS.Textures.LightMap.Texture, 
                            GLOBALS.Level.LightFlatness * 10 * DegreeToVector2(GLOBALS.Level.LightAngle), 
                            Color.Black with { A = 150 }
                        );
                    }
                    EndShaderMode();

                    DrawTextureRec(
                        GLOBALS.Textures.LightMap.Texture,
                        new Rectangle(0, 0, GLOBALS.Textures.LightMap.Texture.Width, -GLOBALS.Textures.LightMap.Texture.Height),
                        new Vector2(0, 0),
                        Color.White with { A = 150 }
                    );
                }
                else if (GLOBALS.Settings.LightEditor.Projection == LightEditorSettings.LightProjection.ThreeLayers && GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) {
                    var vflip = GLOBALS.Shaders.VFlip;
                    
                    BeginShaderMode(_mask);
                    {
                        SetShaderValueTexture(_mask, GetShaderLocation(_mask, "inputTexture"), GLOBALS.Textures.LightMap.Texture);
                        SetShaderValue(_mask, GetShaderLocation(_mask, "vflip"), 1, ShaderUniformDataType.Int);

                        DrawTextureV(
                            GLOBALS.Textures.LightMap.Texture, 
                            GLOBALS.Level.LightFlatness * 30 * DegreeToVector2(GLOBALS.Level.LightAngle), 
                            Color.Black
                        );
                    }
                    EndShaderMode();

                    
                    BeginShaderMode(_mask);
                    {
                        SetShaderValueTexture(_mask, GetShaderLocation(_mask, "inputTexture"), GLOBALS.Textures.LightMap.Texture);
                        SetShaderValue(_mask, GetShaderLocation(_mask, "vflip"), 1, ShaderUniformDataType.Int);

                        DrawTextureV(
                            GLOBALS.Textures.LightMap.Texture, 
                            GLOBALS.Level.LightFlatness * 20 * DegreeToVector2(GLOBALS.Level.LightAngle), 
                            Color.DarkGray
                        );
                    }
                    EndShaderMode();

                    
                    BeginShaderMode(_mask);
                    {
                        SetShaderValueTexture(_mask, GetShaderLocation(_mask, "inputTexture"), GLOBALS.Textures.LightMap.Texture);
                        SetShaderValue(_mask, GetShaderLocation(_mask, "vflip"), 1, ShaderUniformDataType.Int);

                        DrawTextureV(
                            GLOBALS.Textures.LightMap.Texture, 
                            GLOBALS.Level.LightFlatness * 10 * DegreeToVector2(GLOBALS.Level.LightAngle), 
                            Color.Gray
                        );
                    }
                    EndShaderMode();
                }
                
                if (_stretchMode)
                {
                    DrawCircleV(_quadPoints.TopLeft, 7f, Color.Blue);
                    DrawCircleV(_quadPoints.TopRight, 7f, Color.Blue);
                    DrawCircleV(_quadPoints.BottomRight, 7f, Color.Blue);
                    DrawCircleV(_quadPoints.BottomLeft, 7f, Color.Blue);
                }

                // The brush

                if (!indHovered && !_isDraggingIndicator && !_stretchMode)
                {
                    if (_eraseShadow)
                    {
                        BeginShaderMode(GLOBALS.Shaders.LightBrush);
                        SetShaderValueTexture(GLOBALS.Shaders.LightBrush,
                            GetShaderLocation(GLOBALS.Shaders.LightBrush, "inputTexture"),
                            GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex]);

                        DrawTexturePro(
                            GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex],
                            _lightBrushSource,
                            _lightBrushDest,
                            _lightBrushOrigin,
                            _lightBrushRotation,
                            Color.White
                        );
                        EndShaderMode();
                    }
                    else
                    {
                        BeginShaderMode(GLOBALS.Shaders.ShadowBrush);
                        SetShaderValueTexture(GLOBALS.Shaders.ShadowBrush,
                            GetShaderLocation(GLOBALS.Shaders.ShadowBrush, "inputTexture"),
                            GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex]);

                        DrawTexturePro(
                            GLOBALS.Textures.LightBrushes[_lightBrushTextureIndex],
                            _lightBrushSource,
                            _lightBrushDest,
                            _lightBrushOrigin,
                            _lightBrushRotation,
                            Color.White
                        );
                        EndShaderMode();
                    }
                }
            }
            EndMode2D();


            #region Indicator

            DrawCircleLines(
                (int)indicatorOrigin.X,
                (int)indicatorOrigin.Y,
                50.0f,
                new(255, 0, 0, 255)
            );

            DrawCircleLines(
                (int)indicatorOrigin.X,
                (int)indicatorOrigin.Y,
                15 + (GLOBALS.Level.LightFlatness * 7),
                new(255, 0, 0, 255)
            );

            if (_isDraggingIndicator)
            {
                var radius = (int) Raymath.Vector2Distance(mouse, indicatorOrigin);

                var newAngle = (int)float.RadiansToDegrees(Raymath.Vector2Angle(
                    indicatorOrigin with { Y = indicatorOrigin.Y + 1 } - indicatorOrigin,
                    mouse - indicatorOrigin
                    )
                );

                if (newAngle < 0) newAngle += 360;

                GLOBALS.Level.LightAngle = newAngle;

                if (radius > 85) radius = 85;
                if (radius < 1) radius = 1;

                GLOBALS.Level.LightFlatness = (radius - 15) / 7;
            }

            DrawCircleV(indicatorPoint,
                10.0f,
                Color.Red
            );
            
            #endregion
            
            rlImGui.Begin();
            
            ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
            
            // Navigation bar
                
            if (GLOBALS.Settings.GeneralSettings.Navbar) GLOBALS.NavSignal = Printers.ImGui.Nav(out _isNavbarHovered);
            
            #region Brushes
            {
                // Brushes Window

                var menuOpened = ImGui.Begin("Brushes##LightBrushesWindow");
                
                var menuPos = ImGui.GetWindowPos();
                var menuWinSpace = ImGui.GetWindowSize();

                if (CheckCollisionPointRec(GetMousePosition(), new(menuPos.X - 5, menuPos.Y-5, menuWinSpace.X + 10, menuWinSpace.Y+10)))
                {
                    _isBrushesWinHovered = true;

                    if (IsMouseButtonDown(MouseButton.Left)) _isBrushesWinDragged = true;
                }
                else
                {
                    _isBrushesWinHovered = false;
                }

                if (IsMouseButtonReleased(MouseButton.Left) && _isBrushesWinDragged) _isBrushesWinDragged = false;
                
                if (menuOpened)
                {
                    var availableSpace = ImGui.GetContentRegionAvail();
                    
                    if (ImGui.BeginListBox("##LightBrushes", availableSpace))
                    {
                        for (var index = 0; index < GLOBALS.Textures.LightBrushes.Length; index++)
                        {
                            
                            var selected = ImGui.ImageButton(
                                $"Brush {index}",
                                new IntPtr(GLOBALS.Textures.LightBrushes[index].Id), 
                                new Vector2(60, 60));

                            ImGui.SameLine();
                            
                            var selected2 = ImGui.Selectable(
                                $"#{index}", 
                                _lightBrushTextureIndex == index, 
                                ImGuiSelectableFlags.None | ImGuiSelectableFlags.AllowOverlap, 
                                new Vector2(60, 60)
                            );
                            
                            if (selected || selected2)
                            {
                                _lightBrushTextureIndex = index;
                            }
                        }
                        ImGui.End();
                    }
                    ImGui.End();
                }
            }
            #endregion
            
            #region StretchMode
            {
                var stretchModeWinOpened = ImGui.Begin("Stretch Mode");
                
                var menuPos = ImGui.GetWindowPos();
                var menuWinSpace = ImGui.GetWindowSize();

                if (CheckCollisionPointRec(GetMousePosition(), new(menuPos.X - 5, menuPos.Y-5, menuWinSpace.X + 10, menuWinSpace.Y+10)))
                {
                    _isStretchWinHovered = true;

                    if (IsMouseButtonDown(MouseButton.Left)) _isStretchWinDragged = true;
                }
                else
                {
                    _isStretchWinHovered = false;
                }

                if (IsMouseButtonReleased(MouseButton.Left) && _isStretchWinDragged) _isStretchWinDragged = false;
                
                if (stretchModeWinOpened)
                {
                    var availableSpace = ImGui.GetContentRegionAvail();

                    var stretchSwitchClicked = ImGui.Button($"{(_stretchMode ? "Cancel" : "Enable")}", availableSpace with { Y = 20 });

                    if (stretchSwitchClicked)
                    {
                        SwitchStretchMode();
                    }

                    if (_stretchMode)
                    {
                        var applyClicked = ImGui.Button("Apply", availableSpace with { Y = 20 });

                        if (applyClicked)
                        {
                            BeginTextureMode(GLOBALS.Textures.LightMap);
                            ClearBackground(Color.White);
                            Printers.DrawTextureQuad(_stretchTexture, _quadPoints);
                            EndTextureMode();
                            
                            ResetQuadPoints();
                        }
                    }
                    
                    ImGui.End();
                }
            }
            #endregion
            
            #region Settings
            {
                var settingsOpened = ImGui.Begin("Settings##LightSettings");
                
                var menuPos = ImGui.GetWindowPos();
                var menuWinSpace = ImGui.GetWindowSize();
                
                if (CheckCollisionPointRec(GetMousePosition(), new(menuPos.X - 5, menuPos.Y-5, menuWinSpace.X + 10, menuWinSpace.Y+10)))
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
                    var availableSpace = ImGui.GetContentRegionAvail();

                    var liPos = (int)GLOBALS.Settings.LightEditor.LightIndicatorPosition;
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.Combo("Light Indicator Position", ref liPos, "Normal\0Copy\0Paste With Geo\0Paste Without Geo\0Auto Pipes\0Auto Box")) {
                        GLOBALS.Settings.LightEditor.LightIndicatorPosition = (LightEditorSettings.ScreenRelativePosition)liPos;
                    }
                    
                    Vector3 bgColorVec = GLOBALS.Settings.LightEditor.Background;
                    
                    bgColorVec /= 255;
                    
                    ImGui.SetNextItemWidth(200);
                    
                    var bgColorUpdated = ImGui.ColorEdit3(
                        "Background Color##LightBackgroundColor", 
                        ref bgColorVec);
                    if (bgColorUpdated) GLOBALS.Settings.LightEditor.Background = bgColorVec*255;
                    
                    // Level background color
                    
                    ImGui.SeparatorText("Level Background Colors");

                    Vector3 levelBgColorLight = GLOBALS.Settings.LightEditor.LevelBackgroundLight;

                    levelBgColorLight /= 255;

                    ImGui.SetNextItemWidth(200);

                    var levelLightUpdated = ImGui.ColorEdit3("Light Mode", ref levelBgColorLight);

                    if (levelLightUpdated) GLOBALS.Settings.LightEditor.LevelBackgroundLight = levelBgColorLight*255;
                    
                    
                    
                    Vector3 levelBgColorDark = GLOBALS.Settings.LightEditor.LevelBackgroundDark;

                    levelBgColorDark /= 255;

                    ImGui.SetNextItemWidth(200);

                    var levelDarkUpdated = ImGui.ColorEdit3("Dark Mode", ref levelBgColorDark);

                    if (levelDarkUpdated) GLOBALS.Settings.LightEditor.LevelBackgroundDark = levelBgColorDark*255;
                    
                    ImGui.Spacing();

                    if (ImGui.Checkbox("Tiles", ref _showTiles)) _shouldRedrawLevel = true;
                    if (ImGui.Checkbox("Props", ref _showProps)) _shouldRedrawLevel = true;
                    
                    ImGui.Spacing();


                    var projectionIndex = (int)GLOBALS.Settings.LightEditor.Projection;

                    var projectionChanged = ImGui.Combo("Projection", ref projectionIndex, string.Join('\0', Enum.GetNames(typeof(LightEditorSettings.LightProjection))));

                    if (projectionChanged) {
                        GLOBALS.Settings.LightEditor.Projection = (LightEditorSettings.LightProjection)projectionIndex;
                        _shouldRedrawLevel = true;
                    }

                    ImGui.Spacing();

                    if (ImGui.Button("Save Settings", availableSpace with { Y = 20 }))
                    {
                        try
                        {
                            var text = JsonSerializer.Serialize(GLOBALS.Settings, GLOBALS.JsonSerializerOptions);
                            File.WriteAllText(GLOBALS.Paths.SettingsPath, text);
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"Failed to save settings: {e}");
                        }
                    }
                    
                    ImGui.End();
                }
            }
            #endregion
            
            #region Shortcuts
            // Shortcuts window
            
            if (GLOBALS.Settings.GeneralSettings.ShortcutWindow)
            {
                var shortcutWindowRect = Printers.ImGui.ShortcutsWindow(GLOBALS.Settings.Shortcuts.LightEditor);

                _isShortcutsWinHovered = CheckCollisionPointRec(
                    mouse, 
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
            #endregion
            
            rlImGui.End();
        }
        EndDrawing();
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera with { Target = _camera.Target - new Vector2(300, 300)};
    }
}
